using Microsoft.DirectX;
using System.Drawing;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploDisposeMesh2
    /// </summary>
    public class EjemploDisposeMesh2 : TGCExampleViewer
    {
        private TgcScene scene1;

        public EjemploDisposeMesh2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Others";
            Name = "Dispose Mesh 2";
            Description = "Dispose Mesh 2";
        }

        public override void Init()
        {
            for (var i = 0; i < 100; i++)
            {
                var loader = new TgcSceneLoader();
                var scene =
                    loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
                scene.disposeAll();
            }

            var loader1 = new TgcSceneLoader();
            scene1 =
                loader1.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");

            Camara = new TgcRotationalCamera(new Vector3(0f, 300f, 0f), 1500f);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            DrawText.drawText("ok", 100, 100, Color.Red);
            scene1.renderAll();

            PostRender();
        }

        public override void Dispose()
        {
            scene1.disposeAll();
        }
    }
}