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
    /// Ejemplo TrianguloEditable:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     # Unidad 4 - Texturas e Iluminación - Coordenadas de Textura
    /// 
    /// Crea un triángulo 3D con textura y colores por vértice.
    /// Posee Modifiers para variar las posiciones de los vértices,
    /// las coordenadas UV y los colores de cada vértice.
    /// El triángulo se vuelve a armar en cada loop de render en base
    /// a los parámetros configurados en los Modifiers.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class TrianguloEditable : TgcExample
    {

        string currentTexurePah;
        Texture texture;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Triangulo Editable";
        }

        public override string getDescription()
        {
            return "Muestra un triangulo 3D al que se le pueden modificar sus atributos básicos.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Configurar camara en rotacion
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0.5f, 0), 3f);

            //Current texture
            currentTexurePah = GuiController.Instance.ExamplesMediaDir + "Texturas" + "\\" + "baldosaFacultad.jpg";
            loadTexture(d3dDevice, currentTexurePah);

            //Modifiers
            GuiController.Instance.Modifiers.addVertex3f("vertex1", new Vector3(-3,-3,-3),new Vector3(3,3,3), new Vector3(-1,0,0));
            GuiController.Instance.Modifiers.addVertex2f("texCoord1", new Vector2(0,0), new Vector2(1,1), new Vector2(0,0));
            GuiController.Instance.Modifiers.addColor("color1", Color.White);

            GuiController.Instance.Modifiers.addVertex3f("vertex2", new Vector3(-3, -3, -3), new Vector3(3, 3, 3), new Vector3(1, 0, 0));
            GuiController.Instance.Modifiers.addVertex2f("texCoord2", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            GuiController.Instance.Modifiers.addColor("color2", Color.White);

            GuiController.Instance.Modifiers.addVertex3f("vertex3", new Vector3(-3, -3, -3), new Vector3(3, 3, 3), new Vector3(0, 1, 0));
            GuiController.Instance.Modifiers.addVertex2f("texCoord3", new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
            GuiController.Instance.Modifiers.addColor("color3", Color.White);

            GuiController.Instance.Modifiers.addFloat("rotation", -2, 2f, 0f);
            GuiController.Instance.Modifiers.addBoolean("TextureEnable", "Con textura", true);
            GuiController.Instance.Modifiers.addTexture("Texture image", currentTexurePah);
        }

        private void loadTexture(Device d3dDevice, string path)
        {
            texture = TextureLoader.FromFile(d3dDevice, path);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio la textura
            string selectedTexture = (string)GuiController.Instance.Modifiers["Texture image"];
            if (currentTexurePah != selectedTexture)
            {
                currentTexurePah = selectedTexture;
                loadTexture(d3dDevice, currentTexurePah);
            }



            //Crear triangulo segun datos del usuario
            CustomVertex.PositionColoredTextured[] data = new CustomVertex.PositionColoredTextured[3];

            //vertice 1
            Vector3 v1 = (Vector3)GuiController.Instance.Modifiers["vertex1"];
            Vector2 t1 = (Vector2)GuiController.Instance.Modifiers["texCoord1"];
            data[0] = new CustomVertex.PositionColoredTextured(
                v1.X,
                v1.Y,
                v1.Z,
                ((Color)GuiController.Instance.Modifiers["color1"]).ToArgb(),
                t1.X,
                t1.Y);

            //vertice 2
            Vector3 v2 = (Vector3)GuiController.Instance.Modifiers["vertex2"];
            Vector2 t2 = (Vector2)GuiController.Instance.Modifiers["texCoord2"];
            data[1] = new CustomVertex.PositionColoredTextured(
                v2.X,
                v2.Y,
                v2.Z,
                ((Color)GuiController.Instance.Modifiers["color2"]).ToArgb(),
                t2.X,
                t2.Y);

            //vertice 3
            Vector3 v3 = (Vector3)GuiController.Instance.Modifiers["vertex3"];
            Vector2 t3 = (Vector2)GuiController.Instance.Modifiers["texCoord3"];
            data[2] = new CustomVertex.PositionColoredTextured(
                v3.X,
                v3.Y,
                v3.Z,
                ((Color)GuiController.Instance.Modifiers["color3"]).ToArgb(),
                t3.X,
                t3.Y);


            //Rotacion
            float rotation = (float)GuiController.Instance.Modifiers["rotation"];
            d3dDevice.Transform.World = Matrix.Identity * Matrix.RotationY(rotation);


            //Habilitar textura
            bool textureEnable = (bool)GuiController.Instance.Modifiers["TextureEnable"];
            if (textureEnable)
            {
                d3dDevice.SetTexture(0, texture);
            }
            else
            {
                d3dDevice.SetTexture(0, null);
            }
            
                

            //Render triangulo 
            d3dDevice.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 1, data);
        }

        public override void close()
        {
        }

    }
}
