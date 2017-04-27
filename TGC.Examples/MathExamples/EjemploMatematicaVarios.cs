using System.Drawing;
using TGC.Core.Mathematica;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.MathExamples
{
    /// <summary>
    ///     Ejemplo EjemploMatematicaVarios:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Anexo matematica 3D
    ///     Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios.
    ///     Muestra como hacer distintas operaciones matematicas que son comunes a la hora de programar en 3D,
    ///     como producto escalar entre vectores, producto vectorial, sacar el angulo entre vectores, etc.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploMatematicaVarios : TGCExampleViewer
    {
        public EjemploMatematicaVarios(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Math Examples";
            Name = "Operaciones con vectores";
            Description =
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios. Muestra como hacer distintas operaciones matematicas que son comunes.";
        }

        public override void Init()
        {
            //none for init.
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            // 1) Crear un vector en 3D
            var v = new TGCVector3(0, 19, -1759.21f);

            // 2) Producto escalar entre dos vectores (dot product)
            var v1 = new TGCVector3(0, 19, -1759.21f);
            var v2 = new TGCVector3(0, 19, -1759.21f);
            var dotResult = TGCVector3.Dot(v1, v2);

            // 3) Producto vectorial entre dos vectores (cross product). El orden de v1 y v2 influye en la orientacion del resultado
            var crossResultVec = TGCVector3.Cross(v1, v2);

            // 4) Distancia entre dos puntos
            var p1 = new TGCVector3(100, 200, 300);
            var p2 = new TGCVector3(1000, 2000, 3000);
            var distancia = TGCVector3.Length(p2 - p1);
            var distanciaCuadrada = TGCVector3.LengthSq(p2 - p1);
            //Es mas eficiente porque evita la raiz cuadrada (pero te da el valor al cuadrado)

            // 5) Normalizar vector
            var norm = TGCVector3.Normalize(v1);

            // 6) Obtener el angulo que hay entre dos vectores que estan en XZ, expresion A.B=|A||B|cos(a)
            var v3 = new TGCVector3(-1, 0, 19);
            var v4 = new TGCVector3(3, 0, -5);
            var angle = FastMath.Acos(TGCVector3.Dot(TGCVector3.Normalize(v3), TGCVector3.Normalize(v4)));
            //Tienen que estar normalizados

            // 7) Tenemos un objeto que rota un cierto angulo en Y (ej: un auto) y queremos saber los componentes X,Z para donde tiene que avanzar al moverse
            var rotacionY = FastMath.PI_HALF;
            var componenteX = FastMath.Sin(rotacionY);
            var componenteZ = FastMath.Cos(rotacionY);
            float velocidadMovimiento = 100; //Ojo que este valor deberia siempre multiplicarse por el elapsedTime
            var movimientoAdelante = new TGCVector3(componenteX * velocidadMovimiento, 0, componenteZ * velocidadMovimiento);

            DrawText.drawText(
                "Este ejemplo no muestra nada por pantalla. Sino que es para leer el codigo y sus comentarios.", 5, 50,
                Color.Yellow);

            PostRender();
        }

        public override void Dispose()
        {
            //nada en state.
        }
    }
}