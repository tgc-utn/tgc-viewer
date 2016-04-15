using Microsoft.DirectX;
using System.Collections.Generic;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Util;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingFramework
    /// </summary>
    public class AlphaBlendingFramework : TgcExample
    {
        private List<TgcMesh> meshes;

        public override string getCategory()
        {
            return "AlphaBlending";
        }

        public override string getName()
        {
            return "AlphaBlending Framework";
        }

        public override string getDescription()
        {
            return "AlphaBlending Framework";
        }

        public override void init()
        {
            meshes = new List<TgcMesh>();

            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "ModelosTgc\\BoxAlpha\\Box-TgcScene.xml");
            var originalMesh = scene.Meshes[0];

            meshes.Add(originalMesh);
            originalMesh.Position = new Vector3(0, 0, 0);
            originalMesh.AlphaBlendEnable = true;

            TgcMesh instanceMesh;
            for (var i = 0; i < 5; i++)
            {
                instanceMesh = originalMesh.createMeshInstance("Box" + (i + 1));
                instanceMesh.Position = new Vector3(0, 0, i * 50);
                instanceMesh.AlphaBlendEnable = true;
                meshes.Add(instanceMesh);
            }

            GuiController.Instance.FpsCamera.Enable = true;
        }

        public override void render(float elapsedTime)
        {
            foreach (var mesh in meshes)
            {
                mesh.render();
            }
        }

        public override void close()
        {
            meshes[0].dispose();
        }
    }
}