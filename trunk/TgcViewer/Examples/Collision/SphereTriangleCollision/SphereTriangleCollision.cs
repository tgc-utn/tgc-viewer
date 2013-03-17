using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Terrain;

namespace Examples.Collision.SphereTriangleCollision
{
    /// <summary>
    /// Ejemplo SphereCollision 
    /// Unidades Involucradas:
    ///     # Unidad 4 - Texturas e Iluminaci�n - SkyBox
    ///     # Unidad 6 - Detecci�n de Colisiones - Estrategia Integral
    /// 
    /// Ejemplo de nivel avanzado.
    /// Se recomienda leer primero EjemploColisionesThirdPerson que posee el mismo espiritu de ejemplo pero mas sencillo.
    /// 
    /// Muestra una posible implementaci�n de la Estrategia Integral de colisiones de una esfera con gravedad y sliding,
    /// explicada en el apunte de Detecci�n de Colisiones.
    /// Utiliza la clase SphereCollisionManager para encapsular la estrategia de colisi�n. Esta clase se basa
    /// en el paper: http://www.peroxide.dk/papers/collision/collision.pdf.
    /// El paper no ha sido implementado en su totalidad y a�n existen muchos puntos por mejorar.
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo 
    /// 
    /// </summary>
    public class SphereTriangleCollision : TgcExample
    {
        TgcSkeletalMesh personaje;
        List<Collider> objetosColisionables = new List<Collider>();
        TgcBoundingSphere characterSphere;
        TgcArrow directionArrow;
        TgcScene escenario;
        SphereTriangleCollisionManager collisionManager;
        TgcSkyBox skyBox;
        bool jumping;
        float jumpingElapsedTime;
        TgcArrow collisionNormalArrow;
        TgcBox collisionPoint;
        


        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Colision Esfera-Triangulos";
        }

        public override string getDescription()
        {
            return "Estrategia integral de colisi�n: BoundingSphere contra tri�ngulos + Gravedad + Sliding + Jump. Movimiento con W, A, S, D, Space.";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;


            //Cargar escenario espec�fico para este ejemplo
            TgcSceneLoader loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "\\MeshCreator\\Scenes\\Mountains\\Mountains-TgcScene.xml");


