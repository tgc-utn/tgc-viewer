using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Mathematica;
using TGC.Core.Utils;

namespace TGC.Examples.Camara
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

        private static readonly TGCVector3 WORLD_XAXIS = new TGCVector3(1.0f, 0.0f, 0.0f);
        private static readonly TGCVector3 WORLD_YAXIS = new TGCVector3(0.0f, 1.0f, 0.0f);
        private static readonly TGCVector3 WORLD_ZAXIS = new TGCVector3(0.0f, 0.0f, 1.0f);

        private float m_headingDegrees;
        private float m_offsetDistance;
        private Quaternion m_orientation;
        private float m_pitchDegrees;
        private TGCVector3 m_targetYAxis;
        private TGCVector3 m_velocity;
        private TGCMatrix m_viewMatrix;

        public TgcSpringThirdPersonCamera()
        {
            resetValues();
        }

        /// <summary>
        ///     Actualiza los valores de la camara
        /// </summary>
        public override void UpdateCamera(float elapsedTime)
        {
            updateOrientation(elapsedTime);

            if (EnableSpringSystem)
            {
                updateViewMatrix(elapsedTime);
            }
            else
            {
                updateViewMatrix();
            }

            SetCamera(Eye, Target);
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

            Eye = new TGCVector3(0.0f, 0.0f, 0.0f);
            Target = new TGCVector3(0.0f, 0.0f, 0.0f);
            m_targetYAxis = TGCVector3.Up;

            m_velocity = new TGCVector3(0.0f, 0.0f, 0.0f);

            m_viewMatrix = TGCMatrix.Identity;
            m_orientation = Quaternion.Identity;
            SetCamera(Eye, Target);
        }

        /// <summary>
        ///     Asigna target con offsets.
        /// </summary>
        public void setTargetOffset(TGCVector3 target, float offsetY, float offsetZ)
        {
            Eye = new TGCVector3(target.X, target.Y + offsetY, target.Z + offsetZ);

            Target = target;

            m_viewMatrix = TGCMatrix.LookAtLH(Eye, Target, m_targetYAxis);
            m_orientation = Quaternion.RotationMatrix(m_viewMatrix.ToMatrix());

            var offset = Target - Eye;
            m_offsetDistance = offset.Length();

            m_headingDegrees = 0.0f;
            m_pitchDegrees = 0.0f;
        }

        public override TGCMatrix GetViewMatrix()
        {
            return m_viewMatrix;
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

            var heading = FastMath.ToRad(m_headingDegrees);
            var pitch = FastMath.ToRad(m_pitchDegrees);

            Quaternion rot;

            if (heading != 0.0f)
            {
                rot = Quaternion.RotationAxis(m_targetYAxis.ToVector3(), heading);
                m_orientation = Quaternion.Multiply(rot, m_orientation);
            }

            if (pitch != 0.0f)
            {
                rot = Quaternion.RotationAxis(WORLD_XAXIS.ToVector3(), pitch);
                m_orientation = Quaternion.Multiply(m_orientation, rot);
            }
        }

        private void updateViewMatrix()
        {
            m_orientation.Normalize();
            m_viewMatrix = TGCMatrix.RotationQuaternion(m_orientation);

            var m_xAxis = new TGCVector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            var m_yAxis = new TGCVector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            var m_zAxis = new TGCVector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

            Eye = Target + m_zAxis * -m_offsetDistance;

            m_viewMatrix.M41 = -TGCVector3.Dot(m_xAxis, Eye);
            m_viewMatrix.M42 = -TGCVector3.Dot(m_yAxis, Eye);
            m_viewMatrix.M43 = -TGCVector3.Dot(m_zAxis, Eye);
        }

        private void updateViewMatrix(float elapsedTimeSec)
        {
            m_orientation.Normalize();
            m_viewMatrix = TGCMatrix.RotationQuaternion(m_orientation);

            var m_xAxis = new TGCVector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            var m_yAxis = new TGCVector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            var m_zAxis = new TGCVector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

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
            var displacement = Eye - idealPosition;
            var springAcceleration = -Spring * displacement - Damping * m_velocity;

            m_velocity += springAcceleration * elapsedTimeSec;
            Eye += m_velocity * elapsedTimeSec;

            // The view matrix is always relative to the camera's current position
            // 'm_eye'. Since a spring system is being used here 'm_eye' will be
            // relative to 'desiredPosition'. When the camera is no longer being moved
            // 'm_eye' will become the same as 'desiredPosition'. The local x, y, and
            // z axes that were extracted from the camera's orientation 'm_orienation'
            // is correct for the 'desiredPosition' only. We need to recompute these
            // axes so that they're relative to 'm_eye'. Once that's done we can use
            // those axes to reconstruct the view matrix.

            m_zAxis = Target - Eye;
            m_zAxis.Normalize();

            m_xAxis = TGCVector3.Cross(m_targetYAxis, m_zAxis);
            m_xAxis.Normalize();

            m_yAxis = TGCVector3.Cross(m_zAxis, m_xAxis);
            m_yAxis.Normalize();

            m_viewMatrix = TGCMatrix.Identity;

            m_viewMatrix.M11 = m_xAxis.X;
            m_viewMatrix.M21 = m_xAxis.Y;
            m_viewMatrix.M31 = m_xAxis.Z;
            m_viewMatrix.M41 = -TGCVector3.Dot(m_xAxis, Eye);

            m_viewMatrix.M12 = m_yAxis.X;
            m_viewMatrix.M22 = m_yAxis.Y;
            m_viewMatrix.M32 = m_yAxis.Z;
            m_viewMatrix.M42 = -TGCVector3.Dot(m_yAxis, Eye);

            m_viewMatrix.M13 = m_zAxis.X;
            m_viewMatrix.M23 = m_zAxis.Y;
            m_viewMatrix.M33 = m_zAxis.Z;
            m_viewMatrix.M43 = -TGCVector3.Dot(m_zAxis, Eye);
        }

        public TGCVector3 calulateNextPosition(float headingDegrees, float elapsedTimeSec)
        {
            var heading = FastMath.ToRad(-headingDegrees * elapsedTimeSec);

            var quatOrientation = m_orientation;
            if (heading != 0.0f)
            {
                var rot = Quaternion.RotationAxis(m_targetYAxis.ToVector3(), heading);
                quatOrientation = Quaternion.Multiply(rot, quatOrientation);
            }

            quatOrientation.Normalize();
            var viewMatrix = TGCMatrix.RotationQuaternion(quatOrientation);

            var m_zAxis = new TGCVector3(viewMatrix.M13, viewMatrix.M23, viewMatrix.M33);
            var idealPosition = Target + m_zAxis * -m_offsetDistance;

            return idealPosition;
        }

        /// <summary>
        ///     Permite configurar la orientacion de la cámara respecto del Target
        /// </summary>
        /// <param name="cameraRotation">Rotación absoluta a aplicar</param>
        public void setOrientation(TGCVector3 cameraRotation)
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

        /// <summary>
        ///     Habilitar desplazamiento de la camara con delay
        /// </summary>
        public bool EnableSpringSystem { get; set; }

        /// <summary>
        ///     Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public TGCVector3 Target { get; set; }

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
        public TGCVector3 Eye { get; set; }

        #endregion Getters y Setters
    }
}