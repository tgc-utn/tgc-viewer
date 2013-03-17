using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// Ejemplo TrianguloBasico:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como crear un triángulo 3D de la forma más sencilla,
    /// especificando vértice por vértice.
    /// El triángulo se crea con colores por vértice.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class TrianguloBasico : TgcExample
    {
        //Array de vértices para crear el triángulo
        CustomVertex.PositionColored[] data;


        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Triangulo Básico";
        }

        public override string getDescription()
        {
            return "Crea un triangulo 3D básico con color. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Definir array de vertices para el triangulo, del tipo Coordendas (X,Y,Z) + Color
            data = new CustomVertex.PositionColored[3];

            //Cargar información de vertices. Nesitamos 3 vertices para crear un triángulo
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());


            //Configurar camara en rotacion
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0.5f, 0), 3f);

            //Cargar variables de usuario con alguna informacion util para ver en pantalla
            GuiController.Instance.UserVars.addVar("Cantida de Vertices", data.Length);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Especificar formato de triangulo
            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;
            //Dibujar 1 primitiva (nuestro triangulo)
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);
            
        }

        public override void close()
        {
        }

    }
}
