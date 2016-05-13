using System.Drawing;
using Microsoft.DirectX;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core._2D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Otros
{
    /// <summary>
    ///     EjemploDisposeMesh2
    /// </summary>
    public class EjemploDisposeMesh2 : TgcExample
    {
        private TgcScene scene1;

        public EjemploDisposeMesh2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara) : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            this.Category = "Otros";
            this.Name = "Dispose Mesh 2";
            this.Description = "Dispose Mesh 2";
        }

        public override void Init()
        {
            for (var i = 0; i < 100; i++)
            {
                var loader = new TgcSceneLoader();
                var scene = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
                scene.disposeAll();
            }

            var loader1 = new TgcSceneLoader();
            scene1 = loader1.loadSceneFromFile(this.MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");

            ((TgcRotationalCamera)this.Camara).setCamera(new Vector3(0f, 300f, 0f), 1500f);
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override void Render()
        {
            this.IniciarEscena();
            base.Render();
            
            TgcDrawText.Instance.drawText("ok", 100, 100, Color.Red);
            scene1.renderAll();

            this.FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            scene1.disposeAll();
        }
    }
}