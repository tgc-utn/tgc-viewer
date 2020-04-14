using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    public class Bloom : TGCExampleViewer
    {
        private TgcMesh trafficLight;
        private TgcMesh plane;
        private Effect effect;
        private Texture glowyObjectsFrameBuffer, bloomHorizontalFrameBuffer, bloomVerticalFrameBuffer, sceneFrameBuffer;
        private Surface depthStencil;
        private TgcRotationalCamera rotational;

        private TGCBooleanModifier bloomModifier;
        private TGCBooleanModifier sceneModifier;
        private TGCBooleanModifier toneMappingModifier;
        private TGCIntModifier passesModifier;

        private CustomVertex.PositionTextured[] fullScreenQuadVertices =
        {
            new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
            new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
            new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
            new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
        };
        VertexBuffer fullScreenQuad;

        private float timer;

        private struct Light
        {
            public bool Enabled;
            public TGCVector3 Position;
            public TGCVector3 SpecularColor;
            public TGCVector3 DiffuseColor;
            public TGCVector3 AmbientColor;

            public void SetLight(int index, Effect effect)
            {
                effect.SetValue("lights[" + index + "].Position", TGCVector3.TGCVector3ToFloat3Array(Position));
                effect.SetValue("lights[" + index + "].SpecularColor", TGCVector3.TGCVector3ToFloat3Array(SpecularColor));
                effect.SetValue("lights[" + index + "].DiffuseColor", TGCVector3.TGCVector3ToFloat3Array(DiffuseColor));
                effect.SetValue("lights[" + index + "].AmbientColor", TGCVector3.TGCVector3ToFloat3Array(AmbientColor));
            }
        }

        private Light[] lights =
        {
            // Red
            new Light
            {
                Enabled = true,
                Position = new TGCVector3(0f, 60f, 15f),
                AmbientColor = new TGCVector3(0.2f, 0.1f, 0.1f),
                DiffuseColor = new TGCVector3(1f, 0.0f, 0.0f),
                SpecularColor = new TGCVector3(0.9f, 0.8f, 0.8f),
            },
            // Yellow
            new Light
            {
                Enabled = true,
                Position = new TGCVector3(0f, 54f, 15f),
                AmbientColor = new TGCVector3(0.2f, 0.2f, 0.05f),
                DiffuseColor = new TGCVector3(1f, 1f, 0f),
                SpecularColor = new TGCVector3(0.85f, 0.85f, 0.65f)
            },
            // Green
            new Light
            {
                Enabled = true,
                Position = new TGCVector3(0f, 48f, 15f),
                AmbientColor = new TGCVector3(0.05f, 0.2f, 0.05f),
                DiffuseColor = new TGCVector3(0f, 1f, 0f),
                SpecularColor = new TGCVector3(0.65f, 0.9f, 0.65f)
            }
    };


        public Bloom(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Post Process Shaders";
            Name = "Bloom";
            Description = "Graba lo luminoso en un Render Target, graba la escena en otro Render Target y luego aplica Bloom.";
        }

        public override void Init()
        {
            BackgroundColor = Color.Black;

            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "Bloom.fx");
            effect.SetValue("screen_dx", D3DDevice.Instance.Device.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", D3DDevice.Instance.Device.PresentationParameters.BackBufferHeight);

            InitializeTrafficLight();
            InitializeFloor();
            InitializeFrameBuffers();
            InitializeModifiers();
            CreateFullScreenQuad();

            timer = 0.0f;

            rotational = new TgcRotationalCamera(new TGCVector3(20, 20, 0), 300, TgcRotationalCamera.DEFAULT_ZOOM_FACTOR * 0.5f, TgcRotationalCamera.DEFAULT_ROTATION_SPEED, Input);
            Camara = rotational;
        }


        public override void Update()
        {
            PreUpdate();
            rotational.UpdateCamera(ElapsedTime);

            UpdateTrafficLight();

            effect.SetValue("scene", sceneModifier.Value);
            effect.SetValue("bloom", bloomModifier.Value);
            effect.SetValue("toneMapping", toneMappingModifier.Value);
            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(Camara.Position));

            PostUpdate();
        }

        public override void Render()
        {
            //PreRender();

            var device = D3DDevice.Instance.Device;

            var screenSurface = device.GetRenderTarget(0);
            var originalDepthStencil = device.DepthStencilSurface;

            // --------------
            // Guardamos la escena en un framebuffer

            BeginScene(device, sceneFrameBuffer.GetSurfaceLevel(0), depthStencil);

            ConfigureBlinnForGround();
            plane.Render();
            ConfigureBlinnForTrafficLight();
            trafficLight.Technique = "Blinn";
            trafficLight.Render();

            EndScene(device);

            // --------------
            // Guardamos lo brillante de la escena en un framebuffer

            BeginScene(device, glowyObjectsFrameBuffer.GetSurfaceLevel(0), depthStencil);

            trafficLight.Technique = "GlowyObjects";
            trafficLight.Render();

            EndScene(device);

            // --------------
            // Aplicamos una pasada de blur horizontal al framebuffer de los objetos brillantes
            // y una pasada vertical
            var passBuffer = glowyObjectsFrameBuffer;
            for (int index = 0; index < passesModifier.Value; index++)
            {
                BeginScene(device, bloomHorizontalFrameBuffer.GetSurfaceLevel(0), depthStencil);

                effect.Technique = "HorizontalBlur";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("glowyFrameBuffer", passBuffer);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                EndScene(device);


                BeginScene(device, bloomVerticalFrameBuffer.GetSurfaceLevel(0), depthStencil);

                effect.Technique = "VerticalBlur";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);
                effect.SetValue("glowyFrameBuffer", bloomHorizontalFrameBuffer);

                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                EndScene(device);

                passBuffer = bloomVerticalFrameBuffer;
            }

            // --------------
            // Este postprocesado integra lo blureado a la escena

            BeginScene(device, screenSurface, originalDepthStencil);

            effect.Technique = "Integrate";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, fullScreenQuad, 0);
            effect.SetValue("verticalBlurFrameBuffer", bloomVerticalFrameBuffer);
            effect.SetValue("sceneFrameBuffer", sceneFrameBuffer);

            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            EndScene(device);
            device.Present();
        }

        private void BeginScene(Device device, Surface surface, Surface depth)
        {
            device.SetRenderTarget(0, surface);
            device.DepthStencilSurface = depth;

            device.BeginScene();
            
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
        }

        private void EndScene(Device device)
        {
            device.EndScene();
        }



        public override void Dispose()
        {
            trafficLight.Dispose();
            depthStencil.Dispose();
            plane.Dispose();
            effect.Dispose();
            glowyObjectsFrameBuffer.Dispose();
        }

        private void InitializeTrafficLight()
        {
            var sceneLoader = new TgcSceneLoader();
            var scene = sceneLoader.loadSceneFromFile(MediaDir + "ModelosTgc//Semaforo//Semaforo-TgcScene.xml", MediaDir + "ModelosTgc//Semaforo//");
            trafficLight = scene.Meshes[0];
            var size = trafficLight.BoundingBox.calculateSize();
            trafficLight.Transform = TGCMatrix.RotationY(FastMath.PI) * TGCMatrix.Translation(0f, size.Y / 2.0f, 0f);
            trafficLight.Effect = effect;
            trafficLight.Technique = "Blinn";
        }

        private void InitializeFloor()
        {
            var floor = TgcTexture.createTexture(MediaDir + "Texturas\\tierra.jpg");
            var planeSize = new TGCVector3(200f, 0f, 200f);
            var planeMesh = new TgcPlane(TGCVector3.Scale(planeSize, -0.5f), planeSize, TgcPlane.Orientations.XZplane, floor, 5f, 5f);
            planeMesh.updateValues();

            plane = planeMesh.toMesh("floor");
            plane.Transform = TGCMatrix.RotationX(FastMath.PI) * plane.Transform;
            plane.Effect = effect;
            plane.Technique = "Blinn";
        }

        private void InitializeFrameBuffers()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            depthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            glowyObjectsFrameBuffer = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            bloomHorizontalFrameBuffer = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            bloomVerticalFrameBuffer = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            sceneFrameBuffer = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
        }


        private void ConfigureBlinnForGround()
        {
            effect.SetValue("KAmbient", 0.3f);
            effect.SetValue("KDiffuse", 0.5f);
            effect.SetValue("KSpecular", 0.25f);
            effect.SetValue("shininess", 3.0f);
        }
        private void ConfigureBlinnForTrafficLight()
        {
            effect.SetValue("KAmbient", 0.3f);
            effect.SetValue("KDiffuse", 0.6f);
            effect.SetValue("KSpecular", 0.5f);
            effect.SetValue("shininess", 10f);
        }

        private void UpdateTrafficLight()
        {
            float phase = timer % 10f;
            if (phase < 4.0f)
            {
                lights[0].Enabled = true;
                lights[1].Enabled = false;
                lights[2].Enabled = false;
            }
            else if (phase < 5.0f)
            {
                lights[0].Enabled = true;
                lights[1].Enabled = true;
                lights[2].Enabled = false;
            }
            else if (phase < 9.0f)
            {
                lights[0].Enabled = false;
                lights[1].Enabled = false;
                lights[2].Enabled = true;
            }
            else
            {
                lights[0].Enabled = false;
                lights[1].Enabled = true;
                lights[2].Enabled = false;
            }

            timer += ElapsedTime;

            var enabledLights = new List<Light>(lights).FindAll(light => light.Enabled);
            int index = 0;
            enabledLights.ForEach(light => light.SetLight(index++, effect));
            effect.SetValue("lightCount", enabledLights.Count);
        }

        private void CreateFullScreenQuad()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            fullScreenQuad = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            fullScreenQuad.SetData(fullScreenQuadVertices, 0, LockFlags.None);
        }

        private void InitializeModifiers()
        {
            sceneModifier = AddBoolean("Scene", "Render scene", true);
            toneMappingModifier = AddBoolean("ToneMapping", "ToneMapping", true);
            bloomModifier = AddBoolean("Bloom", "Add bloom", true);
            passesModifier = AddInt("Cantidad de pasadas de blur", 1, 30, 10);
        }

    }


}
