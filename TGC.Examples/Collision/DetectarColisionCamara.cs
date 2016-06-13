using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

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
    public class DetectarColisionCamara : TgcExample
    {
        private List<TgcBox> obstaculos;
        private TgcSkeletalMesh personaje;
        private TgcBox piso;
        private TgcThirdPersonCamera camaraInterna; 

        public DetectarColisionCamara(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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
            piso = TgcBox.fromExtremes(new Vector3(-1000, -2, -1000), new Vector3(1000, 0, 1000), pisoTexture);

            //Cargar obstaculos y posicionarlos. Los obstáculos se crean con TgcBox en lugar de cargar un modelo.
            obstaculos = new List<TgcBox>();
            TgcBox obstaculo;

            float wallSize = 1000;
            float wallHeight = 500;

            //Obstaculo 1
            obstaculo = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(wallSize, wallHeight, 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\baldosaFacultad.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 2
            obstaculo = TgcBox.fromExtremes(new Vector3(0, 0, 0), new Vector3(10, wallHeight, wallSize),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\madera.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 3
            obstaculo = TgcBox.fromExtremes(new Vector3(0, 0, wallSize),
                new Vector3(wallSize, wallHeight, wallSize + 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 4
            obstaculo = TgcBox.fromExtremes(new Vector3(wallSize, 0, 0),
                new Vector3(wallSize + 10, wallHeight, wallSize),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
            obstaculos.Add(obstaculo);

            //Obstaculo 5
            obstaculo = TgcBox.fromExtremes(new Vector3(wallSize / 2, 0, wallSize - 400),
                new Vector3(wallSize + 10, wallHeight, wallSize - 400 + 10),
                TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\granito.jpg"));
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
            personaje.Position = new Vector3(100, 0, 100);
            personaje.Scale = new Vector3(0.75f, 0.75f, 0.75f);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));

            //Configurar camara en Tercera Persona y la asigno al TGC.
            camaraInterna = new TgcThirdPersonCamera(personaje.Position, 200, -300);
            Camara = camaraInterna;

            //Modifiers para modificar propiedades de la camara
            Modifiers.addFloat("offsetHeight", 0, 300, 100);
            Modifiers.addFloat("offsetForward", -400, 0, -220);
            Modifiers.addVertex2f("displacement", new Vector2(0, 0), new Vector2(100, 200), new Vector2(0, 100));
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            var velocidadCaminar = 400f;
            var velocidadRotacion = 120f;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var d3dInput = TgcD3dInput.Instance;
            var moving = false;
            var rotating = false;

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
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.rotateY(rotAngle);
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
                personaje.moveOrientedY(moveForward * ElapsedTime);

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
            ajustarPosicionDeCamara(obstaculos);

            //Render piso
            piso.render();

            //Render de obstaculos
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.render();
            }

            //Render personaje
            personaje.animateAndRender(ElapsedTime);
            
            FinalizarEscena();
        }

        /// <summary>
        ///     Ajustar la posicion de la camara segun la colision con los objetos del escenario.
        ///     Acerca la distancia entre el persona y la camara si hay colisiones de objetos
        ///     en el medio
        /// </summary>
        private void ajustarPosicionDeCamara(List<TgcBox> obstaculos)
        {
            //Actualizar valores de camara segun modifiers
            camaraInterna.OffsetHeight = (float)Modifiers["offsetHeight"];
            camaraInterna.OffsetForward = (float)Modifiers["offsetForward"];
            var displacement = (Vector2)Modifiers["displacement"];
            camaraInterna.TargetDisplacement = new Vector3(displacement.X, displacement.Y, 0);

            //Pedirle a la camara cual va a ser su proxima posicion
            Vector3 segmentA;
            Vector3 segmentB;
            camaraInterna.updatePositionTarget(out segmentA, out segmentB);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            Vector3 q;
            var minDistSq = FastMath.Pow2(camaraInterna.OffsetForward);
            foreach (var obstaculo in obstaculos)
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(segmentB, segmentA, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    var distSq = Vector3.Subtract(q, segmentB).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;

                        //Le restamos un poco para que no se acerque tanto
                        minDistSq /= 2;
                    }
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            var newOffsetForward = -FastMath.Sqrt(minDistSq);
            /*
            if(newOffsetForward < 10)
            {
                newOffsetForward = 10;
            }*/
            camaraInterna.OffsetForward = newOffsetForward;
        }

        public override void Close()
        {
            base.Close();

            piso.dispose();
            foreach (var obstaculo in obstaculos)
            {
                obstaculo.dispose();
            }
            personaje.dispose();
        }
    }
}