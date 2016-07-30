using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Cargar una malla original y dos instancias de esta.
    /// </summary>
    public class EjemploMeshInstance : TGCExampleViewer
    {
        private List<TgcMesh> meshes;

        public EjemploMeshInstance(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "SceneLoader";
            Name = "MeshInstance";
            Description = "Cargar una malla original y dos instancias de esta.";
        }

        public override void Init()
        {
            var loader = new TgcSceneLoader();
            var sceneOriginal = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Box\\" +
                                                         "Box-TgcScene.xml");
            var meshOriginal = sceneOriginal.Meshes[0];
            meshOriginal.AutoTransformEnable = true;

            var meshInstance1 = new TgcMesh(meshOriginal.Name + "-1", meshOriginal,
                new Vector3(50, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance1.Enabled = true;
            meshInstance1.AutoTransformEnable = true;

            var meshInstance2 = new TgcMesh(meshOriginal.Name + "-2", meshOriginal,
                new Vector3(100, 0, 0), meshOriginal.Rotation, meshOriginal.Scale);
            meshInstance2.Enabled = true;
            meshInstance2.AutoTransformEnable = true;

            meshes = new List<TgcMesh>();
            meshes.Add(meshOriginal);
            meshes.Add(meshInstance1);
            meshes.Add(meshInstance2);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "ModelosTgc\\Piso\\Textures\\piso2.jpg");
            meshOriginal.changeDiffuseMaps(new[] { texture });

            Camara.setCamera(new Vector3(50f, 20f, -180f), new Vector3(40f, 0f, 0f));
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Camera pos: " + TgcParserUtils.printVector3(Camara.Position), 5, 20, Color.Red);
            DrawText.drawText("Camera LookAt: " + TgcParserUtils.printVector3(Camara.LookAt), 5, 40, Color.Red);

            foreach (var mesh in meshes)
            {
                mesh.render();
            }

            if (Input.keyPressed(Key.H))
            {
                meshes[0].dispose();
            }

            PostRender();
        }

        public override void Dispose()
        {
            foreach (var mesh in meshes)
            {
                mesh.dispose();
            }
        }
    }
}