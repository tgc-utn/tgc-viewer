using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
        private TGCVertex3fModifier startModifier;
        private TGCVertex3fModifier endModifier;
        private TGCFloatModifier thicknessModifier;
        private TGCVertex2fModifier headSizeModifier;
        private TGCColorModifier bodyColorModifier;
        private TGCColorModifier headColorModifier;
        private TGCColorModifier boxColorModifier;

        private TgcArrow arrow;
        private TGCBoxDebug box;

        public EjemploFlechaYLineaBox(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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
            startModifier = AddVertex3f("start", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), TGCVector3.Empty);
            endModifier = AddVertex3f("end", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), new TGCVector3(0, 10, 0));
            thicknessModifier = AddFloat("thickness", 0.01f, 1, 0.06f);
            headSizeModifier = AddVertex2f("headSize", new TGCVector2(0.01f, 0.01f), TGCVector2.One, new TGCVector2(0.3f, 0.6f));
            bodyColorModifier = AddColor("bodyColor", Color.Blue);
            headColorModifier = AddColor("headColor", Color.LightBlue);

            //Crea linea generica
            box = new TGCBoxDebug();

            //Crear modifiers
            boxColorModifier = AddColor("boxColor", Color.Red);

            //Camera FPS
            Camera = new TgcRotationalCamera(new TGCVector3(0, 10f, 0), 30f, Input);
        }

        public override void Update()
        {
            var start = startModifier.Value;
            var end = endModifier.Value;
            var thickness = thicknessModifier.Value;
            var headSize = headSizeModifier.Value;
            var bodyColor = bodyColorModifier.Value;
            var headColor = headColorModifier.Value;

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

            var boxColor = boxColorModifier.Value;

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
            arrow.Render();

            box.Render();

            PostRender();
        }

        public override void Dispose()
        {
            arrow.Dispose();
        }
    }
}