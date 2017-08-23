using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo PhongShading:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    ///     El phongShading es el hola mundo de los shaders
    ///     Aplica el mismo algoritmo de iluminacion standard del directX
    ///     pero por pixel. Es decir que la iluminacion se calcula pixel por pixel
    ///     a diferencia del fixed pipeline, en la cual se calcula solo en los vertices
    ///     y el color del pixel se obtiene interpolando.
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class PhongShading : TGCExampleViewer
    {
        private Effect effect;
        private TGCBox lightBox;
        private TgcMesh mesh;
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene;
        private Viewport View1, View2, View3, ViewF;

        public PhongShading(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Pixel Shaders";
            Name = "Phong Shading Custom con ViewPorts";
            Description = "Ejemplo trivial de iluminación por Pixel. [BARRA] -> Cambia de vista única a 3 vistas";
        }

        public override void Init()
        {
            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Crear loader
            var loader = new TgcSceneLoader();

            // Cargo la escena del cornell box.
            scene = loader.loadSceneFromFile(MyMediaDir + "cornell_box\\cornell_box-TgcScene.xml");

            mesh = scene.Meshes[0];

            //Cargar Shader personalizado
            effect =
                TgcShaders.loadEffect(MyShaderDir + "PhongShading.fx");

            // Pasos standard:
            // le asigno el efecto a la malla
            mesh.Effect = effect;
            mesh.Technique = "DefaultTechnique";

            Modifiers.addBoolean("viewports", "See Viewports", false);
            Modifiers.addVertex3f("LightPosition", new TGCVector3(-100, -100, -100),
                new TGCVector3(100, 100, 100), new TGCVector3(0, 40, 0));
            Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            Modifiers.addFloat("Specular", 0, 1, 0.5f);
            Modifiers.addFloat("SpecularPower", 1, 100, 16);

            //Crear caja para indicar ubicacion de la luz
            lightBox = TGCBox.fromSize(new TGCVector3(5, 5, 5), Color.Yellow);
            lightBox.AutoTransform = true;

            // Creo 3 viewport, para mostrar una comparativa entre los metodos de iluminacion

            Camara = new TgcRotationalCamera(new TGCVector3(20, 20, 0), 200, Input);

            View1 = new Viewport();
            View1.X = 0;
            View1.Y = 0;
            View1.Width = 400;
            View1.Height = 250;
            View1.MinZ = 0;
            View1.MaxZ = 1;

            View2 = new Viewport();
            View2.X = 0;
            View2.Y = 250;
            View2.Width = 400;
            View2.Height = 250;
            View2.MinZ = 0;
            View2.MaxZ = 1;

            View3 = new Viewport();
            View3.X = 400;
            View3.Y = 0;
            View3.Width = 400;
            View3.Height = 250;
            View3.MinZ = 0;
            View3.MaxZ = 1;

            ViewF = D3DDevice.Instance.Device.Viewport;

            // Creo la luz para el fixed pipeline
            D3DDevice.Instance.Device.Lights[0].Type = LightType.Point;
            D3DDevice.Instance.Device.Lights[0].Diffuse = Color.FromArgb(255, 255, 255, 255);
            D3DDevice.Instance.Device.Lights[0].Specular = Color.FromArgb(255, 255, 255, 255);
            D3DDevice.Instance.Device.Lights[0].Attenuation0 = 0.0f;
            D3DDevice.Instance.Device.Lights[0].Range = 50000.0f;
            D3DDevice.Instance.Device.Lights[0].Enabled = true;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            ClearTextures();

            D3DDevice.Instance.Device.BeginScene();

            var lightPosition = (TGCVector3)Modifiers["LightPosition"];

            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TGCVector3.Vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition", TGCVector3.Vector3ToFloat3Array(Camara.Position));
            effect.SetValue("k_la", (float)Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)Modifiers["Specular"]);
            effect.SetValue("fSpecularPower", (float)Modifiers["SpecularPower"]);

            //Mover mesh que representa la luz
            lightBox.Position = lightPosition;

            if (!(bool)Modifiers["viewports"])
            {
                // solo una vista
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                D3DDevice.Instance.Device.Viewport = ViewF;
                foreach (var m in scene.Meshes)
                {
                    m.Effect = effect;
                    m.Technique = "DefaultTechnique";
                    m.UpdateMeshTransform();
                    m.Render();
                }
                lightBox.Render();
            }
            else
            {
                // 3 vistas:
                // 1- vista: usando el shader
                D3DDevice.Instance.Device.Viewport = View1;
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (var m in scene.Meshes)
                {
                    m.Effect = effect;
                    m.Technique = "DefaultTechnique";
                    m.UpdateMeshTransform();
                    m.Render();
                }
                lightBox.Render();

                // 2- vista: fixed pipeline con iluminacion dinamica
                D3DDevice.Instance.Device.Viewport = View2;
                D3DDevice.Instance.Device.SetRenderState(RenderStates.Lighting, true);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.SpecularEnable, true);
                D3DDevice.Instance.Device.Lights[0].Position = lightPosition;
                D3DDevice.Instance.Device.Lights[0].Update();

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (var m in scene.Meshes)
                {
                    m.Effect = TgcShaders.Instance.TgcMeshShader;
                    m.RenderType = TgcMesh.MeshRenderType.DIFFUSE_MAP;
                    m.Technique = TgcShaders.Instance.getTgcMeshTechnique(m.RenderType);
                    m.UpdateMeshTransform();
                    m.Render();
                }

                lightBox.Render();

                // 3- vista: fixed pipeline con iluminacion estatica
                D3DDevice.Instance.Device.Viewport = View3;
                D3DDevice.Instance.Device.SetRenderState(RenderStates.Lighting, false);
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (var m in scene.Meshes)
                {
                    m.Effect = TgcShaders.Instance.TgcMeshShader;
                    m.RenderType = TgcMesh.MeshRenderType.DIFFUSE_MAP;
                    m.Technique = TgcShaders.Instance.getTgcMeshTechnique(m.RenderType);
                    m.UpdateMeshTransform();
                    m.Render();
                }

                lightBox.Render();
            }
            RenderFPS();
            RenderAxis();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public override void Dispose()
        {
            effect.Dispose();
            scene.DisposeAll();
            lightBox.Dispose();

            D3DDevice.Instance.Device.Viewport = ViewF;
        }
    }
}