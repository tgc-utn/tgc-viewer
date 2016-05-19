using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploDisposeMesh2
    /// </summary>
    public class EjemploDisposeMesh2 : TgcExample
    {
        private TgcScene scene1;

        public EjemploDisposeMesh2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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

            ((TgcRotationalCamera)Camara).setCamera(new Vector3(0f, 300f, 0f), 1500f);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            TgcDrawText.Instance.drawText("ok", 100, 100, Color.Red);
            scene1.renderAll();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            scene1.disposeAll();
        }
    }
}