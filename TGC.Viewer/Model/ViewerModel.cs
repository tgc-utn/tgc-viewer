using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.Shaders;
using TGC.Core.Sound;
using TGC.Core.Textures;
using TGC.Viewer.Properties;
using TGC.Viewer.UI;

namespace TGC.Viewer.Model
{
    public class ViewerModel
    {
        private ViewerForm Form { get; set; }

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public bool ApplicationRunning { get; set; }

        private TgcDirectSound DirectSound { get; set; }

        private TgcD3dInput Input { get; set; }

        /// <summary>
        ///     Cargador de ejemplos
        /// </summary>
        public ExampleLoader ExampleLoader { get; private set; }

        public void InitGraphics(ViewerForm form, TreeView treeViewExamples, Panel panel3D,
            ToolStripStatusLabel toolStripStatusCurrentExample)
        {
            ApplicationRunning = true;

            Form = form;

            //Inicio Device
            D3DDevice.Instance.InitializeD3DDevice(panel3D);
            D3DDevice.Instance.Device.DeviceReset += OnResetDevice;

            //Inicio inputs
            Input = new TgcD3dInput();
            Input.Initialize(Form, panel3D);

            //Inicio sonido
            DirectSound = new TgcDirectSound();
            DirectSound.InitializeD3DDevice(panel3D);

            //Directorio actual de ejecucion
            var currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(currentDirectory + Settings.Default.ShadersDirectory +
                                                  Settings.Default.CommonShaders);
        }

        public void LoadExamples(TreeView treeViewExamples, FlowLayoutPanel flowLayoutPanelModifiers,
            DataGridView dataGridUserVars)
        {
            //Configuracion
            var settings = Settings.Default;

            //Directorio actual de ejecucion
            var currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargo los ejemplos en el arbol
            ExampleLoader = new ExampleLoader(currentDirectory + settings.MediaDirectory,
                currentDirectory + settings.ShadersDirectory, dataGridUserVars, flowLayoutPanelModifiers);
            ExampleLoader.LoadExamplesInGui(treeViewExamples, currentDirectory);
        }

        public void InitRenderLoop()
        {
            while (ApplicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo
                if (ExampleLoader.CurrentExample != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                    if (Form.ApplicationActive())
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

        public void Wireframe(bool state)
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
            ExampleLoader.CurrentExample.AxisLinesEnable = state;
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
            ExampleLoader.CurrentExample = example;
            //TODO esto no me cierra mucho OnResetDevice
            OnResetDevice(D3DDevice.Instance.Device, null);
            example.ResetDefaultConfig();
            example.DirectSound = DirectSound;
            example.Input = Input;
            example.Init();
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (ExampleLoader.CurrentExample != null)
            {
                ExampleLoader.CurrentExample.Dispose();
                ExampleLoader.CurrentExample = null;
            }
        }

        /// <summary>
        ///     Finaliza el render loop y hace dispose del ejemplo y recursos
        /// </summary>
        public void Dispose()
        {
            ApplicationRunning = false;

            StopCurrentExample();

            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Dispose();
            TexturesPool.Instance.clearAll();
        }

        internal void UpdateAspectRatio(Panel panel)
        {
            D3DDevice.Instance.UpdateAspectRatioAndProjection(panel.Width, panel.Height);
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

            DoResetDevice();

            if (exampleBackup != null)
            {
                ExecuteExample(exampleBackup);
            }
        }

        /// <summary>
        ///     This event-handler is a good place to create and initialize any
        ///     Direct3D related objects, which may become invalid during a
        ///     device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            //TODO antes hacia esto que no entiendo porque GuiController.Instance.onResetDevice();
            //pero solo detenia el ejemplo ejecutaba doResetDevice y lo volvia a cargar...
            DoResetDevice();
        }

        /// <summary>
        ///     Hace las operaciones de Reset del device
        /// </summary>
        public void DoResetDevice()
        {
            //Default values para el device
            ExampleLoader.CurrentExample.DeviceDefaultValues();

            //Reset Timer
            ExampleLoader.CurrentExample.ResetTimer();
        }

        public void DownloadMediaFolder()
        {
            var client = new WebClient();

            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;

            // Starts the download
            //client.DownloadFileAsync(new Uri("http://tgcutn.com.ar/images/logotp.png"), @"C:\Users\Mito\Downloads\logotp.png");

            //btnStartDownload.Text = "Download In Process";
            //btnStartDownload.Enabled = false;
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = double.Parse(e.BytesReceived.ToString());
            var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            var percentage = bytesIn / totalBytes * 100;

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