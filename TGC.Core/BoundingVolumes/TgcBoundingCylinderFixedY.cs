using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Core.BoundingVolumes
{
    /// <summary>
    ///     Cilindro Bounding alineado a al eje Y, este puede ser utilizado por ejemplo para personajes.
    /// </summary>
    public class TgcBoundingCylinderFixedY : IRenderObject
    {
        private TGCVector3 center;

        public TgcBoundingCylinderFixedY(TGCVector3 center, float radius, float halfLength)
        {
            this.center = center;
            Radius = radius;
            HalfHeight = new TGCVector3(0, halfLength, 0);
            color = Color.Yellow;
        }

        /// <summary>
        ///     Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public TGCVector3 HalfHeight { get; private set; }

        /// <summary>
        ///     Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public TGCMatrix AntiTransformationMatrix { get; private set; }

        /// <summary>
        ///     Centro del cilindro
        /// </summary>
        public TGCVector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        ///     Radio del cilindro
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Media altura del cilindro
        /// </summary>
        public float HalfLength
        {
            get { return HalfHeight.Y; }
            set { HalfHeight = new TGCVector3(0, value, 0); }
        }

        /// <summary>
        ///     Altura del cilindro
        /// </summary>
        public float Length
        {
            get { return 2 * HalfHeight.Y; }
            set { HalfHeight = new TGCVector3(0, value / 2, 0); }
        }

        public void move(TGCVector3 v)
        {
            move(v.X, v.Y, v.Z);
        }

        public void move(float x, float y, float z)
        {
            center.X += x;
            center.Y += y;
            center.Z += z;
        }

        /// <summary>
        ///     Actualiza la matriz de transformacion
        /// </summary>
        public void updateValues()
        {
            AntiTransformationMatrix =
                TGCMatrix.Translation(-center) *
                TGCMatrix.Scaling(1 / Radius, 1 / HalfLength, 1 / Radius);
        }

        #region Rendering

        private const int END_CAPS_RESOLUTION = 15 * 4; //4 para los bordes laterales
        private const int END_CAPS_VERTEX_COUNT = 2 * END_CAPS_RESOLUTION * 2;
        private CustomVertex.PositionColored[] vertices; //line list
        private Color color;

        public void setRenderColor(Color _color)
        {
            color = _color;
        }

        /// <summary>
        ///     Actualiza la posicion de los vertices que componen las tapas
        ///     y los vertices de las lineas laterales. FIXME este update puede ser una transformacion solamente.
        /// </summary>
        private void updateDraw()
        {
            if (vertices == null)
                vertices = new CustomVertex.PositionColored[END_CAPS_VERTEX_COUNT];

            var color = this.color.ToArgb();
            var zeroVector = center;

            //matriz que vamos a usar para girar el vector de dibujado
            var angle = FastMath.TWO_PI / (END_CAPS_RESOLUTION / 4); // /4 ya que agregamos los bordes a la resolucion.
            var upVector = HalfHeight;
            var rotationMatrix = TGCMatrix.RotationAxis(TGCVector3.Up, angle);

            //vector de dibujado
            var n = new TGCVector3(Radius, 0, 0);

            //array donde guardamos los puntos dibujados
            var draw = new TGCVector3[END_CAPS_VERTEX_COUNT];

            for (var i = 0; i < END_CAPS_VERTEX_COUNT / 4; i += 4)
            {
                //vertice inicial de la tapa superior
                draw[i] = zeroVector + upVector + n;
                //vertice inicial de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 4 + i] = zeroVector - upVector + n;
                //vertice inicial del borde de la tapa superior
                draw[END_CAPS_VERTEX_COUNT / 2 + i] = zeroVector + upVector + n;
                //vertice inicial del borde de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 1] = zeroVector - upVector + n;

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);

                //vertice final de la tapa superior
                draw[i + 1] = zeroVector + upVector + n;
                //vertice final de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 4 + i + 1] = zeroVector - upVector + n;
                //vertice final del borde de la tapa superior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 2] = zeroVector + upVector + n;
                //vertice final del borde de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 3] = zeroVector - upVector + n;
            }

            for (var i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                vertices[i] = new CustomVertex.PositionColored(draw[i], color);
        }

        public void Render()
        {
            //actualizamos los vertices de las tapas
            updateDraw();

            //dibujamos
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        public void Dispose()
        {
            vertices = null;
        }

        public bool AlphaBlendEnable { get; set; } //useless?

        #endregion Rendering
    }
}