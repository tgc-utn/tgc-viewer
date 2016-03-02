using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Utils;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploBoundingCylinder : TgcExample
    {
        private const float PICKING_TIME = 0.5f;
        private readonly Color collisionColor = Color.Red;

        private readonly Color noCollisionColor = Color.Yellow;
        private readonly Color pickingColor = Color.DarkGreen;
        private TgcBoundingCylinder collider;
        private TgcBoundingSphere collisionableSphere;

        private float pickingTimeLeft;

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
            return
                "Muestra el testeo de colision entre cilindro orientable y esfera. Hacer click sobre el viewport para testear colision PickingRay-Cylinder.";
        }

        public override void init()
        {
            collider = new TgcBoundingCylinder(new Vector3(0, 0, 0), 2, 4);
            collisionableSphere = new TgcBoundingSphere(new Vector3(0, 0, -6), 3);

            GuiController.Instance.Modifiers.addVertex2f("size", new Vector2(1, 1), new Vector2(5, 10),
                new Vector2(2, 5));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20),
                new Vector3(0, 0, 0));
            var angle = FastMath.TWO_PI;
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-angle, -angle, -angle),
                new Vector3(angle, angle, angle), new Vector3(0, 0, 0));

            collider.setRenderColor(Color.LimeGreen);
        }

        public override void render(float elapsedTime)
        {
            var modifiers = GuiController.Instance.Modifiers;
            var size = (Vector2)modifiers.getValue("size");
            var position = (Vector3)modifiers.getValue("position");
            var rotation = (Vector3)modifiers.getValue("rotation");

            collider.Center = position;
            collider.Rotation = rotation;
            collider.Radius = size.X;
            collider.Length = size.Y;

            collider.updateValues();

            if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, collider))
                collisionableSphere.setRenderColor(collisionColor);
            else
                collisionableSphere.setRenderColor(noCollisionColor);

            if (pickingTimeLeft > 0) pickingTimeLeft -= elapsedTime;
            else collider.setRenderColor(noCollisionColor);

            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                var pickingRay = new TgcPickingRay();
                pickingRay.updateRay();
                if (TgcCollisionUtils.testRayCylinder(pickingRay.Ray, collider))
                {
                    pickingTimeLeft = PICKING_TIME;
                    collider.setRenderColor(pickingColor);
                }
            }

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