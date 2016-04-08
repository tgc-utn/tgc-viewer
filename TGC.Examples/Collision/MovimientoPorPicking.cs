using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo MovimientoPorPicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Picking
    ///     Muestra como desplazar un objeto por un escenario utilizando la técnica de Picking.
    ///     Al hacer clic sobre el terreno, se detecta la posición seleccionada por el mouse
    ///     y el objeto se traslada hasta ahí.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class MovimientoPorPicking : TgcExample
    {
        private bool applyMovement;
        private TgcBox collisionPointMesh;
        private TgcArrow directionArrow;
        private TgcMesh mesh;
        private Matrix meshRotationMatrix;
        private Vector3 newPosition;
        private Vector3 originalMeshRot;
        private TgcPickingRay pickingRay;
        private TgcBox suelo;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Movimiento por Picking";
        }

        public override string getDescription()
        {
            return "Desplazamiento de un objeto por medio de Picking. Hacer clic sobre el suelo para desplazar la nave.";
        }

        public override void init()
        {
            //Cargar suelo
            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg");
            suelo = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(5000, 0.1f, 5000), texture);

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay();

            //Cargar nave
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\NaveEspacial\\NaveEspacial-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Rotación original de la malla, hacia -Z
            originalMeshRot = new Vector3(0, 0, -1);

            //Manipulamos los movimientos del mesh a mano
            mesh.AutoTransformEnable = false;
            meshRotationMatrix = Matrix.Identity;

            newPosition = mesh.Position;
            applyMovement = false;

            //Crear caja para marcar en que lugar hubo colision
            collisionPointMesh = TgcBox.fromSize(new Vector3(3, 100, 3), Color.Red);

            //Flecha para marcar la dirección
            directionArrow = new TgcArrow();
            directionArrow.Thickness = 5;
            directionArrow.HeadSize = new Vector2(10, 10);

            //Camara en tercera persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(mesh.Position, 800, 1500);

            GuiController.Instance.Modifiers.addFloat("speed", 1000, 5000, 2500);
        }

        public override void render(float elapsedTime)
        {
            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colisión en base a posición del mouse
                pickingRay.updateRay();

                //Detectar colisión Ray-AABB
                if (TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, suelo.BoundingBox, out newPosition))
                {
                    //Fijar nueva posición destino
                    applyMovement = true;

                    collisionPointMesh.Position = newPosition;
                    directionArrow.PEnd = new Vector3(newPosition.X, 30f, newPosition.Z);

                    //Rotar modelo en base a la nueva dirección a la que apunta
                    var direction = Vector3.Normalize(newPosition - mesh.Position);
                    var angle = FastMath.Acos(Vector3.Dot(originalMeshRot, direction));
                    var axisRotation = Vector3.Cross(originalMeshRot, direction);
                    meshRotationMatrix = Matrix.RotationAxis(axisRotation, angle);
                }
            }

            var speed = (float)GuiController.Instance.Modifiers["speed"];

            //Interporlar movimiento, si hay que mover
            if (applyMovement)
            {
                //Ver si queda algo de distancia para mover
                var posDiff = newPosition - mesh.Position;
                var posDiffLength = posDiff.LengthSq();
                if (posDiffLength > float.Epsilon)
                {
                    //Movemos el mesh interpolando por la velocidad
                    var currentVelocity = speed * elapsedTime;
                    posDiff.Normalize();
                    posDiff.Multiply(currentVelocity);

                    //Ajustar cuando llegamos al final del recorrido
                    var newPos = mesh.Position + posDiff;
                    if (posDiff.LengthSq() > posDiffLength)
                    {
                        newPos = newPosition;
                    }

                    //Actualizar flecha de movimiento
                    directionArrow.PStart = new Vector3(mesh.Position.X, 30f, mesh.Position.Z);
                    directionArrow.updateValues();

                    //Actualizar posicion del mesh
                    mesh.Position = newPos;

                    //Como desactivamos la transformacion automatica, tenemos que armar nosotros la matriz de transformacion
                    mesh.Transform = meshRotationMatrix * Matrix.Translation(mesh.Position);

                    //Actualizar camara
                    GuiController.Instance.ThirdPersonCamera.Target = mesh.Position;
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
                collisionPointMesh.render();
                directionArrow.render();
            }

            suelo.render();
            mesh.render();
        }

        public override void close()
        {
            suelo.dispose();
            mesh.dispose();
            collisionPointMesh.dispose();
        }
    }
}