using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using TgcViewer;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Camara rotacional customizada para el MeshCreator
    /// </summary>
    public class MeshCreatorCamera : TgcCamera
    {
        public static float DEFAULT_ZOOM_FACTOR = 0.15f;
        public static float DEFAULT_CAMERA_DISTANCE = 10f;
        public static float DEFAULT_ROTATION_SPEED = 100f;

        Vector3 upVector;
        Vector3 cameraCenter;
        Vector3 nextPos;
        float cameraDistance;
        float zoomFactor;
        float diffX;
        float diffY;
        float diffZ;
        Matrix viewMatrix;
        float rotationSpeed;
        float panSpeed;
        
        


        public MeshCreatorCamera()
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
            set { enable = value; }
        }

        /// <summary>
        /// Centro de la camara sobre la cual se rota
        /// </summary>
        public Vector3 CameraCenter
        {
            get { return cameraCenter; }
            set { cameraCenter = value; }
        }

        /// <summary>
        /// Distance entre la camara y el centro
        /// </summary>
        public float CameraDistance
        {
            get { return cameraDistance; }
            set { cameraDistance = value; }
        }

        /// <summary>
        /// Velocidad con la que se hace Zoom
        /// </summary>
        public float ZoomFactor
        {
            get { return zoomFactor; }
            set { zoomFactor = value; }
        }

        /// <summary>
        /// Velocidad de rotacion de la camara
        /// </summary>
        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        /// <summary>
        /// Velocidad de paneo
        /// </summary>
        public float PanSpeed
        {
            get { return panSpeed; }
            set { panSpeed = value; }
        }

        float baseRotX;
        /// <summary>
        /// Rotacion inicial que siempre tiene en el eje X
        /// </summary>
        public float BaseRotX
        {
            get { return baseRotX; }
            set { baseRotX = value; }
        }

        float baseRotY;
        /// <summary>
        /// Rotacion inicial que siempre tiene en el eje Y
        /// </summary>
        public float BaseRotY
        {
            get { return baseRotY; }
            set { baseRotY = value; }
        }

        /// <summary>
        /// Configura el centro de la camara, la distancia y la velocidad de zoom
        /// </summary>
        public void setCamera(Vector3 cameraCenter, float cameraDistance, float zoomFactor)
        {
            this.cameraCenter = cameraCenter;
            this.cameraDistance = cameraDistance;
            this.zoomFactor = zoomFactor;
        }

        /// <summary>
        /// Configura el centro de la camara, la distancia
        /// </summary>
        public void setCamera(Vector3 cameraCenter, float cameraDistance)
        {
            this.cameraCenter = cameraCenter;
            this.cameraDistance = cameraDistance;
            this.zoomFactor = DEFAULT_ZOOM_FACTOR;
        }




        #endregion

        /// <summary>
        /// Carga los valores default de la camara
        /// </summary>
        internal void resetValues()
        {
            upVector = new Vector3(0.0f, 1.0f, 0.0f);
            cameraCenter = new Vector3(0, 0, 0);
            nextPos = new Vector3(0, 0, 0);
            cameraDistance = DEFAULT_CAMERA_DISTANCE;
            zoomFactor = DEFAULT_ZOOM_FACTOR;
            rotationSpeed = DEFAULT_ROTATION_SPEED;
            diffX = 0f;
            diffY = 0f;
            diffZ = 1f;
            viewMatrix = Matrix.Identity;
            panSpeed = 0.01f;
            baseRotX = 0;
            baseRotY = 0;
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

            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            float elapsedTime = GuiController.Instance.ElapsedTime;

            //Obtener variacion XY del mouse
            float mouseX = 0f;
            float mouseY = 0f;
            if (d3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftAlt) && d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_MIDDLE))
            {
                mouseX = d3dInput.XposRelative;
                mouseY = d3dInput.YposRelative;

                diffX += mouseX * elapsedTime * rotationSpeed;
                diffY += mouseY * elapsedTime * rotationSpeed;
            }
            else
            {
                diffX += mouseX;
                diffY += mouseY;
            }
            


            //Calcular rotacion a aplicar
            float rotX = (-diffY / FastMath.PI) + baseRotX;
            float rotY = (diffX / FastMath.PI) + baseRotY;

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
                diffZ += zoomFactor * d3dInput.WheelPos * -1;
            }
            float distance = -cameraDistance * diffZ;

            //Limitar el zoom a 0
            if (distance > 0)
            {
                distance = 0;
            }

            
            //Realizar Transformacion: primero alejarse en Z, despues rotar en X e Y y despues ir al centro de la cmara
            Matrix m = Matrix.Translation(0, 0, -distance)
                * Matrix.RotationX(rotX)
                * Matrix.RotationY(rotY)
            * Matrix.Translation(cameraCenter);


            //Extraer la posicion final de la matriz de transformacion
            nextPos.X = m.M41;
            nextPos.Y = m.M42;
            nextPos.Z = m.M43;


            //Hacer efecto de Pan View
            if (!d3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftAlt) && d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_MIDDLE))
            {
                float dx = -d3dInput.XposRelative;
                float dy = d3dInput.YposRelative;
                float panSpeedZoom = panSpeed * FastMath.Abs(distance);

                Vector3 d = cameraCenter - nextPos;
                d.Normalize();

                Vector3 n = Vector3.Cross(d, upVector);
                n.Normalize();

                Vector3 up = Vector3.Cross(n, d);
                Vector3 desf = Vector3.Scale(up, dy * panSpeedZoom) - Vector3.Scale(n, dx * panSpeedZoom);
                nextPos = nextPos + desf;
                cameraCenter = cameraCenter + desf;
            }

            //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            viewMatrix = Matrix.LookAtLH(nextPos, cameraCenter, upVector);
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

            d3dDevice.Transform.View = viewMatrix;
        }

        /// <summary>
        /// Setear la camara con una determinada posicion y lookAt
        /// </summary>
        public void lookAt(Vector3 pos, Vector3 lookAt)
        {
            //TODO: solo funciona bien para hacer un TopView

            Vector3 v = pos - lookAt;
            float length = Vector3.Length(v);
            v.Scale(1 / length);

            cameraDistance = length;
            upVector = new Vector3(0, 1, 0);
            diffX = 0;
            diffY = 0.01f;
            diffZ = 1;
            baseRotX = -FastMath.Acos(Vector3.Dot(new Vector3(0, 0, -1), v));
            //baseRotY = FastMath.Acos(Vector3.Dot(new Vector3(0, 0, -1), v));
            baseRotY = 0;
            cameraCenter = lookAt;
        }

        public Vector3 getPosition()
        {
            return nextPos;
        }

        public Vector3 getLookAt()
        {
            return cameraCenter;
        }

        /// <summary>
        /// Configura los parámetros de la cámara en funcion del BoundingBox de un modelo
        /// </summary>
        /// <param name="boundingBox">BoundingBox en base al cual configurar</param>
        public void targetObject(TgcBoundingBox boundingBox)
        {
            cameraCenter = boundingBox.calculateBoxCenter();
            float r = boundingBox.calculateBoxRadius();
            cameraDistance = 2 * r;
        }

    }
}
