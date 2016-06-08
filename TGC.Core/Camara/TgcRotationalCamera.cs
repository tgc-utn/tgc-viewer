using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Utils;

namespace TGC.Core.Camara
{
    /// <summary>
    ///     Camara que permite rotar y hacer zoom alrededor de un objeto central
    /// </summary>
    public class TgcRotationalCamera : TgcCamera
    {
        public static float DEFAULT_ZOOM_FACTOR = 0.15f;
        public static float DEFAULT_CAMERA_DISTANCE = 10f;
        public static float DEFAULT_ROTATION_SPEED = 100f;
        public static Vector3 DEFAULT_DOWN = new Vector3(0f, -1f, 0f);

        public TgcRotationalCamera()
        {
            CameraCenter = new Vector3(0, 0, 0);
            NextPos = new Vector3(0, 0, 0);
            CameraDistance = DEFAULT_CAMERA_DISTANCE;
            ZoomFactor = DEFAULT_ZOOM_FACTOR;
            RotationSpeed = DEFAULT_ROTATION_SPEED;
            DiffX = 0f;
            DiffY = 0f;
            DiffZ = 1f;
            PanSpeed = 0.01f;
            UpVector = new Vector3(0f, 1f, 0f);
            this.setCamera(NextPos, LookAt, UpVector);
        }

        public TgcRotationalCamera(Vector3 position, Vector3 target) : this()
        {
            NextPos = position;
            CameraCenter = target;
            this.setCamera(NextPos, LookAt, UpVector);
        }

        public TgcRotationalCamera(Vector3 cameraCenter, float cameraDistance, float zoomFactor, float rotationSpeed) : this()
        {
            CameraCenter = cameraCenter;
            CameraDistance = cameraDistance;
            ZoomFactor = zoomFactor;
            RotationSpeed = rotationSpeed;
        }
        /// <summary>
        ///     Configura el centro de la camara, la distancia y la velocidad de zoom
        /// </summary>
        public TgcRotationalCamera(Vector3 cameraCenter, float cameraDistance, float zoomFactor):
            this(cameraCenter, cameraDistance, zoomFactor, DEFAULT_ROTATION_SPEED)
        {
            
        }

        /// <summary>
        ///     Configura el centro de la camara, la distancia
        /// </summary>
        public TgcRotationalCamera(Vector3 cameraCenter, float cameraDistance):
            this(cameraCenter, cameraDistance, DEFAULT_ZOOM_FACTOR)
        {

        }

        /// <summary>
        ///     Configura los parámetros de la cámara en funcion del BoundingBox de un modelo
        /// </summary>
        /// <param name="boundingBox">BoundingBox en base al cual configurar</param>
        public TgcRotationalCamera(TgcBoundingBox boundingBox):this()
        {
            calculateCenterDistance(boundingBox);
        }

        public void calculateCenterDistance(TgcBoundingBox boundingBox)
        {
            CameraCenter = boundingBox.calculateBoxCenter();
            var r = boundingBox.calculateBoxRadius();
            CameraDistance = 2 * r;
        }

        /// <summary>
        ///     Actualiza los valores de la camara
        /// </summary>
        public override void updateCamera(float elapsedTime)
        {
            var d3dInput = TgcD3dInput.Instance;

            //Obtener variacion XY del mouse
            var mouseX = 0f;
            var mouseY = 0f;
            if (d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                mouseX = d3dInput.XposRelative;
                mouseY = d3dInput.YposRelative;

                DiffX += mouseX * elapsedTime * RotationSpeed;
                DiffY += mouseY * elapsedTime * RotationSpeed;
            }
            else
            {
                DiffX += mouseX;
                DiffY += mouseY;
            }

            //Calcular rotacion a aplicar
            var rotX = -DiffY / FastMath.PI;
            var rotY = DiffX / FastMath.PI;

            //Truncar valores de rotacion fuera de rango
            if (rotX > FastMath.PI * 2 || rotX < -FastMath.PI * 2)
            {
                DiffY = 0;
                rotX = 0;
            }

            //Invertir Y de UpVector segun el angulo de rotacion
            if (rotX < -FastMath.PI / 2 && rotX > -FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else if (rotX > FastMath.PI / 2 && rotX < FastMath.PI * 3 / 2)
            {
                UpVector = DEFAULT_DOWN;
            }
            else
            {
                UpVector = DEFAULT_UP_VECTOR;
            }

            //Determinar distancia de la camara o zoom segun el Mouse Wheel
            if (d3dInput.WheelPos != 0)
            {
                DiffZ += ZoomFactor * d3dInput.WheelPos * -1;
            }
            var distance = -CameraDistance * DiffZ;

            //Limitar el zoom a 0
            if (distance > 0)
            {
                distance = 0;
            }

            //Realizar Transformacion: primero alejarse en Z, despues rotar en X e Y y despues ir al centro de la cmara
            var m = Matrix.Translation(0, 0, -distance)
                    * Matrix.RotationX(rotX)
                    * Matrix.RotationY(rotY)
                    * Matrix.Translation(CameraCenter);

            //Extraer la posicion final de la matriz de transformacion
            NextPos = new Vector3(m.M41,m.M42,m.M43);

            //Hacer efecto de Pan View
            if (d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                var dx = -d3dInput.XposRelative;
                var dy = d3dInput.YposRelative;
                var panSpeedZoom = PanSpeed * FastMath.Abs(distance);

                var d = CameraCenter - NextPos;
                d.Normalize();

                var n = Vector3.Cross(d, UpVector);
                n.Normalize();

                var up = Vector3.Cross(n, d);
                var desf = Vector3.Scale(up, dy * panSpeedZoom) - Vector3.Scale(n, dx * panSpeedZoom);
                NextPos = NextPos + desf;
                CameraCenter = CameraCenter + desf;
            }

            //asigna las posiciones de la camara.
            this.setCamera(NextPos, CameraCenter, UpVector);
        }

       #region Getters y Setters

        /// <summary>
        ///     Centro de la camara sobre la cual se rota
        /// </summary>
        public Vector3 CameraCenter { get; set; }

        /// <summary>
        ///     Distance entre la camara y el centro
        /// </summary>
        public float CameraDistance { get; set; }

        /// <summary>
        ///     Velocidad con la que se hace Zoom
        /// </summary>
        public float ZoomFactor { get; set; }

        /// <summary>
        ///     Velocidad de rotacion de la camara
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        ///     Velocidad de paneo
        /// </summary>
        public float PanSpeed { get; set; }

        public Vector3 NextPos { get; set; }

        public float DiffX { get; set; }
        public float DiffY { get; set; }
        public float DiffZ { get; set; }

       #endregion Getters y Setters
    }
}