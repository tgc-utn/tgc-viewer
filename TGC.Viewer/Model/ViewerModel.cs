using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Viewer.Forms;
using TGC.Viewer.Properties;

namespace TGC.Viewer.Model
{
    public class ViewerModel
    {
        private ViewerForm form;
        private Panel panel3D;
        private ToolStripStatusLabel toolStripStatus;

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public bool ApplicationRunning { get; set; }

        /// <summary>
        ///     Cargador de ejemplos
        /// </summary>
        public ExampleLoader ExampleLoader { get; private set; }

        public void InitGraphics(ViewerForm form, TreeView treeViewExamples, Panel panel3D,
            FlowLayoutPanel flowLayoutPanelModifiers, DataGridView dataGridUserVars,
            ToolStripStatusLabel toolStripStatusCurrentExample)
        {
            ApplicationRunning = true;

            this.form = form;
            this.panel3D = panel3D;
            toolStripStatus = toolStripStatusCurrentExample;

            //Configuracion
            var settings = Settings.Default;

            D3DDevice.Instance.InitializeD3DDevice(this.panel3D);

            //Iniciar otras herramientas
            TgcD3dInput.Instance.Initialize(this.form, this.panel3D);
            TgcDirectSound.Instance.InitializeD3DDevice(this.form);

            //Directorio actual de ejecucion
            var currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(currentDirectory + settings.ShadersDirectory + settings.CommonShaders);

            //Cargo los ejemplos en el arbol
            ExampleLoader = new ExampleLoader(currentDirectory + settings.MediaDirectory,
                currentDirectory + settings.ShadersDirectory, dataGridUserVars, flowLayoutPanelModifiers);
            ExampleLoader.LoadExamplesInGui(treeViewExamples, currentDirectory);

            //Cargar ejemplo default
            try
            {
                var defaultExample = ExampleLoader.GetExampleByName(settings.DefaultExampleName,
                    settings.DefaultExampleCategory);
                ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InitRenderLoop()
        {
            while (ApplicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo
                if (ExampleLoader.CurrentExample != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                    if (form.ApplicationActive())
                    {
                        ExampleLoader.CurrentExample.Update();
                        ExampleLoader.CurrentExample.Render();
                    }
                    else
                    {
                        //Si no tenemos el foco, dormir cada tanto para no consumir gran cantidad de CPU
                        Thread.Sleep(100);
                    }
                }
                // Process application messages
                Application.DoEvents();
            }
        }

        public void WireFrame(bool state)
        {
            if (state)
            {
                D3DDevice.Instance.FillModeWireFrame();
            }
            else
            {
                D3DDevice.Instance.FillModeWireSolid();
            }
        }

        public void ContadorFPS(bool state)
        {
            ExampleLoader.CurrentExample.FPS = state;
        }

        public void AxisLines(bool state)
        {
            ExampleLoader.CurrentExample.AxisLines.Enable = state;
        }

        public void ExecuteExample(TreeNode selectedNode)
        {
            ExecuteExample(ExampleLoader.GetExampleByTreeNode(selectedNode));
        }

        /// <summary>
        ///     Arranca a ejecutar un ejemplo.
        ///     Para el ejemplo anterior, si hay alguno.
        /// </summary>
        /// <param name="example"></param>
        public void ExecuteExample(TgcExample example)
        {
            StopCurrentExample();

            //Ejecutar Init
            try
            {
                example.ResetDefaultConfig();
                example.Init();
                ExampleLoader.CurrentExample = example;
                toolStripStatus.Text = "Ejemplo actual: " + example.Name;
                panel3D.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error en Init() de ejemplo: " + example.Name, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (ExampleLoader.CurrentExample != null)
            {
                ExampleLoader.CurrentExample.Close();
                toolStripStatus.Text = "Ejemplo actual terminado." + ExampleLoader.CurrentExample.Name + " terminado";
                ExampleLoader.CurrentExample = null;
            }
        }

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        public void ShutDown()
        {
            ApplicationRunning = false;

            if (ExampleLoader.CurrentExample != null)
            {
                ExampleLoader.CurrentExample.Close();
            }

            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Dispose();
            TexturesPool.Instance.clearAll();

            //Application.Exit();

            //Matar proceso principal a la fuerza
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }

        /// <summary>
        ///     Cuando el Direct3D Device se resetea.
        ///     Se reinica el ejemplo actual, si hay alguno.
        /// </summary>
        public void OnResetDevice()
        {
            var exampleBackup = ExampleLoader.CurrentExample;

            if (exampleBackup != null)
            {
                StopCurrentExample();
            }

            D3DDevice.Instance.DoResetDevice();

            if (exampleBackup != null)
            {
                ExecuteExample(exampleBackup);
            }
        }

        public void DownloadMediaFolder()
        {
            WebClient client = new WebClient();

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            // Starts the download
            //client.DownloadFileAsync(new Uri("http://tgcutn.com.ar/images/logotp.png"), @"C:\Users\Mito\Downloads\logotp.png");

            //btnStartDownload.Text = "Download In Process";
            //btnStartDownload.Enabled = false;
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            Console.Write(int.Parse(Math.Truncate(percentage).ToString()));
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            MessageBox.Show("Download Completed");

            //btnStartDownload.Text = "Start Download";
            //btnStartDownload.Enabled = true;

        }
    }
}