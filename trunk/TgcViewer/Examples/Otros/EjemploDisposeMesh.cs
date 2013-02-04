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

namespace Examples.Otros
{
    /// <summary>
    /// EjemploDisposeMesh
    /// </summary>
    public class EjemploDisposeMesh : TgcExample
    {
        List<TgcMesh> meshes;

        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Dispose Mesh";
        }

        public override string getDescription()
        {
            return "Dispose Mesh";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            meshes = new List<TgcMesh>();
            for (int i = 0; i < 100; i++)
            {
                TgcSceneLoader loader = new TgcSceneLoader();
                TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
                TgcMesh mesh = scene.Meshes[0];
                mesh.move(0, i * 100, 0);
                meshes.Add(mesh);

                mesh.D3dMesh.Disposing += new EventHandler(D3dMesh_Disposing);
            }
        }

        void D3dMesh_Disposing(object sender, EventArgs e)
        {
            int a = 0;
            a++;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (TgcMesh m in meshes)
            {
                m.render();
            }
        }

        public override void close()
        {
            foreach (TgcMesh m in meshes)
            {
                m.dispose();
            }
            meshes.Clear();
            meshes = null;

            GuiController.Instance.TexturesPool.clearAll();

            GC.Collect();
        }

    }
}
