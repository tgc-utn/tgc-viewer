using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Scene;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcGeometry
{
    public class TgcFixedYBoundingCylinder : IRenderObject
    {
        private float radius;
        private Vector3 halfHeight;
        private Vector3 center;

        private Matrix antiTransformation;


        public TgcFixedYBoundingCylinder(Vector3 _center, float _radius, float _halfLength)
        {
            this.center = _center;
            this.radius = _radius;
            this.halfHeight = new Vector3(0, _halfLength, 0);

            this.color = Color.Yellow;
        }

        #region Rendering

        private const int END_CAPS_RESOLUTION = 15;
        private const int END_CAPS_VERTEX_COUNT = 2 * (END_CAPS_RESOLUTION * 2);
        private CustomVertex.PositionColored[] vertices; //line list
        private Color color;

        public void setRenderColor(Color _color)
        {
            this.color = _color;
        }

        /// <summary>
        /// Actualiza la posicion de los vertices que componen las tapas
        /// </summary>
        private void updateDraw()
        {
            if (this.vertices == null)
                this.vertices = new CustomVertex.PositionColored[END_CAPS_VERTEX_COUNT + 4]; //4 para los bordes laterales

            int color = this.color.ToArgb();
            Vector3 zeroVector = this.center;

            //matriz que vamos a usar para girar el vector de dibujado
            float angle = FastMath.TWO_PI / (float)END_CAPS_RESOLUTION;
            Vector3 upVector = this.halfHeight;
            Matrix rotationMatrix = Matrix.RotationAxis(new Vector3(0, 1, 0), angle);

            //vector de dibujado
            Vector3 n = new Vector3(this.radius, 0, 0);

            //array donde guardamos los puntos dibujados
            Vector3[] draw = new Vector3[END_CAPS_VERTEX_COUNT];

            for (int i = 0; i < END_CAPS_VERTEX_COUNT / 2; i += 2)
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

            for (int i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                this.vertices[i] = new CustomVertex.PositionColored(draw[i], color);
        }

        /// <summary>
        /// Actualiza la posicion de los cuatro vertices que componen los lados
        /// </summary>
        private void updateBordersDraw()
        {
            //obtenemos el vector direccion de vision, y su perpendicular
            Vector3 cameraSeen = GuiController.Instance.CurrentCamera.getPosition() - this.center;
            Vector3 transversalALaCamara = Vector3.Cross(cameraSeen, this.halfHeight);
            transversalALaCamara.Normalize();
            transversalALaCamara *= this.radius;

            //datos para el dibujado
            int color = this.color.ToArgb();
            int firstBorderVertex = END_CAPS_VERTEX_COUNT;

            //linea lateral derecha
            Vector3 point = this.center + this.halfHeight + transversalALaCamara;
            this.vertices[firstBorderVertex] = new CustomVertex.PositionColored(point, color);
            point = this.center - this.halfHeight + transversalALaCamara;
            this.vertices[firstBorderVertex + 1] = new CustomVertex.PositionColored(point, color);

            //linea lateral izquierda
            point = this.center + this.halfHeight - transversalALaCamara;
            this.vertices[firstBorderVertex + 2] = new CustomVertex.PositionColored(point, color);
            point = this.center - this.halfHeight - transversalALaCamara;
            this.vertices[firstBorderVertex + 3] = new CustomVertex.PositionColored(point, color);
        }

        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //actualizamos los vertices de las tapas
            this.updateDraw();
            //actualizamos los vertices de las lineas laterales
            this.updateBordersDraw();
            
            //dibujamos
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, this.vertices.Length / 2, this.vertices);
        }

        public void dispose()
        {
            this.vertices = null;
        }

        public bool AlphaBlendEnable { get; set; } //useless?

        #endregion

        /// <summary>
        /// Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        /// Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 HalfHeight {
            get { return this.halfHeight; }
        }

        /// <summary>
        /// Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public Matrix AntiTransformationMatrix
        {
            get { return this.antiTransformation; }
        }

        /// <summary>
        /// Centro del cilindro
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        public void move(Vector3 v)
        {
            this.move(v.X, v.Y, v.Z);
        }

        public void move(float x, float y, float z)
        {
            this.center.X += x;
            this.center.Y += y;
            this.center.Z += z;
        }

        /// <summary>
        /// Radio del cilindro
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        /// <summary>
        /// Media altura del cilindro
        /// </summary>
        public float HalfLength
        {
            get { return this.halfHeight.Y; }
            set { this.halfHeight = new Vector3(0, value, 0); }
        }

        /// <summary>
        /// Altura del cilindro
        /// </summary>
        public float Length
        {
            get { return 2 * this.halfHeight.Y; }
            set { this.halfHeight = new Vector3(0, value / 2, 0); }
        }

        /// <summary>
        /// Actualiza la matriz de transformacion
        /// </summary>
        public void updateValues()
        {
            this.antiTransformation =
                Matrix.Translation(-this.center) *
                Matrix.Scaling(1 / this.radius, 1 / this.HalfLength, 1 / this.radius);
        }
    }
}
