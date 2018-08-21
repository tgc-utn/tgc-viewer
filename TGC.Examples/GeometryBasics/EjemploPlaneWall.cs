using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo EjemploTGCPlane.
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como utilizar la herramienta TGCPlane para crear
    ///     paredes planas con textura.
    ///     Permite editar su posicion, tamano, textura y mapeo de textura.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploTGCPlane : TGCExampleViewer
    {
        private TGCTextureModifier textureModifier;
        private TGCVertex3fModifier originModifier;
        private TGCVertex3fModifier dimensionModifier;
        private TGCIntervalModifier orientationModifier;
        private TGCVertex2fModifier tilingModifier;
        private TGCBooleanModifier autoAdjustModifier;

        private TgcTexture currentTexture;
        private TgcPlane plane;

        public EjemploTGCPlane(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Plane";
            Description = "Muestra como utilizar la herramienta TgcTGCPlane para crear paredes planas con textura. Permite editar su posicion, tamano, textura y mapeo de textura. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Modifiers para variar parametros de la pared
            originModifier = AddVertex3f("origin", new TGCVector3(-100, -100, -100), new TGCVector3(100, 100, 100), TGCVector3.Empty);
            dimensionModifier = AddVertex3f("dimension", new TGCVector3(-100, -100, -100), new TGCVector3(1000, 1000, 100), new TGCVector3(100, 100, 100));
            orientationModifier = AddInterval("orientation", new[] { "XY", "XZ", "YZ" }, 0);
            tilingModifier = AddVertex2f("tiling", TGCVector2.Zero, new TGCVector2(10, 10), TGCVector2.One);
            autoAdjustModifier = AddBoolean("autoAdjust", "autoAdjust", false);

            //Modifier de textura
            var texturePath = MediaDir + "Texturas\\Quake\\TexturePack2\\brick1_1.jpg";
            currentTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);
            textureModifier = AddTexture("texture", currentTexture.FilePath);

            //Crear pared
            plane = new TgcPlane();
            plane.setTexture(currentTexture);

            //Actualizar segun valores cargados
            updateTGCPlane();

            //Ajustar camara segun tamano de la pared
            Camara = new TgcRotationalCamera(plane.BoundingBox.calculateBoxCenter(), plane.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        /// <summary>
        ///     Actualizar parametros de la pared segun los valores cargados
        /// </summary>
        private void updateTGCPlane()
        {
            //Origen, dimensiones, tiling y AutoAdjust
            var origin = originModifier.Value;
            var dimension = dimensionModifier.Value;
            var tiling = tilingModifier.Value;
            var autoAdjust = autoAdjustModifier.Value;

            //Cambiar orienacion
            var orientation = orientationModifier.Value;
            TgcPlane.Orientations or;
            if (orientation == "XY") or = TgcPlane.Orientations.XYplane;
            else if (orientation == "XZ") or = TgcPlane.Orientations.XZplane;
            else or = TgcPlane.Orientations.YZplane;

            //Cambiar textura
            var text = textureModifier.Value;
            if (text != currentTexture.FilePath)
            {
                currentTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, text);
                plane.setTexture(currentTexture);
            }

            //Aplicar valores en pared
            plane.Origin = origin;
            plane.Size = dimension;
            plane.Orientation = or;
            plane.AutoAdjustUv = autoAdjust;
            plane.UTile = tiling.X;
            plane.VTile = tiling.Y;

            //Es necesario ejecutar updateValues() para que los cambios tomen efecto
            plane.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Actualizar valrores de pared
            updateTGCPlane();

            //Renderizar pared
            plane.Render();

            PostRender();
        }

        public override void Dispose()
        {
            plane.Dispose();
        }
    }
}