using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.Input
{
    /// <summary>
    /// Camara en Tercera Persona que permite seguir a un objeto que se desplaza
    /// desde atrás, con una cierta distancia. Posee un efecto de Spring que realiza
    /// la rotación suavemente hasta llegar al ángulo deseado.
    /// </summary>
    public class TgcSpringThirdPersonCamera : TgcCamera
    {

        static readonly float DEFAULT_SPRING_CONSTANT = 16.0f;
        static readonly float DEFAULT_DAMPING_CONSTANT = 8.0f;

        static readonly Vector3 WORLD_XAXIS = new Vector3(1.0f, 0.0f, 0.0f);
        static readonly Vector3 WORLD_YAXIS = new Vector3(0.0f, 1.0f, 0.0f);
        static readonly Vector3 WORLD_ZAXIS = new Vector3(0.0f, 0.0f, 1.0f);

        bool m_enableSpringSystem;
        float m_springConstant;
        float m_dampingConstant;
        float m_offsetDistance;
        float m_headingDegrees;
        float m_pitchDegrees;
        Vector3 m_eye;
        Vector3 m_target;
        Vector3 m_targetYAxis;
        Vector3 m_velocity;
        Matrix m_viewMatrix;
        Quaternion m_orientation;



        public TgcSpringThirdPersonCamera()
        {
            resetValues();
        }

        #region Getters y Setters

        bool enable;
        /// <summary>
        /// Habilita o no el uso de la camara
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
                    GuiController.Instance.CurrentCamera = this;
                }
            }
        }

        /// <summary>
        /// Habilitar desplazamiento de la camara con delay
        /// </summary>
        public bool EnableSpringSystem
        {
            get { return m_enableSpringSystem; }
            set { m_enableSpringSystem = value; }
        }

        /// <summary>
        /// Objetivo al cual la camara tiene que apuntar
        /// </summary>
        public Vector3 Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        /// <summary>
        /// Valor de Spring para el delay de la camara 
        /// </summary>
        public float Spring
        {
            get { return m_springConstant; }
            set { m_springConstant = value; }
        }

        /// <summary>
        /// Valor de Damping para el delay de la camara 
        /// </summary>
        public float Damping
        {
            get { return m_dampingConstant; }
            set { m_dampingConstant = value; }
        }

        /// <summary>
        /// Posicion del ojo de la camara que apunta hacia el Target
        /// </summary>
        public Vector3 Position
        {
            get { return m_eye; }
        }


        #endregion

        /// <summary>
        /// Carga los valores default de la camara y limpia todos los cálculos intermedios
        /// </summary>
        public void resetValues()
        {
            m_enableSpringSystem = true;
            m_springConstant = DEFAULT_SPRING_CONSTANT;
            m_dampingConstant = DEFAULT_DAMPING_CONSTANT;

            m_offsetDistance = 0.0f;
            m_headingDegrees = 0.0f;
            m_pitchDegrees = 0.0f;

            m_eye = new Vector3(0.0f, 0.0f, 0.0f);
            m_target = new Vector3(0.0f, 0.0f, 0.0f);
            m_targetYAxis = new Vector3(0, 1, 0);

            m_velocity = new Vector3(0.0f, 0.0f, 0.0f);

            m_viewMatrix = Matrix.Identity;
            m_orientation = Quaternion.Identity;
        }

        
        /// <summary>
        /// Configura los valores iniciales de la cámara
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="target"></param>
        public void setCamera(Vector3 target, float offsetY, float offsetZ)
        {
            m_eye = new Vector3(target.X, target.Y + offsetY, target.Z + offsetZ); ;
            m_target = target;

            m_viewMatrix = Matrix.LookAtLH(m_eye, m_target, m_targetYAxis);
            m_orientation = Quaternion.RotationMatrix(m_viewMatrix);

            Vector3 offset = m_target - m_eye;
            m_offsetDistance = offset.Length();

            m_headingDegrees = 0.0f;
            m_pitchDegrees = 0.0f;
        }

        /// <summary>
        /// Rota la cámara.
        /// Debe ser rotada la misma cantidad que se rota al Target a seguir.
        /// Se especifica en Degrees y no en Radianes
        /// </summary>
        /// <param name="headingDegrees">Angulo de rotacion en Grados</param>
        public void rotateY(float headingDegrees)
        {
            m_headingDegrees = -headingDegrees;
            m_pitchDegrees = 0;
        }



        /// <summary>
        /// Actualiza los valores de la camara
        /// </summary>
        public void updateCamera()
        {
            if (!enable)
            {
                return;
            }

            float elapsedTimeSec = GuiController.Instance.ElapsedTime;
            updateOrientation(elapsedTimeSec);

            if (m_enableSpringSystem)
            {
                updateViewMatrix(elapsedTimeSec);
            }
            else
            {
                updateViewMatrix();
            }

        }

        private void updateOrientation(float elapsedTimeSec)
        {
            m_headingDegrees *= elapsedTimeSec;
            m_pitchDegrees *= elapsedTimeSec;

            float heading = Geometry.DegreeToRadian(m_headingDegrees);
            float pitch = Geometry.DegreeToRadian(m_pitchDegrees);

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

            Vector3 m_xAxis = new Vector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            Vector3 m_yAxis = new Vector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            Vector3 m_zAxis = new Vector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

            m_eye = m_target + m_zAxis * -m_offsetDistance;

            m_viewMatrix.M41 = -Vector3.Dot(m_xAxis, m_eye);
            m_viewMatrix.M42 = -Vector3.Dot(m_yAxis, m_eye);
            m_viewMatrix.M43 = -Vector3.Dot(m_zAxis, m_eye);
        }

        private void updateViewMatrix(float elapsedTimeSec)
        {
            m_orientation.Normalize();
            m_viewMatrix = Matrix.RotationQuaternion(m_orientation);

            Vector3 m_xAxis = new Vector3(m_viewMatrix.M11, m_viewMatrix.M21, m_viewMatrix.M31);
            Vector3 m_yAxis = new Vector3(m_viewMatrix.M12, m_viewMatrix.M22, m_viewMatrix.M32);
            Vector3 m_zAxis = new Vector3(m_viewMatrix.M13, m_viewMatrix.M23, m_viewMatrix.M33);

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

            Vector3 idealPosition = m_target + m_zAxis * -m_offsetDistance;
            Vector3 displacement = m_eye - idealPosition;
            Vector3 springAcceleration = (-m_springConstant * displacement) - (m_dampingConstant * m_velocity);

            m_velocity += springAcceleration * elapsedTimeSec;
            m_eye += m_velocity * elapsedTimeSec;

            // The view matrix is always relative to the camera's current position
            // 'm_eye'. Since a spring system is being used here 'm_eye' will be
            // relative to 'desiredPosition'. When the camera is no longer being moved
            // 'm_eye' will become the same as 'desiredPosition'. The local x, y, and
            // z axes that were extracted from the camera's orientation 'm_orienation'
            // is correct for the 'desiredPosition' only. We need to recompute these
            // axes so that they're relative to 'm_eye'. Once that's done we can use
            // those axes to reconstruct the view matrix.

            m_zAxis = m_target - m_eye;
            m_zAxis.Normalize();

            m_xAxis = Vector3.Cross(m_targetYAxis, m_zAxis);
            m_xAxis.Normalize();

            m_yAxis = Vector3.Cross(m_zAxis, m_xAxis);
            m_yAxis.Normalize();

            m_viewMatrix = Matrix.Identity;

            m_viewMatrix.M11 = m_xAxis.X;
            m_viewMatrix.M21 = m_xAxis.Y;
            m_viewMatrix.M31 = m_xAxis.Z;
            m_viewMatrix.M41 = -Vector3.Dot(m_xAxis, m_eye);

            m_viewMatrix.M12 = m_yAxis.X;
            m_viewMatrix.M22 = m_yAxis.Y;
            m_viewMatrix.M32 = m_yAxis.Z;
            m_viewMatrix.M42 = -Vector3.Dot(m_yAxis, m_eye);

            m_viewMatrix.M13 = m_zAxis.X;
            m_viewMatrix.M23 = m_zAxis.Y;
            m_viewMatrix.M33 = m_zAxis.Z;
            m_viewMatrix.M43 = -Vector3.Dot(m_zAxis, m_eye);
        }

        public Vector3 calulateNextPosition(float headingDegrees, float elapsedTimeSec)
        {
            float heading = Geometry.DegreeToRadian(-headingDegrees * elapsedTimeSec);

            Quaternion quatOrientation = m_orientation;
            if (heading != 0.0f)
            {
                Quaternion rot = Quaternion.RotationAxis(m_targetYAxis, heading);
                quatOrientation = Quaternion.Multiply(rot, quatOrientation);
            }


            quatOrientation.Normalize();
            Matrix viewMatrix = Matrix.RotationQuaternion(quatOrientation);

            Vector3 m_zAxis = new Vector3(viewMatrix.M13, viewMatrix.M23, viewMatrix.M33);
            Vector3 idealPosition = m_target + m_zAxis * -m_offsetDistance;

            return idealPosition;
        }

        /// <summary>
        /// Permite configurar la orientacion de la cámara respecto del Target
        /// </summary>
        /// <param name="cameraRotation">Rotación absoluta a aplicar</param>
        public void setOrientation(Vector3 cameraRotation)
        {
            m_orientation = Quaternion.RotationYawPitchRoll(cameraRotation.Y, cameraRotation.X, cameraRotation.Z);
            m_headingDegrees = 0;
            m_pitchDegrees = 0;
        }

        /// <summary>
        /// Actualiza la ViewMatrix, si es que la camara esta activada
        /// </summary>
        public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            if (!enable)
            {
                return;
            }

            d3dDevice.Transform.View = m_viewMatrix;
        }

        public Vector3 getPosition()
        {
            return m_eye;
        }

        public Vector3 getLookAt()
        {
            return m_target;
        }

        public void setOffset(float offset)
        {
            m_offsetDistance = offset;
        }

    }
}
