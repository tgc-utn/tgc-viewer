using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearFlecha
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class CrearFlecha : TgcExample
    {
        private TgcArrow arrow;

        public CrearFlecha(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "Flecha 3D";
            Description = "Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crea flecha genérica
            arrow = new TgcArrow();

            //Crear modifiers
            Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 10, 0));
            Modifiers.addFloat("thickness", 0.01f, 1, 0.06f);
            Modifiers.addVertex2f("headSize", new Vector2(0.01f, 0.01f), new Vector2(1, 1), new Vector2(0.3f, 0.6f));
            Modifiers.addColor("bodyColor", Color.Blue);
            Modifiers.addColor("headColor", Color.LightBlue);

            //Camara FPS
            Camara = new TgcFpsCamera(new Vector3(0.0302f, 5.842f, -18.97f), 10f, 10f);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

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

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            arrow.dispose();
        }
    }
}