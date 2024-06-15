using Microsoft.DirectX.Direct3D;

namespace TGC.Core.Textures
{
    /// <summary>
    ///     Item con informacion de la textura.
    /// </summary>
    public class PoolItem
    {
        public string FilePath { get; set; }
        public int References { get; set; }
        public Texture Texture { get; set; }

        public override string ToString()
        {
            return FilePath + ", [" + References + "]";
        }
    }
}
