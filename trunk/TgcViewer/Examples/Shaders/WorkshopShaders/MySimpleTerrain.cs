using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using TgcViewer.Utils;

namespace Examples.Shaders.WorkshopShaders
{
    /// <summary>
    /// Customizacion de SimpleTerrain para renderizado de terrenos
    /// </summary>
    public class MySimpleTerrain 
    {
        VertexBuffer vbTerrain;
        public Vector3 center;
        public Texture terrainTexture;
        public bool torus;
        public float radio_1;
        public float radio_2;
        public int totalVertices;
        public int[,] heightmapData;
        public float scaleXZ;
        public float scaleY;
        public float ki;
        public float kj;
        public float ftex;      // factor para la textura


        public MySimpleTerrain()
        {
            ftex = 1f;          
            // para toros: 
            torus = false;
            radio_1 = radio_2 = 0;
            ki = 1;
            kj = 1;
        }

        public void loadHeightmap(string heightmapPath, float pscaleXZ, float pscaleY, Vector3 center)
        {
            scaleXZ = pscaleXZ;
            scaleY = pscaleY;

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
            totalVertices = 2 * 3 * (heightmapData.GetLength(0)+1 ) * (heightmapData.GetLength(1)+1 );
            totalVertices *= (int)ki * (int)kj;
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionTextured[] data = new CustomVertex.PositionTextured[totalVertices];

            center.X = center.X * scaleXZ - (width / 2) * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - (length / 2) * scaleXZ;

            if (torus)
            {
                float di = width * ki;
                float dj = length* kj;

                for (int i = 0; i < width*ki; i++)
                {
                    for (int j = 0; j < length*kj; j++)
                    {
                        int ri = i % (int)width;
                        int rj = j % (int)length;
                        int ri1 = (i + 1)  % (int)width;
                        int rj1 = (j + 1)  % (int)length;


                        Vector3 v1, v2, v3, v4;
                        {
                            float r = radio_2 + heightmapData[ri, rj] * scaleY;
                            float s = 2f * (float)Math.PI * j / dj;
                            float t = -(float)Math.PI * i / di;
                            float x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            float z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            float y = r * (float)Math.Sin(t);
                            v1 = new Vector3(x, y, z);
                        }   
                        {
                            float r = radio_2 + heightmapData[ri, rj1] * scaleY;
                            float s = 2f * (float)Math.PI * (j + 1) / dj;
                            float t = -(float)Math.PI * i / di;
                            float x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            float z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            float y = r * (float)Math.Sin(t);
                            v2 = new Vector3(x, y, z);
                        }
                        {
                            float r = radio_2 + heightmapData[ri1, rj] * scaleY;
                            float s = 2f * (float)Math.PI * j / dj;
                            float t = -(float)Math.PI * (i + 1) / di;
                            float x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            float z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            float y = r * (float)Math.Sin(t);
                            v3 = new Vector3(x, y, z);
                        }
                        {
                            float r = radio_2 + heightmapData[ri1, rj1] * scaleY;
                            float s = 2f * (float)Math.PI * (j + 1) / dj;
                            float t = -(float)Math.PI * (i + 1) / di;
                            float x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            float z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            float y = r * (float)Math.Sin(t);
                            v4 = new Vector3(x, y, z);
                        }

                        //Coordendas de textura
                        Vector2 t1 = new Vector2(ftex * i / width, ftex * j / length);
                        Vector2 t2 = new Vector2(ftex * i / width, ftex * (j + 1) / length);
                        Vector2 t3 = new Vector2(ftex * (i + 1) / width, ftex * j / length);
                        Vector2 t4 = new Vector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

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
            }
            else
            {
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
                        Vector2 t1 = new Vector2(ftex * i / width, ftex * j / length);
                        Vector2 t2 = new Vector2(ftex * i / width, ftex * (j + 1) / length);
                        Vector2 t3 = new Vector2(ftex * (i + 1) / width, ftex * j / length);
                        Vector2 t4 = new Vector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

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
        private int[,] loadHeightMap(Device d3dDevice, string path)
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

        // utilizo estos metodos para el render:
        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            d3dDevice.Transform.World = Matrix.Identity;

            //Render terrain 
            d3dDevice.SetTexture(0, terrainTexture);
            d3dDevice.SetTexture(1, null);
            d3dDevice.Material = TgcD3dDevice.DEFAULT_MATERIAL;

            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
        }


        public void executeRender(Effect effect)
        {
            Device device = GuiController.Instance.D3dDevice;
            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);

            //Render terrain 
            effect.SetValue("texDiffuseMap", terrainTexture);

            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, vbTerrain, 0);

            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
                effect.EndPass();
            }
            effect.End();
        }

        public float CalcularAltura(float x, float z)
        {
            float largo = scaleXZ * 64;
            float pos_i = 64f * (0.5f + x / largo);
            float pos_j = 64f * (0.5f + z / largo);

            int pi = (int)pos_i;
            float fracc_i = pos_i - pi;
            int pj = (int)pos_j;
            float fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else
                if (pi > 63)
                    pi = 63;

            if (pj < 0)
                pj = 0;
            else
                if (pj > 63)
                    pj = 63;

            int pi1 = pi + 1;
            int pj1 = pj + 1;
            if (pi1 > 63)
                pi1 = 63;
            if (pj1 > 63)
                pj1 = 63;

            // 2x2 percent closest filtering usual: 
            float H0 = heightmapData[pi, pj] * scaleY;
            float H1 = heightmapData[pi1, pj] * scaleY;
            float H2 = heightmapData[pi, pj1] * scaleY;
            float H3 = heightmapData[pi1, pj1] * scaleY;
            float H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
                      (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;
            return H;
        }



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
