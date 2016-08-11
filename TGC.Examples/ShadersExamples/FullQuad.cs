using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.ShadersExamples
{
    public class FullScreenQuad : TGCExampleViewer
    {
        private Effect effect;
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget;
        private VertexBuffer g_pVBV3D;
        private List<TgcMesh> meshes;
        private string MyShaderDir;
        public float time;

        public FullScreenQuad(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Post Process Shaders";
            Name = "Full Screen Quad";
            Description = "Full screen quad";
        }

        public override void Init()
        {
            time = 0;

            var d3dDevice = D3DDevice.Instance.Device;
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "FullQuad.fx", null, null, ShaderFlags.PreferFlowControl,
                null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera persona
            Camara = new TgcFpsCamera(new Vector3(250, 160, -570), Input);

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            Modifiers.addBoolean("activar_efecto", "Activar efecto", true);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            ClearTextures();

            var device = D3DDevice.Instance.Device;

            time += ElapsedTime;
            var activar_efecto = (bool)Modifiers["activar_efecto"];

            //Cargar variables de shader

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = device.GetRenderTarget(0);
            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            if (activar_efecto)
                device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = device.DepthStencilSurface;
            // Probar de comentar esta linea, para ver como se produce el fallo en el ztest
            // por no soportar usualmente el multisampling en el render to texture.
            if (activar_efecto)
                device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();

            //Dibujamos todos los meshes del escenario
            foreach (var m in meshes)
            {
                m.render();
            }

            device.EndScene();

            pSurf.Dispose();

            if (activar_efecto)
            {
                // restuaro el render target y el stencil
                device.DepthStencilSurface = pOldDS;
                device.SetRenderTarget(0, pOldRT);

                // dibujo el quad pp dicho :
                device.BeginScene();

                effect.Technique = "PostProcess";
                effect.SetValue("time", time);
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                device.EndScene();
            }

            device.BeginScene();
            RenderAxis();
            RenderFPS();
            device.EndScene();
            device.Present();
        }

        public override void Dispose()
        {
            foreach (var m in meshes)
            {
                m.dispose();
            }
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }
}