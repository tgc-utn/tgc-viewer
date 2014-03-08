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
    /// Tutorial 7:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.
    /// 
    /// Autor: Matías Leone
    /// 
    /// </summary>
    public class Tutorial7 : TgcExample
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
            return "Tutorial 7";
        }

        public override string getDescription()
        {
            return "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.";
        }


        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //En este ejemplo primero cargamos una escena 3D entera.
            TgcSceneLoader loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Luego cargamos otro modelo aparte que va a hacer el objeto que controlamos con el teclado
            TgcScene scene2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");
            
            //Solo nos interesa el primer modelo de esta escena (tiene solo uno)
            mainMesh = scene2.Meshes[0];

            //Vamos a utilizar la cámara en 3ra persona para que siga al objeto principal a medida que se mueve
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

            //Aplicar movimiento
            movement *= MOVEMENT_SPEED * elapsedTime;
            mainMesh.move(movement);

            //Hacer que la cámara en 3ra persona se ajuste a la nueva posición del objeto
            GuiController.Instance.ThirdPersonCamera.Target = mainMesh.Position;


            //Dibujar objeto principal
            //Siempre primero hacer todos los cálculos de lógica e input y luego al final dibujar todo (ciclo update-render)
            mainMesh.render();

            //Dibujamos la escena
            scene.renderAll();

        }

        public override void close()
        {
            scene.disposeAll();
            mainMesh.dispose();
        }

    }
}
