using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.Input
{
    /// <summary>
    /// Cámara en primera persona, con movimientos: W, A, S, D, Space, LeftControl
    /// Soporta movimiento con aceleración
    /// </summary>
    public class TgcFpsCamera : TgcCamera
    {
        //Constantes de movimiento
        const float DEFAULT_ROTATION_SPEED = 2f;
        const float DEFAULT_MOVEMENT_SPEED = 100f;
        const float DEFAULT_JUMP_SPEED = 100f;
        readonly Vector3 CAMERA_VELOCITY = new Vector3(DEFAULT_MOVEMENT_SPEED, DEFAULT_JUMP_SPEED, DEFAULT_MOVEMENT_SPEED);
        readonly Vector3 CAMERA_POS = new Vector3(0.0f, 1.0f, 0.0f);
        readonly Vector3 CAMERA_ACCELERATION = new Vector3(400f, 400f, 400f);

        //Ejes para ViewMatrix
        readonly Vector3 WORLD_XAXIS = new Vector3(1.0f, 0.0f, 0.0f);
        readonly Vector3 WORLD_YAXIS = new Vector3(0.0f, 1.0f, 0.0f);
        readonly Vector3 WORLD_ZAXIS = new Vector3(0.0f, 0.0f, 1.0f);
        readonly Vector3 DEFAULT_UP_VECTOR = new Vector3(0.0f, 1.0f, 0.0f);

        float accumPitchDegrees;
        Vector3 eye;
        Vector3 xAxis;
        Vector3 yAxis;
        Vector3 zAxis;
        Vector3 viewDir;
        Vector3 lookAt;

        //Banderas de Input
        bool moveForwardsPressed = false;
        bool moveBackwardsPressed = false;
        bool moveRightPressed = false;
        bool moveLeftPressed = false;
        bool moveUpPressed = false;
        bool moveDownPressed = false;

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

        Vector3 acceleration;
        /// <summary>
        /// Aceleracion de la camara en cada uno de sus ejes
        /// </summary>
        public Vector3 Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        bool accelerationEnable;
        /// <summary>
        /// Activa o desactiva el efecto de Aceleración/Desaceleración
        /// </summary>
        public bool AccelerationEnable
        {
            get { return accelerationEnable; }
            set { accelerationEnable = value; }
        }

        Vector3 currentVelocity;
        /// <summary>
        /// Velocidad de desplazamiento actual, teniendo en cuenta la aceleracion
        /// </summary>
        public Vector3 CurrentVelocity
        {
            get { return currentVelocity; }
        }

        Vector3 velocity;
        /// <summary>
        /// Velocidad de desplazamiento de la cámara en cada uno de sus ejes
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        /// <summary>
        /// Velocidad de desplazamiento de los ejes XZ de la cámara
        /// </summary>
        public float MovementSpeed
        {
            get { return velocity.X; }
            set {
                velocity.X = value;
                velocity.Z = value;
            }
        }

        /// <summary>
        /// Velocidad de desplazamiento del eje Y de la cámara
        /// </summary>
        public float JumpSpeed
        {
            get { return velocity.Y; }
            set { velocity.Y = value; }
        }

        float rotationSpeed;
        /// <summary>
        /// Velocidad de rotacion de la cámara
        /// </summary>
        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        Matrix viewMatrix;
        /// <summary>
        /// View Matrix resultante
        /// </summary>
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        /// <summary>
        /// Posicion actual de la camara
        /// </summary>
        public Vector3 Position
        {
            get { return eye; }
        }

        /// <summary>
        /// Punto hacia donde mira la cámara
        /// </summary>
        public Vector3 LookAt
        {
            get { return lookAt; }
        }

        TgcD3dInput.MouseButtons rotateMouseButton;
        /// <summary>
        /// Boton del mouse que debe ser presionado para rotar la camara.
        /// Por default es boton izquierdo.
        /// </summary>
        public TgcD3dInput.MouseButtons RotateMouseButton
        {
            get { return rotateMouseButton; }
            set { rotateMouseButton = value; }
        }


        #endregion

        /// <summary>
        /// Crea la cámara con valores iniciales.
        /// Aceleración desactivada por Default
        /// </summary>
        public TgcFpsCamera()
        {
            resetValues();
        }

        /// <summary>
        /// Carga los valores default de la camara
        /// </summary>
        public void resetValues()
        {
            accumPitchDegrees = 0.0f;
            rotationSpeed = DEFAULT_ROTATION_SPEED;
            eye = new Vector3(0.0f, 0.0f, 0.0f);
            xAxis = new Vector3(1.0f, 0.0f, 0.0f);
            yAxis = new Vector3(0.0f, 1.0f, 0.0f);
            zAxis = new Vector3(0.0f, 0.0f, 1.0f);
            viewDir = new Vector3(0.0f, 0.0f, 1.0f);
            lookAt = eye + viewDir;

            accelerationEnable = false;
            acceleration = CAMERA_ACCELERATION;
            currentVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            velocity = CAMERA_VELOCITY;
            viewMatrix = Matrix.Identity;
            setPosition(CAMERA_POS);

            rotateMouseButton = TgcD3dInput.MouseButtons.BUTTON_LEFT;
        }


        /// <summary>
        /// Configura la posicion de la cámara
        /// </summary>
        private void setCamera(Vector3 eye, Vector3 target, Vector3 up)
        {
            this.eye = eye;

            zAxis = target - eye;
            zAxis.Normalize();

            viewDir = zAxis;
            lookAt = eye + viewDir;

            xAxis = Vector3.Cross(up, zAxis);
            xAxis.Normalize();

            yAxis = Vector3.Cross(zAxis, xAxis);
            yAxis.Normalize();
            //xAxis.Normalize();

            viewMatrix = Matrix.Identity;

            viewMatrix.M11 = xAxis.X;
            viewMatrix.M21 = xAxis.Y;
            viewMatrix.M31 = xAxis.Z;
            viewMatrix.M41 = -Vector3.Dot(xAxis, eye);

            viewMatrix.M12 = yAxis.X;
            viewMatrix.M22 = yAxis.Y;
            viewMatrix.M32 = yAxis.Z;
            viewMatrix.M42 = -Vector3.Dot(yAxis, eye);

            viewMatrix.M13 = zAxis.X;
            viewMatrix.M23 = zAxis.Y;
            viewMatrix.M33 = zAxis.Z;
            viewMatrix.M43 = -Vector3.Dot(zAxis, eye);

            // Extract the pitch angle from the view matrix.
            accumPitchDegrees = Geometry.RadianToDegree((float)-Math.Asin((double)viewMatrix.M23));
        }

        /// <summary>
        /// Configura la posicion de la cámara
        /// </summary>
        public void setCamera(Vector3 pos, Vector3 lookAt)
        {
            setCamera(pos, lookAt, DEFAULT_UP_VECTOR);
        }

        /// <summary>
        /// Moves the camera by dx world units to the left or right; dy
        /// world units upwards or downwards; and dz world units forwards
        /// or backwards.
        /// </summary>
        private void move(float dx, float dy, float dz)
        {

            Vector3 auxEye = this.eye;
            Vector3 forwards;

            // Calculate the forwards direction. Can't just use the camera's local
            // z axis as doing so will cause the camera to move more slowly as the
            // camera's view approaches 90 degrees straight up and down.
            forwards = Vector3.Cross(xAxis, WORLD_YAXIS);
            forwards.Normalize();


            auxEye += xAxis * dx;
            auxEye += WORLD_YAXIS * dy;
            auxEye += forwards * dz;

            setPosition(auxEye);
        }

        /// <summary>
        /// Moves the camera by the specified amount of world units in the specified
        /// direction in world space. 
        /// </summary>
        private void move(Vector3 direction, Vector3 amount)
        {
            eye.X += direction.X * amount.X;
            eye.Y += direction.Y * amount.Y;
            eye.Z += direction.Z * amount.Z;

            reconstructViewMatrix(false);
        }

        /// <summary>
        /// Rotates the camera based on its current behavior.
        /// Note that not all behaviors support rolling.
        ///
        /// This Camera class follows the left-hand rotation rule.
        /// Angles are measured clockwise when looking along the rotation
        /// axis toward the origin. Since the Z axis is pointing into the
        /// screen we need to negate rolls.
        /// </summary>
        private void rotate(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            rollDegrees = -rollDegrees;
            rotateFirstPerson(headingDegrees, pitchDegrees);
            reconstructViewMatrix(true);
        }

        /// <summary>
        /// This method applies a scaling factor to the rotation angles prior to
        /// using these rotation angles to rotate the camera. This method is usually
        /// called when the camera is being rotated using an input device (such as a
        /// mouse or a joystick).
        /// </summary>
        private void rotateSmoothly(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            headingDegrees *= rotationSpeed;
            pitchDegrees *= rotationSpeed;
            rollDegrees *= rotationSpeed;

            rotate(headingDegrees, pitchDegrees, rollDegrees);
        }

        /// <summary>
        /// Moves the camera using Newton's second law of motion. Unit mass is
        /// assumed here to somewhat simplify the calculations. The direction vector
        /// is in the range [-1,1].
        /// </summary>
        private void updatePosition(Vector3 direction, float elapsedTimeSec)
        {
            if(Vector3.LengthSq(currentVelocity) != 0.0f)
            {
                // Only move the camera if the velocity vector is not of zero length.
                // Doing this guards against the camera slowly creeping around due to
                // floating point rounding errors.

                Vector3 displacement;
                if (accelerationEnable)
                {
                    displacement = (currentVelocity * elapsedTimeSec) +
                    (0.5f * acceleration * elapsedTimeSec * elapsedTimeSec);
                }
                else
                {
                    displacement = (currentVelocity * elapsedTimeSec);
                }
                

                // Floating point rounding errors will slowly accumulate and cause the
                // camera to move along each axis. To prevent any unintended movement
                // the displacement vector is clamped to zero for each direction that
                // the camera isn't moving in. Note that the updateVelocity() method
                // will slowly decelerate the camera's velocity back to a stationary
                // state when the camera is no longer moving along that direction. To
                // account for this the camera's current velocity is also checked.

                if (direction.X == 0.0f && Math.Abs(currentVelocity.X) < 1e-6f)
                    displacement.X = 0.0f;

                if (direction.Y == 0.0f && Math.Abs(currentVelocity.Y) < 1e-6f)
                    displacement.Y = 0.0f;

                if (direction.Z == 0.0f && Math.Abs(currentVelocity.Z) < 1e-6f)
                    displacement.Z = 0.0f;

                move(displacement.X, displacement.Y, displacement.Z);
            }

            // Continuously update the camera's velocity vector even if the camera
            // hasn't moved during this call. When the camera is no longer being moved
            // the camera is decelerating back to its stationary state.

            if (accelerationEnable)
            {
                updateVelocity(direction, elapsedTimeSec);
            }
            else
            {
                updateVelocityNoAcceleration(direction);
            }
        }

        private void setPosition(Vector3 pos)
        {
            eye = pos;
            reconstructViewMatrix(false);
        }

        private void rotateFirstPerson(float headingDegrees, float pitchDegrees)
        {
            accumPitchDegrees += pitchDegrees;

            if (accumPitchDegrees > 90.0f)
            {
                pitchDegrees = 90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = 90.0f;
            }

            if (accumPitchDegrees < -90.0f)
            {
                pitchDegrees = -90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = -90.0f;
            }

            float heading = Geometry.DegreeToRadian(headingDegrees);
            float pitch = Geometry.DegreeToRadian(pitchDegrees);
            
            Matrix rotMtx;
            Vector4 result;

            // Rotate camera's existing x and z axes about the world y axis.
            if (heading != 0.0f)
            {
                rotMtx = Matrix.RotationY(heading);

                result = Vector3.Transform(xAxis, rotMtx);
                xAxis = new Vector3(result.X, result.Y, result.Z);

                result = Vector3.Transform(zAxis, rotMtx);
                zAxis = new Vector3(result.X, result.Y, result.Z);
            }

            // Rotate camera's existing y and z axes about its existing x axis.
            if (pitch != 0.0f)
            {
                rotMtx = Matrix.RotationAxis(xAxis, pitch);

                result = Vector3.Transform(yAxis, rotMtx);
                yAxis = new Vector3(result.X, result.Y, result.Z);

                result = Vector3.Transform(zAxis, rotMtx);
                zAxis = new Vector3(result.X, result.Y, result.Z);
            }
        }
 

        /// <summary>
        /// Updates the camera's velocity based on the supplied movement direction
        /// and the elapsed time (since this method was last called). The movement
        /// direction is the in the range [-1,1].
        /// </summary>
        private void updateVelocity(Vector3 direction, float elapsedTimeSec)
        {
            if (direction.X != 0.0f)
            {
                // Camera is moving along the x axis.
                // Linearly accelerate up to the camera's max speed.

                currentVelocity.X += direction.X * acceleration.X * elapsedTimeSec;

                if (currentVelocity.X > velocity.X)
                    currentVelocity.X = velocity.X;
                else if (currentVelocity.X < -velocity.X)
                    currentVelocity.X = -velocity.X;
            }
            else
            {
                // Camera is no longer moving along the x axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.X > 0.0f)
                {
                    if ((currentVelocity.X -= acceleration.X * elapsedTimeSec) < 0.0f)
                        currentVelocity.X = 0.0f;
                }
                else
                {
                    if ((currentVelocity.X += acceleration.X * elapsedTimeSec) > 0.0f)
                        currentVelocity.X = 0.0f;
                }
            }

            if (direction.Y != 0.0f)
            {
                // Camera is moving along the y axis.
                // Linearly accelerate up to the camera's max speed.

                currentVelocity.Y += direction.Y * acceleration.Y * elapsedTimeSec;

                if (currentVelocity.Y > velocity.Y)
                    currentVelocity.Y = velocity.Y;
                else if (currentVelocity.Y < -velocity.Y)
                    currentVelocity.Y = -velocity.Y;
            }
            else
            {
                // Camera is no longer moving along the y axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.Y > 0.0f)
                {
                    if ((currentVelocity.Y -= acceleration.Y * elapsedTimeSec) < 0.0f)
                        currentVelocity.Y = 0.0f;
                }
                else
                {
                    if ((currentVelocity.Y += acceleration.Y * elapsedTimeSec) > 0.0f)
                        currentVelocity.Y = 0.0f;
                }
            }

            if (direction.Z != 0.0f)
            {
                // Camera is moving along the z axis.
                // Linearly accelerate up to the camera's max speed.

                currentVelocity.Z += direction.Z * acceleration.Z * elapsedTimeSec;

                if (currentVelocity.Z > velocity.Z)
                    currentVelocity.Z = velocity.Z;
                else if (currentVelocity.Z < -velocity.Z)
                    currentVelocity.Z = -velocity.Z;
            }
            else
            {
                // Camera is no longer moving along the z axis.
                // Linearly decelerate back to stationary state.

                if (currentVelocity.Z > 0.0f)
                {
                    if ((currentVelocity.Z -= acceleration.Z * elapsedTimeSec) < 0.0f)
                        currentVelocity.Z = 0.0f;
                }
                else
                {
                    if ((currentVelocity.Z += acceleration.Z * elapsedTimeSec) > 0.0f)
                        currentVelocity.Z = 0.0f;
                }
            }
        }

        /// <summary>
        /// Actualizar currentVelocity sin aplicar aceleracion
        /// </summary>
        private void updateVelocityNoAcceleration(Vector3 direction)
        {
            currentVelocity.X = velocity.X * direction.X;
            currentVelocity.Y = velocity.Y * direction.Y;
            currentVelocity.Z = velocity.Z * direction.Z;
        }

        /// <summary>
        /// Reconstruct the view matrix.
        /// </summary>
        private void reconstructViewMatrix(bool orthogonalizeAxes)
        {
            if (orthogonalizeAxes)
            {
                // Regenerate the camera's local axes to orthogonalize them.

                zAxis.Normalize();

                yAxis = Vector3.Cross(zAxis, xAxis);
                yAxis.Normalize();

                xAxis = Vector3.Cross(yAxis, zAxis);
                xAxis.Normalize();

                viewDir = zAxis;
                lookAt = eye + viewDir;
            }

            // Reconstruct the view matrix.

            viewMatrix.M11 = xAxis.X;
            viewMatrix.M21 = xAxis.Y;
            viewMatrix.M31 = xAxis.Z;
            viewMatrix.M41 = -Vector3.Dot(xAxis, eye);

            viewMatrix.M12 = yAxis.X;
            viewMatrix.M22 = yAxis.Y;
            viewMatrix.M32 = yAxis.Z;
            viewMatrix.M42 = -Vector3.Dot(yAxis, eye);

            viewMatrix.M13 = zAxis.X;
            viewMatrix.M23 = zAxis.Y;
            viewMatrix.M33 = zAxis.Z;
            viewMatrix.M43 = -Vector3.Dot(zAxis, eye);

            viewMatrix.M14 = 0.0f;
            viewMatrix.M24 = 0.0f;
            viewMatrix.M34 = 0.0f;
            viewMatrix.M44 = 1.0f;
        }

        /// <summary>
        /// Actualiza los valores de la camara
        /// </summary>
        public void updateCamera()
        {
            //Si la camara no está habilitada, no procesar el resto del input
            if (!enable)
            {
                return;
            }

            float elapsedTimeSec = GuiController.Instance.ElapsedTime;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;

            //Imprimir por consola la posicion actual de la camara
            if ((d3dInput.keyDown(Key.LeftShift) || d3dInput.keyDown(Key.RightShift)) && d3dInput.keyPressed(Key.P))
            {
                GuiController.Instance.printCurrentPosition();
                return;
            }


            float heading = 0.0f;
            float pitch = 0.0f;

            //Obtener direccion segun entrada de teclado
            Vector3 direction = getMovementDirection(d3dInput);

            pitch = d3dInput.YposRelative * rotationSpeed;
            heading = d3dInput.XposRelative * rotationSpeed;

            //Solo rotar si se esta aprentando el boton del mouse configurado
            if (d3dInput.buttonDown(rotateMouseButton))
            {
                rotate(heading, pitch, 0.0f);
            }
            
                
            updatePosition(direction, elapsedTimeSec);
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
        /// Obtiene la direccion a moverse por la camara en base a la entrada de teclado
        /// </summary>
        private Vector3 getMovementDirection(TgcD3dInput d3dInput)
        {
            Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);

            //Forward
            if (d3dInput.keyDown(Key.W))
            {
                if (!moveForwardsPressed)
                {
                    moveForwardsPressed = true;
                    currentVelocity = new Vector3(currentVelocity.X, currentVelocity.Y, 0.0f);
                }

                direction.Z += 1.0f;
            }
            else
            {
                moveForwardsPressed = false;
            }

            //Backward
            if (d3dInput.keyDown(Key.S))
            {
                if (!moveBackwardsPressed)
                {
                    moveBackwardsPressed = true;
                    currentVelocity = new Vector3(currentVelocity.X, currentVelocity.Y, 0.0f);
                }

                direction.Z -= 1.0f;
            }
            else
            {
                moveBackwardsPressed = false;
            }

            //Strafe right
            if (d3dInput.keyDown(Key.D))
            {
                if (!moveRightPressed)
                {
                    moveRightPressed = true;
                    currentVelocity = new Vector3(0.0f, currentVelocity.Y, currentVelocity.Z);
                }

                direction.X += 1.0f;
            }
            else
            {
                moveRightPressed = false;
            }

            //Strafe left
            if (d3dInput.keyDown(Key.A))
            {
                if (!moveLeftPressed)
                {
                    moveLeftPressed = true;
                    currentVelocity = new Vector3(0.0f, currentVelocity.Y, currentVelocity.Z);
                }

                direction.X -= 1.0f;
            }
            else
            {
                moveLeftPressed = false;
            }

            //Jump
            if (d3dInput.keyDown(Key.Space))
            {
                if (!moveUpPressed)
                {
                    moveUpPressed = true;
                    currentVelocity = new Vector3(currentVelocity.X, 0.0f, currentVelocity.Z);
                }

                direction.Y += 1.0f;
            }
            else
            {
                moveUpPressed = false;
            }

            //Crouch
            if (d3dInput.keyDown(Key.LeftControl))
            {
                if (!moveDownPressed)
                {
                    moveDownPressed = true;
                    currentVelocity = new Vector3(currentVelocity.X, 0.0f, currentVelocity.Z);
                }

                direction.Y -= 1.0f;
            }
            else
            {
                moveDownPressed = false;
            }

            return direction;
        }

        public Vector3 getPosition()
        {
            return eye;
        }

        public Vector3 getLookAt()
        {
            return lookAt;
        }

        /// <summary>
        /// String de codigo para setear la camara desde GuiController, con la posicion actual y direccion de la camara
        /// </summary>
        internal string getPositionCode()
        {
            //TODO ver de donde carajo sacar el LookAt de esta camara
            Vector3 lookAt = this.LookAt;

            return "GuiController.Instance.setCamera(new Vector3(" +
                TgcParserUtils.printFloat(eye.X) + "f, " + TgcParserUtils.printFloat(eye.Y) + "f, " + TgcParserUtils.printFloat(eye.Z) + "f), new Vector3(" +
                TgcParserUtils.printFloat(lookAt.X) + "f, " + TgcParserUtils.printFloat(lookAt.Y) + "f, " + TgcParserUtils.printFloat(lookAt.Z) + "f));";
        }



    }
}
