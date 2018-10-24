using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
        private TGCBooleanModifier lightEnableModifier;
        private TGCVertex3fModifier lightPosModifier;
        private TGCVertex3fModifier lightDirModifier;
        private TGCColorModifier lightColorModifier;
        private TGCFloatModifier lightIntensityModifier;
        private TGCFloatModifier lightAttenuationModifier;
        private TGCFloatModifier specularExModifier;
        private TGCFloatModifier spotAngleModifier;
        private TGCFloatModifier spotExponentModifier;
        private TGCColorModifier mEmissiveModifier;
        private TGCColorModifier mAmbientModifier;
        private TGCColorModifier mDiffuseModifier;
        private TGCColorModifier mSpecularModifier;

        private TGCBox lightMesh;
        private TgcScene scene;

        public EjemploSpotLight(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel Shaders";
            Name = "TGC Spot light";
            Description = "Iluminacion dinamica por PhongShading de una luz del tipo Spot Light.";
        }

        public override void Init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            //Configurar MeshFactory customizado
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(200, 250, 175), 400f, 300f, Input);

            //Mesh para la luz
            lightMesh = TGCBox.fromSize(new TGCVector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            lightEnableModifier = AddBoolean("lightEnable", "lightEnable", true);
            lightPosModifier = AddVertex3f("lightPos", new TGCVector3(-200, -100, -200), new TGCVector3(200, 200, 300), new TGCVector3(-60, 90, 175));
            lightDirModifier = AddVertex3f("lightDir", new TGCVector3(-1, -1, -1), TGCVector3.One, new TGCVector3(-0.05f, 0, 0));
            lightColorModifier = AddColor("lightColor", Color.White);
            lightIntensityModifier = AddFloat("lightIntensity", 0, 150, 35);
            lightAttenuationModifier = AddFloat("lightAttenuation", 0.1f, 2, 0.3f);
            specularExModifier = AddFloat("specularEx", 0, 20, 9f);
            spotAngleModifier = AddFloat("spotAngle", 0, 180, 39f);
            spotExponentModifier = AddFloat("spotExponent", 0, 20, 7f);

            //Modifiers de material
            mEmissiveModifier = AddColor("mEmissive", Color.Black);
            mAmbientModifier = AddColor("mAmbient", Color.White);
            mDiffuseModifier = AddColor("mDiffuse", Color.White);
            mSpecularModifier = AddColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Habilitar luz
            var lightEnable = lightEnableModifier.Value;
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con SpotLight
                currentShader = TGCShaders.Instance.TgcMeshSpotLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TGCShaders.Instance.TgcMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = TGCShaders.Instance.GetTGCMeshTechnique(mesh.RenderType);
            }

            //Actualzar posicion de la luz
            var lightPos = lightPosModifier.Value;
            lightMesh.Position = lightPos;

            //Normalizar direccion de la luz
            var lightDir = lightDirModifier.Value;
            lightDir.Normalize();

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightEnable)
                {
                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(lightColorModifier.Value));
                    mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(Camara.Position));
                    mesh.Effect.SetValue("spotLightDir", TGCVector3.Vector3ToFloat3Array(lightDir));
                    mesh.Effect.SetValue("lightIntensity", lightIntensityModifier.Value);
                    mesh.Effect.SetValue("lightAttenuation", lightAttenuationModifier.Value);
                    mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(spotAngleModifier.Value));
                    mesh.Effect.SetValue("spotLightExponent", spotExponentModifier.Value);

                    //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(mEmissiveModifier.Value));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(mAmbientModifier.Value));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(mDiffuseModifier.Value));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(mSpecularModifier.Value));
                    mesh.Effect.SetValue("materialSpecularExp", specularExModifier.Value);
                }

                //Renderizar modelo
                mesh.Render();
            }

            //Renderizar mesh de luz
            lightMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
            lightMesh.Dispose();
        }
    }
}