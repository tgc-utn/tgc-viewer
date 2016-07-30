using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;

namespace TGC.Examples.Engine2D.Spaceship.Core
{
    public class CustomBitmap : IDisposable
    {
        public CustomBitmap(string filePath, Device d3dDevice)
        {
            try
            {
                D3dTexture = TextureLoader.FromFile(d3dDevice, filePath, 0, 0, 0, Usage.RenderTarget, Format.A8R8G8B8,
                    Pool.Default, Filter.Linear, Filter.Linear, Color.Magenta.ToArgb(), ref imageInformation);
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", filePath));
            }
        }

        #region Miembros de IDisposable

        public void Dispose()
        {
            if (D3dTexture != null)
            {
                D3dTexture.Dispose();
            }
        }

        #endregion Miembros de IDisposable

        #region Public members

        /// <summary>
        ///     returns the underlying texture.
        /// </summary>
        public Texture D3dTexture { get; }

        /// <summary>
        ///     Ancho de la textura
        /// </summary>
        public int Width
        {
            get { return D3dTexture.GetLevelDescription(0).Width; }
        }

        /// <summary>
        ///     Alto de la textura
        /// </summary>
        public int Height
        {
            get { return D3dTexture.GetLevelDescription(0).Height; }
        }

        /// <summary>
        ///     Dimensiones de la textura
        /// </summary>
        public Size Size
        {
            get { return new Size(Width, Height); }
        }

        private ImageInformation imageInformation;

        /// <summary>
        ///     Returns the image information of the bitmap.
        /// </summary>
        public ImageInformation ImageInformation
        {
            get { return imageInformation; }
            set { imageInformation = value; }
        }

        #endregion Public members
    }
}