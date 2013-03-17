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
    /// Ejemplo Caja
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    /// 
    /// Muestra como crear una cara rectanglar 3D (Quad) orientable en base a un vector normal,
    /// utilizando la herramienta TgcQuad.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CrearQuad : TgcExample
    {
        TgcQuad quad;
        TgcArrow normalArrow;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Crear Quad";
        }

        public override string getDescription()
        {
            return "Muestra como crear una cara rectanglar 3D (Quad) orientable en base a un vector normal. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Quad vacio
            quad = new TgcQuad();

            //Modifiers para vararia sus parametros
            GuiController.Instance.Modifiers.addVertex2f("size", new Vector2(0, 0), new Vector2(100, 100), new Vector2(20, 20));
            GuiController.Instance.Modifiers.addVertex3f("normal", new Vector3(-10, -10, -10), new Vector3(10, 10, 10), new Vector3(0, 1, 1));
            GuiController.Instance.Modifiers.addVertex3f("center", new Vector3(-10, -10, -10), new Vector3(10, 10, 10), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addColor("color", Color.Coral);

            //Flecha para mostrar el sentido del vector normal
            normalArrow = new TgcArrow();
            GuiController.Instance.Modifiers.addBoolean("showNormal", "Show normal", true);

            GuiController.Instance.RotCamera.CameraDistance = 50;
        }

        /// <summary>
        /// Actualiza los parámetros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateQuad(bool showNormal)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            Vector2 size = (Vector2)GuiController.Instance.Modifiers["size"];
            Vector3 normal = (Vector3)GuiController.Instance.Modifiers["normal"];
            Vector3 center = (Vector3)GuiController.Instance.Modifiers["center"];
            Color color = (Color)GuiController.Instance.Modifiers["color"];

            //Cargar valores del quad.
            quad.Center = center;
            quad.Size = size;
            quad.Normal = normal;
            quad.Color = color;

            //Actualizar valors para hacerlos efectivos
            quad.updateValues();

            //Actualizar valores de la flecha
            if (showNormal)
            {
                normalArrow.PStart = quad.Center;
                normalArrow.PEnd = quad.Center + quad.Normal * 10;
                normalArrow.updateValues();
            }

        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            bool showNormal = (bool)GuiController.Instance.Modifiers["showNormal"];

            //Actualizar parametros de la caja
            updateQuad(showNormal);

            quad.render();

            if (showNormal)
            {
                normalArrow.render();
            }
        }


        public override void close()
        {
            quad.dispose();
            normalArrow.dispose();
        }

    }
}
