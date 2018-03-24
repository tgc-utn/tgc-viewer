using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EfectoGaussianBlur:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "PostProcess/EfectoBlur"
    ///     Muestra como utilizar la tenica de Render Target para lograr efectos de Post-Procesado.
    ///     Toda la escena no se dibuja a pantalla sino que se dibuja a una textura auxiliar.
    ///     Luego esa textura es renderizada con una pasada de Gaussian blur horizontal.
    ///     Y por ultimo se hace otra pasada mas de Gaussian blur pero vertical.
    ///     Autor: Mariano, Banquiero.
    /// </summary>
    public class GaussianBlur : TGCExampleViewer
    {
        private TGCBooleanModifier activarEfectoModifier;
        private TGCBooleanModifier separableModifier;
        private TGCIntModifier cantPasadasModifier;

        private Effect effect;
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget, g_pRenderTarget4, g_pRenderTarget4Aux;
        private VertexBuffer g_pVBV3D;
        private List<TgcMesh> meshes;
        private string MyShaderDir;

        public GaussianBlur(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Post Process Shaders";
            Name = "Gaussian Blur";
            Description = "Graba la escena a un Render Target y luego con un pixel shader se borronea la imagen con Gaussian Blur.";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "GaussianBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera persona
            Camara = new TgcFpsCamera(new TGCVector3(250, 160, -570), Input);

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4
                , d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4Aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4
                , d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            //Creamos un FullScreen Quad
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

            activarEfectoModifier = AddBoolean("activar_efecto", "Activar efecto", true);
            separableModifier = AddBoolean("separable", "Separable Blur", true);
            cantPasadasModifier = AddInt("cant_pasadas", 1, 10, 1);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            ClearTextures();
            var device = D3DDevice.Instance.Device;

            var activar_efecto = activarEfectoModifier.Value;

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
                m.Render();
            }
            device.EndScene();
            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            //TextureLoader.Save(this.ShadersDir + "render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);
            pSurf.Dispose();

            if (activar_efecto)
            {
                var separable = separableModifier.Value;
                var cant_pasadas = cantPasadasModifier.Value;

                if (separable)
                {
                    // 1er pasada: downfilter x 4
                    // -----------------------------------------------------
                    pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                    device.SetRenderTarget(0, pSurf);
                    device.BeginScene();

                    effect.Technique = "DownFilter4";
                    device.VertexFormat = CustomVertex.PositionTextured.Format;
                    device.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget);

                    device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    pSurf.Dispose();

                    device.EndScene();

                    //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
                    // TextureLoader.Save("scene.bmp", ImageFileFormat.Bmp, g_pRenderTarget4);
                    device.DepthStencilSurface = pOldDS;

                    // Pasadas de blur
                    for (var P = 0; P < cant_pasadas; ++P)
                    {
                        // Gaussian blur Horizontal
                        // -----------------------------------------------------
                        pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                        device.SetRenderTarget(0, pSurf);
                        // dibujo el quad pp dicho :
                        device.BeginScene();

                        effect.Technique = "GaussianBlurSeparable";
                        device.VertexFormat = CustomVertex.PositionTextured.Format;
                        device.SetStreamSource(0, g_pVBV3D, 0);
                        effect.SetValue("g_RenderTarget", g_pRenderTarget4);

                        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                        effect.Begin(FX.None);
                        effect.BeginPass(0);
                        device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                        effect.EndPass();
                        effect.End();
                        pSurf.Dispose();

                        device.EndScene();
                        //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
                        //TextureLoader.Save("blurH.bmp", ImageFileFormat.Bmp, g_pRenderTarget4Aux);

                        if (P < cant_pasadas - 1)
                        {
                            pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                            device.SetRenderTarget(0, pSurf);
                            pSurf.Dispose();
                            device.BeginScene();
                        }
                        else
                            // Ultima pasada vertical va sobre la pantalla pp dicha
                            device.SetRenderTarget(0, pOldRT);

                        //  Gaussian blur Vertical
                        // -----------------------------------------------------

                        effect.Technique = "GaussianBlurSeparable";
                        device.VertexFormat = CustomVertex.PositionTextured.Format;
                        device.SetStreamSource(0, g_pVBV3D, 0);
                        effect.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                        effect.Begin(FX.None);
                        effect.BeginPass(1);
                        device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                        effect.EndPass();
                        effect.End();
                        if (P < cant_pasadas - 1)
                        {
                            device.EndScene();
                        }
                        //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
                        //TextureLoader.Save("blurV.bmp", ImageFileFormat.Bmp, g_pRenderTarget4);
                    }
                }
                else
                {
                    // Naive Gaussian blur
                    // restuaro el render target y el stencil
                    device.DepthStencilSurface = pOldDS;
                    device.SetRenderTarget(0, pOldRT);

                    // dibujo el quad pp dicho :
                    device.BeginScene();

                    effect.Technique = "GaussianBlur";
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
                m.Dispose();
            }
            effect.Dispose();
            g_pRenderTarget.Dispose();
            g_pRenderTarget4Aux.Dispose();
            g_pRenderTarget4.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
        }
    }
}