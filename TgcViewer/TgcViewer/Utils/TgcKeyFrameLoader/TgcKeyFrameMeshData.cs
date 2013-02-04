using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    public class TgcKeyFrameMeshData
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

        //Informacion de Texturas y Materials
        public TgcMaterialData[] materialsData;

        //BoundingBox
        public float[] pMin;
        public float[] pMax;

    }
}
