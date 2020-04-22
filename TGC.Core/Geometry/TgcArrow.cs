using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para dibujar una flecha 3D.
    /// </summary>
    public class TgcArrow
    {
        private readonly TGCVector3 ORIGINAL_DIR = TGCVector3.Up;

        private readonly VertexBuffer vertexBuffer;

        private Color bodyColor;

        private Color headColor;

        public TgcArrow()
        {
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 54, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            Thickness = 0.06f;
            HeadSize = new TGCVector2(0.3f, 0.6f);
            Enabled = true;
            bodyColor = Color.Blue;
            headColor = Color.LightBlue;
            AlphaBlendEnable = false;

            //Shader
            Effect = TGCShaders.Instance.VariosShader;
            Technique = TGCShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Punto de inicio de la linea
        /// </summary>
        public TGCVector3 PStart { get; set; }

        /// <summary>
        ///     Punto final de la linea
        /// </summary>
        public TGCVector3 PEnd { get; set; }

        /// <summary>
        ///     Color del cuerpo de la flecha
        /// </summary>
        public Color BodyColor
        {
            get { return bodyColor; }
            set { bodyColor = value; }
        }

        /// <summary>
        ///     Color de la cabeza de la flecha
        /// </summary>
        public Color HeadColor
        {
            get { return headColor; }
            set { headColor = value; }
        }

        /// <summary>
        ///     Indica si la flecha esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Grosor del cuerpo de la flecha. Debe ser mayor a cero.
        /// </summary>
        public float Thickness { get; set; }

        /// <summary>
        ///     Tamaño de la cabeza de la flecha. Debe ser mayor a cero.
        /// </summary>
        public TGCVector2 HeadSize { get; set; }

        public TGCVector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            get { return PStart; }
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
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Actualizar parámetros de la flecha en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var vertices = new CustomVertex.PositionColored[54];

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            var lineVec = TGCVector3.Subtract(PEnd, PStart);
            var lineLength = lineVec.Length();
            var min = new TGCVector3(-Thickness, 0, -Thickness);
            var max = new TGCVector3(Thickness, lineLength, Thickness);

            //Vertices del cuerpo de la flecha
            var bc = bodyColor.ToArgb();
            // Front face
            vertices[0] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, bc);
            vertices[1] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, bc);
            vertices[2] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, bc);
            vertices[3] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, bc);
            vertices[4] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, bc);
            vertices[5] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, bc);

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[6] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, bc);
            vertices[7] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, bc);
            vertices[8] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, bc);
            vertices[9] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, bc);
            vertices[10] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, bc);
            vertices[11] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, bc);

            // Top face
            vertices[12] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, bc);
            vertices[13] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, bc);
            vertices[14] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, bc);
            vertices[15] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, bc);
            vertices[16] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, bc);
            vertices[17] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, bc);

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[18] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, bc);
            vertices[19] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, bc);
            vertices[20] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, bc);
            vertices[21] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, bc);
            vertices[22] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, bc);
            vertices[23] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, bc);

            // Left face
            vertices[24] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, bc);
            vertices[25] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, bc);
            vertices[26] = new CustomVertex.PositionColored(min.X, min.Y, max.Z, bc);
            vertices[27] = new CustomVertex.PositionColored(min.X, max.Y, min.Z, bc);
            vertices[28] = new CustomVertex.PositionColored(min.X, min.Y, min.Z, bc);
            vertices[29] = new CustomVertex.PositionColored(min.X, max.Y, max.Z, bc);

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[30] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, bc);
            vertices[31] = new CustomVertex.PositionColored(max.X, min.Y, max.Z, bc);
            vertices[32] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, bc);
            vertices[33] = new CustomVertex.PositionColored(max.X, max.Y, min.Z, bc);
            vertices[34] = new CustomVertex.PositionColored(max.X, max.Y, max.Z, bc);
            vertices[35] = new CustomVertex.PositionColored(max.X, min.Y, min.Z, bc);

            //Vertices del cuerpo de la flecha
            var hc = headColor.ToArgb();
            var hMin = new TGCVector3(-HeadSize.X, lineLength, -HeadSize.X);
            var hMax = new TGCVector3(HeadSize.X, lineLength + HeadSize.Y, HeadSize.X);

            //Bottom face
            vertices[36] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMax.Z, hc);
            vertices[37] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMin.Z, hc);
            vertices[38] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMin.Z, hc);
            vertices[39] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMax.Z, hc);
            vertices[40] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMin.Z, hc);
            vertices[41] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMax.Z, hc);

            //Left face
            vertices[42] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMin.Z, hc);
            vertices[43] = new CustomVertex.PositionColored(0, hMax.Y, 0, hc);
            vertices[44] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMax.Z, hc);

            //Right face
            vertices[45] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMin.Z, hc);
            vertices[46] = new CustomVertex.PositionColored(0, hMax.Y, 0, hc);
            vertices[47] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMax.Z, hc);

            //Back face
            vertices[48] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMin.Z, hc);
            vertices[49] = new CustomVertex.PositionColored(0, hMax.Y, 0, hc);
            vertices[50] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMin.Z, hc);

            //Front face
            vertices[51] = new CustomVertex.PositionColored(hMin.X, hMin.Y, hMax.Z, hc);
            vertices[52] = new CustomVertex.PositionColored(0, hMax.Y, 0, hc);
            vertices[53] = new CustomVertex.PositionColored(hMax.X, hMin.Y, hMax.Z, hc);

            //Obtener matriz de rotacion respecto del vector de la linea
            lineVec.Normalize();
            var angle = FastMath.Acos(TGCVector3.Dot(ORIGINAL_DIR, lineVec));
            var axisRotation = TGCVector3.Cross(ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            var t = TGCMatrix.RotationAxis(axisRotation, angle) * TGCMatrix.Translation(PStart);

            //Transformar todos los puntos
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = TGCVector3.TransformCoordinate(new TGCVector3(vertices[i].Position), t);
            }

            //Cargar vertexBuffer
            vertexBuffer.SetData(vertices, 0, LockFlags.None);
        }

        /// <summary>
        ///     Renderizar la flecha
        /// </summary>
        public void Render()
        {
            if (!Enabled)
                return;

            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            TGCShaders.Instance.SetShaderMatrixIdentity(Effect);
            D3DDevice.Instance.Device.VertexDeclaration = TGCShaders.Instance.VdecPositionColored;
            Effect.Technique = Technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 18);
            Effect.EndPass();
            Effect.End();
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

        #region Creacion

        /// <summary>
        ///     Crea una flecha en base a sus puntos extremos
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <returns>Flecha creada</returns>
        public static TgcArrow fromExtremes(TGCVector3 start, TGCVector3 end)
        {
            var arrow = new TgcArrow();
            arrow.PStart = start;
            arrow.PEnd = end;
            arrow.updateValues();
            return arrow;
        }

        /// <summary>
        ///     Crea una flecha en base a sus puntos extremos, con el color y el grosor especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="end">Punto de fin</param>
        /// <param name="bodyColor">Color del cuerpo de la flecha</param>
        /// <param name="headColor">Color de la punta de la flecha</param>
        /// <param name="thickness">Grosor del cuerpo de la flecha</param>
        /// <param name="headSize">Tamaño de la punta de la flecha</param>
        /// <returns>Flecha creada</returns>
        public static TgcArrow fromExtremes(TGCVector3 start, TGCVector3 end, Color bodyColor, Color headColor,
            float thickness, TGCVector2 headSize)
        {
            var arrow = new TgcArrow();
            arrow.PStart = start;
            arrow.PEnd = end;
            arrow.bodyColor = bodyColor;
            arrow.headColor = headColor;
            arrow.Thickness = thickness;
            arrow.HeadSize = headSize;
            arrow.updateValues();
            return arrow;
        }

        /// <summary>
        ///     Crea una flecha en base a su punto de inicio y dirección
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="direction">Dirección de la flecha</param>
        /// <returns>Flecha creada</returns>
        public static TgcArrow fromDirection(TGCVector3 start, TGCVector3 direction)
        {
            var arrow = new TgcArrow();
            arrow.PStart = start;
            arrow.PEnd = start + direction;
            arrow.updateValues();
            return arrow;
        }

        /// <summary>
        ///     Crea una flecha en base a su punto de inicio y dirección, con el color y el grosor especificado
        /// </summary>
        /// <param name="start">Punto de inicio</param>
        /// <param name="direction">Dirección de la flecha</param>
        /// <param name="bodyColor">Color del cuerpo de la flecha</param>
        /// <param name="headColor">Color de la punta de la flecha</param>
        /// <param name="thickness">Grosor del cuerpo de la flecha</param>
        /// <param name="headSize">Tamaño de la punta de la flecha</param>
        /// <returns>Flecha creada</returns>
        public static TgcArrow fromDirection(TGCVector3 start, TGCVector3 direction, Color bodyColor, Color headColor,
            float thickness, TGCVector2 headSize)
        {
            var arrow = new TgcArrow();
            arrow.PStart = start;
            arrow.PEnd = start + direction;
            arrow.bodyColor = bodyColor;
            arrow.headColor = headColor;
            arrow.Thickness = thickness;
            arrow.HeadSize = headSize;
            arrow.updateValues();
            return arrow;
        }

        #endregion Creacion
    }
}