using System;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using Font = Microsoft.DirectX.Direct3D.Font;

namespace TGC.Core.Text
{
    public class TgcText2D : IDisposable
    {
        /// <summary>
        ///     Alternativas de alineación del texto
        /// </summary>
        public enum TextAlign
        {
            LEFT,
            RIGHT,
            CENTER
        }

        private TextAlign align;

        //font default.
        public static readonly System.Drawing.Font VERDANA_10 = new System.Drawing.Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Pixel);

        private Rectangle rectangle;
        private Sprite TextSprite { get; set; }

        public TgcText2D()
        {
            TextSprite = new Sprite(D3DDevice.Instance.Device);
            changeTextAlign(TextAlign.CENTER);
            changeFont(VERDANA_10);
            Color = Color.Black;

            //var viewport = D3DDevice.Instance.Device.Viewport;
            rectangle = new Rectangle(0, 0, D3DDevice.Instance.Width, D3DDevice.Instance.Height);
        }

        /// <summary>
        ///     Fuente de Direct3D para la letra del texto
        /// </summary>
        public Font D3dFont { get; private set; }

        /// <summary>
        ///     Color del texto
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Posicion del texto
        /// </summary>
        public Point Position
        {
            get { return rectangle.Location; }
            set { rectangle.Location = value; }
        }

        /// <summary>
        ///     Tamaño maximo del recuadro del texto
        /// </summary>
        public Size Size
        {
            get { return rectangle.Size; }
            set { rectangle.Size = value; }
        }

        /// <summary>
        ///     Texto a renderizar
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Formato de renderizado del texto.
        ///     Si se cambia a mano el formato, no se respeta la alineacion del texto que se haya configurado.
        /// </summary>
        public DrawTextFormat Format { get; set; }

        /// <summary>
        ///     Alineación del texto
        /// </summary>
        public TextAlign Align
        {
            get { return align; }
            set { changeTextAlign(value); }
        }

        public void render()
        {
            TextSprite.Begin(SpriteFlags.AlphaBlend);
            D3dFont.DrawText(TextSprite, Text, rectangle, Format, Color);
            TextSprite.End();
        }

        /// <summary>
        ///     Cambia la fuente del texto
        /// </summary>
        /// <param name="font">Fuente del sistema</param>
        public void changeFont(System.Drawing.Font font)
        {
            if (D3dFont != null && !D3dFont.Disposed)
                D3dFont.Dispose();
            D3dFont = new Font(D3DDevice.Instance.Device, font);
        }

        /// <summary>
        ///     Cambiar TextAlign y configurar DrawTextFormat
        /// </summary>
        private void changeTextAlign(TextAlign align)
        {
            this.align = align;
            var fAlign = DrawTextFormat.None;
            switch (align)
            {
                case TextAlign.LEFT:
                    fAlign = DrawTextFormat.Left;
                    break;

                case TextAlign.RIGHT:
                    fAlign = DrawTextFormat.Right;
                    break;

                case TextAlign.CENTER:
                    fAlign = DrawTextFormat.Center;
                    break;
            }
            Format = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs | DrawTextFormat.WordBreak | fAlign;
        }

        /// <summary>
        ///     Dibujar un texto en la posición indicada, con el color indicado.
        ///     Utilizar la fuente default del Framework.
        /// </summary>
        /// <param name="text">Texto a dibujar</param>
        /// <param name="x">Posición X de la pantalla</param>
        /// <param name="y">Posición Y de la pantalla</param>
        /// <param name="color">Color del texto</param>
        public void drawText(string text, int x, int y, Color color)
        {
            TextSprite.Begin(SpriteFlags.AlphaBlend);
            D3dFont.DrawText(TextSprite, text, x, y, color);
            TextSprite.End();
        }

        public void Dispose()
        {
            D3dFont.Dispose();
        }
    }
}