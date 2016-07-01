using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Examples.Engine2D.Core;

namespace TGC.Examples.Engine2D
{
    public class Asteroide : GameObject
    {
        private const int SpriteWidth = 64;
        private const int SpriteHeight = 64;

        public static int Size = 64;
        private float angle;

        //El bitmap del spritesheet
        private CustomBitmap asteroidBitmap;

        //El indice el sprite actual.
        private int currentSprite;

        //La posicion
        public Vector2 Position;

        private float size;

        //La lista de sprites.
        private List<CustomSprite> sprites;

        private Vector2 spriteSize;

        public void Load(string exampleDir, CustomBitmap spriteSheet)
        {
            asteroidBitmap = spriteSheet;

            sprites = new List<CustomSprite>();

            spriteSize = new Vector2(SpriteWidth, SpriteHeight);
            size = 1.0f;
            angle = 0.0f;

            CustomSprite newSprite;
            //Creo 64 sprites asignando distintos clipping rects a cada uno.
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    newSprite = new CustomSprite();
                    newSprite.Bitmap = asteroidBitmap;
                    newSprite.SrcRect = new Rectangle(j * (int)spriteSize.X, i * (int)spriteSize.Y, (int)spriteSize.X,
                        (int)spriteSize.Y);
                    newSprite.Scaling = new Vector2(size, size);
                    newSprite.Rotation = angle;
                    sprites.Add(newSprite);
                }
            }

            currentSprite = 0;

            GenerateRandomPosition();
        }

        public override void Update(float ElapsedTime)
        {
            //Chequeo si se escapa de la pantalla.
            if (Position.X < -SpriteWidth || Position.Y < -SpriteHeight * 2 ||
                Position.X > GameManager.ScreenWidth + SpriteWidth
                || Position.Y > GameManager.ScreenHeight + SpriteHeight)
            {
                GenerateRandomPosition();
            }

            float speed = 250;

            Position.X += speed * ElapsedTime * (float)Math.Cos(angle);
            Position.Y += speed * ElapsedTime * (float)Math.Sin(angle);

            currentSprite++;
            if (currentSprite > 63)
                currentSprite = 0;

            sprites[currentSprite].Position = Position;
        }

        //Genero posicion aleatoria para el asteoride.
        public void GenerateRandomPosition()
        {
            var rnd = new Random();

            //Determina de que lado de la pantalla aparece
            var lado = (int)(rnd.NextDouble() * 2);
            if (lado == 0)
                Position.X = 0;
            else
                Position.X = GameManager.ScreenWidth;

            Position.Y = GameManager.ScreenHeight * (float)rnd.NextDouble();

            //Busco el angulo del asteroide para que vaya al centro de la pantalla.
            var ScreenCenterVector = new Vector2();
            var ScreenCenter = new Vector2(GameManager.ScreenWidth / 2, GameManager.ScreenHeight / 2);
            ScreenCenterVector = Vector2.Subtract(ScreenCenter, Position);

            if (ScreenCenterVector.Length() > 0)
                angle = (float)Math.Atan2(ScreenCenterVector.Y, ScreenCenterVector.X);
        }

        public override void Render(float elapsedTime, Drawer2D drawer)
        {
            drawer.DrawSprite(sprites[currentSprite]);
        }
    }
}