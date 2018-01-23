using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Muestra el testeo de colision entre cilindro orientable y esfera. Hacer click sobre el viewport para testear
    ///     colision PickingRay-Cylinder.
    /// </summary>
    public class EjemploBoundingCylinder : TGCExampleViewer
    {
        private const float PICKING_TIME = 0.5f;
        private readonly Color collisionColor = Color.Red;

        private readonly Color noCollisionColor = Color.Yellow;
        private readonly Color pickingColor = Color.DarkGreen;
        private TgcBoundingCylinder colliderCylinder;
        private TgcBoundingCylinderFixedY colliderCylinderFixedY;
        private TgcBoundingSphere collisionableSphere;
        private TgcMesh collisionableMeshAABB;
        private TgcBoundingCylinderFixedY collisionableCylinder;

        private float pickingTimeLeft;
        private TgcThirdPersonCamera camaraInterna;
        private TgcSkeletalMesh personaje;

        public EjemploBoundingCylinder(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "Bounding Cylinder Tests";
            Description =
                "Muestra el testeo de colision entre cilindro orientable y orientado en Y. Muestra distintos testeos de colision: cilindro-esfera, cilindro-cubo, cilindro-cilindro. Hacer click sobre el viewport para testear colision PickingRay-Cylinder.";
        }

        public override void Init()
        {
            //Objetos en movimiento.
            colliderCylinder = new TgcBoundingCylinder(TGCVector3.Empty, 2, 4);
            colliderCylinder.setRenderColor(Color.LimeGreen);
            colliderCylinderFixedY = new TgcBoundingCylinderFixedY(TGCVector3.Empty, 2, 4);
            colliderCylinderFixedY.setRenderColor(Color.LimeGreen);
            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                    });
            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Scale = new TGCVector3(0.04f, 0.04f, 0.04f);

            //El personaje esta en el 0,0,0 hay que bajarlo
            var size = personaje.BoundingBox.PMax.Y - personaje.BoundingBox.PMin.Y;
            personaje.Position = new TGCVector3(0, -3f, 0);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.RotateY(FastMath.PI);

            //Objetos estaticos, pueden ser mesh o objetos simplificados.
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Patrullero\\Patrullero-TgcScene.xml");
            collisionableMeshAABB = scene.Meshes[0];
            collisionableMeshAABB.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            collisionableMeshAABB.Position = new TGCVector3(6, 0, -2);
            collisionableCylinder = new TgcBoundingCylinderFixedY(new TGCVector3(-6, 0, 0), 2, 2);
            collisionableSphere = new TgcBoundingSphere(new TGCVector3(-3, 0, 10), 3);

            Modifiers.addBoolean("fixedY", "use fixed Y", true);
            //Modifier para ver BoundingBox del personaje
            Modifiers.addBoolean("showBoundingBox", "Personaje Bouding Box", false);
            Modifiers.addVertex2f("size", TGCVector2.One, new TGCVector2(5, 10), new TGCVector2(2, 5));
            var angle = FastMath.TWO_PI;
            Modifiers.addVertex3f("rotation", new TGCVector3(-angle, -angle, -angle), new TGCVector3(angle, angle, angle),
                new TGCVector3(FastMath.TWO_PI / 8, 0, FastMath.TWO_PI / 8));

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 25, -45);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();

            var velocidadCaminar = 25f * ElapsedTime;
            //Calcular proxima posicion de personaje segun Input
            var moving = false;
            var movement = TGCVector3.Empty;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                movement.Z = velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                movement.Z = -velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                movement.X = velocidadCaminar;
                moving = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                movement.X = -velocidadCaminar;
                moving = true;
            }
            //Si hubo desplazamiento
            if (moving)
            {
                //Aplicar movimiento
                colliderCylinder.move(movement);
                colliderCylinderFixedY.move(movement);
                personaje.Move(movement);

                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);
            }
            else
            {
                //Si no se esta moviendo, activar animacion de Parado
                personaje.playAnimation("Parado", true);
            }

            var size = (TGCVector2)Modifiers.getValue("size");
            var rotation = (TGCVector3)Modifiers.getValue("rotation");
            //Se tienen dos coliders, un cilindro con rotacion y otro orientado al eje Y.
            colliderCylinder.Rotation = rotation;
            colliderCylinder.Radius = size.X;
            colliderCylinder.Length = size.Y;
            //Actualizar este cilindro es mas costoso.
            colliderCylinder.updateValues();
            //Cilindro sin rotacion, para ser utilizado con personajes.
            colliderCylinderFixedY.Radius = size.X;
            colliderCylinderFixedY.Length = size.Y;
            //Un cilindro orientado es facil de actualizar.
            colliderCylinderFixedY.updateValues();

            if ((bool)Modifiers["fixedY"])
            {
                //Test de colisiones del cilindro orientado en Y, al ser mucho mas simple tiene mas test soportados por el framework.
                if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, colliderCylinderFixedY))
                    collisionableSphere.setRenderColor(collisionColor);
                else
                    collisionableSphere.setRenderColor(noCollisionColor);

                if (TgcCollisionUtils.testAABBCylinder(collisionableMeshAABB.BoundingBox, colliderCylinderFixedY))
                    collisionableMeshAABB.BoundingBox.setRenderColor(collisionColor);
                else
                    collisionableMeshAABB.BoundingBox.setRenderColor(noCollisionColor);

                if (TgcCollisionUtils.testCylinderCylinder(collisionableCylinder, colliderCylinderFixedY))
                    collisionableCylinder.setRenderColor(collisionColor);
                else
                    collisionableCylinder.setRenderColor(noCollisionColor);

                if (pickingTimeLeft > 0) pickingTimeLeft -= ElapsedTime;
                else colliderCylinderFixedY.setRenderColor(noCollisionColor);

                if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    var pickingRay = new TgcPickingRay(Input);
                    pickingRay.updateRay();
                    if (TgcCollisionUtils.testRayCylinder(pickingRay.Ray, colliderCylinderFixedY))
                    {
                        pickingTimeLeft = PICKING_TIME;
                        colliderCylinderFixedY.setRenderColor(pickingColor);
                    }
                }
            }
            else
            {
                //Test de colisiones del cilindro, la cantidad de test que tiene el framewor es acotada.
                if (TgcCollisionUtils.testSphereCylinder(collisionableSphere, colliderCylinder))
                    collisionableSphere.setRenderColor(collisionColor);
                else
                    collisionableSphere.setRenderColor(noCollisionColor);

                if (pickingTimeLeft > 0) pickingTimeLeft -= ElapsedTime;
                else colliderCylinder.setRenderColor(noCollisionColor);

                if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    var pickingRay = new TgcPickingRay(Input);
                    pickingRay.updateRay();
                    if (TgcCollisionUtils.testRayCylinder(pickingRay.Ray, colliderCylinder))
                    {
                        pickingTimeLeft = PICKING_TIME;
                        colliderCylinder.setRenderColor(pickingColor);
                    }
                }
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Bounding volumes.
            //Los bounding volumes realizan un update de los vertices en momento de render, por ello pueden ser mas lentos que utilizar transformadas.
            if ((bool)Modifiers["fixedY"])
                colliderCylinderFixedY.Render();
            else
                colliderCylinder.Render();

            //Render personaje
            personaje.Transform =
                TGCMatrix.Scaling(personaje.Scale)
                            * TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z)
                            * TGCMatrix.Translation(personaje.Position);
            personaje.animateAndRender(ElapsedTime);
            if ((bool)Modifiers["showBoundingBox"])
            {
                personaje.BoundingBox.Render();
            }

            //Render de objetos estaticos
            collisionableSphere.Render();
            collisionableMeshAABB.BoundingBox.Render();
            collisionableCylinder.Render();

            //Dibujar todo mallas.
            //Una vez actualizadas las diferentes posiciones internas solo debemos asignar la matriz de world.
            collisionableMeshAABB.Transform = TGCMatrix.Scaling(collisionableMeshAABB.Scale) * TGCMatrix.Translation(collisionableMeshAABB.Position);
            collisionableMeshAABB.Render();

            PostRender();
        }

        public override void Dispose()
        {
            colliderCylinder.Dispose();
            colliderCylinderFixedY.Dispose();
            collisionableSphere.Dispose();
            collisionableMeshAABB.Dispose();
            collisionableCylinder.Dispose();
        }
    }
}