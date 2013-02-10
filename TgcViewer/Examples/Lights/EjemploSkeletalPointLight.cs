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
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace Examples.Lights
{
    /// <summary>
    /// Ejemplo EjemploSkeletalPointLight:
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    /// 
    /// Ejemplo avanzado. Ver primero ejemplo "SkeletalAnimation/EjemploBasicHuman" y luego "Lights/EjemploPointLight".
    /// 
    /// Muestra como aplicar iluminación dinámica a un personaje animado con Skeletal Mesh, con PhongShading 
    /// por pixel en un Pixel Shader, para un tipo de luz "Point Light".
    /// Permite una única luz por objeto.
    /// Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    /// Las luces poseen atenuación por la distancia.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploSkeletalPointLight : TgcExample
    {
        TgcSkeletalMesh mesh;
        TgcBox lightMesh;

        public override string getCategory()
        {
            return "Lights";
        }

        public override string getName()
        {
            return "Skeletal - Point light";
        }

        public override string getDescription()
        {
            return "Iluminación dinámica para un Skeletal Mesh, por PhongShading de una luz del tipo Point Light";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar mesh con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            mesh = skeletalLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\" + "BasicHuman-TgcSkeletalMesh.xml",
                new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Walk-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "StandBy-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\" + "Jump-TgcSkeletalAnim.xml"
                });


            //Configurar animacion inicial
            mesh.playAnimation("Walk", true);


            //Camara en 1ra persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 20, -150), new Vector3(0, 20, 0));


            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200), new Vector3(200, 200, 300), new Vector3(0, 70, 0));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.White);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 20);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);

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
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight para Skeletal Mesh
                currentShader = GuiController.Instance.Shaders.TgcSkeletalMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = GuiController.Instance.Shaders.TgcSkeletalMeshShader;
            }
            
            //Aplicar al mesh el shader actual
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcSkeletalMeshTechnique(mesh.RenderType);




            //Actualzar posición de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Renderizar mesh
            if (lightEnable)
            {
                //Cargar variables shader de la luz
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
                mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
                mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
            }
            mesh.animateAndRender();


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
