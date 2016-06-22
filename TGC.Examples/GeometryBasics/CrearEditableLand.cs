using Microsoft.DirectX;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.GeometryBasics
{
    /// <summary>
    ///     Ejemplo CrearEditableLand
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como utilizar la utilidad TgcEditableLand para crear una grilla de terreno editable de 4x4 poligonos
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class CrearEditableLand : TgcExample
    {
        private TgcEditableLand land;

        public CrearEditableLand(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "GeometryBasics";
            Name = "EditableLand";
            Description =
                "Muestra como utilizar la utilidad TgcEditableLand para crear una grilla de terreno editable de 4x4 poligonos. Movimiento con mouse.";
        }

        public override void Init()
        {
            //Crear Land
            land = new TgcEditableLand();
            land.setTexture(
                TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Vegetacion\\blackrock_3.jpg"));

            //Modifiers para configurar altura
            Modifiers.addInterval("vertices",
                new[] { "CENTER", "INTERIOR_RING", "EXTERIOR_RING", "TOP_SIDE", "LEFT_SIDE", "RIGHT_SIDE", "BOTTOM_SIDE" },
                0);
            Modifiers.addFloat("height", -50, 50, 0);
            Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f), new Vector2(0, 0));
            Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4), new Vector2(1, 1));

            Camara = new TgcRotationalCamera(new Vector3(40f, 40f, 40f), 150f);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Configurar altura de los vertices seleccionados
            var selectedVertices = (string)Modifiers["vertices"];
            var height = (float)Modifiers["height"];
            if (selectedVertices == "CENTER")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_CENTER, height);
            }
            else if (selectedVertices == "INTERIOR_RING")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_INTERIOR_RING, height);
            }
            else if (selectedVertices == "EXTERIOR_RING")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_EXTERIOR_RING, height);
            }
            else if (selectedVertices == "TOP_SIDE")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_TOP_SIDE, height);
            }
            else if (selectedVertices == "LEFT_SIDE")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_LEFT_SIDE, height);
            }
            else if (selectedVertices == "RIGHT_SIDE")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_RIGHT_SIDE, height);
            }
            else if (selectedVertices == "BOTTOM_SIDE")
            {
                land.setVerticesY(TgcEditableLand.SELECTION_BOTTOM_SIDE, height);
            }

            //Offset y Tiling de textura
            land.UVOffset = (Vector2)Modifiers["offset"];
            land.UVTiling = (Vector2)Modifiers["tiling"];

            //Actualizar valores
            land.updateValues();

            //Dibujar
            land.render();

            PostRender();
        }

        public override void Dispose()
        {
            land.dispose();
        }
    }
}