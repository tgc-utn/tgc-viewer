using System.Drawing;
using TGC.Core.Direct3D;
using Directx = Microsoft.DirectX.Direct3D;

namespace TGC.Core._2D
{
    /// <summary>
    ///     Herramienta para dibujar texto genérico del Framework
    /// </summary>
    public class TgcDrawText
    {
        /// <summary>
        ///     Fuente default del framework
        /// </summary>
        public static readonly Font VERDANA_10 = new Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Pixel);

        private readonly Directx.Font dxFont;

        public TgcDrawText()
        {
            TextSprite = new Directx.Sprite(D3DDevice.Instance.Device);

            //Fuente default
            dxFont = new Directx.Font(D3DDevice.Instance.Device, VERDANA_10);
        }

        /// <summary>
        ///     Sprite para renderizar texto
        /// </summary>
        public Directx.Sprite TextSprite { get; }

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
            TextSprite.Begin(Directx.SpriteFlags.AlphaBlend);
            dxFont.DrawText(TextSprite, text, x, y, color);
            TextSprite.End();
        }
    }
}