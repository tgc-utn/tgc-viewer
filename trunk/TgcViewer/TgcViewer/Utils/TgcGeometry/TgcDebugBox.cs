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
    /// Herramienta para dibujar una caja 3D que muestra solo sus aristas, con grosor configurable.
    /// </summary>
    public class TgcDebugBox : IRenderObject
    {

        
        #region Creacion

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromSize(Vector3 center, Vector3 size)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.setPositionSize(center, size);
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado, con color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromSize(Vector3 center, Vector3 size, Color color)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.setPositionSize(center, size);
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado, con el grosor y color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromSize(Vector3 center, Vector3 size, Color color, float thickness)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.setPositionSize(center, size);
            box.color = color;
            box.thickness = thickness;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromExtremes(Vector3 pMin, Vector3 pMax)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.pMin = pMin;
            box.pMax = pMax;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromExtremes(Vector3 pMin, Vector3 pMax, Color color)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.pMin = pMin;
            box.pMax = pMax;
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo, con el grosor y color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcDebugBox fromExtremes(Vector3 pMin, Vector3 pMax, Color color, float thickness)
        {
            TgcDebugBox box = new TgcDebugBox();
            box.pMin = pMin;
            box.pMax = pMax;
            box.color = color;
            box.thickness = thickness;
            box.updateValues();
            return box;
        }

        #endregion


        /// <summary>
        /// Cantidad de vertices total de la caja
        /// </summary>
        private const int LINE_VERTICES_COUNT = 36;
        private const int LINES_COUNT = 12;
        private const int VERTICES_COUNT = LINES_COUNT * LINE_VERTICES_COUNT;
        private const int TRIANGLES_COUNT = LINES_COUNT * 12;

        VertexBuffer vertexBuffer;


        Vector3 pMin;
        /// <summary>
        /// Punto mínimo de la caja
        /// </summary>
        public Vector3 PMin
        {
          get { return pMin; }
          set { pMin = value; }
        }
        
        Vector3 pMax;
        /// <summary>
        /// Punto máximo de la caja
        /// </summary>
        public Vector3 PMax
        {
          get { return pMax; }
          set { pMax = value; }
        }

        Color color;
        /// <summary>
        /// Color de la caja
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la caja esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private float thickness;
        /// <summary>
        /// Grosor de la caja. Debe ser mayor a cero.
        /// </summary>
        public float Thickness
        {
            get { return thickness; }
            set { thickness = value; }
        }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return pMin; }
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



        public TgcDebugBox()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), VERTICES_COUNT, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            
            this.thickness = 1f;
            this.enabled = true;
            this.color = Color.White;
            this.alphaBlendEnable = false;

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        /// Actualizar parámetros de la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[VERTICES_COUNT];
            int idx;
            int c = color.ToArgb();

            //Botton Face
            idx = 0;
            createLineZ(vertices, idx, c, 
                pMin, new Vector3(pMin.X, pMin.Y, pMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(pMin.X, pMin.Y, pMax.Z), new Vector3(pMax.X, pMin.Y, pMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new Vector3(pMax.X, pMin.Y, pMax.Z), new Vector3(pMax.X, pMin.Y, pMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(pMax.X, pMin.Y, pMin.Z), pMin);


            //Top Face
            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new Vector3(pMin.X, pMax.Y, pMin.Z), new Vector3(pMin.X, pMax.Y, pMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(pMin.X, pMax.Y, pMax.Z), pMax);

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                pMax, new Vector3(pMax.X, pMax.Y, pMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(pMax.X, pMax.Y, pMin.Z), new Vector3(pMin.X, pMax.Y, pMin.Z));


            //Conexión Bottom-Top
            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                pMin, new Vector3(pMin.X, pMax.Y, pMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(pMin.X, pMin.Y, pMax.Z), new Vector3(pMin.X, pMax.Y, pMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(pMax.X, pMin.Y, pMax.Z), pMax);

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(pMax.X, pMin.Y, pMin.Z), new Vector3(pMax.X, pMax.Y, pMin.Z));


            //Cargar VertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        /// Crear linea en X
        /// </summary>
        private void createLineX(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            Vector3 min2 = new Vector3(min.X, min.Y - thickness, min.Z - thickness);
            Vector3 max2 = new Vector3(max.X, max.Y + thickness, max.Z + thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        /// Crear linea en Y
        /// </summary>
        private void createLineY(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            Vector3 min2 = new Vector3(min.X - thickness, min.Y, min.Z - thickness);
            Vector3 max2 = new Vector3(max.X + thickness, max.Y, max.Z + thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        /// Crear linea en Z
        /// </summary>
        private void createLineZ(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            Vector3 min2 = new Vector3(min.X - thickness, min.Y - thickness, min.Z);
            Vector3 max2 = new Vector3(max.X + thickness, max.Y + thickness, max.Z);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        /// Crear los vértices de la línea con valores extremos especificados
        /// </summary>
        private void createLineVertices(CustomVertex.PositionColored[] vertices, int idx, Vector3 min, Vector3 max, int c)
        {
            // Front face
            vertices[idx] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[idx + 1] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[idx + 2] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[idx + 3] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[idx + 4] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);
            vertices[idx + 5] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[idx + 6] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[idx + 7] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[idx + 8] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[idx + 9] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[idx + 10] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[idx + 11] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);

            // Top face
            vertices[idx + 12] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[idx + 13] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[idx + 14] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[idx + 15] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[idx + 16] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[idx + 17] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[idx + 18] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[idx + 19] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[idx + 20] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[idx + 21] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[idx + 22] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[idx + 23] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);

            // Left face
            vertices[idx + 24] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);
            vertices[idx + 25] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[idx + 26] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, c);
            vertices[idx + 27] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, c);
            vertices[idx + 28] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, c);
            vertices[idx + 29] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, c);

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[idx + 30] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[idx + 31] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, c);
            vertices[idx + 32] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
            vertices[idx + 33] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, c);
            vertices[idx + 34] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, c);
            vertices[idx + 35] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, c);
        }

        /// <summary>
        /// Configurar valores de posicion y tamaño en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        public void setPositionSize(Vector3 position, Vector3 size)
        {
            Vector3 radius = Vector3.Scale(size, 0.5f);
            this.pMin = Vector3.Subtract(position, radius);
            this.pMax = Vector3.Add(position, radius);
        }


        /// <summary>
        /// Renderizar la caja
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
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, TRIANGLES_COUNT);
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
