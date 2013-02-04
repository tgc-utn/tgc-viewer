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
using TgcViewer.Utils.Sound;

namespace Examples.Sound
{
    /// <summary>
    /// Ejemplo PlaySound3D:
    /// Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh, GameEngine
    ///     # Unidad 6- Detecci�n de Colisiones - BoundingBox
    /// 
    /// Muestra como reproducir un archivo de sonido 3D en formato WAV.
    /// El volumen del sonido var�a seg�n su posici�n en el espacio.
    /// Crea un modelo de un auto que se desplaza por un escenario con 3
    /// objetos que tienen un sonido 3D asociado.
    /// El volumen del sonido var�a seg�n la posici�n del auto.
    /// El sonido 3D solo funciona con archivos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
    /// y tiene que decir "1 Channel".
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class PlaySound3D : TgcExample
    {
        const float VELODICAD_CAMINAR = 250f;
        const float VELOCIDAD_ROTACION = 120f;

        TgcBox piso;
        List<TgcBox> obstaculos;
        List<Tgc3dSound> sonidos;
        TgcMesh personaje;


        public override string getCategory()
        {
            return "Sound";
        }

        public override string getName()
        {
            return "Play Sound3D";
        }

        public override string getDescription()
        {
            return "Muestra como reproducir un archivo de sonido 3D en formato WAV.";
        }

        public override void init()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\tierra.jpg");
            piso = TgcBox.fromSize(new Vector3(0, -60, 0), new Vector3(5000, 5, 5000), pisoTexture);


            //Cargar obstaculos y posicionarlos. Los obst�culos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            sonidos = new List<Tgc3dSound>();
            TgcBox obstaculo;
            Tgc3dSound sound;
            

            //Obstaculo 1
            obstaculo = TgcBox.fromSize(
                new Vector3(-200, 0, 0),
                new Vector3(80, 150, 80),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack3\\goo2.jpg"));
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 1
            //OJO, solo funcionan sonidos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
            //y tiene que decir "1 Channel".
            sound = new Tgc3dSound(GuiController.Instance.ExamplesMediaDir + "Sound\\armon�a, continuo.wav", obstaculo.Position);
            //Hay que configurar la m�nima distancia a partir de la cual se empieza a atenuar el sonido 3D
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            //Obstaculo 2
            obstaculo = TgcBox.fromSize(
                new Vector3(200, 0, 800),
                new Vector3(80, 300, 80),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack3\\lun_dirt.jpg"));
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 2
            sound = new Tgc3dSound(GuiController.Instance.ExamplesMediaDir + "Sound\\viento helado.wav", obstaculo.Position);
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            //Obstaculo 3
            obstaculo = TgcBox.fromSize(
                new Vector3(600, 0, 400),
                new Vector3(80, 100, 150),
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack3\\Metal2_1.jpg"));
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 3
            sound = new Tgc3dSound(GuiController.Instance.ExamplesMediaDir + "Sound\\risa de man�aco.wav", obstaculo.Position);
            sound.MinDistance = 50f;
            sonidos.Add(sound);


            //Cargar personaje principal
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");
            personaje = scene.Meshes[0];
            personaje.Position = new Vector3(0,-50,0);


            //Hacer que el Listener del sonido 3D siga al personaje
            GuiController.Instance.DirectSound.ListenerTracking = personaje;

            //Configurar camara en Tercer Persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(personaje.Position, 200, 300);
            GuiController.Instance.ThirdPersonCamera.TargetDisplacement = new Vector3(0, 100, 0);

            //Ejecutar en loop los sonidos
            foreach (Tgc3dSound s in sonidos)
            {
                s.play(true);
            }
        }


        public override void render(float elapsedTime)
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;
            
            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                moveForward = -VELODICAD_CAMINAR;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = VELODICAD_CAMINAR;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = VELOCIDAD_ROTACION;
                rotating = true;
            }
        
            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -VELOCIDAD_ROTACION;
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
                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = personaje.Position;
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
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = personaje.Position;



            //Render piso
            piso.render();


            //Render obstaculos
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.render();
            }
            
            //Render personaje
            personaje.render();
           

        }

        public override void close()
        {
            piso.dispose();
            foreach (TgcBox obstaculo in obstaculos)
            {
                obstaculo.dispose();
            }
            personaje.dispose();

            foreach (Tgc3dSound sound in sonidos)
            {
                sound.dispose();
            }
        }

    }
}
