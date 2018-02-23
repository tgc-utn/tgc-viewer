using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para reproducir archivos MP3s
    /// </summary>
    public class TgcMp3Player
    {
        /// <summary>
        ///     Estados del reproductor
        /// </summary>
        public enum States
        {
            Open,
            Playing,
            Paused,
            Stopped
        }

        // Constante con la longitud máxima de un nombre de archivo.
        private const int MAX_PATH = 260;

        // Constante con el formato de archivo a reproducir.
        private const string WINMM_FILE_TYPE = "MPEGVIDEO";

        // Alias asignado al archivo especificado.
        private const string WINMM_FILE_ALIAS = "TgcMp3MediaFile";

        /// <summary>
        ///     Path del archivo a reproducir
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Inicia la reproducción del archivo MP3.
        ///     <param name="playLoop">True para reproducir en loop</param>
        /// </summary>
        public void play(bool playLoop)
        {
            // Nos cersioramos que hay un archivo que reproducir.
            if (FileName != "")
            {
                // intentamos iniciar la reproducción.
                if (LoadFile())
                {
                    var command = new StringBuilder("play ");
                    command.Append(WINMM_FILE_ALIAS);
                    if (playLoop)
                    {
                        command.Append(" REPEAT");
                    }

                    var mciResul = mciSendString(command.ToString(), null, 0, 0);
                    if (mciResul != 0)
                    {
                        throw new Exception("Error al reproducir MP3: " + MciMensajesDeError(mciResul));
                    }
                }
                else
                {
                    throw new Exception("Error al abrir MP3: " + FileName);
                }
            }
            else
            {
                throw new Exception("No se ha especificado ninguna ruta de MP3");
            }
        }

        /// <summary>
        ///     Pausa la reproducción en proceso.
        /// </summary>
        public void pause()
        {
            // Enviamos el comando de pausa mediante la función mciSendString,
            var mciResul = mciSendString("pause " + WINMM_FILE_ALIAS, null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        ///     Continúa con la reproducción actual.
        /// </summary>
        public void resume()
        {
            // Enviamos el comando de pausa mediante la función mciSendString,
            var mciResul = mciSendString("resume " + WINMM_FILE_ALIAS, null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        ///     Detiene la reproducción del archivo de audio.
        /// </summary>
        public void stop()
        {
            // Detenemos la reproducción, mediante el comando adecuado.
            mciSendString("stop " + WINMM_FILE_ALIAS, null, 0, 0);
        }

        /// <summary>
        ///     Detiene la reproducción actual y cierra el archivo de audio.
        /// </summary>
        public void closeFile()
        {
            // Establecemos los comando detener reproducción y cerrar archivo.
            mciSendString("stop " + WINMM_FILE_ALIAS, null, 0, 0);
            mciSendString("Close " + WINMM_FILE_ALIAS, null, 0, 0);
        }

        /// <summary>
        ///     Obtiene el estado de la reproducción en proceso.
        /// </summary>
        /// <returns>Estado del reproducto</returns>
        public States getStatus()
        {
            var sbBuffer = new StringBuilder(MAX_PATH);
            // Obtenemos la información mediante el comando adecuado.
            mciSendString("status " + WINMM_FILE_ALIAS + " mode", sbBuffer, MAX_PATH, 0);
            var strState = sbBuffer.ToString();

            if (strState == "playing")
            {
                return States.Playing;
            }
            if (strState == "paused")
            {
                return States.Paused;
            }
            if (strState == "stopped")
            {
                return States.Stopped;
            }
            return States.Open;
        }

        /// <summary>
        ///     Mueve el apuntador de archivo al inicio del mismo.
        /// </summary>
        public void goToBeginning()
        {
            // Establecemos la cadena de comando para mover el apuntador del archivo,
            // al inicio de este.
            var mciResul = mciSendString("seek " + WINMM_FILE_ALIAS + " to start", null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        ///     Mueve el apuntador de archivo al final del mismo.
        /// </summary>
        public void goToEnd()
        {
            // Establecemos la cadena de comando para mover el apuntador del archivo,
            // al final de este.
            var mciResul = mciSendString("seek " + WINMM_FILE_ALIAS + " to end", null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        #region DLLs externas

        [DllImport("winmm.dll")]
        public static extern int mciSendString(string lpstrCommand,
            StringBuilder lpstrReturnString, int uReturnLengh, int hwndCallback);

        [DllImport("winmm.dll")]
        public static extern int mciGetErrorString(int fwdError, StringBuilder lpszErrorText,
            int cchErrorText);

        [DllImport("winmm.dll")]
        public static extern int waveOutGetNumDevs();

        [DllImport("kernel32.dll")]
        public static extern int GetShortPathName(string lpszLongPath,
            StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("kernel32.dll")]
        public static extern int GetLongPathName(string
            lpszShortPath, StringBuilder lpszLongPath, int cchBuffer);

        #endregion DLLs externas

        #region Metodos auxiliares

        /// <summary>
        ///     Método para convertir un nombre de archivo largo en uno corto,
        ///     necesario para usarlo como parámetro de la función MciSendString.
        /// </summary>
        /// <param name="nombreLargo">Nombre y ruta del archivo a convertir.</param>
        /// <returns>Nombre corto del archivo especificado.</returns>
        private string NombreCorto(string nombreLargo)
        {
            // Creamos un buffer usando un constructor de la clase StringBuider.
            var sBuffer = new StringBuilder(MAX_PATH);
            // intentamos la conversión del archivo.
            if (GetShortPathName(nombreLargo, sBuffer, MAX_PATH) > 0)
                // si la función ha tenido éxito devolvemos el buffer formateado
                // a tipo string.
                return sBuffer.ToString();
            return "";
        }

        /// <summary>
        ///     Método que convierte un nombre de archivo corto, en uno largo.
        /// </summary>
        /// <param name="NombreCorto">Nombre del archivo a convertir.</param>
        /// <returns>Cadena con el nombre de archivo resultante.</returns>
        public string NombreLargo(string NombreCorto)
        {
            var sbBuffer = new StringBuilder(MAX_PATH);
            if (GetLongPathName(NombreCorto, sbBuffer, MAX_PATH) > 0)
                return sbBuffer.ToString();
            return "";
        }

        /// <summary>
        ///     Método para convertir los mensajes de error numéricos, generados por la
        ///     función mciSendString, en su correspondiente cadena de caracteres.
        /// </summary>
        /// <param name="ErrorCode">
        ///     Código de error devuelto por la función
        ///     mciSendString
        /// </param>
        /// <returns>Cadena de tipo string, con el mensaje de error</returns>
        private string MciMensajesDeError(int ErrorCode)
        {
            // Creamos un buffer, con suficiente espacio, para almacenar el mensaje
            // devuelto por la función.
            var sbBuffer = new StringBuilder(MAX_PATH);
            // Obtenemos la cadena de mensaje.
            if (mciGetErrorString(ErrorCode, sbBuffer, MAX_PATH) != 0)
                // Sí la función ha tenido éxito, valor devuelto diferente de 0,
                // devolvemos el valor del buffer, formateado a string.
                return sbBuffer.ToString();
            return "";
        }

        /// <summary>
        ///     Abre el archivo MP3 específicado.
        /// </summary>
        /// <returns>
        ///     Verdadero si se tuvo éxito al abrir el archivo
        ///     falso en caso contrario.
        /// </returns>
        private bool LoadFile()
        {
            // verificamos que el archivo existe; si no, regresamos falso.
            if (!File.Exists(FileName)) return false;
            // obtenemos el nombre corto del archivo.
            var nombreCorto = NombreCorto(FileName);
            // intentamos abrir el archivo, utilizando su nombre corto
            // y asignándole un alias para trabajar con él.
            if (mciSendString("open " + nombreCorto + " type " + WINMM_FILE_TYPE +
                              " alias " + WINMM_FILE_ALIAS, null, 0, 0) == 0)
                // si el resultado es igual a 0, la función tuvo éxito,
                // devolvemos verdadero.
                return true;
            return false;
        }

        #endregion Metodos auxiliares

        #region Metodos aun no probados

        /*
        FIXME
        //Especificamos el delegado al que se va a asociar el evento.
        public delegate void ReproductorMessageHandler(string Msg);
        //Declaramos nuestro evento.
        public event ReproductorMessageHandler ReproductorMessageEvent;

        /// <summary>
        /// Inicia la reproducción desde una posición específica.
        /// </summary>
        /// <param name="Desde">Nuevo valor de la posición a iniciar</param>
        public void PlayFrom(long Desde)
        {
            int mciResul = mciSendString("play " + sAlias + " from " +
            (Desde * 1000).ToString(), null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        /// Modifica la velocidad actual de reproducción.
        /// </summary>
        /// <param name="Tramas">Nuevo valor de la velocidad.</param>
        public void setSpeed(int speed)
        {
            // Establecemos la nueva velocidad pasando como parámetro,
            // la cadena adecuada, incluyendo el nuevo valor de la velocidad,
            // medido en tramas por segundo.
            int mciResul = mciSendString("set " + sAlias + " tempo " +
            speed.ToString(), null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        /// Devuelve el número de dispositivos de salida,
        /// instalados en nuestro sistema.
        /// </summary>
        /// <returns>Número de dispositivos.</returns>
        public int getNumSoundDevices()
        {
            return waveOutGetNumDevs();
        }

        /// <summary>
        /// Mueve el apuntador del archivo a la posición especificada.
        /// </summary>
        /// <param name="NuevaPosicion">Nueva posición</param>
        public void Reposicionar(int NuevaPosicion)
        {
            // Enviamos la cadena de comando adecuada a la función mciSendString,
            // pasando como parte del mismo, la cantidad a mover el apuntador de
            // archivo.
            int mciResul = mciSendString("seek " + sAlias + " to " +
            (NuevaPosicion * 1000).ToString(), null, 0, 0);
            if (mciResul == 0)
                ReproductorEstado("Nueva Posición: " + NuevaPosicion.ToString());
            else
                ReproductorEstado(MciMensajesDeError(mciResul));
        }

        /// <summary>
        /// Calcula la posición actual del apuntador del archivo.
        /// </summary>
        /// <returns>Posición actual</returns>
        public long CalcularPosicion()
        {
            StringBuilder sbBuffer = new StringBuilder(MAX_PATH);
            // Establecemos el formato de tiempo.
            mciSendString("set " + sAlias + " time format milliseconds", null, 0, 0);
            // Enviamos el comando para conocer la posición del apuntador.
            mciSendString("status " + sAlias + " position", sbBuffer, MAX_PATH, 0);

            // Sí hay información en el buffer,
            if (sbBuffer.ToString() != "")
                // la devolvemos, eliminando el formato de milisegundos
                // y formateando la salida a entero largo;
                return long.Parse(sbBuffer.ToString()) / 1000;
            else // si no, devolvemos cero.
                return 0L;
        }
        /// <summary>
        /// Devuelve una cadena con la información de posición del apuntador del archivo.
        /// </summary>
        /// <returns>Cadena con la información.</returns>
        public string Posicion()
        {
            // obtenemos los segundos.
            long sec = CalcularPosicion();
            long mins;

            // Si la cantidad de segundos es menor que 60 (1 minuto),
            if (sec < 60)
                // devolvemos la cadena formateada a 0:Segundos.
                return "0:" + String.Format("{0:D2}", sec);
            // Si los segundos son mayores que 59 (60 o más),
            else if (sec > 59)
            {
                // calculamos la cantidad de minutos,
                mins = (int)(sec / 60);
                // restamos los segundos de la cantida de minutos obtenida,
                sec = sec - (mins * 60);
                // devolvemos la cadena formateada a Minustos:Segundos.
                return String.Format("{0:D2}", mins) + ":" + String.Format("{0:D2}", sec);
            }
            else // en caso de obtener un valor menos a 0, devolvemos una cadena vacía.
                return "";
        }
        /// <summary>
        /// Cálcula el tamaño del archivo abierto para reproducción.
        /// </summary>
        /// <returns>Tamaño en segundos del archivo.</returns>
        public long CalcularTamaño()
        {
            StringBuilder sbBuffer = new StringBuilder(MAX_PATH);
            mciSendString("set " + sAlias + " time format milliseconds", null, 0, 0);
            // Obtenemos el largo del archivo, en millisegundos.
            mciSendString("status " + sAlias + " length", sbBuffer, MAX_PATH, 0);

            // Sí el buffer contiene información,
            if (sbBuffer.ToString() != "")
                // la devolvemos, formateando la salida a entero largo;
                return long.Parse(sbBuffer.ToString()) / 1000;
            else // si no, devolvemos cero.
                return 0L;
        }
        /// <summary>
        /// Obtiene una cadena con la información sobre el tamaño (largo) del archivo.
        /// </summary>
        /// <returns>Largo del archivo de audio.</returns>
        public string Tamaño()
        {
            long sec = CalcularTamaño();
            long mins;

            // Si la cantidad de segundos es menor que 60 (1 minuto),
            if (sec < 60)
                // devolvemos la cadena formateada a 0:Segundos.
                return "0:" + String.Format("{0:D2}", sec);
            // Si los segundos son mayores que 59 (60 o más),
            else if (sec > 59)
            {
                mins = (int)(sec / 60);
                sec = sec - (mins * 60);
                // devolvemos la cadena formateada a Minustos:Segundos.
                return String.Format("{0:D2}", mins) + ":" + String.Format("{0:D2}", sec);
            }
            else
                return "";
        }

        private void ReproductorEstado(string text)
        {
            if (ReproductorMessageEvent != null)
            {
                ReproductorMessageEvent.Invoke(text);
            }
        }

        */

        #endregion Metodos aun no probados
    }
}