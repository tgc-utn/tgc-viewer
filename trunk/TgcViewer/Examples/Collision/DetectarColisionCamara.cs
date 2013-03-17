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
    /// Ejemplo EjemploPicking:
    /// Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Ray
    /// 
    /// Muestra como detectar los objetos que se interponen en la visión del usuario
    /// cuando se utiliza una cámara en 3ra persona.
    /// Acorta la distancia de la camara a la mínima colision encontrada con los objetos del escenario.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class DetectarColisionCamara : TgcExample
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
            return "Colision con Camara";
        }

        public override string getDescription()
        {
            return "Detecta los objetos que se interponen en la visión de la cámara en 3ra persona y renderiza solo su BoundingBox. Movimiento con W, A, S, D.";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\tierra.jpg");
            piso = TgcBox.fromExtremes(new Vector3(-1000, -2, -1000), new Vector3(1000, 0, 1000), pisoTexture);


            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;


            float wallSize = 1000;
            float wallHeight = 500;

            //Obstaculo 1
            obstaculo = TgcBox.fromExtremes(
                new Vector3(0,0,0),
                new Vector3(wallSize, wallHeight, 10),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromExtremes(
                new Vector3(0, 0, 0),
                new Vector3(10, wallHeight, wallSize),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromExtremes(
                new Vector3(0, 0, wallSize),
                new Vector3(wallSize, wallHeight, wallSize + 10),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 4
            obstaculo = TgcBox.fromExtremes(
                new Vector3(wallSize, 0, 0),
                new Vector3(wallSize + 10, wallHeight, wallSize),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 5
            obstaculo = TgcBox.fromExtremes(
                new Vector3(wallSize / 2, 0, wallSize - 400),
                new Vector3(wallSize + 10, wallHeight, wallSize - 400 + 10),
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


            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(100, 0, 100);
            personaje.Scale = new Vector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));
            

            //Configurar camara en Tercera Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(personaje.Position, 200, -300);


            //Modifiers para modificar propiedades de la camara
            GuiController.Instance.Modifiers.addFloat("offsetHeight", 0, 300, 100);
            GuiController.Instance.Modifiers.addFloat("offsetForward", -400, 0, -220);
            GuiController.Instance.Modifiers.addVertex2f("displacement", new Vector2(0, 0), new Vector2(100, 200), new Vector2(0, 100));
        }



        


        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            float velocidadCaminar = 400f;
            float velocidadRotacion = 120f;


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

                //Hacer que la camara siga al personaje en su nueva posicion
                GuiController.Instance.ThirdPersonCamera.Target = personaje.Position;
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }


            //Ajustar la posicion de la camara segun la colision con los objetos del escenario
            ajustarPosicionDeCamara(personaje, obstaculos);




            //Render piso
            piso.render();

            //Render de obstaculos
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.render();
            }

            //Render personaje
            personaje.animateAndRender();

        }

        /// <summary>
        /// Ajustar la posicion de la camara segun la colision con los objetos del escenario.
        /// Acerca la distancia entre el persona y la camara si hay colisiones de objetos
        /// en el medio
        /// </summary>
        private void ajustarPosicionDeCamara(TgcSkeletalMesh personaje, List<TgcBox> obstaculos)
        {
            //Actualizar valores de camara segun modifiers
            TgcThirdPersonCamera camera = GuiController.Instance.ThirdPersonCamera;
            camera.OffsetHeight = (float)GuiController.Instance.Modifiers["offsetHeight"];
            camera.OffsetForward = (float)GuiController.Instance.Modifiers["offsetForward"];
            Vector2 displacement = (Vector2)GuiController.Instance.Modifiers["displacement"];
            camera.TargetDisplacement = new Vector3(displacement.X, displacement.Y, 0);

            //Pedirle a la camara cual va a ser su proxima posicion
            Vector3 segmentA;
            Vector3 segmentB;
            camera.generateViewMatrix(out segmentA, out segmentB);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            Vector3 q;
            float minDistSq = FastMath.Pow2(camera.OffsetForward);
            foreach (TgcBox obstaculo in obstaculos)
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(segmentB, segmentA, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    float distSq = (Vector3.Subtract(q, segmentB)).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;

                        //Le restamos un poco para que no se acerque tanto
                        minDistSq /= 2;
                    }
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            float newOffsetForward = -FastMath.Sqrt(minDistSq);
            /*
            if(newOffsetForward < 10)
            {
                newOffsetForward = 10;
            }*/
            camera.OffsetForward = newOffsetForward;

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
