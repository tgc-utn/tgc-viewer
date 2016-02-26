using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TGC.Core.Utils;

namespace Examples.Matematica
{
    /// <summary>
    /// Ejemplo EjemploMatematicaVarios:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Anexo matemática 3D
    /// 
    /// Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.
    /// Muestra como hacer distintas operaciones matemáticas que son comunes a la hora de programar en 3D,
    /// como producto escalar entre vectores, producto vectorial, sacar el ángulo entre vectores, etc.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploMatematicaVarios : TgcExample
    {

        public override string getCategory()
        {
            return "Matematica";
        }

        public override string getName()
        {
            return "Matematica varios";
        }

        public override string getDescription()
        {
            return "Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios. Muestra como hacer distintas operaciones matemáticas que son comunes.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            // 1) Crear un vector en 3D
            Vector3 v = new Vector3(0, 19, -1759.21f);


            // 2) Producto escalar entre dos vectores (dot product)
            Vector3 v1 = new Vector3(0, 19, -1759.21f);
            Vector3 v2 = new Vector3(0, 19, -1759.21f);
            float dotResult = Vector3.Dot(v1, v2);


            // 3) Producto vectorial entre dos vectores (cross product). El orden de v1 y v2 influye en la orientacion del resultado
            Vector3 crossResultVec = Vector3.Cross(v1, v2);


            // 4) Distancia entre dos puntos
            Vector3 p1 = new Vector3(100, 200, 300);
            Vector3 p2 = new Vector3(1000, 2000, 3000);
            float distancia = Vector3.Length(p2 - p1);
            float distanciaCuadrada = Vector3.LengthSq(p2 - p1); //Es mas eficiente porque evita la raiz cuadrada (pero te da el valor al cuadrado)


            // 5) Normalizar vector
            Vector3 norm = Vector3.Normalize(v1);


            // 6) Obtener el ángulo que hay entre dos vectores que están en XZ, expresion A.B=|A||B|cos(a)
            Vector3 v3 = new Vector3(-1, 0, 19);
            Vector3 v4 = new Vector3(3, 0, -5);
            float angle = FastMath.Acos(Vector3.Dot(Vector3.Normalize(v3), Vector3.Normalize(v4))); //Tienen que estar normalizados


            // 7) Tenemos un objeto que rota un cierto ángulo en Y (ej: un auto) y queremos saber los componentes X,Z para donde tiene que avanzar al moverse
            float rotacionY = FastMath.PI_HALF;
            float componenteX = FastMath.Sin(rotacionY);
            float componenteZ = FastMath.Cos(rotacionY);
            float velocidadMovimiento = 100; //Ojo que este valor deberia siempre multiplicarse por el elapsedTime
            Vector3 movimientoAdelante = new Vector3(componenteX * velocidadMovimiento, 0, componenteZ * velocidadMovimiento);










            GuiController.Instance.Text3d.drawText("Este ejemplo no muestra nada por pantalla. Sino que es para leer el código y sus comentarios.", 5, 50, Color.Yellow);
        }

        public override void close()
        {

        }

    }
}
