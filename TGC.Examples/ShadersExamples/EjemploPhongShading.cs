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

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EjemploPhongShading:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/EjemploShaderTgcMesh".
    ///     Muestra como utilizar un Shader para lograr iluminacion dinamica del tipo Phong-Shading.
    ///     El ejemplo permite modificar los parametros de iluminacion para ver como afectan sobre el objeto
    ///     en tiempo real.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPhongShading : TGCExampleViewer
    {
        private TgcBox lightMesh;
        private TgcMesh mesh;

        public EjemploPhongShading(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "PhongShading";
            Description = "Muestra como utilizar un Shader para lograr iluminacion dinamica del tipo Phong-Shading.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar mesh
            var scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Olla\\Olla-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Crear caja para indicar ubicacion de la luz
            lightMesh = TgcBox.fromSize(new Vector3(20, 20, 20), Color.Yellow);

            //Modifiers de la luz
            Modifiers.addBoolean("lightEnable", "lightEnable", true);
            Modifiers.addVertex3f("lightPos", new Vector3(-500, -500, -500), new Vector3(500, 800, 500),
                new Vector3(0, 500, 0));
            Modifiers.addColor("ambient", Color.Gray);
            Modifiers.addColor("diffuse", Color.Blue);
            Modifiers.addColor("specular", Color.White);
            Modifiers.addFloat("specularEx", 0, 40, 20f);

            //Centrar camara rotacional respecto a este mesh
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2);
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
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PhongShading
                currentShader = TgcShaders.Instance.TgcMeshPhongShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TgcShaders.Instance.TgcMeshShader;
            }

            //Aplicar al mesh el shader actual
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);

            //Actualzar posicion de la luz
            var lightPos = (Vector3)Modifiers["lightPos"];
            lightMesh.Position = lightPos;

            if (lightEnable)
            {
                //Cargar variables shader
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(Camara.Position));
                mesh.Effect.SetValue("ambientColor", ColorValue.FromColor((Color)Modifiers["ambient"]));
                mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor((Color)Modifiers["diffuse"]));
                mesh.Effect.SetValue("specularColor", ColorValue.FromColor((Color)Modifiers["specular"]));
                mesh.Effect.SetValue("specularExp", (float)Modifiers["specularEx"]);
            }

            //Renderizar modelo
            mesh.render();

            //Renderizar mesh de luz
            lightMesh.render();

            PostRender();
        }

        public override void Dispose()
        {
            mesh.dispose();
            lightMesh.dispose();
        }
    }
}