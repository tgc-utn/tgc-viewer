using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Example;

namespace Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploBoundingBox:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    ///     Carga un modelo 3D estático mediante la herramienta TgcSceneLoader
    ///     y muestra como renderizar su BoundingBox.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBoundingBox : TgcExample
    {
        private TgcMesh mesh;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "BoundingBox";
        }

        public override string getDescription()
        {
            return
                "Carga un modelo 3D estático mediante la herramienta TgcSceneLoader y muestra como renderizar su BoundingBox. Movimiento con mouse.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar modelo estatico
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Buggy\\Buggy-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Alejar camara rotacional segun tamaño del BoundingBox del objeto
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar modelo
            mesh.render();

            //Renderizar BoundingBox
            mesh.BoundingBox.render();
        }

        public override void close()
        {
            mesh.dispose();
        }
    }
}