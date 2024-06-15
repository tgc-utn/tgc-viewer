using System;
using System.Drawing;
using System.IO;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Core.Textures
{
    /// <summary>
    ///     Encapsula una textura de DirectX junto con información adicional
    /// </summary>
    public class TGCTexture : IDisposable
    {
        /// <summary>
        ///     Crear textura de TGC
        /// </summary>
        public TGCTexture(string fileName, string filePath, Texture d3DTexture, bool inPool)
        {
            FileName = fileName;
            FilePath = filePath;
            D3DTexture = d3DTexture;
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
        public Texture D3DTexture { get; private set; }

        /// <summary>
        ///     Indica si la textura fue creada dentro del TexturesPool de texturas del framework
        /// </summary>
        public bool InPool { get; }

        /// <summary>
        ///     Ancho de la textura
        /// </summary>
        public int Width => D3DTexture.GetLevelDescription(0).Width;

        /// <summary>
        ///     Alto de la textura
        /// </summary>
        public int Height => D3DTexture.GetLevelDescription(0).Height;

        /// <summary>
        ///     Dimensiones de la textura
        /// </summary>
        public Size Size => new Size(Width, Height);

        /// <summary>
        ///     Libera los recursos de la textura
        /// </summary>
        public void Dispose()
        {
            //Dispose de textura dentro de pool
            if (InPool)
            {
                TexturesPool.Instance.DisposeTexture(FilePath);

                /*TODO creo que esto no hace falta, lo hace solo DirectX
                //Si hubo un dispose fisico, quitar del TexturesManager
                if (disposed)
                {
                    GuiController.Instance.TexturesManager.clearDisposedTexture(this);
                }
                */
            }

            //Dispose de textura fuera de pool
            else
            {
                if (D3DTexture != null && !D3DTexture.Disposed)
                {
                    D3DTexture.Dispose();
                    D3DTexture = null;
                }
            }
        }

        /// <summary>
        ///     Calcula el Aspect Ratio de la imagen
        /// </summary>
        /// <returns>Aspect Ratio</returns>
        public float GetAspectRatio()
        {
            return (float)Width / Height;
        }

        public override string ToString()
        {
            return FileName;
        }

        /// <summary>
        ///     Crear una nueva textura igual a esta.
        /// </summary>
        /// <returns>Textura clonada</returns>
        public TGCTexture Clone()
        {
            TGCTexture cloneTexture;
            if (InPool)
            {
                cloneTexture = CreateTexture(FilePath);
            }
            else
            {
                cloneTexture = CreateTextureNoPool(FilePath);
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
        public static TGCTexture CreateTexture(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                var d3dTexture = TexturesPool.Instance.CreateTexture(d3dDevice, filePath);
                return new TGCTexture(fileName, filePath, d3dTexture, true);
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
        public static TGCTexture CreateTexture(Device d3dDevice, string filePath)
        {
            var fInfo = new FileInfo(filePath);
            return CreateTexture(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     Se utiliza un TexturesPool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TGCTexture CreateTexture(string filePath)
        {
            return CreateTexture(D3DDevice.Instance.Device, filePath);
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
        public static TGCTexture CreateTexture(Device d3dDevice, string fileName, string filePath, Texture d3dTexture)
        {
            try
            {
                filePath = filePath.Replace("\\\\", "\\");
                var d3dTexture2 = TexturesPool.Instance.CreateTexture(d3dDevice, filePath, d3dTexture);
                return new TGCTexture(fileName, filePath, d3dTexture2, true);
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
        public static TGCTexture CreateTextureNoPool(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                var d3dTexture = TextureLoader.FromFile(d3dDevice, filePath);
                return new TGCTexture(fileName, filePath, d3dTexture, false);
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
        public static TGCTexture CreateTextureNoPool(Device d3dDevice, string filePath)
        {
            var fInfo = new FileInfo(filePath);
            return CreateTextureNoPool(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        ///     Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        ///     Infiere el nombre de la textura en base al path completo
        ///     No se utiliza TexturesPool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TGCTexture CreateTextureNoPool(string filePath)
        {
            return CreateTextureNoPool(D3DDevice.Instance.Device, filePath);
        }

        #endregion Creacion Static
    }
}
