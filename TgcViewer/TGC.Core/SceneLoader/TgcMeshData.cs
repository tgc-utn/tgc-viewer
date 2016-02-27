using System.Collections.Generic;

namespace TGC.Core.SceneLoader
{
    public class TgcMeshData
    {
        //Tipos de Mesh Instancias
        public static readonly string ORIGINAL = "Original";

        public static readonly string INSTANCE = "Instance";

        //AlphaBlending activado
        public bool alphaBlending;

        //Color general, por si no tiene Material
        public float[] color;
        public int[] colorIndices;

        //Valores por triangulo
        public int[] coordinatesIndices;

        //Tipo de instancia
        public string instanceType;
        public string layerName;

        //Filename del LightMap
        public string lightmap;

        public bool lightmapEnabled;

        //Material global
        public int materialId;

        //SubMaterials para cada triangulo
        public int[] materialsIds;

        public string name;

        //Indice de la malla original
        public int originalMesh;

        public float[] pMax;

        //BoundingBox
        public float[] pMin;

        //Datos de transformacion para instancia
        public float[] position;

        public float[] rotation;
        public float[] scale;

        public int[] texCoordinatesIndices;
        public int[] texCoordinatesIndicesLightMap;

        public float[] textureCoordinates;
        public float[] textureCoordinatesLightMap;

        //UserProperties
        public Dictionary<string, string> userProperties;
        public float[] verticesBinormals;
        public int[] verticesColors;

        //Valores por vertice
        public float[] verticesCoordinates;
        public float[] verticesNormals;
        public float[] verticesTangents;
    }
}