using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Input;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Shaders.WorkshopShaders
{

  
    public class MotionBlur : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        Effect effect;
        TgcMesh mesh;
        float time = 0;

        VertexBuffer g_pVBV3D;
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget;
        Texture g_pVel1, g_pVel2;   // velocidad
        Matrix antMatWorldView;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-MotionBlur";
        }

        public override string getDescription()
        {
            return "Motion Effect";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;

            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Cargar mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Teapot\\Teapot-TgcScene.xml").Meshes[0];

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice,MyShaderDir + "MotionBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";
            mesh.Effect = effect;
            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 150;

            GuiController.Instance.BackgroundColor = Color.Black;


            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

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

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            antMatWorldView  = Matrix.Identity;



        }

        public void update(float elapsedTime)
        {
            time += elapsedTime;
            float r = 40;
            mesh.Position = new Vector3(r * (float)Math.Cos(time*0.5), 0, 0 * (float)Math.Sin(time*0.5));
            //mesh.rotateY(elapsedTime);
        }

        public void renderScene(String technique)
        {
            mesh.Technique = technique;
            mesh.render();
        }


        public override void render(float elapsedTime)
        {
            update(elapsedTime);
            Device device = GuiController.Instance.D3dDevice;

            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pVel1.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            // 1 - Genero un mapa de velocidad 
            effect.Technique = "VelocityMap";
            // necesito mandarle la matrix de view proj anterior
            effect.SetValue("matWorldViewProjAnt", antMatWorldView * device.Transform.Projection);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            renderScene("VelocityMap");
            device.EndScene();
            pSurf.Dispose();


            // 2- Genero la imagen pp dicha 
            effect.Technique = "DefaultTechnique";
            pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            renderScene("DefaultTechnique");
            device.EndScene();
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

            // actualizo los valores para el proximo frame
            antMatWorldView = mesh.Transform* device.Transform.View;
            Texture aux = g_pVel2;
            g_pVel2 = g_pVel1;
            g_pVel1 = aux;


        }




        public override void close()
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
