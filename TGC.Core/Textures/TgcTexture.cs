using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.IO;
using TGC.Core.Direct3D;

namespace TGC.Core.Textures
{
    /// <summary>
    ///     Encapsula una textura de DirectX junto con información adicional
    /// </summary>
    public class TgcTexture
    {
        /// <summary>
        ///     Crear textura de TGC
        /// </summary>
        public TgcTexture(string fileName, string filePath, Texture d3dTexture, bool inPool)
        {
            FileName = fileName;
            FilePath = filePath;
            D3dTexture = d3dTexture;
            InPool = inPool;
        }

        /// <summary>
        ///     Nombre del archivo de la textura. Ejemplo: miTextura.jpg
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Ruta de la textura. Ejemplo: C:\Texturas\miTextura.jpg
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Textura de DirectX
        /// </summary>
        public Texture D3dTexture { get; private set; }

        /// <summary>
        ///     Indica si la textura fue creada dentro del TexturesPool de texturas del framework
        /// </summary>
        public bool InPool { get; }

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

        /// <summary>
        ///     Calcula el Aspect Ratio de la imagen
        /// </summary>
        /// <returns>Aspect Ratio</returns>
        public float getAspectRatio()
        {
            return (float)Width / Height;
        }

        /// <summary>
        ///     Libera los recursos de la textura
        /// </summary>
        public void dispose()
        {
            //dispose de textura dentro de pool
            if (InPool)
            {
                TexturesPool.Instance.disposeTexture(FilePath);

                /*TODO creo que esto no hace falta, lo hace solo DirectX
                //Si hubo un dispose fisico, quitar del TexturesManager
                if (disposed)
                {
                    GuiController.Instance.TexturesManager.clearDisposedTexture(this);
                }
                */
            }

            //dispose de textura fuera de pool
            else
            {
                if (D3dTexture != null && !D3dTexture.Disposed)
                {
                    D3dTexture.Dispose();
                    D3dTexture = null;
                }
            }
        }

        public override string ToString()
        {
            return FileName;
        }

        /// <summary>
        ///     Crear una nueva textura igual a esta.
        /// </summary>
        /// <returns>Textura clonada</returns>
        public TgcTexture clone()
        {
            TgcTexture cloneTexture;
            if (InPool)
            {
                cloneTexture = createTexture(FilePath);
            }
            else
            {
                cloneTexture = createTextureNoPool(FilePath);
            }
            return cloneTexture;
        }

        #region Creacion Static

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Se utiliza un TexturesPool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="fileName">Nombre de la textura. Ejemplo: miTextura.jpg</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                var d3dTexture = TexturesPool.Instance.createTexture(d3dDevice, filePath);
                return new TgcTexture(fileName, filePath, d3dTexture, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     Se utiliza un TexturesPool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(Device d3dDevice, string filePath)
        {
            var fInfo = new FileInfo(filePath);
            return createTexture(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     Se utiliza un TexturesPool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(string filePath)
        {
            return createTexture(D3DDevice.Instance.Device, filePath);
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Se utiliza un TexturesPool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="fileName">Nombre de la textura. Ejemplo: miTextura.jpg</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <param name="d3dTexture">Textura de DirectX ya cargada por el usuario</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(Device d3dDevice, string fileName, string filePath, Texture d3dTexture)
        {
            try
            {
                filePath = filePath.Replace("\\\\", "\\");
                var d3dTexture2 = TexturesPool.Instance.createTexture(d3dDevice, filePath, d3dTexture);
                return new TgcTexture(fileName, filePath, d3dTexture2, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     No se utiliza TexturesPool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="fileName">Nombre de la textura. Ejemplo: miTextura.jpg</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                var d3dTexture = TextureLoader.FromFile(d3dDevice, filePath);
                return new TgcTexture(fileName, filePath, d3dTexture, false);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     No se utiliza TexturesPool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(Device d3dDevice, string filePath)
        {
            var fInfo = new FileInfo(filePath);
            return createTextureNoPool(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     No se utiliza TexturesPool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(string filePath)
        {
            return createTextureNoPool(D3DDevice.Instance.Device, filePath);
        }

        #endregion Creacion Static
    }
}