using System;
using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;
using Microsoft.DirectX.DirectInput;

namespace TGC.Examples.Transformations
{
    public class EjemploBrazoRobot : TGCExampleViewer
    {
        private const float VELOCIDAD_ANGULAR = 1.1f;
        TgcBox box;

        // medidas fijas y trasnformaciones de cada componente.
        float baseDX = 1.0f;
        float baseDY = 0.5f;
        float baseDZ = 1.0f;
        Matrix escalaBase;
        float brazoDx = 0.5f;
        float brazoDY = 1f;
        float brazoDZ = 0.5f;
        float brazoDAng = 0.0f;
        Matrix escalaBrazo;
        Matrix transformacionBrazo;
        float antebrazoDX = 0.75f;
        float antebrazoDY = 1.5f;
        float antebrazoDZ = 0.75f;
        float antebrazoDAng = 0.0f;
        Matrix escalaAntebrazo;
        Matrix transformacionAntebrazo;
        float pinzaDX = 0.15f;
        float pinzaDY = 0.5f;
        float pinzaDZ = 0.15f;
        float pinzaDang = 0.0f;
        float pinzaTraslacion = 0.0f;
        Matrix escalaPinza;
        Matrix transformacionPinzaIzquierda;
        Matrix transformacionPinzaDerecha;

        public EjemploBrazoRobot(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Transformations";
            Name = "Brazo de robot";
            Description = "Ejemplo brazo de robot. ARR/ABA->ang brazo, Der/Izq->ang antebrazo, A/S -> ang pinza, Q/W -> abrir y cerrar pinza";
        }
        
        public override void Init()
        {
            TgcTexture texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Metal\\floor1.jpg");
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(1f, 1f, 1f);
            box = TgcBox.fromSize(center, size, texture);
            //Por defecto se deshabilito esto, cada uno debe implementar su modelo de transformaciones.
            //box.AutoTransformEnable = false;
            box.Transform = Matrix.Identity;
            Camara = new TgcRotationalCamera(new Vector3(0f, 1.5f, 0f), 5f, Input);
            
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
            escalaBase = Matrix.Scaling(baseDX, baseDY, baseDZ);            

            // 2- Brazo 
            // ajusto a la medida fija
            escalaBrazo = Matrix.Scaling(brazoDx, brazoDY, brazoDZ);
            // y lo traslado un poco para arriba, para que quede ubicado arriba de la base
            Matrix T = Matrix.Translation(0, brazoDY / 2.0f + baseDY / 2.0f, 0);
            // Guardo el punto donde tiene que girar el brazo = en la parte de abajo del brazo
            // le aplico la misma transformacion que al brazo (sin tener en cuenta el escalado)
            Vector3 pivoteBrazo = Vector3.TransformCoordinate(new Vector3(0, -brazoDY / 2.0f, 0.0f), T);
            // Ahora giro el brazo sobre el pivote, para ello, primero traslado el centro del mesh al pivote,
            // ahi aplico la rotacion, y luego vuelvo a trasladar a la posicion original
            Matrix Rot = Matrix.RotationZ(brazoDAng);
            Matrix A = Matrix.Translation(-pivoteBrazo.X, -pivoteBrazo.Y, -pivoteBrazo.Z);
            Matrix B = Matrix.Translation(pivoteBrazo.X, pivoteBrazo.Y, pivoteBrazo.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionBrazo = T * A * Rot * B;
            

            // 3- ante brazo
            // ajusto a la medida fija 
            escalaAntebrazo = Matrix.Scaling(antebrazoDX, antebrazoDY, antebrazoDZ);
            T = Matrix.Translation(0, brazoDY / 2 + antebrazoDY / 2.0f, 0) * transformacionBrazo;
            // Guardo el punto donde tiene que girar el antebrazo
            Vector3 pivoteAntebrazo = Vector3.TransformCoordinate(new Vector3(0, -antebrazoDY / 2.0f, 0.0f), T);
            // orientacion del antebrazo
            Rot = Matrix.RotationZ(antebrazoDAng);
            A = Matrix.Translation(-pivoteAntebrazo.X, -pivoteAntebrazo.Y, -pivoteAntebrazo.Z);
            B = Matrix.Translation(pivoteAntebrazo.X, pivoteAntebrazo.Y, pivoteAntebrazo.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionAntebrazo = T * A * Rot * B;
            

            // 4- pinza izquierda
            escalaPinza = Matrix.Scaling(pinzaDX, pinzaDY, pinzaDZ);
            Matrix C = Matrix.Translation(pinzaTraslacion, 0f, 0f);
            T = C * Matrix.Translation(pinzaDX / 2 - antebrazoDX / 2, antebrazoDY / 2.0f + pinzaDY / 2.0f, 0) * transformacionAntebrazo;
            // Guardo el punto donde tiene que girar la pinza
            Vector3 pivotePinzaIzquierda = Vector3.TransformCoordinate(new Vector3(0, -pinzaDY / 2.0f, 0.0f), T);
            // orientacion de la pinza
            Rot = Matrix.RotationZ(pinzaDang);
            A = Matrix.Translation(-pivotePinzaIzquierda.X, -pivotePinzaIzquierda.Y, -pivotePinzaIzquierda.Z);
            B = Matrix.Translation(pivotePinzaIzquierda.X, pivotePinzaIzquierda.Y, pivotePinzaIzquierda.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionPinzaIzquierda = T * A * Rot * B;
            

            // mano derecha
            escalaPinza = Matrix.Scaling(pinzaDX, pinzaDY, pinzaDZ);
            C = Matrix.Translation(-pinzaTraslacion, 0f, 0f);
            T = C * Matrix.Translation(antebrazoDX / 2 - pinzaDX / 2, antebrazoDY / 2 + pinzaDY / 2.0f, 0) * transformacionAntebrazo;
            // Guardo el punto donde tiene que girar la pinza
            Vector3 pivotePinzaDerecha = Vector3.TransformCoordinate(new Vector3(0, -pinzaDY / 2.0f, 0.0f), T);
            // orientacion de la pinza
            Rot = Matrix.RotationZ(-pinzaDang);
            A = Matrix.Translation(-pivotePinzaDerecha.X, -pivotePinzaDerecha.Y, -pivotePinzaDerecha.Z);
            B = Matrix.Translation(pivotePinzaDerecha.X, pivotePinzaDerecha.Y, pivotePinzaDerecha.Z);
            // Se calcula la matriz resultante, para utilizarse en render.
            transformacionPinzaDerecha = T * A * Rot * B;            
        }

        public override void Render()
        {
            PreRender();

            // Primero asignamos la transformacion de la base, esta solo se escala.
            box.Transform = escalaBase;
            box.render();

            // Asignamos la transformacion del brazo, se escala ya que estamos utilizando siempre la misma caja 
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaBrazo * transformacionBrazo;
            box.render();

            // Asignamos la transformacion del antebrazo, se escala ya que estamos utilizando siempre la misma caja 
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaAntebrazo * transformacionAntebrazo;
            box.render();

            // Asignamos la transformacion de la mano, se escala ya que estamos utilizando siempre la misma caja 
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaPinza * transformacionPinzaIzquierda;
            box.render();

            // Asignamos la transformacion de la pinza, se escala ya que estamos utilizando siempre la misma caja 
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaPinza * transformacionPinzaDerecha;
            box.render();

            PostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}
