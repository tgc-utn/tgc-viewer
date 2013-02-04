using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using Microsoft.DirectX;
using System.Drawing;

namespace TgcViewer.Utils._2D
{
    /// <summary>
    /// Herramienta para dibujar Sprites 2D
    /// </summary>
    public class TgcDrawer2D
    {
        Device d3dDevice;
        Microsoft.DirectX.Direct3D.Sprite dxSprite;

        public TgcDrawer2D()
        {
            this.d3dDevice = GuiController.Instance.D3dDevice;
            dxSprite = new Microsoft.DirectX.Direct3D.Sprite(d3dDevice);
        }

        /// <summary>
        /// Iniciar render de Sprites
        /// </summary>
        public void beginDrawSprite()
        {
            dxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
        }

        /// <summary>
        /// Finalizar render de Sprites
        /// </summary>
        public void endDrawSprite()
        {
            dxSprite.End();
        }

        /// <summary>
        /// Renderizar Sprite
        /// </summary>
        /// <param name="sprite">Sprite a dibujar</param>
        public void drawSprite(TgcSprite sprite)
        {
            dxSprite.Transform = sprite.TransformationMatrix;
            dxSprite.Draw(sprite.Texture.D3dTexture, sprite.SrcRect, Vector3.Empty, Vector3.Empty, sprite.Color);
        }

    }

}
