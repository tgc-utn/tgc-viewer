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
    /// Ejemplo CrearFlecha
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CrearFlecha : TgcExample
    {

        TgcArrow arrow;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Flecha 3D";
        }

        public override string getDescription()
        {
            return "Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crea flecha genérica
            arrow = new TgcArrow();

            //Crear modifiers
            GuiController.Instance.Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0,0,0));
            GuiController.Instance.Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, 10, 0));
            GuiController.Instance.Modifiers.addFloat("thickness", 0.01f, 1, 0.06f);
            GuiController.Instance.Modifiers.addVertex2f("headSize", new Vector2(0.01f, 0.01f), new Vector2(1, 1), new Vector2(0.3f, 0.6f));
            GuiController.Instance.Modifiers.addColor("bodyColor", Color.Blue);
            GuiController.Instance.Modifiers.addColor("headColor", Color.LightBlue);

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
            Vector2 headSize = (Vector2)GuiController.Instance.Modifiers["headSize"];
            Color bodyColor = (Color)GuiController.Instance.Modifiers["bodyColor"];
            Color headColor = (Color)GuiController.Instance.Modifiers["headColor"];

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
        }

        public override void close()
        {
            arrow.dispose();
        }

    }
}
