using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer;

namespace Examples.Engine2D
{
    /// <summary>
    /// Draws sprites and primitives.
    /// </summary>
    public class Drawer
    {
        Device d3dDevice;
        Microsoft.DirectX.Direct3D.Sprite DxSprite;
        Line line;

        CustomVertex.PositionColoredTextured[] LineVertexData = new CustomVertex.PositionColoredTextured[2];

        public Drawer()
        {
            this.d3dDevice = GuiController.Instance.D3dDevice;
            DxSprite = new Microsoft.DirectX.Direct3D.Sprite(d3dDevice);
            line = new Line(d3dDevice);
        }

        /// <summary>
        /// Call this method before drawing.
        /// </summary>
        public void BeginDrawSprite()
        {
            DxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
        }

        /// <summary>
        /// Call this method after drawing.
        /// </summary>
        public void EndDrawSprite()
        {
            DxSprite.End();
        }

        /// <summary>
        /// Draws a sprite on the screen.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        public void DrawSprite(Sprite sprite)
        {
            DxSprite.Transform = sprite.TransformationMatrix;
            DxSprite.Draw(sprite.Bitmap.Texture, sprite.SrcRect, Vector3.Empty, Vector3.Empty, sprite.Color);

        }

        /// <summary>
        /// Draws a colored point.
        /// </summary>
        /// <param name="position">The location.</param>
        /// <param name="color">The color.</param>
        public void DrawPoint(Vector2 position, Color color)
        {
            LineVertexData[0].X = position.X;
            LineVertexData[0].Y = position.Y;
            LineVertexData[0].Color = color.ToArgb();

            d3dDevice.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            d3dDevice.DrawUserPrimitives(PrimitiveType.PointList, 1, LineVertexData);
        }

        /// <summary>
        /// Draws a line segment between two points.
        /// </summary>
        /// <param name="position1">The first point.</param>
        /// <param name="position2">The second point.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width</param>
        /// <param name="antiAlias">Anti-alias enabled.</param>
        public void DrawLine(Vector2 position1, Vector2 position2, Color color, int width, bool antiAlias)
        {
            Vector2[] positionList = new Vector2[2] { position1, position2 };
            DrawPolyline(positionList, color, width, antiAlias);
        }

        /// <summary>
        /// Draws a polyline.
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
