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
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;

namespace Examples.Tutorial
{
    /// <summary>
    /// Tutorial 8:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detecci�n de Colisiones - Bounding Box.
    /// 
    /// Muestra como mover un objeto sobre una escena evitando chocar con el resto de los objetos.
    /// 
    /// Autor: Mat�as Leone
    /// 
    /// </summary>
    public class Tutorial8 : TgcExample
    {
        const float MOVEMENT_SPEED = 200f;

        TgcScene scene;
        TgcMesh mainMesh;


        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 8";
        }

        public override string getDescription()
        {
            return "Muestra como mover un objeto sobre una escena evitando chocar con el resto de los objetos.";
        }


        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar escena
            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Cargar mesh principal
            mainMesh = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];

            //Movemos el mesh un poco para arriba. Porque sino choca con el piso todo el tiempo y no se puede mover.
            mainMesh.move(0, 5, 0);

            //Camera en 3ra persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(mainMesh.Position, 200, 300);
        }

        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;


            //Procesamos input de teclado para mover el objeto principal en el plano XZ
            TgcD3dInput input = GuiController.Instance.D3dInput;
            Vector3 movement = new Vector3(0, 0, 0);
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movement.X = -1;
            }
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movement.Z = 1;
            }


            //Guardar posicion original antes de cambiarla
            Vector3 originalPos = mainMesh.Position;

            //Aplicar movimiento
            movement *= MOVEMENT_SPEED * elapsedTime;
            mainMesh.move(movement);

            //Chequear si el objeto principal en su nueva posici�n choca con alguno de los objetos de la escena.
            //Si es as�, entonces volvemos a la posici�n original.
            //Cada TgcMesh tiene un objeto llamado BoundingBox. El BoundingBox es una caja 3D que representa al objeto
            //de forma simplificada (sin tener en cuenta toda la complejidad interna del modelo).
            //Este BoundingBox se utiliza para chequear si dos objetos colisionan entre s�.
            //El framework posee la clase TgcCollisionUtils con muchos algoritmos de colisi�n de distintos tipos de objetos.
            //Por ejemplo chequear si dos cajas colisionan entre s�, o dos esferas, o esfera con caja, etc.
            bool collisionFound = false;
            foreach (TgcMesh mesh in scene.Meshes)
            {
                //Los dos BoundingBox que vamos a testear
                TgcBoundingBox mainMeshBoundingBox = mainMesh.BoundingBox;
                TgcBoundingBox sceneMeshBoundingBox = mesh.BoundingBox;

                //Ejecutar algoritmo de detecci�n de colisiones
                TgcCollisionUtils.BoxBoxResult collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                //Hubo colisi�n con un objeto. Guardar resultado y abortar loop.
                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    collisionFound = true;
                    break;
                }
            }

            //Si hubo alguna colisi�n, entonces restaurar la posici�n original del mesh
            if (collisionFound)
            {
                mainMesh.Position = originalPos;
            }



            //Actualizar posicion de c�mara
            GuiController.Instance.ThirdPersonCamera.Target = mainMesh.Position;


            //Dibujar
            mainMesh.render();
            scene.renderAll();

            //En este ejemplo a modo de debug vamos a dibujar los BoundingBox de todos los objetos.
            //Asi puede verse como se efect�a el testeo de colisiones.
            mainMesh.BoundingBox.render();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.BoundingBox.render();
            }

        }

        public override void close()
        {
            scene.disposeAll();
            mainMesh.dispose();
        }

    }
}
