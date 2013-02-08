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
    /// Herramienta para crear un Quad 3D, o un plano con ancho y largo acotado,
    /// en base al centro y una normal.
    /// </summary>
    public class TgcQuad : IRenderObject
    {

        #region Creacion


        #endregion


        readonly Vector3 ORIGINAL_DIR = new Vector3(0, 1, 0);

        VertexBuffer vertexBuffer;

        Vector3 center;
        /// <summary>
        /// Centro del plano
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        Vector3 normal;
        /// <summary>
        /// Normal del plano
        /// </summary>
        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        Vector2 size;
        /// <summary>
        /// Tamaño del plano, en ancho y longitud
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
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
            get { return center; }
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



        public TgcQuad()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 6, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            this.center = Vector3.Empty;
            this.normal = new Vector3(0, 1, 0);
            this.size = new Vector2(10, 10);
            this.enabled = true;
            this.color = Color.Blue;
            this.alphaBlendEnable = false;

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        /// Actualizar parámetros del plano en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[6];
            
            //Crear un Quad con dos triángulos sobre XZ con normal default (0, 1, 0)
            Vector3 min = new Vector3(-size.X / 2, 0, -size.Y / 2);
            Vector3 max = new Vector3(size.X / 2, 0, size.Y / 2);
            int c = color.ToArgb();

            vertices[0] = new CustomVertex.PositionColored(min, c);
            vertices[1] = new CustomVertex.PositionColored(min.X, 0, max.Z, c);
            vertices[2] = new CustomVertex.PositionColored(max, c);

            vertices[3] = new CustomVertex.PositionColored(min, c);
            vertices[4] = new CustomVertex.PositionColored(max, c);
            vertices[5] = new CustomVertex.PositionColored(max.X, 0, min.Z, c);

            //Obtener matriz de rotacion respecto de la normal del plano
            normal.Normalize();
            float angle = FastMath.Acos(Vector3.Dot(ORIGINAL_DIR, normal));
            Vector3 axisRotation = Vector3.Cross(ORIGINAL_DIR, normal);
            axisRotation.Normalize();
            Matrix t = Matrix.RotationAxis(axisRotation, angle) * Matrix.Translation(center);

            //Transformar todos los puntos
            for (int i = 0; i < vertices.Length; i++)
			{
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, t);
			}

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        

        /// <summary>
        /// Renderizar el Quad
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
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            effect.EndPass();
            effect.End();
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


    }
}
