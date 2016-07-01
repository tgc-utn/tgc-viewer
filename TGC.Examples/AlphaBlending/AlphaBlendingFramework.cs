using Microsoft.DirectX;
using System.Collections.Generic;
using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingFramework
    /// </summary>
    public class AlphaBlendingFramework : TGCExampleViewer
    {
        private List<TgcMesh> meshes;

        public AlphaBlendingFramework(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "AlphaBlending";
            Name = "AlphaBlending Framework";
            Description = "AlphaBlending Framework";
        }

        public override void Init()
        {
            meshes = new List<TgcMesh>();

            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\BoxAlpha\\Box-TgcScene.xml");
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

            Camara = new TgcFpsCamera(new Vector3(-100.0f, 0.0f, -50.0f), Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            foreach (var mesh in meshes)
            {
                mesh.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            meshes[0].dispose();
        }
    }
}