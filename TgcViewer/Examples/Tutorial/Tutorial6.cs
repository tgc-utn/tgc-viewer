using TGC.Core.Example;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 6:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como cargar una escena 3D completa.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial6 : TgcExample
    {
        //Variable para la escena 3D
        private TgcScene scene;

        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 6";
        }

        public override string getDescription()
        {
            return "Muestra como cargar una escena 3D completa.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //En este ejemplo no cargamos un solo modelo 3D sino una escena completa, compuesta por varios modelos.
            //El framework posee varias escenas ya hechas en la carpeta TgcViewer\Examples\Media\MeshCreator\Scenes.
            var loader = new TgcSceneLoader();
            scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Scenes\\Iglesia\\Iglesia-TgcScene.xml");

            //Hacemos que la cámara esté centrada sobre la escena
            GuiController.Instance.RotCamera.targetObject(scene.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Dibujar la escena entera
            scene.renderAll();

            //Hacer renderAll() es el equivalente a recorrer todos sus modelos internos y dibujar cada uno:
            /*
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.render();
            }
            */
        }

        public override void close()
        {
            //Liberar memoria de toda la escena
            scene.disposeAll();
        }
    }
}