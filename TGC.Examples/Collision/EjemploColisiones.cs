using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Util;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploColisiones:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    ///     Muestra como utilizar detección de colisiones con BoundingBox.
    ///     Además muestra como desplazar un modelo animado en base a la entrada de teclado.
    ///     El modelo animado utiliza la herramienta TgcKeyFrameLoader.
    ///     La cámara se encuentra fija en este ejemplo.
    ///     Los obstáculos se cargan como modelos estáticos con TgcSceneLoader
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploColisiones : TgcExample
    {
        //Velocidad de desplazamiento
        private const float VELOCIDAD_DESPLAZAMIENTO = 200f;

        private Vector3 move = new Vector3();
        private List<TgcMesh> obstaculos;
        private TgcSkeletalMesh personaje;

        private TgcBox piso;

        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Detección Simple";
        }

        public override string getDescription()
        {
            return "Ejemplo de Detección de Colisiones y manejo de Input. Movimiento con W, A, S, D.";
        }

        public override void init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\pasto.jpg");
            piso = TgcBox.fromSize(new Vector3(1000, 1, 1000), pisoTexture);

            //Cargar obstaculos y posicionarlos
            var loader = new TgcSceneLoader();
            obstaculos = new List<TgcMesh>();
            TgcScene scene;
            TgcMesh obstaculo;

            //Obstaculo 1: Malla estatática de Box de formato TGC
            scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstáculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(-100, 20, 0);
            obstaculos.Add(obstaculo);

            //Obstaculo 2: Malla estatática de Box de formato TGC
            scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstáculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(0, 20, 100);
            //Le cambiamos la textura a este modelo particular
            obstaculo.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg")
            });
            obstaculos.Add(obstaculo);

            //Obstaculo 2: Malla estatática de Box de formato TGC
            scene = loader.loadSceneFromFile(
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\" + "Box-TgcScene.xml",
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstáculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(100, 20, 100);
            obstaculos.Add(obstaculo);

            //Cargar personaje con animaciones con herramienta TgcKeyFrameLoader
            var keyFrameLoader = new TgcSkeletalLoader();
            personaje = keyFrameLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\",
                new[]
                {
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" +
                    "Caminando-TgcSkeletalAnim.xml"
                });

            //Configurar animacion inicial
            personaje.playAnimation("Caminando", true);

            //Escalar y posicionar
            personaje.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            personaje.Position = new Vector3(0, 0, 0);

            //Hacer que la cámara mire hacia un determinado lugar del escenario
            GuiController.Instance.setCamera(new Vector3(-80, 165, 230), new Vector3(0, 0, 0));

            //Deshabilitar camara para que no interfiera con los controles de nuestro ejemplo
            GuiController.Instance.RotCamera.Enable = false;

            //Modifier para habilitar o no el renderizado del BoundingBox del personaje
            GuiController.Instance.Modifiers.addBoolean("showBoundingBox", "Bouding Box", false);
        }

        public override void render(float elapsedTime)
        {
            //Ver si hay que mostrar el BoundingBox
            var showBB = (bool)GuiController.Instance.Modifiers.getValue("showBoundingBox");

            //Calcular proxima posicion de personaje segun Input
            var move = new Vector3(0, 0, 0);

            //Multiplicar la velocidad por el tiempo transcurrido, para no acoplarse al CPU
            var speed = VELOCIDAD_DESPLAZAMIENTO * elapsedTime;

            var d3dInput = GuiController.Instance.D3dInput;
            var moving = false;

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                move.Z = -speed;
                personaje.Rotation = new Vector3(0, 0, 0);
                moving = true;
            }

            //Atras
            else if (d3dInput.keyDown(Key.S))
            {
                move.Z = speed;
                personaje.Rotation = new Vector3(0, (float)Math.PI, 0);
                moving = true;
            }

            //Izquierda
            else if (d3dInput.keyDown(Key.A))
            {
                move.X = +speed;
                personaje.Rotation = new Vector3(0, -(float)Math.PI / 2, 0);
                moving = true;
            }

            //Derecha
            else if (d3dInput.keyDown(Key.D))
            {
                move.X = -speed;
                personaje.Rotation = new Vector3(0, (float)Math.PI / 2, 0);
                moving = true;
            }

            //Si hubo desplazamientos
            if (moving)
            {
                //Mover personaje
                var lastPos = personaje.Position;
                personaje.move(move);

                //Detectar colisiones de BoundingBox utilizando herramienta TgcCollisionUtils
                var collide = false;
                foreach (var obstaculo in obstaculos)
                {
                    var result = TgcCollisionUtils.classifyBoxBox(personaje.BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro ||
                        result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collide = true;
                        break;
                    }
                }

                //Si hubo colision, restaurar la posicion anterior
                if (collide)
                {
                    personaje.Position = lastPos;
                }
            }

            //Renderizar piso
            piso.render();

            //Renderizar obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.render();
                //Renderizar BoundingBox si asi lo pidieron
                if (showBB)
                {
                    obstaculo.BoundingBox.render();
                }
            }

            //Render personaje
            personaje.animateAndRender(elapsedTime);
            //Renderizar BoundingBox si asi lo pidieron
            if (showBB)
            {
                personaje.BoundingBox.render();
            }
        }

        public override void close()
        {
            piso.dispose();
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.dispose();
            }
            personaje.dispose();
        }
    }
}