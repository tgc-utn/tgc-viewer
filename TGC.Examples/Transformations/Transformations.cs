using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Transformations
{
    /// <summary>
    ///     Ejemplo EjemploTextureFiltering:
    ///     Unidades:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Transformaciones
    ///     Muestra como aplicar transformaciones de DirectX a un triangulo
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class Transformations : TGCExampleViewer
    {
        //Array de vertices para crear el triangulo
        private CustomVertex.PositionColored[] data;

        private VertexBuffer vertexBuffer;

        public Transformations(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Transformations";
            Name = "Transformaciones";
            Description = "Transformaciones en 2d.";
        }

        public override void Init()
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
            Camara = new TgcFpsCamera(new Vector3(0.5f, 0, -3));

            //User Vars
            UserVars.addVar("Vertices", 0);
            UserVars.addVar("Triangles", 0);

            //Modifiers
            Modifiers.addFloat("translateX", -5, 5f, 0f);
            Modifiers.addFloat("rotationZ", 0, (float)(2.0f * Math.PI), 0f);
            Modifiers.addFloat("ScaleXYZ", 0, 3, 1f);
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

            //Configurar transformacion
            transform(D3DDevice.Instance.Device);

            //Cargar VertexBuffer a renderizar
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Dibujar 1 primitiva
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);

            PostRender();
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
            matTranslate.Translate((float)Modifiers["translateX"], 0, 0);
            matRotate.RotateZ((float)Modifiers["rotationZ"]);
            var scale = (float)Modifiers["ScaleXYZ"];
            matScale.Scale(scale, scale, scale);

            //Multiplicar todas las matrices en una sola final
            matFinal = matScale * matRotate * matTranslate;

            //Configurar la matriz de transformacion actual de DirectX para todo lo que se va a dibujar a continuacion
            d3dDevice.Transform.World = matFinal;
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
        }
    }
}