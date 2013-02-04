using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;

namespace Examples.Shaders.WorkshopShaders
{

    /// <summary>
    /// Ejemplo PhongShading:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/WorkshopShaders/BasicShader".
    /// El phongShading es el hola mundo de los shaders
    /// Aplica el mismo algoritmo de iluminacion standard del directX
    /// pero por pixel. Es decir que la iluminacion se calcula pixel por pixel
    /// a diferencia del fixed pipeline, en la cual se calcula solo en los vertices
    /// y el color del pixel se obtiene interpolando.
    /// 
    /// Autor: Mariano Banquiero
    /// 
    /// </summary>
    public class PhongShading: TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        MyMesh mesh;
        Effect effect;
        TgcBox lightBox;
        Viewport View1,View2,View3,ViewF;
        bool vista_unica = true;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new MyCustomMeshFactory();

            // Cargo la escena del cornell box.
            scene = loader.loadSceneFromFile(MyMediaDir + "cornell_box\\cornell_box-TgcScene.xml");

            mesh = (MyMesh)scene.Meshes[0];

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir  + "PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            // Pasos standard: 
            // configurar la tecnica 
            effect.Technique = "DefaultTechnique";
            // le asigno el efecto a la malla 
            mesh.effect = effect;

            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 40, 0));
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

            ViewF = d3dDevice.Viewport;

            // Creo la luz para el fixed pipeline
            d3dDevice.Lights[0].Type = LightType.Point;
            d3dDevice.Lights[0].Diffuse = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Specular = Color.FromArgb(255, 255, 255, 255);
            d3dDevice.Lights[0].Attenuation0 = 0.0f;
            d3dDevice.Lights[0].Range = 50000.0f;
            d3dDevice.Lights[0].Enabled = true;

            
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space ))
                vista_unica = !vista_unica;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            effect.Technique = "DefaultTechnique";
            
            //Cargar variables de shader
            effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            effect.SetValue("k_la", (float)GuiController.Instance.Modifiers["Ambient"]);
            effect.SetValue("k_ld", (float)GuiController.Instance.Modifiers["Diffuse"]);
            effect.SetValue("k_ls", (float)GuiController.Instance.Modifiers["Specular"]);
            effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);

            //Mover mesh que representa la luz
            lightBox.Position = lightPosition;

            if (vista_unica)
            {
                // solo una vista
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.Viewport = ViewF;
                foreach (MyMesh m in scene.Meshes)
                {
                    m.effect = effect;
                    m.render();
                }
                lightBox.render();
            }
            else
            {
                // 3 vistas:
                // 1- vista: usando el shader
                device.Viewport = View1;
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (MyMesh m in scene.Meshes)
                {
                    m.effect = effect;
                    m.render();
                }
                lightBox.render();

                // 2- vista: fixed pipeline con iluminacion dinamica
                device.Viewport = View2;
                device.SetRenderState(RenderStates.Lighting, true);
                device.SetRenderState(RenderStates.SpecularEnable, true);
                device.Lights[0].Position = lightPosition;
                device.Lights[0].Update();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (TgcMesh m in scene.Meshes)
                    m.render();
                lightBox.render();


                // 3- vista: fixed pipeline con iluminacion estatica
                device.Viewport = View3;
                device.SetRenderState(RenderStates.Lighting, false);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                foreach (TgcMesh m in scene.Meshes)
                    m.render();
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
