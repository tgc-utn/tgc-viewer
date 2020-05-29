using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Deferred
{
    public class DeferredRendering : TGCExampleViewer
    {
        private Microsoft.DirectX.Direct3D.Effect deferredEffect;

        private TGCBooleanModifier toggle, visualizeRenderTargets;

        private Texture position, baseColor, normals;
        private Surface depth;

        private List<TgcMesh> meshes;

        private List<Light> lights;

        private VertexBuffer fullScreenQuad;

        private Random random;

        private bool lastValue = true;

        private TGCMatrix quadSizes = TGCMatrix.Scaling(0.2f, 0.2f, 0.2f);
        struct Light
        {
            TGCVector3 position;
            TGCVector3 color;

            public Light(TGCVector3 position, TGCVector3 color)
            {
                this.position = position;
                this.color = color;
            }

            public void SetLight(int index, Microsoft.DirectX.Direct3D.Effect effect)
            {
                effect.SetValue("lights[" + index + "].Position", TGCVector3.TGCVector3ToFloat3Array(position));
                effect.SetValue("lights[" + index + "].Color", TGCVector3.TGCVector3ToFloat3Array(color));
            }
        }


        public DeferredRendering(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Deferred Rendering";
            Name = "Deferred Rendering Basico";
            Description = "Ejemplo con multiples luces";
        }

        public override void Init()
        {
            //Cargamos un escenario
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");
            meshes = scene.Meshes;

            InitializeLights();
            InitializeRenderTargets();
            InitializeEffect();
            InitializeFullScreenQuad();

            meshes.ForEach(mesh => mesh.Effect = deferredEffect);
            meshes.ForEach(mesh => mesh.Technique = "Deferred");

            toggle = AddBoolean("Prender/Apagar", "Usar Deferred Rendering", true);
            visualizeRenderTargets = AddBoolean("Visualizar render targets", "Muestra o deja de mostrar los render targets", false);

            FixedTickEnable = true;

            Camera = new TgcFpsCamera(Input);
        }

        float time = 0.0f;
        public override void Update()
        {
            deferredEffect.SetValue("time", time);
            deferredEffect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array( Camera.Position));

            D3DDevice.Instance.Device.RenderState.FillMode = Input.keyDown(Key.J) ? FillMode.WireFrame : FillMode.Solid;

            time += ElapsedTime;
        }


        public override void Render()
        {
            if (toggle.Value != lastValue)
            {
                if (toggle.Value)
                    meshes.ForEach(mesh => mesh.Technique = "Deferred");
                else
                    meshes.ForEach(mesh => mesh.Technique = "Default");
                lastValue = toggle.Value;
            }
            if (!toggle.Value)
            {
                PreRender();
                meshes.ForEach(mesh => mesh.Render());
                PostRender();
            }
            else
            {
                var device = D3DDevice.Instance.Device;

                var defaultRenderTarget = device.GetRenderTarget(0);
                var defaultDepth = device.DepthStencilSurface;

                var positionRenderTarget = position.GetSurfaceLevel(0);
                var normalsRenderTarget = normals.GetSurfaceLevel(0);
                var baseColorRenderTarget = baseColor.GetSurfaceLevel(0);

                #region Pasada MultipleRenderTargets

                // Le decimos a la API que dibuje en nuestras tres texturas
                device.SetRenderTarget(0, positionRenderTarget);
                device.SetRenderTarget(1, normalsRenderTarget);
                device.SetRenderTarget(2, baseColorRenderTarget);

                // Le decimos a la API que dibuje la profundidad en esta superficie
                device.DepthStencilSurface = depth;

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

                device.BeginScene();

                meshes.ForEach(mesh => mesh.Render());

                device.EndScene();

                #endregion

                #region Pasada Integradora

                // Le decimos a la API que dibuje en pantalla, y anulamos los otros render targets
                device.SetRenderTarget(0, defaultRenderTarget);
                device.SetRenderTarget(1, null);
                device.SetRenderTarget(2, null);

                // Le decimos a la API que dibuje la profundidad en la superficie default
                device.DepthStencilSurface = defaultDepth;

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


                device.BeginScene();

                // Asignamos la tecnica para esta pasada
                deferredEffect.Technique = "IntegrateDeferred";

                // Pasamos cada una de nuestras texturas al shader
                deferredEffect.SetValue("positionTexture", position);
                deferredEffect.SetValue("normalTexture", normals);
                deferredEffect.SetValue("baseColorTexture", baseColor);

                // Dibujamos nuestro full screen quad
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, fullScreenQuad, 0);

                deferredEffect.Begin(FX.None);
                deferredEffect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                deferredEffect.EndPass();
                deferredEffect.End();

                if (visualizeRenderTargets.Value)
                {
                    deferredEffect.Technique = "RenderTargets";


                    deferredEffect.SetValue("target", 0);
                    deferredEffect.SetValue("matQuad", (quadSizes * TGCMatrix.Translation(-0.5f, -0.5f, 0f)).ToMatrix());

                    deferredEffect.Begin(FX.None);
                    deferredEffect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    deferredEffect.EndPass();
                    deferredEffect.End();

                    deferredEffect.SetValue("target", 1);
                    deferredEffect.SetValue("matQuad", (quadSizes * TGCMatrix.Translation(0f, -0.5f, 0f)).ToMatrix());

                    deferredEffect.Begin(FX.None);
                    deferredEffect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    deferredEffect.EndPass();
                    deferredEffect.End();

                    deferredEffect.SetValue("target", 2);
                    deferredEffect.SetValue("matQuad", (quadSizes * TGCMatrix.Translation(0.5f, -0.5f, 0f)).ToMatrix());

                    deferredEffect.Begin(FX.None);
                    deferredEffect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    deferredEffect.EndPass();
                    deferredEffect.End();

                }


                RenderAxis();
                RenderFPS();

                device.EndScene();

                device.Present();


                #endregion

                positionRenderTarget.Dispose();
                normalsRenderTarget.Dispose();
                baseColorRenderTarget.Dispose();
            }
        }


        public override void Dispose()
        {
            meshes.ForEach(mesh => mesh.Dispose());
        }


        private void InitializeLights()
        {
            random = new Random();
            lights = new List<Light>();
            for(int index = 0; index < 50; index++)
            {
                var position = new TGCVector3(RandomNumber(-200.0f, 200f), RandomNumber(2f, 50f), RandomNumber(-800f, 800f));
                var color = new TGCVector3(RandomNumber(0.0f, 1f), RandomNumber(0f, 1f), RandomNumber(0f, 1f));
                var light = new Light(position, color);
                lights.Add(light);
            }
        }

        private void InitializeRenderTargets()
        {
            var device = D3DDevice.Instance.Device;

            position = new Texture(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
            baseColor = new Texture(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);
            normals = new Texture(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A16B16G16R16F, Pool.Default);

            depth = device.CreateDepthStencilSurface(device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);
        }

        private void InitializeEffect()
        {
            deferredEffect = TGCShaders.Instance.LoadEffect(ShadersDir + "Deferred.fx");

            deferredEffect.SetValue("lightCount", lights.Count);
            int index = 0;
            lights.ForEach(light =>
            {
                light.SetLight(index, deferredEffect);
                index++;
            });
        }

        private void InitializeFullScreenQuad()
        {
            var device = D3DDevice.Instance.Device;
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            // Vertex buffer de los triangulos
            fullScreenQuad = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            fullScreenQuad.SetData(vertices, 0, LockFlags.None);
        }

        public float RandomNumber(float minimum, float maximum)
        {
            return (float)random.NextDouble() * (maximum - minimum) + minimum;
        }


    }
}
