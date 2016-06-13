using Microsoft.DirectX;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

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

        public EjemploPlaneWall(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "PlaneWall";
            Description =
                "Muestra como utilizar la herramienta TgcPlaneWall para crear paredes planas con textura. Permite editar su posición, tamaño, textura y mapeo de textura. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Modifiers para variar parámetros de la pared
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

            //Actualizar según valores cargados
            updateWall();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Actualizar parámetros de la pared según los valores cargados
        /// </summary>
        private void updateWall()
        {
            //Origen, dimensiones, tiling y AutoAdjust
            var origin = (Vector3)Modifiers["origin"];
            var dimension = (Vector3)Modifiers["dimension"];
            var tiling = (Vector2)Modifiers["tiling"];
            var autoAdjust = (bool)Modifiers["autoAdjust"];

            //Cambiar orienación
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

            //Ajustar camara segun tamaño de la pared
            Camara = new TgcRotationalCamera(wall.BoundingBox.calculateBoxCenter(), wall.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Actualizar valrores de pared
            updateWall();

            //Renderizar pared
            wall.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            wall.dispose();
        }
    }
}