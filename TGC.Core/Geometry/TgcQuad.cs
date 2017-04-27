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
    public class TgcQuad : IRenderObject
    {
        private readonly TGCVector3 ORIGINAL_DIR = TGCVector3.Up;

        private readonly VertexBuffer vertexBuffer;

        private Color color;

        protected Effect effect;

        private TGCVector3 normal;

        protected string technique;

        public TgcQuad()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 6, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Center = TGCVector3.Empty;
            normal = TGCVector3.Up;
            Size = new TGCVector2(10, 10);
            Enabled = true;
            color = Color.Blue;
            AlphaBlendEnable = false;

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Centro del plano
        /// </summary>
        public TGCVector3 Center { get; set; }

        /// <summary>
        ///     Normal del plano
        /// </summary>
        public TGCVector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        /// <summary>
        ///     Tamaño del plano, en ancho y longitud
        /// </summary>
        public TGCVector2 Size { get; set; }

        /// <summary>
        ///     Color del plano
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

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
        ///     Renderizar el Quad
        /// </summary>
        public void Render()
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
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            effect.EndPass();
            effect.End();
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

        /// <summary>
        ///     Actualizar parámetros del plano en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[6];

            //Crear un Quad con dos triángulos sobre XZ con normal default (0, 1, 0)
            var min = new TGCVector3(-Size.X / 2, 0, -Size.Y / 2);
            var max = new TGCVector3(Size.X / 2, 0, Size.Y / 2);
            var c = color.ToArgb();

            vertices[0] = new CustomVertex.PositionColored(min, c);
            vertices[1] = new CustomVertex.PositionColored(min.X, 0, max.Z, c);
            vertices[2] = new CustomVertex.PositionColored(max, c);

            vertices[3] = new CustomVertex.PositionColored(min, c);
            vertices[4] = new CustomVertex.PositionColored(max, c);
            vertices[5] = new CustomVertex.PositionColored(max.X, 0, min.Z, c);

            //Obtener matriz de rotacion respecto de la normal del plano
            normal.Normalize();
            var angle = FastMath.Acos(TGCVector3.Dot(ORIGINAL_DIR, normal));
            var axisRotation = TGCVector3.Cross(ORIGINAL_DIR, normal);
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
    }
}