using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer;
using Microsoft.DirectX;

namespace Examples.Engine2D
{
    public class Misil : GameObject
    {
        static Bitmap misilBitmap;
        Sprite sprite;

        public Vector2 Position;

        public float Angle;

        float speed;

        public Misil()
        {
            
        }

        public void Load(string exampleDir, Bitmap spriteBitmap)
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
