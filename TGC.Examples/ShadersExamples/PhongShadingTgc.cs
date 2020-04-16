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
    public class PhongShadingTgc : TGCExampleViewer
    {
        private TGCBooleanModifier lightEnableModifier;
        private TGCVertex3fModifier lightPosModifier;
        private TGCColorModifier ambientModifier;
        private TGCColorModifier diffuseModifier;
        private TGCColorModifier specularModifier;
        private TGCFloatModifier specularExModifier;

        private TgcMesh lightMesh;
        private TgcMesh mesh;

        public PhongShadingTgc(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel Shaders";
            Name = "TGC Phong Shading";
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
            lightMesh = TGCBox.fromSize(new TGCVector3(20, 20, 20), Color.Yellow).ToMesh("Box");

            //Modifiers de la luz
            lightEnableModifier = AddBoolean("lightEnable", "lightEnable", true);
            lightPosModifier = AddVertex3f("lightPos", new TGCVector3(-500, -500, -500), new TGCVector3(500, 800, 500), new TGCVector3(0, 500, 0));
            ambientModifier = AddColor("ambient", Color.Gray);
            diffuseModifier = AddColor("diffuse", Color.Blue);
            specularModifier = AddColor("specular", Color.White);
            specularExModifier = AddFloat("specularEx", 0, 40, 20f);

            //Centrar camara rotacional respecto a este mesh
            Camera = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            //Habilitar luz
            var lightEnable = lightEnableModifier.Value;
            Effect currentShader;
            if (lightEnable)
            {
                //Con luz: Cambiar el shader actual por el shader default que trae el framework para iluminacion dinamica con PhongShading
                currentShader = TGCShaders.Instance.TgcMeshPhongShader;
            }
            else
            {
                //Sin luz: Restaurar shader default
                currentShader = TGCShaders.Instance.TgcMeshShader;
            }

            //Aplicar al mesh el shader actual
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = TGCShaders.Instance.GetTGCMeshTechnique(mesh.RenderType);

            //Actualzar posicion de la luz
            var lightPos = lightPosModifier.Value;
            lightMesh.Position = lightPos;

            if (lightEnable)
            {
                //Cargar variables shader
                mesh.Effect.SetValue("lightPosition", TGCVector3.TGCVector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat4Array(Camera.Position));
                mesh.Effect.SetValue("ambientColor", ColorValue.FromColor(ambientModifier.Value));
                mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor(diffuseModifier.Value));
                mesh.Effect.SetValue("specularColor", ColorValue.FromColor(specularModifier.Value));
                mesh.Effect.SetValue("specularExp", specularExModifier.Value);
            }

            //No hace falta actualizar la matriz de transformacion ya que es un objeto estatico.
            //Renderizar modelo
            mesh.Render();
            lightMesh.UpdateMeshTransform();
            //Renderizar mesh de luz
            lightMesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
            lightMesh.Dispose();
        }
    }
}