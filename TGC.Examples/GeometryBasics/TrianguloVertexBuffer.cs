using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Util;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloVertexBuffer:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - VertexBuffer
    ///     Crea el mismo triángulo que el ejemplo TrianguloBasico.
    ///     Pero utiliza la herramienta de VertexBuffer para crearlo en forma más
    ///     óptima.
    ///     En lugar de mandar a renderizar cada primitiva por separado, se envia
    ///     el VertexBuffer entero.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TrianguloVertexBuffer : TgcExample
    {
        //Vertex buffer que se va a utilizar
        private VertexBuffer vertexBuffer;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Triangulo VertexBuffer";
        }

        public override string getDescription()
        {
            return "Crea un triangulo 3D con color, utilizando Vertex Buffer. Movimiento con mouse.";
        }

        public override void init()
        {
            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            var data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Almacenar información en VertexBuffer
            vertexBuffer.SetData(data, 0, LockFlags.None);

            //Configurar camara en rotacion
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0.5f, 0), 3f);

            //User Vars
            GuiController.Instance.UserVars.addVar("Vertices");
            GuiController.Instance.UserVars.setValue("Vertices", data.Length);
        }

        public override void render(float elapsedTime)
        {
            //Especificar formato de triangulos
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            //Cargar VertexBuffer a renderizar
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            //Dibujar 1 primitiva
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }

        public override void close()
        {
            //liberar VertexBuffer
            vertexBuffer.Dispose();
        }
    }
}