using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Viewer.Utils.Shaders;

namespace TGC.Viewer.Utils.TgcGeometry
{
    /// <summary>
    ///     Representa un polígono convexo plano en 3D de una sola cara, compuesto
    ///     por varios vértices que lo delimitan.
    /// </summary>
    public class TgcConvexPolygon : IRenderObject
    {
        public TgcConvexPolygon()
        {
            Enabled = true;
            AlphaBlendEnable = false;
            color = Color.Purple;
        }

        /// <summary>
        ///     Vertices que definen el contorno polígono.
        ///     Están dados en clockwise-order.
        /// </summary>
        public Vector3[] BoundingVertices { get; set; }

        /// <summary>
        ///     Indica si la flecha esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        # region Renderizado del poligono

        protected Effect effect;

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        private VertexBuffer vertexBuffer;

        /// <summary>
        ///     Actualizar valores de renderizado.
        ///     Hay que llamarlo al menos una vez para poder hacer render()
        /// </summary>
        public void updateValues()
        {
            //Crear VertexBuffer on demand
            if (vertexBuffer == null || vertexBuffer.Disposed)
            {
                vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), BoundingVertices.Length,
                    D3DDevice.Instance.Device,
                    Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
                //Shader
                effect = GuiController.Instance.Shaders.VariosShader;
                technique = TgcShaders.T_POSITION_COLORED;
            }

            //Crear como TriangleFan
            var c = color.ToArgb();
            var vertices = new CustomVertex.PositionColored[BoundingVertices.Length];
            for (var i = 0; i < BoundingVertices.Length; i++)
            {
                vertices[i] = new CustomVertex.PositionColored(BoundingVertices[i], c);
            }

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar el polígono
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            var texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Renderizar RenderFarm
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleFan, 0, BoundingVertices.Length - 2);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Liberar recursos del polígono
        /// </summary>
        public void dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return BoundingVertices[0]; }
        }

        private Color color;

        /// <summary>
        ///     Color del polígono
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        # endregion
    }
}