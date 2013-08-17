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
    public class EjemploFixedYCylinder : TgcExample
    {
        private TgcFixedYBoundingCylinder collider;
        private TgcBoundingSphere collisionableSphere;
        private TgcBoundingBox collisionableAABB;
        private TgcFixedYBoundingCylinder collisionableCylinder;

        private Color noCollisionColor = Color.Yellow;
        private Color collisionColor = Color.Red;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "FixedYCylinder";
        }

        public override string getDescription()
        {
            return "Muestra distintos testeos de colision: cilindro-esfera, cilindro-cubo, cilindro-cilindro.";
        }

        public override void init()
        {
            collider = new TgcFixedYBoundingCylinder(new Vector3(0, 0, 0), 3, 3);
            collisionableSphere = new TgcBoundingSphere(new Vector3(-6, 0, 0), 3);
            collisionableAABB = new TgcBoundingBox(new Vector3(4, 0, -1), new Vector3(6, 2, 1));
            collisionableCylinder = new TgcFixedYBoundingCylinder(new Vector3(0, 0, -6), 2, 2);

            GuiController.Instance.Modifiers.addVertex2f("size", new Vector2(1, 1), new Vector2(5, 10), new Vector2(2, 5));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));

            collider.setRenderColor(Color.LimeGreen);
        }


        public override void render(float elapsedTime)
        {
            TgcModifiers modifiers = GuiController.Instance.Modifiers;
            Vector2 size = (Vector2)modifiers.getValue("size");
            Vector3 position = (Vector3)modifiers.getValue("position");

            collider.Center = position;
            collider.Radius = size.X;
            collider.Length = size.Y;

            if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, collider))
                collisionableSphere.setRenderColor(collisionColor);
            else
                collisionableSphere.setRenderColor(noCollisionColor);

            if (TgcCollisionUtils.testAABBCylinder(collisionableAABB, collider))
                collisionableAABB.setRenderColor(collisionColor);
            else
                collisionableAABB.setRenderColor(noCollisionColor);

            if (TgcCollisionUtils.testCylinderCylinder(collisionableCylinder, collider))
                collisionableCylinder.setRenderColor(collisionColor);
            else
                collisionableCylinder.setRenderColor(noCollisionColor);


            collider.render();
            collisionableSphere.render();
            collisionableAABB.render();
            collisionableCylinder.render();
        }

        public override void close()
        {
            collider.dispose();
            collisionableSphere.dispose();
            collisionableAABB.dispose();
            collisionableCylinder.dispose();
        }

    }
}
