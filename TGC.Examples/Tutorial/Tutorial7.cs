using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 7:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.
    ///     Autor: Matias Leone
    /// </summary>
    public class Tutorial7 : TGCExampleViewer
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcThirdPersonCamera camaraInterna;
        private TgcMesh mainMesh;
        private TgcScene scene;

        public Tutorial7(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 7";
            Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.";
        }

        public override void Init()
        {
            //En este ejemplo primero cargamos una escena 3D entera.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Luego cargamos otro modelo aparte que va a hacer el objeto que controlamos con el teclado
            var scene2 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //Solo nos interesa el primer modelo de esta escena (tiene solo uno)
            mainMesh = scene2.Meshes[0];

            //Vamos a utilizar la camara en 3ra persona para que siga al objeto principal a medida que se mueve
            camaraInterna = new TgcThirdPersonCamera(mainMesh.Position, 200, 300);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Procesamos input de teclado para mover el objeto principal en el plano XZ
            var movement = new Vector3(0, 0, 0);
            if (Input.keyDown(Key.Left) || Input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (Input.keyDown(Key.Right) || Input.keyDown(Key.D))
            {
                movement.X = -1;
            }
            if (Input.keyDown(Key.Up) || Input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (Input.keyDown(Key.Down) || Input.keyDown(Key.S))
            {
                movement.Z = 1;
            }

            //Aplicar movimiento
            movement *= MOVEMENT_SPEED * ElapsedTime;
            mainMesh.move(movement);

            //Hacer que la camara en 3ra persona se ajuste a la nueva posicion del objeto
            camaraInterna.Target = mainMesh.Position;

            //Dibujar objeto principal
            //Siempre primero hacer todos los calculos de logica e input y luego al final dibujar todo (ciclo update-render)
            mainMesh.render();

            //Dibujamos la escena
            scene.renderAll();

            PostRender();
        }

        public override void Dispose()
        {
            scene.disposeAll();
            mainMesh.dispose();
        }
    }
}