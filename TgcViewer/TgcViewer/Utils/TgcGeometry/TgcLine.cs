using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Shaders;
using TGC.Core.SceneLoader;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    ///     Herramienta para crear una línea 3D y renderizarla con color.
    /// </summary>
    public class TgcLine : IRenderObject
    {
        private Color color;

        protected Effect effect;

        protected string technique;

        private readonly CustomVertex.PositionColored[] vertices;

        public TgcLine()
        {
            vertices = new CustomVertex.PositionColored[2];
            color = Color.White;
            Enabled = true;
            AlphaBlendEnable = false;

            //Shader
            effect = GuiController.Instance.Shaders.VariosShader;
            technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Punto de inicio de la linea
        /// </summary>
        public Vector3 PStart { get; set; }

        /// <summary>
        ///     Punto final de la linea
        /// </summary>
        public Vector3 PEnd { get; set; }

        /// <summary>
        ///     Color de la linea
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        ///     Indica si la linea esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return PStart; }
        }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar la línea
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            var d3dDevice = GuiController.Instance.D3dDevice;
            var texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 1, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la línea
        /// </summary>
        public void dispose()
        {
        }

        /// <summary>
        ///     Actualizar parámetros de la línea en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var c = color.ToArgb();

            vertices[0] = new CustomVertex.PositionColored(PStart, c);
            vertices[1] = new CustomVertex.PositionColored(PEnd, c);
        }

        #region Creacion

        /// <summary>
        ///     Crea una línea en base a sus puntos extremos
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <returns>Línea creada</returns>
        public static TgcLine fromExtremes(Vector3 start, Vector3 end)
        {
            var line = new TgcLine();
            line.PStart = start;
            line.PEnd = end;
            line.updateValues();
            return line;
        }

        /// <summary>
        ///     Crea una línea en base a sus puntos extremos, con el color especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <param name="color">Color de la línea</param>
        /// <returns>Línea creada</returns>
        public static TgcLine fromExtremes(Vector3 start, Vector3 end, Color color)
        {
            var line = new TgcLine();
            line.PStart = start;
            line.PEnd = end;
            line.color = color;
            line.updateValues();
            return line;
        }

        #endregion Creacion
    }
}