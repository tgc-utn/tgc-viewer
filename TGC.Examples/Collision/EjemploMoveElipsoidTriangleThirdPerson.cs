using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Collision.ElipsoidCollision;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploElipsoidCollision
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - SkyBox
    ///     # Unidad 6 - Detecci�n de Colisiones - Estrategia Integral
    ///     Ejemplo de nivel avanzado.
    ///     Se recomienda leer primero SphereCollision
    ///     Muestra una posible implementaci�n de la Estrategia Integral de colisiones de un Elipsoide con gravedad y sliding,
    ///     explicada en el apunte de Detecci�n de Colisiones.
    ///     Es una mejora del algoritmo utilizado en SphereCollision, pero utilizando un Elipsoide y m�todos de colisi�n
    ///     mas robustos.
    ///     Se utilizan distintos volumenes de colision (llamados Colliders): basados en triangulos y basados en BoundingBox.
    ///     Tener Colliders del tipo Triangulos permite armar terrenos con desniveles irregulares.
    ///     En cambio SphereCollision solo soporta BoundingBox.
    ///     Aun esta en estado BETA.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMoveElipsoidTriangleThirdPerson : TGCExampleViewer
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
        private TgcBoundingElipsoid characterElipsoid;
        private ElipsoidCollisionManager collisionManager;
        private TgcArrow collisionNormalArrow;
        private TGCBox collisionPoint;
        private TgcArrow directionArrow;
        private TgcScene escenario;
        private bool jumping;
        private float jumpingElapsedTime;
        private TgcSkeletalMesh personaje;
        private TgcSkyBox skyBox;

        public EjemploMoveElipsoidTriangleThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Movimientos Elipsoid-Triangulos 3ra Persona";
            Description = "Colisi�n de un Elipsoide contra todo un escenario, aplicando gravedad y sliding. Movimiento con W, A, S, D, Space.";
        }

        public override void Init()
        {
            //Cargar escenario espec�fico para este ejemplo. Este escenario tiene dos layers: objetos normales y objetos con colisi�n a nivel de tri�ngulo.
            //La colisi�n a nivel de tri�ngulos es costosa. Solo debe utilizarse para objetos puntuales (como el piso). Y es recomendable dividirlo en varios
            //meshes (y no hacer un �nico piso que ocupe todo el escenario)
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "\\MeshCreator\\Scenes\\Mountains\\Mountains-TgcScene.xml");

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });

            //Se utiliza autotransform, aunque este es un claro ejemplo de que no se debe usar autotransform,
            //hay muchas operaciones y la mayoria las maneja el manager de colisiones, con lo cual se esta
            //perdiendo el control de las transformaciones del personaje.
            personaje.AutoTransform = true;
            //Configurar animacion inicial
            personaje.playAnimation("StandBy", true);
            //Escalarlo porque es muy grande
            personaje.Position = new TGCVector3(0, 1000, -450);
            //Rotarlo 180� porque esta mirando para el otro lado
            personaje.RotateY(Geometry.DegreeToRadian(180f));
            //escalamos un poco el personaje.
            personaje.Scale = new TGCVector3(0.75f, 0.75f, 0.75f);
            //BoundingSphere que va a usar el personaje
            personaje.AutoUpdateBoundingBox = false;
            characterElipsoid = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter() + TGCVector3.Empty, new TGCVector3(12, 28, 12));
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
                //El resto de los objetos son colisiones de BoundingBox. Las colisiones a nivel de triangulo son muy costosas asi que deben utilizarse solo
                //donde es extremadamente necesario (por ejemplo en el piso). El resto se simplifica con un BoundingBox
                else
                {
                    objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
            }

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;

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

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, new TGCVector3(0, 45, 0), 20, -120);
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
            velocidadCaminarModifier = AddFloat("VelocidadCaminar", 0, 20, 2);
            velocidadRotacionModifier = AddFloat("VelocidadRotacion", 1f, 360f, 150f);
            habilitarGravedadModifier = AddBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            gravedadModifier = AddVertex3f("Gravedad", new TGCVector3(-5, -5, -5), new TGCVector3(5, 5, 5), new TGCVector3(0, -4, 0));
            slideFactorModifier = AddFloat("SlideFactor", 0f, 2f, 1f);
            pendienteModifier = AddFloat("Pendiente", 0f, 1f, 0.72f);
            velocidadSaltoModifier = AddFloat("VelocidadSalto", 0f, 50f, 10f);
            tiempoSaltoModifier = AddFloat("TiempoSalto", 0f, 2f, 0.5f);

            UserVars.addVar("Movement");
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
                //Se puede saltar solo si hubo colision antes
                if (collisionManager.Result.collisionFound)
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

            //Saltando
            if (jumping)
            {
                //Activar animacion de saltando
                personaje.playAnimation("Jump", true);
            }
            //Si hubo desplazamiento
            else if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }
            //Si no se esta moviendo ni saltando, activar animacion de Parado
            else
            {
                personaje.playAnimation("StandBy", true);
            }

            //Actualizar salto
            if (jumping)
            {
                //El salto dura un tiempo hasta llegar a su fin
                jumpingElapsedTime += ElapsedTime;
                if (jumpingElapsedTime > tiempoSalto)
                {
                    jumping = false;
                }
                else
                {
                    jump = velocidadSalto * (tiempoSalto - jumpingElapsedTime);
                }
            }

            //Vector de movimiento
            var movementVector = TGCVector3.Empty;
            if (moving || jumping)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
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

            //Mover personaje con detecci�n de colisiones, sliding y gravedad
            if (collisionsModifier.Value)
            {
                //Aca se aplica toda la l�gica de detecci�n de colisiones del CollisionManager. Intenta mover el Elipsoide
                //del personaje a la posici�n deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
                var realMovement = collisionManager.moveCharacter(characterElipsoid, movementVector, objetosColisionables);
                personaje.Move(realMovement);

                //Cargar desplazamiento realizar en UserVar
                UserVars.setValue("Movement", TGCVector3.PrintVector3(realMovement));
            }
            else
            {
                personaje.Move(movementVector);
            }

            /*
            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Result.collisionNormal.Y < 0)
            {
                jumping = false;
            }
            */

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterElipsoid.Center;
            directionArrow.PEnd = characterElipsoid.Center + TGCVector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Actualizar valores de normal de colision
            if (collisionManager.Result.collisionFound)
            {
                collisionNormalArrow.PStart = collisionManager.Result.collisionPoint;
                collisionNormalArrow.PEnd = collisionManager.Result.collisionPoint + TGCVector3.Multiply(collisionManager.Result.collisionNormal, 80);

                collisionNormalArrow.updateValues();

                collisionPoint.Position = collisionManager.Result.collisionPoint;
                collisionPoint.updateValues();
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = showBoundingBoxModifier.Value;

            if (collisionManager.Result.collisionFound)
            {
                collisionNormalArrow.Render();
                collisionPoint.Transform = TGCMatrix.RotationYawPitchRoll(collisionPoint.Rotation.Y, collisionPoint.Rotation.X, collisionPoint.Rotation.Z) *
                            TGCMatrix.Translation(collisionPoint.Position);
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
                characterElipsoid.Render();
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
            characterElipsoid.Dispose();
        }
    }
}