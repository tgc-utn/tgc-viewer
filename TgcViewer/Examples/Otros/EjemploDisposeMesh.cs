using System;
using System.Collections.Generic;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Viewer;

namespace TGC.Examples.Otros
{
    /// <summary>
    ///     EjemploDisposeMesh
    /// </summary>
    public class EjemploDisposeMesh : TgcExample
    {
        private List<TgcMesh> meshes;

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
            meshes = new List<TgcMesh>();
            for (var i = 0; i < 100; i++)
            {
                var loader = new TgcSceneLoader();
                var scene =
                    loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                             "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
                var mesh = scene.Meshes[0];
                mesh.move(0, i * 100, 0);
                meshes.Add(mesh);

                mesh.D3dMesh.Disposing += D3dMesh_Disposing;
            }
        }

        private void D3dMesh_Disposing(object sender, EventArgs e)
        {
            var a = 0;
            a++;
        }

        public override void render(float elapsedTime)
        {
            foreach (var m in meshes)
            {
                m.render();
            }
        }

        public override void close()
        {
            foreach (var m in meshes)
            {
                m.dispose();
            }
            meshes.Clear();
            meshes = null;

            TexturesPool.Instance.clearAll();

            GC.Collect();
        }
    }
}