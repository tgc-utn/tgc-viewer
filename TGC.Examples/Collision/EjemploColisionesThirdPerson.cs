using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.Camara;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

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
    ///     Autor: Matías Leone, Leandro Barbagallo, Rodrigo Garcia.
    /// </summary>
    public class EjemploColisionesThirdPerson : TGCExampleViewer
    {
        private TgcThirdPersonCamera camaraInterna;
        private List<TgcBox> obstaculos;
        private TgcSkeletalMesh personaje;
        private TgcPlane piso;

        public EjemploColisionesThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars,
            TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Collision";
            Name = "Movimientos 3ra Persona";
            Description =
                "Ejemplo de Detección de Colisiones de un personaje, utilizando la cámara en Tercera Persona. Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = new TgcPlane(new Vector3(-500, -60, -500), new Vector3(1000, 0, 1000), TgcPlane.Orientations.XZplane, pisoTexture);

            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;

            //Obstaculo 1
            obstaculo = TgcBox.fromSize(new Vector3(-100, 0, 0), new Vector3(80, 150, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\baldosaFacultad.jpg"));
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            obstaculo.AutoTransformEnable = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromSize(new Vector3(50, 0, 200), new Vector3(80, 300, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg"));
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            obstaculo.AutoTransformEnable = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromSize(new Vector3(300, 0, 100), new Vector3(80, 100, 150),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            obstaculo.AutoTransformEnable = true;
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
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            personaje.AutoTransformEnable = true;

            //Escalarlo porque es muy grande            
            personaje.Position = new Vector3(0, -45, 0);
            personaje.Scale = new Vector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 200, -300);
            Camara = camaraInterna;

            //Modifier para ver BoundingBox
            Modifiers.addBoolean("showBoundingBox", "Bouding Box", false);
            Modifiers.addBoolean("activateSliding", "Activate Sliding", true);

            //Modifiers para desplazamiento del personaje
            Modifiers.addFloat("VelocidadCaminar", 1f, 400f, 250f);
            Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 120f);
        }

        public override void Update()
        {
            PreUpdate();
            //obtener velocidades de Modifiers
            var velocidadCaminar = (float)Modifiers.getValue("VelocidadCaminar");
            var velocidadRotacion = (float)Modifiers.getValue("VelocidadRotacion");

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
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

                //NO SE RECOMIENDA UTILIZAR! moveOrientedY mueve el personaje segun la direccion actual, realiza operaciones de seno y coseno.
                personaje.moveOrientedY(moveForward * ElapsedTime);

                //Detectar colisiones
                var collide = false;
                //Guardamos los objetos colicionados para luego resolver la respuesta. (para este ejemplo simple es solo 1 caja)
                TgcBox collider = null;
                foreach (var obstaculo in obstaculos)
                {
                    if (TgcCollisionUtils.testAABBAABB(personaje.BoundingBox, obstaculo.BoundingBox))
                    {
                        collide = true;
                        collider = obstaculo;
                        break;
                    }
                }

                //Si hubo colision, restaurar la posicion anterior, CUIDADO!!!!!
                //Hay que tener cuidado con este tipo de respuesta a colision, puede darse el caso que el objeto este parcialmente dentro en este y en el frame anterior.
                //para solucionar el problema que tiene hacer este tipo de respuesta a colisiones y que los elementos no queden pegados hay varios algoritmos y hacks.
                //almacenar la posicion anterior no es lo mejor para responder a una colision.
                //Una primera aproximacion para evitar que haya inconsistencia es realizar sliding
                if (collide)
                {
                    //si no esta activo el sliding es la solucion anterior de este ejemplo.
                    if (!(bool)Modifiers["activateSliding"])
                        personaje.Position = lastPos; //Por como esta el framework actualmente esto actualiza el BoundingBox.
                    else
                    {
                        //La idea del slinding es simplificar el problema, sabemos que estamos moviendo bounding box alineadas a los ejes.
                        //Significa que si estoy colisionando con alguna de las caras de un AABB los planos siempre son los ejes coordenados.
                        //Entones creamos un rayo de movimiento, esto dado por la posicion anterior y la posicion actual.
                        var movementRay = lastPos - personaje.Position;
                        //Luego debemos clasificar sobre que plano estamos chocando y la direccion de movimiento
                        //Para todos los casos podemos deducir que la normal del plano cancela el movimiento en dicho plano.
                        //Esto quiere decir que podemos cancelar el movimiento en el plano y movernos en el otros.
                        var t = "";
                        var rx = Vector3.Empty;
                        var rz = Vector3.Empty;
                        if (personaje.BoundingBox.PMax.X > collider.BoundingBox.PMax.X && movementRay.X > 0)
                        {
                            t += "Colision X +";
                            personaje.Position = new Vector3(lastPos.X, lastPos.Y - movementRay.Y, lastPos.Z - movementRay.Z);
                        }
                        else if (personaje.BoundingBox.PMin.X < collider.BoundingBox.PMin.X && movementRay.X < 0)
                        {
                            t += "Colision X -";
                            personaje.Position = new Vector3(lastPos.X, lastPos.Y - movementRay.Y, lastPos.Z - movementRay.Z);
                        } 
                        if (personaje.BoundingBox.PMin.Z > collider.BoundingBox.PMin.Z && movementRay.Z > 0)
                        {
                            t += "Colision Z +";
                            personaje.Position = new Vector3(lastPos.X - movementRay.X, lastPos.Y - movementRay.Y, lastPos.Z);
                        }
                        else if (personaje.BoundingBox.PMin.Z < collider.BoundingBox.PMin.Z && movementRay.Z < 0)
                        {
                            t += "Colision Z -";
                            personaje.Position = new Vector3(lastPos.X - movementRay.X, lastPos.Y - movementRay.Y, lastPos.Z);
                        }
                        text = t;
                        //Este ejemplo solo se mueve en X y Z con lo cual realizar el test en el plano Y no tiene sentido.
                        /*if (personaje.BoundingBox.PMax.Y > collider.BoundingBox.PMax.Y && movementRay.Y > 0)
                        {
                            personaje.Position = new Vector3(lastPos.X - movementRay.X, lastPos.Y, lastPos.Z - movementRay.Z);
                        }
                        else if (personaje.BoundingBox.PMin.Y < collider.BoundingBox.PMin.Y && movementRay.Y < 0)
                        {
                            personaje.Position = new Vector3(lastPos.X - movementRay.X, lastPos.Y, lastPos.Z - movementRay.Z);
                        }*/
                    }
                }



            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

        }
        string text = "No colision";
        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = (bool)Modifiers.getValue("showBoundingBox");

            DrawText.drawText(text, 5, 20, System.Drawing.Color.Red);


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