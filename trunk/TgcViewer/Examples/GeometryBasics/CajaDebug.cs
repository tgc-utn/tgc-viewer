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
    /// Ejemplo CajaDebug
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    /// 
    /// Muestra como crear una caja 3D WireFrame, en la cual solo se ven sus aristas, pero no sus caras.
    /// Se utiliza la herramienta TgcDebugBox.
    /// Cada arista es un Box rectangular.
    /// Es �til para hacer debug de ciertas estructuras.
    /// 
    /// 
    /// Autor: Mat�as Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class CajaDebug : TgcExample
    {
        TgcDebugBox debugBox;

        public override string getCategory()
        {
            return "GeometryBasics";
        }

        public override string getName()
        {
            return "Caja Debug";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja que solo renderiza sus aristas, y no sus caras. Movimiento con mouse.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear caja debug vacia
            debugBox = new TgcDebugBox();

            //Modifiers para vararis sus parametros
            GuiController.Instance.Modifiers.addVertex3f("size", new Vector3(0, 0, 0), new Vector3(100, 100, 100), new Vector3(20, 20, 20));
            GuiController.Instance.Modifiers.addVertex3f("position", new Vector3(-100, -100, -100), new Vector3(100, 100, 100), new Vector3(0, 0, 0));
            GuiController.Instance.Modifiers.addFloat("thickness", 0.1f, 5, 0.2f);
            GuiController.Instance.Modifiers.addColor("color", Color.BurlyWood);

            GuiController.Instance.RotCamera.CameraDistance = 50;
        }

        /// <summary>
        /// Actualiza los par�metros de la caja en base a lo cargado por el usuario
        /// </summary>
        private void updateBox()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            Vector3 size = (Vector3)GuiController.Instance.Modifiers["size"];
            Vector3 position = (Vector3)GuiController.Instance.Modifiers["position"];
            float thickness = (float)GuiController.Instance.Modifiers["thickness"];
            Color color = (Color)GuiController.Instance.Modifiers["color"];

            //Actualizar valores en la caja.
            debugBox.setPositionSize(position, size);
            debugBox.Thickness = thickness;
            debugBox.Color = color;
            debugBox.updateValues();
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Actualizar parametros de la caja
            updateBox();

            debugBox.render();
        }


        public override void close()
        {
            debugBox.dispose();
        }

    }
}
