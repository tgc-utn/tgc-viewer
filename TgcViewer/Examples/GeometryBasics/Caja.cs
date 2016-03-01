using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una caja 3D con la herramienta TgcBox, cuyos parámetros
    ///     pueden ser modificados.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class Caja : TgcExample
    {
        private TgcBox box;
        private string currentTexture;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Crear Caja 3D";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D con la herramienta TgcBox, cuyos parámetros " +
                   "pueden ser modificados. Movimiento con mouse.";
        }

        public override void init()
        {
            //Crear caja vacia
            box = new TgcBox();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            GuiController.Instance.Modifiers.addVertex3f("size", new Vector3(0, 0, 0), new Vector3(100, 100, 100),
                new Vector3(20, 20, 20));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-100, -100, -100),
                new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-180, -180, -180),
                new Vector3(180, 180, 180), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addTexture("texture",
                GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            GuiController.Instance.Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f),
                new Vector2(0, 0));
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4),
                new Vector2(1, 1));
            GuiController.Instance.Modifiers.addColor("color", Color.White);
            GuiController.Instance.Modifiers.addBoolean("boundingBox", "BoundingBox", false);

            GuiController.Instance.RotCamera.CameraDistance = 50;
        }

        /// <summary>
        ///     Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            //Cambiar textura
            var texturePath = (string)GuiController.Instance.Modifiers["texture"];
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                box.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
            }

            //Tamaño, posición y color
            box.Size = (Vector3)GuiController.Instance.Modifiers["size"];
            box.Position = (Vector3)GuiController.Instance.Modifiers["position"];
            box.Color = (Color)GuiController.Instance.Modifiers["color"];

            //Rotación, converitr a radianes
            var rotaion = (Vector3)GuiController.Instance.Modifiers["rotation"];
            box.Rotation = new Vector3(Geometry.DegreeToRadian(rotaion.X), Geometry.DegreeToRadian(rotaion.Y),
                Geometry.DegreeToRadian(rotaion.Z));

            //Offset y Tiling de textura
            box.UVOffset = (Vector2)GuiController.Instance.Modifiers["offset"];
            box.UVTiling = (Vector2)GuiController.Instance.Modifiers["tiling"];

            //Actualizar valores en la caja.
            box.updateValues();
        }

        public override void render(float elapsedTime)
        {
            //Actualizar parametros de la caja
            updateBox();

            //Renderizar caja
            box.render();

            //Mostrar BoundingBox de la caja
            var boundingBox = (bool)GuiController.Instance.Modifiers["boundingBox"];
            if (boundingBox)
            {
                box.BoundingBox.render();
            }
        }

        public override void close()
        {
            box.dispose();
        }
    }
}