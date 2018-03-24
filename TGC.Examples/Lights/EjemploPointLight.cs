using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.SkeletalAnimation;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploPointLight:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminacion - Iluminacion dinamica
    ///     # Unidad 5 - Animacion - Skeletal Animation
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplos "SceneLoader/CustomMesh", "Animation/SkeletalAnimation" y luego
    ///     "Shaders/EjemploShaderTgcMesh".
    ///     Muestra como aplicar iluminacion dinamica a mesh estaticos o a un personaje animado con Skeletal Mesh, con
    ///     PhongShading por pixel en un Pixel Shader, para un tipo de luz "Point Light".
    ///     Permite una unica luz por objeto.
    ///     Calcula todo el modelo de iluminacion completo (Ambient, Diffuse, Specular)
    ///     Las luces poseen atenuacion por la distancia.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPointLight : TGCExampleViewer
    {
        private TGCBooleanModifier lightEnableModifier;
        private TGCVertex3fModifier lightPosModifier;
        private TGCColorModifier lightColorModifier;
        private TGCFloatModifier lightIntensityModifier;
        private TGCFloatModifier lightAttenuationModifier;
        private TGCFloatModifier specularExModifier;
        private TGCColorModifier mEmissiveModifier;
        private TGCColorModifier mAmbientModifier;
        private TGCColorModifier mDiffuseModifier;
        private TGCColorModifier mSpecularModifier;

        private TGCBox lightMesh;
        private TgcScene scene;
        private TgcSkeletalMesh skeletalMesh;

        public EjemploPointLight(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel Shaders";
            Name = "TGC Point light skeletal pong shading";
            Description = "Iluminacion dinamica por PhongShading de una luz del tipo Point Light.";
        }

        public override void Init()
        {
            //Cargar escenario
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Deposito\\Deposito-TgcScene.xml");

            //Cargar mesh con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            skeletalMesh = skeletalLoader.loadMeshAndAnimationsFromFile(MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    new[] { MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml" });

            //Configurar animacion inicial
            skeletalMesh.playAnimation("Parado", true);

            //Corregir normales
            skeletalMesh.computeNormals();

            //Pongo al mesh en posicion, activo e AutoTransform
            skeletalMesh.AutoTransform = true;
            skeletalMesh.Position = new TGCVector3(0, 0, 100);
            skeletalMesh.RotateY(FastMath.PI);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(250, 140, 150), Input);

            //Mesh para la luz
            lightMesh = TGCBox.fromSize(new TGCVector3(10, 10, 10));

            //Pongo al mesh en posicion, activo e AutoTransform
            lightMesh.AutoTransform = true;
            lightMesh.Position = new TGCVector3(0, 150, 150);

            //Modifiers de la luz
            lightEnableModifier = AddBoolean("lightEnable", "lightEnable", lightMesh.Enabled);
            lightPosModifier = AddVertex3f("lightPos", new TGCVector3(-200, -100, -200), new TGCVector3(200, 200, 300), lightMesh.Position);
            lightColorModifier = AddColor("lightColor", lightMesh.Color);
            lightIntensityModifier = AddFloat("lightIntensity", 0, 150, 20);
            lightAttenuationModifier = AddFloat("lightAttenuation", 0.1f, 2, 0.3f);
            specularExModifier = AddFloat("specularEx", 0, 20, 9f);

            //Modifiers de material
            mEmissiveModifier = AddColor("mEmissive", Color.Black);
            mAmbientModifier = AddColor("mAmbient", Color.White);
            mDiffuseModifier = AddColor("mDiffuse", Color.White);
            mSpecularModifier = AddColor("mSpecular", Color.White);
        }

        public override void Update()
        {
            PreUpdate();

            //Actualizo los valores de la luz
            lightMesh.Enabled = lightEnableModifier.Value;
            lightMesh.Position = lightPosModifier.Value;
            lightMesh.Color = lightColorModifier.Value;
            lightMesh.updateValues();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            Effect currentShader;
            Effect currentShaderSkeletalMesh;

            if (lightMesh.Enabled)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight
                currentShader = TgcShaders.Instance.TgcMeshPointLightShader;
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight para Skeletal Mesh
                currentShaderSkeletalMesh = TgcShaders.Instance.TgcSkeletalMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
                currentShaderSkeletalMesh = TgcShaders.Instance.TgcSkeletalMeshShader;
            }

            //Aplicar a cada mesh el shader actual
            foreach (var mesh in scene.Meshes)
            {
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
            }

            //Aplicar al mesh el shader actual
            skeletalMesh.Effect = currentShaderSkeletalMesh;
            //El Technique depende del tipo RenderType del mesh
            skeletalMesh.Technique = TgcShaders.Instance.getTgcSkeletalMeshTechnique(skeletalMesh.RenderType);

            //Renderizar meshes
            foreach (var mesh in scene.Meshes)
            {
                if (lightMesh.Enabled)
                {
                    //Cargar variables shader de la luz
                    mesh.Effect.SetValue("lightColor", ColorValue.FromColor(lightMesh.Color));
                    mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(lightMesh.Position));
                    mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(Camara.Position));
                    mesh.Effect.SetValue("lightIntensity", lightIntensityModifier.Value);
                    mesh.Effect.SetValue("lightAttenuation", lightAttenuationModifier.Value);

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

            //Renderizar mesh
            if (lightMesh.Enabled)
            {
                //Cargar variables shader de la luz
                skeletalMesh.Effect.SetValue("lightColor", ColorValue.FromColor(lightMesh.Color));
                skeletalMesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(lightMesh.Position));
                skeletalMesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(Camara.Position));
                skeletalMesh.Effect.SetValue("lightIntensity", lightIntensityModifier.Value);
                skeletalMesh.Effect.SetValue("lightAttenuation", lightAttenuationModifier.Value);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                skeletalMesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(mEmissiveModifier.Value));
                skeletalMesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(mAmbientModifier.Value));
                skeletalMesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(mDiffuseModifier.Value));
                skeletalMesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(mSpecularModifier.Value));
                skeletalMesh.Effect.SetValue("materialSpecularExp", specularExModifier.Value);
            }
            skeletalMesh.animateAndRender(ElapsedTime);

            //Renderizar mesh de luz
            lightMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
            skeletalMesh.Dispose();
            lightMesh.Dispose();
        }
    }
}