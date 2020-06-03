using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.WorkshopShaders
{
    public class HDRLighting : TGCExampleViewer
    {
        private TGCBooleanModifier activarGlowModifier;
        private TGCBooleanModifier pantallaCompletaModifier;
        private TGCEnumModifier tmIzqModifier;
        private TGCEnumModifier tmDerModifier;
        private TGCIntervalModifier adaptacionPupilaModifier;

        private List<TgcMesh> meshes;
        private TgcSkyBox skyBox;
        private TgcSimpleTerrain terrain;
        private TgcMesh pasto, arbol, arbusto;
        private Effect effect;
        private Surface g_pDepthStencil;     // Depth-stencil buffer
        private Texture g_pRenderTarget, g_pGlowMap, g_pRenderTarget4, g_pRenderTarget4Aux;

        private const int NUM_REDUCE_TX = 5;
        private Texture[] g_pLuminance = new Texture[NUM_REDUCE_TX];
        private Texture g_pLuminance_ant;

        private VertexBuffer g_pVBV3D;
        private int cant_pasadas = 5;

        private float pupila_time = 0;
        private float MAX_PUPILA_TIME = 3;

        public enum ToneMapping : int
        {
            Nada = 0,
            Reinhard = 1,
            Modified_Reinhard = 2,
            Logaritmico = 3,
            MiddleGray = 4
        };

        public HDRLighting(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-HDRLighting";
            Description = "HDR lighting";
        }

        public override void Init()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();

            TgcScene scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Selva\\Selva-TgcScene.xml");
            meshes = scene.Meshes;
            TgcScene scene2 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml");
            pasto = scene2.Meshes[0];
            TgcScene scene3 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml");
            arbol = scene3.Meshes[0];

            arbol.Transform = TGCMatrix.Scaling(1, 3, 1);
            TgcScene scene4 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Arbusto2\\Arbusto2-TgcScene.xml");
            arbusto = scene4.Meshes[0];

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(MediaDir + "Heighmaps\\" + "TerrainTexture2.jpg", 20, 0.3f, new TGCVector3(0, -115, 0));
            terrain.loadTexture(MediaDir + "Heighmaps\\" + "grass.jpg");

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new TGCVector3(0, 500, 0);
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            string texturesPath = MediaDir + "Texturas\\Quake\\SkyBox2\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lun4_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lun4_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lun4_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lun4_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lun4_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lun4_ft.jpg");
            skyBox.Init();

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, ShadersDir + "WorkshopShaders\\GaussianBlur.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera persona
            TGCVector3 positionEye = new TGCVector3(-944.1269f, 100f, -1033.307f);
            Camera = new TgcFpsCamera(positionEye, 300, 10, Input);

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            g_pGlowMap = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4, d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            g_pRenderTarget4Aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4, d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            // Para computar el promedio de Luminance
            int tx_size = 1;
            for (int i = 0; i < NUM_REDUCE_TX; ++i)
            {
                g_pLuminance[i] = new Texture(d3dDevice, tx_size, tx_size, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
                tx_size *= 4;
            }

            g_pLuminance_ant = new Texture(d3dDevice, 1, 1, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
            {
                new CustomVertex.PositionTextured( -1, 1, 1, 0,0),
                new CustomVertex.PositionTextured(1,  1, 1, 1,0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
                new CustomVertex.PositionTextured(1,-1, 1, 1,1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            activarGlowModifier = AddBoolean("activar_glow", "Activar Glow", true);
            pantallaCompletaModifier = AddBoolean("pantalla_completa", "Pant.completa", true);
            tmIzqModifier = AddEnum("tm_izq", typeof(ToneMapping), ToneMapping.MiddleGray);
            tmDerModifier = AddEnum("tm_der", typeof(ToneMapping), ToneMapping.Nada);
            adaptacionPupilaModifier = AddInterval("adaptacion_pupila", new object[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f }, 2);
        }

        public override void Update()
        {
            Camera.UpdateCamera(ElapsedTime);
        }

        public override void Render()
        {
            renderConEfectos(ElapsedTime);
        }

        public void renderSinEfectos(float elapsedTime)
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            d3dDevice.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene("DefaultTechnique");
            //Render skybox
            skyBox.Render();
            d3dDevice.EndScene();
        }

        public void renderConEfectos(float elapsedTime)
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            // Resolucion de pantalla
            float screen_dx = d3dDevice.PresentationParameters.BackBufferWidth;
            float screen_dy = d3dDevice.PresentationParameters.BackBufferHeight;
            effect.SetValue("screen_dx", screen_dx);
            effect.SetValue("screen_dy", screen_dy);

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = d3dDevice.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = d3dDevice.DepthStencilSurface;
            d3dDevice.DepthStencilSurface = g_pDepthStencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("KLum", 0.05f);
            d3dDevice.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene("DefaultTechnique");
            // y el skybox (el skybox no tiene efectos, va por fixed OJOOO)
            skyBox.Render();

            d3dDevice.EndScene();
            pSurf.Dispose();

            MAX_PUPILA_TIME = (float)adaptacionPupilaModifier.Value;
            bool glow = activarGlowModifier.Value;
            effect.SetValue("glow", glow);
            if (glow)
            {
                // dibujo el glow map
                effect.SetValue("KLum", 1.0f);
                effect.Technique = "DefaultTechnique";
                pSurf = g_pGlowMap.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                d3dDevice.BeginScene();

                // dibujo el skybox que es brillante, con la tecnica estandard
                skyBox.Render();

                // El resto opacos
                renderScene("DibujarObjetosOscuros");

                d3dDevice.EndScene();
                pSurf.Dispose();

                // Hago un blur sobre el glow map
                // 1er pasada: downfilter x 4
                // -----------------------------------------------------
                pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf);
                d3dDevice.BeginScene();
                effect.Technique = "DownFilter4";
                d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pGlowMap);

                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();
                d3dDevice.EndScene();
                d3dDevice.DepthStencilSurface = pOldDS;

                // Pasadas de blur
                for (int P = 0; P < cant_pasadas; ++P)
                {
                    // Gaussian blur Horizontal
                    // -----------------------------------------------------
                    pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                    d3dDevice.SetRenderTarget(0, pSurf);
                    // dibujo el quad pp dicho :
                    d3dDevice.BeginScene();
                    effect.Technique = "GaussianBlurSeparable";
                    d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                    d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget4);

                    d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(0);
                    d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    pSurf.Dispose();
                    d3dDevice.EndScene();

                    pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                    d3dDevice.SetRenderTarget(0, pSurf);
                    pSurf.Dispose();

                    //  Gaussian blur Vertical
                    // -----------------------------------------------------
                    d3dDevice.BeginScene();
                    effect.Technique = "GaussianBlurSeparable";
                    d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                    d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                    d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(1);
                    d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    d3dDevice.EndScene();
                }
                //TextureLoader.Save("glowmap", ImageFileFormat.Bmp, g_pRenderTarget4Aux);
            }

            // computo el promedio
            pSurf = g_pLuminance[NUM_REDUCE_TX - 1].GetSurfaceLevel(0);
            screen_dx = pSurf.Description.Width;
            screen_dy = pSurf.Description.Height;
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.BeginScene();
            effect.Technique = "DownFilter4";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            pSurf.Dispose();
            d3dDevice.EndScene();
            d3dDevice.DepthStencilSurface = pOldDS;
            string fname2 = string.Format("Pass{0:D}.bmp", NUM_REDUCE_TX);
            //SurfaceLoader.Save(fname2, ImageFileFormat.Bmp, pSurf);

            // Reduce
            for (int i = NUM_REDUCE_TX - 1; i > 0; i--)
            {
                pSurf = g_pLuminance[i - 1].GetSurfaceLevel(0);
                effect.SetValue("screen_dx", screen_dx);
                effect.SetValue("screen_dy", screen_dy);

                d3dDevice.SetRenderTarget(0, pSurf);
                effect.SetValue("g_RenderTarget", g_pLuminance[i]);
                d3dDevice.BeginScene();
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();
                d3dDevice.EndScene();

                string fname = string.Format("Pass{0:D}.bmp", i);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pSurf);

                screen_dx /= 4.0f;
                screen_dy /= 4.0f;
            }

            //  Tone mapping
            // -----------------------------------------------------
            effect.SetValue("tone_mapping_izq", (int)tmIzqModifier.Value);
            effect.SetValue("tone_mapping_der", (int)tmDerModifier.Value);
            effect.SetValue("pantalla_completa", pantallaCompletaModifier.Value);
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            d3dDevice.SetRenderTarget(0, pOldRT);
            d3dDevice.BeginScene();
            effect.Technique = "ToneMapping";
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_GlowMap", g_pRenderTarget4Aux);
            pupila_time += elapsedTime;
            if (pupila_time >= MAX_PUPILA_TIME)
            {
                pupila_time = 0;
                effect.SetValue("g_Luminance_ant", g_pLuminance[0]);
                Texture aux = g_pLuminance[0];
                g_pLuminance[0] = g_pLuminance_ant;
                g_pLuminance_ant = aux;
            }
            else
            {
                effect.SetValue("g_Luminance", g_pLuminance[0]);
            }

            effect.SetValue("pupila_time", pupila_time / MAX_PUPILA_TIME);      // 0..1
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            PostRender();
        }

        public void renderScene(string Technique)
        {
            //Dibujamos todos los meshes del escenario
            Random rnd = new Random(1);
            pasto.Effect = effect;
            pasto.Technique = Technique;
            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < 10; ++j)
                {
                    pasto.Transform = TGCMatrix.Scaling(3, 4 + rnd.Next(0, 4), 5) * TGCMatrix.Translation(-i * 200 + rnd.Next(0, 50), 0, -j * 200 + rnd.Next(0, 50));
                    pasto.Render();
                }

            arbusto.Effect = effect;
            arbusto.Technique = Technique;
            for (int i = 0; i < 5; ++i)
                for (int j = 0; j < 5; ++j)
                {
                    arbusto.Transform = TGCMatrix.Translation(-i * 400 + rnd.Next(0, 50), 0, -j * 400 + rnd.Next(0, 50));
                    arbusto.Render();
                }

            arbol.Effect = effect;
            arbol.Technique = Technique;
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                {
                    arbol.Transform = TGCMatrix.Translation(-i * 700 + rnd.Next(0, 50), 0, -j * 700 + rnd.Next(0, 50));
                    arbol.Render();
                }

            // -------------------------------------
            //Renderizar terreno
            terrain.Effect = effect;
            terrain.Technique = Technique;
            terrain.Render();
        }

        public override void Dispose()
        {
            foreach (TgcMesh m in meshes)
            {
                m.Dispose();
            }
            effect.Dispose();
            skyBox.Dispose();
            terrain.Dispose();
            pasto.Dispose();
            arbol.Dispose();
            arbusto.Dispose();
            g_pRenderTarget.Dispose();
            g_pGlowMap.Dispose();
            g_pRenderTarget4Aux.Dispose();
            g_pRenderTarget4.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
            for (int i = 0; i < NUM_REDUCE_TX; i++)
            {
                g_pLuminance[i].Dispose();
            }
            g_pLuminance_ant.Dispose();
        }
    }
}