using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Shaders
{
    /// <summary>
    /// Ejemplo EjemploPhongShading:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/EjemploShaderTgcMesh".
    /// Muestra como extender utilizar un Shader para lograr iluminación dinámica del tipo Phong-Shading.
    /// El ejemplo permite modificar los parámetros de iluminación para ver como afectan sobre el objeto
    /// en tiempo real.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploPhongShading: TgcExample
    {

        TgcMeshShader mesh;
        TgcBox lightBox;


        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "PhongShading";
        }

        public override string getDescription()
        {
            return "PhongShading";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Configurar MeshFactory customizado
            loader.MeshFactory = new CustomMeshShaderFactory();

            //Cargar mesh
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Olla\\Olla-TgcScene.xml");
            mesh = (TgcMeshShader)scene.Meshes[0];

            //Cargar Shader de PhonhShading
            string compilationErrors;
            mesh.Effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (mesh.Effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            mesh.Effect.Technique = "DefaultTechnique";

            //Agregar evento para cargar valores de shader
            mesh.ShaderBegin += new TgcMeshShader.ShaderBeginHandler(mesh_ShaderBegin);


            //Modifier para variables de shader
            GuiController.Instance.Modifiers.addVertex3f("LightPosition", new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000), new Vector3(0, 400, 0));
            GuiController.Instance.Modifiers.addColor("AmbientColor", Color.White);
            GuiController.Instance.Modifiers.addColor("DiffuseColor", Color.Green);
            GuiController.Instance.Modifiers.addColor("SpecularColor", Color.Red);
            GuiController.Instance.Modifiers.addFloat("SpecularPower", 1, 100, 16);
            GuiController.Instance.Modifiers.addVertex3f("MeshPos", new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000), new Vector3(0, 0, 0));
            

            //Crear caja para indicar ubicacion de la luz
            lightBox = TgcBox.fromSize(new Vector3(50, 50, 50), Color.Yellow);
            

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            Vector3 lightPosition = (Vector3)GuiController.Instance.Modifiers["LightPosition"];

            //Cargar variables de shader globales a todos los objetos
            mesh.Effect.SetValue("fvLightPosition", TgcParserUtils.vector3ToFloat3Array(lightPosition));
            mesh.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            mesh.Effect.SetValue("fvAmbient", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["AmbientColor"]));
            mesh.Effect.SetValue("fvDiffuse", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["DiffuseColor"]));
            mesh.Effect.SetValue("fvSpecular", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["SpecularColor"]));
            mesh.Effect.SetValue("fSpecularPower", (float)GuiController.Instance.Modifiers["SpecularPower"]);
            
            //Mover mesh que representa la luz
            lightBox.Position = lightPosition;

            //Mover mesh
            Vector3 meshPos = (Vector3)GuiController.Instance.Modifiers["MeshPos"];
            mesh.Position = meshPos;

            mesh.render();
            lightBox.render();
              
        }


        /// <summary>
        /// Evento que se llama antes de ejecutar el shader de un mesh particular.
        /// Ideal para cargar parametros de shader propios de cada mesh
        /// </summary>
        public void mesh_ShaderBegin(TgcMeshShader mesh)
        {
            //nada en este ejemplo
        }


        public override void close()
        {
            mesh.dispose();
            mesh.Effect.Dispose();
            lightBox.dispose();
        }

    }

    

}
