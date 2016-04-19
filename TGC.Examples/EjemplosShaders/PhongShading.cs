using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;
using TGC.Util;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.EjemplosShaders
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
    public class PhongShading : TgcExample
    {
        private Effect effect;
        private TgcBox lightBox;
        private TgcMesh mesh;
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene;
        private Viewport View1, View2, View3, ViewF;
        private bool vista_unica = true;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-PhongShading";
        }

        public override string getDescription()
        {
            return "Ejemplo trivial de iluminaci�n por Pixel. [BARRA] -> Cambia de vista �nica a 3 vistas";
        }

        public override void init()
        {
            MyMediaDir = GuiController.Instance.ExamplesMediaDir+ "WorkshopShaders\\";
            MyShaderDir = GuiController.Instance.ShadersDir+"WorkshopShaders\\";

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

            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100, -100, -100),
                new Vector3(100, 100, 100), new Vector3(0, 40, 0));
            GuiController.Instance.Modifiers.addFloat("Ambient", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("Diffuse", 0, 1, 0.6f);
            GuiController.Instance.Modifiers.addFloat("Specular", 0, 1, 0.5f);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);

            //Crear caja para indicar ubicacion de la luz
            lightBox = TgcBox.fromSize(new Vector3(5, 5, 5), Color.Yellow);

            // Creo 3 viewport, para mostrar una comparativa entre los metodos de iluminacion
            GuiController.Instance.RotCamera.setCamera(new Vector3(20, 20, 0), 200);
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

        public override void render(float elapsedTime)
        {
            var panel3d = GuiController.Instance.Panel3d;
            var aspectRatio = panel3d.Width / (float)panel3d.Height;

            if (GuiController.Instance.D3dInput.keyPressed(Key.Space))
                vista_unica = !vista_unica;

            var lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition",
                TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);

            //Mover mesh que representa la luz
            lightBox.Position = lightPosition;

            if (vista_unica)
            {
                // solo una vista
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                D3DDevice.Instance.Device.Viewport = ViewF;
                foreach (var m in scene.Meshes)
                {
                    m.Effect = effect;
                    m.Technique = "DefaultTechnique";
                    m.render();
                }
                lightBox.render();
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
                    m.render();
                }
                lightBox.render();

                // 2- vista: fixed pipeline con iluminacion dinamica
                D3DDevice.Instance.Device.Viewport = View2;
                D3DDevice.Instance.Device.SetRenderState(RenderStates.Lighting, true);
                D3DDevice.Instance.Device.SetRenderState(RenderStates.SpecularEnable, true);
                D3DDevice.Instance.Device.Lights[0].Position = lightPosition;
                D3DDevice.Instance.Device.Lights[0].Update();

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (var m in scene.Meshes)
                {
                    m.Technique = "DefaultTechnique";
                    m.render();
                }

                lightBox.render();

                // 3- vista: fixed pipeline con iluminacion estatica
                D3DDevice.Instance.Device.Viewport = View3;
                D3DDevice.Instance.Device.SetRenderState(RenderStates.Lighting, false);
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (var m in scene.Meshes)
                {
                    m.Technique = "DefaultTechnique";
                    m.render();
                }

                lightBox.render();
            }
        }

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            lightBox.dispose();
        }
    }
}