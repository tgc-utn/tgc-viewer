using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Viewer;

namespace TGC.Examples.SceneLoader
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

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Piso\\Textures\\piso2.jpg");
            meshOriginal.changeDiffuseMaps(new[] { texture });

            GuiController.Instance.FpsCamera.Enable = true;
        }

        public override void render(float elapsedTime)
        {
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