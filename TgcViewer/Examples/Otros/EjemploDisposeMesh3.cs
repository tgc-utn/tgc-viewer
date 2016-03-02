using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Otros
{
    /// <summary>
    ///     EjemploDisposeMesh3
    /// </summary>
    public class EjemploDisposeMesh3 : TgcExample
    {
        private TgcMesh boxMesh;
        private float time;

        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Dispose Mesh 3";
        }

        public override string getDescription()
        {
            return "Dispose Mesh 3";
        }

        public override void init()
        {
            var box = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            boxMesh = box.toMesh("box");
            box.dispose();
            time = 0;
        }

        public override void render(float elapsedTime)
        {
            time += elapsedTime;
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

                //d3dMesh.SetVertexBufferData(newVert, LockFlags.None);
                d3dMesh.UnlockVertexBuffer();

                boxMesh.changeD3dMesh(d3dMesh);

                time = 0;
            }

            boxMesh.render();
        }

        public override void close()
        {
            boxMesh.dispose();
        }
    }
}