using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Viewer;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Ejemplo EjemploTextureFiltering:
    ///     Unidades:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    ///     Muestra como aplicar transformaciones de DirectX a un triángulo
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class Transformations : TgcExample
    {
        //Array de vértices para crear el triángulo
        private CustomVertex.PositionColored[] data;

        private VertexBuffer vertexBuffer;

        public override string getCategory()
        {
            return "Transformations";
        }

        public override string getName()
        {
            return "Transformaciones";
        }

        public override string getDescription()
        {
            return "Transformaciones en 2d";
        }

        public override void init()
        {
            //Crear VertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //FPS Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0.5f, 0, -3), new Vector3(0, 0, 0));

            //User Vars
            GuiController.Instance.UserVars.addVar("Vertices", 0);
            GuiController.Instance.UserVars.addVar("Triangles", 0);

            GuiController.Instance.Modifiers.addFloat("translateX", -5, 5f, 0f);
            GuiController.Instance.Modifiers.addFloat("rotationZ", 0, (float)(2.0f * Math.PI), 0f);
            GuiController.Instance.Modifiers.addFloat("ScaleXYZ", 0, 3, 1f);
        }

        public override void render(float elapsedTime)
        {
            //Especificar formato de triangulos
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;

            //Configurar transformacion
            transform(D3DDevice.Instance.Device);

            //Cargar VertexBuffer a renderizar
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Dibujar 1 primitiva
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);
        }

        /// <summary>
        ///     Aplicar transformacion
        /// </summary>
        private void transform(Device d3dDevice)
        {
            //Crear matrices
            var matTranslate = new Matrix();
            var matRotate = new Matrix();
            var matScale = new Matrix();
            var matFinal = new Matrix();

            matTranslate = Matrix.Identity;
            matRotate = Matrix.Identity;
            matScale = Matrix.Identity;
            matFinal = Matrix.Identity;

            //Generar las matrices para cada movimiento
            matTranslate.Translate((float)GuiController.Instance.Modifiers["translateX"], 0, 0);
            matRotate.RotateZ((float)GuiController.Instance.Modifiers["rotationZ"]);
            var scale = (float)GuiController.Instance.Modifiers["ScaleXYZ"];
            matScale.Scale(scale, scale, scale);

            //Multiplicar todas las matrices en una sola final
            matFinal = matScale * matRotate * matTranslate;

            //Configurar la matriz de transformación actual de DirectX para todo lo que se va a dibujar a continuacion
            d3dDevice.Transform.World = matFinal;
        }

        public override void close()
        {
        }
    }
}