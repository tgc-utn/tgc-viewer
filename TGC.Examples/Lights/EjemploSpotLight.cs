using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploSpotLight:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Lights/EjemploPointLight"
    ///     Muestra como aplicar iluminacion dinamica con PhongShading por pixel en un Pixel Shader, para un tipo
    ///     de luz "Spot Light".
    ///     Permite una unica luz por objeto.
    ///     Calcula todo el modelo de iluminacion completo (Ambient, Diffuse, Specular)
    ///     Las luces poseen atenuacion por la distancia.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSpotLight : TGCExampleViewer
    {
        private TgcBox lightMesh;
        private TgcScene scene;

        public EjemploSpotLight(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Lights";
            Name = "Spot light";
            Description = "Iluminacion dinamica por PhongShading de una luz del tipo Spot Light.";
        }

        public override void Init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            //Configurar MeshFactory customizado
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new Vector3(-20, 80, 450), 400f, 300f);

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200), new Vector3(200, 200, 300),
                new Vector3(-60, 90, 175));
            Modifiers.addVertex3f("lightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            Modifiers.addColor("lightColor", Color.White);
            Modifiers.addFloat("lightIntensity", 0, 150, 35);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);
            Modifiers.addFloat("spotAngle", 0, 180, 39f);
            Modifiers.addFloat("spotExponent", 0, 20, 7f);

            //Modifiers de material
            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Habilitar luz
            var lightEnable = (bool)Modifiers["lightEnable"];
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con SpotLight
                currentShader = TgcShaders.Instance.TgcMeshSpotLightShader;
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

            //Actualzar posicion de la luz
            var lightPos = (Vector3)Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Normalizar direccion de la luz
            var lightDir = (Vector3)Modifiers["lightDir"];
            lightDir.Normalize();

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)Modifiers["lightColor"]));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(Camara.Position));
                    mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
                    mesh.Effect.SetValue("lightIntensity", (float)Modifiers["lightIntensity"]);
                    mesh.Effect.SetValue("lightAttenuation", (float)Modifiers["lightAttenuation"]);
                    mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad((float)Modifiers["spotAngle"]));
                    mesh.Effect.SetValue("spotLightExponent", (float)Modifiers["spotExponent"]);

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

            PostRender();
        }

        public override void Dispose()
        {
            scene.disposeAll();
            lightMesh.dispose();
        }
    }
}