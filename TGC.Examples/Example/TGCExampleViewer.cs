using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;
using TGC.Examples.UserControls.Networking;

namespace TGC.Examples.Example
{
    public abstract class TGCExampleViewer : TgcExample
    {
        private readonly Panel modifiersPanel;
        private readonly Panel spacePanel;

        public TGCExampleViewer(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir)
        {
            UserVars = userVars;
            this.modifiersPanel = modifiersPanel;
            spacePanel = new Panel();
            spacePanel.Size = new Size(10, 100);
        }

        /// <summary>
        ///     Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicacion.
        /// </summary>
        public TgcUserVars UserVars { get; set; }

        /// <summary>
        ///     Utilidad para crear modificadores de variables de usuario, que son mostradas en el panel derecho de la aplicacion.
        /// </summary>
        //public TgcModifiers Modifiers { get; set; }

        //TODO ARREGLAR TODOS LOS SUMMARY!!!!!!! de movida no tienen los return.
        private void AddModifier(UserControl tgcModifier)
        {
            tgcModifier.Dock = DockStyle.Top;
            modifiersPanel.Controls.Add(tgcModifier);
            modifiersPanel.Controls.Add(spacePanel);
        }

        #region Facilitadores de Modifiers

        /// <summary>
        ///     Modificador para valores Boolean
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="text">Descripcion</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCBooleanModifier AddBoolean(string varName, string text, bool defaultValue)
        {
            var booleanModifier = new TGCBooleanModifier(varName, text, defaultValue);
            AddModifier(booleanModifier);
            return booleanModifier;
        }

        /// <summary>
        ///     Modificador que agrega un Boton
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="text">Descripcion</param>
        /// <param name="defaultValue">Evento para manejer el click</param>
        public TGCButtonModifier AddButton(string varName, string text, EventHandler clickEventHandler)
        {
            var buttonModifier = new TGCButtonModifier(varName, text, clickEventHandler);
            AddModifier(buttonModifier);
            return buttonModifier;
        }

        /// <summary>
        ///     Modificador para elegir un color
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCColorModifier AddColor(string varName, Color defaultValue)
        {
            var colorModifier = new TGCColorModifier(varName, defaultValue);
            AddModifier(colorModifier);
            return colorModifier;
        }

        /// <summary>
        ///     Modificador para un intervalo discreto de valores creados con una estructura Enum
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="enumType">tipo del Enum a utilizar. Se obtiene con typeof(MyEnum)</param>
        /// <param name="defaultValue">variable del Enum que se carga como default. Ejemplo MyEnum.OpcionA</param>
        public TGCEnumModifier AddEnum(string varName, Type enumType, object defaultValue)
        {
            var enumModifier = new TGCEnumModifier(varName, enumType, defaultValue);
            AddModifier(enumModifier);
            return enumModifier;
        }

        /// <summary>
        ///     Modificador para elegir un archivo del FileSystem
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="defaultPath">path del archivo default</param>
        /// <param name="fileFilter">
        ///     string que especifca el filtro de archivos.
        ///     Ejemplo: .Imagenes JPG|*.jpg|Archivos XML|*.xml
        ///     Enviar null si no se quiere filtro
        /// </param>
        public TGCFileModifier AddFile(string varName, string defaultPath, string fileFilter)
        {
            var fileModifier = new TGCFileModifier(varName, defaultPath, fileFilter);
            AddModifier(fileModifier);
            return fileModifier;
        }

        /// <summary>
        ///     Modificador para valores Float
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="minValue">Valor minimo</param>
        /// <param name="maxValue">Valor maximo</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCFloatModifier AddFloat(string varName, float minValue, float maxValue, float defaultValue)
        {
            var floatModifier = new TGCFloatModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(floatModifier);
            return floatModifier;
        }

        /// <summary>
        ///     Modificador para valores Int
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="minValue">Valor minimo</param>
        /// <param name="maxValue">Valor maximo</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCIntModifier AddInt(string varName, int minValue, int maxValue, int defaultValue)
        {
            var intModifier = new TGCIntModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(intModifier);
            return intModifier;
        }

        /// <summary>
        ///     Modificador para un intervalo discreto de valores
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="values">Array de valores discretos</param>
        /// <param name="defaultIndex">Indice default del array</param>
        public TGCIntervalModifier AddInterval(string varName, object[] values, int defaultIndex)
        {
            var intervalModifier = new TGCIntervalModifier(varName, values, defaultIndex);
            AddModifier(intervalModifier);
            return intervalModifier;
        }

        /// <summary>
        ///     Modifier para Networking.
        ///     Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
        ///     Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
        /// </summary>
        /// <param name="varName">Identificador del modifier</param>
        /// <param name="serverName">Nombre default que va a usar el servidor</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente</param>
        /// <param name="port">Puerto en el cual se va a crear y buscar conexiones</param>
        /// <returns>Modificador creado</returns>
        public TGCNetworkingModifier AddNetworking(string varName, string serverName, string clientName, int port)
        {
            var networkingModifier = new TGCNetworkingModifier(varName, serverName, clientName, port);
            AddModifier(networkingModifier);
            return networkingModifier;
        }

        /// <summary>
        ///     Modifier para Networking.
        ///     Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
        ///     Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
        ///     Utiliza el puerto default del framework.
        /// </summary>
        /// <param name="varName">Identificador del modifier</param>
        /// <param name="serverName">Nombre default que va a usar el servidor</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente</param>
        /// <returns>Modificador creado</returns>
        public TGCNetworkingModifier AddNetworking(string varName, string serverName, string clientName)
        {
            var networkingModifier = new TGCNetworkingModifier(varName, serverName, clientName, TgcSocketMessages.DEFAULT_PORT);
            AddModifier(networkingModifier);
            return networkingModifier;
        }

        /// <summary>
        ///     Modificador para elegir una textura
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="values">path de la textura default</param>
        public TGCTextureModifier AddTexture(string varName, string defaultPath)
        {
            var textureModifier = new TGCTextureModifier(varName, defaultPath);
            AddModifier(textureModifier);
            return textureModifier;
        }

        /// <summary>
        ///     Modificador para valores floats (X,Y) o (U,V) de un vertice
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="minValue">Valor minimo</param>
        /// <param name="maxValue">Valor maximo</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCVertex2fModifier AddVertex2f(string varName, TGCVector2 minValue, TGCVector2 maxValue, TGCVector2 defaultValue)
        {
            var vertex2Modifier = new TGCVertex2fModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(vertex2Modifier);
            return vertex2Modifier;
        }

        /// <summary>
        ///     Modificador para valores floats (X,Y,Z) de un vertice
        /// </summary>
        /// <param name="varName">Nombre del modificador</param>
        /// <param name="minValue">Valor minimo</param>
        /// <param name="maxValue">Valor maximo</param>
        /// <param name="defaultValue">Valor default</param>
        public TGCVertex3fModifier AddVertex3f(string varName, TGCVector3 minValue, TGCVector3 maxValue, TGCVector3 defaultValue)
        {
            var vertex3Modifier = new TGCVertex3fModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(vertex3Modifier);
            return vertex3Modifier;
        }

        #endregion Facilitadores de Modifiers

        public void ClearModifiers()
        {
            modifiersPanel.Controls.Clear();
        }

        /// <summary>
        ///     Vuelve la configuracion de Render y otras cosas a la configuracion inicial
        /// </summary>
        public override void ResetDefaultConfig()
        {
            base.ResetDefaultConfig();
            UserVars.ClearVars();
            ClearModifiers();
        }
    }
}