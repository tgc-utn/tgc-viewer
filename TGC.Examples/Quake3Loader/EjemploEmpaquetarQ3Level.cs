using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

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
    public class EjemploEmpaquetarQ3Level : TGCExampleViewer
    {
        private TGCFileModifier openFileModifier;

        private string currentFile;
        private string quake3MediaPath;

        public EjemploEmpaquetarQ3Level(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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
            openFileModifier = AddFile("BspFile", currentFile, ".Niveles Quake 3|*.bsp");
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();

            //Ver si se seleccionó alguno nivel a empaquetar
            var selectedFile = openFileModifier.Value;

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

                DrawText.drawText("Este ejemplo no posee salida gráfica. verificar código y sus comentarios.", 5, 50, Color.OrangeRed);
            }

            PostRender();
        }

        public override void Dispose()
        {
            //No hay recursos que liberar.
        }
    }
}