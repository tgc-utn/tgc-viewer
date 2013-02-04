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

namespace Examples.AlphaBlending
{
    /// <summary>
    /// AlphaBlendingFramework
    /// </summary>
    public class AlphaBlendingFramework : TgcExample
    {

        List<TgcMesh> meshes;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            meshes = new List<TgcMesh>();

            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BoxAlpha\\Box-TgcScene.xml");
            TgcMesh originalMesh = scene.Meshes[0];

            meshes.Add(originalMesh);
            originalMesh.Position = new Vector3(0,0,0);
            originalMesh.AlphaBlendEnable = true;

            TgcMesh instanceMesh;
            for (int i = 0; i < 5; i++)
            {
                instanceMesh = originalMesh.createMeshInstance("Box" + (i+1));
                instanceMesh.Position = new Vector3(0, 0, i * 50);
                instanceMesh.AlphaBlendEnable = true;
                meshes.Add(instanceMesh);
            }


            GuiController.Instance.FpsCamera.Enable = true;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (TgcMesh mesh in meshes)
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
