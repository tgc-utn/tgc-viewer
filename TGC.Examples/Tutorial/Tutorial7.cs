using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls.Modifier;
using TgcUserVars = TGC.Core.UserControls.TgcUserVars;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 7:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial7 : TgcExample
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcMesh mainMesh;
        private TgcScene scene;

        public Tutorial7(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara) : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            this.Category = "Tutorial";
            this.Name = "Tutorial 7";
            this.Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.";
        }

        public override void Init()
        {
            //En este ejemplo primero cargamos una escena 3D entera.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Luego cargamos otro modelo aparte que va a hacer el objeto que controlamos con el teclado
            var scene2 = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //Solo nos interesa el primer modelo de esta escena (tiene solo uno)
            mainMesh = scene2.Meshes[0];

            //Vamos a utilizar la cámara en 3ra persona para que siga al objeto principal a medida que se mueve
            this.Camara = new TgcThirdPersonCamera();
            ((TgcThirdPersonCamera)this.Camara).setCamera(mainMesh.Position, 200, 300);
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override void Render()
        {
            this.IniciarEscena();
            base.Render();

            //Procesamos input de teclado para mover el objeto principal en el plano XZ
            var input = TgcD3dInput.Instance;
            var movement = new Vector3(0, 0, 0);
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
            movement *= MOVEMENT_SPEED * this.ElapsedTime;
            mainMesh.move(movement);

            //Hacer que la cámara en 3ra persona se ajuste a la nueva posición del objeto
            ((TgcThirdPersonCamera)this.Camara).Target = mainMesh.Position;

            //Dibujar objeto principal
            //Siempre primero hacer todos los cálculos de lógica e input y luego al final dibujar todo (ciclo update-render)
            mainMesh.render();

            //Dibujamos la escena
            scene.renderAll();

            this.FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            scene.disposeAll();
            mainMesh.dispose();
        }
    }
}