using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace Examples.WorkshopShaders
{
    public class POMTerrain
    {
        private VertexBuffer vbTerrain;
        public TGCVector3 center;
        public Texture terrainTexture;
        public int totalVertices;
        public int[,] heightmapData;
        public float scaleXZ;
        public float scaleY;
        public float ki;
        public float kj;
        public float ftex;      // factor para la textura

        public POMTerrain()
        {
            ftex = 1f;
            ki = 1;
            kj = 1;
        }

        public void loadHeightmap(string heightmapPath, float pscaleXZ, float pscaleY, TGCVector3 center)
        {
            scaleXZ = pscaleXZ;
            scaleY = pscaleY;

            Device d3dDevice = D3DDevice.Instance.Device;
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
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) + 1) * (heightmapData.GetLength(1) + 1);
            totalVertices *= (int)ki * (int)kj;
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Cargar vertices
            int dataIdx = 0;
            CustomVertex.PositionNormalTextured[] data = new CustomVertex.PositionNormalTextured[totalVertices];

            center.X = center.X * scaleXZ - (width / 2) * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - (length / 2) * scaleXZ;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {
                    //Vertices
                    TGCVector3 v1 = new TGCVector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j] * scaleY, center.Z + j * scaleXZ);
                    TGCVector3 v2 = new TGCVector3(center.X + i * scaleXZ, center.Y + heightmapData[i, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);
                    TGCVector3 v3 = new TGCVector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j] * scaleY, center.Z + j * scaleXZ);
                    TGCVector3 v4 = new TGCVector3(center.X + (i + 1) * scaleXZ, center.Y + heightmapData[i + 1, j + 1] * scaleY, center.Z + (j + 1) * scaleXZ);

                    //Coordendas de textura
                    TGCVector2 t1 = new TGCVector2(ftex * i / width, ftex * j / length);
                    TGCVector2 t2 = new TGCVector2(ftex * i / width, ftex * (j + 1) / length);
                    TGCVector2 t3 = new TGCVector2(ftex * (i + 1) / width, ftex * j / length);
                    TGCVector2 t4 = new TGCVector2(ftex * (i + 1) / width, ftex * (j + 1) / length);

                    //Cargar triangulo 1
                    TGCVector3 n1 = TGCVector3.Cross(v2 - v1, v3 - v1);
                    n1.Normalize();
                    data[dataIdx] = new CustomVertex.PositionNormalTextured(v1, n1, t1.X, t1.Y);
                    data[dataIdx + 1] = new CustomVertex.PositionNormalTextured(v2, n1, t2.X, t2.Y);
                    data[dataIdx + 2] = new CustomVertex.PositionNormalTextured(v4, n1, t4.X, t4.Y);

                    //Cargar triangulo 2
                    TGCVector3 n2 = TGCVector3.Cross(v4 - v1, v3 - v1);
                    n2.Normalize();
                    data[dataIdx + 3] = new CustomVertex.PositionNormalTextured(v1, n2, t1.X, t1.Y);
                    data[dataIdx + 4] = new CustomVertex.PositionNormalTextured(v4, n2, t4.X, t4.Y);
                    data[dataIdx + 5] = new CustomVertex.PositionNormalTextured(v3, n2, t3.X, t3.Y);

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

            Device d3dDevice = D3DDevice.Instance.Device;

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

        public void executeRender(Effect effect)
        {
            Device d3dDevice = D3DDevice.Instance.Device;
            TgcShaders.Instance.setShaderMatrixIdentity(effect);

            //Render terrain
            effect.SetValue("texDiffuseMap", terrainTexture);

            d3dDevice.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);

            int numPasses = effect.Begin(0);
            for (int n = 0; n < numPasses; n++)
            {
                effect.BeginPass(n);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
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

    public class POMTerrainSample : TGCExampleViewer
    {
        private TGCVertex3fModifier lightDirModifier;
        private TGCFloatModifier minSampleModifier;
        private TGCFloatModifier maxSampleModifier;
        private TGCFloatModifier heightMapScaleModifier;

        private string MyShaderDir;
        private Effect effect;
        private Texture g_pBaseTexture;
        private Texture g_pHeightmap;
        private POMTerrain terrain;
        private TGCVector2 pos = new TGCVector2(0, 0);
        private float dir_an = 0;
        private float kvel = 1.0f;

        private float time;

        public POMTerrainSample(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-POMTerrain";
            Description = "POM Terrain";
        }

        public override void Init()
        {
            time = 0f;
            Device d3dDevice = D3DDevice.Instance.Device;

            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            g_pBaseTexture = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\rocks.jpg");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_rocks.tga");

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            effect.Technique = "ParallaxOcclusion";
            effect.SetValue("aux_Tex", g_pBaseTexture);
            effect.SetValue("height_map", g_pHeightmap);
            effect.SetValue("phong_lighting", true);
            effect.SetValue("k_alpha", 0.75f);

            lightDirModifier = AddVertex3f("LightDir", new TGCVector3(-1, -1, -1), new TGCVector3(1, 1, 1), TGCVector3.Down);
            minSampleModifier = AddFloat("minSample", 1f, 10f, 10f);
            maxSampleModifier = AddFloat("maxSample", 11f, 50f, 50f);
            heightMapScaleModifier = AddFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            terrain = new POMTerrain();
            terrain.ftex = 250f;
            terrain.loadHeightmap(MediaDir + "Heighmaps\\" + "Heightmap3.jpg", 100f, 2.25f, new TGCVector3(0, 0, 0));
            terrain.loadTexture(MediaDir + "Heighmaps\\" + "TerrainTexture3.jpg");

            Camara.SetCamera(new TGCVector3(-350, 1000, -1100), new TGCVector3(0, 0, 0), TGCVector3.Up);
        }

        public override void Update()
        {
            PreUpdate();

            Device d3dDevice = D3DDevice.Instance.Device;

            // Actualizo la direccion
            if (Input.keyDown(Microsoft.DirectX.DirectInput.Key.A))
            {
                dir_an += 1f * ElapsedTime;
            }
            if (Input.keyDown(Microsoft.DirectX.DirectInput.Key.D))
            {
                dir_an -= 1f * ElapsedTime;
            }

            // calculo la velocidad
            TGCVector2 vel = new TGCVector2((float)Math.Sin(dir_an), (float)Math.Cos(dir_an));
            // actualizo la posicion
            pos += vel * kvel * ElapsedTime;

            // actualizo los parametros de la camara
            float dH = 2.0f;       // altura del personaje
            float H = terrain.CalcularAltura(pos.X, pos.Y);
            TGCVector2 pos_s = pos + vel * 2;
            TGCVector3 lookFrom = new TGCVector3(pos.X, H + dH, pos.Y);
            TGCVector3 lookAt = new TGCVector3(pos_s.X, H + 1.5f, pos_s.Y);
            d3dDevice.Transform.View = TGCMatrix.LookAtLH(lookFrom, lookAt, TGCVector3.Up);
            effect.SetValue("fvEyePosition", TGCVector3.Vector3ToFloat3Array(lookFrom));
        }

        public override void Render()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            time += ElapsedTime;

            TGCVector3 lightDir = lightDirModifier.Value;
            effect.SetValue("g_LightDir", TGCVector3.Vector3ToFloat3Array(lightDir));
            effect.SetValue("min_cant_samples", minSampleModifier.Value);
            effect.SetValue("max_cant_samples", maxSampleModifier.Value);
            effect.SetValue("fHeightMapScale", heightMapScaleModifier.Value);
            //effect.SetValue("fvEyePosition", TgcParserUtils.TGCVector3ToFloat3Array(GuiController.Instance.FpsCamera.getPosition()));
            effect.SetValue("time", time);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            d3dDevice.BeginScene();

            //Renderizar terreno con POM
            effect.Technique = "ParallaxOcclusion";
            terrain.executeRender(effect);

            PostRender();
        }

        public override void Dispose()
        {
            effect.Dispose();
            g_pBaseTexture.Dispose();
            g_pHeightmap.Dispose();
            terrain.dispose();
        }
    }
}