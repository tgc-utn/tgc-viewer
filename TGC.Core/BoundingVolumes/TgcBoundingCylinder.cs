using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TGC.Core.BoundingVolumes
{
    /// <summary>
    ///     Cilindro Bounding con posibilidad de orietar y rotar, este puede ser utilizado por ejemplo para personajes.
    /// </summary>
    public class TgcBoundingCylinder : IRenderObject
    {
        private TGCVector3 center;
        private TGCVector3 rotation;

        public TgcBoundingCylinder(TGCVector3 center, float radius, float halfLength)
        {
            this.center = center;
            Radius = radius;
            HalfLength = halfLength;
            rotation = TGCVector3.Empty;
            updateValues();

            color = Color.Yellow;
        }

        /// <summary>
        ///     Devuelve el vector HalfHeight (va del centro a la tapa superior del cilindro)
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public TGCVector3 HalfHeight
        {
            get { return TGCVector3.TransformNormal(TGCVector3.Up, Transform); }
        }

        /// <summary>
        ///     Devuelve el vector Direccion (apunta desde el centro hacia la tapa superior)
        ///     Esta normalizado
        ///     Se utiliza para testeo de colisiones
        /// </summary>
        public TGCVector3 Direction
        {
            get { return TGCVector3.TransformNormal(new TGCVector3(0, 1 / HalfLength, 0), Transform); }
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
            get { return 2 * HalfLength; }
            set { HalfLength = value / 2; }
        }

        /// <summary>
        ///     Radio del cilindro
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Centro del cilindro
        /// </summary>
        public TGCVector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        ///     Actualiza la matriz de transformacion
        /// </summary>
        public void updateValues()
        {
            var rotationMatrix = TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);

            //la matriz Transformation se usa para ubicar los vertices y calcular el vector HalfHeight
            Transform =
                TGCMatrix.Scaling(Radius, HalfLength, Radius) *
                rotationMatrix *
                TGCMatrix.Translation(center);

            //la matriz AntiTransformation convierte el cilindro a uno generico de radio 1 y altura 2
            AntiTransformationMatrix =
                TGCMatrix.Invert(Transform);

            //la matriz AntiRotation sirve para alinear el cilindro con los ejes
            AntiRotationMatrix =
                TGCMatrix.Translation(-center) *
                TGCMatrix.Invert(rotationMatrix) *
                TGCMatrix.Translation(center);
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

        private const int END_CAPS_RESOLUTION = 15 * 4; //4 para los bordes laterales
        private const int END_CAPS_VERTEX_COUNT = 2 * END_CAPS_RESOLUTION * 2;
        private CustomVertex.PositionColored[] vertices; //line list
        private Color color;

        public void setRenderColor(Color _color)
        {
            color = _color;
        }

        /// <summary>
        ///     Actualiza la posicion de los vertices que componen las tapas y los vertices de las lineas laterales.
        /// </summary>
        private void updateDraw()
        {
            if (vertices == null)
                vertices = new CustomVertex.PositionColored[END_CAPS_VERTEX_COUNT];

            var color = this.color.ToArgb();

            //matriz que vamos a usar para girar el vector de dibujado
            var angle = FastMath.TWO_PI / (END_CAPS_RESOLUTION / 4); // /4 ya que agregamos los bordes a la resolucion.
            var upVector = TGCVector3.Up;
            var rotationMatrix = TGCMatrix.RotationAxis(upVector, angle);

            //vector de dibujado
            var n = new TGCVector3(1, 0, 0);

            //array donde guardamos los puntos dibujados
            var draw = new TGCVector3[vertices.Length];

            for (var i = 0; i < END_CAPS_VERTEX_COUNT / 4; i += 4)
            {
                //vertice inicial de la tapa superior
                draw[i] = upVector + n;
                //vertice inicial de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 4 + i] = -upVector + n;
                //vertice inicial del borde de la tapa superior
                draw[END_CAPS_VERTEX_COUNT / 2 + i] = upVector + n;
                //vertice inicial del borde de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 1] = -upVector + n;

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);

                //vertice final de la tapa superior
                draw[i + 1] = upVector + n;
                //vertice final de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 4 + i + 1] = -upVector + n;
                //vertice final del borde de la tapa superior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 2] = upVector + n;
                //vertice final del borde de la tapa inferior
                draw[END_CAPS_VERTEX_COUNT / 2 + i + 3] = -upVector + n;
            }

            //rotamos y trasladamos los puntos, y los volcamos al vector de vertices
            var transformation = Transform;
            for (var i = 0; i < END_CAPS_VERTEX_COUNT; i++)
                vertices[i] = new CustomVertex.PositionColored(TGCVector3.TransformCoordinate(draw[i], transformation),
                    color);
        }

        public void Render()
        {
            //actualizamos los vertices de las tapas y los vertices de las lineas laterales
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

        #region Transform

        public TGCMatrix Transform { get; private set; }

        /// <summary>
        ///     Matriz que lleva cualquier punto al espacio UVW del cilindro
        /// </summary>
        public TGCMatrix AntiRotationMatrix { get; private set; }

        /// <summary>
        ///     Matriz que lleva el radio del cilindro a 1, la altura a 2, y el centro al origen de coordenadas
        /// </summary>
        public TGCMatrix AntiTransformationMatrix { get; private set; }

        public TGCVector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
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