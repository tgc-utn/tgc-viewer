using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearFlecha
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.
    ///     Muestra como crear una linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploFlechaYLineaBox : TGCExampleViewer
    {
        private TgcArrow arrow;
        private TgcBoxDebug box;

        public EjemploFlechaYLineaBox(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Geometry Basics";
            Name = "Flecha y linea box";
            Description =
                "Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow y linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine.";
        }

        public override void Init()
        {
            //Crea flecha generica
            arrow = new TgcArrow();

            //Crear modifiers
            Modifiers.addVertex3f("start", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), TGCVector3.Empty);
            Modifiers.addVertex3f("end", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), new TGCVector3(0, 10, 0));
            Modifiers.addFloat("thickness", 0.01f, 1, 0.06f);
            Modifiers.addVertex2f("headSize", new Vector2(0.01f, 0.01f), new Vector2(1, 1), new Vector2(0.3f, 0.6f));
            Modifiers.addColor("bodyColor", Color.Blue);
            Modifiers.addColor("headColor", Color.LightBlue);

            //Crea linea generica
            box = new TgcBoxDebug();

            //Crear modifiers
            Modifiers.addColor("boxColor", Color.Red);

            //Camara FPS
            Camara = new TgcRotationalCamera(new TGCVector3(0, 10f, 0), 30f, Input);
        }

        public override void Update()
        {
            PreUpdate();

            var start = (TGCVector3)Modifiers["start"];
            var end = (TGCVector3)Modifiers["end"];
            var thickness = (float)Modifiers["thickness"];
            var headSize = (Vector2)Modifiers["headSize"];
            var bodyColor = (Color)Modifiers["bodyColor"];
            var headColor = (Color)Modifiers["headColor"];

            var offset = new TGCVector3(10, 0, 0);
            //Cargar valores de la flecha
            arrow.PStart = start - offset;
            arrow.PEnd = end - offset;
            arrow.Thickness = thickness;
            arrow.HeadSize = headSize;
            arrow.BodyColor = bodyColor;
            arrow.HeadColor = headColor;

            //Actualizar valores para hacerlos efectivos, ADVERTENCIA verificar que estemetodo crea los vertices nuevamente.
            //Recomendado de ser posible realizar transformaciones!!!
            arrow.updateValues();

            var boxColor = (Color)Modifiers["boxColor"];

            //Cargar valores de la linea
            box.PMin = start + offset;
            box.PMax = end + offset;
            box.Thickness = thickness;
            box.Color = boxColor;

            //Actualizar valores para hacerlos efectivos, ADVERTENCIA verificar que estemetodo crea los vertices nuevamente.
            //Recomendado de ser posible realizar transformaciones!!!
            box.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Render
            arrow.render();

            box.render();

            PostRender();
        }

        public override void Dispose()
        {
            arrow.dispose();
        }
    }
}