using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using Device = Microsoft.DirectX.Direct3D.Device;

namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Camara en primera persona personalizada para niveles de Quake 3.
    ///     Evita utilizar senos y cosenos
    ///     Autor: Martin Giachetti
    /// </summary>
    public class Q3FpsCamera : TgcCamera
    {
        private const float LADO_CUBO = 1.0f;
        private const float MEDIO_LADO_CUBO = LADO_CUBO * 0.5f;
        private readonly float STEP_ANGULO = LADO_CUBO / 90;
        private readonly TGCVector3 up = new TGCVector3(0.0f, 1.0f, 0.0f);
        /*
         * Esta Camara es un prototipo. Esta pensada para no utilizar senos y cosenos en las rotaciones.
         * Se utiliza una camara que se desplaza sobre las caras de un cubo sin techo, ni piso.
         * La teoria es la siguiente: La direccion donde mira la camara esta formado por dos puntos, el ojo y el target.
         * Si el ojo es el centro del cubo y el target es un punto que se desplaza por las caras del cubo.
         * Entonces se puede cambiar el angulo de la direccion desplazando proporcionalmente a la cantidad de grados el punto
         * target sobre las caras del cubo.
         */
        private TGCVector3 eye = TGCVector3.Empty;
        private float latitud;
        private bool lockCam;
        private float longitud;
        protected Point mouseCenter;
        private TGCVector3 sideDirection = TGCVector3.Empty;
        private TGCVector3 target = TGCVector3.Empty;
        private TGCMatrix viewMatrix = TGCMatrix.Identity;

        public Q3FpsCamera()
        {
            var focusWindows = D3DDevice.Instance.Device.CreationParameters.FocusWindow;
            mouseCenter = focusWindows.PointToScreen(
                new Point(
                    focusWindows.Width / 2,
                    focusWindows.Height / 2)
                );
        }

        public TGCVector3 ForwardDirection { get; private set; } = TGCVector3.Empty;

        public TGCVector3 SideDirection
        {
            get { return sideDirection; }
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

        public float MovementSpeed { get; set; }

        public float RotationSpeed { get; set; }

        public float JumpSpeed { get; set; }

        //No se usa, solo esta por la herencia
        public bool Enable { get; set; }

        ~Q3FpsCamera()
        {
            LockCam = false;
        }

        private void recalcularDirecciones()
        {
            var forward = target - eye;
            forward.Y = 0;
            forward.Normalize();

            ForwardDirection = forward;
            sideDirection.X = forward.Z;
            sideDirection.Z = -forward.X;
        }

        public void move(TGCVector3 v)
        {
            eye.Add(v);
            target.Add(v);
        }

        public void moveForward(float movimiento)
        {
            var v = ForwardDirection * movimiento;
            move(v);
        }

        public void moveSide(float movimiento)
        {
            var v = SideDirection * movimiento;
            move(v);
        }

        public void moveUp(float movimiento)
        {
            move(up * movimiento);
        }

        public void rotateY(float movimiento)
        {
            //rotate(movimiento, 0.0f, 0.0f);

            rotate(movimiento, 0);
        }

        public void rotateXZ(float movimiento)
        {
            rotate(0, movimiento);
        }

        public void rotate(float lat, float lon)
        {
            latitud += lat;
            if (latitud >= 360)
                latitud -= 360;

            if (latitud < 0)
                latitud += 360;

            longitud += lon;
            if (longitud > 90)
                longitud = 90;
            if (longitud < 0)
                longitud = 0;

            recalcularTarget();
        }

        private void recalcularTarget()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (latitud < 180)
            {
                if (latitud < 90)
                {
                    z = latitud * STEP_ANGULO;
                }
                else
                {
                    z = LADO_CUBO;
                    x = (latitud - 90) * STEP_ANGULO;
                }
                z = z - MEDIO_LADO_CUBO;
                x = MEDIO_LADO_CUBO - x;
            }
            else
            {
                if (latitud < 270)
                {
                    z = (latitud - 180) * STEP_ANGULO;
                }
                else
                {
                    z = LADO_CUBO;
                    x = (latitud - 270) * STEP_ANGULO;
                }
                z = MEDIO_LADO_CUBO - z;
                x = x - MEDIO_LADO_CUBO;
            }

            y = longitud * STEP_ANGULO - MEDIO_LADO_CUBO;

            target = eye + new TGCVector3(x, y, z);

            recalcularDirecciones();
        }

        public void setCamera(TGCVector3 eye, TGCVector3 target)
        {
            this.eye = eye;
            this.target = target;

            var dir = eye - target;

            //calculo el angulo correspondiente a la latitud y longitud.
            if (Math.Abs(dir.X) > 0)
                latitud = 180 * (float)Math.Atan(dir.Z / dir.X) / (float)Math.PI + 45;
            else
                latitud = 135;

            longitud = 180 * (float)Math.Atan(dir.Y / Math.Sqrt(dir.X * dir.X + dir.Z * dir.Z)) / (float)Math.PI + 45;

            rotateY(0);

            recalcularDirecciones();
        }

        #region Miembros de TgcCamera

        public TGCVector3 getPosition()
        {
            return eye;
        }

        public TGCVector3 getLookAt()
        {
            return target;
        }

        public void Update(float elapsedTime, TgcD3dInput input)
        {
            //Forward
            if (input.keyDown(Key.W))
            {
                moveForward(MovementSpeed * elapsedTime);
            }

            //Backward
            if (input.keyDown(Key.S))
            {
                moveForward(-MovementSpeed * elapsedTime);
            }

            //Strafe right
            if (input.keyDown(Key.D))
            {
                moveSide(MovementSpeed * elapsedTime);
            }

            //Strafe left
            if (input.keyDown(Key.A))
            {
                moveSide(-MovementSpeed * elapsedTime);
            }

            //Jump
            if (input.keyDown(Key.Space))
            {
                moveUp(JumpSpeed * elapsedTime);
            }

            //Crouch
            if (input.keyDown(Key.LeftControl))
            {
                moveUp(-JumpSpeed * elapsedTime);
            }

            if (input.keyPressed(Key.L))
            {
                LockCam = !LockCam;
            }

            //Solo rotar si se esta aprentando el boton izq del mouse
            if (lockCam || input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                rotate(-input.XposRelative * RotationSpeed, -input.YposRelative * RotationSpeed);
            }

            if (lockCam)
                Cursor.Position = mouseCenter;

            viewMatrix = TGCMatrix.LookAtLH(eye, target, up);

            updateViewMatrix(D3DDevice.Instance.Device);
        }

        public void updateViewMatrix(Device d3dDevice)
        {
            d3dDevice.Transform.View = viewMatrix;
        }

        #endregion Miembros de TgcCamera
    }
}