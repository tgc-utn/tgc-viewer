using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;

namespace TGC.Examples.Engine2D.Spaceship.Core
{
    /// <summary>
    ///     Draws sprites and primitives.
    /// </summary>
    public class Drawer2D
    {
        private readonly Sprite DxSprite;
        private readonly Line line;

        private readonly CustomVertex.PositionColoredTextured[] LineVertexData =
            new CustomVertex.PositionColoredTextured[2];

        public Drawer2D()
        {
            DxSprite = new Sprite(D3DDevice.Instance.Device);
            line = new Line(D3DDevice.Instance.Device);
        }

        /// <summary>
        ///     Call this method before drawing.
        /// </summary>
        public void BeginDrawSprite()
        {
            DxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
        }

        /// <summary>
        ///     Call this method after drawing.
        /// </summary>
        public void EndDrawSprite()
        {
            DxSprite.End();
        }

        /// <summary>
        ///     Draws a sprite on the screen.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        public void DrawSprite(CustomSprite sprite)
        {
            DxSprite.Transform = sprite.TransformationMatrix.ToMatrix();
            DxSprite.Draw(sprite.Bitmap.D3dTexture, sprite.SrcRect, TGCVector3.Empty.ToVector3(), TGCVector3.Empty.ToVector3(), sprite.Color);
        }

        /// <summary>
        ///     Draws a colored point.
        /// </summary>
        /// <param name="position">The location.</param>
        /// <param name="color">The color.</param>
        public void DrawPoint(Vector2 position, Color color)
        {
            LineVertexData[0].X = position.X;
            LineVertexData[0].Y = position.Y;
            LineVertexData[0].Color = color.ToArgb();

            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.PointList, 1, LineVertexData);
        }

        /// <summary>
        ///     Draws a line segment between two points.
        /// </summary>
        /// <param name="position1">The first point.</param>
        /// <param name="position2">The second point.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width</param>
        /// <param name="antiAlias">Anti-alias enabled.</param>
        public void DrawLine(Vector2 position1, Vector2 position2, Color color, int width, bool antiAlias)
        {
            var positionList = new Vector2[2] { position1, position2 };
            DrawPolyline(positionList, color, width, antiAlias);
        }

        /// <summary>
        ///     Draws a polyline.
        /// </summary>
        /// <param name="positionList">The list of lines.</param>
        /// <param name="color">The color</param>
        /// <param name="width">The width</param>
        /// <param name="antiAlias">Anti-alias enabled.</param>
        public void DrawPolyline(Vector2[] positionList, Color color, int width, bool antiAlias)
        {
            line.Antialias = antiAlias;
            line.Width = width;
            line.Draw(positionList, color);
        }
    }
}