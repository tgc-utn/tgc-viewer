using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     EjemploDisposeMesh
    /// </summary>
    public class EjemploDisposeMesh : TGCExampleViewer
    {
        private TgcMesh boxMesh;
        private float disposed;
        private List<TgcMesh> meshes;
        private TgcScene scene1;
        private float time;

        public EjemploDisposeMesh(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Mesh Examples";
            Name = "Dispose Mesh";
            Description = "Muestra diferentes formas y objetos que deben ser liberados (Dispose) Mesh";
        }

        public override void Init()
        {
            //Primero creamos una lista de mesh con la misma esena.
            meshes = new List<TgcMesh>();
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            var baseMesh = scene.Meshes[0];
            for (var i = 0; i < 50; i++)
            {
                for (var j = 0; j < 50; j++)
                {
                    var mesh = baseMesh.clone(i + " - " + j);
                    mesh.AutoTransformEnable = true;
                    mesh.move(i * 100, 0, j * 100);
                    meshes.Add(mesh);
                    //Se agrega un callback function para informar cuando se realiza el dispose.
                    mesh.D3dMesh.Disposing += D3dMesh_Disposing;
                }
            }
            //Una vez clonamos todas las mesh que queriamos eliminamos la esena.
            scene.disposeAll();

            //Se crea una box y se convierte en mesh.
            var box = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            boxMesh = box.toMesh("box");
            //Liberamos la caja pero nos quedamos con el mesh.
            box.dispose();

            //Creamos una esena
            var loader1 = new TgcSceneLoader();
            scene1 =
                loader1.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");

            time = 0;

            Camara = new TgcRotationalCamera(new Vector3(100f, 300f, 100f), 1500f, Input);
        }

        private void D3dMesh_Disposing(object sender, EventArgs e)
        {
            disposed++;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Cantidad de elementos liberados: " + disposed, 5, 20, Color.Red);

            //Renderisamos todo hasta que pase cierto tiempo y ahi liberamos todos los recursos.
            if (time >= 0f && time < 30f)
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
                    newVert[i].Position.Y += time;
                }

                d3dMesh.UnlockVertexBuffer();

                boxMesh.changeD3dMesh(d3dMesh);

                //Render de todas las palmeras.
                foreach (var m in meshes)
                {
                    m.render();
                }

                //Render de una esena.
                scene1.renderAll();

                //Render de la caja.
                boxMesh.render();

                time += ElapsedTime;
            }
            else
            {
                //ATENCION ESTO QUE SIGUE es solo para el ejemplo este, no es buena practica invocar al dispose completo.
                //En su lugar se puede invocar caso por caso segun las necidades.
                //Hacemos el dispose 1 vez sola.
                if (time != -1)
                {
                    time = -1;
                    Dispose();
                }
            }

            PostRender();
        }

        public override void Dispose()
        {
            //El ejemplo fuerza al dispose por eso no se debe llamar devuelta.
            if (time != -1)
            {
                foreach (var m in meshes)
                {
                    m.dispose();
                }
                meshes.Clear();

                scene1.disposeAll();

                boxMesh.dispose();
            }
        }
    }
}