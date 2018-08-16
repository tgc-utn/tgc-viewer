using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Sound
{
    /// <summary>
    ///     Ejemplo PlaySound3D:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh, GameEngine
    ///     # Unidad 6- Deteccion de Colisiones - BoundingBox
    ///     Muestra como reproducir un archivo de sonido 3D en formato WAV.
    ///     El volumen del sonido varia segun su posicion en el espacio.
    ///     Crea un modelo de un auto que se desplaza por un escenario con 3
    ///     objetos que tienen un sonido 3D asociado.
    ///     El volumen del sonido varia segun la posicion del auto.
    ///     El sonido 3D solo funciona con archivos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
    ///     y tiene que decir "1 Channel".
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class PlaySound3D : TGCExampleViewer
    {
        private const float VELODICAD_CAMINAR = 250f;
        private const float VELOCIDAD_ROTACION = 120f;
        private TgcThirdPersonCamera camaraInterna;
        private List<TGCBox> obstaculos;
        private TgcMesh personaje;

        private TGCBox piso;
        private TgcSoundListener receptor;
        private List<TgcSoundEmitter> emisores;

        public PlaySound3D(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Sound";
            Name = "Play Sound3D";
            Description = "Muestra como reproducir un archivo de sonido 3D en formato WAV.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = TGCBox.fromSize(new TGCVector3(5000, 5, 5000), pisoTexture);
            piso.AutoTransform = true;
            piso.Position = new TGCVector3(0, -60, 0);

            //Cargar obstaculos y posicionarlos. Los obstaculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TGCBox>();
            emisores = new List<TgcSoundEmitter>();

            TGCBox obstaculo;

            //Obstaculo 1
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 150, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\Quake\\TexturePack3\\goo2.jpg"));
            obstaculo.AutoTransform = true;
            obstaculo.Position = new TGCVector3(-250, 0, 0);
            obstaculos.Add(obstaculo);


            //Obstaculo 2
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 300, 80),
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    MediaDir + "Texturas\\Quake\\TexturePack3\\lun_dirt.jpg"));
            obstaculo.AutoTransform = true;
            obstaculo.Position = new TGCVector3(250, 0, 800);
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 2

            //Obstaculo 3
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 100, 150),
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    MediaDir + "Texturas\\Quake\\TexturePack3\\Metal2_1.jpg"));
            obstaculo.AutoTransform = true;
            obstaculo.Position = new TGCVector3(500, 0, -400);
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 3

            //Cargar personaje principal
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");
            personaje = scene.Meshes[0];
            personaje.AutoTransform = true;
            personaje.Position = new TGCVector3(0, -50, 0);

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 250, 500);
            Camara = camaraInterna;

            TgcSoundManager.Initialize();

            //Sondio obstaculo 1
            //OJO, solo funcionan sonidos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
            //y tiene que decir "1 Channel".
            emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\armon�a, continuo.wav", obstaculos[0]));
            //Hay que configurar la m�nima distancia a partir de la cual se empieza a atenuar el sonido 3D
            //sound.MinDistance = 50f;
            //emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\risa de man�aco.wav", obstaculos[1]));
            //emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\viento helado.wav", obstaculos[2]));

            this.receptor = new TgcSoundListener(personaje);
            /*sound = new Tgc3dSound(MediaDir + "Sound\\risa de man�aco.wav", obstaculo.Position, DirectSound.DsDevice);
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            sound = new Tgc3dSound(MediaDir + "Sound\\viento helado.wav", obstaculo.Position, DirectSound.DsDevice);
            sound.MinDistance = 50f;
            sonidos.Add(sound);*/

        }

        public override void Update()
        {
            PreUpdate();

            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -VELODICAD_CAMINAR;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = VELODICAD_CAMINAR;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }
            personaje.Move(0, 0, 10);

            //Si hubo desplazamiento
            if (moving)
            {
                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personaje.Position;
                personaje.MoveOrientedY(moveForward * ElapsedTime);

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

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            this.receptor.SetOrientation((camaraInterna.Position - camaraInterna.LookAt), new TGCVector3(0, 1, 0));

            foreach(TgcSoundEmitter emisor in emisores)
            {
                emisor.Update(ElapsedTime, receptor);
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            
            //Calcular proxima posicion de personaje segun Input
           
            //Render piso
            piso.Render();

            //Render obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.Render();
            }

            //Render personaje
            personaje.Render();
            
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

            foreach (var emisor in emisores)
            {
                emisor.Dispose();
            }
        }
    }
}