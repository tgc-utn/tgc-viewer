using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using Font = System.Drawing.Font;

namespace TGC.Core.Text
{
    public class TGCText2D : IDisposable
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

        //font default.
        public static readonly Font VERDANA_10 = new Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Pixel);

        private TextAlign align;

        private Rectangle rectangle;

        public TGCText2D()
        {
            TextSprite = new Sprite(D3DDevice.Instance.Device);
            changeTextAlign(TextAlign.CENTER);
            changeFont(VERDANA_10);
            Color = Color.Black;

            //var viewport = D3DDevice.Instance.Device.Viewport;
            rectangle = new Rectangle(0, 0, D3DDevice.Instance.Width, D3DDevice.Instance.Height);
        }

        private Sprite TextSprite { get; }

        /// <summary>
        ///     Fuente de Direct3D para la letra del texto
        /// </summary>
        public Microsoft.DirectX.Direct3D.Font D3dFont { get; private set; }

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

        public void Dispose()
        {
            D3dFont.Dispose();
        }

        public void Render()
        {
            TextSprite.Begin(SpriteFlags.AlphaBlend);
            D3dFont.DrawText(TextSprite, Text, rectangle, Format, Color);
            TextSprite.End();
        }

        /// <summary>
        ///     Cambia la fuente del texto
        /// </summary>
        /// <param name="font">Fuente del sistema</param>
        public void changeFont(Font font)
        {
            if (D3dFont != null && !D3dFont.Disposed)
                D3dFont.Dispose();
            D3dFont = new Microsoft.DirectX.Direct3D.Font(D3DDevice.Instance.Device, font);
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
    }
}