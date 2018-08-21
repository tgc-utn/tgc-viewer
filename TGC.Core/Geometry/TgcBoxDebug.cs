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

        public TgcBoxDebug()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), VERTICES_COUNT,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Thickness = 1f;
            Enabled = true;
            Color = Color.White;
            AlphaBlendEnable = false;

            //Shader
            Effect = TgcShaders.Instance.VariosShader;
            Technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Punto m�nimo de la caja
        /// </summary>
        public TGCVector3 PMin { get; set; }

        /// <summary>
        ///     Punto m�ximo de la caja
        /// </summary>
        public TGCVector3 PMax { get; set; }

        /// <summary>
        ///     Color de la caja
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Indica si la caja esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Grosor de la caja. Debe ser mayor a cero.
        /// </summary>
        public float Thickness { get; set; }

        public TGCVector3 Position
        {
            //Lo correcto ser�a calcular el centro, pero con un extremo es suficiente.
            get { return PMin; }
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
        ///     con textura o colores por v�rtice de canal Alpha.
        ///     Por default est� deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Actualizar par�metros de la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[VERTICES_COUNT];
            int idx;
            var c = Color.ToArgb();

            //Botton Face
            idx = 0;
            createLineZ(vertices, idx, c,
                PMin, new TGCVector3(PMin.X, PMin.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new TGCVector3(PMin.X, PMin.Y, PMax.Z), new TGCVector3(PMax.X, PMin.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new TGCVector3(PMax.X, PMin.Y, PMax.Z), new TGCVector3(PMax.X, PMin.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new TGCVector3(PMax.X, PMin.Y, PMin.Z), PMin);

            //Top Face
            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                new TGCVector3(PMin.X, PMax.Y, PMin.Z), new TGCVector3(PMin.X, PMax.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new TGCVector3(PMin.X, PMax.Y, PMax.Z), PMax);

            idx += LINE_VERTICES_COUNT;
            createLineZ(vertices, idx, c,
                PMax, new TGCVector3(PMax.X, PMax.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineX(vertices, idx, c,
                new TGCVector3(PMax.X, PMax.Y, PMin.Z), new TGCVector3(PMin.X, PMax.Y, PMin.Z));

            //Conexi�n Bottom-Top
            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                PMin, new TGCVector3(PMin.X, PMax.Y, PMin.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new TGCVector3(PMin.X, PMin.Y, PMax.Z), new TGCVector3(PMin.X, PMax.Y, PMax.Z));

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new TGCVector3(PMax.X, PMin.Y, PMax.Z), PMax);

            idx += LINE_VERTICES_COUNT;
            createLineY(vertices, idx, c,
                new TGCVector3(PMax.X, PMin.Y, PMin.Z), new TGCVector3(PMax.X, PMax.Y, PMin.Z));

            //Cargar VertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar la caja
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
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, TRIANGLES_COUNT);
            Effect.EndPass();
            Effect.End();
        }

        /// <summary>
        ///     Liberar recursos de la l�nea
        /// </summary>
        public void Dispose()
        {
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
        }

        /// <summary>
        ///     Crear linea en X
        /// </summary>
        private void createLineX(CustomVertex.PositionColored[] vertices, int idx, int color, TGCVector3 min, TGCVector3 max)
        {
            var min2 = new TGCVector3(min.X, min.Y - Thickness, min.Z - Thickness);
            var max2 = new TGCVector3(max.X, max.Y + Thickness, max.Z + Thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear linea en Y
        /// </summary>
        private void createLineY(CustomVertex.PositionColored[] vertices, int idx, int color, TGCVector3 min, TGCVector3 max)
        {
            var min2 = new TGCVector3(min.X - Thickness, min.Y, min.Z - Thickness);
            var max2 = new TGCVector3(max.X + Thickness, max.Y, max.Z + Thickness);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear linea en Z
        /// </summary>
        private void createLineZ(CustomVertex.PositionColored[] vertices, int idx, int color, TGCVector3 min, TGCVector3 max)
        {
            var min2 = new TGCVector3(min.X - Thickness, min.Y - Thickness, min.Z);
            var max2 = new TGCVector3(max.X + Thickness, max.Y + Thickness, max.Z);
            createLineVertices(vertices, idx, min2, max2, color);
        }

        /// <summary>
        ///     Crear los v�rtices de la l�nea con valores extremos especificados
        /// </summary>
        private void createLineVertices(CustomVertex.PositionColored[] vertices, int idx, TGCVector3 min, TGCVector3 max, int c)
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
        ///     Configurar valores de posicion y tama�o en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la caja</param>
        /// <param name="size">Tama�o de la caja</param>
        public void setPositionSize(TGCVector3 position, TGCVector3 size)
        {
            var radius = TGCVector3.Scale(size, 0.5f);
            PMin = TGCVector3.Subtract(position, radius);
            PMax = TGCVector3.Add(position, radius);
        }

        #region Creacion

        /// <summary>
        ///     Crea una caja con el centro y tama�o especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tama�o de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(TGCVector3 center, TGCVector3 size)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tama�o especificado, con color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tama�o de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(TGCVector3 center, TGCVector3 size, Color color)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.Color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tama�o especificado, con el grosor y color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tama�o de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromSize(TGCVector3 center, TGCVector3 size, Color color, float thickness)
        {
            var box = new TgcBoxDebug();
            box.setPositionSize(center, size);
            box.Color = color;
            box.Thickness = thickness;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo
        /// </summary>
        /// <param name="pMin">Punto m�nimo</param>
        /// <param name="pMax">Punto m�ximo</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(TGCVector3 pMin, TGCVector3 pMax)
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
        /// <param name="pMin">Punto m�nimo</param>
        /// <param name="pMax">Punto m�ximo</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(TGCVector3 pMin, TGCVector3 pMax, Color color)
        {
            var box = new TgcBoxDebug();
            box.PMin = pMin;
            box.PMax = pMax;
            box.Color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo, con el grosor y color especificado
        /// </summary>
        /// <param name="pMin">Punto m�nimo</param>
        /// <param name="pMax">Punto m�ximo</param>
        /// <param name="color">Color de la caja</param>
        /// <param name="thickness">Grosor de las aristas de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBoxDebug fromExtremes(TGCVector3 pMin, TGCVector3 pMax, Color color, float thickness)
        {
            var box = new TgcBoxDebug();
            box.PMin = pMin;
            box.PMax = pMax;
            box.Color = color;
            box.Thickness = thickness;
            box.updateValues();
            return box;
        }

        #endregion Creacion
    }
}