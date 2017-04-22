using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

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
        private TgcBox lightMesh;
        private TgcScene scene;
        private TgcSkeletalMesh skeletalMesh;

        public EjemploPointLight(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
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
            skeletalMesh =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    new[] { MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml" });

            //Configurar animacion inicial
            skeletalMesh.playAnimation("Parado", true);

            //Corregir normales
            skeletalMesh.computeNormals();

            //Pongo al mesh en posicion, activo e AutoTransform
            skeletalMesh.AutoTransformEnable = true;
            skeletalMesh.Position = new TGCVector3(0, 0, 100);
            skeletalMesh.RotateY(FastMath.PI);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(250, 140, 150), Input);

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new TGCVector3(10, 10, 10));

            //Pongo al mesh en posicion, activo e AutoTransform
            lightMesh.AutoTransformEnable = true;
            lightMesh.Position = new TGCVector3(0, 150, 150);

            //Modifiers de la luz
            Modifiers.addBoolean("lightEnable", "lightEnable", lightMesh.Enabled);
            Modifiers.addVertex3f("lightPos", new TGCVector3(-200, -100, -200), new TGCVector3(200, 200, 300),
                lightMesh.Position);
            Modifiers.addColor("lightColor", lightMesh.Color);
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
            PreUpdate();

            //Actualizo los valores de la luz
            lightMesh.Enabled = (bool)Modifiers["lightEnable"];
            lightMesh.Position = (TGCVector3)Modifiers["lightPos"];
            lightMesh.Color = (Color)Modifiers["lightColor"];
            lightMesh.updateValues();
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
                mesh.Render();
            }

            //Renderizar mesh
            if (lightMesh.Enabled)
            {
                //Cargar variables shader de la luz
                skeletalMesh.Effect.SetValue("lightColor", ColorValue.FromColor(lightMesh.Color));
                skeletalMesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(lightMesh.Position));
                skeletalMesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(Camara.Position));
                skeletalMesh.Effect.SetValue("lightIntensity", (float)Modifiers["lightIntensity"]);
                skeletalMesh.Effect.SetValue("lightAttenuation", (float)Modifiers["lightAttenuation"]);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                skeletalMesh.Effect.SetValue("materialEmissiveColor",
                    ColorValue.FromColor((Color)Modifiers["mEmissive"]));
                skeletalMesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)Modifiers["mAmbient"]));
                skeletalMesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)Modifiers["mDiffuse"]));
                skeletalMesh.Effect.SetValue("materialSpecularColor",
                    ColorValue.FromColor((Color)Modifiers["mSpecular"]));
                skeletalMesh.Effect.SetValue("materialSpecularExp", (float)Modifiers["specularEx"]);
            }
            skeletalMesh.animateAndRender(ElapsedTime);

            //Renderizar mesh de luz
            lightMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            scene.disposeAll();
            skeletalMesh.Dispose();
            lightMesh.Dispose();
        }
    }
}