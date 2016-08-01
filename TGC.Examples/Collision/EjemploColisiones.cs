using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core.Camara;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploColisiones:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Deteccion de Colisiones - BoundingBox
    ///     Muestra como utilizar deteccion de colisiones con BoundingBox.
    ///     Ademas muestra como desplazar un modelo animado en base a la entrada de teclado.
    ///     El modelo animado utiliza la herramienta TgcKeyFrameLoader.
    ///     La camara se encuentra fija en este ejemplo.
    ///     Los obstaculos se cargan como modelos estaticos con TgcSceneLoader
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploColisiones : TGCExampleViewer
    {
        //Velocidad de desplazamiento
        private const float VELOCIDAD_DESPLAZAMIENTO = 200f;

        private Vector3 move;
        private List<TgcMesh> obstaculos;
        private TgcSkeletalMesh personaje;

        private TgcBox piso;

        public EjemploColisiones(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "Deteccion Simple";
            Description = "Ejemplo de Deteccion de Colisiones y manejo de Input. Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\pasto.jpg");
            piso = TgcBox.fromSize(new Vector3(1000, 1, 1000), pisoTexture);

            //Cargar obstaculos y posicionarlos
            var loader = new TgcSceneLoader();
            obstaculos = new List<TgcMesh>();
            TgcScene scene;
            TgcMesh obstaculo;

            //Obstaculo 1: Malla estatica de Box de formato TGC
            scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Box\\Box-TgcScene.xml",
                MediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstaculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(-100, 20, 0);
            obstaculos.Add(obstaculo);

            //Obstaculo 2: Malla estatica de Box de formato TGC
            scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Box\\Box-TgcScene.xml",
                MediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstaculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(0, 20, 100);
            //Le cambiamos la textura a este modelo particular
            obstaculo.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg")
            });
            obstaculos.Add(obstaculo);

            //Obstaculo 2: Malla estatica de Box de formato TGC
            scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Box\\Box-TgcScene.xml",
                MediaDir + "ModelosTgc\\Box\\");
            //Escalarlo, posicionarlo y agregar a array de obstaculos
            obstaculo = scene.Meshes[0];
            obstaculo.Scale = new Vector3(1, 2, 1);
            obstaculo.move(100, 20, 100);
            obstaculos.Add(obstaculo);

            //Cargar personaje con animaciones con herramienta TgcKeyFrameLoader
            var keyFrameLoader = new TgcSkeletalLoader();
            personaje =
                keyFrameLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml"
                    });

            //Configurar animacion inicial
            personaje.playAnimation("Caminando", true);

            //Escalar y posicionar
            personaje.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            personaje.Position = new Vector3(0, 0, 0);

            //Hacer que la camara mire hacia un determinado lugar del escenario
            Camara = new TgcRotationalCamera(new Vector3(-80, 165, 230), 200f, Input);

            //Modifier para habilitar o no el renderizado del BoundingBox del personaje
            Modifiers.addBoolean("showBoundingBox", "Bouding Box", false);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Ver si hay que mostrar el BoundingBox
            var showBB = (bool)Modifiers.getValue("showBoundingBox");

            //Calcular proxima posicion de personaje segun Input
            move = new Vector3(0, 0, 0);

            //Multiplicar la velocidad por el tiempo transcurrido, para no acoplarse al CPU
            var speed = VELOCIDAD_DESPLAZAMIENTO * ElapsedTime;

            var moving = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                move.Z = -speed;
                personaje.Rotation = new Vector3(0, 0, 0);
                moving = true;
            }

            //Atras
            else if (Input.keyDown(Key.S))
            {
                move.Z = speed;
                personaje.Rotation = new Vector3(0, (float)Math.PI, 0);
                moving = true;
            }

            //Izquierda
            else if (Input.keyDown(Key.A))
            {
                move.X = +speed;
                personaje.Rotation = new Vector3(0, -(float)Math.PI / 2, 0);
                moving = true;
            }

            //Derecha
            else if (Input.keyDown(Key.D))
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
            personaje.animateAndRender(ElapsedTime);
            //Renderizar BoundingBox si asi lo pidieron
            if (showBB)
            {
                personaje.BoundingBox.render();
            }

            PostRender();
        }

        public override void Dispose()
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