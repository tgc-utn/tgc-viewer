using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Drawing;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Encapsula una textura de DirectX junto con información adicional
    /// </summary>
    public class TgcTexture
    {

        #region Creacion Static

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Se utiliza un Pool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="fileName">Nombre de la textura. Ejemplo: miTextura.jpg</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                Texture d3dTexture = GuiController.Instance.TexturesPool.createTexture(d3dDevice, filePath);
                return new TgcTexture(fileName, filePath, d3dTexture, true);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Infiere el nombre de la textura en base al path completo
        /// Se utiliza un Pool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(Device d3dDevice, string filePath)
        {
            FileInfo fInfo = new FileInfo(filePath);
            return TgcTexture.createTexture(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Infiere el nombre de la textura en base al path completo
        /// Se utiliza un Pool de Texturas para no cargar mas de una vez el mismo archivo.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTexture(string filePath)
        {
            return createTexture(GuiController.Instance.D3dDevice, filePath);
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Se utiliza un Pool de Texturas para no cargar mas de una vez el mismo archivo.
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
                Texture d3dTexture2 = GuiController.Instance.TexturesPool.createTexture(d3dDevice, filePath, d3dTexture);
                return new TgcTexture(fileName, filePath, d3dTexture2, true);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// No se utiliza Pool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="fileName">Nombre de la textura. Ejemplo: miTextura.jpg</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(Device d3dDevice, string fileName, string filePath)
        {
            try
            {
                Texture d3dTexture = TextureLoader.FromFile(d3dDevice, filePath);
                return new TgcTexture(fileName, filePath, d3dTexture, false);
            }
            catch (Exception ex)
            {

                throw new Exception("Error al intentar cargar la textura: " + filePath, ex);
            }
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Infiere el nombre de la textura en base al path completo
        /// No se utiliza Pool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="d3dDevice">Device de Direct3D</param>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(Device d3dDevice, string filePath)
        {
            FileInfo fInfo = new FileInfo(filePath);
            return TgcTexture.createTextureNoPool(d3dDevice, fInfo.Name, filePath);
        }

        /// <summary>
        /// Crea una nueva textura, haciendo el Loading del archivo de imagen especificado.
        /// Infiere el nombre de la textura en base al path completo
        /// No se utiliza Pool de Texturas. Se carga nuevamente cada una.
        /// </summary>
        /// <param name="filePath">Ruta completa de la textura. Ejemplo: C:\Texturas\miTextura.jpg</param>
        /// <returns>Textura creada</returns>
        public static TgcTexture createTextureNoPool(string filePath)
        {
            return createTextureNoPool(GuiController.Instance.D3dDevice, filePath);
        }

        #endregion


        /// <summary>
        /// Crear textura de TGC
        /// </summary>
        public TgcTexture(string fileName, string filePath, Texture d3dTexture, bool inPool)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.d3dTexture = d3dTexture;
            this.inPool = inPool;
        }

        private string fileName;
        /// <summary>
        /// Nombre del archivo de la textura. Ejemplo: miTextura.jpg
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        private string filePath;
        /// <summary>
        /// Ruta de la textura. Ejemplo: C:\Texturas\miTextura.jpg
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
        }

        private Texture d3dTexture;
        /// <summary>
        /// Textura de DirectX
        /// </summary>
        public Texture D3dTexture
        {
            get { return d3dTexture; }
        }

        private bool inPool;
        /// <summary>
        /// Indica si la textura fue creada dentro del Pool de texturas del framework
        /// </summary>
        public bool InPool
        {
            get { return inPool; }
        }

        /// <summary>
        /// Ancho de la textura
        /// </summary>
        public int Width
        {
            get { return d3dTexture.GetLevelDescription(0).Width; }
        }

        /// <summary>
        /// Alto de la textura
        /// </summary>
        public int Height
        {
            get { return d3dTexture.GetLevelDescription(0).Height; }
        }

        /// <summary>
        /// Dimensiones de la textura
        /// </summary>
        public Size Size
        {
            get 
            { 
                return new Size(Width, Height);
            }
        }

        /// <summary>
        /// Calcula el Aspect Ratio de la imagen
        /// </summary>
        /// <returns>Aspect Ratio</returns>
        public float getAspectRatio()
        {
            return (float)Width / Height;
        }


        /// <summary>
        /// Libera los recursos de la textura
        /// </summary>
        public void dispose()
        {
            //dispose de textura dentro de pool
            if (this.inPool)
            {
                bool disposed = GuiController.Instance.TexturesPool.disposeTexture(filePath);

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
                if (d3dTexture != null && !d3dTexture.Disposed)
                {
                    d3dTexture.Dispose();
                    d3dTexture = null;
                }
            }
        }

        public override string ToString()
        {
            return this.fileName;
        }

        /// <summary>
        /// Crear una nueva textura igual a esta.
        /// </summary>
        /// <returns>Textura clonada</returns>
        public TgcTexture clone()
        {
            TgcTexture cloneTexture;
            if (this.inPool)
            {
                cloneTexture = createTexture(this.filePath);
            }
            else
            {
                cloneTexture = createTextureNoPool(this.filePath);
            }
            return cloneTexture;
        }




        #region Textures Pool


        /// <summary>
        /// Pool para reutilizar texturas de igual path
        /// </summary>
        public class Pool
        {
            Dictionary<string, PoolItem> texturesPool;
            private static object syncRoot = new Object();

            public Pool()
            {
                texturesPool = new Dictionary<string, PoolItem>();
            }

            /// <summary>
            /// Agrega una textura al pool.
            /// Si no existe la crea. Sino reutiliza una existente.
            /// </summary>
            public Texture createTexture(Device d3dDevice, string filePath)
            {
                return createTexture(d3dDevice, filePath, null);
            }

            /// <summary>
            /// Agrega una textura al pool.
            /// Si no existe la crea. Sino reutiliza una existente.
            /// </summary>
            public Texture createTexture(Device d3dDevice, string filePath, Texture d3dTexture)
            {
                /*lock (syncRoot) No hace falta que este sincronizado*/


                //Si no existe, crear textura
                if (!texturesPool.ContainsKey(filePath))
                {
                    PoolItem newItem = new PoolItem();
                    if (d3dTexture == null)
                    {
                        d3dTexture = TextureLoader.FromFile(d3dDevice, filePath);
                    }
                    newItem.Texture = d3dTexture;
                    newItem.FilePath = filePath;
                    newItem.References = 0;
                    texturesPool.Add(filePath, newItem);
                }

                //aumentar las referencias a esta textura
                PoolItem item = texturesPool[filePath];
                item.References++;
                return item.Texture;
            }

            /// <summary>
            /// Hace Dispose de una textura del pool, pero solo si nadie mas la está utilizando.
            /// </summary>
            /// <returns>True si se hizo un Dispose físico</returns>
            public bool disposeTexture(string filePath)
            {
                /*lock (syncRoot) No hace falta que este sincronizado*/


                if (texturesPool.ContainsKey(filePath))
                {
                    PoolItem item = texturesPool[filePath];

                    //Quitar una referencia a esta textura
                    item.References--;

                    //Si nadie mas referencia esta textura, eliminar realmente
                    if (item.References <= 0)
                    {
                        //Dispose real de textura de DirectX
                        if (item.Texture != null && !item.Texture.Disposed)
                        {
                            item.Texture.Dispose();
                        }
                        //Quitar del pool
                        texturesPool.Remove(filePath);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Limpia todos los elementos del pool
            /// </summary>
            public void clearAll()
            {
                foreach (KeyValuePair<string, PoolItem> entry in texturesPool)
                {
                    PoolItem item = entry.Value;
                    if (item.Texture != null && !item.Texture.Disposed)
                    {
                        item.Texture.Dispose();
                    }
                }
                texturesPool.Clear();
            }

            /// <summary>
            /// Item con informacion de la textura
            /// </summary>
            class PoolItem
            {
                public Texture Texture;
                public string FilePath;
                public int References;
            }


            
        }

        #endregion


        #region Textures Manager

        /// <summary>
        /// Herrramienta para administrar las texturas cargadas en el Device.
        /// Antes evita hacer device.SetTexture() innecesarios, dado que es una operación
        /// bastante costosa.
        /// Pero ahora quedo deprecada esa parte, porque DirectX hace ese control internamente y no
        /// tiene sentido hacerlo.
        /// </summary>
        public class Manager
        {
            public Manager()
            {
            }

            /// <summary>
            /// Carga una textura en el Stage especificado.
            /// Si la textura es null, es similar aa hacer clear()
            /// </summary>
            /// <param name="stage">Stage en el cual configurar la textura</param>
            /// <param name="texture">Textura a configurar</param>
            public void set(int stage, TgcTexture texture)
            {
                GuiController.Instance.D3dDevice.SetTexture(stage, texture.d3dTexture);
            }

            /// <summary>
            /// Carga una textura como parámetro de un Shader
            /// </summary>
            /// <param name="effect">Shader</param>
            /// <param name="parameterName">Nombre del parámetro en el Shader</param>
            /// <param name="texture">Textura a aplicar</param>
            public void shaderSet(Effect effect, string parameterName, TgcTexture texture)
            {
                effect.SetValue(parameterName, texture.D3dTexture);
            }

            /// <summary>
            /// Limpiar la textura de un Stage particular
            /// </summary>
            public void clear(int stage)
            {
                GuiController.Instance.D3dDevice.SetTexture(stage, null);
            }

            /// <summary>
            /// Limpiar las texturas de todos los Stages
            /// </summary>
            public void clearAll()
            {
                for (int i = 0; i < TgcD3dDevice.DIRECTX_MULTITEXTURE_COUNT; i++)
                {
                    clear(i);
                }
            }
        }



        #endregion

    }
}
