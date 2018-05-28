using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Collision.SphereCollision;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo SphereCollision
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminación - SkyBox
    ///     # Unidad 6 - Detección de Colisiones - Estrategia Integral
    ///     Ejemplo de nivel avanzado.
    ///     Se recomienda leer primero EjemploColisionesThirdPerson que posee el mismo espiritu de ejemplo pero mas sencillo.
    ///     Muestra una posible implementación de la Estrategia Integral de colisiones de una esfera con gravedad y sliding,
    ///     explicada en el apunte de Detección de Colisiones.
    ///     Utiliza la clase SphereCollisionManager para encapsular la estrategia de colisión. Esta clase se basa
    ///     en el paper: http://www.peroxide.dk/papers/collision/collision.pdf.
    ///     El paper no ha sido implementado en su totalidad y aún existen muchos puntos por mejorar y algunos bugs.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    class EjemploMoveSphereTriangleThirdPerson : TGCExampleViewer
    {
        private TGCBooleanModifier collisionsModifier;
        private TGCBooleanModifier showBoundingBoxModifier;
        private TGCFloatModifier velocidadCaminarModifier;
        private TGCFloatModifier velocidadRotacionModifier;
        private TGCBooleanModifier habilitarGravedadModifier;
        private TGCVertex3fModifier gravedadModifier;
        private TGCFloatModifier slideFactorModifier;
        private TGCFloatModifier pendienteModifier;
        private TGCFloatModifier velocidadSaltoModifier;
        private TGCFloatModifier tiempoSaltoModifier;

        private readonly List<Collider> objetosColisionables = new List<Collider>();
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingSphere characterSphere;
        private SphereTriangleCollisionManager collisionManager;
        private TgcArrow collisionNormalArrow;
        private TGCBox collisionPoint;
        private TgcArrow directionArrow;
        private TgcScene escenario;
        private bool jumping;
        private float jumpingElapsedTime;
        private TgcSkeletalMesh personaje;
        private TgcSkyBox skyBox;

        public EjemploMoveSphereTriangleThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Movimientos Esfera-Triangulos 3ra Persona";
            Description = "Estrategia integral de colisión: BoundingSphere contra triángulos + Gravedad + Sliding + Jump. Movimiento con W, A, S, D, Space. No ha sido implementado en su totalidad y aún existen muchos puntos por mejorar y algunos bugs.";
        }

        public override void Init()
        {
            //Cargar escenario específico para este ejemplo
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "\\MeshCreator\\Scenes\\Mountains\\Mountains-TgcScene.xml");

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

            //Se utiliza autotransform, aunque este es un claro ejemplo de que no se debe usar autotransform,
            //hay muchas operaciones y la mayoria las maneja el manager de colisiones, con lo cual se esta
            //perdiendo el control de las transformaciones del personaje.
            personaje.AutoTransform = true;
            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Position = new TGCVector3(0, 2500, -150);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.RotateY(Geometry.DegreeToRadian(180f));
            //escalamos el personaje porque es muy grande.
            personaje.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);

            //BoundingSphere que va a usar el personaje
            personaje.AutoUpdateBoundingBox = false;
            characterSphere = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateBoxRadius());
            jumping = false;

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                //Los objetos del layer "TriangleCollision" son colisiones a nivel de triangulo
                if (mesh.Layer == "TriangleCollision")
                {
                    objetosColisionables.Add(TriangleMeshCollider.fromMesh(mesh));
                }
                //El resto de los objetos son colisiones de BoundingBox
                else
                {
                    objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
            }

            //Crear linea para mostrar la direccion del movimiento del personaje
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.4f;
            directionArrow.HeadSize = new TGCVector2(5, 10);

            //Linea para normal de colision
            collisionNormalArrow = new TgcArrow();
            collisionNormalArrow.BodyColor = Color.Blue;
            collisionNormalArrow.HeadColor = Color.Yellow;
            collisionNormalArrow.Thickness = 0.4f;
            collisionNormalArrow.HeadSize = new TGCVector2(2, 5);

            //Caja para marcar punto de colision
            collisionPoint = TGCBox.fromSize(new TGCVector3(4, 4, 4), Color.Red);
            collisionPoint.AutoTransform = true;

            //Crear manejador de colisiones
            collisionManager = new SphereTriangleCollisionManager();
            collisionManager.GravityEnabled = true;

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, new TGCVector3(0, 45, 0), 35, -150);
            Camara = camaraInterna;

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox3\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "Up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "Down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "Left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "Right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "Back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "Front.jpg");
            skyBox.Init();

            //Modifier para ver BoundingBox
            collisionsModifier = AddBoolean("Collisions", "Collisions", true);
            showBoundingBoxModifier = AddBoolean("showBoundingBox", "Bouding Box", true);

            //Modifiers para desplazamiento del personaje
            velocidadCaminarModifier = AddFloat("VelocidadCaminar", 0, 10, 2);
            velocidadRotacionModifier = AddFloat("VelocidadRotacion", 1f, 360f, 150f);
            habilitarGravedadModifier = AddBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            gravedadModifier = AddVertex3f("Gravedad", new TGCVector3(-5, -10, -5), new TGCVector3(5, 5, 5), new TGCVector3(0, -6, 0));
            slideFactorModifier = AddFloat("SlideFactor", 0f, 2f, 1f);
            pendienteModifier = AddFloat("Pendiente", 0f, 1f, 0.7f);
            velocidadSaltoModifier = AddFloat("VelocidadSalto", 0f, 10f, 2f);
            tiempoSaltoModifier = AddFloat("TiempoSalto", 0f, 2f, 0.5f);

            UserVars.addVar("Movement");
            UserVars.addVar("ySign");
        }

        public override void Update()
        {
            PreUpdate();
            //obtener velocidades de Modifiers
            var velocidadCaminar = velocidadCaminarModifier.Value;
            var velocidadRotacion = velocidadRotacionModifier.Value;
            var velocidadSalto = velocidadSaltoModifier.Value;
            var tiempoSalto = tiempoSaltoModifier.Value;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;

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

            //Jump
            if (!jumping && Input.keyPressed(Key.Space))
            {
                if (collisionManager.Collision)
                {
                    jumping = true;
                    jumpingElapsedTime = 0f;
                    jump = 0;
                }
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Actualizar salto
            if (jumping)
            {
                jumpingElapsedTime += ElapsedTime;
                if (jumpingElapsedTime > tiempoSalto)
                {
                    jumping = false;
                }
                else
                {
                    jump = velocidadSalto;
                }
            }

            //Vector de movimiento
            var movementVector = TGCVector3.Empty;
            if (moving || jumping)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                //jump *= elapsedTime;
                movementVector = new TGCVector3(FastMath.Sin(personaje.Rotation.Y) * moveForward, jump, FastMath.Cos(personaje.Rotation.Y) * moveForward);
            }

            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = habilitarGravedadModifier.Value;
            collisionManager.GravityForce = gravedadModifier.Value /** elapsedTime*/;
            collisionManager.SlideFactor = slideFactorModifier.Value;
            collisionManager.OnGroundMinDotValue = pendienteModifier.Value;

            //Si esta saltando, desactivar gravedad
            if (jumping)
            {
                collisionManager.GravityEnabled = false;
            }

            //Mover personaje con detección de colisiones, sliding y gravedad
            if (collisionsModifier.Value)
            {
                var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
                personaje.Move(realMovement);

                //Cargar desplazamiento realizar en UserVar
                UserVars.setValue("Movement", TGCVector3.PrintVector3(realMovement));
                UserVars.setValue("ySign", realMovement.Y);
            }
            else
            {
                personaje.Move(movementVector);
            }

            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Collision)
            {
                jumping = false;
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterSphere.Center;
            directionArrow.PEnd = characterSphere.Center + TGCVector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Actualizar valores de normal de colision
            if (collisionManager.Collision)
            {
                collisionNormalArrow.PStart = collisionManager.LastCollisionPoint;
                collisionNormalArrow.PEnd = collisionManager.LastCollisionPoint + TGCVector3.Multiply(collisionManager.LastCollisionNormal, 80);

                collisionNormalArrow.updateValues();

                collisionPoint.Position = collisionManager.LastCollisionPoint;
                collisionPoint.Render();
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = showBoundingBoxModifier.Value;

            if (collisionManager.Collision)
            {
                collisionNormalArrow.Render();
                collisionPoint.Render();
            }

            //Render de mallas
            foreach (var mesh in escenario.Meshes)
            {
                mesh.Render();
            }

            //Render personaje
            personaje.animateAndRender(ElapsedTime);
            if (showBB)
            {
                characterSphere.Render();
            }

            //Render linea
            directionArrow.Render();

            //Render SkyBox
            skyBox.Render();

            PostRender();
        }

        public override void Dispose()
        {
            escenario.DisposeAll();
            personaje.Dispose();
            skyBox.Dispose();
            collisionNormalArrow.Dispose();
            directionArrow.Dispose();
        }
    }
}