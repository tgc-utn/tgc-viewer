using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploColisionesThirdPerson
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Detecci�n de Colisiones - BoundingBox
    ///     Se recomienda leer primero EjemploColisiones que posee el mismo espiritu de ejemplo pero mas sencillo.
    ///     Muestra como utilizar la c�mara en Tercera Persona junto con detecci�n de colisiones
    ///     de BoundingBox y manejo de Input de Teclado.
    ///     Se utiliza una forma diferente de movimiento con el personaje segun la direccion actual, realiza operaciones de seno y coseno.
    ///     El modelo animado utiliza la herramienta TgcSkeletalLoader.
    ///     Se muestra como cambiarle la textura al modelo animado.
    ///     Los obst�culos son cargados utilizando TgcBox para crear cajas de tama�o din�mico, en lugar de utilizar
    ///     modelos est�ticos como en el otro ejemplo.
    ///     Autor: Mat�as Leone, Leandro Barbagallo, Rodrigo Garcia.
    /// </summary>
    public class EjemploMoveThirdPerson : TGCExampleViewer
    {
        private TGCBooleanModifier showBoundingBoxModifier;
        private TGCBooleanModifier activateSlidingModifier;
        private TGCFloatModifier velocidadCaminarModifier;
        private TGCFloatModifier velocidadRotacionModifier;

        private TgcThirdPersonCamera camaraInterna;
        private List<TGCBox> obstaculos;
        private TgcSkeletalMesh personaje;
        private TgcPlane piso;
        private string text = "No colision";

        public EjemploMoveThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Movimientos AABB 3ra Persona";
            Description = "Ejemplo de Detecci�n de Colisiones de un personaje, utilizando la c�mara en Tercera Persona. Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = new TgcPlane(new TGCVector3(-500, -60, -500), new TGCVector3(1000, 0, 1000), TgcPlane.Orientations.XZplane, pisoTexture);

            //Cargar obstaculos y posicionarlos. Los obst�culos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TGCBox>();
            TGCBox obstaculo;

            //Obstaculo 1
            obstaculo = TGCBox.fromSize(new TGCVector3(-100, 0, 0), new TGCVector3(80, 150, 80), TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\baldosaFacultad.jpg"));
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            obstaculo.AutoTransformEnable = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TGCBox.fromSize(new TGCVector3(50, 0, 200), new TGCVector3(80, 300, 80), TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg"));
            //No es recomendado utilizar autotransform en casos mas complicados, se pierde el control.
            obstaculo.AutoTransformEnable = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TGCBox.fromSize(new TGCVector3(300, 0, 100), new TGCVector3(80, 100, 150), TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
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
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "SkeletalAnimations\\Robot\\Textures\\uvwGreen.jpg")
            });

            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);

            //Escalarlo porque es muy grande
            personaje.Position = new TGCVector3(0, -45, 0);
            personaje.Scale = new TGCVector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180� porque esta mirando para el otro lado
            personaje.RotateY(Geometry.DegreeToRadian(180f));

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 200, -300);
            Camara = camaraInterna;

            //Modifier para ver BoundingBox
            showBoundingBoxModifier = AddBoolean("showBoundingBox", "Bouding Box", false);
            activateSlidingModifier = AddBoolean("activateSliding", "Activate Sliding", false);

            //Modifiers para desplazamiento del personaje
            velocidadCaminarModifier = AddFloat("VelocidadCaminar", 1f, 400f, 250f);
            velocidadRotacionModifier = AddFloat("VelocidadRotacion", 1f, 360f, 120f);
        }

        public override void Update()
        {
            PreUpdate();
            //obtener velocidades de Modifiers
            var velocidadCaminar = velocidadCaminarModifier.Value;
            var velocidadRotacion = velocidadRotacionModifier.Value;

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
                personaje.Rotation += new TGCVector3(0, rotAngle, 0);
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
                var moveF = moveForward * ElapsedTime;
                var z = (float)Math.Cos(personaje.Rotation.Y) * moveF;
                var x = (float)Math.Sin(personaje.Rotation.Y) * moveF;

                personaje.Position += new TGCVector3(x, 0, z);

                //Detectar colisiones
                var collide = false;
                //Guardamos los objetos colicionados para luego resolver la respuesta. (para este ejemplo simple es solo 1 caja)
                TGCBox collider = null;
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
                    if (!activateSlidingModifier.Value)
                    {
                        personaje.Position = lastPos; //Por como esta el framework actualmente esto actualiza el BoundingBox.
                        text = "";
                    }
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
                        var rs = TGCVector3.Empty;
                        if (((personaje.BoundingBox.PMax.X > collider.BoundingBox.PMax.X && movementRay.X > 0) ||
                            (personaje.BoundingBox.PMin.X < collider.BoundingBox.PMin.X && movementRay.X < 0)) &&
                            ((personaje.BoundingBox.PMax.Z > collider.BoundingBox.PMax.Z && movementRay.Z > 0) ||
                            (personaje.BoundingBox.PMin.Z < collider.BoundingBox.PMin.Z && movementRay.Z < 0)))
                        {
                            //Este primero es un caso particularse dan las dos condiciones simultaneamente entonces para saber de que lado moverse hay que hacer algunos calculos mas.
                            //por el momento solo se esta verificando que la posicion actual este dentro de un bounding para moverlo en ese plano.
                            t += "Coso conjunto!\n" +
                                "PMin X: " + personaje.BoundingBox.PMin.X + " - " + collider.BoundingBox.PMin.X + "\n" +
                                "PMax X: " + personaje.BoundingBox.PMax.X + " - " + collider.BoundingBox.PMax.X + "\n" +
                                "PMin Z: " + personaje.BoundingBox.PMin.Z + " - " + collider.BoundingBox.PMin.Z + "\n" +
                                "PMax Z: " + personaje.BoundingBox.PMax.Z + " - " + collider.BoundingBox.PMax.Z + "\n" +
                                "Last X: " + (lastPos.X - rs.X) + " - Z: " + (lastPos.Z - rs.Z) + "\n" +
                                "Actual X: " + (personaje.Position.X) + " - Z: " + (personaje.Position.Z) + "\n" +
                                "move X: " + (movementRay.X) + " - Z: " + (movementRay.Z);
                            if (personaje.Position.X > collider.BoundingBox.PMin.X && personaje.Position.X < collider.BoundingBox.PMax.X)
                            {
                                //El personaje esta contenido en el bounding X
                                t += "\n Sliding Z Dentro de X";
                                rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                            }
                            if (personaje.Position.Z > collider.BoundingBox.PMin.Z && personaje.Position.Z < collider.BoundingBox.PMax.Z)
                            {
                                //El personaje esta contenido en el bounding Z
                                t += "\n Sliding X Dentro de Z";
                                rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                            }

                            //Seria ideal sacar el punto mas proximo al bounding que colisiona y chequear con eso, en ves que con la posicion.
                        }
                        else
                        {
                            if ((personaje.BoundingBox.PMax.X > collider.BoundingBox.PMax.X && movementRay.X > 0) ||
                                (personaje.BoundingBox.PMin.X < collider.BoundingBox.PMin.X && movementRay.X < 0))
                            {
                                t += "Sliding X";
                                rs = new TGCVector3(0, movementRay.Y, movementRay.Z);
                            }
                            if ((personaje.BoundingBox.PMax.Z > collider.BoundingBox.PMax.Z && movementRay.Z > 0) ||
                                (personaje.BoundingBox.PMin.Z < collider.BoundingBox.PMin.Z && movementRay.Z < 0))
                            {
                                t += "Sliding Z";
                                rs = new TGCVector3(movementRay.X, movementRay.Y, 0);
                            }
                        }
                        text = t;
                        personaje.Position = lastPos - rs;
                        //Este ejemplo solo se mueve en X y Z con lo cual realizar el test en el plano Y no tiene sentido.
                    }
                }
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            personaje.Transform = TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z) * TGCMatrix.Translation(personaje.Position);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = showBoundingBoxModifier.Value;

            DrawText.drawText(text, 5, 20, System.Drawing.Color.Red);

            //Render piso
            piso.Render();

            //Render obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.Render();
                if (showBB)
                {
                    obstaculo.BoundingBox.Render();
                }
            }

            //Render personaje
            personaje.animateAndRender(ElapsedTime);
            if (showBB)
            {
                personaje.BoundingBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            piso.Dispose();
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.Dispose();
            }
            personaje.Dispose();
        }
    }
}