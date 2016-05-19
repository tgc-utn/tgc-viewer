using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingFramework
    /// </summary>
    public class AlphaBlendingFramework : TgcExample
    {
        private List<TgcMesh> meshes;

        public AlphaBlendingFramework(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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

            Camara = new TgcFpsCamera();
            Camara.setCamera(new Vector3(-100.0f, 0.0f, -50.0f), new Vector3(0.0f, 0.0f, 50.0f));
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            foreach (var mesh in meshes)
            {
                mesh.render();
            }

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();
            meshes[0].dispose();
        }
    }
}