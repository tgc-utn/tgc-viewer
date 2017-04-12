using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using System;
using TGC.Core.Mathematica;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para reproducir un sonido WAV en 3D, variando como suena respecto de su posición
    ///     en el espacio.
    ///     Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
    ///     Sonidos stereos (2 channels) no pueden ser utilizados.
    /// </summary>
    public class Tgc3dSound
    {
        /// <summary>
        ///     Crea un sonido 3D
        ///     Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        ///     Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="position">Posicion del sonido en el espacio</param>
        public Tgc3dSound(string soundPath, TGCVector3 position, Device device)
        {
            loadSound(soundPath, device);
            Position = position;
        }

        /// <summary>
        ///     Buffer con la información del sonido cargado
        /// </summary>
        public SecondaryBuffer SoundBuffer { get; private set; }

        /// <summary>
        ///     Buffer que manipula la parte 3D del sonido cargado
        /// </summary>
        public Buffer3D Buffer3d { get; private set; }

        /// <summary>
        ///     Posición del sonido dentro del espacio.
        ///     La forma de escuchar el sonido varia según esta ubicación y la posición
        ///     del Listener3D de sonidos.
        /// </summary>
        public TGCVector3 Position
        {
            get { return TGCVector3.FromVector3(Buffer3d.Position); }
            set { Buffer3d.Position = value; }
        }

        /// <summary>
        ///     Mínima distancia a partir de la cual el sonido 3D comienza a atenuarse respecto de la posicion
        ///     del Listener3D
        /// </summary>
        public float MinDistance
        {
            get { return Buffer3d.MinDistance; }
            set { Buffer3d.MinDistance = value; }
        }

        /// <summary>
        ///     Carga un archivo WAV de audio, indicando el volumen del mismo
        ///     Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        ///     Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="volume">Volumen del mismo</param>
        public void loadSound(string soundPath, int volume, Device device)
        {
            try
            {
                dispose();

                var bufferDescription = new BufferDescription();
                bufferDescription.Control3D = true;
                if (volume != -1)
                {
                    bufferDescription.ControlVolume = true;
                }

                SoundBuffer = new SecondaryBuffer(soundPath, bufferDescription, device);
                Buffer3d = new Buffer3D(SoundBuffer);
                Buffer3d.MinDistance = 50;

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
        ///     Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        ///     Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        public void loadSound(string soundPath, Device device)
        {
            loadSound(soundPath, -1, device);
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