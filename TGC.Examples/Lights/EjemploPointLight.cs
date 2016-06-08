using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploPointLight:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "SceneLoader/CustomMesh" y luego "Shaders/EjemploShaderTgcMesh".
    ///     Muestra como aplicar iluminación dinámica con PhongShading por pixel en un Pixel Shader, para un tipo
    ///     de luz "Point Light".
    ///     Permite una única luz por objeto.
    ///     Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    ///     Las luces poseen atenuación por la distancia.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPointLight : TgcExample
    {
        private TgcBox lightMesh;
        private TgcScene scene;

        public EjemploPointLight(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Lights";
            Name = "Point light";
            Description = "Iluminación dinámica por PhongShading de una luz del tipo Point Light.";
        }

        public override void Init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            Camara = new TgcFpsCamera();
            ((TgcFpsCamera)Camara).MovementSpeed = 400f;
            ((TgcFpsCamera)Camara).JumpSpeed = 300f;
            Camara.setCamera(new Vector3(-20, 80, 450), new Vector3(0, 80, 1));

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200),
                new Vector3(200, 200, 300), new Vector3(60, 35, 250));
            Modifiers.addColor("lightColor", Color.White);
            Modifiers.addFloat("lightIntensity", 0, 150, 20);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);

            //Modifiers de material
            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Habilitar luz
            var lightEnable = (bool)Modifiers["lightEnable"];
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight
                currentShader = TgcShaders.Instance.TgcMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
            }

            //Actualzar posición de la luz
            var lightPos = (Vector3)Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)Modifiers["lightColor"]));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(Camara.Position));
                    mesh.Effect.SetValue("lightIntensity", (float)Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation", (float)Modifiers["lightAttenuation"]);

                    //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)Modifiers["specularEx"]);
                }

                //Renderizar modelo
                mesh.render();
            }

            //Renderizar mesh de luz
            lightMesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            scene.disposeAll();
            lightMesh.dispose();
        }
    }
}