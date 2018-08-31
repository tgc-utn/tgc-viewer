using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using TGC.Core.Mathematica;
using static TGC.Core.Sound.TgcSoundManager;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Sonido de TGC en formato WAV o de onda
    /// </summary>
    public class TgcSound
    {
        /// <summary>
        ///     Usar esta constante en LoopCount para que el sonido se reproduzca infinitamente
        /// </summary>
        public const int LOOP_INFINITE = 255;

        private SourceVoice voice;
        private CustomAudioBuffer buffer;
        private TgcSoundState state;

        /// <summary>
        ///     Crea un sonido
        /// </summary>
        public TgcSound(string path)
        {
            buffer = TgcSoundManager.CreateSoundBuffer(path);
            voice = TgcSoundManager.CreateVoice(buffer);
            voice.SetVolume(1);
            LoopCount = LOOP_INFINITE;
            state = new TgcSoundStateStopped(this);
        }

        internal void Process3D(DspSettings settings, float rangeMin, float rangeMax)
        {
            voice.SetOutputMatrix(1, 2, settings.MatrixCoefficients);
            voice.SetFrequencyRatio(settings.DopplerFactor);
            voice.SetVolume(CalculateVolume(rangeMin, rangeMax, settings.EmitterToListenerDistance));
        }

        private float CalculateVolume(float rangeMin, float rangeMax, float distance)
        {
            return FastMath.Clamp(1 - ((distance - rangeMin) / (rangeMax - rangeMin)), 0, 1);
        }

        internal void SwitchState(TgcSoundState newState)
        {
            this.state = newState;
        }

        internal SourceVoice Voice { get { return voice; } }

        internal CustomAudioBuffer Buffer { get { return buffer; } }

        /// <summary>
        ///     Indica cuantas veces se va a reproducir el sonido.
        ///     Detiene la reproduccion del mismo si su valor se cambia.
        /// </summary>
        public int LoopCount
        {
            get
            {
                return buffer.LoopCount;
            }
            set
            {
                if (buffer.LoopCount != value)
                {
                    buffer.LoopCount = value;
                    SwitchState(new TgcSoundStateStopped(this));
                }                
            }
        }

        /// <summary>
        ///     Reproduce el sonido, o lo reanuda si fue pausado 
        /// </summary>
        public void Play()
        {
            this.state.Play();
        }

        /// <summary>
        ///     Asigna u obtiene el volumen del sonido
        /// </summary>
        public float Volume { get { return voice.Volume; } set { voice.SetVolume(value); } }

        /// <summary>
        ///     Pausa el sonido. Cuando se vuelva a reproducir se escuchara desde donde se pauso.
        /// </summary>
        public void Pause()
        {
            this.state.Pause();
        }

        /// <summary>
        ///     Para el sonido. Cuando se vuelva a reproducir se escuchara desde el principio.
        /// </summary>
        public void Stop()
        {
            this.state.Stop();
        }

        /// <summary>
        ///     Libera todos los recursos asociados con el sonido
        /// </summary>
        public void Dispose()
        {
            this.state.Dispose();
        }

    }
}
