using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    /// Información de la Malla de un modelo animado por Animación Esquelética
    /// </summary>
    public class TgcSkeletalMeshData
    {
        public string name;
        public string texturesDir;

        //Valores por triangulo
        public int[] coordinatesIndices;
        public int[] texCoordinatesIndices;
        public int[] colorIndices;

        //Color general, por si no tiene Material
        public float[] color;

        //SubMaterials para cada triangulo
        public int materialId;
        public int[] materialsIds;

        //Valores por vertice
        public float[] verticesCoordinates;
        public float[] textureCoordinates;
        public int[] verticesColors;
        public float[] verticesNormals;
        public float[] verticesTangents;
        public float[] verticesBinormals;

        //Informacion de Texturas y Materials
        public TgcMaterialData[] materialsData;

        //Huesos del esqueleto
        public TgcSkeletalBoneData[] bones;

        //Tomar de a 3: VertesIdx, BoneIdx, Weight
        public float[] verticesWeights;

        //BoundingBox
        public float[] pMin;
        public float[] pMax;

    }
}
