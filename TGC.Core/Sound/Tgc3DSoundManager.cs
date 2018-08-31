using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Core.Sound
{
    static class Tgc3DSoundManager
    {
        private static X3DAudio audio3D;

        private static TgcSoundListener active;
        private static List<TgcSoundEmitter> emitters;        

        internal static void Initialize()
        {
            if(audio3D == null)
            {
                audio3D = new X3DAudio(Speakers.FrontLeft | Speakers.FrontRight);
                emitters = new List<TgcSoundEmitter>();
            }
        }

        internal static void Register(TgcSoundListener listener)
        {
            active = listener;
        }

        internal static void Register(TgcSoundEmitter emitter)
        {
            emitters.Add(emitter);
        }

        internal static void Unregister(TgcSoundListener listener)
        {
            if (listener.Equals(active))
                active = null;
        }

        internal static void Unregister(TgcSoundEmitter emitter)
        {
            emitters.Remove(emitter);
        }
        
        internal static void Update()
        {
            if(active != null)
            {
                active.UpdatePosition();

                foreach (var emitter in emitters)
                {
                    emitter.UpdatePosition();
                    var settings = Calculate(active, emitter, CalculateFlags.Matrix | CalculateFlags.Doppler);
                    emitter.Process3D(settings);
                }
            }            
        }

        internal static DspSettings Calculate(TgcSoundListener listener, TgcSoundEmitter emitter, CalculateFlags flags)
        {
            DspSettings settings = new DspSettings(1, 2);
            audio3D.Calculate(listener.Native, emitter.Native, flags, settings);
            return settings;
        }

        /// <summary>
        ///     Devuelve el Listener activo, quien recibe el sonido que sera emitido por el dispositivo
        /// </summary>
        public static TgcSoundListener Active { get { return active; } }

    }
}
