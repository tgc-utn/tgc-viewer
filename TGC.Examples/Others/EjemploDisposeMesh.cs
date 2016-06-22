using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploDisposeMesh
    /// </summary>
    public class EjemploDisposeMesh : TgcExample
    {
        private List<TgcMesh> meshes;

        public EjemploDisposeMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Others";
            Name = "Dispose Mesh";
            Description = "Dispose Mesh";
        }

        public override void Init()
        {
            meshes = new List<TgcMesh>();
            for (var i = 0; i < 100; i++)
            {
                var loader = new TgcSceneLoader();
                var scene =
                    loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
                var mesh = scene.Meshes[0];
                mesh.move(0, i * 100, 0);
                meshes.Add(mesh);

                mesh.D3dMesh.Disposing += D3dMesh_Disposing;
            }

            Camara = new TgcRotationalCamera(new Vector3(0f, 300f, 0f), 1500f);
        }

        private void D3dMesh_Disposing(object sender, EventArgs e)
        {
            var a = 0;
            a++;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            foreach (var m in meshes)
            {
                m.render();
            }

            PostRender();
        }

        public override void Dispose()
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