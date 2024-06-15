﻿using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Core.Mathematica;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Example
{
    /// <summary>
    /// Clase especializada para poder tener un ejemplo en el viewer y algunas herramientas visuales para interactuar.
    /// </summary>
    public abstract class TGCExampleViewer : TGCExample
    {
        private readonly Panel modifiersPanel;
        private readonly Panel spacePanel;

        /// <summary>
        /// Crea un ejemplo con lo necesario para realizar un juego dentro del viewer con modificadores.
        /// </summary>
        /// <param name="mediaDir">Ruta donde estan los Media.</param>
        /// <param name="shadersDir">Ruta donde estan los Shaders.</param>
        /// <param name="userVars">Variables de usuario visibles.</param>
        /// <param name="modifiersPanel">Modificadores disponibles en el ejemplo.</param>
        public TGCExampleViewer(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir)
        {
            UserVars = userVars;
            this.modifiersPanel = modifiersPanel;
            spacePanel = new Panel();
            spacePanel.Size = new Size(10, 100);
        }

        /// <summary>
        /// Utilidad para administrar las variables de usuario visibles en el panel derecho de la aplicacion.
        /// </summary>
        public TgcUserVars UserVars { get; set; }

        /// <summary>
        /// Agrega un Modifier al panel y un espacio despues.
        /// </summary>
        /// <param name="tgcModifier">Modifier a agregar.</param>
        protected void AddModifier(UserControl tgcModifier)
        {
            tgcModifier.Dock = DockStyle.Top;
            modifiersPanel.Controls.Add(tgcModifier);
            modifiersPanel.Controls.Add(spacePanel);
        }

        #region Facilitadores de Modifiers

        /// <summary>
        /// Modificador para valores Boolean.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="text">Descripcion.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCBooleanModifier AddBoolean(string varName, string text, bool defaultValue)
        {
            var booleanModifier = new TGCBooleanModifier(varName, text, defaultValue);
            AddModifier(booleanModifier);
            return booleanModifier;
        }

        /// <summary>
        /// Modificador que agrega un Boton.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param></param>
        /// <param name="text">Descripcion.</param>
        /// <param name="clickEventHandler">Evento para manejer el click.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCButtonModifier AddButton(string varName, string text, EventHandler clickEventHandler)
        {
            var buttonModifier = new TGCButtonModifier(varName, text, clickEventHandler);
            AddModifier(buttonModifier);
            return buttonModifier;
        }

        /// <summary>
        /// Modificador para elegir un color.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCColorModifier AddColor(string varName, Color defaultValue)
        {
            var colorModifier = new TGCColorModifier(varName, defaultValue);
            AddModifier(colorModifier);
            return colorModifier;
        }

        /// <summary>
        /// Modificador para un intervalo discreto de valores creados con una estructura Enum.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="enumType">Tipo del Enum a utilizar. Se obtiene con typeof(MyEnum).</param>
        /// <param name="defaultValue">Variable del Enum que se carga como default. Ejemplo MyEnum.OpcionA.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCEnumModifier AddEnum(string varName, Type enumType, object defaultValue)
        {
            var enumModifier = new TGCEnumModifier(varName, enumType, defaultValue);
            AddModifier(enumModifier);
            return enumModifier;
        }

        /// <summary>
        /// Modificador para elegir un archivo del FileSystem.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="defaultPath">Path del archivo default.</param>
        /// <param name="fileFilter">
        /// string que especifca el filtro de archivos.
        /// Ejemplo: .Imagenes JPG|*.jpg|Archivos XML|*.xml
        /// Enviar null si no se quiere filtro.
        /// </param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCFileModifier AddFile(string varName, string defaultPath, string fileFilter)
        {
            var fileModifier = new TGCFileModifier(varName, defaultPath, fileFilter);
            AddModifier(fileModifier);
            return fileModifier;
        }

        /// <summary>
        /// Modificador para valores Float.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="minValue">Valor minimo.</param>
        /// <param name="maxValue">Valor maximo.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCFloatModifier AddFloat(string varName, float minValue, float maxValue, float defaultValue)
        {
            var floatModifier = new TGCFloatModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(floatModifier);
            return floatModifier;
        }

        /// <summary>
        /// Modificador para valores Int.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="minValue">Valor minimo.</param>
        /// <param name="maxValue">>Valor maximo.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCIntModifier AddInt(string varName, int minValue, int maxValue, int defaultValue)
        {
            var intModifier = new TGCIntModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(intModifier);
            return intModifier;
        }

        /// <summary>
        /// Modificador para un intervalo discreto de valores.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="values">Array de valores discretos.</param>
        /// <param name="defaultIndex">Indice default del array.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCIntervalModifier AddInterval(string varName, object[] values, int defaultIndex)
        {
            var intervalModifier = new TGCIntervalModifier(varName, values, defaultIndex);
            AddModifier(intervalModifier);
            return intervalModifier;
        }

        /// <summary>
        /// Modificador para elegir una textura.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="defaultPath">Path de la textura default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCTextureModifier AddTexture(string varName, string defaultPath)
        {
            var textureModifier = new TGCTextureModifier(varName, defaultPath);
            AddModifier(textureModifier);
            return textureModifier;
        }

        /// <summary>
        /// Modificador para valores floats (X,Y) o (U,V) de un vertice.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="minValue">Valor minimo.</param>
        /// <param name="maxValue">Valor maximo.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCVertex2fModifier AddVertex2f(string varName, TGCVector2 minValue, TGCVector2 maxValue, TGCVector2 defaultValue)
        {
            var vertex2Modifier = new TGCVertex2fModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(vertex2Modifier);
            return vertex2Modifier;
        }

        /// <summary>
        /// Modificador para valores floats (X,Y,Z) de un vertice.
        /// </summary>
        /// <param name="varName">Nombre del modificador.</param>
        /// <param name="minValue">Valor minimo.</param>
        /// <param name="maxValue">Valor maximo.</param>
        /// <param name="defaultValue">Valor default.</param>
        /// <returns>Modificador que se agrego.</returns>
        public TGCVertex3fModifier AddVertex3f(string varName, TGCVector3 minValue, TGCVector3 maxValue, TGCVector3 defaultValue)
        {
            var vertex3Modifier = new TGCVertex3fModifier(varName, minValue, maxValue, defaultValue);
            AddModifier(vertex3Modifier);
            return vertex3Modifier;
        }

        #endregion Facilitadores de Modifiers

        /// <summary>
        /// Limpia los Modifiers
        /// </summary>
        public void ClearModifiers()
        {
            modifiersPanel.Controls.Clear();
        }

        /// <summary>
        /// Vuelve la configuracion de render y otras cosas a los valores iniciales.
        /// </summary>
        public override void ResetDefaultConfig()
        {
            base.ResetDefaultConfig();
            UserVars.ClearVars();
            ClearModifiers();
        }
    }
}