using Microsoft.DirectX.DirectSound;
using System;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para reproducir un sonido WAV estático
    /// </summary>
    public class TgcStaticSound
    {
        /// <summary>
        ///     Buffer con la información del sonido cargado
        /// </summary>
        public SecondaryBuffer SoundBuffer { get; private set; }

        /// <summary>
        ///     Carga un archivo WAV de audio, indicando el volumen del mismo
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="volume">Volumen del mismo</param>
        public void loadSound(string soundPath, int volume)
        {
            try
            {
                dispose();

                var bufferDescription = new BufferDescription();
                if (volume != -1)
                {
                    bufferDescription.ControlVolume = true;
                }

                SoundBuffer = new SecondaryBuffer(soundPath, bufferDescription,
                    TgcDirectSound.Instance.DsDevice);

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
        public void loadSound(string soundPath)
        {
            loadSound(soundPath, -1);
        }

        /// <summary>
        ///     Reproduce el sonido, indicando si se hace con Loop.
        ///     Si ya se está reproduciedo, no vuelve a empezar.
        /// </summary>
        /// <param name="playLoop">TRUE para reproducir en loop</param>
        public void play(bool playLoop)
        {
            SoundBuffer.Play(0, playLoop ? BufferPlayFlags.Looping : BufferPlayFlags.Default);
        }

        /// <summary>
        ///     Reproduce el sonido, sin Loop.
        ///     Si ya se está reproduciedo, no vuelve a empezar.
        /// </summary>
        public void play()
        {
            play(false);
        }

        /// <summary>
        ///     Pausa la ejecución del sonido.
        ///     Si el sonido no se estaba ejecutando, no hace nada.
        ///     Si se hace stop() y luego play(), el sonido continua desde donde había dejado la última vez.
        /// </summary>
        public void stop()
        {
            SoundBuffer.Stop();
        }

        /// <summary>
        ///     Liberar recursos del sonido
        /// </summary>
        public void dispose()
        {
            if (SoundBuffer != null && !SoundBuffer.Disposed)
            {
                SoundBuffer.Dispose();
                SoundBuffer = null;
            }
        }
    }
}