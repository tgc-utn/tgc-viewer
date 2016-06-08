using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Shaders;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Lights
{
    /// <summary>
    ///     Ejemplo EjemploSkeletalPointLight:
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - Iluminación dinámica
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "SkeletalAnimation/EjemploBasicHuman" y luego "Lights/EjemploPointLight".
    ///     Muestra como aplicar iluminación dinámica a un personaje animado con Skeletal Mesh, con PhongShading
    ///     por pixel en un Pixel Shader, para un tipo de luz "Point Light".
    ///     Permite una única luz por objeto.
    ///     Calcula todo el modelo de iluminación completo (Ambient, Diffuse, Specular)
    ///     Las luces poseen atenuación por la distancia.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSkeletalPointLight : TgcExample
    {
        private TgcBox lightMesh;
        private TgcSkeletalMesh mesh;

        public EjemploSkeletalPointLight(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Lights";
            Name = "Skeletal - Point light";
            Description =
                "Iluminación dinámica para un Skeletal Mesh, por PhongShading de una luz del tipo Point Light.";
        }

        public override void Init()
        {
            //Cargar mesh con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            mesh =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });

            //Configurar animacion inicial
            mesh.playAnimation("Walk", true);

            //Camara en 1ra persona
            Camara = new TgcFpsCamera();
            ((TgcFpsCamera)Camara).MovementSpeed = 400f;
            ((TgcFpsCamera)Camara).JumpSpeed = 300f;
            Camara.setCamera(new Vector3(0, 20, -150), new Vector3(0, 20, 0));

            //Mesh para la luz
            lightMesh = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);

            //Modifiers de la luz
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addVertex3f("lightPos", new Vector3(-200, -100, -200), new Vector3(200, 200, 300),
                new Vector3(0, 70, 0));
            Modifiers.addColor("lightColor", Color.White);
            Modifiers.addFloat("lightIntensity", 0, 150, 20);
            Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            Modifiers.addFloat("specularEx", 0, 20, 9f);

            //Modifiers de material
            Modifiers.addColor("mEmissive", Color.Black);
            Modifiers.addColor("mAmbient", Color.White);
            Modifiers.addColor("mDiffuse", Color.White);
            Modifiers.addColor("mSpecular", Color.White);

            /*
            //corregir normales
            int[] adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);
            */
            mesh.computeNormals();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        private void computeNormals(TgcSkeletalMesh mesh)
        {
            mesh.getVertexPositions();
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
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PointLight para Skeletal Mesh
                currentShader = TgcShaders.Instance.TgcSkeletalMeshPointLightShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcSkeletalMeshShader;
            }

            //Aplicar al mesh el shader actual
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = TgcShaders.Instance.getTgcSkeletalMeshTechnique(mesh.RenderType);

            //Actualzar posición de la luz
            var lightPos = (Vector3)Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            //Renderizar mesh
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
            mesh.animateAndRender(ElapsedTime);

            //Renderizar mesh de luz
            lightMesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
            lightMesh.dispose();
        }
    }
}