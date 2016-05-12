using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;

namespace TGC.Core.Textures
{
    /// <summary>
    ///     TexturesPool para reutilizar texturas de igual path
    /// </summary>
    public class TexturesPool
    {
        private readonly Dictionary<string, PoolItem> texturesPool;

        private TexturesPool()
        {
            texturesPool = new Dictionary<string, PoolItem>();
        }

        /// <summary>
        ///     Permite acceder a una instancia de la clase TexturesPool desde cualquier parte del codigo.
        /// </summary>
        public static TexturesPool Instance { get; } = new TexturesPool();

        /// <summary>
        ///     Agrega una textura al pool.
        ///     Si no existe la crea. Sino reutiliza una existente.
        /// </summary>
        public Microsoft.DirectX.Direct3D.Texture createTexture(Device d3dDevice, string filePath)
        {
            return createTexture(d3dDevice, filePath, null);
        }

        /// <summary>
        ///     Agrega una textura al pool.
        ///     Si no existe la crea. Sino reutiliza una existente.
        /// </summary>
        public Microsoft.DirectX.Direct3D.Texture createTexture(Device d3dDevice, string filePath, Microsoft.DirectX.Direct3D.Texture d3dTexture)
        {
            //Si no existe, crear textura
            if (!texturesPool.ContainsKey(filePath))
            {
                var newItem = new PoolItem();
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
            var item = texturesPool[filePath];
            item.References++;
            return item.Texture;
        }

        /// <summary>
        ///     Hace Dispose de una textura del pool, pero solo si nadie mas la está utilizando.
        /// </summary>
        /// <returns>True si se hizo un Dispose físico</returns>
        public bool disposeTexture(string filePath)
        {
            if (texturesPool.ContainsKey(filePath))
            {
                var item = texturesPool[filePath];

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
        ///     Limpia todos los elementos del pool
        /// </summary>
        public void clearAll()
        {
            foreach (var entry in texturesPool)
            {
                var item = entry.Value;
                if (item.Texture != null && !item.Texture.Disposed)
                {
                    item.Texture.Dispose();
                }
            }
            texturesPool.Clear();
        }

        /// <summary>
        ///     Item con informacion de la textura
        /// </summary>
        private class PoolItem
        {
            public string FilePath;
            public int References;
            public Microsoft.DirectX.Direct3D.Texture Texture;

            public override string ToString()
            {
                return FilePath + ", [" + References + "]";
            }
        }
    }
}