using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Input;

namespace TGC.Viewer.Utils.Input
{
    /// <summary>
    ///     Camara en Tercera Persona que permite seguir a un objeto que se desplaza
    ///     desde atrás, con una cierta distancia. Posee un efecto de Spring que realiza
    ///     la rotación suavemente hasta llegar al ángulo deseado.
    /// </summary>
    public class TgcSpringThirdPersonCamera : TgcCamera
    {
        private static readonly float DEFAULT_SPRING_CONSTANT = 16.0f;
        private static readonly float DEFAULT_DAMPING_CONSTANT = 8.0f;

        private static readonly Vector3 WORLD_XAXIS = new Vector3(1.0f, 0.0f, 0.0f);
        private static readonly Vector3 WORLD_YAXIS = new Vector3(0.0f, 1.0f, 0.0f);
        private static readonly Vector3 WORLD_ZAXIS = new Vector3(0.0f, 0.0f, 1.0f);

        private float m_headingDegrees;
        private float m_offsetDistance;
        private Quaternion m_orientation;
        private float m_pitchDegrees;
        private Vector3 m_targetYAxis;
        private Vector3 m_velocity;
        private Matrix m_viewMatrix;

        public TgcSpringThirdPersonCamera()
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

            updateOrientation(elapsedTime);

            if (EnableSpringSystem)
            {
                updateViewMatrix(elapsedTime);
            }
            else
            {
                updateViewMatrix();
            }
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

            d3dDevice.Transform.View = m_viewMatrix;
        }

        public Vector3 getPosition()
        {
            return Position;
        }

        public Vector3 getLookAt()
        {
            return Target;
        }

        /// <summary>
        ///     Carga los valores default de la camara y limpia todos los cálculos intermedios
        /// </summary>
        public void resetValues()
        {
            EnableSpringSystem = true;
            Spring = DEFAULT_SPRING_CONSTANT;
            Damping = DEFAULT_DAMPING_CONSTANT;

            m_offsetDistance = 0.0f;
            m_headingDegrees = 0.0f;
            m_pitchDegrees = 0.0f;

            Position = new Vector3(0.0f, 0.0f, 0.0f);
            Target = new Vector3(0.0f, 0.0f, 0.0f);
            m_targetYAxis = new Vector3(0, 1, 0);

            m_velocity = new Vector3(0.0f, 0.0f, 0.0f);

            m_viewMatrix = Matrix.Identity;
            m_orientation = Quaternion.Identity;
        }

        /// <summary>
        ///     Configura los valores iniciales de la cámara
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="target"></param>
        public void setCamera(Vector3 target, float offsetY, float offsetZ)
        {
            Position = new Vector3(target.X, target.Y + offsetY, target.Z + offsetZ);
            ;
            Target = target;

            m_viewMatrix = Matrix.LookAtLH(Position, Target, m_targetYAxis);
            m_orientation = Quaternion.RotationMatrix(m_viewMatrix);

            var offset = Target - Position;
            m_offsetDistance = offset.Length();

            m_headingDegrees = 0.0f;
            m_pitchDegrees = 0.0f;
        }

        /// <summary>
        ///     Rota la cámara.
        ///     Debe ser rotada la misma cantidad que se rota al Target a seguir.
        ///     Se especifica en Degrees y no en Radianes
        /// </summary>
        /// <param name="headingDegrees">Angulo de rotacion en Grados</param>
        public void rotateY(float headingDegrees)
        {
            m_headingDegrees = -headingDegrees;
            m_pitchDegrees = 0;
        }

        private void updateOrientation(float elapsedTimeSec)
        {
            m_headingDegrees *= elapsedTimeSec;
            m_pitchDegrees *= elapsedTimeSec;

            var heading = Geometry.DegreeToRadian(m_headingDegrees);
            var pitch = Geometry.DegreeToRadian(m_pitchDegrees);

            Quaternion rot;

            if (heading != 0.0f)
            {
                rot = Quaternion.RotationAxis(m_targetYAxis, heading);
                m_orientation = Quaternion.Multiply(rot, m_orientation);
            }

            if (pitch != 0.0f)
            {
                rot = Quaternion.RotationAxis(WORLD_XAXIS, pitch);
                m_orientation = Quaternion.Multiply(m_orientation, rot);
            }
        }

        private void updateViewMatrix()
        {
            m_orientation.Normalize();
            m_viewMatrix = Matrix.RotationQuaternion(m_orientation);

            var m_xAxis = new Vector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            var m_yAxis = new Vector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            var m_zAxis = new Vector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

            Position = Target + m_zAxis * -m_offsetDistance;

            m_viewMatrix.M41 = -Vector3.Dot(m_xAxis, Position);
            m_viewMatrix.M42 = -Vector3.Dot(m_yAxis, Position);
            m_viewMatrix.M43 = -Vector3.Dot(m_zAxis, Position);
        }

