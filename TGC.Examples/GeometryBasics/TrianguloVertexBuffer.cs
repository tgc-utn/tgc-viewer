using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloVertexBuffer:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - VertexBuffer
    ///     Crea el mismo triangulo que el ejemplo TrianguloBasico.
    ///     Pero utiliza la herramienta de VertexBuffer para crearlo en forma mas optima.
    ///     En lugar de mandar a renderizar cada primitiva por separado, se envia
    ///     el VertexBuffer entero.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class TrianguloVertexBuffer : TGCExampleViewer
    {
        //Vertex buffer que se va a utilizar
        private VertexBuffer vertexBuffer;

        public TrianguloVertexBuffer(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "Triangulo VertexBuffer";
            Description = "Crea un triangulo 3D con color, utilizando Vertex Buffer. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            var data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Almacenar informacion en VertexBuffer
            vertexBuffer.SetData(data, 0, LockFlags.None);

            //Configurar camara en rotacion
            Camara = new TgcRotationalCamera(new Vector3(0, 0.5f, 0), 3f, Input);

            //User Vars
            UserVars.addVar("Vertices");
            UserVars.setValue("Vertices", data.Length);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Especificar formato de triangulos
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            //Cargar VertexBuffer a renderizar
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            //Dibujar 1 primitiva
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

            PostRender();
        }

        public override void Dispose()
        {
            //liberar VertexBuffer
            vertexBuffer.Dispose();
        }
    }
}