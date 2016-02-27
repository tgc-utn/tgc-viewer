using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TGC.Core.Example;

namespace Examples.DirectX
{
    /// <summary>
    ///     Ejemplo EjemploBoxDirectX:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - VertexBuffer
    ///     Muestra como crear una caja 3D usando DirectX a secas, sin utilizar nada del framework.
    ///     El objetivo del ejemplo es explicar como utilizar la API de DirectX directamente para poder hacer cosas
    ///     personalizadas.
    ///     Si lo que se desea es dibujar una caja 3D de forma rápida utilizar TgcBox.
    ///     Muestra como usar un vertex buffer y un index buffer.
    ///     El formato de vertice utilizado para la caja es 100% customizado.
    ///     Luego se usa un shader customizado para mostrar como interactuar con estos atributos de vertice personalizados.
    ///     El shader en si no tiene ninguna finalidad especial, solo es para mostrar como agregar nuevos atributos a un
    ///     vertice.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploBoxDirectX : TgcExample
    {
        /// <summary>
        ///     Vertex declaration para el vertice customizado. Hay que tener cuidado con la suma de bytes
        ///     1 float = 4 bytes
        ///     Vector2 = 8 bytes
        ///     Vector3 = 12 bytes
        ///     Color = 4 bytes (es un int)
        /// </summary>
        public static readonly VertexElement[] MyCustomVertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0), //Position

            new VertexElement(0, 12, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0), //Normal

            new VertexElement(0, 24, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0), //Color

            new VertexElement(0, 28, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0), //texcoord0

            new VertexElement(0, 36, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 1), //texcoord1

            new VertexElement(0, 44, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 2), //texcoord2

            new VertexElement(0, 56, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 3), //auxValue1 (se mandan como coordenadas de textura float3)

            new VertexElement(0, 68, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 4), //auxValue2 (se mandan como coordenadas de textura float3)

