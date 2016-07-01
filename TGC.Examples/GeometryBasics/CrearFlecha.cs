using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearFlecha
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class CrearFlecha : TGCExampleViewer
    {
        private TgcArrow arrow;

        public CrearFlecha(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "Flecha 3D";
            Description = "Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crea flecha generica
            arrow = new TgcArrow();

            //Crear modifiers
            Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 10, 0));
            Modifiers.addFloat("thickness", 0.01f, 1, 0.06f);
            Modifiers.addVertex2f("headSize", new Vector2(0.01f, 0.01f), new Vector2(1, 1), new Vector2(0.3f, 0.6f));
            Modifiers.addColor("bodyColor", Color.Blue);
            Modifiers.addColor("headColor", Color.LightBlue);

            //Camara FPS
            Camara = new TgcFpsCamera(new Vector3(0.0302f, 5.842f, -18.97f), 10f, 10f, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var start = (Vector3)Modifiers["start"];
            var end = (Vector3)Modifiers["end"];
            var thickness = (float)Modifiers["thickness"];
            var headSize = (Vector2)Modifiers["headSize"];
            var bodyColor = (Color)Modifiers["bodyColor"];
            var headColor = (Color)Modifiers["headColor"];

            //Cargar valores de la flecha
            arrow.PStart = start;
            arrow.PEnd = end;
            arrow.Thickness = thickness;
            arrow.HeadSize = headSize;
            arrow.BodyColor = bodyColor;
            arrow.HeadColor = headColor;

            //Actualizar valores para hacerlos efectivos
            arrow.updateValues();

            //Render
            arrow.render();

            PostRender();
        }

        public override void Dispose()
        {
            arrow.dispose();
        }
    }
}