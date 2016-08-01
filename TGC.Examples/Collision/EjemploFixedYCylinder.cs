using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Muestra distintos testeos de colision: cilindro-esfera, cilindro-cubo, cilindro-cilindro.
    ///     Hacer click sobre el viewport para testear colision PickingRay-Cylinder.
    /// </summary>
    public class EjemploFixedYCylinder : TGCExampleViewer
    {
        private const float PICKING_TIME = 0.5f;
        private readonly Color collisionColor = Color.Red;

        private readonly Color noCollisionColor = Color.Yellow;
        private readonly Color pickingColor = Color.DarkGreen;
        private TgcBoundingCylinderFixedY collider;
        private TgcBoundingAxisAlignBox collisionableAABB;
        private TgcBoundingCylinderFixedY collisionableCylinder;
        private TgcBoundingSphere collisionableSphere;

        private float pickingTimeLeft;

        public EjemploFixedYCylinder(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "FixedYCylinder";
            Description =
                "Muestra distintos testeos de colision: cilindro-esfera, cilindro-cubo, cilindro-cilindro. Hacer click sobre el viewport para testear colision PickingRay-Cylinder.";
        }

        public override void Init()
        {
            collider = new TgcBoundingCylinderFixedY(new Vector3(0, 0, 0), 3, 3);
            collisionableSphere = new TgcBoundingSphere(new Vector3(-6, 0, 0), 3);
            collisionableAABB = new TgcBoundingAxisAlignBox(new Vector3(4, 0, -1), new Vector3(6, 2, 1));
            collisionableCylinder = new TgcBoundingCylinderFixedY(new Vector3(0, 0, -6), 2, 2);

            Modifiers.addVertex2f("size", new Vector2(1, 1), new Vector2(5, 10), new Vector2(2, 5));
            Modifiers.addVertex3f("position", new Vector3(-20, -20, -20), new Vector3(20, 20, 20), new Vector3(0, 0, 0));

            collider.setRenderColor(Color.LimeGreen);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var size = (Vector2)Modifiers.getValue("size");
            var position = (Vector3)Modifiers.getValue("position");

            collider.Center = position;
            collider.Radius = size.X;
            collider.Length = size.Y;

            collider.updateValues();

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

            if (pickingTimeLeft > 0) pickingTimeLeft -= ElapsedTime;
            else collider.setRenderColor(noCollisionColor);

            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                var pickingRay = new TgcPickingRay(Input);
                pickingRay.updateRay();
                if (TgcCollisionUtils.testRayCylinder(pickingRay.Ray, collider))
                {
                    pickingTimeLeft = PICKING_TIME;
                    collider.setRenderColor(pickingColor);
                }
            }

            collider.render();
            collisionableSphere.render();
            collisionableAABB.render();
            collisionableCylinder.render();

            PostRender();
        }

        public override void Dispose()
        {
            collider.dispose();
            collisionableSphere.dispose();
            collisionableAABB.dispose();
            collisionableCylinder.dispose();
        }
    }
}