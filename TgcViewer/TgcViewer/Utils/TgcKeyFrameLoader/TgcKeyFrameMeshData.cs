using TGC.Core.SceneLoader;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    public class TgcKeyFrameMeshData
    {
        //Color general, por si no tiene Material
        public float[] color;
        public int[] colorIndices;

        //Valores por triangulo
        public int[] coordinatesIndices;

        //SubMaterials para cada triangulo
        public int materialId;

        //Informacion de Texturas y Materials
        public TgcMaterialData[] materialsData;

        public int[] materialsIds;
        public string name;

        public float[] pMax;

        //BoundingBox
        public float[] pMin;

        public int[] texCoordinatesIndices;

        public float[] textureCoordinates;
        public string texturesDir;
        public int[] verticesColors;

        //Valores por vertice
        public float[] verticesCoordinates;
    }
}