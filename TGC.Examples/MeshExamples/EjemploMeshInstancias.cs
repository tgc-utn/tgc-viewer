using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo EjemploInstanciasPalmeras
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     # Unidad 7 - Tecnicas de Optimizacion - Instancias de Modelos
    ///     Muestra como crear varias instancias de un mismo TgcMesh.
    ///     De esta forma se reutiliza su informacion grafica (triangulos, vertices, textura, etc).
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMeshInstancias : TGCExampleViewer
    {
        private List<TgcMesh> meshes;
        private TgcMesh palmeraOriginal;
        private TgcPlane suelo;

        public EjemploMeshInstancias(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Mesh Examples";
            Name = "Crear Instancias Mesh";
            Description = "Muestra como crear varias instancias de un mismo TgcMesh.";
        }

        public override void Init()
        {
            //Crear suelo
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg");
            suelo = new TgcPlane(new TGCVector3(-500, 0, -500), new TGCVector3(2000, 0, 2000), TgcPlane.Orientations.XZplane, pisoTexture, 10f, 10f);

            //Cargar modelo de palmera original
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmeraOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            var rows = 5;
            var cols = 6;
            float offset = 250;
            meshes = new List<TgcMesh>();
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    var instance = palmeraOriginal.createMeshInstance(palmeraOriginal.Name + i + "_" + j);
                    //No recomendamos utilizar AutoTransform, en juegos complejos se pierde el control. mejor utilizar Transformaciones con matrices.
                    instance.AutoTransform = true;
                    //Desplazarlo
                    instance.Move(i * offset, 0, j * offset);
                    //instance.Scale = new TGCVector3(0.25f, 0.25f, 0.25f);

                    meshes.Add(instance);
                }
            }

            //Camara en primera persona
            Camara = new TgcFpsCamera(new TGCVector3(900f, 400f, 900f), Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            DrawText.drawText("Camera pos: " + TGCVector3.PrintVector3(Camara.Position), 5, 20, System.Drawing.Color.Red);
            DrawText.drawText("Camera LookAt: " + TGCVector3.PrintVector3(Camara.LookAt), 5, 40, System.Drawing.Color.Red);
            //Renderizar suelo
            suelo.Render();

            //Renderizar instancias
            foreach (var mesh in meshes)
            {
                mesh.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            suelo.Dispose();

            //Al hacer dispose del original, se hace dispose automaticamente de todas las instancias
            palmeraOriginal.Dispose();
        }
    }
}