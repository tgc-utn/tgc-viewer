using Microsoft.DirectX;
using System;
using TGC.Core.Mathematica;
using TGC.Examples.Engine2D.Spaceship.Core;

namespace TGC.Examples.Engine2D.Spaceship
{
    public class Misil : GameObject
    {
        private static CustomBitmap misilBitmap;

        public float Angle;

        public TGCVector2 Position;

        private CustomSprite sprite;

        public void Load(CustomBitmap spriteBitmap)
        {
            misilBitmap = spriteBitmap;

            sprite = new CustomSprite();
            sprite.Bitmap = misilBitmap;
        }

        public override void Update(float ElapsedTime)
        {
            float speed = 500;

            Position.X += speed * ElapsedTime * (float)Math.Cos(Angle);
            Position.Y += speed * ElapsedTime * (float)Math.Sin(Angle);

            sprite.Position = Position;
            sprite.Rotation = Angle;
        }

        public override void Render(float ElapsedTime, Drawer2D drawer)
        {
            drawer.DrawSprite(sprite);
        }
    }
}