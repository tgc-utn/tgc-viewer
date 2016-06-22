using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo TrianguloEditable:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminación - Coordenadas de Textura
    ///     Crea un triángulo 3D con textura y colores por vértice.
    ///     Posee Modifiers para variar las posiciones de los vértices,
    ///     las coordenadas UV y los colores de cada vértice.
    ///     El triángulo se vuelve a armar en cada loop de render en base
    ///     a los parámetros configurados en los Modifiers.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TrianguloEditable : TgcExample
    {
        private string currentTexurePah;
        private Texture texture;

        public TrianguloEditable(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "Triangulo Editable";
            Description =
                "Muestra un triangulo 3D al que se le pueden modificar sus atributos básicos. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Configurar camara en rotacion
            Camara = new TgcRotationalCamera(new Vector3(0, 0.5f, 0), 3f);

            //Current texture
            currentTexurePah = MediaDir + "Texturas" + "\\" + "baldosaFacultad.jpg";
            loadTexture(D3DDevice.Instance.Device, currentTexurePah);

            //Modifiers
            Modifiers.addVertex3f("vertex1", new Vector3(-3, -3, -3), new Vector3(3, 3, 3),
                new Vector3(-1, 0, 0));
            Modifiers.addVertex2f("texCoord1", new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 0));
            Modifiers.addColor("color1", Color.White);

            Modifiers.addVertex3f("vertex2", new Vector3(-3, -3, -3), new Vector3(3, 3, 3),
                new Vector3(1, 0, 0));
            Modifiers.addVertex2f("texCoord2", new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 1));
            Modifiers.addColor("color2", Color.White);

            Modifiers.addVertex3f("vertex3", new Vector3(-3, -3, -3), new Vector3(3, 3, 3),
                new Vector3(0, 1, 0));
            Modifiers.addVertex2f("texCoord3", new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(1, 1));
            Modifiers.addColor("color3", Color.White);

            Modifiers.addFloat("rotation", -2, 2f, 0f);
            Modifiers.addBoolean("TextureEnable", "Con textura", true);
            Modifiers.addTexture("Texture image", currentTexurePah);
        }

        public override void Update()
        {
            PreUpdate();
        }

        private void loadTexture(Device d3dDevice, string path)
        {
            texture = TextureLoader.FromFile(d3dDevice, path);
        }

        public override void Render()
        {
            PreRender();

            //Ver si cambio la textura
            var selectedTexture = (string)Modifiers["Texture image"];
            if (currentTexurePah != selectedTexture)
            {
                currentTexurePah = selectedTexture;
                loadTexture(D3DDevice.Instance.Device, currentTexurePah);
            }

            //Crear triangulo segun datos del usuario
            var data = new CustomVertex.PositionColoredTextured[3];

            //vertice 1
            var v1 = (Vector3)Modifiers["vertex1"];
            var t1 = (Vector2)Modifiers["texCoord1"];
            data[0] = new CustomVertex.PositionColoredTextured(
                v1.X,
                v1.Y,
                v1.Z,
                ((Color)Modifiers["color1"]).ToArgb(),
                t1.X,
                t1.Y);

            //vertice 2
            var v2 = (Vector3)Modifiers["vertex2"];
            var t2 = (Vector2)Modifiers["texCoord2"];
            data[1] = new CustomVertex.PositionColoredTextured(
                v2.X,
                v2.Y,
                v2.Z,
                ((Color)Modifiers["color2"]).ToArgb(),
                t2.X,
                t2.Y);

            //vertice 3
            var v3 = (Vector3)Modifiers["vertex3"];
            var t3 = (Vector2)Modifiers["texCoord3"];
            data[2] = new CustomVertex.PositionColoredTextured(
                v3.X,
                v3.Y,
                v3.Z,
                ((Color)Modifiers["color3"]).ToArgb(),
                t3.X,
                t3.Y);

            //Rotacion
            var rotation = (float)Modifiers["rotation"];
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity * Matrix.RotationY(rotation);

            //Habilitar textura
            var textureEnable = (bool)Modifiers["TextureEnable"];
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

            PostRender();
        }

        public override void Dispose()
        {
            texture.Dispose();
        }
    }
}