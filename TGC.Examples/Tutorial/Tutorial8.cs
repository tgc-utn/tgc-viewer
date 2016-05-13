using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls.Modifier;
using TgcUserVars = TGC.Core.UserControls.TgcUserVars;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 8:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Bounding Box.
    ///     Muestra como mover un objeto sobre una escena evitando chocar con el resto de los objetos.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial8 : TgcExample
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcMesh mainMesh;
        private TgcScene scene;

        public Tutorial8(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara) : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            this.Category = "Tutorial";
            this.Name = "Tutorial 8";
            this.Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado.";
        }

        public override void Init()
        {
            //Cargar escena
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Cargar mesh principal
            mainMesh = loader.loadSceneFromFile(this.MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];

            //Movemos el mesh un poco para arriba. Porque sino choca con el piso todo el tiempo y no se puede mover.
            mainMesh.move(0, 5, 0);

            //Camera en 3ra persona
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

            //Guardar posicion original antes de cambiarla
            var originalPos = mainMesh.Position;

            //Aplicar movimiento
            movement *= MOVEMENT_SPEED * this.ElapsedTime;
            mainMesh.move(movement);

            //Chequear si el objeto principal en su nueva posición choca con alguno de los objetos de la escena.
            //Si es así, entonces volvemos a la posición original.
            //Cada TgcMesh tiene un objeto llamado BoundingBox. El BoundingBox es una caja 3D que representa al objeto
            //de forma simplificada (sin tener en cuenta toda la complejidad interna del modelo).
            //Este BoundingBox se utiliza para chequear si dos objetos colisionan entre sí.
            //El framework posee la clase TgcCollisionUtils con muchos algoritmos de colisión de distintos tipos de objetos.
            //Por ejemplo chequear si dos cajas colisionan entre sí, o dos esferas, o esfera con caja, etc.
            var collisionFound = false;
            foreach (var mesh in scene.Meshes)
            {
                //Los dos BoundingBox que vamos a testear
                var mainMeshBoundingBox = mainMesh.BoundingBox;
                var sceneMeshBoundingBox = mesh.BoundingBox;

                //Ejecutar algoritmo de detección de colisiones
                var collisionResult = TgcCollisionUtils.classifyBoxBox(mainMeshBoundingBox, sceneMeshBoundingBox);

                //Hubo colisión con un objeto. Guardar resultado y abortar loop.
                if (collisionResult != TgcCollisionUtils.BoxBoxResult.Afuera)
                {
                    collisionFound = true;
                    break;
                }
            }

            //Si hubo alguna colisión, entonces restaurar la posición original del mesh
            if (collisionFound)
            {
                mainMesh.Position = originalPos;
            }

            //Actualizar posicion de cámara
            ((TgcThirdPersonCamera)this.Camara).Target = mainMesh.Position;

            //Dibujar
            mainMesh.render();
            scene.renderAll();

            //En este ejemplo a modo de debug vamos a dibujar los BoundingBox de todos los objetos.
            //Asi puede verse como se efectúa el testeo de colisiones.
            mainMesh.BoundingBox.render();
            foreach (var mesh in scene.Meshes)
            {
                mesh.BoundingBox.render();
            }

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