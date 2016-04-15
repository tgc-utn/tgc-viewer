using Microsoft.DirectX;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Util;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo EjemploPlaneWall.
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como utilizar la herramienta TgcPlaneWall para crear
    ///     paredes planas con textura.
    ///     Permite editar su posición, tamaño, textura y mapeo de textura.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPlaneWall : TgcExample
    {
        private TgcTexture currentTexture;
        private TgcPlaneWall wall;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "PlaneWall";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar la herramienta TgcPlaneWall para crear paredes planas con textura." +
                   "Permite editar su posición, tamaño, textura y mapeo de textura. Movimiento con mouse.";
        }

        public override void init()
        {
            //Modifiers para variar parámetros de la pared
            GuiController.Instance.Modifiers.addVertex3f("origin", new Vector3(-100, -100, -100),
                new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addVertex3f("dimension", new Vector3(-100, -100, -100),
                new Vector3(1000, 1000, 100), new Vector3(100, 100, 100));
            GuiController.Instance.Modifiers.addInterval("orientation", new[] { "XY", "XZ", "YZ" }, 0);
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0, 0), new Vector2(10, 10),
                new Vector2(1, 1));
            GuiController.Instance.Modifiers.addBoolean("autoAdjust", "autoAdjust", false);

            //Modifier de textura
            var texturePath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);
            GuiController.Instance.Modifiers.addTexture("texture", currentTexture.FilePath);

            //Crear pared
            wall = new TgcPlaneWall();
            wall.setTexture(currentTexture);

            //Actualizar según valores cargados
            updateWall();
        }

        /// <summary>
        ///     Actualizar parámetros de la pared según los valores cargados
        /// </summary>
        private void updateWall()
        {
            //Origen, dimensiones, tiling y AutoAdjust
            var origin = (Vector3)GuiController.Instance.Modifiers["origin"];
            var dimension = (Vector3)GuiController.Instance.Modifiers["dimension"];
            var tiling = (Vector2)GuiController.Instance.Modifiers["tiling"];
            var autoAdjust = (bool)GuiController.Instance.Modifiers["autoAdjust"];

            //Cambiar orienación
            var orientation = (string)GuiController.Instance.Modifiers["orientation"];
            TgcPlaneWall.Orientations or;
            if (orientation == "XY") or = TgcPlaneWall.Orientations.XYplane;
            else if (orientation == "XZ") or = TgcPlaneWall.Orientations.XZplane;
            else or = TgcPlaneWall.Orientations.YZplane;

            //Cambiar textura
            var text = (string)GuiController.Instance.Modifiers["texture"];
            if (text != currentTexture.FilePath)
            {
                currentTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, text);
                wall.setTexture(currentTexture);
            }

            //Aplicar valores en pared
            wall.Origin = origin;
            wall.Size = dimension;
            wall.Orientation = or;
            wall.AutoAdjustUv = autoAdjust;
            wall.UTile = tiling.X;
            wall.VTile = tiling.Y;

            //Es necesario ejecutar updateValues() para que los cambios tomen efecto
            wall.updateValues();

            //Ajustar camara segun tamaño de la pared
            GuiController.Instance.RotCamera.targetObject(wall.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            //Actualizar valrores de pared
            updateWall();

            //Renderizar pared
            wall.render();
        }

        public override void close()
        {
            wall.dispose();
        }
    }
}