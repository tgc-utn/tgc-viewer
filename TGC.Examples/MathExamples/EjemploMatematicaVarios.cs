using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.MathExamples
{
    /// <summary>
    ///     Ejemplo EjemploMatematicaVarios:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Anexo matemática 3D
    ///     Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.
    ///     Muestra como hacer distintas operaciones matemáticas que son comunes a la hora de programar en 3D,
    ///     como producto escalar entre vectores, producto vectorial, sacar el ángulo entre vectores, etc.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMatematicaVarios : TgcExample
    {
        public EjemploMatematicaVarios(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Math";
            Name = "Matematica varios";
            Description =
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios. Muestra como hacer distintas operaciones matemáticas que son comunes.";
        }

        public override void Init()
        {
            //none for init.
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            // 1) Crear un vector en 3D
            var v = new Vector3(0, 19, -1759.21f);

            // 2) Producto escalar entre dos vectores (dot product)
            var v1 = new Vector3(0, 19, -1759.21f);
            var v2 = new Vector3(0, 19, -1759.21f);
            var dotResult = Vector3.Dot(v1, v2);

            // 3) Producto vectorial entre dos vectores (cross product). El orden de v1 y v2 influye en la orientacion del resultado
            var crossResultVec = Vector3.Cross(v1, v2);

            // 4) Distancia entre dos puntos
            var p1 = new Vector3(100, 200, 300);
            var p2 = new Vector3(1000, 2000, 3000);
            var distancia = Vector3.Length(p2 - p1);
            var distanciaCuadrada = Vector3.LengthSq(p2 - p1);
            //Es mas eficiente porque evita la raiz cuadrada (pero te da el valor al cuadrado)

            // 5) Normalizar vector
            var norm = Vector3.Normalize(v1);

            // 6) Obtener el ángulo que hay entre dos vectores que están en XZ, expresion A.B=|A||B|cos(a)
            var v3 = new Vector3(-1, 0, 19);
            var v4 = new Vector3(3, 0, -5);
            var angle = FastMath.Acos(Vector3.Dot(Vector3.Normalize(v3), Vector3.Normalize(v4)));
            //Tienen que estar normalizados

            // 7) Tenemos un objeto que rota un cierto ángulo en Y (ej: un auto) y queremos saber los componentes X,Z para donde tiene que avanzar al moverse
            var rotacionY = FastMath.PI_HALF;
            var componenteX = FastMath.Sin(rotacionY);
            var componenteZ = FastMath.Cos(rotacionY);
            float velocidadMovimiento = 100; //Ojo que este valor deberia siempre multiplicarse por el elapsedTime
            var movimientoAdelante = new Vector3(componenteX * velocidadMovimiento, 0, componenteZ * velocidadMovimiento);

            TgcDrawText.Instance.drawText(
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.", 5, 50,
                Color.Yellow);

            helperPostRender();
        }
    }
}