using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplos con cajas
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    ///     Muestra como crear una caja 3D con la herramienta TgcBox, cuyos parametros
    ///     pueden ser modificados.
    ///     Muestra como crear una caja 3D WireFrame, en la cual solo se ven sus aristas, pero no sus caras.
    ///     Se utiliza la herramienta TgcDebugBox.
    ///     Cada arista es un Box rectangular.
    ///     Es util para hacer debug de ciertas estructuras.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploCajas : TGCExampleViewer
    {
        private TGCBox box;
        private string currentTexture;
        private TgcBoxDebug debugBox;

        public EjemploCajas(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Geometry Basics";
            Name = "Cajas";
            Description =
                "Muestra como crear una caja 3D con la herramienta TgcBox y TgcDebugBox, cuyos parametros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear caja vacia
            box = new TGCBox();
            box.AutoTransformEnable = true;
            //Crear caja debug vacia
            debugBox = new TgcBoxDebug();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            Modifiers.addBoolean("box", "box", true);
            Modifiers.addBoolean("boundingBox", "BoundingBox", false);
            Modifiers.addBoolean("debugBox", "debugBox", true);
            Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            Modifiers.addTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            Modifiers.addVertex2f("offset", new TGCVector2(-0.5f, -0.5f), new TGCVector2(0.9f, 0.9f), TGCVector2.Zero);
            Modifiers.addVertex2f("tiling", new TGCVector2(0.1f, 0.1f), new TGCVector2(4, 4), TGCVector2.One);
            Modifiers.addColor("color", Color.BurlyWood);
            Modifiers.addVertex3f("size", TGCVector3.Empty, new TGCVector3(100, 100, 100), new TGCVector3(20, 20, 20));
            Modifiers.addVertex3f("position", new TGCVector3(-100, -100, -100), new TGCVector3(100, 100, 100),
                TGCVector3.Empty);
            Modifiers.addVertex3f("rotation", new TGCVector3(-180, -180, -180), new TGCVector3(180, 180, 180),
                TGCVector3.Empty);

            Camara = new TgcRotationalCamera(new TGCVector3(), 200f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            //Actualizar parametros de la caja
            updateBox();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            //Cambiar textura
            var texturePath = (string)Modifiers["texture"];
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                box.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
            }

            var size = (TGCVector3)Modifiers["size"];
            var position = (TGCVector3)Modifiers["position"];
            var thickness = (float)Modifiers["thickness"];
            var color = (Color)Modifiers["color"];

            //Tamano, posicion y color
            box.Size = size;
            box.Position = position + new TGCVector3(15f, 0, 0);
            box.Color = color;

            //Actualizar valores en la caja.
            debugBox.setPositionSize(position - new TGCVector3(15f, 0, 0), size);
            debugBox.Thickness = thickness;
            debugBox.Color = color;

            //Rotacion, converitr a radianes
            var rotaion = (TGCVector3)Modifiers["rotation"];
            box.Rotation = new TGCVector3(Geometry.DegreeToRadian(rotaion.X), Geometry.DegreeToRadian(rotaion.Y),
                Geometry.DegreeToRadian(rotaion.Z));

            //Offset y Tiling de textura
            box.UVOffset = (TGCVector2)Modifiers["offset"];
            box.UVTiling = (TGCVector2)Modifiers["tiling"];

            //Actualizar valores en la caja. IMPORTANTE, es mejor realizar transformaciones con matrices.
            debugBox.updateValues();
            //Actualizar valores en la caja. IMPORTANTE, es mejor realizar transformaciones con matrices.
            //Otra cosa importante, las rotaciones no modifican el AABB. con lo cual hay que tener cuidado.
            box.updateValues();
        }

        public override void Render()
        {
            PreRender();

            //Renderizar caja
            if ((bool)Modifiers["box"])
            {
                box.Render();
            }

            //Mostrar BoundingBox de la caja
            if ((bool)Modifiers["boundingBox"])
            {
                box.BoundingBox.Render();
            }

            if ((bool)Modifiers["debugBox"])
            {
                debugBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            box.Dispose();
        }
    }
}