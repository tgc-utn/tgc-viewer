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
    ///     Representa un pol�gono convexo plano en 3D de una sola cara, compuesto
    ///     por varios v�rtices que lo delimitan.
    /// </summary>
    public class TgcConvexPolygon : IRenderObject
    {
        private VertexBuffer vertexBuffer;

        public TgcConvexPolygon()
        {
            Enabled = true;
            AlphaBlendEnable = false;
            Color = Color.Purple;
        }

        public TGCVector3 Position
        {
            //Lo correcto ser�a calcular el centro, pero con un extremo es suficiente.
            get { return BoundingVertices[0]; }
        }

        /// <summary>
        ///     Color del pol�gono
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por v�rtice de canal Alpha.
        ///     Por default est� deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Vertices que definen el contorno pol�gono.
        ///     Est�n dados en clockwise-order.
        /// </summary>
        public TGCVector3[] BoundingVertices { get; set; }

        /// <summary>
        ///     Indica si la flecha esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

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
        ///     Actualizar valores de renderizado.
        ///     Hay que llamarlo al menos una vez para poder hacer Render()
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
                Effect = TgcShaders.Instance.VariosShader;
                Technique = TgcShaders.T_POSITION_COLORED;
            }

            //Crear como TriangleFan
            var c = Color.ToArgb();
            var vertices = new CustomVertex.PositionColored[BoundingVertices.Length];
            for (var i = 0; i < BoundingVertices.Length; i++)
            {
                vertices[i] = new CustomVertex.PositionColored(BoundingVertices[i], c);
            }

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar el pol�gono
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

            //Renderizar RenderFarm
            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleFan, 0, BoundingVertices.Length - 2);
            Effect.EndPass();
            Effect.End();
        }

        /// <summary>
        ///     Liberar recursos del pol�gono
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