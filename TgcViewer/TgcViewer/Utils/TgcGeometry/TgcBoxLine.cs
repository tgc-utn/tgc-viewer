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
    /// Herramienta para dibujar una línea 3D con color y grosor específico.
    /// </summary>
    public class TgcBoxLine : IRenderObject
    {

        #region Creacion

        /// <summary>
        /// Crea una línea en base a sus puntos extremos
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <returns>Línea creada</returns>
        public static TgcBoxLine fromExtremes(Vector3 start, Vector3 end)
        {
            TgcBoxLine line = new TgcBoxLine();
            line.pStart = start;
            line.pEnd = end;
            line.updateValues();
            return line;
        }

        /// <summary>
        /// Crea una línea en base a sus puntos extremos, con el color y el grosor especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <param name="color">Color de la línea</param>
        /// <param name="thickness">Grosor de la línea</param>
        /// <returns>Línea creada</returns>
        public static TgcBoxLine fromExtremes(Vector3 start, Vector3 end, Color color, float thickness)
        {
            TgcBoxLine line = new TgcBoxLine();
            line.pStart = start;
            line.pEnd = end;
            line.color = color;
            line.thickness = thickness;
            line.updateValues();
            return line;
        }

        #endregion


        readonly Vector3 ORIGINAL_DIR = new Vector3(0, 1, 0);

        VertexBuffer vertexBuffer;

        Vector3 pStart;
        /// <summary>
        /// Punto de inicio de la linea
        /// </summary>
        public Vector3 PStart
        {
            get { return pStart; }
            set { pStart = value; }
        }

        Vector3 pEnd;
        /// <summary>
        /// Punto final de la linea
        /// </summary>
        public Vector3 PEnd
        {
            get { return pEnd; }
            set { pEnd = value; }
        }

        Color color;
        /// <summary>
        /// Color de la linea
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la linea esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private float thickness;
        /// <summary>
        /// Grosor de la línea. Debe ser mayor a cero.
        /// </summary>
        public float Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return pStart; }
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



        public TgcBoxLine()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 36, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            this.thickness = 0.06f;
            this.enabled = true;
            this.color = Color.White;
            this.alphaBlendEnable = false;

            //shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        /// Actualizar parámetros de la línea en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            int c = color.ToArgb();
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[36];

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            Vector3 lineVec = Vector3.Subtract(pEnd, pStart);
            float lineLength = lineVec.Length();
            Vector3 min = new Vector3(-thickness, 0, -thickness);
            Vector3 max = new Vector3(thickness, lineLength, thickness);

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
            float angle = FastMath.Acos(Vector3.Dot(ORIGINAL_DIR, lineVec));
            Vector3 axisRotation = Vector3.Cross(ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            Matrix t = Matrix.RotationAxis(axisRotation, angle) * Matrix.Translation(pStart);

            //Transformar todos los puntos
            for (int i = 0; i < vertices.Length; i++)
			{
                vertices[i].Position = Vector3.TransformCoordinate(vertices[i].Position, t);
			}

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }


        /// <summary>
        /// Renderizar la línea
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
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// Liberar recursos de la línea
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
