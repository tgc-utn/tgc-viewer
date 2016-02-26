using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Scene;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcGeometry
{
    public class TgcBoundingCylinder : IRenderObject
    {
        private Vector3 center;
        private float radius;
        private float halfLength;
        private Vector3 rotation;

        private Matrix transformation;
        private Matrix antiRotationMatrix;
        private Matrix antiTransformation;

        public TgcBoundingCylinder(Vector3 _center, float _radius, float _halfLength)
        {
            this.center = _center;
            this.radius = _radius;
            this.halfLength = _halfLength;

            this.rotation = new Vector3(0, 0, 0);
            this.updateValues();

            this.color = Color.Yellow;
        }

        /// <summary>
        /// Actualiza la matriz de transformacion
        /// </summary>
        public void updateValues()
        {
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(this.rotation.Y, this.rotation.X, this.rotation.Z);

            //la matriz Transformation se usa para ubicar los vertices y calcular el vector HalfHeight
            this.transformation =
                Matrix.Scaling(this.radius, this.halfLength, this.radius) *
                rotationMatrix *
                Matrix.Translation(this.center);

            //la matriz AntiTransformation convierte el cilindro a uno generico de radio 1 y altura 2
            this.antiTransformation =
                Matrix.Invert(this.transformation);

            //la matriz AntiRotation sirve para alinear el cilindro con los ejes
            this.antiRotationMatrix =
                Matrix.Translation(-this.center) *
                Matrix.Invert(rotationMatrix) *
                Matrix.Translation(this.center);
        }

        /// <summary>
        /// Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        /// Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 HalfHeight
        {
            get { return Vector3.TransformNormal(new Vector3(0, 1, 0), this.transformation); }
        }

        /// <summary>
        /// Devuelve el vector Direccion (apunta desde el centro hacia la tapa superior)
        /// Esta normalizado
        /// Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 Direction
        {
            get { return Vector3.TransformNormal(new Vector3(0, 1 / this.halfLength, 0), this.transformation); }
        }

        /// <summary>
        /// Calcula el radio de la esfera que contiene al cilindro
        /// Se la puede utilizar para acelerar el testeo de colisiones
        /// </summary>
        public float calculateSphereRadius()
        {
            return FastMath.Sqrt(FastMath.Pow2(this.radius) + FastMath.Pow2(this.halfLength));
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

            //matriz que vamos a usar para girar el vector de dibujado
            float angle = FastMath.TWO_PI / (float)END_CAPS_RESOLUTION;
            Vector3 upVector = new Vector3(0, 1, 0);
            Matrix rotationMatrix = Matrix.RotationAxis(upVector, angle);

            //vector de dibujado
            Vector3 n = new Vector3(1, 0, 0);

            //array donde guardamos los puntos dibujados
            Vector3[] draw = new Vector3[this.vertices.Length];

            for (int i = 0; i < END_CAPS_VERTEX_COUNT / 2; i += 2)
            {
                //vertice inicial de la tapa superior
                draw[i] = upVector + n;
                //vertice inicial de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i] = -upVector + n;

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);

                //vertice final de la tapa superior
                draw[i + 1] = upVector + n;
                //vertice final de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 1] = -upVector + n;
            }

            //rotamos y trasladamos los puntos, y los volcamos al vector de vertices
            Matrix transformation = this.transformation;
            for (int i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                this.vertices[i] = new CustomVertex.PositionColored(Vector3.TransformCoordinate(draw[i], transformation), color);
        }

        /// <summary>
        /// Actualiza la posicion de los cuatro vertices que componen los lados
        /// </summary>
        private void updateBordersDraw()
        {
            //obtenemos las matrices de transformacion y antitransformacion
            Matrix transformation = this.transformation;
            Matrix antiTransformation = transformation;
            antiTransformation.Invert();

            //obtenemos datos utiles del cilindro
            Vector3 cylHalfHeight = this.HalfHeight;
            Vector3 cylCenter = Vector3.TransformCoordinate(new Vector3(0, 0, 0), transformation);

            //obtenemos el vector direccion de vision, y su perpendicular
            Vector3 cameraSeen = GuiController.Instance.CurrentCamera.getPosition() - cylCenter;
            Vector3 transversalALaCamara = Vector3.Cross(cameraSeen, cylHalfHeight);

            //destransformamos la perpendicular para hallar el radio en esa direccion
            Vector3 destransformado = Vector3.TransformNormal(transversalALaCamara, antiTransformation);
            destransformado.Normalize();
            float length = Vector3.TransformNormal(destransformado, transformation).Length();
            transversalALaCamara.Normalize();
            transversalALaCamara *= length;

            //datos para el dibujado
            int color = this.color.ToArgb();
            int firstBorderVertex = END_CAPS_VERTEX_COUNT;

            Vector3 cylTop = cylCenter + cylHalfHeight;
            Vector3 cylBottom = cylCenter - cylHalfHeight;

            //linea lateral derecha
            Vector3 point = cylTop + transversalALaCamara;
            this.vertices[firstBorderVertex] = new CustomVertex.PositionColored(point, color);
            point = cylBottom + transversalALaCamara;
            this.vertices[firstBorderVertex + 1] = new CustomVertex.PositionColored(point, color);

            //linea lateral izquierda
            point = cylTop - transversalALaCamara;
            this.vertices[firstBorderVertex + 2] = new CustomVertex.PositionColored(point, color);
            point = cylBottom - transversalALaCamara;
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

        #region Transform

        public Matrix Transform
        {
            get { return this.transformation; }
        }

        /// <summary>
        /// Matriz que lleva cualquier punto al espacio UVW del cilindro
        /// </summary>
        public Matrix AntiRotationMatrix
        {
            get { return this.antiRotationMatrix; }
        }

        /// <summary>
        /// Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public Matrix AntiTransformationMatrix
        {
            get { return this.antiTransformation; }
        }

        public Vector3 Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
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

        public void rotateX(float angle)
        {
            this.rotation.X += angle;
        }

        public void rotateY(float angle)
        {
            this.rotation.Y += angle;
        }

        public void rotateZ(float angle)
        {
            this.rotation.Z += angle;
        }

        #endregion

        /// <summary>
        /// Media altura del cilindro
        /// </summary>
        public float HalfLength
        {
            get { return this.halfLength; }
            set { this.halfLength = value; }
        }

        /// <summary>
        /// Altura del cilindro
        /// </summary>
        public float Length
        {
            get { return 2 * this.halfLength; }
            set { this.halfLength = value / 2; }
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
        /// Centro del cilindro
        /// </summary>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }
    }
}
