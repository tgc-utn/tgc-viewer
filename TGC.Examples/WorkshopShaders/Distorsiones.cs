using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace Examples.WorkshopShaders
{
    public class Distorsiones : TGCExampleViewer
    {
        private string MyMediaDir;
        private string MyShaderDir;
        private List<TgcMesh> meshes;
        private Effect effect;
        private Surface g_pDepthStencil;     // Depth-stencil buffer
        private Texture g_pRenderTarget;
        private VertexBuffer g_pVBV3D;
        public float time;

        public enum Distorciones : int
        {
            Nada = 0,
            FishEye = 1,
            Pincushion = 2,
            Barrel = 3,
            OculusRift = 4,
        }

        public Distorsiones(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-Distorsionador";
            Description = "Distorsionadores";
        }

        public override void Init()
        {
            time = 0;

            Device d3dDevice = D3DDevice.Instance.Device;
            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Cargamos un escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, ShadersDir + "WorkshopShaders\\FullQuad.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera personas
            Camara = new TgcFpsCamera(new TGCVector3(-182.3816f, 82.3252f, -811.9061f),Input);
            //Camara.SetCamera(new TGCVector3(-182.3816f, 82.3252f, -811.9061f), new TGCVector3(-182.0957f, 82.3147f, -810.9479f));

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

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

            Modifiers.addEnum("distorcionador", typeof(Distorciones), Distorciones.Pincushion);
            Modifiers.addBoolean("grid", "mostrar grilla", false);
            Modifiers.addFloat("Ku", 0, 1, 0.1f);
            Modifiers.addFloat("Kv", 0, 1, 0.1f);

            Modifiers.addFloat("oc_scale_in", 0.1f, 4, 2.5f);
            Modifiers.addFloat("oc_scale", 0.01f, 1f, 0.35f);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            time += ElapsedTime;
            //Cargar variables de shader

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = d3dDevice.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = d3dDevice.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            d3dDevice.DepthStencilSurface = g_pDepthStencil;
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            d3dDevice.BeginScene();

            //Dibujamos todos los meshes del escenario
            foreach (TgcMesh m in meshes)
            {
                m.Render();
            }
            d3dDevice.EndScene();
            pSurf.Dispose();

            int distorcionador = (int)Modifiers["distorcionador"];
            // restuaro el render target y el stencil
            d3dDevice.DepthStencilSurface = pOldDS;
            d3dDevice.SetRenderTarget(0, pOldRT);

            // dibujo el quad pp dicho :
            d3dDevice.BeginScene();

            switch (distorcionador)
            {
                case 1:
                    effect.Technique = "OjoPez";
                    break;

                case 2:
                    effect.Technique = "Pincusion";
                    break;

                case 3:
                    effect.Technique = "Barrel";
                    break;

                case 4:
                    effect.Technique = "OculusRift";
                    break;

                default:
                    effect.Technique = "ScreenCopy";
                    break;
            }
            effect.SetValue("time", time);
            effect.SetValue("fish_kU", (float)Modifiers["Ku"]);
            effect.SetValue("fish_kV", (float)Modifiers["Kv"]);
            effect.SetValue("grid", (bool)Modifiers["grid"]);
            effect.SetValue("ScaleIn", (float)Modifiers["oc_scale_in"]);
            effect.SetValue("Scale", (float)Modifiers["oc_scale"]);

            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            PostRender();
        }

        public override void Dispose()
        {
            foreach (TgcMesh m in meshes)
            {
                m.Dispose();
            }
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }
}