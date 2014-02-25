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
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Otros
{
    /// <summary>
    /// EjemploDisposeMesh3
    /// </summary>
    public class EjemploDisposeMesh3 : TgcExample
    {

        TgcMesh boxMesh;
        float time;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            TgcBox box = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            boxMesh = box.toMesh("box");
            box.dispose();
            time = 0;
        }



        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            
            time += elapsedTime;
            if (time > 1f)
            {
                Mesh d3dMesh = new Mesh(boxMesh.NumberTriangles, boxMesh.NumberVertices, MeshFlags.Managed, TgcSceneLoader.VertexColorVertexElements, d3dDevice);
                
                TgcSceneLoader.VertexColorVertex[] origVert = (TgcSceneLoader.VertexColorVertex[])boxMesh.D3dMesh.LockVertexBuffer(
                            typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, boxMesh.D3dMesh.NumberVertices);

                boxMesh.D3dMesh.UnlockVertexBuffer();

                TgcSceneLoader.VertexColorVertex[] newVert = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                            typeof(TgcSceneLoader.VertexColorVertex), LockFlags.None, d3dMesh.NumberVertices);

                for (int i = 0; i < origVert.Length; i++)
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
