using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Emisor de sonido
    /// </summary>
    public class TgcSoundEmitter : TgcSoundActor
    {
        private Emitter emitter;
        private TgcSound sound;

        /// <summary>
        ///     Crea un emisor de sonido
        /// </summary>
        /// <param name="path">Ruta al archivo con el sonido en formato WAV. Debe ser de un unico canal</param>
        /// <param name="emisor">Objeto emisor del sonido. Desde el mismo se emitira el sonido especificado</param>
        public TgcSoundEmitter(string path, ITransformObject emisor)
        {
            sound = new TgcSound(path);
            sound.LoopCount = TgcSound.LOOP_INFINITE;
            this.origin = emisor;

            emitter = new Emitter();
            emitter.ChannelCount = 1;
            emitter.CurveDistanceScaler = float.MinValue;

            RangeMin = float.MaxValue;
            RangeMax = float.MaxValue;

            Register();

            // Como XAudio2 funciona en otro thread,
            // el sonido se va a reproducir en volumen maximo hasta que se realice un Update de este objeto.
            // En ese intervalo queremos que el sonido se reproduzca, pero sea inaudible.
            sound.Volume = 0f;
            sound.Play();
        }

        /// <summary>
        ///     Asigna la orientacion del emisor.
        ///     Ambos vectores deben estar normalizados y deben ser ortonormales.
        ///     Debe ser llamada cada vez que cambia la orientacion del actor.
        /// </summary>
        /// <param name="front">Direccion del frente del objeto</param>
        /// <param name="top">Direccion hacia arriba del objeto</param>
        public override void SetOrientation(TGCVector3 front, TGCVector3 top)
        {
            emitter.OrientFront = front.ToRawVector;
            emitter.OrientTop = top.ToRawVector;
        }

        /// <summary>
        ///     Retorna la posicion del emisor
        /// </summary>
        public TGCVector3 Position { get { return origin.Position; } }

        /// <summary>
        ///     El Rango Minimo del emisor.
        ///     Debe ser menor o igual que RangeMax
        ///     Cualquier Listener cuya distancia al Emitter sea menor que RangeMin escuchara el sonido a todo volumen.
        ///     Si la distancia es mayor que RangeMin pero menor que RangeMax, escuchara el sonido proporcionalmente a como este situado el Listener entre estos dos.
        ///     Si la distancia es mayor que RangeMax, el sonido no se escuchara.
        /// </summary>
        public float RangeMin { get; set; }

        /// <summary>
        ///     El Rango Minimo del emisor.
        ///     Debe ser mayor o igual que RangeMin.
        ///     Cualquier Listener cuya distancia al Emitter sea menor que RangeMin escuchara el sonido a todo volumen.
        ///     Si la distancia es mayor que RangeMin pero menor que RangeMax, escuchara el sonido proporcionalmente a como este situado el Listener entre estos dos.
        ///     Si la distancia es mayor que RangeMax, el sonido no se escuchara.
        /// </summary>
        public float RangeMax { get; set; }


        /// <summary>
        ///     Libera los recursos del Emitter
        /// </summary>
        public override void Dispose()
        {
            Unregister();
            sound.Dispose();
        }

        internal Emitter Native { get { return this.emitter; } }

        internal override void Register()
        {
            Tgc3DSoundManager.Register(this);
        }

        internal override void Unregister()
        {
            Tgc3DSoundManager.Unregister(this);
        }

        internal override void UpdatePosition()
        {
            emitter.Position = origin.Position.ToRawVector;
        }

        internal void Process3D(DspSettings settings)
        {
            sound.Process3D(settings, RangeMin, RangeMax);
        }

    }
}
