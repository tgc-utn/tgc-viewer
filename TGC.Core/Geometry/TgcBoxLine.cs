using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para dibujar una línea 3D con color y grosor específico.
    /// </summary>
    public class TgcBoxLine : IRenderObject
    {
        private readonly Vector3 ORIGINAL_DIR = new Vector3(0, 1, 0);

        private readonly VertexBuffer vertexBuffer;

        private Color color;

        protected Effect effect;

        protected string technique;

        public TgcBoxLine()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 36, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Thickness = 0.06f;
            Enabled = true;
            color = Color.White;
            AlphaBlendEnable = false;

            //shader
            effect = TgcShaders.Instance.VariosShader;
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

        /// <summary>
        ///     Grosor de la línea. Debe ser mayor a cero.
        /// </summary>
        public float Thickness { get; set; }

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
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
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

            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            TgcShaders.Instance.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColored;
            effect.Technique = technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la línea
        /// </summary>
        public void dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }

        /// <summary>
        ///     Actualizar parámetros de la línea en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var c = color.ToArgb();
            var vertices = new CustomVertex.PositionColored[36];

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            var lineVec = Vector3.Subtract(PEnd, PStart);
            var lineLength = lineVec.Length();
            var min = new Vector3(-Thickness, 0, -Thickness);
            var max = new Vector3(Thickness, lineLength, Thickness);

            //Vértices de la caja con forma de linea
            // Front face
            vertices[0] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[1] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[2] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[3] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[4] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);
            vertices[5] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[6] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[7] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[8] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[9] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[10] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[11] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);

            // Top face
            vertices[12] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[13] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[14] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[15] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[16] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[17] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[18] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[19] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[20] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[21] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[22] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[23] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);

            // Left face
            vertices[24] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[25] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[26] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[27] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[28] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[29] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[30] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[31] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);
            vertices[32] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[33] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[34] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[35] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);

            //Obtener matriz de rotacion respecto del vector de la linea
            lineVec.Normalize();
            var angle = FastMath.Acos(Vector3.Dot(ORIGINAL_DIR, lineVec));
            var axisRotation = Vector3.Cross(ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            var t = Matrix.RotationAxis(axisRotation, angle) * Matrix.Translation(PStart);

            //Transformar todos los puntos
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, t);
            }

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        #region Creacion

        /// <summary>
        ///     Crea una línea en base a sus puntos extremos
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <returns>Línea creada</returns>
        public static TgcBoxLine fromExtremes(Vector3 start, Vector3 end)
        {
            var line = new TgcBoxLine();
            line.PStart = start;
            line.PEnd = end;
            line.updateValues();
            return line;
        }

        /// <summary>
        ///     Crea una línea en base a sus puntos extremos, con el color y el grosor especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <param name="color">Color de la línea</param>
        /// <param name="thickness">Grosor de la línea</param>
        /// <returns>Línea creada</returns>
        public static TgcBoxLine fromExtremes(Vector3 start, Vector3 end, Color color, float thickness)
        {
            var line = new TgcBoxLine();
            line.PStart = start;
            line.PEnd = end;
            line.color = color;
            line.Thickness = thickness;
            line.updateValues();
            return line;
        }

        #endregion Creacion
    }
}