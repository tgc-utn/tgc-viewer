using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TGC.Core.Geometry
{
    public class TgcFixedYBoundingCylinder : IRenderObject
    {
        private Vector3 center;
        private TgcCamera camara;

        public TgcFixedYBoundingCylinder(Vector3 center, float radius, float halfLength, TgcCamera camara)
        {
            this.center = center;
            this.Radius = radius;
            this.HalfHeight = new Vector3(0, halfLength, 0);
            this.camara = camara;
            color = Color.Yellow;
        }

        /// <summary>
        ///     Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 HalfHeight { get; private set; }

        /// <summary>
        ///     Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public Matrix AntiTransformationMatrix { get; private set; }

        /// <summary>
        ///     Centro del cilindro
        /// </summary>
        public Vector3 Center
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
            set { HalfHeight = new Vector3(0, value, 0); }
        }

        /// <summary>
        ///     Altura del cilindro
        /// </summary>
        public float Length
        {
            get { return 2 * HalfHeight.Y; }
            set { HalfHeight = new Vector3(0, value / 2, 0); }
        }

        public void move(Vector3 v)
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
                Matrix.Translation(-center) *
                Matrix.Scaling(1 / Radius, 1 / HalfLength, 1 / Radius);
        }

        #region Rendering

        private const int END_CAPS_RESOLUTION = 15;
        private const int END_CAPS_VERTEX_COUNT = 2 * END_CAPS_RESOLUTION * 2;
        private CustomVertex.PositionColored[] vertices; //line list
        private Color color;

        public void setRenderColor(Color _color)
        {
            color = _color;
        }

        /// <summary>
        ///     Actualiza la posicion de los vertices que componen las tapas
        /// </summary>
        private void updateDraw()
        {
            if (vertices == null)
                vertices = new CustomVertex.PositionColored[END_CAPS_VERTEX_COUNT + 4]; //4 para los bordes laterales

            var color = this.color.ToArgb();
            var zeroVector = center;

            //matriz que vamos a usar para girar el vector de dibujado
            var angle = FastMath.TWO_PI / END_CAPS_RESOLUTION;
            var upVector = HalfHeight;
            var rotationMatrix = Matrix.RotationAxis(new Vector3(0, 1, 0), angle);

            //vector de dibujado
            var n = new Vector3(Radius, 0, 0);

            //array donde guardamos los puntos dibujados
            var draw = new Vector3[END_CAPS_VERTEX_COUNT];

            for (var i = 0; i < END_CAPS_VERTEX_COUNT / 2; i += 2)
            {
                //vertice inicial de la tapa superior
                draw[i] = zeroVector + upVector + n;
                //vertice inicial de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i] = zeroVector - upVector + n;

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);

                //vertice final de la tapa superior
                draw[i + 1] = zeroVector + upVector + n;
                //vertice final de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 1] = zeroVector - upVector + n;
            }

            for (var i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                vertices[i] = new CustomVertex.PositionColored(draw[i], color);
        }

        /// <summary>
        ///     Actualiza la posicion de los cuatro vertices que componen los lados
        /// </summary>
        private void updateBordersDraw()
        {
            //obtenemos el vector direccion de vision, y su perpendicular
            var cameraSeen = this.camara.getPosition() - center;
            var transversalALaCamara = Vector3.Cross(cameraSeen, HalfHeight);
            transversalALaCamara.Normalize();
            transversalALaCamara *= Radius;

            //datos para el dibujado
            var color = this.color.ToArgb();
            var firstBorderVertex = END_CAPS_VERTEX_COUNT;

            //linea lateral derecha
            var point = center + HalfHeight + transversalALaCamara;
            vertices[firstBorderVertex] = new CustomVertex.PositionColored(point, color);
            point = center - HalfHeight + transversalALaCamara;
            vertices[firstBorderVertex + 1] = new CustomVertex.PositionColored(point, color);

            //linea lateral izquierda
            point = center + HalfHeight - transversalALaCamara;
            vertices[firstBorderVertex + 2] = new CustomVertex.PositionColored(point, color);
            point = center - HalfHeight - transversalALaCamara;
            vertices[firstBorderVertex + 3] = new CustomVertex.PositionColored(point, color);
        }

        public void render()
        {
            //actualizamos los vertices de las tapas
            updateDraw();
            //actualizamos los vertices de las lineas laterales
            updateBordersDraw();

            //dibujamos
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        public void dispose()
        {
            vertices = null;
        }

        public bool AlphaBlendEnable { get; set; } //useless?

        #endregion Rendering
    }
}