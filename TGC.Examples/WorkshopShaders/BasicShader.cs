using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace Examples.WorkshopShaders
{
    /// <summary>
    /// Ejemplo EnvMap:
    /// Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///
    /// Es el hola mundo de los shaders
    ///
    /// Autor: Mariano Banquiero
    ///
    /// </summary>
    public class BasicShader : TGCExampleViewer
    {
        private Effect effect;
        private TgcScene scene;
        private TgcMesh mesh;
        private float time;

        public BasicShader(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-BasicShader";
            Description = "Ejemplo de Shader Basico";
        }

        public override void Init()
        {
            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            //Cargar los mesh:
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new TGCVector3(5, 5, 5);
            mesh.Position = new TGCVector3(0f, 0f, 0f);

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "WorkshopShaders\\BasicShader.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;

            // indico que tecnica voy a usar
            // Hay effectos que estan organizados con mas de una tecnica.
            mesh.Technique = "RenderScene";

            //Centrar camara rotacional respecto a este mesh
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 5, Input);

            time = 0;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            ClearTextures();
            D3DDevice.Instance.Device.BeginScene();

            time += ElapsedTime;

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

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
            scene.disposeAll();
        }
    }
}