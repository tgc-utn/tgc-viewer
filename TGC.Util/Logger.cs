using Microsoft.DirectX;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Utils;

namespace TGC.Util
{
    /// <summary>
    ///     Loguea en diferentes modalidades el texto solicitado en el textbox configurado.
    /// </summary>
    public class Logger
    {
        private readonly RichTextBox logArea;

        public Logger(RichTextBox logArea)
        {
            this.logArea = logArea;
            clear();
        }

        /// <summary>
        ///     Logea el string txt en la consola de mensajes con el color indicado
        /// </summary>
        public void log(string txt, Color color)
        {
            logArea.SelectionColor = color;
            logArea.SelectedText = txt;
            logArea.SelectedText = Environment.NewLine;

            //ir al final
            logArea.ScrollToCaret();
        }

        /// <summary>
        ///     Logea el string txt en la consola de mensajes con el color default
        /// </summary>
        public void log(string txt)
        {
            log(txt, Color.Black);
        }

        /// <summary>
        ///     Logea un Vector3 en la consola de mensajes con el color indicado
        /// </summary>
        public void logVector3(Vector3 v, Color color)
        {
            log(TgcParserUtils.printVector3(v), color);
        }

        /// <summary>
        ///     Logea un Vector3 en la consola de mensajes con el color default
        /// </summary>
        public void logVector3(Vector3 v)
        {
            logVector3(v, Color.Black);
        }

        /// <summary>
        ///     Loguea el string txt en la consola de mensajes con color rojo
        ///     indicando que se trata de un error
        /// </summary>
        /// <param name="txt">string con el texto a loguear</param>
        public void logError(string txt, Exception e)
        {
            log(txt + Environment.NewLine + e, Color.Red);
        }

        /// <summary>
        ///     Loguea el string txt en la consola de mensajes con color rojo
        ///     indicando que se trata de un error
        /// </summary>
        /// <param name="txt">string con el texto a loguear</param>
        public void logError(string txt)
        {
            log(txt + Environment.NewLine, Color.Red);
        }

        /// <summary>
        ///     Limpia la consola de logueo
        /// </summary>
        public void clear()
        {
            logArea.Clear();
        }

        /// <summary>
        ///     Metodo para logear en la consola de mensajes cuando estamos desde otro Thread
        ///     distinto del de la interfaz grafica
        /// </summary>
        public static void logInThread(string txt, Color color)
        {
            LogInThreadHandler handler = GuiController.Instance.Logger.log;
            GuiController.Instance.MainForm.BeginInvoke(handler, txt, color);
        }

        private delegate void LogInThreadHandler(string txt, Color color);
    }
}