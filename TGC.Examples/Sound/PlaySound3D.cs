using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
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
        private List<TGC3DSound> sonidos;

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
            var pisoTexture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = TGCBox.fromSize(new TGCVector3(5000, 5, 5000), pisoTexture);
            piso.Transform = TGCMatrix.Scaling(piso.Scale)
                        * TGCMatrix.RotationYawPitchRoll(piso.Rotation.Y, piso.Rotation.X, piso.Rotation.Z)
                        * TGCMatrix.Translation(piso.Position);
            piso.Position = new TGCVector3(0, -60, 0);

            //Cargar obstaculos y posicionarlos. Los obstaculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TGCBox>();
            sonidos = new List<TGC3DSound>();
            TGCBox obstaculo;
            TGC3DSound sound;

            //Obstaculo 1
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 150, 80),
                TGCTexture.CreateTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\Quake\\TexturePack3\\goo2.jpg"));
            obstaculo.Position = new TGCVector3(-250, 0, 0);
            obstaculo.Transform = TGCMatrix.Scaling(obstaculo.Scale)
                        * TGCMatrix.RotationYawPitchRoll(obstaculo.Rotation.Y, obstaculo.Rotation.X, obstaculo.Rotation.Z)
                        * TGCMatrix.Translation(obstaculo.Position);
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 1
            //OJO, solo funcionan sonidos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
            //y tiene que decir "1 Channel".
            sound = new TGC3DSound(MediaDir + "Sound\\armon�a, continuo.wav", obstaculo.Position, DirectSound.DsDevice);
            //Hay que configurar la m�nima distancia a partir de la cual se empieza a atenuar el sonido 3D
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            //Obstaculo 2
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 300, 80),
                TGCTexture.CreateTexture(D3DDevice.Instance.Device,
                    MediaDir + "Texturas\\Quake\\TexturePack3\\lun_dirt.jpg"));
            obstaculo.Position = new TGCVector3(250, 0, 800);
            obstaculo.Transform = TGCMatrix.Translation(obstaculo.Position);
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 2
            sound = new TGC3DSound(MediaDir + "Sound\\viento helado.wav", obstaculo.Position, DirectSound.DsDevice);
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            //Obstaculo 3
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 100, 150),
                TGCTexture.CreateTexture(D3DDevice.Instance.Device,
                    MediaDir + "Texturas\\Quake\\TexturePack3\\Metal2_1.jpg"));
            obstaculo.Position = new TGCVector3(500, 0, -400);
            obstaculo.Transform = TGCMatrix.Scaling(obstaculo.Scale)
                        * TGCMatrix.RotationYawPitchRoll(obstaculo.Rotation.Y, obstaculo.Rotation.X, obstaculo.Rotation.Z)
                        * TGCMatrix.Translation(obstaculo.Position);
            obstaculos.Add(obstaculo);

            //Sondio obstaculo 3
            sound = new TGC3DSound(MediaDir + "Sound\\risa de man�aco.wav", obstaculo.Position, DirectSound.DsDevice);
            sound.MinDistance = 50f;
            sonidos.Add(sound);

            //Cargar personaje principal
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");
            personaje = scene.Meshes[0];
            personaje.Position = new TGCVector3(0, 0, 0);
            personaje.Transform = TGCMatrix.Scaling(personaje.Scale)
                * TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z)
                * TGCMatrix.Translation(personaje.Position);

            //Hacer que el Listener del sonido 3D siga al personaje
            DirectSound.ListenerTracking = personaje;

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 250, 500);
            Camera = camaraInterna;

            //Ejecutar en loop los sonidos
            foreach (var s in sonidos)
            {
                s.Play(true);
            }
        }

        public override void Update()
        {
            //Calcular proxima posicion de personaje segun Input
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
                personaje.Rotation += new TGCVector3(0, rotAngle, 0);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personaje.Position;
                var moveF = moveForward * ElapsedTime;
                var z = (float)Math.Cos(personaje.Rotation.Y) * moveF;
                var x = (float)Math.Sin(personaje.Rotation.Y) * moveF;

                personaje.Position += new TGCVector3(x, 0, z);

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

            personaje.Transform = TGCMatrix.Scaling(personaje.Scale)
                                  * TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z)
                                  * TGCMatrix.Translation(personaje.Position);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;
        }

        public override void Render()
        {
            PreRender();

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

            foreach (var sound in sonidos)
            {
                sound.Dispose();
            }
        }
    }
}