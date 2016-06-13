using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     EjemploDisposeMesh3
    /// </summary>
    public class EjemploDisposeMesh3 : TgcExample
    {
        private TgcMesh boxMesh;
        private float time;

        public EjemploDisposeMesh3(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Others";
            Name = "Dispose Mesh 3";
            Description = "Dispose Mesh 3";
        }

        public override void Init()
        {
            var box = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            boxMesh = box.toMesh("box");
            box.dispose();
            time = 0;

            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(), box.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            time += ElapsedTime;
            if (time > 1f)
            {
                var d3dMesh = new Mesh(boxMesh.NumberTriangles, boxMesh.NumberVertices, MeshFlags.Managed,
                    TgcSceneLoader.VertexColorVertexElements, D3DDevice.Instance.Device);

                var origVert = (TgcSceneLoader.VertexColorVertex[])boxMesh.D3dMesh.LockVertexBuffer(
                    typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, boxMesh.D3dMesh.NumberVertices);

                boxMesh.D3dMesh.UnlockVertexBuffer();

                var newVert = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                    typeof(TgcSceneLoader.VertexColorVertex), LockFlags.None, d3dMesh.NumberVertices);

                for (var i = 0; i < origVert.Length; i++)
                {
                    newVert[i] = origVert[i];
                }

                d3dMesh.UnlockVertexBuffer();

                boxMesh.changeD3dMesh(d3dMesh);

                time = 0;
            }

            boxMesh.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            boxMesh.dispose();
        }
    }
}