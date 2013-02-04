using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.Ui
{
    /// <summary>
    /// Configuracion de arranque de TgcViewer
    /// </summary>
    public class TgcViewerConfig
    {
        /// <summary>
        /// Indica si la aplicacion arranca en modo full screen
        /// </summary>
        public bool fullScreenMode;

        /// <summary>
        /// Nombre del primer ejemplo a ejecutar
        /// </summary>
        public string defaultExampleName;

        /// <summary>
        /// Categoria del primer ejemplo a ejecutar
        /// </summary>
        public string defaultExampleCategory;

        /// <summary>
        /// En true muestra el panel derecho de modifiers
        /// </summary>
        public bool showModifiersPanel;

        /// <summary>
        /// Titulo de la ventana de la aplicacion
        /// </summary>
        public string title;

        /// <summary>
        /// En true muestra la barra superior de la ventana de la aplicacion
        /// </summary>
        public bool showTitleBar;

        /// <summary>
        /// Crear con configuracion default
        /// </summary>
        public TgcViewerConfig()
        {
            fullScreenMode = false;
            defaultExampleName = "Logo de TGC";
            defaultExampleCategory = "Otros";
            showModifiersPanel = true;
            title = "TgcViewer - Técnicas de Gráficos por Computadora - UTN - FRBA";
            showTitleBar = true;
        }

        /// <summary>
        /// Crea la configuracion a partir de parametros pasados por consola
        /// Ejemplo: fullScreenMode=true defaultExampleName="Bump Mapping" defaultExampleCategory=Lights showModifiersPanel=false title="Mi titulo" showTitleBar=false
        /// </summary>
        public void parseCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string[] values = args[i].Split('=');
                string key = values[0].Trim();
                string value = values[1].Trim();

                //fullScreenMode=bool
                if (key == "fullScreenMode")
                {
                    fullScreenMode = bool.Parse(value);
                }
                //defaultExampleName=string (si tiene espacios ponerle comillas dobles)
                else if (key == "defaultExampleName")
                {
                    defaultExampleName = value;
                }
                //defaultExampleCategory=string (si tiene espacios ponerle comillas dobles)
                else if (key == "defaultExampleCategory")
                {
                    defaultExampleCategory = value;
                }
                //showModifiersPanel=string
                else if (key == "showModifiersPanel")
                {
                    showModifiersPanel = bool.Parse(value);
                }
                //title=string (si tiene espacios ponerle comillas dobles)
                else if (key == "title")
                {
                    title = value;
                }
                //showTitleBar=string
                else if (key == "showTitleBar")
                {
                    showTitleBar = bool.Parse(value);
                }
            }
            


        }

    }
}
