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
using Examples.Shaders;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Interpolation;

namespace Examples.Lights
{
    /// <summary>
    /// Ejemplo EjemploSpotLight:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    /// 
    /// Muestra como aplicar iluminación dinámica con PhongShading por pixel en un Pixel Shader, para un tipo
    /// de luz "Spot Light".
    /// Permite una única luz por objeto.
    /// Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    /// Las luces poseen atenuación por la distancia.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploSpotLight : TgcExample
    {
        TgcScene scene;
        TgcBox lightMesh;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Spot light";
        }

        public override string getDescription()
        {
            return "Iluminación dinámica por PhongShading de una luz del tipo Spot Light";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar escenario
            TgcSceneLoader loader = new TgcSceneLoader();
            //Configurar MeshFactory customizado
            loader.MeshFactory = new CustomMeshShaderFactory();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-20, 80, 450), new Vector3(0, 80, 1));


            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200), new Vector3(200, 200, 300), new Vector3(-60, 90, 175));
            GuiController.Instance.Modifiers.addVertex3f("lightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 35);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);
            GuiController.Instance.Modifiers.addFloat("spotAngle", 0, 180, 39f);
            GuiController.Instance.Modifiers.addFloat("spotExponent", 0, 20, 7f);

            //Modifiers de material
            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mAmbient", Color.White);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);
            GuiController.Instance.Modifiers.addColor("mSpecular", Color.White);
        }

        


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;


            //Habilitar luz
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con SpotLight
                currentShader = GuiController.Instance.Shaders.TgcMeshSpotLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = GuiController.Instance.Shaders.TgcMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
            }


            //Actualzar posición de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Normalizar direccion de la luz
            Vector3 lightDir = (Vector3)GuiController.Instance.Modifiers["lightDir"];
            lightDir.Normalize();

            //Renderizar meshes
            foreach (TgcMesh mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
                    mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
                    mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad((float)GuiController.Instance.Modifiers["spotAngle"]));
                    mesh.Effect.SetValue("spotLightExponent", (float)GuiController.Instance.Modifiers["spotExponent"]);

                    //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
                }

                //Renderizar modelo
                mesh.render();
            }


            //Renderizar mesh de luz
            lightMesh.render();
        }




        public override void close()
        {
            scene.disposeAll();
            lightMesh.dispose();
        }



    }

    

}
