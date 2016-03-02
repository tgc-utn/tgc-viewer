using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Shaders;

namespace TGC.Examples.Shaders.WorkshopShaders
{
    /// <summary>
    ///     Customizacion de SimpleTerrain para renderizado de terrenos
    /// </summary>
    public class MySimpleTerrain
    {
        public Vector3 center;
        public float ftex; // factor para la textura
        public int[,] heightmapData;
        public float ki;
        public float kj;
        public float radio_1;
        public float radio_2;
        public float scaleXZ;
        public float scaleY;
        public Texture terrainTexture;
        public bool torus;
        public int totalVertices;
        private VertexBuffer vbTerrain;

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

            this.center = center;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            heightmapData = loadHeightMap(D3DDevice.Instance.Device, heightmapPath);
            float width = heightmapData.GetLength(0);
            float length = heightmapData.GetLength(1);

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) + 1) * (heightmapData.GetLength(1) + 1);
            totalVertices *= (int)ki * (int)kj;
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionTextured), totalVertices,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            var dataIdx = 0;
            var data = new CustomVertex.PositionTextured[totalVertices];

            center.X = center.X * scaleXZ - width / 2 * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - length / 2 * scaleXZ;

            if (torus)
            {
                var di = width * ki;
                var dj = length * kj;

                for (var i = 0; i < width * ki; i++)
                {
                    for (var j = 0; j < length * kj; j++)
                    {
                        var ri = i % (int)width;
                        var rj = j % (int)length;
                        var ri1 = (i + 1) % (int)width;
                        var rj1 = (j + 1) % (int)length;

                        Vector3 v1, v2, v3, v4;
                        {
                            var r = radio_2 + heightmapData[ri, rj] * scaleY;
                            var s = 2f * (float)Math.PI * j / dj;
                            var t = -(float)Math.PI * i / di;
                            var x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            var z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            var y = r * (float)Math.Sin(t);
                            v1 = new Vector3(x, y, z);
                        }
                        {
                            var r = radio_2 + heightmapData[ri, rj1] * scaleY;
                            var s = 2f * (float)Math.PI * (j + 1) / dj;
                            var t = -(float)Math.PI * i / di;
                            var x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            var z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            var y = r * (float)Math.Sin(t);
                            v2 = new Vector3(x, y, z);
                        }
                        {
                            var r = radio_2 + heightmapData[ri1, rj] * scaleY;
                            var s = 2f * (float)Math.PI * j / dj;
                            var t = -(float)Math.PI * (i + 1) / di;
                            var x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            var z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            var y = r * (float)Math.Sin(t);
                            v3 = new Vector3(x, y, z);
                        }
                        {
                            var r = radio_2 + heightmapData[ri1, rj1] * scaleY;
                            var s = 2f * (float)Math.PI * (j + 1) / dj;
                            var t = -(float)Math.PI * (i + 1) / di;
                            var x = (float)Math.Cos(s) * (radio_1 + r * (float)Math.Cos(t));
                            var z = (float)Math.Sin(s) * (radio_1 + r * (float)Math.Cos(t));
                            var y = r * (float)Math.Sin(t);
                            v4 = new Vector3(x, y, z);
                        }

                        //Coordendas de textura
                        var t1 = new Vector2(ftex * i / width, ftex * j / length);
                        var t2 = new Vector2(ftex * i / width, ftex * (j + 1) / length);
                        var t3 = new Vector2(ftex * (i + 1) / width, ftex * j / length);
                        var t4 = new Vector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

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
                for (var i = 0; i < width - 1; i++)
                {
                    for (var j = 0; j < length - 1; j++)
                    {
                        //Vertices
                        var v1 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j] * scaleY,
                            center.Z + j * scaleXZ);
                        var v2 = new Vector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j + 1] * scaleY,
                            center.Z + (j + 1) * scaleXZ);
                        var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j] * scaleY,
                            center.Z + j * scaleXZ);
                        var v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j + 1] * scaleY,
                            center.Z + (j + 1) * scaleXZ);

                        //Coordendas de textura
                        var t1 = new Vector2(ftex * i / width, ftex * j / length);
                        var t2 = new Vector2(ftex * i / width, ftex * (j + 1) / length);
                        var t3 = new Vector2(ftex * (i + 1) / width, ftex * j / length);
                        var t4 = new Vector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

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
            terrainTexture = Texture.FromBitmap(D3DDevice.Instance.Device, b, Usage.None, Pool.Managed);
        }

        /// <summary>
        ///     Carga los valores del Heightmap en una matriz
        /// </summary>
        private int[,] loadHeightMap(Device d3dDevice, string path)
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

        // utilizo estos metodos para el render:
        public void render()
        {
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

            //Render terrain
            D3DDevice.Instance.Device.SetTexture(0, terrainTexture);
            D3DDevice.Instance.Device.SetTexture(1, null);
            D3DDevice.Instance.Device.Material = D3DDevice.DEFAULT_MATERIAL;

            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, vbTerrain, 0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
        }

        public void executeRender(Effect effect)
        {
            TgcShaders.Instance.setShaderMatrixIdentity(effect);

            //Render terrain
            effect.SetValue("texDiffuseMap", terrainTexture);

            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, vbTerrain, 0);

            var numPasses = effect.Begin(0);
            for (var n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
                effect.EndPass();
            }
            effect.End();
        }

        public float CalcularAltura(float x, float z)
        {
            var largo = scaleXZ * 64;
            var pos_i = 64f * (0.5f + x / largo);
            var pos_j = 64f * (0.5f + z / largo);

            var pi = (int)pos_i;
            var fracc_i = pos_i - pi;
            var pj = (int)pos_j;
            var fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else if (pi > 63)
                pi = 63;

            if (pj < 0)
                pj = 0;
            else if (pj > 63)
                pj = 63;

            var pi1 = pi + 1;
            var pj1 = pj + 1;
            if (pi1 > 63)
                pi1 = 63;
            if (pj1 > 63)
                pj1 = 63;

            // 2x2 percent closest filtering usual:
            var H0 = heightmapData[pi, pj] * scaleY;
            var H1 = heightmapData[pi1, pj] * scaleY;
            var H2 = heightmapData[pi, pj1] * scaleY;
            var H3 = heightmapData[pi1, pj1] * scaleY;
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) +
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