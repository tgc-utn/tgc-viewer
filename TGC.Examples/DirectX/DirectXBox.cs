using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.DirectX
{
    /// <summary>
    ///     Ejemplo DirectXBox:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - VertexBuffer
    ///     Muestra como crear una caja 3D usando DirectX a secas, sin utilizar nada del framework.
    ///     El objetivo del ejemplo es explicar como utilizar la API de DirectX directamente para poder hacer cosas
    ///     personalizadas.
    ///     Si lo que se desea es dibujar una caja 3D de forma rapida utilizar TgcBox.
    ///     Muestra como usar un vertex buffer y un index buffer.
    ///     El formato de vertice utilizado para la caja es 100% customizado.
    ///     Luego se usa un shader customizado para mostrar como interactuar con estos atributos de vertice personalizados.
    ///     El shader en si no tiene ninguna finalidad especial, solo es para mostrar como agregar nuevos atributos a un
    ///     vertice.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class DirectXBox : TGCExampleViewer
    {
        /// <summary>
        ///     Vertex declaration para el vertice customizado. Hay que tener cuidado con la suma de bytes
        ///     1 float = 4 bytes
        ///     TGCVector2 = 8 bytes
        ///     TGCVector3 = 12 bytes
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

        private float acumTime;
        private float dir;
        private Effect effect;
        private IndexBuffer indexBuffer;
        private int indexCount;

        private Texture texture0;
        private Texture texture1;
        private Texture texture2;
        private int triangleCount;
        private VertexBuffer vertexBuffer;
        private int vertexCount;
        private MyCustomVertex[] vertexData;
        private VertexDeclaration vertexDeclaration;

        public DirectXBox(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "DirectX";
            Name = "DirectX Box";
            Description = "Muestra como crear una caja 3D usando DirectX a secas, sin utilizar nada del framework.";
        }

        public override void Init()
        {
            //Dimensiones de la caja
            var center = TGCVector3.Empty;
            var size = new TGCVector3(10, 10, 10);
            var color1 = Color.Red.ToArgb();
            var color2 = Color.Green.ToArgb();
            var extents = size * 0.5f;
            var min = center - extents;
            var max = center + extents;

            //Crear vertex declaration
            vertexDeclaration = new VertexDeclaration(D3DDevice.Instance.Device, MyCustomVertexElements);

            //Crear un VertexBuffer con 8 vertices (los 8 extremos de la caja)
            vertexCount = 8;
            vertexData = new MyCustomVertex[vertexCount];
            vertexBuffer = new VertexBuffer(typeof(MyCustomVertex), vertexCount, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, MyCustomVertexFormat, Pool.Default);

            //Llenar array con los 8 vertices de la caja
            //La normal se carga combinando la direccion de las 3 caras que toca
            //inferiores (son las 4 combinaciones de min-max entre XZ)
            vertexData[0] = new MyCustomVertex(new TGCVector3(min.X, min.Y, min.Z),
                TGCVector3.Normalize(new TGCVector3(-1, -1, -1)), color1);
            vertexData[1] = new MyCustomVertex(new TGCVector3(max.X, min.Y, min.Z),
                TGCVector3.Normalize(new TGCVector3(1, -1, -1)), color1);
            vertexData[2] = new MyCustomVertex(new TGCVector3(min.X, min.Y, max.Z),
                TGCVector3.Normalize(new TGCVector3(-1, -1, 1)), color1);
            vertexData[3] = new MyCustomVertex(new TGCVector3(max.X, min.Y, max.Z),
                TGCVector3.Normalize(new TGCVector3(1, -1, 1)), color1);

            //superiores (idem inferiores pero con max.Y)
            vertexData[4] = new MyCustomVertex(new TGCVector3(min.X, max.Y, min.Z),
                TGCVector3.Normalize(new TGCVector3(-1, 1, -1)), color2);
            vertexData[5] = new MyCustomVertex(new TGCVector3(max.X, max.Y, min.Z),
                TGCVector3.Normalize(new TGCVector3(1, 1, -1)), color2);
            vertexData[6] = new MyCustomVertex(new TGCVector3(min.X, max.Y, max.Z),
                TGCVector3.Normalize(new TGCVector3(-1, 1, 1)), color2);
            vertexData[7] = new MyCustomVertex(new TGCVector3(max.X, max.Y, max.Z), TGCVector3.Normalize(TGCVector3.One),
                color2);

            //Setear informacion en VertexBuffer
            vertexBuffer.SetData(vertexData, 0, LockFlags.None);

            //Crear IndexBuffer con 36 vertices para los 12 triangulos que forman la caja (2 triangulos por cada => 6 vertices por cara => 6 caras)
            indexCount = 36;
            triangleCount = indexCount / 3;
            var indexData = new short[indexCount];
            var iIdx = 0;
            indexBuffer = new IndexBuffer(typeof(short), indexCount, D3DDevice.Instance.Device, Usage.None,
                Pool.Default);

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

            //Setear informacion en IndexBuffer
            indexBuffer.SetData(indexData, 0, LockFlags.None);

            //Cargar shader customizado para este ejemplo
            var shaderPath = ShadersDir + "EjemploBoxDirectX.fx";
            string compilationErrors;
            effect = Effect.FromFile(D3DDevice.Instance.Device, shaderPath, null, null, ShaderFlags.None, null,
                out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader: " + shaderPath + ". Errores: " + compilationErrors);
            }

            //Setear el unico technique que tiene
            effect.Technique = "EjemploBoxDirectX";

            //Cargamos 3 texturas cualquiera para mandar al shader
            texture0 = TextureLoader.FromFile(D3DDevice.Instance.Device, MediaDir + "Texturas\\BM_DiffuseMap_pared.jpg");
            texture1 = TextureLoader.FromFile(D3DDevice.Instance.Device, MediaDir + "Texturas\\BM_NormalMap.jpg");
            texture2 = TextureLoader.FromFile(D3DDevice.Instance.Device, MediaDir + "Texturas\\efecto_alarma.png");

            dir = 1;

            Camara = new TgcRotationalCamera(TGCVector3.Empty, 20f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            acumTime += ElapsedTime;
            var speed = 20 * ElapsedTime;
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
                vertexData[i].Position += new TGCVector3(0, dir * speed, 0);
            }
            vertexBuffer.SetData(vertexData, 0, LockFlags.None); //Manda la informacion actualizada a la GPU

            //Cargar vertex declaration
            D3DDevice.Instance.Device.VertexDeclaration = vertexDeclaration;

            //Cargar vertexBuffer e indexBuffer
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            D3DDevice.Instance.Device.Indices = indexBuffer;

            //Arrancar shader
            effect.Begin(0);
            effect.BeginPass(0);

            //Cargar matrices en shader
            var matWorldView = D3DDevice.Instance.Device.Transform.View;
            var matWorldViewProj = matWorldView * D3DDevice.Instance.Device.Transform.Projection;
            effect.SetValue("matWorld", TGCMatrix.Identity.ToMatrix());
            effect.SetValue("matWorldView", matWorldView);
            effect.SetValue("matWorldViewProj", matWorldViewProj);
            effect.SetValue("matInverseTransposeWorld", TGCMatrix.Identity.ToMatrix());

            //Cargar las 3 texturas en el shader
            effect.SetValue("tex0", texture0);
            effect.SetValue("tex1", texture1);
            effect.SetValue("tex2", texture2);

            //Dibujar los triangulos haciendo uso del indexBuffer
            D3DDevice.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexCount, 0,
                triangleCount);

            //Finalizar shader
            effect.EndPass();
            effect.End();

            PostRender();
        }

        public override void Dispose()
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
            public TGCVector3 Position;
            public TGCVector3 Normal;
            public int Color;

            //Varias coordenadas de textura (se puede guardar cualquier cosa ahi adentro)
            public TGCVector2 texcoord0;

            public TGCVector2 texcoord1;
            public TGCVector2 texcoord2;

            //Varios TGCVector3 auxiliares para guardar cualquier cosa
            public TGCVector3 auxValue1;

            public TGCVector3 auxValue2;

            public MyCustomVertex(TGCVector3 pos, TGCVector3 normal, int color)
            {
                Position = pos;
                Normal = normal;
                Color = color;

                //Los demas valores los llenamos con cualquier cosa porque para este ejemplo no se usan para nada
                texcoord0 = new TGCVector2(0, 1);
                texcoord1 = TGCVector2.One;
                texcoord2 = new TGCVector2(0.5f, 0.75f);
                auxValue1 = new TGCVector3(10, 10, 10);
                auxValue2 = new TGCVector3(0, 0, 5);
            }
        }
    }
}