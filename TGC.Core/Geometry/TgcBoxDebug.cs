using SharpDX;
using SharpDX.Direct3D9;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para dibujar una caja 3D que muestra solo sus aristas, con grosor configurable.
    /// </summary>
    public class TgcBoxDebug : IRenderObject
    {
        /// <summary>
        ///     Cantidad de vertices total de la caja
        /// </summary>
        private const int LINE_VERTICES_COUNT = 36;

        private const int LINES_COUNT = 12;
        private const int VERTICES_COUNT = LINES_COUNT * LINE_VERTICES_COUNT;
        private const int TRIANGLES_COUNT = LINES_COUNT * 12;

        private readonly VertexBuffer vertexBuffer;

        private Color color;

        protected Effect effect;

        protected string technique;

        public TgcBoxDebug()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), VERTICES_COUNT,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Thickness = 1f;
            Enabled = true;
            color = Color.White;
            AlphaBlendEnable = false;

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Punto mínimo de la caja
        /// </summary>
        public Vector3 PMin { get; set; }

        /// <summary>
        ///     Punto máximo de la caja
        /// </summary>
        public Vector3 PMax { get; set; }

        /// <summary>
        ///     Color de la caja
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        ///     Indica si la caja esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Grosor de la caja. Debe ser mayor a cero.
        /// </summary>
        public float Thickness { get; set; }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return PMin; }
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
        ///     Renderizar la caja
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
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, TRIANGLES_COUNT);
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
        ///     Actualizar parámetros de la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[VERTICES_COUNT];
            int idx;
            var c = color.ToArgb();

            //Botton Face
            idx = 0;
            createLineZ(vertices, idx, c,
                PMin, new Vector3(PMin.X, PMin.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(PMin.X, PMin.Y, PMax.Z), new Vector3(PMax.X, PMin.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new Vector3(PMax.X, PMin.Y, PMax.Z), new Vector3(PMax.X, PMin.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(PMax.X, PMin.Y, PMin.Z), PMin);

            //Top Face
            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new Vector3(PMin.X, PMax.Y, PMin.Z), new Vector3(PMin.X, PMax.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(PMin.X, PMax.Y, PMax.Z), PMax);

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                PMax, new Vector3(PMax.X, PMax.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new Vector3(PMax.X, PMax.Y, PMin.Z), new Vector3(PMin.X, PMax.Y, PMin.Z));

            //Conexión Bottom-Top
            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                PMin, new Vector3(PMin.X, PMax.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(PMin.X, PMin.Y, PMax.Z), new Vector3(PMin.X, PMax.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(PMax.X, PMin.Y, PMax.Z), PMax);

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new Vector3(PMax.X, PMin.Y, PMin.Z), new Vector3(PMax.X, PMax.Y, PMin.Z));

            //Cargar VertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Crear linea en X
        /// </summary>
        private void createLineX(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            var min2 = new Vector3(min.X, min.Y - Thickness, min.Z - Thickness);
            var max2 = new Vector3(max.X, max.Y + Thickness, max.Z + Thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear linea en Y
        /// </summary>
        private void createLineY(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            var min2 = new Vector3(min.X - Thickness, min.Y, min.Z - Thickness);
            var max2 = new Vector3(max.X + Thickness, max.Y, max.Z + Thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear linea en Z
        /// </summary>
        private void createLineZ(CustomVertex.PositionColored[] vertices, int idx, int color, Vector3 min, Vector3 max)
        {
            var min2 = new Vector3(min.X - Thickness, min.Y - Thickness, min.Z);
            var max2 = new Vector3(max.X + Thickness, max.Y + Thickness, max.Z);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear los vértices de la línea con valores extremos especificados
        /// </summary>
        private void createLineVertices(CustomVertex.PositionColored[] vertices, int idx, Vector3 min, Vector3 max,
            int c)
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
        ///     Configurar valores de posicion y tamaño en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        public void setPositionSize(Vector3 position, Vector3 size)
        {
            var radius = Vector3.Scale(size, 0.5f);
            PMin = Vector3.Subtract(position, radius);
            PMax = Vector3.Add(position, radius);
        }

        #region Creacion

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(Vector3 center, Vector3 size)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado, con color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(Vector3 center, Vector3 size, Color color)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado, con el grosor y color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(Vector3 center, Vector3 size, Color color, float thickness)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.color = color;
            box.Thickness = thickness;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(Vector3 pMin, Vector3 pMax)
        {
            var box = new TgcBoxDebug();
            box.PMin = pMin;
            box.PMax = pMax;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(Vector3 pMin, Vector3 pMax, Color color)
        {
            var box = new TgcBoxDebug();
            box.PMin = pMin;
            box.PMax = pMax;
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo, con el grosor y color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(Vector3 pMin, Vector3 pMax, Color color, float thickness)
        {
            var box = new TgcBoxDebug();
            box.PMin = pMin;
            box.PMax = pMax;
            box.color = color;
            box.Thickness = thickness;
            box.updateValues();
            return box;
        }

        #endregion Creacion
    }
}