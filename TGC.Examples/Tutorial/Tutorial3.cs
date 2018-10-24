using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 3:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    /// 	# Unidad 6 - Detección de Colisiones - Bounding Box.
    ///     Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos.
    ///     Autor: Matias Leone
    /// </summary>
    public class Tutorial3 : TGCExampleViewer
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcThirdPersonCamera camaraInterna;
        private TgcMesh mainMesh;
        private TgcScene scene;

        public Tutorial3(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Tutorial";
            Name = "Tutorial 3";
            Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos.";
        }

        public override void Init()
        {
            //En este ejemplo primero cargamos una escena 3D entera.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            //Luego cargamos otro modelo aparte que va a hacer el objeto que controlamos con el teclado
            var scene2 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //Solo nos interesa el primer modelo de esta escena (tiene solo uno)
            mainMesh = scene2.Meshes[0];
            mainMesh.AutoTransformEnable = true;
            //Movemos el mesh un poco para arriba. Porque sino choca con el piso todo el tiempo y no se puede mover.
            mainMesh.Move(0, 5, 0);

            //Vamos a utilizar la camara en 3ra persona para que siga al objeto principal a medida que se mueve
            camaraInterna = new TgcThirdPersonCamera(mainMesh.Position, 200, 300);
            Camara = camaraInterna;
        }

        public override void Update()
        {
            PreUpdate();

            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;

            //Declaramos un vector de movimiento inicializado en cero.
            //El movimiento sobre el suelo es sobre el plano XZ.
            //Sobre XZ nos movemos con las flechas del teclado o con las letas WASD.
            var movement = TGCVector3.Empty;

            //Movernos de izquierda a derecha, sobre el eje X.
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movement.X = -1;
            }

            //Movernos adelante y atras, sobre el eje Z.
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

            //Multiplicar movimiento por velocidad y elapsedTime
            movement *= MOVEMENT_SPEED * ElapsedTime;
            mainMesh.Move(movement);

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

            //Hacer que la camara en 3ra persona se ajuste a la nueva posicion del objeto
            camaraInterna.Target = mainMesh.Position;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Dibujar objeto principal
            //Siempre primero hacer todos los calculos de logica e input y luego al final dibujar todo (ciclo update-render)
            mainMesh.Render();

            //Dibujamos la escena
            scene.RenderAll();

            //En este ejemplo a modo de debug vamos a dibujar los BoundingBox de todos los objetos.
            //Asi puede verse como se efectúa el testeo de colisiones.
            mainMesh.BoundingBox.Render();
            foreach (var mesh in scene.Meshes)
            {
                mesh.BoundingBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
            mainMesh.Dispose();
        }
    }
}