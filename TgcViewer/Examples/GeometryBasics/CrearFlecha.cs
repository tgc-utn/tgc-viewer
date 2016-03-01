using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Example;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearFlecha
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    ///     Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class CrearFlecha : TgcExample
    {
        private TgcArrow arrow;

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
            return "Muestra como crear una flecha 3D, utilizando la herramienta TgcArrow. Movimiento con mouse.";
        }

        public override void init()
        {
            //Crea flecha gen�rica
            arrow = new TgcArrow();

            //Crear modifiers
            GuiController.Instance.Modifiers.addVertex3f("start", new Vector3(-50, -50, -50), new Vector3(50, 50, 50),
                new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("end", new Vector3(-50, -50, -50), new Vector3(50, 50, 50),
                new Vector3(0, 10, 0));
            GuiController.Instance.Modifiers.addFloat("thickness", 0.01f, 1, 0.06f);
            GuiController.Instance.Modifiers.addVertex2f("headSize", new Vector2(0.01f, 0.01f), new Vector2(1, 1),
                new Vector2(0.3f, 0.6f));
            GuiController.Instance.Modifiers.addColor("bodyColor", Color.Blue);
            GuiController.Instance.Modifiers.addColor("headColor", Color.LightBlue);

            //Camara FPS
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0.0302f, 5.842f, -18.97f),
                new Vector3(27.9348f, -29.0575f, 980.0311f));
            GuiController.Instance.FpsCamera.MovementSpeed = 10f;
            GuiController.Instance.FpsCamera.JumpSpeed = 10f;
        }

        public override void render(float elapsedTime)
        {
            var start = (Vector3)GuiController.Instance.Modifiers["start"];
            var end = (Vector3)GuiController.Instance.Modifiers["end"];
            var thickness = (float)GuiController.Instance.Modifiers["thickness"];
            var headSize = (Vector2)GuiController.Instance.Modifiers["headSize"];
            var bodyColor = (Color)GuiController.Instance.Modifiers["bodyColor"];
            var headColor = (Color)GuiController.Instance.Modifiers["headColor"];

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