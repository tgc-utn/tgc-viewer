using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils._2D
{
    /// <summary>
    ///     Herramienta para dibujar Sprites 2D
    /// </summary>
    public class TgcDrawer2D
    {
        private readonly Device d3dDevice;
        private readonly Sprite dxSprite;

        public TgcDrawer2D()
        {
            d3dDevice = GuiController.Instance.D3dDevice;
            dxSprite = new Sprite(d3dDevice);
        }

        /// <summary>
        ///     Iniciar render de Sprites
        /// </summary>
        public void beginDrawSprite()
        {
            dxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
        }

        /// <summary>
        ///     Finalizar render de Sprites
        /// </summary>
        public void endDrawSprite()
        {
            dxSprite.End();
        }

        /// <summary>
        ///     Renderizar Sprite
        /// </summary>
        /// <param name="sprite">Sprite a dibujar</param>
        public void drawSprite(TgcSprite sprite)
        {
            dxSprite.Transform = sprite.TransformationMatrix;
            dxSprite.Draw(sprite.Texture.D3dTexture, sprite.SrcRect, Vector3.Empty, Vector3.Empty, sprite.Color);
        }
    }
}