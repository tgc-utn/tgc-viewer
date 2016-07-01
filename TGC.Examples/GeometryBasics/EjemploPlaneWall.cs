using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo EjemploPlaneWall.
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como utilizar la herramienta TgcPlaneWall para crear
    ///     paredes planas con textura.
    ///     Permite editar su posicion, tamano, textura y mapeo de textura.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploPlaneWall : TGCExampleViewer
    {
        private TgcTexture currentTexture;
        private TgcPlaneWall wall;

        public EjemploPlaneWall(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "GeometryBasics";
            Name = "PlaneWall";
            Description =
                "Muestra como utilizar la herramienta TgcPlaneWall para crear paredes planas con textura. Permite editar su posicion, tamano, textura y mapeo de textura. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Modifiers para variar parametros de la pared
            Modifiers.addVertex3f("origin", new Vector3(-100, -100, -100), new Vector3(100, 100, 100),
                new Vector3(0, 0, 0));
            Modifiers.addVertex3f("dimension", new Vector3(-100, -100, -100), new Vector3(1000, 1000, 100),
                new Vector3(100, 100, 100));
            Modifiers.addInterval("orientation", new[] { "XY", "XZ", "YZ" }, 0);
            Modifiers.addVertex2f("tiling", new Vector2(0, 0), new Vector2(10, 10), new Vector2(1, 1));
            Modifiers.addBoolean("autoAdjust", "autoAdjust", false);

            //Modifier de textura
            var texturePath = MediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);
            Modifiers.addTexture("texture", currentTexture.FilePath);

            //Crear pared
            wall = new TgcPlaneWall();
            wall.setTexture(currentTexture);

            //Actualizar segun valores cargados
            updateWall();
        }

        public override void Update()
        {
            PreUpdate();
        }

        /// <summary>
        ///     Actualizar parametros de la pared segun los valores cargados
        /// </summary>
        private void updateWall()
        {
            //Origen, dimensiones, tiling y AutoAdjust
            var origin = (Vector3)Modifiers["origin"];
            var dimension = (Vector3)Modifiers["dimension"];
            var tiling = (Vector2)Modifiers["tiling"];
            var autoAdjust = (bool)Modifiers["autoAdjust"];

            //Cambiar orienacion
            var orientation = (string)Modifiers["orientation"];
            TgcPlaneWall.Orientations or;
            if (orientation == "XY") or = TgcPlaneWall.Orientations.XYplane;
            else if (orientation == "XZ") or = TgcPlaneWall.Orientations.XZplane;
            else or = TgcPlaneWall.Orientations.YZplane;

            //Cambiar textura
            var text = (string)Modifiers["texture"];
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

            //Ajustar camara segun tamano de la pared
            Camara = new TgcRotationalCamera(wall.BoundingBox.calculateBoxCenter(),
                wall.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Render()
        {
            PreRender();

            //Actualizar valrores de pared
            updateWall();

            //Renderizar pared
            wall.render();

            PostRender();
        }

        public override void Dispose()
        {
            wall.dispose();
        }
    }
}