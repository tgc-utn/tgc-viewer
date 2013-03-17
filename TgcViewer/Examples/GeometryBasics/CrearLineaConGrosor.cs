using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// Ejemplo Caja
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como crear una linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CrearLineaConGrosor : TgcExample
    {

        TgcBoxLine line;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Linea con Grosor";
        }

        public override string getDescription()
        {
            return "Muestra como crear una linea 3D con grosor configurable, utilizando la herramienta TgcBoxLine. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crea línea genérica
            line = new TgcBoxLine();

            //Crear modifiers
            GuiController.Instance.Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0,0,0));
            GuiController.Instance.Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 10, 0));
            GuiController.Instance.Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            GuiController.Instance.Modifiers.addColor("color", Color.Red);

            //Camara FPS
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0.0302f, 5.842f, -18.97f), new Vector3(27.9348f, -29.0575f, 980.0311f));
            GuiController.Instance.FpsCamera.MovementSpeed = 10f;
            GuiController.Instance.FpsCamera.JumpSpeed = 10f;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            Vector3 start = (Vector3)GuiController.Instance.Modifiers["start"];
            Vector3 end = (Vector3)GuiController.Instance.Modifiers["end"];
            float thickness = (float)GuiController.Instance.Modifiers["thickness"];
            Color color = (Color)GuiController.Instance.Modifiers["color"];

            //Cargar valores de la línea
            line.PStart = start;
            line.PEnd = end;
            line.Thickness = thickness;
            line.Color = color;

            //Actualizar valores para hacerlos efectivos
            line.updateValues();

            //Render
            line.render();
        }

        public override void close()
        {
            line.dispose();
        }

    }
}
