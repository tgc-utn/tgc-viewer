namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Datos parseados de un mapa BSP
    /// </summary>
    public class BspMapData
    {
        public QBrush[] brushes;
        public QBrushSide[] brushSides;
        public int[] drawIndexes;
        public QSurface[] drawSurfaces;
        public QDrawVert[] drawVerts;

        /// <summary>
        ///     Metadata del escenario (sin parsear)
        /// </summary>
        public string entdata;

        /// <summary>
        ///     Ruta del mapa
        /// </summary>
        public string filePath;

        public QFog[] fogs;
        public byte[] gridData;
        public int[] leafbrushes;
        public QLeaf[] leafs;
        public int[] leafSurfaces;
        public byte[] lightBytes;

        public QModel[] models;
        public QNode[] nodes;
        public QPlane[] planes;
        public QShader[] shaders;
        public QShaderData[] shaderXSurface;

        /// <summary>
        ///     Matriz PVS
        /// </summary>
        public QVisData visData;

        //public byte[] visBytes;
    }
}