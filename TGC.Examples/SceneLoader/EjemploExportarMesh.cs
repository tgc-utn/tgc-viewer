using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Exportar una malla a XML
    /// </summary>
    public class EjemploExportarMesh : TgcExample
    {
        private TgcScene sceneRecover;

        public EjemploExportarMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            sceneRecover.renderAll();

            helperPostRender();
        }

        public override void Dispose()
        {
            

            sceneRecover.disposeAll();
        }
    }
}