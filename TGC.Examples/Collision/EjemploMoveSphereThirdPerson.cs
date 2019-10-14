using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
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
    public class EjemploMoveSphereThirdPerson : TGCExampleViewer
    {
        private TGCBooleanModifier showBoundingBoxModifier;
        private TGCFloatModifier velocidadCaminarModifier;
        private TGCFloatModifier velocidadRotacionModifier;
        private TGCBooleanModifier habilitarGravedadModifier;
        private TGCVertex3fModifier gravedadModifier;
        private TGCFloatModifier slideFactorModifier;

        private readonly List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private readonly List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private readonly List<TgcBoundingAxisAlignBox> objetosColisionables = new List<TgcBoundingAxisAlignBox>();
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingSphere characterSphere;
        private SphereCollisionManager collisionManager;
        private TgcArrow directionArrow;
        private TgcScene escenario;
        private TgcSkeletalMesh personaje;
        private TgcSkyBox skyBox;

        private float jumping;

        public EjemploMoveSphereThirdPerson(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Movimientos Esfera 3ra Persona";
            Description =
                "Estrategia integral de colisión: BoundingSphere + Gravedad + Sliding + Jump. Movimiento con W, A, S, D, Space. No ha sido implementado en su totalidad y aún existen muchos puntos por mejorar y algunos bugs.";
        }

        public override void Init()
        {
            //Cargar escenario específico para este ejemplo
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "PatioDeJuegos\\PatioDeJuegos-TgcScene.xml");

            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
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
            personaje.Position = new TGCVector3(0, 500, -100);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.RotateY(Geometry.DegreeToRadian(180f));
            //Escalamos el personaje ya que sino la escalera es demaciado grande.
            personaje.Scale = new TGCVector3(1.5f, 1.5f, 1.5f);

            characterSphere = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateBoxRadius());

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                objetosColisionables.Add(mesh.BoundingBox);
            }

            //Crear linea para mostrar la direccion del movimiento del personaje
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 1;
            directionArrow.HeadSize = new TGCVector2(10, 20);

            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, new TGCVector3(0, 100, 0), 100, -400);
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
            showBoundingBoxModifier = AddBoolean("showBoundingBox", "Bouding Box", true);

            //Modifiers para desplazamiento del personaje
            velocidadCaminarModifier = AddFloat("VelocidadCaminar", 0, 20, 10);
            velocidadRotacionModifier = AddFloat("VelocidadRotacion", 1f, 360f, 150f);
            habilitarGravedadModifier = AddBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            gravedadModifier = AddVertex3f("Gravedad", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), new TGCVector3(0, -10, 0));
            slideFactorModifier = AddFloat("SlideFactor", 1f, 2f, 1.3f);

            UserVars.addVar("Movement");
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
            if (Input.keyUp(Key.Space) && jumping < 30)
            {
                jumping = 30;
            }
            if (Input.keyUp(Key.Space) || jumping > 0)
            {
                jumping -= 30 * ElapsedTime;
                jump = jumping;
                moving = true;
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
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Vector de movimiento
            var movementVector = TGCVector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new TGCVector3(FastMath.Sin(personaje.Rotation.Y) * moveForward, jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward);
            }

            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = habilitarGravedadModifier.Value;
            collisionManager.GravityForce = gravedadModifier.Value;
            collisionManager.SlideFactor = slideFactorModifier.Value;

            //Mover personaje con detección de colisiones, sliding y gravedad
            var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);

            personaje.Position += realMovement;

            personaje.Transform = TGCMatrix.Scaling(personaje.Scale) *
                                    TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z) *
                                      TGCMatrix.Translation(personaje.Position);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterSphere.Center;
            directionArrow.PEnd = characterSphere.Center + TGCVector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Cargar desplazamiento realizar en UserVar
            UserVars.setValue("Movement", TGCVector3.PrintTGCVector3(realMovement));

            //Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            objectsBehind.Clear();
            objectsInFront.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                TGCVector3 q;
                if (TgcCollisionUtils.intersectSegmentAABB(Camara.Position, camaraInterna.Target,
                    mesh.BoundingBox, out q))
                {
                    objectsBehind.Add(mesh);
                }
                else
                {
                    objectsInFront.Add(mesh);
                }
            }
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = showBoundingBoxModifier.Value;

            //Render mallas que no se interponen
            foreach (var mesh in objectsInFront)
            {
                mesh.Render();
                if (showBB)
                {
                    mesh.BoundingBox.Render();
                }
            }

            //Para las mallas que se interponen a la cámara, solo renderizar su BoundingBox
            foreach (var mesh in objectsBehind)
            {
                mesh.BoundingBox.Render();
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
        }
    }
}