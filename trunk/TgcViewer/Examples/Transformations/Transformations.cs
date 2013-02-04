using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX;
using System.Drawing;

namespace Examples.Transformations
{
    /// <summary>
    /// Ejemplo EjemploTextureFiltering:
    /// Unidades:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    /// 
    /// Muestra como aplicar transformaciones de DirectX a un triángulo
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class Transformations : TgcExample
    {

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
        //Array de vértices para crear el triángulo
        CustomVertex.PositionColored[] data;

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear VertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //FPS Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0.5f, 0,-3), new Vector3(0, 0, 0));

            //User Vars
            GuiController.Instance.UserVars.addVar("Vertices", 0);
            GuiController.Instance.UserVars.addVar("Triangles", 0);

            GuiController.Instance.Modifiers.addFloat("translateX", -5, 5f, 0f);
            GuiController.Instance.Modifiers.addFloat("rotationZ", 0, (float)(2.0f * Math.PI), 0f);
            GuiController.Instance.Modifiers.addFloat("ScaleXYZ", 0, 3, 1f);
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Especificar formato de triangulos
            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;

            //Configurar transformacion
            transform(d3dDevice);

            //Cargar VertexBuffer a renderizar
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);

            //Dibujar 1 primitiva
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 1,data);
            

        }

        /// <summary>
        /// Aplicar transformacion
        /// </summary>
        private void transform(Device d3dDevice)
        {
            //Crear matrices
            Matrix matTranslate = new Matrix();
            Matrix matRotate = new Matrix();
            Matrix matScale = new Matrix();
            Matrix matFinal = new Matrix();

            matTranslate = Matrix.Identity;
            matRotate = Matrix.Identity;
            matScale = Matrix.Identity;
            matFinal = Matrix.Identity;

            //Generar las matrices para cada movimiento
            matTranslate.Translate((float)GuiController.Instance.Modifiers["translateX"], 0, 0);
            matRotate.RotateZ((float)GuiController.Instance.Modifiers["rotationZ"]);
            float scale = (float)GuiController.Instance.Modifiers["ScaleXYZ"];
            matScale.Scale(scale,scale,scale);
            

            //Multiplicar todas las matrices en una sola final
            matFinal = matScale * matRotate* matTranslate;

            //Configurar la matriz de transformación actual de DirectX para todo lo que se va a dibujar a continuacion
            d3dDevice.Transform.World = matFinal;
        }


        public override void close()
        {
        }

    }
}
