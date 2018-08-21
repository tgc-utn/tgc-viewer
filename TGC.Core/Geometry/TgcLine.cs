using SharpDX;
using SharpDX.Direct3D9;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para crear una l�nea 3D y renderizarla con color.
    /// </summary>
    public class TgcLine : IRenderObject
    {
        private readonly CustomVertex.PositionColored[] vertices;

        public TgcLine()
        {
            vertices = new CustomVertex.PositionColored[2];
            Color = Color.White;
            Enabled = true;
            AlphaBlendEnable = false;

            //Shader
            Effect = TgcShaders.Instance.VariosShader;
            Technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Punto de inicio de la linea
        /// </summary>
        public TGCVector3 PStart { get; set; }

        /// <summary>
        ///     Punto final de la linea
        /// </summary>
        public TGCVector3 PEnd { get; set; }

        /// <summary>
        ///     Color de la linea
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Indica si la linea esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        public TGCVector3 Position
        {
            //Lo correcto ser�a calcular el centro, pero con un extremo es suficiente.
            get { return PStart; }
        }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique { get; set; }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por v�rtice de canal Alpha.
        ///     Por default est� deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Actualizar par�metros de la l�nea en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var c = Color.ToArgb();

            vertices[0] = new CustomVertex.PositionColored(PStart, c);
            vertices[1] = new CustomVertex.PositionColored(PEnd, c);
        }

        /// <summary>
        ///     Renderizar la l�nea
        /// </summary>
        public void Render()
        {
            if (!Enabled)
                return;

            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            TgcShaders.Instance.setShaderMatrixIdentity(Effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColored;
            Effect.Technique = Technique;

            //Render con shader
            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, 1, vertices);
            Effect.EndPass();
            Effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la l�nea
        /// </summary>
        public void Dispose()
        {
            //TODO porque esto esta vacio?
        }

        #region Creacion

        /// <summary>
        ///     Crea una l�nea en base a sus puntos extremos
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <returns>L�nea creada</returns>
        public static TgcLine fromExtremes(TGCVector3 start, TGCVector3 end)
        {
            var line = new TgcLine();
            line.PStart = start;
            line.PEnd = end;
            line.updateValues();
            return line;
        }

        /// <summary>
        ///     Crea una l�nea en base a sus puntos extremos, con el color especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <param name="color">Color de la l�nea</param>
        /// <returns>L�nea creada</returns>
        public static TgcLine fromExtremes(TGCVector3 start, TGCVector3 end, Color color)
        {
            var line = new TgcLine();
            line.PStart = start;
            line.PEnd = end;
            line.Color = color;
            line.updateValues();
            return line;
        }

        #endregion Creacion
    }
}