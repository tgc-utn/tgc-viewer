using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Viewer;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo Caja
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como crear una esfera 3D con la herramienta TgcSphere, cuyos parámetros
    ///     pueden ser modificados.
    ///     Autor: Daniela Kazarian
    /// </summary>
    public class Esfera : TgcExample
    {
        private string currentTexture;
        private TgcSphere sphere;
        private bool useTexture;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Crear Esfera 3D";
        }

        public override string getDescription()
        {
            return "Muestra como crear una  esfera 3D con la herramienta TgcSphere, cuyos parámetros " +
                   "pueden ser modificados. Movimiento con mouse.";
        }

        public override void init()
        {
            //Crear esfera
            sphere = new TgcSphere();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            GuiController.Instance.Modifiers.addEnum("base", typeof(TgcSphere.eBasePoly),
                TgcSphere.eBasePoly.ICOSAHEDRON);
            GuiController.Instance.Modifiers.addBoolean("inflate", "yes", true);
            GuiController.Instance.Modifiers.addInterval("level of detail", new object[] { 0, 1, 2, 3, 4 }, 2);
            GuiController.Instance.Modifiers.addBoolean("edges", "show", false);
            GuiController.Instance.Modifiers.addFloat("radius", 0, 100, 10);
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-100, -100, -100),
                new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("rotation", new Vector3(-180, -180, -180),
                new Vector3(180, 180, 180), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addBoolean("Use texture", "yes", true);
            GuiController.Instance.Modifiers.addTexture("texture",
                GuiController.Instance.ExamplesMediaDir + "\\Texturas\\madera.jpg");
            GuiController.Instance.Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f),
                new Vector2(0, 0));
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4),
                new Vector2(1, 1));

            GuiController.Instance.Modifiers.addColor("color", Color.White);
            GuiController.Instance.Modifiers.addBoolean("boundingsphere", "show", false);

            GuiController.Instance.UserVars.addVar("Vertices");
            GuiController.Instance.UserVars.addVar("Triangulos");

            GuiController.Instance.RotCamera.CameraDistance = 50;
        }

        /// <summary>
        ///     Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateSphere()
        {
            var bTexture = (bool)GuiController.Instance.Modifiers["Use texture"];
            var color = (Color)GuiController.Instance.Modifiers["color"];
            sphere.RenderEdges = (bool)GuiController.Instance.Modifiers["edges"];
            sphere.Inflate = (bool)GuiController.Instance.Modifiers["inflate"];
            sphere.BasePoly = (TgcSphere.eBasePoly)GuiController.Instance.Modifiers.getValue("base");

            if (bTexture)
            {
                //Cambiar textura
                var texturePath = (string)GuiController.Instance.Modifiers["texture"];
                if (texturePath != currentTexture || !useTexture || (sphere.RenderEdges && sphere.Color != color))
                {
                    currentTexture = texturePath;
                    sphere.setColor(color);
                    sphere.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
                }
            }
            else sphere.setColor(color);

            useTexture = bTexture;

            //Radio, posición y color
            sphere.Radius = (float)GuiController.Instance.Modifiers["radius"];
            sphere.Position = (Vector3)GuiController.Instance.Modifiers["position"];
            sphere.LevelOfDetail = (int)GuiController.Instance.Modifiers["level of detail"];

            //Rotación, converitr a radianes
            var rotation = (Vector3)GuiController.Instance.Modifiers["rotation"];
            sphere.Rotation = new Vector3(Geometry.DegreeToRadian(rotation.X), Geometry.DegreeToRadian(rotation.Y),
                Geometry.DegreeToRadian(rotation.Z));

            //Offset de textura
            sphere.UVOffset = (Vector2)GuiController.Instance.Modifiers["offset"];

            //Tiling de textura
            sphere.UVTiling = (Vector2)GuiController.Instance.Modifiers["tiling"];

            //Actualizar valores en la caja.
            sphere.updateValues();
        }

        public override void render(float elapsedTime)
        {
            //Actualizar parametros de la caja
            updateSphere();

            GuiController.Instance.UserVars.setValue("Vertices", sphere.VertexCount);
            GuiController.Instance.UserVars.setValue("Triangulos", sphere.TriangleCount);
            //Renderizar caja
            sphere.render();

            //Mostrar Boundingsphere de la caja
            var boundingsphere = (bool)GuiController.Instance.Modifiers["boundingsphere"];
            if (boundingsphere)
            {
                sphere.BoundingSphere.render();
            }
        }

        public override void close()
        {
            sphere.dispose();
        }
    }
}