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

namespace Examples.Collision
{
    /// <summary>
    /// Ejemplo EjemploColisionesThirdPerson 
    /// Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Ciclo acoplado vs Ciclo desacoplado
    ///     # Unidad 5 - Animaciones - Skeletal Animation
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox
    /// 
    /// Se recomienda leer primero EjemploColisiones que posee el mismo espiritu de ejemplo pero mas sencillo.
    /// 
    /// Muestra como utilizar la cámara en Tercera Persona junto con detección de colisiones
    /// de BoundingBox y manejo de Input de Teclado.
    /// Se utiliza una forma diferente de movimiento con el método moveOrientedY()
    /// El modelo animado utiliza la herramienta TgcSkeletalLoader.
    /// Se muestra como cambiarle la textura al modelo animado.
    /// Los obstáculos son cargados utilizando TgcBox para crear cajas de tamaño dinámico, en lugar de utilizar
    /// modelos estáticos como en el otro ejemplo.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo 
    /// 
    /// </summary>
    public class EjemploColisionesThirdPerson : TgcExample
    {
        TgcBox piso;
        List<TgcBox> obstaculos;
        TgcSkeletalMesh personaje;


        public override string getCategory()
        {
            return "Collision";
        }

        public override string getName()
        {
            return "Detección + 3ra Persona";
        }

        public override string getDescription()
        {
            return "Ejemplo de Detección de Colisiones utilizando la cámara en Tercera Persona";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\tierra.jpg");
            piso = TgcBox.fromSize(new Vector3(0,-60,0), new Vector3(1000, 5, 1000), pisoTexture);


            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;
            

            //Obstaculo 1
            obstaculo = TgcBox.fromSize(
                new Vector3(-100, 0, 0),
                new Vector3(80, 150, 80),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromSize(
                new Vector3(50, 0, 200),
                new Vector3(80, 300, 80),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromSize(
                new Vector3(300, 0, 100),
                new Vector3(80, 100, 150),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);


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
            personaje.Position = new Vector3(0,-45,0);
            personaje.Scale = new Vector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));
            
            
            

            //Configurar camara en Tercer Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(personaje.Position, 200, -300);


            //Modifier para ver BoundingBox
            GuiController.Instance.Modifiers.addBoolean("showBoundingBox", "Bouding Box", false);

            //Modifiers para desplazamiento del personaje
            GuiController.Instance.Modifiers.addFloat("VelocidadCaminar", 1f, 400f, 250f);
            GuiController.Instance.Modifiers.addFloat("VelocidadRotacion", 1f, 360f, 120f);

        }


        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            
            //Obtener boolean para saber si hay que mostrar Bounding Box
            bool showBB = (bool)GuiController.Instance.Modifiers.getValue("showBoundingBox");

            
            //obtener velocidades de Modifiers
            float velocidadCaminar = (float)GuiController.Instance.Modifiers.getValue("VelocidadCaminar");
            float velocidadRotacion = (float)GuiController.Instance.Modifiers.getValue("VelocidadRotacion");


            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;
            
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
                float rotAngle = Geometry.DegreeToRadian(rotate * elapsedTime);
                personaje.rotateY(rotAngle);
                GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Caminando", true);

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = personaje.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                personaje.moveOrientedY(moveForward * elapsedTime); 
                
                //Detectar colisiones
                bool collide = false;
                foreach (TgcBox obstaculo in obstaculos)
                {
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(personaje.BoundingBox, obstaculo.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
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
                else
                {
                    
                }
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = personaje.Position;



            //Render piso
            piso.render();


            //Render obstaculos
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.render();
                if (showBB)
                {
                    obstaculo.BoundingBox.render();
                }
                
            }
            
            //Render personaje
            personaje.animateAndRender();
            if (showBB)
            {
                personaje.BoundingBox.render();
            }
            

        }

        public override void close()
        {
            piso.dispose();
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.dispose();
            }
            personaje.dispose();
        }

    }
}
