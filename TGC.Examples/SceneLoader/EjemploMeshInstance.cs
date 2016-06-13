using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.SceneLoader
{
    /// <summary>
    ///     Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploMeshInstance : TgcExample
    {
        private List<TgcMesh> meshes;

        public EjemploMeshInstance(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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
                MediaDir + "ModelosTgc\\Piso\\Textures\\piso2.jpg");
            meshOriginal.changeDiffuseMaps(new[] { texture });

            Camara = new TgcFpsCamera();
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            foreach (var mesh in meshes)
            {
                mesh.render();
            }

            if (TgcD3dInput.Instance.keyPressed(Key.H))
            {
                meshes[0].dispose();
            }

            helperPostRender();
        }

        public override void Close()
        {
            base.Close();

            foreach (var mesh in meshes)
            {
                mesh.dispose();
            }
        }
    }
}