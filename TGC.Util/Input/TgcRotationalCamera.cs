using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Geometries;
using TGC.Core.Input;
using TGC.Core.Utils;

namespace TGC.Util.Input
{
    /// <summary>
    ///     Camara que permite rotar y hacer zoom alrededor de un objeto central
    /// </summary>
    public class TgcRotationalCamera : TgcCamera
    {
        public static float DEFAULT_ZOOM_FACTOR = 0.15f;
        public static float DEFAULT_CAMERA_DISTANCE = 10f;
        public static float DEFAULT_ROTATION_SPEED = 100f;
        private float diffX;
        private float diffY;
        private float diffZ;
        private Vector3 nextPos;

        private Vector3 upVector;
        private Matrix viewMatrix;

        public TgcRotationalCamera()
        {
            resetValues();
        }

        /// <summary>
        ///     Actualiza los valores de la camara
        /// </summary>
        public void updateCamera(float elapsedTime)
        {
            if (!enable)
            {
                return;
            }

            var d3dInput = GuiController.Instance.D3dInput;

            //Obtener variacion XY del mouse
            var mouseX = 0f;
            var mouseY = 0f;
            if (d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                mouseX = d3dInput.XposRelative;
                mouseY = d3dInput.YposRelative;

                diffX += mouseX * elapsedTime * RotationSpeed;
                diffY += mouseY * elapsedTime * RotationSpeed;
            }
            else
            {
                diffX += mouseX;
                diffY += mouseY;
            }

            //Calcular rotacion a aplicar
            var rotX = -diffY / FastMath.PI;
            var rotY = diffX / FastMath.PI;

            //Truncar valores de rotacion fuera de rango
            if (rotX > FastMath.PI * 2 || rotX < -FastMath.PI * 2)
            {
                diffY = 0;
                rotX = 0;
            }

            //Invertir Y de UpVector segun el angulo de rotacion
            if (rotX < -FastMath.PI / 2 && rotX > -FastMath.PI * 3 / 2)
            {
                upVector.Y = -1;
            }
            else if (rotX > FastMath.PI / 2 && rotX < FastMath.PI * 3 / 2)
            {
                upVector.Y = -1;
            }
            else
            {
                upVector.Y = 1;
            }

            //Determinar distancia de la camara o zoom segun el Mouse Wheel
            if (d3dInput.WheelPos != 0)
            {
                diffZ += ZoomFactor * d3dInput.WheelPos * -1;
            }
            var distance = -CameraDistance * diffZ;

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
            nextPos.X = m.M41;
            nextPos.Y = m.M42;
            nextPos.Z = m.M43;

            //Hacer efecto de Pan View
            if (d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                var dx = -d3dInput.XposRelative;
                var dy = d3dInput.YposRelative;
                var panSpeedZoom = PanSpeed * FastMath.Abs(distance);

                var d = CameraCenter - nextPos;
                d.Normalize();

                var n = Vector3.Cross(d, upVector);
                n.Normalize();

                var up = Vector3.Cross(n, d);
                var desf = Vector3.Scale(up, dy * panSpeedZoom) - Vector3.Scale(n, dx * panSpeedZoom);
                nextPos = nextPos + desf;
                CameraCenter = CameraCenter + desf;
            }

            //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            viewMatrix = Matrix.LookAtLH(nextPos, CameraCenter, upVector);
        }

        /// <summary>
        ///     Actualiza la ViewMatrix, si es que la camara esta activada
        /// </summary>
        public void updateViewMatrix(Device d3dDevice)
        {
            if (!enable)
            {
                return;
            }

            d3dDevice.Transform.View = viewMatrix;
        }

        public Vector3 getPosition()
        {
            return nextPos;
        }

        public Vector3 getLookAt()
        {
            return CameraCenter;
        }

        /// <summary>
        ///     Carga los valores default de la camara
        /// </summary>
        public void resetValues()
        {
            upVector = new Vector3(0.0f, 1.0f, 0.0f);
            CameraCenter = new Vector3(0, 0, 0);
            nextPos = new Vector3(0, 0, 0);
            CameraDistance = DEFAULT_CAMERA_DISTANCE;
            ZoomFactor = DEFAULT_ZOOM_FACTOR;
            RotationSpeed = DEFAULT_ROTATION_SPEED;
            diffX = 0f;
            diffY = 0f;
            diffZ = 1f;
            viewMatrix = Matrix.Identity;
            PanSpeed = 0.01f;
        }

        /// <summary>
        ///     Configura los parámetros de la cámara en funcion del BoundingBox de un modelo
        /// </summary>
        /// <param name="boundingBox">BoundingBox en base al cual configurar</param>
        public void targetObject(TgcBoundingBox boundingBox)
        {
            CameraCenter = boundingBox.calculateBoxCenter();
            var r = boundingBox.calculateBoxRadius();
            CameraDistance = 2 * r;
        }

        #region Getters y Setters

        private bool enable;

        /// <summary>
        ///     Habilita o no el uso de la camara
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;

                //Si se habilito la camara, cargar como la cámara actual
                if (value)
                {
                    CamaraManager.Instance.CurrentCamera = this;
                }
            }
        }

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

        /// <summary>
        ///     Configura el centro de la camara, la distancia y la velocidad de zoom
        /// </summary>
        public void setCamera(Vector3 cameraCenter, float cameraDistance, float zoomFactor)
        {
            CameraCenter = cameraCenter;
            CameraDistance = cameraDistance;
            ZoomFactor = zoomFactor;
        }

        /// <summary>
        ///     Configura el centro de la camara, la distancia
        /// </summary>
        public void setCamera(Vector3 cameraCenter, float cameraDistance)
        {
            CameraCenter = cameraCenter;
            CameraDistance = cameraDistance;
            ZoomFactor = DEFAULT_ZOOM_FACTOR;
        }

        #endregion Getters y Setters
    }
}