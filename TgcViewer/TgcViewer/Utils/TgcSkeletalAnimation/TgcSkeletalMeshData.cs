using TGC.Core.SceneLoader;

namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Información de la Malla de un modelo animado por Animación Esquelética
    /// </summary>
    public class TgcSkeletalMeshData
    {
        //Huesos del esqueleto
        public TgcSkeletalBoneData[] bones;

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
        public float[] verticesBinormals;
        public int[] verticesColors;

        //Valores por vertice
        public float[] verticesCoordinates;

        public float[] verticesNormals;
        public float[] verticesTangents;

        //Tomar de a 3: VertesIdx, BoneIdx, Weight
        public float[] verticesWeights;
    }
}