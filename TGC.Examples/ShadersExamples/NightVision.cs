using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    public class NightVision : TGCExampleViewer
    {
        private readonly int[] bot_status = new int[100];

        private readonly int cant_balas = 100;
        private readonly int cant_pasadas = 3;
        private readonly List<TgcSkeletalMesh> enemigos = new List<TgcSkeletalMesh>();
        private readonly float total_timer_firing = 2f;
        private readonly float vel_bala = 300;
        private readonly float vel_bot = 100;
        private TGCVector3[] dir_bala;
        private Effect effect;
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget, g_pGlowMap, g_pRenderTarget4, g_pRenderTarget4Aux;
        private VertexBuffer g_pVBV3D;
        private List<TgcMesh> meshes;
        private string MyShaderDir;
        private TgcMesh pasto, arbol, arbusto;
        private TGCVector3[] pos_bala;
        private float[] timer_firing;

        public NightVision(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Post Process Shaders";
            Name = "NightVision";
            Description = "NightVision Effect";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Cargamos un escenario
            var loader = new TgcSceneLoader();

            //TgcScene scene = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Selva\\Selva-TgcScene.xml");
            meshes = scene.Meshes;
            var scene2 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml");
            pasto = scene2.Meshes[0];
            pasto.AutoTransform = true;
            var scene3 =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico\\ArbolSelvatico-TgcScene.xml");
            arbol = scene3.Meshes[0];
            arbol.AutoTransform = true;
            var scene4 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Arbusto2\\Arbusto2-TgcScene.xml");
            arbusto = scene4.Meshes[0];
            arbusto.AutoTransform = true;

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            var rnd = new Random();
            for (var t = 0; t < 25; ++t)
            {
                enemigos.Add(skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\" + "CombineSoldier-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\BasicHuman\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Run-TgcSkeletalAnim.xml"
                    }));

                //Configurar animacion inicial
                enemigos[t].playAnimation("StandBy", true);
                enemigos[t].Position = new TGCVector3(-rnd.Next(0, 1500) - 250, 0, -rnd.Next(0, 1500) - 250);
                enemigos[t].Scale = new TGCVector3(2f, 2f, 2f);
                enemigos[t].UpdateMeshTransform();
                bot_status[t] = 0;
            }

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(D3DDevice.Instance.Device, MyShaderDir + "GaussianBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Camara en primera personas
            Camara = new TgcFpsCamera(new TGCVector3(-1000, 250, -1000), 1000f, 600f, Input);

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            g_pGlowMap = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
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

            timer_firing = new float[100];
            pos_bala = new TGCVector3[100];
            dir_bala = new TGCVector3[100];

            for (var i = 0; i < cant_balas; ++i)
            {
                timer_firing[i] = i / (float)cant_balas * total_timer_firing;
            }
        }

        public override void Update()
        {
            PreUpdate();

            var pos = Camara.Position;
            if (pos.X < -2000 || pos.Z < -2000 || pos.X > 0 || pos.Z > 0)
            {
                // reset pos camara
                Camara.SetCamera(new TGCVector3(-1000, 250, -1000), new TGCVector3(0, 0, -1));
            }

            //Activar animacion de caminando
            var t = 0;
            foreach (var m in enemigos)
            {
                var dir_escape = m.Position - pos;
                dir_escape.Y = 0;
                var dist = dir_escape.Length();

                if (Math.Abs(dist) < 10)
                {
                    // lo alcance, lo mato
                    bot_status[t] = 99;
                }
                else
                    switch (bot_status[t])
                    {
                        // escondido
                        case 0:
                            if (dist < 400)
                                // me escapo
                                bot_status[t] = 1;
                            break;

                        // escapando
                        case 1:
                            if (dist > 1000)
                                // me esconde
                                bot_status[t] = 0;
                            break;

                        // perseguir
                        case 2:
                            break;
                    }

                switch (bot_status[t])
                {
                    // escondido
                    case 0:
                        m.playAnimation("StandBy", true);
                        break;

                    // escapando
                    case 1:
                        dir_escape.Normalize();
                        m.RotateY((float)Math.Atan2(dir_escape.X, dir_escape.Z) - m.Rotation.Y + 3.1415f);
                        m.Move(dir_escape * (vel_bot * ElapsedTime));
                        var X = m.Position.X;
                        var Z = m.Position.Z;
                        if (X < -2000)
                            X = -1000;
                        if (X > 0)
                            X = -1000;
                        if (Z < -2000)
                            Z = -1000;
                        if (Z > 0)
                            Z = -1000;
                        m.Position = new TGCVector3(X, m.Position.Y, Z);
                        m.playAnimation("Run", true, 20);
                        break;

                    // persiguiendo
                    case 2:
                        dir_escape.Normalize();
                        if (Math.Abs(dir_escape.Z) > 0.01f)
                        {
                            m.RotateY((float)Math.Atan2(dir_escape.X, dir_escape.Z) - m.Rotation.Y);
                            m.Move(dir_escape * (-60 * ElapsedTime));
                        }
                        m.playAnimation("Run", true, 20);
                        break;

                    case 99:
                        m.RotateZ(3.1415f * 0.5f - m.Rotation.Z);
                        m.playAnimation("StandBy", true);
                        break;
                }
                m.updateAnimation(ElapsedTime);
                ++t;
            }

            var rnd = new Random();
            for (var i = 0; i < cant_balas; ++i)
            {
                timer_firing[i] -= ElapsedTime;
                if (timer_firing[i] < 0)
                {
                    timer_firing[i] += total_timer_firing;
                    pos_bala[i] = pos + new TGCVector3(rnd.Next(-10, 10), rnd.Next(-10, 10), rnd.Next(-10, 10));
                    dir_bala[i] = Camara.LookAt - pos;
                    dir_bala[i].Normalize();
                }
                else
                {
                    pos_bala[i] = pos_bala[i] + dir_bala[i] * (vel_bala * ElapsedTime);
                }
            }

            PostUpdate();
        }

        public override void Render()
        {
            ClearTextures();

            if ((bool)Modifiers["activar_efecto"])
                renderConEfectos(ElapsedTime);
            else
                renderSinEfectos(ElapsedTime);

            D3DDevice.Instance.Device.BeginScene();
            RenderFPS();
            RenderAxis();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public void renderSinEfectos(float elapsedTime)
        {
            var device = D3DDevice.Instance.Device;

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene(elapsedTime, "DefaultTechnique");
            //Render personames enemigos
            foreach (var m in enemigos)
            {
                m.UpdateMeshTransform();//Transformacion default
                m.Render();
            }

            DrawText.drawText("Pos: " + Camara.Position, 5, 20, Color.Yellow);

            device.EndScene();
        }

        public void renderConEfectos(float elapsedTime)
        {
            var device = D3DDevice.Instance.Device;

            // dibujo la escena una textura
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = device.GetRenderTarget(0);
            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderScene(elapsedTime, "DefaultTechnique");
            //Render personames enemigos
            foreach (var m in enemigos)
            {
                m.UpdateMeshTransform();
                m.Render();
            }

            device.EndScene();

            pSurf.Dispose();

            // dibujo el glow map
            effect.Technique = "DefaultTechnique";
            pSurf = g_pGlowMap.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();

            //Dibujamos SOLO los meshes que tienen glow brillantes
            //Render personaje brillante
            //Render personames enemigos
            foreach (var m in enemigos)
            {
                m.UpdateMeshTransform();
                m.Render();
            }

            if (Input.keyDown(Key.F))
                for (var i = 0; i < cant_balas; ++i)
                    if (timer_firing[i] > 0)
                    {
                        var bala = new TgcArrow();
                        bala.PStart = pos_bala[i];
                        bala.PEnd = pos_bala[i] + dir_bala[i] * 3;
                        bala.Thickness = 0.05f;
                        bala.HeadSize = new TGCVector2(0.01f, 0.01f);
                        bala.Effect = effect;
                        bala.Technique = "DefaultTechnique";
                        bala.updateValues();
                        bala.Render();
                    }

            // El resto opacos
            renderScene(elapsedTime, "DibujarObjetosOscuros");

            device.EndScene();

            pSurf.Dispose();

            // Hago un blur sobre el glow map
            // 1er pasada: downfilter x 4
            // -----------------------------------------------------
            pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);

            device.BeginScene();
            effect.Technique = "DownFilter4";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pGlowMap);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            pSurf.Dispose();

            device.EndScene();

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

                pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                pSurf.Dispose();

                //  Gaussian blur Vertical
                // -----------------------------------------------------

                device.BeginScene();
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

                device.EndScene();
            }

            //  To Gray Scale
            // -----------------------------------------------------
            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.SetRenderTarget(0, pOldRT);
            //pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
            //device.SetRenderTarget(0, pSurf);

            device.BeginScene();

            effect.Technique = "GrayScale";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_GlowMap", g_pRenderTarget4Aux);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            device.EndScene();
        }

        public void renderScene(float elapsedTime, string Technique)
        {
            //Dibujamos todos los meshes del escenario

            /*foreach (TgcMesh m in meshes)
            {
                m.Effect = effect;
                m.Technique = Technique;
                m.UpdateMeshTransform();
                m.render();
            }*/

            var rnd = new Random(1);
            pasto.Effect = effect;
            pasto.Technique = Technique;
            for (var i = 0; i < 10; ++i)
                for (var j = 0; j < 10; ++j)
                {
                    pasto.Position = new TGCVector3(-i * 200 + rnd.Next(0, 50), 0, -j * 200 + rnd.Next(0, 50));
                    pasto.Scale = new TGCVector3(3, 4 + rnd.Next(0, 4), 5);
                    pasto.UpdateMeshTransform();
                    //pasto.Transform = TGCMatrix.Identity*TGCMatrix.Scaling(3, 4 + rnd.Next(0, 4), 5) * TGCMatrix.Translation(-i * 200 + rnd.Next(0, 50), 0, -j * 200 + rnd.Next(0, 50));
                    pasto.Render();
                }

            arbusto.Effect = effect;
            arbusto.Technique = Technique;
            for (var i = 0; i < 5; ++i)
                for (var j = 0; j < 5; ++j)
                {
                    arbusto.Position = new TGCVector3(-i * 400 + rnd.Next(0, 50), 0, -j * 400 + rnd.Next(0, 50));
                    //arbusto.Transform = TGCMatrix.Identity*TGCMatrix.Translation(-i * 400 + rnd.Next(0, 50), 0, -j * 400 + rnd.Next(0, 50));
                    arbusto.UpdateMeshTransform();
                    arbusto.Render();
                }

            arbol.Effect = effect;
            arbol.Technique = Technique;
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                {
                    arbol.Position = new TGCVector3(-i * 700 + rnd.Next(0, 50), 0, -j * 700 + rnd.Next(0, 50));
                    //arbol.Transform = TGCMatrix.Identity*TGCMatrix.Translation(-i * 700 + rnd.Next(0, 50), 0, -j * 700 + rnd.Next(0, 50));
                    arbol.UpdateMeshTransform();
                    arbol.Render();
                }
        }

        public override void Dispose()
        {
            foreach (var m in meshes)
            {
                m.Dispose();
            }
            effect.Dispose();
            pasto.Dispose();
            arbol.Dispose();
            arbusto.Dispose();
            g_pRenderTarget.Dispose();
            g_pGlowMap.Dispose();
            g_pRenderTarget4Aux.Dispose();
            g_pRenderTarget4.Dispose();
            g_pVBV3D.Dispose();
            g_pDepthStencil.Dispose();
            foreach (var m in enemigos)
                m.Dispose();
        }
    }
}