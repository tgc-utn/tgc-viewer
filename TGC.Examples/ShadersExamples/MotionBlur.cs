using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
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
    public class MotionBlur : TGCExampleViewer
    {
        private Matrix antMatWorldView;
        private Effect effect;
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget;

        private VertexBuffer g_pVBV3D;
        private Texture g_pVel1, g_pVel2; // velocidad
        private TgcMesh mesh;
        private string MyShaderDir;
        private float time;

        public MotionBlur(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-MotionBlur";
            Description = "Motion Effect";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Cargar mesh
            var loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Teapot\\Teapot-TgcScene.xml").Meshes[0];

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(D3DDevice.Instance.Device, MyShaderDir + "MotionBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";
            mesh.Effect = effect;

            //Camara
            Camara = new TgcRotationalCamera(new Vector3(), 150f, Input);

            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            // velocidad del pixel
            g_pVel1 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            g_pVel2 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);

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

            antMatWorldView = Matrix.Identity;
        }

        public override void Update()
        {
            PreUpdate();
            time += ElapsedTime;
            float r = 40;
            mesh.Position = new Vector3(r * (float)Math.Cos(time * 0.5), 0, 0 * (float)Math.Sin(time * 0.5));
            //mesh.rotateY(elapsedTime);
        }

        public void renderScene(string technique)
        {
            mesh.Technique = technique;
            mesh.render();
        }

        public override void Render()
        {
            ClearTextures();
            var device = D3DDevice.Instance.Device;

            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = device.GetRenderTarget(0);
            var pSurf = g_pVel1.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            // 1 - Genero un mapa de velocidad
            effect.Technique = "VelocityMap";
            // necesito mandarle la matrix de view proj anterior
            effect.SetValue("matWorldViewProjAnt", antMatWorldView * device.Transform.Projection);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            renderScene("VelocityMap");

            device.EndScene();
            device.Present();

            pSurf.Dispose();

            // 2- Genero la imagen pp dicha
            effect.Technique = "DefaultTechnique";
            pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            renderScene("DefaultTechnique");

            device.EndScene();
            device.Present();
            pSurf.Dispose();

            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.SetRenderTarget(0, pOldRT);

            device.BeginScene();
            effect.Technique = "PostProcessMotionBlur";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("texVelocityMap", g_pVel1);
            effect.SetValue("texVelocityMapAnt", g_pVel2);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            device.EndScene();
            device.Present();

            // actualizo los valores para el proximo frame
            antMatWorldView = mesh.Transform * device.Transform.View;
            var aux = g_pVel2;
            g_pVel2 = g_pVel1;
            g_pVel1 = aux;
        }

        public override void Dispose()
        {
            mesh.dispose();

            g_pRenderTarget.Dispose();
            g_pDepthStencil.Dispose();
            g_pVBV3D.Dispose();
            g_pVel1.Dispose();
            g_pVel2.Dispose();
        }
    }
}