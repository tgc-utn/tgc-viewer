using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Core.Text;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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

        private List<TgcCylinder> cilindros;
        private List<TGCFloatModifier> rangos;

        public PlaySound3D(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Sound";
            Name = "Play Sound3D";
            Description = "Muestra como reproducir un archivo de sonido 3D en formato WAV.";
        }

        public override void Init()
        {

            this.InitializeScene();
            this.InitializeModifiers();
            this.InitializeSound();
            this.InitializeCilinders();
        }

        private void InitializeScene()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = TGCBox.fromSize(new TGCVector3(5000, 5, 5000), pisoTexture);
            piso.AutoTransform = true;
            piso.Position = new TGCVector3(0, -60, 0);

            //Cargar obstaculos y posicionarlos. Los obstaculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TGCBox>();          

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

            //Obstaculo 3
            obstaculo = TGCBox.fromSize(new TGCVector3(80, 100, 150),
                TgcTexture.createTexture(D3DDevice.Instance.Device,
                    MediaDir + "Texturas\\Quake\\TexturePack3\\Metal2_1.jpg"));
            obstaculo.AutoTransform = true;
            obstaculo.Position = new TGCVector3(500, 0, -400);
            obstaculos.Add(obstaculo);

            //Cargar personaje principal
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");
            personaje = scene.Meshes[0];
            personaje.AutoTransform = true;
            personaje.Position = new TGCVector3(500, -50, 0);

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 250, 500);
            Camara = camaraInterna;
        }

        private void InitializeSound()
        {
            // Para manejar sonido 3D, solamente creamos emisores y un receptor con sus respectivos objetos
            // que implementen ITransformObject. El motor de sonido se encarga de actualizar las posiciones.
            // Notar que las direcciones y orientaciones de los objetos las debemos informar si corresponden.
            // Por eso en el Update informamos la orientacion del auto (listener)

            emisores = new List<TgcSoundEmitter>();

            // OJO, solo funcionan sonidos WAV Mono (No stereo). Hacer boton der => Propiedades sobre el archivo
            // y tiene que decir "1 Channel".
            emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\armonía, continuo.wav", obstaculos[0]));
            emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\risa de maníaco.wav", obstaculos[1]));
            emisores.Add(new TgcSoundEmitter(MediaDir + "Sound\\viento helado.wav", obstaculos[2]));

            receptor = new TgcSoundListener(personaje);
        }

        private void InitializeModifiers()
        {
            rangos = new List<TGCFloatModifier>();
            AddModifier("Rango Minimo Armonia", 200);
            AddModifier("Rango Maximo Armonia", 500);
            AddModifier("Rango Minimo Risa", 200);
            AddModifier("Rango Maximo Risa", 500);
            AddModifier("Rango Minimo Viento", 200);
            AddModifier("Rango Maximo Viento", 500);
        }

        private void AddModifier(string name, float defaultValue)
        {
            rangos.Add(AddFloat(name, 0, 1500, defaultValue));
        }

        private void InitializeCilinders()
        {
            cilindros = new List<TgcCylinder>();

            CreateCylinder(Color.FromArgb(122, 255, 0, 0));
            CreateCylinder(Color.FromArgb(122, 0, 255, 0));
            CreateCylinder(Color.FromArgb(122, 255, 0, 0));
            CreateCylinder(Color.FromArgb(122, 0, 255, 0));
            CreateCylinder(Color.FromArgb(122, 255, 0, 0));
            CreateCylinder(Color.FromArgb(122, 0, 255, 0));
        }

        private void CreateCylinder(Color color)
        {
            TgcCylinder cilindro = new TgcCylinder(TGCVector3.Empty, 1, 1);
            cilindro.Color = color;
            cilindro.AlphaBlendEnable = true;
            cilindro.updateValues();
            cilindros.Add(cilindro);
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

            // Actualizamos los valores de rangos minimos y maximos segun modificadores
            emisores[0].RangeMin = rangos[0].Value;
            emisores[0].RangeMax = rangos[1].Value;
            emisores[1].RangeMin = rangos[2].Value;
            emisores[1].RangeMax = rangos[3].Value;
            emisores[2].RangeMin = rangos[4].Value;
            emisores[2].RangeMax = rangos[5].Value;

            emisores[0].RangeMin = 3;
            // Transformamos los cilindros
            cilindros[0].Transform = TGCMatrix.Scaling(emisores[0].RangeMin, 20, emisores[0].RangeMin) * TGCMatrix.Translation(emisores[0].Position);
            cilindros[1].Transform = TGCMatrix.Scaling(emisores[0].RangeMax, 20, emisores[0].RangeMax) * TGCMatrix.Translation(emisores[0].Position);
            cilindros[2].Transform = TGCMatrix.Scaling(emisores[1].RangeMin, 20, emisores[1].RangeMin) * TGCMatrix.Translation(emisores[1].Position);
            cilindros[3].Transform = TGCMatrix.Scaling(emisores[1].RangeMax, 20, emisores[1].RangeMax) * TGCMatrix.Translation(emisores[1].Position);
            cilindros[4].Transform = TGCMatrix.Scaling(emisores[2].RangeMin, 20, emisores[2].RangeMin) * TGCMatrix.Translation(emisores[2].Position);
            cilindros[5].Transform = TGCMatrix.Scaling(emisores[2].RangeMax, 20, emisores[2].RangeMax) * TGCMatrix.Translation(emisores[2].Position);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = personaje.Position;

            // Calculamos la orientacion del auto
            // Esta operacion es importante para tener sonido a izquierda y a derecha independientes    
            // Tambien es necesario el vector arriba
            TGCVector3 orientation = (camaraInterna.Position - camaraInterna.LookAt);
            orientation.Normalize();
            this.receptor.SetOrientation(orientation, new TGCVector3(0, 1, 0));

            PostUpdate();
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

            //Render cilindros
            foreach (var cilindro in cilindros)
            {
                cilindro.Render();
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

            foreach(var cilindro in cilindros)
            {
                cilindro.Dispose();
            }

            foreach(var modificador in rangos)
            {
                modificador.Dispose();
            }

            receptor.Dispose();
        }
    }
}