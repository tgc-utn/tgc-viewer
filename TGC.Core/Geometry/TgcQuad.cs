using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para crear un Quad 3D, o un plano con ancho y largo acotado,
    ///     en base al centro y una normal.
    /// </summary>
    public class TGCQuad : IRenderObject
    {
        private readonly TGCVector3 ORIGINAL_DIR = TGCVector3.Up;

        private readonly VertexBuffer vertexBuffer;

        public TGCQuad()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 6, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Center = TGCVector3.Empty;
            Normal = TGCVector3.Up;
            Size = new TGCVector2(10, 10);
            Enabled = true;
            Color = Color.Blue;
            AlphaBlendEnable = false;

            //Shader
            Effect = TgcShaders.Instance.VariosShader;
            Technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Centro del plano
        /// </summary>
        public TGCVector3 Center { get; set; }

        /// <summary>
        ///     Normal del plano
        /// </summary>
        public TGCVector3 Normal { get; set; }

        /// <summary>
        ///     Tamaño del plano, en ancho y longitud
        /// </summary>
        public TGCVector2 Size { get; set; }

        /// <summary>
        ///     Color del plano
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Indica si el plano habilitado para ser renderizado
        /// </summary>
        public bool Enabled { get; set; }

        public TGCVector3 Position
        {
            get { return Center; }
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
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Actualizar parámetros del plano en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[6];

            //Crear un Quad con dos triángulos sobre XZ con normal default (0, 1, 0)
            var min = new TGCVector3(-Size.X / 2, 0, -Size.Y / 2);
            var max = new TGCVector3(Size.X / 2, 0, Size.Y / 2);
            var c = Color.ToArgb();

            vertices[0] = new CustomVertex.PositionColored(min, c);
            vertices[1] = new CustomVertex.PositionColored(min.X, 0, max.Z, c);
            vertices[2] = new CustomVertex.PositionColored(max, c);

            vertices[3] = new CustomVertex.PositionColored(min, c);
            vertices[4] = new CustomVertex.PositionColored(max, c);
            vertices[5] = new CustomVertex.PositionColored(max.X, 0, min.Z, c);

            //Obtener matriz de rotacion respecto de la normal del plano
            Normal.Normalize();
            var angle = FastMath.Acos(TGCVector3.Dot(ORIGINAL_DIR, Normal));
            var axisRotation = TGCVector3.Cross(ORIGINAL_DIR, Normal);
            axisRotation.Normalize();
            var t = TGCMatrix.RotationAxis(axisRotation, angle) * TGCMatrix.Translation(Center);

            //Transformar todos los puntos
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = TGCVector3.TransformCoordinate(TGCVector3.FromVector3(vertices[i].Position), t);
            }

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar el Quad
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
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            Effect.EndPass();
            Effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la flecha
        /// </summary>
        public void Dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }
    }
}