using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;

namespace Examples.Quake3Loader
{
    /// <summary>
    /// Datos parseados de un mapa BSP
    /// </summary>
    public class BspMapData
    {
        /// <summary>
        /// Ruta del mapa
        /// </summary>
        public string filePath;

        /// <summary>
        /// Metadata del escenario (sin parsear)
        /// </summary>
        public string entdata;

        public QModel[] models;
        public QShader[] shaders;
        public QLeaf[] leafs;
        public QPlane[] planes;
        public QNode[] nodes;
        public int[] leafSurfaces;
        public int[] leafbrushes;
        public QBrush[] brushes;
        public QBrushSide[] brushSides;
        public byte[] lightBytes;
        public byte[] gridData;
        public QDrawVert[] drawVerts;
        public int[] drawIndexes;
        public QSurface[] drawSurfaces;
        public QFog[] fogs;
        public QShaderData[] shaderXSurface;

        /// <summary>
        /// Matriz PVS
        /// </summary>
        public QVisData visData;
        //public byte[] visBytes;

    }
}
