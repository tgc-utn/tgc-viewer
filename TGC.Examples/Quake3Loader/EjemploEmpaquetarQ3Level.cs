using System.IO;
using System.Windows.Forms;
using TGC.Core;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Quake3Loader
{
    /// <summary>
    ///     Ejemplo EjemploEmpaquetarQ3Level
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimización - BSP y PVS
    ///     Ver primero ejemplo EjemploLoadQ3Level
    ///     Permite empaquetar un escenario de Quake 3 hecho con el editor GtkRadiant.
    ///     Genera como salida una carpeta con todo el contenido mínimo requerido para el escenario especificado.
    ///     Este ejemplo no posee salida gráfica.
    ///     Autor: Martin Giachetti, Matías Leone
    /// </summary>
    public class EjemploEmpaquetarQ3Level : TgcExample
    {
        private string currentFile;
        private string quake3MediaPath;

        public EjemploEmpaquetarQ3Level(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines) : base(mediaDir, shadersDir, userVars, modifiers, axisLines)
        {
            this.Category = "Quake3";
            this.Name = "Pack level";
            this.Description = "Utilidad para empaquetar escenarios de Quake 3 hechos con el editor GtkRadiant.";
        }

        public override void Init()
        {
            //Path de recursos del Quake 3 original (descomprimir archivo pak0.pk3 de la carpeta de instalación del Quake 3, renombrar a .zip)
            quake3MediaPath = "C:\\Program Files\\Quake III Arena\\baseq3\\pak0\\";

            //Modifier para abrir archivo
            currentFile = "C:\\Program Files\\Quake III Arena\\baseq3\\maps\\prueba.bsp";
            this.Modifiers.addFile("BspFile", currentFile, ".Niveles Quake 3|*.bsp");
        }

        public override void Update(float elapsedTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);

            //Ver si se seleccionó alguno nivel a empaquetar
            var selectedFile = (string)this.Modifiers["BspFile"];

            if (selectedFile != currentFile)
            {
                currentFile = selectedFile;

                //Cargar nivel
                var loader = new BspLoader();
                var bspMap = loader.loadBsp(currentFile, quake3MediaPath);

                //Empaquetar
                var info = new FileInfo(currentFile);
                var fileName = info.Name.Substring(0, info.Name.IndexOf('.'));
                var outputDir = info.DirectoryName + "\\" + fileName;

                loader.packLevel(bspMap, quake3MediaPath, outputDir);

                //Librer recursos
                bspMap.dispose();

                MessageBox.Show("Empaquetado almacenado en: " + outputDir, "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

                TgcDrawText.Instance.drawText("Este ejemplo no posee salida gráfica. verificar código y sus comentarios.", 5, 50, Color.Yellow);
            }
        }
    }
}