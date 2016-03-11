using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;

namespace TGC.Examples.Engine2D
{
    public class Bitmap : IDisposable
    {
        public Bitmap(string filePath, Device d3dDevice)
        {
            try
            {
                Texture = TextureLoader.FromFile(d3dDevice, filePath, 0, 0, 0, Usage.RenderTarget, Format.A8R8G8B8,
                    Pool.Default, Filter.Linear, Filter.Linear, Color.Magenta.ToArgb(), ref imageInformation);
            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", filePath));
            }
        }

        #region Miembros de IDisposable

        void IDisposable.Dispose()
        {
            if (Texture != null)
            {
                Texture.Dispose();
            }
        }

        #endregion Miembros de IDisposable

        #region Public members

        /// <summary>
        ///     returns the underlying texture.
        /// </summary>
        public Texture Texture { get; }

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