            //Cargar personaje con animaciones
            TgcSkeletalLoader skeletalLoader = new TgcSkeletalLoader();
            personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Robot-TgcSkeletalMesh.xml",
                GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\",
                new string[] { 
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Caminando-TgcSkeletalAnim.xml",
                    GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\" + "Parado-TgcSkeletalAnim.xml",
                });

            //Le cambiamos la textura para diferenciarlo un poco
            personaje.changeDiffuseMaps(new TgcTexture[] { TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Textures\\" + "uvwGreen.jpg") });
            
            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(0,2500,-150);
            //Rotarlo 180� porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));


            //BoundingSphere que va a usar el personaje
            personaje.AutoUpdateBoundingBox = false;
            characterSphere = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateBoxRadius());
            jumping = false;

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (TgcMesh mesh in escenario.Meshes)
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
            directionArrow.Thickness = 1;
            directionArrow.HeadSize = new Vector2(10, 20);

            //Linea para normal de colision
            collisionNormalArrow = new TgcArrow();
            collisionNormalArrow.BodyColor = Color.Blue;
            collisionNormalArrow.HeadColor = Color.Yellow;
            collisionNormalArrow.Thickness = 1;
            collisionNormalArrow.HeadSize = new Vector2(5, 10);

            //Caja para marcar punto de colision
            collisionPoint = TgcBox.fromSize(new Vector3(20, 20, 20), Color.Red);

            //Crear manejador de colisiones
            collisionManager = new SphereTriangleCollisionManager();
            collisionManager.GravityEnabled = true;


            //Configurar camara en Tercer Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(personaje.Position, 100, -400);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 100, 0);

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox3\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "Up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "Down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "Left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "Right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "Back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "Front.jpg");
            skyBox.updateValues();


            //Modifier para ver BoundingBox
            GuiController.Instance.Modifiers.addBoolean("Collisions", "Collisions", true);
            GuiController.Instance.Modifiers.addBoolean("showBoundingBox", "Bouding Box", true);

            //Modifiers para desplazamiento del personaje
            GuiController.Instance.Modifiers.addFloat("VelocidadCaminar", 0, 50, 10);
            GuiController.Instance.Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 150f);
            GuiController.Instance.Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            GuiController.Instance.Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, -24, 0));
            GuiController.Instance.Modifiers.addFloat("SlideFactor", 0f, 2f, 1f);
            GuiController.Instance.Modifiers.addFloat("Pendiente", 0f, 1f, 0.7f);
            GuiController.Instance.Modifiers.addFloat("VelocidadSalto", 0f, 100f, 20f);
            GuiController.Instance.Modifiers.addFloat("TiempoSalto", 0f, 2f, 0.5f);


            GuiController.Instance.UserVars.addVar("Movement");
            GuiController.Instance.UserVars.addVar("ySign");
        }


        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            
            //Obtener boolean para saber si hay que mostrar Bounding Box
            bool showBB = (bool)GuiController.Instance.Modifiers.getValue("showBoundingBox");

            
            //obtener velocidades de Modifiers
            float velocidadCaminar = (float)GuiController.Instance.Modifiers.getValue("VelocidadCaminar");
            float velocidadRotacion = (float)GuiController.Instance.Modifiers.getValue("VelocidadRotacion");
            float velocidadSalto = (float)GuiController.Instance.Modifiers.getValue("VelocidadSalto");
            float tiempoSalto = (float)GuiController.Instance.Modifiers.getValue("TiempoSalto");


            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;
            float jump = 0;
            
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

            //Jump
            if (!jumping && d3dInput.keyPressed(Key.Space))
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
                float rotAngle = Geometry.DegreeToRadian(rotate * elapsedTime);
                personaje.rotateY(rotAngle);
                GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
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
                jumpingElapsedTime += elapsedTime;
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
            Vector3 movementVector = Vector3.Empty;
            if (moving || jumping)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                //jump *= elapsedTime;
                movementVector = new Vector3(
                    FastMath.Sin(personaje.Rotation.Y) * moveForward,
                    jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward
                    );
            }


            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = (bool)GuiController.Instance.Modifiers["HabilitarGravedad"];
            collisionManager.GravityForce = (Vector3)GuiController.Instance.Modifiers["Gravedad"] /** elapsedTime*/;
            collisionManager.SlideFactor = (float)GuiController.Instance.Modifiers["SlideFactor"];
            collisionManager.OnGroundMinDotValue = (float)GuiController.Instance.Modifiers["Pendiente"];

            //Si esta saltando, desactivar gravedad
            if (jumping)
            {
                collisionManager.GravityEnabled = false;
            }


            //Mover personaje con detecci�n de colisiones, sliding y gravedad
            if ((bool)GuiController.Instance.Modifiers["Collisions"])
            {
                Vector3 realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
                personaje.move(realMovement);

                //Cargar desplazamiento realizar en UserVar
                GuiController.Instance.UserVars.setValue("Movement", TgcParserUtils.printVector3(realMovement));
                GuiController.Instance.UserVars.setValue("ySign", realMovement.Y);
            }
            else
            {
                personaje.move(movementVector);
            }
            

            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Collision)
            {
                jumping = false;
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = personaje.Position;


            //Actualizar valores de la linea de movimiento
            directionArrow.PStart = characterSphere.Center;
            directionArrow.PEnd = characterSphere.Center + Vector3.Multiply(movementVector, 50);
            directionArrow.updateValues();

            //Actualizar valores de normal de colision
            if (collisionManager.Collision)
            {
                collisionNormalArrow.PStart = collisionManager.LastCollisionPoint;
                collisionNormalArrow.PEnd = collisionManager.LastCollisionPoint + Vector3.Multiply(collisionManager.LastCollisionNormal, 200); ;
                collisionNormalArrow.updateValues();
                collisionNormalArrow.render();

                collisionPoint.Position = collisionManager.LastCollisionPoint;
                collisionPoint.render();
            }

            




            //Render de mallas
            foreach (TgcMesh mesh in escenario.Meshes)
            {
                mesh.render();
            }



            //Render personaje
            personaje.animateAndRender();
            if (showBB)
            {
                characterSphere.render();
            }

            //Render linea
            directionArrow.render();

            //Render SkyBox
            skyBox.render();
        }


        
        


        public override void close()
        {
            escenario.disposeAll();
            personaje.dispose();
            skyBox.dispose();
            collisionNormalArrow.dispose();
            directionArrow.dispose();
        }

    }
}
