using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;

namespace TgcViewer.Utils.Terrain
{
    /// <summary>
    /// Permite crear la malla de un terreno en base a una textura de Heightmap
    /// </summary>
    public class TgcSimpleTerrain : IRenderObject
    {
        VertexBuffer vbTerrain;
        Texture terrainTexture;
        int totalVertices;
        
        int[,] heightmapData;
        /// <summary>
        /// Valor de Y para cada par (X,Z) del Heightmap
        /// </summary>
        public int[,] HeightmapData
        {
            get { return heightmapData; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private Vector3 center;
        /// <summary>
        /// Centro del terreno
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }

        private bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        protected Effect effect;
        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;
        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }


        public TgcSimpleTerrain()
        {
            enabled = true;
            alphaBlendEnable = false;

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        /// Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmapPath">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void loadHeightmap(string heightmapPath, float scaleXZ, float scaleY, Vector3 center)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.center = center;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            heightmapData = loadHeightMap(d3dDevice, heightmapPath);
            float width = (float)heightmapData.GetLength(0);
            float length = (float)heightmapData.GetLength(1);


            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) - 1) * (heightmapData.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[totalVertices];

            center.X = center.X * scaleXZ - (width / 2) * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - (length / 2) * scaleXZ;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {
                    //Vertices
                    Vector3 v1 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j] * scaleY, center.Z + j * scaleXZ);
                    Vector3 v2 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);
                    Vector3 v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j] * scaleY, center.Z + j * scaleXZ);
                    Vector3 v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    Vector2 t1 = new Vector2(i / width, j / length);
                    Vector2 t2 = new Vector2(i / width, (j + 1) / length);
                    Vector2 t3 = new Vector2((i + 1) / width, j / length);
                    Vector2 t4 = new Vector2((i + 1) / width, (j + 1) / length);

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
        /// Carga la textura del terreno
        /// </summary>
        public void loadTexture(string path)
        {
            //Dispose textura anterior, si habia
            if (terrainTexture != null && !terrainTexture.Disposed)
            {
                terrainTexture.Dispose();
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Rotar e invertir textura
            Bitmap b = (Bitmap)Bitmap.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }




        /// <summary>
        /// Carga los valores del Heightmap en una matriz
        /// </summary>
        protected int[,] loadHeightMap(Device d3dDevice, string path)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;
            int[,] heightmap = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //(j, i) invertido para primero barrer filas y despues columnas
                    Color pixel = bitmap.GetPixel(j, i);
                    float intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = (int)intensity;
                }
                
            }

            bitmap.Dispose();
            return heightmap;
        }


        /// <summary>
        /// Renderiza el terreno
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Textura
            effect.SetValue("texDiffuseMap", terrainTexture);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(this.effect, Matrix.Identity);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
            effect.EndPass();
            effect.End();

        }

        public Vector3 Position
        {
            get { return center; }
        }

        /// <summary>
        /// Libera los recursos del Terreno
        /// </summary>
        public void dispose()
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

    }
}
