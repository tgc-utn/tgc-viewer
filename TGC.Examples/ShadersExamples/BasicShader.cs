using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
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
    /// Ejemplo EnvMap:
    /// Unidades Involucradas:
    /// # Unidad 8 - Adaptadores de Video - Shaders
    ///
    /// Es el hola mundo de los shaders
    ///
    /// Autor: Mariano Banquiero
    /// </summary>
    public class BasicShader : TGCExampleViewer
    {
        private TGCIntervalModifier shaderEffectModifier;

        private Effect effect;
        private TgcMesh mesh;
        private TgcScene scene;
        private float time;

        public BasicShader(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "Shader Basico";
            Description = "Ejemplo de Shader Basico. Animacion por VS y coloracion por PS.";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar los mesh:
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
            mesh.Position = TGCVector3.Empty;

            //Cargar Shader personalizado
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "WorkshopShaders\\BasicShader.fx");

            //Le asigno el efecto a la malla
            mesh.Effect = effect;

            //Selecciona la tecnica del shader.
            shaderEffectModifier = AddInterval("Effect", new[] { "RenderScene", "RenderScene2" }, 1);

            //Indico que tecnica voy a usar
            // Hay effectos que estan organizados con mas de una tecnica.
            mesh.Technique = shaderEffectModifier.Value.ToString();

            //Centrar camara rotacional respecto a este mesh

            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 5, Input);

            time = 0;
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            ClearTextures();
            D3DDevice.Instance.Device.BeginScene();

            time += ElapsedTime;

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Indico que tecnica voy a usar
            mesh.Technique = shaderEffectModifier.Value.ToString();

            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);

            // dibujo la malla pp dicha
            mesh.Render();

            RenderAxis();
            RenderFPS();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public override void Dispose()
        {
            effect.Dispose();
            scene.DisposeAll();
        }
    }
}