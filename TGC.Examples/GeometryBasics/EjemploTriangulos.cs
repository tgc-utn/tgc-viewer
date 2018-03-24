using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloBasico:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh - VertexBuffer.
    ///     # Unidad 4 - Texturas e Iluminacion - Coordenadas de Textura.
    ///     Muestra como crear un triangulo 3D de la forma mas sencilla,
    ///     especificando vertice por vertice.
    ///     El triangulo se crea con colores por vertice.
    ///     Crea un triangulo 3D con textura y colores por vertice.
    ///     Posee Modifiers para variar las posiciones de los vertices,
    ///     las coordenadas UV y los colores de cada vertice.
    ///     El triangulo se vuelve a armar en cada loop de render en base
    ///     a los parametros configurados en los Modifiers.
    ///     Crea el mismo triangulo que el ejemplo TrianguloBasico.
    ///     Pero utiliza la herramienta de VertexBuffer para crearlo en forma mas optima.
    ///     En lugar de mandar a renderizar cada primitiva por separado, se envia
    ///     el VertexBuffer entero.
    ///     Autor: Matias Leone, Leandro Barbagallo, Rodrigo Garcia
    /// </summary>
    public class EjemploTriangulos : TGCExampleViewer
    {
        private Texture texture;
        private TGCVertex3fModifier vertex1Modifier;
        private TGCVertex2fModifier texCoord1Modifier;
        private TGCColorModifier color1Modifier;
        private TGCVertex3fModifier vertex2Modifier;
        private TGCVertex2fModifier texCoord2Modifier;
        private TGCColorModifier color2Modifier;
        private TGCVertex3fModifier vertex3Modifier;
        private TGCVertex2fModifier texCoord3Modifier;
        private TGCColorModifier color3Modifier;
        private TGCFloatModifier rotationModifier;
        private TGCBooleanModifier textureEnableModifier;
        private TGCTextureModifier textureImageModifier;

        //Para 2. Triangulo editable.
        private string currentTexurePah;

        //Para 1. Array de vertices para crear el triangulo
        private CustomVertex.PositionColored[] simpleTriangleData;

        //Para 3. Vertex buffer que se va a utilizar
        private VertexBuffer vertexBuffer;

        public EjemploTriangulos(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Triangulos";
            Description = "Crea tres triangulos 3D, 1. basico con color. 2. acepta modificaciones de atributos basicos. 3. Utilizando Vertex Buffer";
        }

        public override void Init()
        {
            //Triangulo 1.
            //Definir array de vertices para el triangulo, del tipo Coordendas (X,Y,Z) + Color
            simpleTriangleData = new CustomVertex.PositionColored[3];

            //Cargar informacion de vertices. Nesitamos 3 vertices para crear un triangulo
            simpleTriangleData[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            simpleTriangleData[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            simpleTriangleData[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Cargar variables de usuario con alguna informacion util para ver en pantalla
            UserVars.addVar("Triangle 1 vertices", simpleTriangleData.Length);

            //Triangulo 2.
            //Current texture
            currentTexurePah = MediaDir + "Texturas\\baldosaFacultad.jpg";
            texture = TextureLoader.FromFile(D3DDevice.Instance.Device, currentTexurePah);

            //Modifiers
            vertex1Modifier = AddVertex3f("vertex1", new TGCVector3(-3, -3, -3), new TGCVector3(3, 3, 3), new TGCVector3(-1, 0, 0));
            texCoord1Modifier = AddVertex2f("texCoord1", TGCVector2.Zero, TGCVector2.One, new TGCVector2(1, 0));
            color1Modifier = AddColor("color1", Color.White);

            vertex2Modifier = AddVertex3f("vertex2", new TGCVector3(-3, -3, -3), new TGCVector3(3, 3, 3), new TGCVector3(1, 0, 0));
            texCoord2Modifier = AddVertex2f("texCoord2", TGCVector2.Zero, TGCVector2.One, new TGCVector2(0, 1));
            color2Modifier = AddColor("color2", Color.White);

            vertex3Modifier = AddVertex3f("vertex3", new TGCVector3(-3, -3, -3), new TGCVector3(3, 3, 3), TGCVector3.Up);
            texCoord3Modifier = AddVertex2f("texCoord3", TGCVector2.Zero, TGCVector2.One, TGCVector2.One);
            color3Modifier = AddColor("color3", Color.White);

            rotationModifier = AddFloat("rotation", -2, 2f, 0f);
            textureEnableModifier = AddBoolean("TextureEnable", "Con textura", true);
            textureImageModifier = AddTexture("Texture image", currentTexurePah);

            //Triangulo 3.
            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            var data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Almacenar informacion en VertexBuffer
            vertexBuffer.SetData(data, 0, LockFlags.None);

            //User Vars
            UserVars.addVar("Triangle 3 vertices");
            UserVars.setValue("Triangle 3 vertices", data.Length);

            //Configurar camara en rotacion
            Camara = new TgcRotationalCamera(new TGCVector3(0, 0.5f, 0), 7.5f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Triangulo 1 se corre para la izquierda.
            //Como tenemos un triangulo con textura y estamos haciendo una Draw primitive tenemos el problema de bingind de texturuas.
            //D3DDevice.Instance.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, true);
            //D3DDevice.Instance.Device.SetTextureStageState(0, TextureStageStates.AlphaOperation, true);
            //FIX IT!!!!!
            //al realizar el clean nos aseguramos que este triangulo no tendra la textura que tiene el otro.

            //Especificar formato de triangulo
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Translation(-2.5f, 0, 0).ToMatrix();
            //Dibujar 1 primitiva (nuestro triangulo)
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, simpleTriangleData);

            //Triangulo 2 centro.
            //Ver si cambio la textura
            var selectedTexture = textureImageModifier.Value;
            if (currentTexurePah != selectedTexture)
            {
                currentTexurePah = selectedTexture;
                texture = TextureLoader.FromFile(D3DDevice.Instance.Device, currentTexurePah);
            }

            //Crear triangulo segun datos del usuario
            var data = new CustomVertex.PositionColoredTextured[3];

            //vertice 1
            var v1 = vertex1Modifier.Value;
            var t1 = texCoord1Modifier.Value;
            data[0] = new CustomVertex.PositionColoredTextured(v1.X, v1.Y, v1.Z, color1Modifier.Value.ToArgb(), t1.X, t1.Y);

            //vertice 2
            var v2 = vertex2Modifier.Value;
            var t2 = texCoord2Modifier.Value;
            data[1] = new CustomVertex.PositionColoredTextured(v2.X, v2.Y, v2.Z, color2Modifier.Value.ToArgb(), t2.X, t2.Y);

            //vertice 3
            var v3 = vertex3Modifier.Value;
            var t3 = texCoord3Modifier.Value;
            data[2] = new CustomVertex.PositionColoredTextured(v3.X, v3.Y, v3.Z, color3Modifier.Value.ToArgb(), t3.X, t3.Y);

            //Rotacion
            var rotation = rotationModifier.Value;
            D3DDevice.Instance.Device.Transform.World = (TGCMatrix.Identity * TGCMatrix.RotationY(rotation)).ToMatrix();

            //Habilitar textura
            var textureEnable = textureEnableModifier.Value;
            if (textureEnable)
            {
                D3DDevice.Instance.Device.SetTexture(0, texture);
            }
            else
            {
                D3DDevice.Instance.Device.SetTexture(0, null);
            }

            //Render triangulo
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);

            //Triangulo 3 se corre a la derecha.
            //Especificar formato de triangulos
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionColored.Format;
            //Cargar VertexBuffer a renderizar
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            D3DDevice.Instance.Device.Transform.World = TGCMatrix.Translation(2.5f, 0, 0).ToMatrix();
            //Dibujar 1 primitiva
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

            PostRender();
        }

        public override void Dispose()
        {
            texture.Dispose();
            //liberar VertexBuffer
            vertexBuffer.Dispose();
        }
    }
}