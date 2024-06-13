using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.WorkshopShaders
{
    public class DepthOfField : TGCExampleViewer
    {
        private TGCBooleanModifier activarEfectoModifier;
        private TGCFloatModifier focusPlaneModifier;
        private TGCFloatModifier blurFactorModifier;

        private TgcMesh mesh;
        private Effect effect;
        private Surface g_pDepthStencil;     // Depth-stencil buffer
        private Texture g_pRenderTarget, g_pBlurFactor;
        private VertexBuffer g_pVBV3D;

        public DepthOfField(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Shaders";
            Name = "Workshop-DepthOfField";
            Description = "Depth of Field Sample";
        }

        public override void Init()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            //Cargamos un escenario

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(MediaDir + "\\MeshCreator\\Meshes\\Esqueletos\\EsqueletoHumano3\\Esqueleto3-TgcScene.xml");
            mesh = scene.Meshes[0];

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
            Camera = new TgcFpsCamera(new TGCVector3(50, 30, 50), 100, 10, Input);

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Blur Factor
            g_pBlurFactor = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
            effect.SetValue("g_BlurFactor", g_pBlurFactor);

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

            activarEfectoModifier = AddBoolean("activar_efecto", "Activar efecto", true);
            focusPlaneModifier = AddFloat("focus_plane", 1, 300, 10);
            blurFactorModifier = AddFloat("blur_factor", 0.1f, 5f, 0.5f);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            bool activar_efecto = activarEfectoModifier.Value;
            effect.SetValue("zfoco", focusPlaneModifier.Value);
            effect.SetValue("blur_k", blurFactorModifier.Value);

            // dibujo la escena una textura
            // guardo el render target anterior y seteo la textura como render target
            Surface pOldRT = d3dDevice.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            if (activar_efecto)
                d3dDevice.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = d3dDevice.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            if (activar_efecto)
                d3dDevice.DepthStencilSurface = g_pDepthStencil;

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            d3dDevice.BeginScene();
            renderScene("DefaultTechnique");
            d3dDevice.EndScene();
            pSurf.Dispose();

            if (activar_efecto)
            {
                // Genero el depth map
                Surface pSurf2 = g_pBlurFactor.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf2);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                d3dDevice.BeginScene();
                renderScene("RenderBlurFactor");
                d3dDevice.EndScene();
                pSurf2.Dispose();

                // restuaro el render target y el stencil
                d3dDevice.DepthStencilSurface = pOldDS;
                d3dDevice.SetRenderTarget(0, pOldRT);

                // dibujo el quad pp dicho :
                d3dDevice.BeginScene();
                effect.Technique = "DepthOfField";
                d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
                d3dDevice.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget);
                effect.SetValue("g_BlurFactor", g_pBlurFactor);

                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                d3dDevice.EndScene();
            }

            RenderAxis();
            RenderFPS();
            D3DDevice.Instance.Device.Present();
        }

        public void renderScene(string technique)
        {
            // seteo la tecnica en el efecto
            effect.Technique = technique;
            mesh.Effect = effect;
            mesh.Technique = technique;
            for (int j = 0; j < 5; ++j)
            {
                for (int i = 0; i < 15; ++i)
                {
                    mesh.Transform = TGCMatrix.Translation(j * 20, 0, i * 50);
                    mesh.Render();
                }
            }
        }

        public override void Dispose()
        {
            mesh.Dispose();
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pBlurFactor.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }
}