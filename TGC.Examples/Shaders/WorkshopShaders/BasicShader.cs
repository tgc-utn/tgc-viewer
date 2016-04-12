using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Util;

namespace TGC.Examples.Shaders.WorkshopShaders
{
    /// <summary>
    ///     Ejemplo EnvMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Es el hola mundo de los shaders
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class BasicShader : TgcExample
    {
        private Effect effect;
        private TgcMesh mesh;
        private TgcScene scene;
        private float time;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-BasicShader";
        }

        public override string getDescription()
        {
            return "Ejemplo de Shader Basico";
        }

        public override void init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            //Cargar los mesh:
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                                             +
                                             "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];
            mesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new Vector3(0f, 0f, 0f);

            //Cargar Shader personalizado
            effect =
                TgcShaders.loadEffect(GuiController.Instance.ExamplesDir +
                                      "Shaders\\WorkshopShaders\\Shaders\\BasicShader.fx");

            // le asigno el efecto a la malla
            mesh.Effect = effect;

            // indico que tecnica voy a usar
            // Hay effectos que estan organizados con mas de una tecnica.
            mesh.Technique = "RenderScene";

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);

            time = 0;
        }

        public override void render(float elapsedTime)
        {
            time += elapsedTime;

            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
            CamaraManager.Instance.CurrentCamera.updateCamera(elapsedTime);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);

            // dibujo la malla pp dicha
            mesh.render();
        }

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
        }
    }
}