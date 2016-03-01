using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using Font = Microsoft.DirectX.Direct3D.Font;

namespace TGC.Viewer.Utils._2D
{
    public class TgcText2d
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

        private Rectangle rectangle;

        public TgcText2d()
        {
            changeTextAlign(TextAlign.CENTER);
            changeFont(TgcDrawText.VERDANA_10);
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
            var sprite = GuiController.Instance.Text3d.TextSprite;
            sprite.Begin(SpriteFlags.AlphaBlend);
            D3dFont.DrawText(sprite, Text, rectangle, Format, Color);
            sprite.End();
        }

        /// <summary>
        ///     Cambia la fuente del texto
        /// </summary>
        /// <param name="font">Fuente del sistema</param>
        public void changeFont(System.Drawing.Font font)
        {
            var sprite = GuiController.Instance.Text3d.TextSprite;
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

        public void dispose()
        {
            //TODO: No se por que pero esto da error al hacer resize de la pantalla
            //d3dFont.Dispose();
        }
    }
}