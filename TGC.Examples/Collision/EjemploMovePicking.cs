using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo MovimientoPorPicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detecci�n de Colisiones - Picking
    ///     Muestra como desplazar un objeto por un escenario utilizando la t�cnica de Picking.
    ///     Al hacer clic sobre el terreno, se detecta la posici�n seleccionada por el mouse
    ///     y el objeto se traslada hasta ah�.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMovePicking : TGCExampleViewer
    {
        private TGCFloatModifier speedModifier;

        private bool applyMovement;
        private TgcThirdPersonCamera camaraInterna;
        private TGCBox collisionPointMesh;
        private TgcArrow directionArrow;
        private TgcMesh mesh;
        private TGCMatrix meshRotationMatrix;
        private TGCVector3 newPosition;
        private TGCVector3 originalMeshRot;
        private TgcPickingRay pickingRay;
        private TgcPlane suelo;

        public EjemploMovePicking(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Colisiones con movimiento mouse";
            Description =
                "Desplazamiento de un objeto por medio de Picking. Hacer clic sobre el suelo para desplazar la nave.";
        }

        public override void Init()
        {
            //Cargar suelo
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg");
            suelo = new TgcPlane(new TGCVector3(-5000, 0, -5000), new TGCVector3(10000, 0f, 10000), TgcPlane.Orientations.XZplane, texture);

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay(Input);

            //Cargar nave
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\NaveEspacial\\NaveEspacial-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Rotaci�n original de la malla, hacia -Z
            originalMeshRot = new TGCVector3(0, 0, -1);

            //Manipulamos los movimientos del mesh a mano
            mesh.AutoTransform = false;
            meshRotationMatrix = TGCMatrix.Identity;

            newPosition = mesh.Position;
            applyMovement = false;

            //Crear caja para marcar en que lugar hubo colision
            collisionPointMesh = TGCBox.fromSize(new TGCVector3(3, 100, 3), Color.Red);

            //Flecha para marcar la direcci�n
            directionArrow = new TgcArrow();
            directionArrow.Thickness = 5;
            directionArrow.HeadSize = new TGCVector2(10, 10);

            //Camara en tercera persona
            camaraInterna = new TgcThirdPersonCamera(mesh.Position, 800, 1500);
            Camara = camaraInterna;
            speedModifier = AddFloat("speed", 1000, 5000, 2500);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colisi�n en base a posici�n del mouse
                pickingRay.updateRay();

                //Detectar colisi�n Ray-AABB
                if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out newPosition))
                {
                    //Fijar nueva posici�n destino
                    applyMovement = true;

                    collisionPointMesh.Position = newPosition;
                    directionArrow.PEnd = new TGCVector3(newPosition.X, 30f, newPosition.Z);

                    //Rotar modelo en base a la nueva direcci�n a la que apunta
                    var direction = TGCVector3.Normalize(newPosition - mesh.Position);
                    var angle = FastMath.Acos(TGCVector3.Dot(originalMeshRot, direction));
                    var axisRotation = TGCVector3.Cross(originalMeshRot, direction);
                    meshRotationMatrix = TGCMatrix.RotationAxis(axisRotation, angle);
                }
            }

            var speed = speedModifier.Value;

            //Interporlar movimiento, si hay que mover
            if (applyMovement)
            {
                //Ver si queda algo de distancia para mover
                var posDiff = newPosition - mesh.Position;
                var posDiffLength = posDiff.LengthSq();
                if (posDiffLength > float.Epsilon)
                {
                    //Movemos el mesh interpolando por la velocidad
                    var currentVelocity = speed * ElapsedTime;
                    posDiff.Normalize();
                    posDiff.Multiply(currentVelocity);

                    //Ajustar cuando llegamos al final del recorrido
                    var newPos = mesh.Position + posDiff;
                    if (posDiff.LengthSq() > posDiffLength)
                    {
                        newPos = newPosition;
                    }

                    //Actualizar flecha de movimiento
                    directionArrow.PStart = new TGCVector3(mesh.Position.X, 30f, mesh.Position.Z);
                    directionArrow.updateValues();

                    //Actualizar posicion del mesh
                    mesh.Position = newPos;

                    //Como desactivamos la transformacion automatica, tenemos que armar nosotros la matriz de transformacion
                    mesh.Transform = meshRotationMatrix * TGCMatrix.Translation(mesh.Position);

                    //Actualizar camara
                    camaraInterna.Target = mesh.Position;
                }
                //Se acabo el movimiento
                else
                {
                    applyMovement = false;
                }
            }

            //Mostrar caja con lugar en el que se hizo click, solo si hay movimiento
            if (applyMovement)
            {
                collisionPointMesh.Render();
                directionArrow.Render();
            }

            suelo.Render();
            mesh.Render();

            PostRender();
        }

        public override void Dispose()
        {
            suelo.Dispose();
            mesh.Dispose();
            collisionPointMesh.Dispose();
        }
    }
}