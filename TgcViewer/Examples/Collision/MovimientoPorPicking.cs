using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo MovimientoPorPicking:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Picking
    /// 
    /// Muestra como desplazar un objeto por un escenario utilizando la técnica de Picking.
    /// Al hacer clic sobre el terreno, se detecta la posición seleccionada por el mouse
    /// y el objeto se traslada hasta ahí.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class MovimientoPorPicking : TgcExample
    {
        TgcBox suelo;
        TgcMesh mesh;
        TgcPickingRay pickingRay;
        Vector3 newPosition;
        bool applyMovement;
        TgcBox collisionPointMesh;
        TgcArrow directionArrow;
        Vector3 originalMeshRot;
        Matrix meshRotationMatrix;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar suelo
            TgcTexture texture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg");
            suelo = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(5000, 0.1f, 5000), texture);


            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay();


            //Cargar nave
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\NaveEspacial\\NaveEspacial-TgcScene.xml");
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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Si hacen clic con el mouse, ver si hay colision con el suelo
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
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
                    Vector3 direction = Vector3.Normalize(newPosition - mesh.Position);
                    float angle = FastMath.Acos(Vector3.Dot(originalMeshRot, direction));
                    Vector3 axisRotation = Vector3.Cross(originalMeshRot, direction);
                    meshRotationMatrix = Matrix.RotationAxis(axisRotation, angle);
                }
            }


            float speed = (float)GuiController.Instance.Modifiers["speed"];


            //Interporlar movimiento, si hay que mover
            if (applyMovement)
            {
                //Ver si queda algo de distancia para mover
                Vector3 posDiff = newPosition - mesh.Position;
                float posDiffLength = posDiff.LengthSq();
                if (posDiffLength > float.Epsilon)
                {
                    //Movemos el mesh interpolando por la velocidad
                    float currentVelocity = speed * elapsedTime;
                    posDiff.Normalize();
                    posDiff.Multiply(currentVelocity);

                    //Ajustar cuando llegamos al final del recorrido
                    Vector3 newPos = mesh.Position + posDiff;
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
