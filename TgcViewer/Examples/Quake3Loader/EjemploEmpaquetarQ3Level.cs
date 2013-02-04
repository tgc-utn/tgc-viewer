using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using System.IO;

namespace Examples.Quake3Loader
{
    /// <summary>
    /// Ejemplo EjemploEmpaquetarQ3Level
    /// Unidades Involucradas:
    ///     # Unidad 7 - Optimización - BSP y PVS
    ///     
    /// Ver primero ejemplo EjemploLoadQ3Level
    /// Permite empaquetar un escenario de Quake 3 hecho con el editor GtkRadiant.
    /// Genera como salida una carpeta con todo el contenido mínimo requerido para el escenario especificado.
    /// Este ejemplo no posee salida gráfica.
    /// 
    /// 
    /// Autor: Martin Giachetti, Matías Leone
    /// 
    /// </summary>
    public class EjemploEmpaquetarQ3Level : TgcExample
    {
 
        string quake3MediaPath;
        string currentFile;

        public override string getCategory()
        {
            return "Quake3";
        }

        public override string getName()
        {
            return "Pack level";
        }

        public override string getDescription()
        {
            return "Utilidad para empaquetar escenarios de Quake 3 hechos con el editor GtkRadiant.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path de recursos del Quake 3 original (descomprimir archivo pak0.pk3 de la carpeta de instalación del Quake 3, renombrar a .zip)
            quake3MediaPath = "C:\\Program Files\\Quake III Arena\\baseq3\\pak0\\";

            //Modifier para abrir archivo
            currentFile = "C:\\Program Files\\Quake III Arena\\baseq3\\maps\\prueba.bsp";
            GuiController.Instance.Modifiers.addFile("BspFile", currentFile, ".Niveles Quake 3|*.bsp");
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si se seleccionó alguno nivel a empaquetar
            string selectedFile = (string)GuiController.Instance.Modifiers["BspFile"];
            if (selectedFile != currentFile)
            {
                currentFile = selectedFile;

                //Cargar nivel
                BspLoader loader = new BspLoader();
                BspMap bspMap = loader.loadBsp(currentFile, quake3MediaPath);

                //Empaquetar
                FileInfo info = new FileInfo(currentFile);
                string fileName = info.Name.Substring(0, info.Name.IndexOf('.'));
                string outputDir = info.DirectoryName + "\\" + fileName;

                loader.packLevel(bspMap, quake3MediaPath, outputDir);

                //Librer recursos
                bspMap.dispose();

                MessageBox.Show(GuiController.Instance.MainForm, "Empaquetado almacenado en: " + outputDir, 
                    "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public override void close()
        {

        }

    }
}
