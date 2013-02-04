using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace Examples.Engine2D
{
    public class Bitmap : IDisposable
    {        
        #region Public members

        Texture texture;

        /// <summary>
        /// returns the underlying texture.
        /// </summary>
        public Texture Texture
        {
            get { return texture; }

        }

        ImageInformation imageInformation;

        /// <summary>
        /// Returns the image information of the bitmap.
        /// </summary>
        public ImageInformation ImageInformation
        {
            get { return imageInformation; }
            set { imageInformation = value; }
        }
            
        #endregion

        public Bitmap(string filePath, Device d3dDevice)
        {
            try
            {

                texture = TextureLoader.FromFile(d3dDevice, filePath, 0, 0, 0, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, Filter.Linear, Filter.Linear, Color.Magenta.ToArgb(), ref imageInformation);

            }
            catch
            {
                throw new Exception(string.Format("Error at loading texture: {0}", filePath));
            }
        }

        #region Miembros de IDisposable

        void IDisposable.Dispose()
        {
            if (texture != null)
            {
                texture.Dispose();
            } 
        }

        #endregion
    }
}
