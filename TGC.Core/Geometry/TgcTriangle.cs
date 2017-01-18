using Microsoft.DirectX;
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
    ///     Herramienta para crear un Triangulo 3D.
    ///     No está pensado para rasterizar gran cantidad de triangulos, sino mas
    ///     para una herramienta de debug.
    /// </summary>
    public class TgcTriangle : IRenderObject
    {
        private readonly VertexBuffer vertexBuffer;
        private Color color;

        protected Effect effect;

        protected string technique;

        public TgcTriangle()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            A = TGCVector3.Empty;
            B = TGCVector3.Empty;
            C = TGCVector3.Empty;
            Enabled = true;
            color = Color.Blue;
            AlphaBlendEnable = false;

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Primer vértice del triángulo
        /// </summary>
        public TGCVector3 A { get; set; }

        /// <summary>
        ///     Segundo vértice del triángulo
        /// </summary>
        public TGCVector3 B { get; set; }

        /// <summary>
        ///     Tercer vértice del triángulo
        /// </summary>
        public TGCVector3 C { get; set; }

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
            //Habria que devolver el centro pero es costoso calcularlo cada vez
            get { return A; }
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
        ///     Renderizar el Triángulo
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
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la flecha
        /// </summary>
        public void dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }

        /// <summary>
        ///     Actualizar parámetros del triángulo en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[3];

            //Crear triángulo
            var ci = color.ToArgb();
            vertices[0] = new CustomVertex.PositionColored(A.ToVector3(), ci);
            vertices[1] = new CustomVertex.PositionColored(B.ToVector3(), ci);
            vertices[2] = new CustomVertex.PositionColored(C.ToVector3(), ci);

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Calcular normal del Triángulo
        /// </summary>
        /// <returns>Normal (esta normalizada)</returns>
        public TGCVector3 computeNormal()
        {
            var n = TGCVector3.Cross(B - A, C - A);
            n.Normalize();
            return n;
        }

        /// <summary>
        ///     Calcular centro del Triángulo
        /// </summary>
        /// <returns>Centro</returns>
        public TGCVector3 computeCenter()
        {
            return TGCVector3.Scale(A + B + C, 1 / 3f);
        }

        /// <summary>
        ///     Crea una flecha a modo debug para mostrar la normal de la cara del triangulo
        /// </summary>
        /// <returns>TgcArrow que representa la face-normal</returns>
        public TgcArrow createNormalArrow()
        {
            return TgcArrow.fromDirection(computeCenter(), TGCVector3.Scale(computeNormal(), 10f));
        }

        /// <summary>
        ///     Convierte el Triángulo en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            //Crear Mesh con solo color
            var d3dMesh = new Mesh(1, 3, MeshFlags.Managed, TgcSceneLoader.VertexColorVertexElements,
                D3DDevice.Instance.Device);

            //Calcular normal: left-handed
            var normal = computeNormal();
            var ci = color.ToArgb();

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                TgcSceneLoader.VertexColorVertex v;

                //a
                v = new TgcSceneLoader.VertexColorVertex();
                v.Position = A;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);

                //b
                v = new TgcSceneLoader.VertexColorVertex();
                v.Position = B;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);

                //c
                v = new TgcSceneLoader.VertexColorVertex();
                v.Position = C;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);

                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (var ib = d3dMesh.IndexBuffer)
            {
                var indices = new short[3];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Malla de TGC
            var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.VERTEX_COLOR);
            tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }
    }
}