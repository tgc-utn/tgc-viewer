using Microsoft.DirectX.Direct3D;
using System.Drawing;
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
        private TGCBooleanModifier boxModifier;
        private TGCBooleanModifier boundingBoxModifier;
        private TGCBooleanModifier debugBoxModifier;
        private TGCFloatModifier thicknessModifier;
        private TGCTextureModifier textureModifier;
        private TGCVertex2fModifier offsetModifier;
        private TGCVertex2fModifier tilingModifier;
        private TGCColorModifier colorModifier;
        private TGCVertex3fModifier sizeModifier;
        private TGCVertex3fModifier positionModifier;
        private TGCVertex3fModifier rotationModifier;

        private TGCBox box;
        private string currentTexture;
        private TgcBoxDebug debugBox;

        public EjemploCajas(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Geometry Basics";
            Name = "Cajas";
            Description = "Muestra como crear una caja 3D con la herramienta TgcBox y TgcDebugBox, cuyos parametros pueden ser modificados. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear caja vacia
            box = new TGCBox();
            box.AutoTransform = true;
            //Crear caja debug vacia
            debugBox = new TgcBoxDebug();
            currentTexture = null;

            //Modifiers para vararis sus parametros
            boxModifier = AddBoolean("box", "box", true);
            boundingBoxModifier = AddBoolean("boundingBox", "BoundingBox", false);
            debugBoxModifier = AddBoolean("debugBox", "debugBox", true);
            thicknessModifier = AddFloat("thickness", 0.1f, 5, 0.2f);
            textureModifier = AddTexture("texture", MediaDir + "\\Texturas\\madera.jpg");
            offsetModifier = AddVertex2f("offset", new TGCVector2(-0.5f, -0.5f), new TGCVector2(0.9f, 0.9f), TGCVector2.Zero);
            tilingModifier = AddVertex2f("tiling", new TGCVector2(0.1f, 0.1f), new TGCVector2(4, 4), TGCVector2.One);
            colorModifier = AddColor("color", Color.BurlyWood);
            sizeModifier = AddVertex3f("size", TGCVector3.Empty, new TGCVector3(100, 100, 100), new TGCVector3(20, 20, 20));
            positionModifier = AddVertex3f("position", new TGCVector3(-100, -100, -100), new TGCVector3(100, 100, 100), TGCVector3.Empty);
            rotationModifier = AddVertex3f("rotation", new TGCVector3(-180, -180, -180), new TGCVector3(180, 180, 180), TGCVector3.Empty);

            Camara = new TgcRotationalCamera(TGCVector3.Empty, 200f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            //Actualizar parametros de la caja
            updateBox();

            PostUpdate();
        }

        /// <summary>
        ///     Actualiza los parametros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            //Cambiar textura
            var texturePath = textureModifier.Value;
            if (texturePath != currentTexture)
            {
                currentTexture = texturePath;
                box.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device, currentTexture));
            }

            var size = sizeModifier.Value;
            var position = positionModifier.Value;
            var thickness = thicknessModifier.Value;
            var color = colorModifier.Value;

            //Tamano, posicion y color
            box.Size = size;
            box.Position = position + new TGCVector3(15f, 0, 0);
            box.Color = color;

            //Actualizar valores en la caja.
            debugBox.setPositionSize(position - new TGCVector3(15f, 0, 0), size);
            debugBox.Thickness = thickness;
            debugBox.Color = color;

            //Rotacion, converitr a radianes
            var rotaion = rotationModifier.Value;
            box.Rotation = new TGCVector3(Geometry.DegreeToRadian(rotaion.X), Geometry.DegreeToRadian(rotaion.Y), Geometry.DegreeToRadian(rotaion.Z));

            //Offset y Tiling de textura
            box.UVOffset = offsetModifier.Value;
            box.UVTiling = tilingModifier.Value;

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
            if (boxModifier.Value)
            {
                box.Render();
            }

            //Mostrar BoundingBox de la caja
            if (boundingBoxModifier.Value)
            {
                box.BoundingBox.Render();
            }

            if (debugBoxModifier.Value)
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