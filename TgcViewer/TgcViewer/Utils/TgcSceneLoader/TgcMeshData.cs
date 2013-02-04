using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.TgcSceneLoader
{
    public class TgcMeshData
    {
        //Tipos de Mesh Instancias
        public static readonly string ORIGINAL = "Original";
        public static readonly string INSTANCE = "Instance";

        public string name;
        public string layerName;

        //UserProperties
        public Dictionary<string, string> userProperties;

        //Material global
        public int materialId;

        //Filename del LightMap
        public string lightmap;
        public bool lightmapEnabled;

        //Color general, por si no tiene Material
        public float[] color;

        //Valores por triangulo
        public int[] coordinatesIndices;
        public int[] texCoordinatesIndices;
        public int[] colorIndices;
        public int[] texCoordinatesIndicesLightMap;

        //SubMaterials para cada triangulo
        public int[] materialsIds;

        //Valores por vertice
        public float[] verticesCoordinates;
        public float[] textureCoordinates;
        public float[] verticesNormals;
        public int[] verticesColors;
        public float[] textureCoordinatesLightMap;
        public float[] verticesTangents;
        public float[] verticesBinormals;

        //BoundingBox
        public float[] pMin;
        public float[] pMax;

        //Tipo de instancia
        public string instanceType;

        //Indice de la malla original
        public int originalMesh;

        //Datos de transformacion para instancia
        public float[] position;
        public float[] rotation;
        public float[] scale;

        //AlphaBlending activado
        public bool alphaBlending;
    }
}
