using System;
using System.IO;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

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
        public Tgc3dSound(string soundPath, Vector3 position, DirectSound device)
        {
            loadSound(soundPath, device);
            Position = position;
        }

        /// <summary>
        ///     Buffer con la información del sonido cargado
        /// </summary>
        public SecondarySoundBuffer SoundBuffer { get; private set; }

        /// <summary>
        ///     Buffer que manipula la parte 3D del sonido cargado
        /// </summary>
        public SoundBuffer3D Buffer3d { get; private set; }

        /// <summary>
        ///     Posición del sonido dentro del espacio.
        ///     La forma de escuchar el sonido varia según esta ubicación y la posición
        ///     del Listener3D de sonidos.
        /// </summary>
        public Vector3 Position
        {
            get { return Buffer3d.Position; }
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
        public void loadSound(string soundPath, int volume, DirectSound device)
        {
            try
            {
                dispose();

                var stream = new SoundStream(File.OpenRead(soundPath));
                var waveFormat = stream.Format;
                stream.Close();

                var bufferDescription = new SoundBufferDescription();
                bufferDescription.Flags = BufferFlags.Control3D;

                if (volume != -1)
                {
                    bufferDescription.Flags = BufferFlags.Control3D | BufferFlags.ControlVolume;
                }

                SoundBuffer = new SecondarySoundBuffer(device, bufferDescription);
                Buffer3d = new SoundBuffer3D(SoundBuffer);
                Buffer3d.MinDistance = 50;

                // Get Capabilties from secondary sound buffer
                var capabilities = SoundBuffer.Capabilities;

                // Lock the buffer
                DataStream dataPart2;
                var dataPart1 = SoundBuffer.Lock(0, capabilities.BufferBytes, LockFlags.EntireBuffer, out dataPart2);

                // Fill the buffer with some sound
                int numberOfSamples = capabilities.BufferBytes / waveFormat.BlockAlign;
                //TODO: Direct option investigate
                //SoundBuffer.Write(stream.ToDataStream(), 0, numberOfSamples, capabilities.BufferBytes, LockFlags.EntireBuffer);

                for (int i = 0; i < numberOfSamples; i++)
                {
                    byte[] buffer = null;
                    stream.Read(buffer, i, capabilities.BufferBytes);
                    dataPart1.Write(buffer, i, capabilities.BufferBytes);
                    dataPart2.Write(buffer, i, capabilities.BufferBytes);
                }

                // Unlock the buffer
                SoundBuffer.Unlock(dataPart1, dataPart2);

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
        public void loadSound(string soundPath, DirectSound device)
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
            SoundBuffer.Play(0, playLoop ? PlayFlags.Looping : PlayFlags.None);
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
            if (SoundBuffer != null) //TODO: Check SoundBuffer.Dispossed error
            {
                SoundBuffer.Dispose();
                SoundBuffer = null;
            }
        }
    }
}