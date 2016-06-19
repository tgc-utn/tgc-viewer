using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploColisionesThirdPerson
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    ///     Se recomienda leer primero EjemploColisiones que posee el mismo espiritu de ejemplo pero mas sencillo.
    ///     Muestra como utilizar la cámara en Tercera Persona junto con detección de colisiones
    ///     de BoundingBox y manejo de Input de Teclado.
    ///     Se utiliza una forma diferente de movimiento con el método moveOrientedY()
    ///     El modelo animado utiliza la herramienta TgcSkeletalLoader.
    ///     Se muestra como cambiarle la textura al modelo animado.
    ///     Los obstáculos son cargados utilizando TgcBox para crear cajas de tamaño dinámico, en lugar de utilizar
    ///     modelos estáticos como en el otro ejemplo.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploColisionesThirdPerson : TgcExample
    {
        private List<TgcBox> obstaculos;
        private TgcSkeletalMesh personaje;
        private TgcBox piso;
        private TgcThirdPersonCamera camaraInterna;

        public EjemploColisionesThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Collision";
            Name = "Detección + 3ra Persona";
            Description =
                "Ejemplo de Detección de Colisiones utilizando la cámara en Tercera Persona. Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = TgcBox.fromSize(new Vector3(0, -60, 0), new Vector3(1000, 5, 1000), pisoTexture);

            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;

            //Obstaculo 1
            obstaculo = TgcBox.fromSize(new Vector3(-100, 0, 0), new Vector3(80, 150, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromSize(new Vector3(50, 0, 200), new Vector3(80, 300, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromSize(new Vector3(300, 0, 100), new Vector3(80, 100, 150),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                    });

            //Le cambiamos la textura para diferenciarlo un poco
            personaje.changeDiffuseMaps(new[]
            {
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    MediaDir + "SkeletalAnimations\\Robot\\Textures\\uvwGreen.jpg")
            });

            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(0, -45, 0);
            personaje.Scale = new Vector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));

            //Configurar camara en Tercer Persona
            camaraInterna  = new TgcThirdPersonCamera(personaje.Position, 200, -300);
            Camara = camaraInterna;

            //Modifier para ver BoundingBox
            Modifiers.addBoolean("showBoundingBox", "Bouding Box", false);

            //Modifiers para desplazamiento del personaje
            Modifiers.addFloat("VelocidadCaminar", 1f, 400f, 250f);
            Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 120f);
        }

        public override void Update()
        {
            base.PreUpdate();
        }

        public override void Render()
        {
            base.PreRender();
            

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = (bool)Modifiers.getValue("showBoundingBox");

            //obtener velocidades de Modifiers
            var velocidadCaminar = (float)Modifiers.getValue("VelocidadCaminar");
            var velocidadRotacion = (float)Modifiers.getValue("VelocidadRotacion");

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var d3dInput = TgcD3dInput.Instance;
            var moving = false;
            var rotating = false;

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.rotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personaje.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                personaje.moveOrientedY(moveForward * ElapsedTime);

                //Detectar colisiones
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

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            //Render piso
            piso.render();

            //Render obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.render();
                if (showBB)
                {
                    obstaculo.BoundingBox.render();
                }
            }

            //Render personaje
            personaje.animateAndRender(ElapsedTime);
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