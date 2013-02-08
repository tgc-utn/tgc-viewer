using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Herramienta para crear un Triangulo 3D.
    /// No está pensado para rasterizar gran cantidad de triangulos, sino mas
    /// para una herramienta de debug.
    /// </summary>
    public class TgcTriangle : IRenderObject
    {

        VertexBuffer vertexBuffer;

        Vector3 a;
        /// <summary>
        /// Primer vértice del triángulo
        /// </summary>
        public Vector3 A
        {
            get { return a; }
            set { a = value; }
        }

        Vector3 b;
        /// <summary>
        /// Segundo vértice del triángulo
        /// </summary>
        public Vector3 B
        {
            get { return b; }
            set { b = value; }
        }

        Vector3 c;
        /// <summary>
        /// Tercer vértice del triángulo
        /// </summary>
        public Vector3 C
        {
            get { return c; }
            set { c = value; }
        }

        Color color;
        /// <summary>
        /// Color del plano
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }


        private bool enabled;
        /// <summary>
        /// Indica si el plano habilitado para ser renderizado
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Vector3 Position
        {
            //Habria que devolver el centro pero es costoso calcularlo cada vez
            get { return a; }
        }

        private bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        protected Effect effect;
        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;
        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }




        public TgcTriangle()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);


            this.a = Vector3.Empty;
            this.b = Vector3.Empty;
            this.c = Vector3.Empty;
            this.enabled = true;
            this.color = Color.Blue;
            this.alphaBlendEnable = false;

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        /// Actualizar parámetros del triángulo en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[3];
            
            //Crear triángulo
            int ci = color.ToArgb();
            vertices[0] = new CustomVertex.PositionColored(a, ci);
            vertices[1] = new CustomVertex.PositionColored(b, ci);
            vertices[2] = new CustomVertex.PositionColored(c, ci);

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        

        /// <summary>
        /// Renderizar el Triángulo
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrixIdentity(this.effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// Calcular normal del Triángulo
        /// </summary>
        /// <returns>Normal (esta normalizada)</returns>
        public Vector3 computeNormal()
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            n.Normalize();
            return n;
        }

        /// <summary>
        /// Calcular centro del Triángulo
        /// </summary>
        /// <returns>Centro</returns>
        public Vector3 computeCenter()
        {
            return Vector3.Scale(a + b + c, 1/3f);
        }

        /// <summary>
        /// Crea una flecha a modo debug para mostrar la normal de la cara del triangulo
        /// </summary>
        /// <returns>TgcArrow que representa la face-normal</returns>
        public TgcArrow createNormalArrow()
        {
            return TgcArrow.fromDirection(computeCenter(), Vector3.Scale(computeNormal(), 10f));
        }

        /// <summary>
        /// Liberar recursos de la flecha
        /// </summary>
        public void dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }

        /// <summary>
        /// Convierte el Triángulo en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Mesh con solo color
            Mesh d3dMesh = new Mesh(1, 3, MeshFlags.Managed, TgcSceneLoader.TgcSceneLoader.VertexColorVertexElements, d3dDevice);

            //Calcular normal: left-handed
            Vector3 normal = computeNormal();
            int ci = color.ToArgb();

            //Cargar VertexBuffer
            using (VertexBuffer vb = d3dMesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                TgcSceneLoader.TgcSceneLoader.VertexColorVertex v;

                //a
                v = new TgcSceneLoader.TgcSceneLoader.VertexColorVertex();
                v.Position = a;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);
                
                //b
                v = new TgcSceneLoader.TgcSceneLoader.VertexColorVertex();
                v.Position = b;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);

                //c
                v = new TgcSceneLoader.TgcSceneLoader.VertexColorVertex();
                v.Position = c;
                v.Normal = normal;
                v.Color = ci;
                data.Write(v);

                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (IndexBuffer ib = d3dMesh.IndexBuffer)
            {
                short[] indices = new short[3];
                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }


            //Malla de TGC
            TgcMesh tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.VERTEX_COLOR);
            tgcMesh.Materials = new Material[] { TgcD3dDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }


    }
}
