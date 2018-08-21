using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Collision
{
    /// <summary>
    ///     Ejemplo EjemploPicking:
    ///     Unidades Involucradas:
    ///     # Unidad 6 - Detección de Colisiones - Ray
    ///     Muestra como detectar los objetos que se interponen en la visión del usuario
    ///     cuando se utiliza una cámara en 3ra persona.
    ///     Acorta la distancia de la camara a la mínima colision encontrada con los objetos del escenario.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploColisionCamara : TGCExampleViewer
    {
        private TGCFloatModifier offsetHeightModifier;
        private TGCFloatModifier offsetForwardModifier;
        private TGCVertex2fModifier displacementModifier;

        private TgcThirdPersonCamera camaraInterna;
        private List<TGCBox> obstaculos;
        private TgcSkeletalMesh personaje;
        private TgcPlane piso;

        public EjemploColisionCamara(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Collision";
            Name = "Colision con Camara";
            Description =
                "Detecta los objetos que se interponen en la visión de la cámara en 3ra persona y renderiza solo su BoundingBox. Movimiento con W, A, S, D.";
        }

        public override void Init()
        {
            //Crear piso
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\tierra.jpg");
            piso = new TgcPlane(TGCVector3.Empty, new TGCVector3(2000, 0, 2000), TgcPlane.Orientations.XZplane, pisoTexture, 50f, 50f);
            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TGCBox>();
            TGCBox obstaculo;

            float wallSize = 1000;
            float wallHeight = 500;

            //Obstaculo 1
            obstaculo = TGCBox.fromExtremes(TGCVector3.Empty, new TGCVector3(wallSize, wallHeight, 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculo.AutoTransform = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TGCBox.fromExtremes(TGCVector3.Empty, new TGCVector3(10, wallHeight, wallSize),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg"));
            obstaculo.AutoTransform = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TGCBox.fromExtremes(new TGCVector3(0, 0, wallSize),
                new TGCVector3(wallSize, wallHeight, wallSize + 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculo.AutoTransform = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 4
            obstaculo = TGCBox.fromExtremes(new TGCVector3(wallSize, 0, 0),
                new TGCVector3(wallSize + 10, wallHeight, wallSize),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculo.AutoTransform = true;
            obstaculos.Add(obstaculo);

            //Obstaculo 5
            obstaculo = TGCBox.fromExtremes(new TGCVector3(wallSize / 2, 0, wallSize - 400),
                new TGCVector3(wallSize + 10, wallHeight, wallSize - 400 + 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculo.AutoTransform = true;
            obstaculos.Add(obstaculo);

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

            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            //Escalarlo porque es muy grande
            personaje.Position = new TGCVector3(100, 0, 100);
            personaje.Scale = new TGCVector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.RotateY(Geometry.DegreeToRadian(180f));

            //Configurar camara en Tercera Persona y la asigno al TGC.
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 200, -300);
            Camara = camaraInterna;

            //Modifiers para modificar propiedades de la camara
            offsetHeightModifier = AddFloat("offsetHeight", 0, 300, 100);
            offsetForwardModifier = AddFloat("offsetForward", -400, 0, -220);
            displacementModifier = AddVertex2f("displacement", TGCVector2.Zero, new TGCVector2(100, 200), new TGCVector2(0, 100));
        }

        public override void Update()
        {
            PreUpdate();
            var velocidadCaminar = 400f;
            var velocidadRotacion = 120f;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

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

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                var lastPos = personaje.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
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

                //Hacer que la camara siga al personaje en su nueva posicion
                camaraInterna.Target = personaje.Position;
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                personaje.playAnimation("Parado", true);
            }

            //Ajustar la posicion de la camara segun la colision con los objetos del escenario
            ajustarPosicionDeCamara();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Render piso
            piso.Render();

            //Render de obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.Render();
            }

            personaje.Transform = TGCMatrix.Scaling(personaje.Scale)
                            * TGCMatrix.RotationYawPitchRoll(personaje.Rotation.Y, personaje.Rotation.X, personaje.Rotation.Z)
                            * TGCMatrix.Translation(personaje.Position);
            //Render personaje
            personaje.animateAndRender(ElapsedTime);

            PostRender();
        }

        /// <summary>
        ///     Ajustar la posicion de la camara segun la colision con los objetos del escenario.
        ///     Acerca la distancia entre el persona y la camara si hay colisiones de objetos
        ///     en el medio
        /// </summary>
        private void ajustarPosicionDeCamara()
        {
            //Actualizar valores de camara segun modifiers
            camaraInterna.OffsetHeight = offsetHeightModifier.Value;
            camaraInterna.OffsetForward = offsetForwardModifier.Value;
            var displacement = displacementModifier.Value;
            camaraInterna.TargetDisplacement = new TGCVector3(displacement.X, displacement.Y, 0);

            //Pedirle a la camara cual va a ser su proxima posicion
            TGCVector3 position;
            TGCVector3 target;
            camaraInterna.CalculatePositionTarget(out position, out target);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            TGCVector3 q;
            var minDistSq = FastMath.Pow2(camaraInterna.OffsetForward);
            foreach (var obstaculo in obstaculos)
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(target, position, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    var distSq = TGCVector3.Subtract(q, target).LengthSq();
                    //Hay dos casos singulares, puede que tengamos mas de una colision hay que quedarse con el menor offset.
                    //Si no dividimos la distancia por 2 se acerca mucho al target.
                    minDistSq = FastMath.Min(distSq / 2, minDistSq);
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            var newOffsetForward = -FastMath.Sqrt(minDistSq);

            if (FastMath.Abs(newOffsetForward) < 10)
            {
                newOffsetForward = 10;
            }
            camaraInterna.OffsetForward = newOffsetForward;

            //Asignar la ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            camaraInterna.CalculatePositionTarget(out position, out target);
            camaraInterna.SetCamera(position, target);
        }

        public override void Dispose()
        {
            piso.Dispose();
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.Dispose();
            }
            personaje.Dispose();
        }
    }
}