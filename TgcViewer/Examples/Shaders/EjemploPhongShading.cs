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
    /// Ejemplo avanzado. Ver primero ejemplo "Shaders/EjemploShaderTgcMesh".
    /// Muestra como utilizar un Shader para lograr iluminación dinámica del tipo Phong-Shading.
    /// El ejemplo permite modificar los parámetros de iluminación para ver como afectan sobre el objeto
    /// en tiempo real.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploPhongShading: TgcExample
    {

        TgcMesh mesh;
        TgcBox lightMesh;


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
            return "Muestra como utilizar un Shader para lograr iluminación dinámica del tipo Phong-Shading.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Cargar mesh
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Olla\\Olla-TgcScene.xml");
            mesh = scene.Meshes[0];


            //Crear caja para indicar ubicacion de la luz
            lightMesh = TgcBox.fromSize(new Vector3(20, 20, 20), Color.Yellow);


            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-500, -500, -500), new Vector3(500, 800, 500), new Vector3(0, 500, 0));
            GuiController.Instance.Modifiers.addColor("ambient", Color.Gray);
            GuiController.Instance.Modifiers.addColor("diffuse", Color.Blue);
            GuiController.Instance.Modifiers.addColor("specular", Color.White);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 40, 20f);

            
            

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Habilitar luz
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PhongShading
                currentShader = GuiController.Instance.Shaders.TgcMeshPhongShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = GuiController.Instance.Shaders.TgcMeshShader;
            }


            //Aplicar al mesh el shader actual
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);


            //Actualzar posición de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            if (lightEnable)
            {

                //Cargar variables shader
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                mesh.Effect.SetValue("ambientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["ambient"]));
                mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["diffuse"]));
                mesh.Effect.SetValue("specularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["specular"]));
                mesh.Effect.SetValue("specularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
            }

            //Renderizar modelo
            mesh.render();

            //Renderizar mesh de luz
            lightMesh.render();
        }



        public override void close()
        {
            mesh.dispose();
            lightMesh.dispose();
        }

    }

    

}
