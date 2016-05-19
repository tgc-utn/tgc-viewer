using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Core._2D
{
    /// <summary>
    ///     Herramienta para dibujar Sprites 2D
    /// </summary>
    public class TgcDrawer2D
    {
        private readonly Sprite dxSprite;

        /// <summary>
        ///     Constructor privado para poder hacer el singleton
        /// </summary>
        private TgcDrawer2D()
        {
            dxSprite = new Sprite(D3DDevice.Instance.Device);
        }

        public static TgcDrawer2D Instance { get; } = new TgcDrawer2D();

        /// <summary>
        ///     Iniciar Render de Sprites
        /// </summary>
        public void beginDrawSprite()
        {
            dxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortDepthFrontToBack);
        }

        /// <summary>
        ///     Finalizar Render de Sprites
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