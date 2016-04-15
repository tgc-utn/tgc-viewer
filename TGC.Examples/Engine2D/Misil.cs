using Microsoft.DirectX;
using System;

namespace TGC.Examples.Engine2D
{
    public class Misil : GameObject
    {
        private static Bitmap misilBitmap;

        public float Angle;

        public Vector2 Position;

        private float speed;
        private Sprite sprite;

        public void Load(Bitmap spriteBitmap)
        {
            misilBitmap = spriteBitmap;

            sprite = new Sprite();
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

        public override void Render(float ElapsedTime, Drawer drawer)
        {
            drawer.DrawSprite(sprite);
        }
    }
}