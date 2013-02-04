using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using System.Drawing;

namespace TgcViewer.Utils
{
    /// <summary>
    /// Herramienta para dibujar texto genérico del Framework
    /// </summary>
    public class TgcDrawText
    {
        /// <summary>
        /// Fuente default del framework
        /// </summary>
        public static readonly System.Drawing.Font VERDANA_10 = new System.Drawing.Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Pixel);

        Microsoft.DirectX.Direct3D.Font dxFont;

        Sprite textSprite;
        /// <summary>
        /// Sprite para renderizar texto
        /// </summary>
        public Sprite TextSprite
        {
            get { return textSprite; }
        }

        public TgcDrawText(Device d3dDevice)
        {
            textSprite = new Sprite(d3dDevice);

            //Fuente default
            dxFont = new Microsoft.DirectX.Direct3D.Font(d3dDevice, VERDANA_10);
        }

        /// <summary>
        /// Dibujar un texto en la posición indicada, con el color indicado. 
        /// Utilizar la fuente default del Framework.
        /// </summary>
        /// <param name="text">Texto a dibujar</param>
        /// <param name="x">Posición X de la pantalla</param>
        /// <param name="y">Posición Y de la pantalla</param>
        /// <param name="color">Color del texto</param>
        public void drawText(string text, int x, int y, Color color)
        {
            textSprite.Begin(SpriteFlags.AlphaBlend);
            dxFont.DrawText(textSprite, text, x, y, color);
            textSprite.End();
        }

    }
}
