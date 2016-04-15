using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Util;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Exportar una malla a XML
    /// </summary>
    public class EjemploExportarMesh : TgcExample
    {
        private TgcScene sceneRecover;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "MeshExporter";
        }

        public override string getDescription()
        {
            return "Exportar una malla a XML";
        }

        public override void init()
        {
            var loader = new TgcSceneLoader();
            var sceneOriginal =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" +
                                         "Iglesia-TgcScene.xml");
            var sceneOriginal2 =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" +
                                         "Iglesia-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\CajaVerde\\" + "CajaVerde-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Avion\\" + "Avion-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" + "Iglesia-TgcScene.xml");
            //TgcScene sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");
            //TgcScene sceneOriginal2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");

            var destFolder = GuiController.Instance.ExamplesMediaDir + "PruebaExporter";
            var unifiedScene = new TgcScene("PruebaExporter", destFolder);
            unifiedScene.Meshes.AddRange(sceneOriginal.Meshes);
            unifiedScene.Meshes.AddRange(sceneOriginal2.Meshes);

            var exporter = new TgcSceneExporter();
            //string fileSaved = exporter.exportSceneToXml(unifiedScene, destFolder);
            //string fileSaved = exporter.exportAndAppendSceneToXml(unifiedScene, destFolder);

            var r = exporter.exportAndAppendSceneToXml(sceneOriginal, destFolder);

            sceneRecover = loader.loadSceneFromFile(r.filePath);
        }

        public override void render(float elapsedTime)
        {
            sceneRecover.renderAll();
        }

        public override void close()
        {
        }
    }
}