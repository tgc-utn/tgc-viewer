using System.IO;
using Microsoft.DirectX;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Exportar una malla a XML
    /// </summary>
    public class EjemploExportarMesh : TGCExampleViewer
    {
        private TgcScene sceneRecover;

        public EjemploExportarMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "SceneLoader";
            Name = "MeshExporter";
            Description = "Exportar una malla a XML.";
        }

        public override void Init()
        {
            var loader = new TgcSceneLoader();
            var sceneOriginal = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Iglesia\\Iglesia-TgcScene.xml");
            var sceneOriginal2 = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Iglesia\\Iglesia-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(this.MediaDir + "ModelosTgc\\CajaVerde\\" + "CajaVerde-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(this.MediaDir + "ModelosTgc\\Avion\\" + "Avion-TgcScene.xml");
            //sceneOriginal = loader.loadSceneFromFile(this.MediaDir + "ModelosTgc\\Iglesia\\" + "Iglesia-TgcScene.xml");
            //TgcScene sceneOriginal = loader.loadSceneFromFile(this.MediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");
            //TgcScene sceneOriginal2 = loader.loadSceneFromFile(this.MediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");

            var destFolder = MediaDir + "PruebaExporter";

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            var unifiedScene = new TgcScene("PruebaExporter", destFolder);
            unifiedScene.Meshes.AddRange(sceneOriginal.Meshes);
            unifiedScene.Meshes.AddRange(sceneOriginal2.Meshes);

            var exporter = new TgcSceneExporter();
            //string fileSaved = exporter.exportSceneToXml(unifiedScene, destFolder);
            //string fileSaved = exporter.exportAndAppendSceneToXml(unifiedScene, destFolder);

            var r = exporter.exportAndAppendSceneToXml(sceneOriginal, destFolder);

            sceneRecover = loader.loadSceneFromFile(r.filePath);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            sceneRecover.renderAll();

            PostRender();
        }

        public override void Dispose()
        {
            sceneRecover.disposeAll();
        }
    }
}