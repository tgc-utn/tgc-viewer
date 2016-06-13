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
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class CrearLineaConGrosor : TgcExample
    {
        private TgcBoxLine line;

        public CrearLineaConGrosor(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "Linea con Grosor";
            Description =
                "Muestra como crear una linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crea línea genérica
            line = new TgcBoxLine();

            //Crear modifiers
            Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 0, 0));
            Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 10, 0));
            Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            Modifiers.addColor("color", Color.Red);

            //Camara FPS
            Camara = new TgcFpsCamera(new Vector3(0.0302f, 5.842f, -18.97f), 10f, 10f);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            var start = (Vector3)Modifiers["start"];
            var end = (Vector3)Modifiers["end"];
            var thickness = (float)Modifiers["thickness"];
            var color = (Color)Modifiers["color"];

            //Cargar valores de la línea
            line.PStart = start;
            line.PEnd = end;
            line.Thickness = thickness;
            line.Color = color;

            //Actualizar valores para hacerlos efectivos
            line.updateValues();

            //Render
            line.render();

            helperPostRender();
        }

        public override void Dispose()
        {
            

            line.dispose();
        }
    }
}