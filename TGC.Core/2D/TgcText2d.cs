using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using Font = Microsoft.DirectX.Direct3D.Font;

namespace TGC.Core._2D
{
    public class TgcText2D
    {
        /// <summary>
        ///     Alternativas de alineaci�n del texto
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

            var viewport = D3DDevice.Instance.Device.Viewport;
            rectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
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
        ///     Tama�o maximo del recuadro del texto
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
        ///     Alineaci�n del texto
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
        ///     Dibujar un texto en la posici�n indicada, con el color indicado.
        ///     Utilizar la fuente default del Framework.
        /// </summary>
        /// <param name="text">Texto a dibujar</param>
        /// <param name="x">Posici�n X de la pantalla</param>
        /// <param name="y">Posici�n Y de la pantalla</param>
        /// <param name="color">Color del texto</param>
        public void drawText(string text, int x, int y, Color color)
        {
            TextSprite.Begin(SpriteFlags.AlphaBlend);
            D3dFont.DrawText(TextSprite, text, x, y, color);
            TextSprite.End();
        }

        public void dispose()
        {
            D3dFont.Dispose();
        }
    }
}