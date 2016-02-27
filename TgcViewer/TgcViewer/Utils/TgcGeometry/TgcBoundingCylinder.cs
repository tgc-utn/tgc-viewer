using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcGeometry
{
    public class TgcBoundingCylinder : IRenderObject
    {
        private Vector3 center;
        private Vector3 rotation;

        public TgcBoundingCylinder(Vector3 _center, float _radius, float _halfLength)
        {
            center = _center;
            Radius = _radius;
            HalfLength = _halfLength;

            rotation = new Vector3(0, 0, 0);
            updateValues();

            color = Color.Yellow;
        }

        /// <summary>
        ///     Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 HalfHeight
        {
            get { return Vector3.TransformNormal(new Vector3(0, 1, 0), Transform); }
        }

        /// <summary>
        ///     Devuelve el vector Direccion (apunta desde el centro hacia la tapa superior)
        ///     Esta normalizado
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public Vector3 Direction
        {
            get { return Vector3.TransformNormal(new Vector3(0, 1/HalfLength, 0), Transform); }
        }

        /// <summary>
        ///     Media altura del cilindro
        /// </summary>
        public float HalfLength { get; set; }

        /// <summary>
        ///     Altura del cilindro
        /// </summary>
        public float Length
        {
            get { return 2*HalfLength; }
            set { HalfLength = value/2; }
        }

        /// <summary>
        ///     Radio del cilindro
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Centro del cilindro
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        ///     Actualiza la matriz de transformacion
        /// </summary>
        public void updateValues()
        {
            var rotationMatrix = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);

            //la matriz Transformation se usa para ubicar los vertices y calcular el vector HalfHeight
            Transform =
                Matrix.Scaling(Radius, HalfLength, Radius)*
                rotationMatrix*
                Matrix.Translation(center);

            //la matriz AntiTransformation convierte el cilindro a uno generico de radio 1 y altura 2
            AntiTransformationMatrix =
                Matrix.Invert(Transform);

            //la matriz AntiRotation sirve para alinear el cilindro con los ejes
            AntiRotationMatrix =
                Matrix.Translation(-center)*
                Matrix.Invert(rotationMatrix)*
                Matrix.Translation(center);
        }

        /// <summary>
        ///     Calcula el radio de la esfera que contiene al cilindro
        ///     Se la puede utilizar para acelerar el testeo de colisiones
        /// </summary>
        public float calculateSphereRadius()
        {
            return FastMath.Sqrt(FastMath.Pow2(Radius) + FastMath.Pow2(HalfLength));
        }

        #region Rendering

        private const int END_CAPS_RESOLUTION = 15;
        private const int END_CAPS_VERTEX_COUNT = 2*END_CAPS_RESOLUTION*2;
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

            //matriz que vamos a usar para girar el vector de dibujado
            var angle = FastMath.TWO_PI/END_CAPS_RESOLUTION;
            var upVector = new Vector3(0, 1, 0);
            var rotationMatrix = Matrix.RotationAxis(upVector, angle);

            //vector de dibujado
            var n = new Vector3(1, 0, 0);

            //array donde guardamos los puntos dibujados
            var draw = new Vector3[vertices.Length];

            for (var i = 0; i < END_CAPS_VERTEX_COUNT/2; i += 2)
            {
                //vertice inicial de la tapa superior
                draw[i] = upVector + n;
                //vertice inicial de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT/2 + i] = -upVector + n;

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);

                //vertice final de la tapa superior
                draw[i + 1] = upVector + n;
                //vertice final de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT/2 + i + 1] = -upVector + n;
            }

            //rotamos y trasladamos los puntos, y los volcamos al vector de vertices
            var transformation = Transform;
            for (var i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                vertices[i] = new CustomVertex.PositionColored(Vector3.TransformCoordinate(draw[i], transformation),
                    color);
        }

        /// <summary>
        ///     Actualiza la posicion de los cuatro vertices que componen los lados
        /// </summary>
        private void updateBordersDraw()
        {
            //obtenemos las matrices de transformacion y antitransformacion
            var transformation = Transform;
            var antiTransformation = transformation;
            antiTransformation.Invert();

            //obtenemos datos utiles del cilindro
            var cylHalfHeight = HalfHeight;
            var cylCenter = Vector3.TransformCoordinate(new Vector3(0, 0, 0), transformation);

            //obtenemos el vector direccion de vision, y su perpendicular
            var cameraSeen = GuiController.Instance.CurrentCamera.getPosition() - cylCenter;
            var transversalALaCamara = Vector3.Cross(cameraSeen, cylHalfHeight);

            //destransformamos la perpendicular para hallar el radio en esa direccion
            var destransformado = Vector3.TransformNormal(transversalALaCamara, antiTransformation);
            destransformado.Normalize();
            var length = Vector3.TransformNormal(destransformado, transformation).Length();
            transversalALaCamara.Normalize();
            transversalALaCamara *= length;

            //datos para el dibujado
            var color = this.color.ToArgb();
            var firstBorderVertex = END_CAPS_VERTEX_COUNT;

            var cylTop = cylCenter + cylHalfHeight;
            var cylBottom = cylCenter - cylHalfHeight;

            //linea lateral derecha
            var point = cylTop + transversalALaCamara;
            vertices[firstBorderVertex] = new CustomVertex.PositionColored(point, color);
            point = cylBottom + transversalALaCamara;
            vertices[firstBorderVertex + 1] = new CustomVertex.PositionColored(point, color);

            //linea lateral izquierda
            point = cylTop - transversalALaCamara;
            vertices[firstBorderVertex + 2] = new CustomVertex.PositionColored(point, color);
            point = cylBottom - transversalALaCamara;
            vertices[firstBorderVertex + 3] = new CustomVertex.PositionColored(point, color);
        }

        public void render()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //actualizamos los vertices de las tapas
            updateDraw();
            //actualizamos los vertices de las lineas laterales
            updateBordersDraw();

            //dibujamos
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length/2, vertices);
        }

        public void dispose()
        {
            vertices = null;
        }

        public bool AlphaBlendEnable { get; set; } //useless?

        #endregion Rendering

        #region Transform

        public Matrix Transform { get; private set; }

        /// <summary>
        ///     Matriz que lleva cualquier punto al espacio UVW del cilindro
        /// </summary>
        public Matrix AntiRotationMatrix { get; private set; }

        /// <summary>
        ///     Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public Matrix AntiTransformationMatrix { get; private set; }

        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
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

        public void rotateX(float angle)
        {
            rotation.X += angle;
        }

        public void rotateY(float angle)
        {
            rotation.Y += angle;
        }

        public void rotateZ(float angle)
        {
            rotation.Z += angle;
        }

        #endregion Transform
    }
}