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

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploBoundingCylinder : TgcExample
    {
        private TgcBoundingCylinder collider;
        private TgcBoundingSphere collisionableSphere;

        private Color noCollisionColor = Color.Yellow;
        private Color collisionColor = Color.Red;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "BoundingCylinder";
        }

        public override string getDescription()
        {
            return "Muestra el testeo de colision entre cilindro orientable y esfera.";
        }

        public override void init()
        {
            collider = new TgcBoundingCylinder(new Vector3(0, 0, 0), 2, 4);
            collisionableSphere = new TgcBoundingSphere(new Vector3(0, 0, -6), 3);

            GuiController.Instance.Modifiers.addVertex2f("size", new Vector2(1, 1), new Vector2(5, 10), new Vector2(2, 5));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));
            float angle = FastMath.TWO_PI;
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-angle, -angle, -angle), new Vector3(angle, angle, angle), new Vector3(0, 0, 0));

            collider.setRenderColor(Color.LimeGreen);
        }


        public override void render(float elapsedTime)
        {
            TgcModifiers modifiers = GuiController.Instance.Modifiers;
            Vector2 size = (Vector2)modifiers.getValue("size");
            Vector3 position = (Vector3)modifiers.getValue("position");
            Vector3 rotation = (Vector3)modifiers.getValue("rotation");

            collider.Center = position;
            collider.Rotation = rotation;
            collider.Radius = size.X;
            collider.Length = size.Y;

            collider.updateValues();

            if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, collider))
                collisionableSphere.setRenderColor(collisionColor);
            else
                collisionableSphere.setRenderColor(noCollisionColor);

            collider.render();
            collisionableSphere.render();
        }

        public override void close()
        {
            collider.dispose();
            collisionableSphere.dispose();
        }

    }
}
