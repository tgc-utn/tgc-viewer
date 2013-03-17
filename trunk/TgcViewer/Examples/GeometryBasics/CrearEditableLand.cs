using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.GeometryBasics
{
    /// <summary>
    /// Ejemplo CrearEditableLand
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como utilizar la utilidad TgcEditableLand para crear una grilla de terreno editable de 4x4 poligonos
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CrearEditableLand : TgcExample
    {
        TgcEditableLand land;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "EditableLand";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar la utilidad TgcEditableLand para crear una grilla de terreno editable de 4x4 poligonos. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Land
            land = new TgcEditableLand();
            land.setTexture(TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Textures\\Vegetacion\\blackrock_3.jpg"));

            //Modifiers para configurar altura
            GuiController.Instance.Modifiers.addInterval("vertices", new string[] { "CENTER", "INTERIOR_RING", "EXTERIOR_RING", "TOP_SIDE", "LEFT_SIDE", "RIGHT_SIDE", "BOTTOM_SIDE" }, 0);
            GuiController.Instance.Modifiers.addFloat("height", -50, 50, 0);
            GuiController.Instance.Modifiers.addVertex2f("offset", new Vector2(-0.5f, -0.5f), new Vector2(0.9f, 0.9f), new Vector2(0, 0));
            GuiController.Instance.Modifiers.addVertex2f("tiling", new Vector2(0.1f, 0.1f), new Vector2(4, 4), new Vector2(1, 1));

            GuiController.Instance.RotCamera.targetObject(land.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Configurar altura de los vertices seleccionados
            string selectedVertices = (string)GuiController.Instance.Modifiers["vertices"];
            float height = (float)GuiController.Instance.Modifiers["height"];
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
            land.UVOffset = (Vector2)GuiController.Instance.Modifiers["offset"];
            land.UVTiling = (Vector2)GuiController.Instance.Modifiers["tiling"];

            //Actualizar valores
            land.updateValues();

            //Dibujar
            land.render();
        }

        public override void close()
        {
            land.dispose();
        }

    }
}
