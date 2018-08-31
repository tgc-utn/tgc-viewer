using SharpDX.X3DAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Receptor de sonido
    /// </summary>
    public class TgcSoundListener : TgcSoundActor
    {
        private Listener listener;

        /// <summary>
        ///     Crea un receptor de sonido
        /// </summary>
        /// <param name="receptor">Objeto receptor de sonido. Desde el mismo se escucharan los sonidos emitidos</param>
        public TgcSoundListener(ITransformObject receptor)
        {
            origin = receptor;
            listener = new Listener();
            Active = true;
        }

        /// <summary>
        ///     Posicion del receptor
        /// </summary>
        public TGCVector3 Position { get { return origin.Position; } }

        /// <summary>
        ///     Indica si el receptor esta activo.
        ///     Que este activo significa que este receptor actualmente esta escuchando sonidos y, segun como los reciba, seran emitidos por el dispositivo de audio.
        ///     Puede haber mas de un receptor, pero solo un receptor activo a la vez.
        /// </summary>
        public bool Active
        {
            get
            {
                return Tgc3DSoundManager.Active.Equals(this);
            }
            set
            {
                if (value)
                    Register();
                else
                    Unregister();
            }
        }

        /// <summary>
        ///     Asigna la orientacion del receptor.
        ///     Ambos vectores deben estar normalizados y deben ser ortonormales.
        ///     Debe ser llamada cada vez que cambia la orientacion del actor.
        /// </summary>
        /// <param name="front">Direccion del frente del objeto</param>
        /// <param name="top">Direccion hacia arriba del objeto</param>
        public override void SetOrientation(TGCVector3 front, TGCVector3 top)
        {
            listener.OrientFront = front.ToRawVector;
            listener.OrientTop = top.ToRawVector;
        }

        /// <summary>
        ///     Libera los recursos consumidos por el receptor
        /// </summary>
        public override void Dispose()
        {
            Active = false;
        }

        internal override void Register()
        {
            Tgc3DSoundManager.Register(this);
        }

        internal override void Unregister()
        {
            Tgc3DSoundManager.Unregister(this);
        }

        internal Listener Native { get { return listener; } }
        
        internal override void UpdatePosition()
        {
            listener.Position = origin.Position.ToRawVector;
        }

    }
}
