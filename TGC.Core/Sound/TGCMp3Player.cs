using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TGC.Core.Sound
{
    /// <summary>
    ///     Herramienta para reproducir archivos MP3s
    /// </summary>
    public class TGCMP3Player
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

        // Constante con la longitud maxima de un nombre de archivo.
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
        ///     Inicia la reproduccion del archivo MP3.
        ///     <param name="playLoop">True para reproducir en loop</param>
        /// </summary>
        public void Play(bool playLoop)
        {
            // Nos cersioramos que hay un archivo que reproducir.
            if (FileName != "")
            {
                // intentamos iniciar la reproducci�n.
                if (LoadFile())
                {
                    var command = new StringBuilder("Play ");
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
        ///     Pausa la reproduccion en proceso.
        /// </summary>
        public void Pause()
        {
            // Enviamos el comando de pausa mediante la funcion mciSendString,
            var mciResul = mciSendString("pause " + WINMM_FILE_ALIAS, null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        ///     Continua con la reproduccion actual.
        /// </summary>
        public void Resume()
        {
            // Enviamos el comando de pausa mediante la funcion mciSendString,
            var mciResul = mciSendString("resume " + WINMM_FILE_ALIAS, null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        ///     Detiene la reproduccion del archivo de audio.
        /// </summary>
        public void Stop()
        {
            // Detenemos la reproduccion, mediante el comando adecuado.
            mciSendString("Stop " + WINMM_FILE_ALIAS, null, 0, 0);
        }

        /// <summary>
        ///     Detiene la reproduccion actual y cierra el archivo de audio.
        /// </summary>
        public void CloseFile()
        {
            // Establecemos los comando detener reproduccion y cerrar archivo.
            mciSendString("Stop " + WINMM_FILE_ALIAS, null, 0, 0);
            mciSendString("Close " + WINMM_FILE_ALIAS, null, 0, 0);
        }

        /// <summary>
        ///     Obtiene el estado de la reproduccion en proceso.
        /// </summary>
        /// <returns>Estado del reproducto</returns>
        public States GetStatus()
        {
            var sbBuffer = new StringBuilder(MAX_PATH);
            // Obtenemos la informacion mediante el comando adecuado.
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
        public void GoToBeginning()
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
        public void GoToEnd()
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
        ///     Metodo para convertir un nombre de archivo largo en uno corto,
        ///     necesario para usarlo como par�metro de la funci�n MciSendString.
        /// </summary>
        /// <param name="nombreLargo">Nombre y ruta del archivo a convertir.</param>
        /// <returns>Nombre corto del archivo especificado.</returns>
        private string NombreCorto(string nombreLargo)
        {
            // Creamos un buffer usando un constructor de la clase StringBuider.
            var sBuffer = new StringBuilder(MAX_PATH);
            // intentamos la conversi�n del archivo.
            if (GetShortPathName(nombreLargo, sBuffer, MAX_PATH) > 0)
                // si la funci�n ha tenido �xito devolvemos el buffer formateado
                // a tipo string.
                return sBuffer.ToString();
            return "";
        }

        /// <summary>
        ///     Metodo que convierte un nombre de archivo corto, en uno largo.
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
        ///     Metodo para convertir los mensajes de error num�ricos, generados por la
        ///     funci�n mciSendString, en su correspondiente cadena de caracteres.
        /// </summary>
        /// <param name="ErrorCode">
        ///     Codigo de error devuelto por la funci�n
        ///     mciSendString
        /// </param>
        /// <returns>Cadena de tipo string, con el mensaje de error</returns>
        private string MciMensajesDeError(int ErrorCode)
        {
            // Creamos un buffer, con suficiente espacio, para almacenar el mensaje
            // devuelto por la funci�n.
            var sbBuffer = new StringBuilder(MAX_PATH);
            // Obtenemos la cadena de mensaje.
            if (mciGetErrorString(ErrorCode, sbBuffer, MAX_PATH) != 0)
                // S� la funcion ha tenido �xito, valor devuelto diferente de 0,
                // devolvemos el valor del buffer, formateado a string.
                return sbBuffer.ToString();
            return "";
        }

        /// <summary>
        ///     Abre el archivo MP3 espec�ficado.
        /// </summary>
        /// <returns>
        ///     Verdadero si se tuvo exito al abrir el archivo
        ///     falso en caso contrario.
        /// </returns>
        private bool LoadFile()
        {
            // verificamos que el archivo existe; si no, regresamos falso.
            if (!File.Exists(FileName)) return false;
            // obtenemos el nombre corto del archivo.
            var nombreCorto = NombreCorto(FileName);
            // intentamos abrir el archivo, utilizando su nombre corto
            // y asignandole un alias para trabajar con �l.
            if (mciSendString("open " + nombreCorto + " type " + WINMM_FILE_TYPE +
                              " alias " + WINMM_FILE_ALIAS, null, 0, 0) == 0)
                // si el resultado es igual a 0, la funcion tuvo exito,
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
        /// Inicia la reproducci�n desde una posicion especifica.
        /// </summary>
        /// <param name="Desde">Nuevo valor de la posici�n a iniciar</param>
        public void PlayFrom(long Desde)
        {
            int mciResul = mciSendString("Play " + sAlias + " from " +
            (Desde * 1000).ToString(), null, 0, 0);
            if (mciResul != 0)
            {
                throw new Exception(MciMensajesDeError(mciResul));
            }
        }

        /// <summary>
        /// Modifica la velocidad actual de reproduccion.
        /// </summary>
        /// <param name="Tramas">Nuevo valor de la velocidad.</param>
        public void setSpeed(int speed)
        {
            // Establecemos la nueva velocidad pasando como parametro,
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
        /// Devuelve el numero de dispositivos de salida,
        /// instalados en nuestro sistema.
        /// </summary>
        /// <returns>N�mero de dispositivos.</returns>
        public int getNumSoundDevices()
        {
            return waveOutGetNumDevs();
        }

        /// <summary>
        /// Mueve el apuntador del archivo a la posicion especificada.
        /// </summary>
        /// <param name="NuevaPosicion">Nueva posicion</param>
        public void Reposicionar(int NuevaPosicion)
        {
            // Enviamos la cadena de comando adecuada a la funci�n mciSendString,
            // pasando como parte del mismo, la cantidad a mover el apuntador de
            // archivo.
            int mciResul = mciSendString("seek " + sAlias + " to " +
            (NuevaPosicion * 1000).ToString(), null, 0, 0);
            if (mciResul == 0)
                ReproductorEstado("Nueva Posici�n: " + NuevaPosicion.ToString());
            else
                ReproductorEstado(MciMensajesDeError(mciResul));
        }

        /// <summary>
        /// Calcula la posicion actual del apuntador del archivo.
        /// </summary>
        /// <returns>Posici�n actual</returns>
        public long CalcularPosicion()
        {
            StringBuilder sbBuffer = new StringBuilder(MAX_PATH);
            // Establecemos el formato de tiempo.
            mciSendString("set " + sAlias + " time format milliseconds", null, 0, 0);
            // Enviamos el comando para conocer la posici�n del apuntador.
            mciSendString("status " + sAlias + " position", sbBuffer, MAX_PATH, 0);

            // S� hay informaci�n en el buffer,
            if (sbBuffer.ToString() != "")
                // la devolvemos, eliminando el formato de milisegundos
                // y formateando la salida a entero largo;
                return long.Parse(sbBuffer.ToString()) / 1000;
            else // si no, devolvemos cero.
                return 0L;
        }
        /// <summary>
        /// Devuelve una cadena con la informacion de posici�n del apuntador del archivo.
        /// </summary>
        /// <returns>Cadena con la informacion.</returns>
        public string Posicion()
        {
            // obtenemos los segundos.
            long sec = CalcularPosicion();
            long mins;

            // Si la cantidad de segundos es menor que 60 (1 minuto),
            if (sec < 60)
                // devolvemos la cadena formateada a 0:Segundos.
                return "0:" + String.Format("{0:D2}", sec);
            // Si los segundos son mayores que 59 (60 o m�s),
            else if (sec > 59)
            {
                // calculamos la cantidad de minutos,
                mins = (int)(sec / 60);
                // restamos los segundos de la cantida de minutos obtenida,
                sec = sec - (mins * 60);
                // devolvemos la cadena formateada a Minustos:Segundos.
                return String.Format("{0:D2}", mins) + ":" + String.Format("{0:D2}", sec);
            }
            else // en caso de obtener un valor menos a 0, devolvemos una cadena vac�a.
                return "";
        }
        /// <summary>
        /// Calcula el tamano del archivo abierto para reproduccion.
        /// </summary>
        /// <returns>Tamano en segundos del archivo.</returns>
        public long CalcularTama�o()
        {
            StringBuilder sbBuffer = new StringBuilder(MAX_PATH);
            mciSendString("set " + sAlias + " time format milliseconds", null, 0, 0);
            // Obtenemos el largo del archivo, en millisegundos.
            mciSendString("status " + sAlias + " length", sbBuffer, MAX_PATH, 0);

            // Si el buffer contiene informacion,
            if (sbBuffer.ToString() != "")
                // la devolvemos, formateando la salida a entero largo;
                return long.Parse(sbBuffer.ToString()) / 1000;
            else // si no, devolvemos cero.
                return 0L;
        }
        /// <summary>
        /// Obtiene una cadena con la informaci�n sobre el tamano (largo) del archivo.
        /// </summary>
        /// <returns>Largo del archivo de audio.</returns>
        public string Tamano()
        {
            long sec = CalcularTamano();
            long mins;

            // Si la cantidad de segundos es menor que 60 (1 minuto),
            if (sec < 60)
                // devolvemos la cadena formateada a 0:Segundos.
                return "0:" + String.Format("{0:D2}", sec);
            // Si los segundos son mayores que 59 (60 o m�s),
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