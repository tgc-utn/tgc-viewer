using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Font = System.Drawing.Font;

namespace TGC.Viewer.Utils
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

        private readonly Microsoft.DirectX.Direct3D.Font dxFont;

        public TgcDrawText(Device d3dDevice)
        {
            TextSprite = new Sprite(d3dDevice);

            //Fuente default
            dxFont = new Microsoft.DirectX.Direct3D.Font(d3dDevice, VERDANA_10);
        }

        /// <summary>
        ///     Sprite para renderizar texto
        /// </summary>
        public Sprite TextSprite { get; }

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
            dxFont.DrawText(TextSprite, text, x, y, color);
            TextSprite.End();
        }
    }
}