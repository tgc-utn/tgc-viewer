using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Utils;

namespace TGC.Core.Camara
{
    /// <summary>
    ///     Camara en primera persona que utiliza matrices de rotacion, solo almacena las rotaciones en updown y costados.
    ///     Ref: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Mouse_camera.php
    ///     Autor: Rodrigo Garcia.
    /// </summary>
    public class TgcFpsCamera : TgcCamera
    {
        private Vector3 positionEye;
        private Vector3 directionView;

        //No hace falta la base ya que siempre es la misma, la base se arma segun las rotaciones de esto costados y updown.
        private float leftrightRot = FastMath.PI_HALF;
        private float updownRot = -FastMath.PI / 10.0f;

        private bool lockCam;
        private Point mouseCenter; //Centro de mause 2D para ocultarlo.
        //Se mantiene la matriz rotacion para no hacer este calculo cada vez.
        private Matrix cameraRotation;

        public TgcFpsCamera()
        {
            positionEye = new Vector3();
            directionView = new Vector3(0, 0, -1);
            mouseCenter = new Point(
                    D3DDevice.Instance.Device.Viewport.Width / 2,
                    D3DDevice.Instance.Device.Viewport.Height / 2);
            RotationSpeed = 0.1f;
            MovementSpeed = 500f;
            JumpSpeed = 500f;
            cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
        }

        public TgcFpsCamera(Vector3 positionEye, Vector3 directionView) : this()
        {
            this.positionEye = positionEye;
            this.directionView = directionView;
        }

        public TgcFpsCamera(Vector3 positionEye) : this()
        {
            this.positionEye = positionEye;
        }

        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed) : this(positionEye)
        {
            MovementSpeed = moveSpeed;
            JumpSpeed = jumpSpeed;
        }
        public TgcFpsCamera(Vector3 positionEye, float moveSpeed, float jumpSpeed, float rotationSpeed) : this(positionEye, moveSpeed, jumpSpeed)
        {
            RotationSpeed = rotationSpeed;
        }

        public bool LockCam
        {
            get { return lockCam; }
            set
            {
                if (!lockCam && value)
                {
                    Cursor.Position = mouseCenter;

                    Cursor.Hide();
                }
                if (lockCam && !value)
                    Cursor.Show();
                lockCam = value;
            }
        }

        /// <summary>
        /// Cuando se elimina esto hay que desbloquear la camera.
        /// </summary>
        ~TgcFpsCamera()
        {
            LockCam = false;
        }

        public float MovementSpeed { get; set; }

        public float RotationSpeed { get; set; }

        public float JumpSpeed { get; set; }
        
        public override void updateCamera(float elapsedTime)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            //Forward
            if (TgcD3dInput.Instance.keyDown(Key.W))
            {
                moveVector += new Vector3(0, 0, -1) * MovementSpeed;
            }

            //Backward
            if (TgcD3dInput.Instance.keyDown(Key.S))
            {
                moveVector += new Vector3(0, 0, 1) * MovementSpeed;
            }

            //Strafe right
            if (TgcD3dInput.Instance.keyDown(Key.D))
            {
                moveVector += new Vector3(-1, 0, 0) * MovementSpeed;
            }

            //Strafe left
            if (TgcD3dInput.Instance.keyDown(Key.A))
            {
                moveVector += new Vector3(1, 0, 0) * MovementSpeed;
            }

            //Jump
            if (TgcD3dInput.Instance.keyDown(Key.Space))
            {
                 moveVector += new Vector3(0, 1, 0) * JumpSpeed;
            }

            //Crouch
            if (TgcD3dInput.Instance.keyDown(Key.LeftControl))
            {
                moveVector += new Vector3(0, -1, 0) * JumpSpeed;
            }

            if (TgcD3dInput.Instance.keyPressed(Key.L) || TgcD3dInput.Instance.keyPressed(Key.Escape))
            {
                LockCam = !lockCam;
            }

            //Solo rotar si se esta aprentando el boton izq del mouse
            if (lockCam || TgcD3dInput.Instance.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                leftrightRot -= (-TgcD3dInput.Instance.XposRelative * RotationSpeed);
                updownRot -= (TgcD3dInput.Instance.YposRelative * RotationSpeed);
                //Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
                cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
            }

            if (lockCam)
                Cursor.Position = mouseCenter;

            //Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
            Vector3 cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, cameraRotation);
            positionEye += cameraRotatedPositionEye; 

            //Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
            Vector3 cameraRotatedTarget = Vector3.TransformNormal(directionView, cameraRotation);
            Vector3 cameraFinalTarget = positionEye + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = base.DEFAULT_UP_VECTOR;
            Vector3 cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, cameraRotation);

            base.setCamera(positionEye, cameraFinalTarget, cameraRotatedUpVector);
        }

        /// <summary>
        ///  se hace override para actualizar las posiones internas, estas seran utilizadas en el proximo update.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="directionView"></param>
        public override void setCamera(Vector3 position, Vector3 directionView)
        {
            this.positionEye = position;
            this.directionView = directionView;
        }
        
    }
}