            VertexElement.VertexDeclarationEnd
        };

        private float acumTime;
        private float dir;
        private Effect effect;
        private IndexBuffer indexBuffer;
        private int indexCount;

        /// <summary>
        ///     Formato de vertice customizado
        /// </summary>
        private readonly VertexFormats MyCustomVertexFormat =
            VertexFormats.Position | //Position
            VertexFormats.Normal | //Normal
            VertexFormats.Diffuse | //Color
            VertexFormats.Texture0 | //texcoord0
            VertexFormats.Texture1 | //texcoord1
            VertexFormats.Texture2 | //texcoord2
            VertexFormats.Texture3 | //auxValue1 (se mandan como coordenadas de textura float3)
            VertexFormats.Texture4 //auxValue2 (se mandan como coordenadas de textura float3)
            ;

        private Texture texture0;
        private Texture texture1;
        private Texture texture2;
        private int triangleCount;
        private VertexBuffer vertexBuffer;
        private int vertexCount;
        private MyCustomVertex[] vertexData;
        private VertexDeclaration vertexDeclaration;

        public override string getCategory()
        {
            return "DirectX";
        }

        public override string getName()
        {
            return "Box DirectX";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D usando DirectX a secas, sin utilizar nada del framework.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Dimensiones de la caja
            var center = new Vector3(0, 0, 0);
            var size = new Vector3(10, 10, 10);
            var color1 = Color.Red.ToArgb();
            var color2 = Color.Green.ToArgb();
            var extents = size*0.5f;
            var min = center - extents;
            var max = center + extents;

            //Crear vertex declaration
            vertexDeclaration = new VertexDeclaration(d3dDevice, MyCustomVertexElements);

            //Crear un VertexBuffer con 8 vertices (los 8 extremos de la caja)
            vertexCount = 8;
            vertexData = new MyCustomVertex[vertexCount];
            vertexBuffer = new VertexBuffer(typeof (MyCustomVertex), vertexCount, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, MyCustomVertexFormat, Pool.Default);

            //Llenar array con los 8 vertices de la caja
            //La normal se carga combinando la dirección de las 3 caras que toca
            //inferiores (son las 4 combinaciones de min-max entre XZ)
            vertexData[0] = new MyCustomVertex(new Vector3(min.X, min.Y, min.Z),
                Vector3.Normalize(new Vector3(-1, -1, -1)), color1);
            vertexData[1] = new MyCustomVertex(new Vector3(max.X, min.Y, min.Z),
                Vector3.Normalize(new Vector3(1, -1, -1)), color1);
            vertexData[2] = new MyCustomVertex(new Vector3(min.X, min.Y, max.Z),
                Vector3.Normalize(new Vector3(-1, -1, 1)), color1);
            vertexData[3] = new MyCustomVertex(new Vector3(max.X, min.Y, max.Z),
                Vector3.Normalize(new Vector3(1, -1, 1)), color1);

            //superiores (idem inferiores pero con max.Y)
            vertexData[4] = new MyCustomVertex(new Vector3(min.X, max.Y, min.Z),
                Vector3.Normalize(new Vector3(-1, 1, -1)), color2);
            vertexData[5] = new MyCustomVertex(new Vector3(max.X, max.Y, min.Z),
                Vector3.Normalize(new Vector3(1, 1, -1)), color2);
            vertexData[6] = new MyCustomVertex(new Vector3(min.X, max.Y, max.Z),
                Vector3.Normalize(new Vector3(-1, 1, 1)), color2);
            vertexData[7] = new MyCustomVertex(new Vector3(max.X, max.Y, max.Z), Vector3.Normalize(new Vector3(1, 1, 1)),
                color2);

            //Setear información en VertexBuffer
            vertexBuffer.SetData(vertexData, 0, LockFlags.None);

            //Crear IndexBuffer con 36 vertices para los 12 triangulos que forman la caja (2 triangulos por cada => 6 vertices por cara => 6 caras)
            indexCount = 36;
            triangleCount = indexCount/3;
            var indexData = new short[indexCount];
            var iIdx = 0;
            indexBuffer = new IndexBuffer(typeof (short), indexCount, d3dDevice, Usage.None, Pool.Default);

            //Diagrama de caja (con un poco de imaginacion):
            /*
	            Superior:
	              6 ----- 7
	              |		  |
	              |		  |
	              4 ----- 5

	            Inferior:
	            2 ----- 3
	            |		|
	            |		|
	            0 ----- 1
            */

            //Abajo
            indexData[iIdx++] = 0;
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 2;
            indexData[iIdx++] = 0;

            //Arriba
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 6;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 4;

            //Atras
            indexData[iIdx++] = 2;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 6;
            indexData[iIdx++] = 2;

            //Adelante
            indexData[iIdx++] = 0;
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 0;

            //Izquierda
            indexData[iIdx++] = 0;
            indexData[iIdx++] = 2;
            indexData[iIdx++] = 6;
            indexData[iIdx++] = 6;
            indexData[iIdx++] = 4;
            indexData[iIdx++] = 0;

            //Derecha
            indexData[iIdx++] = 1;
            indexData[iIdx++] = 5;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 7;
            indexData[iIdx++] = 3;
            indexData[iIdx++] = 1;

            //Setear información en IndexBuffer
            indexBuffer.SetData(indexData, 0, LockFlags.None);

            //Cargar shader customizado para este ejemplo
            var shaderPath = GuiController.Instance.ExamplesMediaDir + "Shaders\\EjemploBoxDirectX.fx";
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, shaderPath, null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + shaderPath + ". Errores: " + compilationErrors);
            }

            //Setear el único technique que tiene
            effect.Technique = "EjemploBoxDirectX";

            //Cargamos 3 texturas cualquiera para mandar al shader
            texture0 = TextureLoader.FromFile(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_DiffuseMap.jpg");
            texture1 = TextureLoader.FromFile(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Shaders\\BumpMapping_NormalMap.jpg");
            texture2 = TextureLoader.FromFile(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Shaders\\efecto_alarma.png");

            dir = 1;

            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraDistance = 20;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            acumTime += elapsedTime;
            var speed = 20*elapsedTime;
            if (acumTime > 0.5f)
            {
                acumTime = 0;
                dir *= -1;
            }

            //Actualizamos el vertexBuffer. Esto no es para nada necesario. Si la geometria no va a cambiar entonces no es necesario actualizarlo
            //Aca se muestra para poder explicar como se hace
            //Actualizar el vertexBuffer es una operatoria lenta porque debe traer los datos de GPU a CPU y luego volverlos a mandar
            //En este ejemplo subimos y bajamos la caja en Y en forma intermitente
            //No se leen los datos que ya estan en el vertexBuffer sino que se usa una copia local (vertexData) y se pisan los del vertexBuffer
            for (var i = 0; i < vertexCount; i++)
            {
                var v = vertexData[i];
                vertexData[i].Position += new Vector3(0, dir*speed, 0);
            }
            vertexBuffer.SetData(vertexData, 0, LockFlags.None); //Manda la informacion actualizada a la GPU

            //Cargar vertex declaration
            d3dDevice.VertexDeclaration = vertexDeclaration;

            //Cargar vertexBuffer e indexBuffer
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);
            d3dDevice.Indices = indexBuffer;

            //Arrancar shader
            effect.Begin(0);
            effect.BeginPass(0);

            //Cargar matrices en shader
            var matWorldView = d3dDevice.Transform.View;
            var matWorldViewProj = matWorldView*d3dDevice.Transform.Projection;
            effect.SetValue("matWorld", Matrix.Identity);
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", Matrix.Identity);

            //Cargar las 3 texturas en el shader
            effect.SetValue("tex0", texture0);
            effect.SetValue("tex1", texture1);
            effect.SetValue("tex2", texture2);

            //Dibujar los triangulos haciendo uso del indexBuffer
            d3dDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCount, 0, triangleCount);

            //Finalizar shader
            effect.EndPass();
            effect.End();
        }

        public override void close()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            effect.Dispose();
            vertexDeclaration.Dispose();
            texture0.Dispose();
            texture1.Dispose();
            texture2.Dispose();
        }

        /// <summary>
        ///     Estructura de vertice personalizada
        /// </summary>
        private struct MyCustomVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public int Color;

            //Varias coordenadas de textura (se puede guardar cualquier cosa ahi adentro)
            public Vector2 texcoord0;

            public Vector2 texcoord1;
            public Vector2 texcoord2;

            //Varios Vector3 auxiliares para guardar cualquier cosa
            public Vector3 auxValue1;

            public Vector3 auxValue2;

            public MyCustomVertex(Vector3 pos, Vector3 normal, int color)
            {
                Position = pos;
                Normal = normal;
                Color = color;

                //Los demas valores los llenamos con cualquier cosa porque para este ejemplo no se usan para nada
                texcoord0 = new Vector2(0, 1);
                texcoord1 = new Vector2(1, 1);
                texcoord2 = new Vector2(0.5f, 0.75f);
                auxValue1 = new Vector3(10, 10, 10);
                auxValue2 = new Vector3(0, 0, 5);
            }
        }
    }
}