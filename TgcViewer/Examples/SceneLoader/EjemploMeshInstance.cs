using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Example;

namespace Examples.SceneLoader
{
    /// <summary>
    ///     Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploMeshInstance : TgcExample
    {
        private List<TgcMesh> meshes;

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
            var d3dDevice = GuiController.Instance.D3dDevice;

            var loader = new TgcSceneLoader();
            var sceneOriginal =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" +
                                         "Box-TgcScene.xml");
            var meshOriginal = sceneOriginal.Meshes[0];

            var meshInstance1 = new TgcMesh(meshOriginal.Name + "-1", meshOriginal,
                new Vector3(50, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance1.Enabled = true;

            var meshInstance2 = new TgcMesh(meshOriginal.Name + "-2", meshOriginal,
                new Vector3(100, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance2.Enabled = true;

            meshes = new List<TgcMesh>();
            meshes.Add(meshOriginal);
            meshes.Add(meshInstance1);
            meshes.Add(meshInstance2);

            var texture = TgcTexture.createTexture(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Piso\\Textures\\piso2.jpg");
            meshOriginal.changeDiffuseMaps(new[] {texture});

            GuiController.Instance.FpsCamera.Enable = true;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            foreach (var mesh in meshes)
            {
                mesh.render();
            }

            if (GuiController.Instance.D3dInput.keyPressed(Key.H))
            {
                meshes[0].dispose();
            }
        }

        public override void close()
        {
            foreach (var mesh in meshes)
            {
                mesh.dispose();
            }
        }
    }
}