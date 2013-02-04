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

namespace Examples.Fog
{
    /// <summary>
    /// Ejemplo EfectoNiebla 
    /// Unidades Involucradas:
    ///     # Unidad 4 - Conceptos Básicos de 3D - Fog
    /// 
    /// Muestra como utilizar los efectos de niebla provistos por el Pipeline, a través de la herramienta TgcFog
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo 
    /// 
    /// </summary>
    public class EfectoNiebla : TgcExample
    {
        TgcBox box;

        public override string getCategory()
        {
            return "Fog";
        }

        public override string getName()
        {
            return "Efecto Niebla";
        }

        public override string getDescription()
        {
            return "Muestra como utilizar el efecto niebla y como configurar sus diversos atributos.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear caja
            box = TgcBox.fromSize(new Vector3(100, 100, 100), 
                TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\pasto.jpg"));

            //Camara rotacional
            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);

            //Modifiers para configurar valores de niebla
            GuiController.Instance.Modifiers.addBoolean("Enabled", "Enabled", true);
            GuiController.Instance.Modifiers.addFloat("startDistance", 1, 1000, 100);
            GuiController.Instance.Modifiers.addFloat("endDistance", 1, 1000, 500);
            GuiController.Instance.Modifiers.addFloat("density", 1, 10, 1);
            GuiController.Instance.Modifiers.addColor("color", Color.Gray);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar valores de niebla
            GuiController.Instance.Fog.Enabled = (bool)GuiController.Instance.Modifiers["Enabled"];
            GuiController.Instance.Fog.StartDistance = (float)GuiController.Instance.Modifiers["startDistance"];
            GuiController.Instance.Fog.EndDistance = (float)GuiController.Instance.Modifiers["endDistance"];
            GuiController.Instance.Fog.Density = (float)GuiController.Instance.Modifiers["density"];
            GuiController.Instance.Fog.Color = (Color)GuiController.Instance.Modifiers["color"];
            
            //Actualizar valores
            GuiController.Instance.Fog.updateValues();

            
            box.render();
        }

        public override void close()
        {
            box.dispose();
        }

    }
}
