using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectSound;
using Microsoft.DirectX;

namespace TgcViewer.Utils.Sound
{
    /// <summary>
    /// Herramienta para reproducir un sonido WAV en 3D, variando como suena respecto de su posici�n
    /// en el espacio.
    /// Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
    /// Sonidos stereos (2 channels) no pueden ser utilizados.
    /// </summary>
    public class Tgc3dSound
    {
        private SecondaryBuffer soundBuffer;
        /// <summary>
        /// Buffer con la informaci�n del sonido cargado
        /// </summary>
        public SecondaryBuffer SoundBuffer
        {
            get { return soundBuffer; }
        }

        private Buffer3D buffer3d;
        /// <summary>
        /// Buffer que manipula la parte 3D del sonido cargado
        /// </summary>
        public Buffer3D Buffer3d
        {
            get { return buffer3d; }
        }

        /// <summary>
        /// Posici�n del sonido dentro del espacio.
        /// La forma de escuchar el sonido varia seg�n esta ubicaci�n y la posici�n
        /// del Listener3D de sonidos.
        /// </summary>
        public Vector3 Position
        {
            get { return buffer3d.Position; }
            set { buffer3d.Position = value; }
        }

        /// <summary>
        /// M�nima distancia a partir de la cual el sonido 3D comienza a atenuarse respecto de la posicion
        /// del Listener3D
        /// </summary>
        public float MinDistance
        {
            get { return buffer3d.MinDistance; }
            set { buffer3d.MinDistance = value; }
        }

        public Tgc3dSound()
        {
        }

        /// <summary>
        /// Crea un sonido 3D
        /// Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        /// Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="position">Posicion del sonido en el espacio</param>
        public Tgc3dSound(string soundPath, Vector3 position)
        {
            loadSound(soundPath);
            Position = position;
        }

        /// <summary>
        /// Carga un archivo WAV de audio, indicando el volumen del mismo
        /// Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        /// Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        /// <param name="volume">Volumen del mismo</param>
        public void loadSound(string soundPath, int volume)
        {
            try
            {
                dispose();

                BufferDescription bufferDescription = new BufferDescription();
                bufferDescription.Control3D = true;
                if (volume != -1)
                {
                    bufferDescription.ControlVolume = true;
                }

                soundBuffer = new SecondaryBuffer(soundPath, bufferDescription, GuiController.Instance.DirectSound.DsDevice);
                buffer3d = new Buffer3D(soundBuffer);
                buffer3d.MinDistance = 50;

                if (volume != -1)
                {
                    soundBuffer.Volume = volume;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar sonido est�tico WAV: " + soundPath, ex);
            }
        }

        /// <summary>
        /// Carga un archivo WAV de audio, con el volumen default del mismo
        /// Solo se pueden cargar sonidos WAV que sean MONO (1 channel).
        /// Sonidos stereos (2 channels) no pueden ser utilizados.
        /// </summary>
        /// <param name="soundPath">Path del archivo WAV</param>
        public void loadSound(string soundPath)
        {
            loadSound(soundPath, -1);
        }

        /// <summary>
        /// Reproduce el sonido, indicando si se hace con Loop.
        /// Si ya se est� reproduciedo, no vuelve a empezar.
        /// </summary>
        /// <param name="playLoop">TRUE para reproducir en loop</param>
        public void play(bool playLoop)
        {
            soundBuffer.Play(0, playLoop ? BufferPlayFlags.Looping : BufferPlayFlags.Default);
        }

        /// <summary>
        /// Reproduce el sonido, sin Loop.
        /// Si ya se est� reproduciedo, no vuelve a empezar.
        /// </summary>
        public void play()
        {
            play(false);
        }

        /// <summary>
        /// Pausa la ejecuci�n del sonido.
        /// Si el sonido no se estaba ejecutando, no hace nada.
        /// Si se hace stop() y luego play(), el sonido continua desde donde hab�a dejado la �ltima vez.
        /// </summary>
        public void stop()
        {
            soundBuffer.Stop();
        }

        /// <summary>
        /// Liberar recursos del sonido
        /// </summary>
        public void dispose()
        {
            if (soundBuffer != null && !soundBuffer.Disposed)
            {
                soundBuffer.Dispose();
                soundBuffer = null;
            }
        }

    }
}
