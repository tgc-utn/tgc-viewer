using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Terrain
{
    /// <summary>
    ///     Permite crear la malla de un terreno en base a una textura de Heightmap
    /// </summary>
    public class TgcSimpleTerrain : IRenderObject
    {
        protected Effect effect;

        protected string technique;
        private Texture terrainTexture;
        private int totalVertices;
        private VertexBuffer vbTerrain;
        private CustomVertex.PositionTextured[] data;

        public TgcSimpleTerrain()
        {
            Enabled = true;
            AlphaBlendEnable = false;

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        ///     Devuelve la informacion de Custom Vertex Buffer del HeightMap cargado
        /// </summary>
        /// <returns>Custom Vertex Buffer de tipo PositionTextured</returns>
        public CustomVertex.PositionTextured[] getData()
        {
            return data;
        }

        /// <summary>
        ///     Valor de Y para cada par (X,Z) del Heightmap
        /// </summary>
        public int[,] HeightmapData { get; private set; }

        /// <summary>
        ///     Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Centro del terreno
        /// </summary>
        public TGCVector3 Center { get; private set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        public TGCVector3 Position
        {
            get { return Center; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por v�rtice de canal Alpha.
        ///     Por default est� deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderiza el terreno
        /// </summary>
        public void Render()
        {
            if (!Enabled)
                return;

            //Textura
            effect.SetValue("texDiffuseMap", terrainTexture);
            TexturesManager.Instance.clear(1);

            TgcShaders.Instance.setShaderMatrix(effect, TGCMatrix.Identity);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionTextured;
            effect.Technique = technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vbTerrain, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Libera los recursos del Terreno
        /// </summary>
        public void Dispose()
        {
            if (vbTerrain != null)
            {
                vbTerrain.Dispose();
            }

            if (terrainTexture != null)
            {
                terrainTexture.Dispose();
            }
        }

        /// <summary>
        ///     Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmapPath">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void loadHeightmap(string heightmapPath, float scaleXZ, float scaleY, TGCVector3 center)
        {
            Center = center;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            HeightmapData = loadHeightMap(D3DDevice.Instance.Device, heightmapPath);
            float width = HeightmapData.GetLength(0);
            float length = HeightmapData.GetLength(1);

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (HeightmapData.GetLength(0) - 1) * (HeightmapData.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            var dataIdx = 0;
            data = new CustomVertex.PositionTextured[totalVertices];

            center.X = center.X * scaleXZ - width / 2 * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - length / 2 * scaleXZ;

            for (var i = 0; i < width - 1; i++)
            {
                for (var j = 0; j < length - 1; j++)
                {
                    //Vertices
                    var v1 = new TGCVector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v2 = new TGCVector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);
                    var v3 = new TGCVector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                        center.Z + j * scaleXZ);
                    var v4 = new TGCVector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j + 1] * scaleY,
                        center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    var t1 = new TGCVector2(i / width, j / length);
                    var t2 = new TGCVector2(i / width, (j + 1) / length);
                    var t3 = new TGCVector2((i + 1) / width, j / length);
                    var t4 = new TGCVector2((i + 1) / width, (j + 1) / length);

                    //Cargar triangulo 1
                    data[dataIdx] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 1] = new CustomVertex.PositionTextured(v2, t2.X, t2.Y);
                    data[dataIdx + 2] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);

                    //Cargar triangulo 2
                    data[dataIdx + 3] = new CustomVertex.PositionTextured(v1, t1.X, t1.Y);
                    data[dataIdx + 4] = new CustomVertex.PositionTextured(v4, t4.X, t4.Y);
                    data[dataIdx + 5] = new CustomVertex.PositionTextured(v3, t3.X, t3.Y);

                    dataIdx += 6;
                }
            }

            vbTerrain.SetData(data, 0, LockFlags.None);
        }

        /// <summary>
        ///     Carga la textura del terreno
        /// </summary>
        public void loadTexture(string path)
        {
            //Dispose textura anterior, si habia
            if (terrainTexture != null && !terrainTexture.Disposed)
            {
                terrainTexture.Dispose();
            }

            //Rotar e invertir textura
            var b = (Bitmap)Image.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(D3DDevice.Instance.Device, b, Usage.AutoGenerateMipMap, Pool.Managed);
        }

        /// <summary>
        ///     Carga los valores del Heightmap en una matriz
        /// </summary>
        protected int[,] loadHeightMap(Device d3dDevice, string path)
        {
            var bitmap = (Bitmap)Image.FromFile(path);
            var width = bitmap.Size.Width;
            var height = bitmap.Size.Height;
            var heightmap = new int[width, height];
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    //(j, i) invertido para primero barrer filas y despues columnas
                    var pixel = bitmap.GetPixel(j, i);
                    var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
                }
            }

            bitmap.Dispose();
            return heightmap;
        }
    }
}