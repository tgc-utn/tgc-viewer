using System;
using Microsoft.DirectX.DirectSound;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para reproducir un sonido WAV estatico
    /// </summary>
    public class TGCStaticSound : IDisposable
    {
        /// <summary>
        ///     Buffer con la informacion del sonido cargado
        /// </summary>
        public SecondaryBuffer SoundBuffer { get; private set; }

        /// <summary>
        ///     Liberar recursos del sonido
        /// </summary>
        public void Dispose()
        {
            if (SoundBuffer != null && !SoundBuffer.Disposed)
            {
                SoundBuffer.Dispose();
                SoundBuffer = null;
            }
        }

        /// <summary>
        ///     Carga un archivo WAV de audio, indicando el volumen del mismo
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="volume">Volumen del mismo</param>
        public void LoadSound(string soundPath, int volume, Device device)
        {
            try
            {
                Dispose();

                var bufferDescription = new BufferDescription();
                if (volume != -1)
                {
                    bufferDescription.ControlVolume = true;
                }

                SoundBuffer = new SecondaryBuffer(soundPath, bufferDescription, device);

                if (volume != -1)
                {
                    SoundBuffer.Volume = volume;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar sonido estático WAV: " + soundPath, ex);
            }
        }

        /// <summary>
        ///     Carga un archivo WAV de audio, con el volumen default del mismo
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        public void LoadSound(string soundPath, Device device)
        {
            LoadSound(soundPath, -1, device);
        }

        /// <summary>
        ///     Reproduce el sonido, indicando si se hace con Loop.
        ///     Si ya se esta reproduciendo, no vuelve a empezar.
        /// </summary>
        /// <param name="playLoop">TRUE para reproducir en loop</param>
        public void Play(bool playLoop)
        {
            SoundBuffer.Play(0, playLoop ? BufferPlayFlags.Looping : BufferPlayFlags.Default);
        }

        /// <summary>
        ///     Reproduce el sonido, sin Loop.
        ///     Si ya se esta reproduciedo, no vuelve a empezar.
        /// </summary>
        public void Play()
        {
            Play(false);
        }

        /// <summary>
        ///     Pausa la ejecucion del sonido.
        ///     Si el sonido no se estaba ejecutando, no hace nada.
        ///     Si se hace Stop() y luego Play(), el sonido continua desde donde habaa dejado la ultima vez.
        /// </summary>
        public void Stop()
        {
            SoundBuffer.Stop();
        }
    }
}
