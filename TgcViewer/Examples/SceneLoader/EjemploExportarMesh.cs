using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SceneLoader
{
    /// <summary>
    /// Exportar una malla a XML
    /// </summary>
    public class EjemploExportarMesh : TgcExample
    {
        TgcScene sceneRecover;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" + "Iglesia-TgcScene.xml");
            TgcScene sceneOriginal2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" + "Iglesia-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\CajaVerde\\" + "CajaVerde-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Avion\\" + "Avion-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Iglesia\\" + "Iglesia-TgcScene.xml");
            //TgcScene sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");
            //TgcScene sceneOriginal2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");
            
            string destFolder = GuiController.Instance.ExamplesDir + "\\" + "PruebaExporter";
            TgcScene unifiedScene = new TgcScene("PruebaExporter", destFolder);
            unifiedScene.Meshes.AddRange(sceneOriginal.Meshes);
            unifiedScene.Meshes.AddRange(sceneOriginal2.Meshes);


            
            TgcSceneExporter exporter = new TgcSceneExporter();
            //string fileSaved = exporter.exportSceneToXml(unifiedScene, destFolder);
            //string fileSaved = exporter.exportAndAppendSceneToXml(unifiedScene, destFolder);

            string fileSaved = exporter.exportAndAppendSceneToXml(sceneOriginal, destFolder);

            sceneRecover = loader.loadSceneFromFile(fileSaved);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            sceneRecover.renderAll();

        }

        public override void close()
        {

        }

    }
}
