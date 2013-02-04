using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer;
using System.Drawing;
using Microsoft.DirectX;

namespace Examples.Engine2D
{
    class Spaceship : GameObject
    {
        
        enum StateEnum
        {
            Idle,
            Moving
        }

        //El estado de la nave.
        StateEnum state;

        //El numero de sprite dentro del spritesheet
        int currentSprite;

        //La posicion de la nave.
        public Vector2 Position;
        
        //La velocidad de la nave.
        Vector2 speed;

        //La posicion del centro del sprite
        Vector2 centerPosition;

        //El angulo al que apunta la nave.
        float angle;
        
        //El factor de escalado.
        float size;

       
        public Vector2 spriteSize;
        
        //El bitmap del spritesheet.
        Bitmap spaceshipBitmap;

        //Los distintos sprites de animacion.
        List<Sprite> sprites;

        //El angulo al mouse.
        float angleToMousePointer;
        
        //Cuanto tiempo paso desde que se disparo el ultimo misil.
        float misilRateCounter;

        float elapsedTime;

        int contadorAnimacion;
        
        public void Load(string exampleDir, Bitmap bitmap)
        {
            spaceshipBitmap = bitmap;

            sprites = new List<Sprite>();

            spriteSize = new Vector2(41, 44);
            size = 2.0f;
            
            
            Sprite newSprite;
            for (int i = 0; i < 3; i++)
            {
                newSprite = new Sprite();
                newSprite.Bitmap = spaceshipBitmap;
                newSprite.SrcRect = new Rectangle(i * (int)spriteSize.X, 0, (int)spriteSize.X, (int)spriteSize.Y);
                newSprite.Scaling = new Vector2(size, size);
                sprites.Add(newSprite);
            }

            currentSprite = 0;
            state = StateEnum.Idle;
            
            Position = new Vector2(100, 100);
            speed = new Vector2(0, 0);

            angleToMousePointer = 0;


            RestartPosition();

            GuiController.Instance.UserVars.addVar("elapsed");
            GuiController.Instance.UserVars.addVar("speedX");
            GuiController.Instance.UserVars.addVar("speedY");

            GuiController.Instance.UserVars.addVar("PosX");
            GuiController.Instance.UserVars.addVar("PosY");

            GuiController.Instance.UserVars.addVar("MousePosX");
            GuiController.Instance.UserVars.addVar("MousePosY");

            GuiController.Instance.UserVars.addVar("AngleMouse");

            GuiController.Instance.UserVars.addVar("Misiles");
         }

        private void fireMissile()
        {
            //Si paso el tiempo suficiente
            if (misilRateCounter > 0.10f)
             {
                 GameManager.Instance.fireMissile(centerPosition,angle - (float)Math.PI / 2.0f);
                 misilRateCounter = 0;

             }
             
        }

        public void RestartPosition()
        {
            Position.X = GameManager.ScreenWidth / 2;
            Position.Y = GameManager.ScreenHeight / 2;
        }

        public override void Update(float elapsedTime)
        {
            this.elapsedTime = elapsedTime;
            
            float dirX = 0;
            float dirY = 0;

            state = StateEnum.Idle;
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                dirX = -1;
                state = StateEnum.Moving;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                dirX = 1;
                state = StateEnum.Moving;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.W))
            {
                dirY = -1;
                state = StateEnum.Moving;
            }
            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.S))
            {
                dirY = 1;
                state = StateEnum.Moving;
            }


            if (GuiController.Instance.D3dInput.buttonDown(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                fireMissile();
            }

            if (state == StateEnum.Idle)
            {
                currentSprite = 0;
            }
            else
            {
                currentSprite = contadorAnimacion;

                if (contadorAnimacion > 1)
                {
                    contadorAnimacion = 1;
                }
                else
                {
                    contadorAnimacion++;
                }
                
                
            }

            //Las constantes
            const float maxSpeed = 400.0f;
            const float acceleration = 300.0f;
            const float deacceleration = 300.0f;
            const float Epsilon = 0.2f;

            Vector2 spriteMouseVector = new Vector2();
            Vector2 mouseVector = new Vector2(GuiController.Instance.D3dInput.Xpos, GuiController.Instance.D3dInput.Ypos);
            spriteMouseVector = Vector2.Subtract(mouseVector, Position + new Vector2((spriteSize.X / 2) * size, (spriteSize.Y / 2) * size));

            if (spriteMouseVector.Length() > 10f)
                angleToMousePointer = (float)Math.Atan2(spriteMouseVector.Y, spriteMouseVector.X);


            float MouseXAngle = (float)Math.Cos(angleToMousePointer);
            float MouseYAngle = (float)Math.Sin(angleToMousePointer);


            angle = angleToMousePointer + (float)Math.PI / 2.0f;

            if (dirY == 0)
            {

                speed.X -= Math.Sign(speed.X) * deacceleration * elapsedTime;
                speed.Y -= Math.Sign(speed.Y) * deacceleration * elapsedTime;
            }

            if (dirY == -1)
            {
                speed.X += acceleration * MouseXAngle * elapsedTime;
                speed.Y += acceleration * MouseYAngle * elapsedTime;
            }


            //Limitar la velocidad
            if (speed.Length() > maxSpeed)
            {
                speed.X = maxSpeed * MouseXAngle;
                speed.Y = maxSpeed * MouseYAngle;
            }


            Position.X += speed.X * elapsedTime;
            Position.Y += speed.Y * elapsedTime;

            centerPosition = Position + (spriteSize * 0.5f * size);

            sprites[currentSprite].Position = Position;
            sprites[currentSprite].Rotation = angle;

            sprites[currentSprite].RotationCenter = new Vector2(spriteSize.X / 2 * size, spriteSize.Y / 2 * size);



            misilRateCounter += elapsedTime;

            if (Position.X > GameManager.ScreenWidth +spriteSize.X)
                Position.X = -spriteSize.X;

            if (Position.X < -spriteSize.X)
                Position.X = GameManager.ScreenWidth + spriteSize.X;

            if (Position.Y > GameManager.ScreenHeight + spriteSize.Y)
                Position.Y = -spriteSize.Y;

            if (Position.Y < -spriteSize.Y)
                Position.Y = GameManager.ScreenHeight + spriteSize.Y;

            GuiController.Instance.UserVars.setValue("elapsed", elapsedTime);
            GuiController.Instance.UserVars.setValue("speedX", speed.X);
            GuiController.Instance.UserVars.setValue("speedY", speed.Y);

            GuiController.Instance.UserVars.setValue("PosX", Position.X);
            GuiController.Instance.UserVars.setValue("PosY", Position.Y);
            GuiController.Instance.UserVars.setValue("MousePosX", GuiController.Instance.D3dInput.Xpos);
            GuiController.Instance.UserVars.setValue("MousePosY", GuiController.Instance.D3dInput.Ypos);
            GuiController.Instance.UserVars.setValue("AngleMouse", (angleToMousePointer * 360) / (2 * Math.PI));

        }

        public override void Render(float elapsedTime, Drawer drawer)
        {
            drawer.DrawSprite(sprites[currentSprite]);

        }
    }
}
