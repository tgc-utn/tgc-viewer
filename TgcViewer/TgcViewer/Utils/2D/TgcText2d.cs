using System;
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;

namespace TgcViewer.Utils._2D
{
    public class TgcText2d
    {
        
        Microsoft.DirectX.Direct3D.Font d3dFont;
        /// <summary>
        /// Fuente de Direct3D para la letra del texto
        /// </summary>
        public Microsoft.DirectX.Direct3D.Font D3dFont
        {
            get { return d3dFont; }
        }

        private Color color;
        /// <summary>
        /// Color del texto
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private Rectangle rectangle;
        /// <summary>
        /// Posicion del texto
        /// </summary>
        public Point Position
        {
            get { return rectangle.Location; }
            set { rectangle.Location = value; }
        }
        /// <summary>
        /// Tamaño maximo del recuadro del texto
        /// </summary>
        public Size Size
        {
            get { return rectangle.Size; }
            set { rectangle.Size = value; }
        }

        private string text;
        /// <summary>
        /// Texto a renderizar
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private DrawTextFormat format;
        /// <summary>
        /// Formato de renderizado del texto.
        /// Si se cambia a mano el formato, no se respeta la alineacion del texto que se haya configurado.
        /// </summary>
        public DrawTextFormat Format
        {
            get { return format; }
            set { format = value; }
        }

        /// <summary>
        /// Alternativas de alineación del texto
        /// </summary>
        public enum TextAlign
        {
            LEFT,
            RIGHT,
            CENTER
        }
        private TextAlign align;
        /// <summary>
        /// Alineación del texto
        /// </summary>
        public TextAlign Align
        {
            get { return align; }
            set { changeTextAlign(value); }
        }

        

        public TgcText2d()
        {
            changeTextAlign(TextAlign.CENTER);
            changeFont(TgcDrawText.VERDANA_10);
            color = Color.Black;

            Viewport viewport = GuiController.Instance.D3dDevice.Viewport;
            rectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
        }

        public void render()
        {
            Sprite sprite = GuiController.Instance.Text3d.TextSprite;
            sprite.Begin(SpriteFlags.AlphaBlend);
            d3dFont.DrawText(sprite, text, rectangle, format, color);
            sprite.End();
        }

        /// <summary>
        /// Cambia la fuente del texto
        /// </summary>
        /// <param name="font">Fuente del sistema</param>
        public void changeFont(System.Drawing.Font font)
        {
            Sprite sprite = GuiController.Instance.Text3d.TextSprite;
            d3dFont = new Microsoft.DirectX.Direct3D.Font(GuiController.Instance.D3dDevice, font);
        }

        /// <summary>
        /// Cambiar TextAlign y configurar DrawTextFormat
        /// </summary>
        private void changeTextAlign(TextAlign align)
        {
            this.align = align;
            DrawTextFormat fAlign = DrawTextFormat.None;
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
            format = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs | DrawTextFormat.WordBreak | fAlign;
        }

        public void dispose()
        {
            //TODO: No se por que pero esto da error al hacer resize de la pantalla
            //d3dFont.Dispose();
        }
        

    }
}
