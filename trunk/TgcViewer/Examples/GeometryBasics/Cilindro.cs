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
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class Cilindro : TgcExample
    {
        private TgcCylinder cylinder;
        private string currentTexture;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Crear Cilindro 3D";
        }

        public override string getDescription()
        {
            return "Muestra como crear un cilindro 3D con la herramienta TgcCylinder, cuyos par�metros " +
                "pueden ser modificados. Movimiento con mouse.";
        }

        public override void init()
        {
            cylinder = new TgcCylinder(new Vector3(0, 0, 0), 2, 4);

            //cylinder.Transform = Matrix.Scaling(2, 1, 1) * Matrix.RotationYawPitchRoll(0, 0, 1);
            //cylinder.AutoTransformEnable = false;
            //cylinder.updateValues();

            cylinder.AlphaBlendEnable = true;

            GuiController.Instance.Modifiers.addBoolean("boundingCylinder", "boundingCylinder", false);
            GuiController.Instance.Modifiers.addColor("color", Color.White);
            GuiController.Instance.Modifiers.addInt("alpha", 0, 255, 255);
            GuiController.Instance.Modifiers.addTexture("texture", GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            GuiController.Instance.Modifiers.addBoolean("useTexture", "useTexture", true);

            GuiController.Instance.Modifiers.addVertex3f("size", new Vector3(-3, -3, 1), new Vector3(7, 7, 10), new Vector3(2, 2, 5));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));
            float angle = FastMath.TWO_PI;
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-angle, -angle, -angle), new Vector3(angle, angle, angle), new Vector3(0, 0, 0));
        }


        public override void render(float elapsedTime)
        {
            TgcModifiers modifiers = GuiController.Instance.Modifiers;
            Vector3 size = (Vector3)modifiers.getValue("size");
            Vector3 position = (Vector3)modifiers.getValue("position");
            Vector3 rotation = (Vector3)modifiers.getValue("rotation");
            
            string texturePath = (string)modifiers.getValue("texture");
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                cylinder.setTexture(TgcTexture.createTexture(GuiController.Instance.D3dDevice, currentTexture));
            }

            cylinder.UseTexture = (bool)modifiers.getValue("useTexture");

            cylinder.Position = position;
            cylinder.Rotation = rotation;
            cylinder.TopRadius = size.X;
            cylinder.BottomRadius = size.Y;
            cylinder.Length = size.Z;

            int alpha = (int)modifiers.getValue("alpha");
            Color color = (Color)modifiers.getValue("color");
            cylinder.Color = Color.FromArgb(alpha, color);

            cylinder.updateValues();

            if ((bool)modifiers.getValue("boundingCylinder"))
                cylinder.BoundingCylinder.render();
            else
                cylinder.render();
        }

        public override void close()
        {
            cylinder.dispose();
        }

    }
}
