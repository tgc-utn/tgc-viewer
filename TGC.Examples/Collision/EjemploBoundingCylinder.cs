using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

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

        public EjemploBoundingCylinder(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "BoundingCylinder";
            Description =
                "Muestra el testeo de colision entre cilindro orientable y esfera. Hacer click sobre el viewport para testear colision PickingRay-Cylinder.";
        }

        public override void Init()
        {
            collider = new TgcBoundingCylinder(new Vector3(0, 0, 0), 2, 4);
            collisionableSphere = new TgcBoundingSphere(new Vector3(0, 0, -6), 3);

            Modifiers.addVertex2f("size", new Vector2(1, 1), new Vector2(5, 10), new Vector2(2, 5));
            Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));
            var angle = FastMath.TWO_PI;
            Modifiers.addVertex3f("rotation", new Vector3(-angle, -angle, -angle), new Vector3(angle, angle, angle),
                new Vector3(0, 0, 0));

            collider.setRenderColor(Color.LimeGreen);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            var size = (Vector2)Modifiers.getValue("size");
            var position = (Vector3)Modifiers.getValue("position");
            var rotation = (Vector3)Modifiers.getValue("rotation");

            collider.Center = position;
            collider.Rotation = rotation;
            collider.Radius = size.X;
            collider.Length = size.Y;

            collider.updateValues();

            if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, collider))
                collisionableSphere.setRenderColor(collisionColor);
            else
                collisionableSphere.setRenderColor(noCollisionColor);

            if (pickingTimeLeft > 0) pickingTimeLeft -= ElapsedTime;
            else collider.setRenderColor(noCollisionColor);

            if (TgcD3dInput.Instance.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
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

            helperPostRender();
        }

        public override void Close()
        {
            base.Close();

            collider.dispose();
            collisionableSphere.dispose();
        }
    }
}