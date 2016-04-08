namespace TGC.Core.KeyFrameLoader
{
    /// <summary>
    ///     Informacion del MeshData original que hay que guardar para poder alterar el VertexBuffer con la animacion
    /// </summary>
    public class OriginalData
    {
        public int[] colorIndices;
        public int[] coordinatesIndices;
        public int[] texCoordinatesIndices;
        public float[] textureCoordinates;
        public int[] verticesColors;
    }
}