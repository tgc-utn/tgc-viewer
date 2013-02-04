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
    /// Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploMeshInstance : TgcExample
    {
        List<TgcMesh> meshes;

        public override string getCategory()
        {
            return "SceneLoader";
        }

        public override string getName()
        {
            return "MeshInstance";
        }

        public override string getDescription()
        {
            return "Cargar una malla original y dos instancias de esta";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene sceneOriginal = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml");
            TgcMesh meshOriginal = sceneOriginal.Meshes[0];

            TgcMesh meshInstance1 = new TgcMesh(meshOriginal.Name + "-1", meshOriginal, 
                new Vector3(50, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance1.Enabled = true;

            TgcMesh meshInstance2 = new TgcMesh(meshOriginal.Name + "-2", meshOriginal,
                new Vector3(100, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance2.Enabled = true;

            meshes = new List<TgcMesh>();
            meshes.Add(meshOriginal);
            meshes.Add(meshInstance1);
            meshes.Add(meshInstance2);


            TgcTexture texture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Piso\\Textures\\piso2.jpg");
            meshOriginal.changeDiffuseMaps(new TgcTexture[] { texture });


            GuiController.Instance.FpsCamera.Enable = true;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            foreach (TgcMesh mesh in meshes)
            {
                mesh.render();
            }

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.H))
            {
                meshes[0].dispose();
            }
        }

        public override void close()
        {
            foreach (TgcMesh mesh in meshes)
            {
                mesh.dispose();
            }
        }

    }
}
