using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Transformations
{
    public class EjemploBrazoRobot : TGCExampleViewer
    {
        private const float VELOCIDAD_ANGULAR = 1.1f;
        private float antebrazoDAng;
        private readonly float antebrazoDX = 0.75f;
        private readonly float antebrazoDY = 1.5f;
        private readonly float antebrazoDZ = 0.75f;

        // medidas fijas y trasnformaciones de cada componente.
        private readonly float baseDX = 1.0f;

        private readonly float baseDY = 0.5f;
        private readonly float baseDZ = 1.0f;
        private TGCBox box;
        private float brazoDAng;
        private readonly float brazoDx = 0.5f;
        private readonly float brazoDY = 1f;
        private readonly float brazoDZ = 0.5f;
        private TGCMatrix escalaAntebrazo;
        private TGCMatrix escalaBase;
        private TGCMatrix escalaBrazo;
        private TGCMatrix escalaPinza;
        private float pinzaDang;
        private readonly float pinzaDX = 0.15f;
        private readonly float pinzaDY = 0.5f;
        private readonly float pinzaDZ = 0.15f;
        private float pinzaTraslacion;
        private TGCMatrix transformacionAntebrazo;
        private TGCMatrix transformacionBrazo;
        private TGCMatrix transformacionPinzaDerecha;
        private TGCMatrix transformacionPinzaIzquierda;

        public EjemploBrazoRobot(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Transformations";
            Name = "Brazo de robot";
            Description =
                "Ejemplo brazo de robot. ARR/ABA->ang brazo, Der/Izq->ang antebrazo, A/S -> ang pinza, Q/W -> abrir y cerrar pinza";
        }

        public override void Init()
        {
            var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Metal\\floor1.jpg");
            var center = TGCVector3.Empty;
            var size = new TGCVector3(1f, 1f, 1f);
            box = TGCBox.fromSize(center, size, texture);
            box.Transform = TGCMatrix.Identity;
            Camara = new TgcRotationalCamera(new TGCVector3(0f, 1.5f, 0f), 5f, Input);
        }

        public override void Update()
        {
            PreUpdate();

            // Los movimientos de teclado no validan que la mesh se atraviecen, solo modifican el angulo o traslacion.
            if (Input.keyDown(Key.A))
            {
                pinzaDang += VELOCIDAD_ANGULAR * ElapsedTime;
            }
            else if (Input.keyDown(Key.S))
            {
                pinzaDang -= VELOCIDAD_ANGULAR * ElapsedTime;
            }

            if (Input.keyDown(Key.Q))
            {
                pinzaTraslacion += VELOCIDAD_ANGULAR * ElapsedTime;
            }
            else if (Input.keyDown(Key.W))
            {
                pinzaTraslacion -= VELOCIDAD_ANGULAR * ElapsedTime;
            }

            if (Input.keyDown(Key.Left))
            {
                antebrazoDAng += VELOCIDAD_ANGULAR * ElapsedTime;
            }
            else if (Input.keyDown(Key.Right))
            {
                antebrazoDAng -= VELOCIDAD_ANGULAR * ElapsedTime;
            }

            if (Input.keyDown(Key.Up))
            {
                brazoDAng += VELOCIDAD_ANGULAR * ElapsedTime;
            }
            else if (Input.keyDown(Key.Down))
            {
                brazoDAng -= VELOCIDAD_ANGULAR * ElapsedTime;
            }

            // 1- Base del brazo
            // ajusto a la medida fija
            // Estas medidas fijas de escalas podrian calcularse en Init, a fines didacticos se hacen en cada update.
            escalaBase = TGCMatrix.Scaling(baseDX, baseDY, baseDZ);

            // 2- Brazo
            // ajusto a la medida fija
            escalaBrazo = TGCMatrix.Scaling(brazoDx, brazoDY, brazoDZ);
            // y lo traslado un poco para arriba, para que quede ubicado arriba de la base
            var T = TGCMatrix.Translation(0, brazoDY / 2.0f + baseDY / 2.0f, 0);
            // Guardo el punto donde tiene que girar el brazo = en la parte de abajo del brazo
            // le aplico la misma transformacion que al brazo (sin tener en cuenta el escalado)
            var pivoteBrazo = TGCVector3.TransformCoordinate(new TGCVector3(0, -brazoDY / 2.0f, 0.0f), T);
            // Ahora giro el brazo sobre el pivote, para ello, primero traslado el centro del mesh al pivote,
            // ahi aplico la rotacion, y luego vuelvo a trasladar a la posicion original
            var Rot = TGCMatrix.RotationZ(brazoDAng);
            var A = TGCMatrix.Translation(-pivoteBrazo.X, -pivoteBrazo.Y, -pivoteBrazo.Z);
            var B = TGCMatrix.Translation(pivoteBrazo.X, pivoteBrazo.Y, pivoteBrazo.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionBrazo = T * A * Rot * B;

            // 3- ante brazo
            // ajusto a la medida fija
            escalaAntebrazo = TGCMatrix.Scaling(antebrazoDX, antebrazoDY, antebrazoDZ);
            T = TGCMatrix.Translation(0, brazoDY / 2 + antebrazoDY / 2.0f, 0) * transformacionBrazo;
            // Guardo el punto donde tiene que girar el antebrazo
            var pivoteAntebrazo = TGCVector3.TransformCoordinate(new TGCVector3(0, -antebrazoDY / 2.0f, 0.0f), T);
            // orientacion del antebrazo
            Rot = TGCMatrix.RotationZ(antebrazoDAng);
            A = TGCMatrix.Translation(-pivoteAntebrazo.X, -pivoteAntebrazo.Y, -pivoteAntebrazo.Z);
            B = TGCMatrix.Translation(pivoteAntebrazo.X, pivoteAntebrazo.Y, pivoteAntebrazo.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionAntebrazo = T * A * Rot * B;

            // 4- pinza izquierda
            escalaPinza = TGCMatrix.Scaling(pinzaDX, pinzaDY, pinzaDZ);
            var C = TGCMatrix.Translation(pinzaTraslacion, 0f, 0f);
            T = C * TGCMatrix.Translation(pinzaDX / 2 - antebrazoDX / 2, antebrazoDY / 2.0f + pinzaDY / 2.0f, 0) *
                transformacionAntebrazo;
            // Guardo el punto donde tiene que girar la pinza
            var pivotePinzaIzquierda = TGCVector3.TransformCoordinate(new TGCVector3(0, -pinzaDY / 2.0f, 0.0f), T);
            // orientacion de la pinza
            Rot = TGCMatrix.RotationZ(pinzaDang);
            A = TGCMatrix.Translation(-pivotePinzaIzquierda.X, -pivotePinzaIzquierda.Y, -pivotePinzaIzquierda.Z);
            B = TGCMatrix.Translation(pivotePinzaIzquierda.X, pivotePinzaIzquierda.Y, pivotePinzaIzquierda.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionPinzaIzquierda = T * A * Rot * B;

            // mano derecha
            escalaPinza = TGCMatrix.Scaling(pinzaDX, pinzaDY, pinzaDZ);
            C = TGCMatrix.Translation(-pinzaTraslacion, 0f, 0f);
            T = C * TGCMatrix.Translation(antebrazoDX / 2 - pinzaDX / 2, antebrazoDY / 2 + pinzaDY / 2.0f, 0) * transformacionAntebrazo;
            // Guardo el punto donde tiene que girar la pinza
            var pivotePinzaDerecha = TGCVector3.TransformCoordinate(new TGCVector3(0, -pinzaDY / 2.0f, 0.0f), T);
            // orientacion de la pinza
            Rot = TGCMatrix.RotationZ(-pinzaDang);
            A = TGCMatrix.Translation(-pivotePinzaDerecha.X, -pivotePinzaDerecha.Y, -pivotePinzaDerecha.Z);
            B = TGCMatrix.Translation(pivotePinzaDerecha.X, pivotePinzaDerecha.Y, pivotePinzaDerecha.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionPinzaDerecha = T * A * Rot * B;

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            // Primero asignamos la transformacion de la base, esta solo se escala.
            box.Transform = escalaBase;
            box.Render();

            // Asignamos la transformacion del brazo, se escala ya que estamos utilizando siempre la misma caja
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaBrazo * transformacionBrazo;
            box.Render();

            // Asignamos la transformacion del antebrazo, se escala ya que estamos utilizando siempre la misma caja
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaAntebrazo * transformacionAntebrazo;
            box.Render();

            // Asignamos la transformacion de la mano, se escala ya que estamos utilizando siempre la misma caja
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaPinza * transformacionPinzaIzquierda;
            box.Render();

            // Asignamos la transformacion de la pinza, se escala ya que estamos utilizando siempre la misma caja
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaPinza * transformacionPinzaDerecha;
            box.Render();

            PostRender();
        }

        public override void Dispose()
        {
            box.Dispose();
        }
    }
}