        private void updateViewMatrix(float elapsedTimeSec)
        {
            m_orientation.Normalize();
            m_viewMatrix = Matrix.RotationQuaternion(m_orientation);

            var m_xAxis = new Vector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            var m_yAxis = new Vector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            var m_zAxis = new Vector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

            // Calculate the new camera position. The 'idealPosition' is where the
            // camera should be position. The camera should be positioned directly
            // behind the target at the required offset distance. What we're doing here
            // is rather than have the camera immediately snap to the 'idealPosition'
            // we slowly move the camera towards the 'idealPosition' using a spring
            // system.
            //
            // References:
            //   Stone, Jonathan, "Third-Person Camera Navigation," Game Programming
            //     Gems 4, Andrew Kirmse, Editor, Charles River Media, Inc., 2004.

            var idealPosition = Target + m_zAxis * -m_offsetDistance;
            var displacement = Position - idealPosition;
            var springAcceleration = -Spring * displacement - Damping * m_velocity;

            m_velocity += springAcceleration * elapsedTimeSec;
            Position += m_velocity * elapsedTimeSec;

            // The view matrix is always relative to the camera's current position
            // 'm_eye'. Since a spring system is being used here 'm_eye' will be
            // relative to 'desiredPosition'. When the camera is no longer being moved
            // 'm_eye' will become the same as 'desiredPosition'. The local x, y, and
            // z axes that were extracted from the camera's orientation 'm_orienation'
            // is correct for the 'desiredPosition' only. We need to recompute these
            // axes so that they're relative to 'm_eye'. Once that's done we can use
            // those axes to reconstruct the view matrix.

            m_zAxis = Target - Position;
            m_zAxis.Normalize();

            m_xAxis = Vector3.Cross(m_targetYAxis, m_zAxis);
            m_xAxis.Normalize();

            m_yAxis = Vector3.Cross(m_zAxis, m_xAxis);
            m_yAxis.Normalize();

            m_viewMatrix = Matrix.Identity;

            m_viewMatrix.M11 = m_xAxis.X;
            m_viewMatrix.M21 = m_xAxis.Y;
            m_viewMatrix.M31 = m_xAxis.Z;
            m_viewMatrix.M41 = -Vector3.Dot(m_xAxis, Position);

            m_viewMatrix.M12 = m_yAxis.X;
            m_viewMatrix.M22 = m_yAxis.Y;
            m_viewMatrix.M32 = m_yAxis.Z;
            m_viewMatrix.M42 = -Vector3.Dot(m_yAxis, Position);

            m_viewMatrix.M13 = m_zAxis.X;
            m_viewMatrix.M23 = m_zAxis.Y;
            m_viewMatrix.M33 = m_zAxis.Z;
            m_viewMatrix.M43 = -Vector3.Dot(m_zAxis, Position);
        }

        public Vector3 calulateNextPosition(float headingDegrees, float elapsedTimeSec)
        {
            var heading = Geometry.DegreeToRadian(-headingDegrees * elapsedTimeSec);

            var quatOrientation = m_orientation;
            if (heading != 0.0f)
            {
                var rot = Quaternion.RotationAxis(m_targetYAxis, heading);
                quatOrientation = Quaternion.Multiply(rot, quatOrientation);
            }

            quatOrientation.Normalize();
            var viewMatrix = Matrix.RotationQuaternion(quatOrientation);

            var m_zAxis = new Vector3(viewMatrix.M13, viewMatrix.M23, viewMatrix.M33);
            var idealPosition = Target + m_zAxis * -m_offsetDistance;

            return idealPosition;
        }

        /// <summary>
        ///     Permite configurar la orientacion de la cámara respecto del Target
        /// </summary>
        /// <param name="cameraRotation">Rotación absoluta a aplicar</param>
        public void setOrientation(Vector3 cameraRotation)
        {
            m_orientation = Quaternion.RotationYawPitchRoll(cameraRotation.Y, cameraRotation.X, cameraRotation.Z);
            m_headingDegrees = 0;
            m_pitchDegrees = 0;
        }

        public void setOffset(float offset)
        {
            m_offsetDistance = offset;
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
        ///     Habilitar desplazamiento de la camara con delay
        /// </summary>
        public bool EnableSpringSystem { get; set; }

        /// <summary>
        ///     Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public Vector3 Target { get; set; }

        /// <summary>
        ///     Valor de Spring para el delay de la camara
        /// </summary>
        public float Spring { get; set; }

        /// <summary>
        ///     Valor de Damping para el delay de la camara
        /// </summary>
        public float Damping { get; set; }

        /// <summary>
        ///     Posicion del ojo de la camara que apunta hacia el Target
        /// </summary>
        public Vector3 Position { get; private set; }

        #endregion Getters y Setters
    }
}