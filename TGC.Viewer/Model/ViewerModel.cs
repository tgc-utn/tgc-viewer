using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using TGC.Core;
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
        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public bool ApplicationRunning { get; set; }

        /// <summary>
        /// Cargador de ejemplos
        /// </summary>
        public ExampleLoader ExampleLoader { get; private set; }

        private ViewerForm form;
        private Panel panel3D;
        private ToolStripStatusLabel toolStripStatus;

        public void InitGraphics(ViewerForm form, TreeView treeViewExamples, Panel panel3D, FlowLayoutPanel flowLayoutPanelModifiers, DataGridView dataGridUserVars, ToolStripStatusLabel toolStripStatusCurrentExample)
        {
            this.ApplicationRunning = true;

            this.form = form;
            this.panel3D = panel3D;
            this.toolStripStatus = toolStripStatusCurrentExample;

            //Configuracion
            Settings settings = Settings.Default;

            D3DDevice.Instance.InitializeD3DDevice(this.panel3D);
            D3DDevice.Instance.Device.DeviceReset += this.OnResetDevice;
            this.OnResetDevice(D3DDevice.Instance.Device, null);

            //Iniciar otras herramientas
            TgcD3dInput.Instance.Initialize(this.form, this.panel3D);
            TgcDirectSound.Instance.InitializeD3DDevice(this.form);

            //Directorio actual de ejecucion
            string currentDirectory = Environment.CurrentDirectory + "\\";

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(currentDirectory + settings.ShadersDirectory + settings.CommonShaders);

            //Cargo los ejemplos en el arbol
            this.ExampleLoader = new ExampleLoader(currentDirectory + settings.MediaDirectory, currentDirectory + settings.ShadersDirectory, dataGridUserVars, flowLayoutPanelModifiers);
            this.ExampleLoader.LoadExamplesInGui(treeViewExamples, currentDirectory);

            //Cargar ejemplo default
            try
            {
                var defaultExample = this.ExampleLoader.GetExampleByName(settings.DefaultExampleName, settings.DefaultExampleCategory);
                this.ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InitRenderLoop()
        {
            while (this.ApplicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo
                if (this.ExampleLoader.CurrentExample != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                    if (this.form.ApplicationActive())
                    {
                        this.ExampleLoader.CurrentExample.Render();
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
            this.ExampleLoader.CurrentExample.FPS = state;

        }

        public void AxisLines(bool state)
        {
            this.ExampleLoader.CurrentExample.AxisLines.Enable = state;
        }

        public void ExecuteExample(TreeNode selectedNode)
        {
            this.ExecuteExample(ExampleLoader.GetExampleByTreeNode(selectedNode));
        }

        /// <summary>
        ///     This event-handler is a good place to create and initialize any
        ///     Direct3D related objects, which may become invalid during a
        ///     device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            //TODO antes hacia esto que no entiendo porque GuiController.Instance.onResetDevice();
            //ese metodo se movio a mainform, pero solo detenia el ejemplo ejecutaba doresetdevice y lo volvia a cargar...
            this.doResetDevice();
        }

        /// <summary>
        ///     Hace las operaciones de Reset del device
        /// </summary>
        public void doResetDevice()
        {
            D3DDevice.Instance.DefaultValues();

            //Reset Timer
            HighResolutionTimer.Instance.Reset();
        }

        /// <summary>
        ///     Arranca a ejecutar un ejemplo.
        ///     Para el ejemplo anterior, si hay alguno.
        /// </summary>
        /// <param name="example"></param>
        public void ExecuteExample(TgcExample example)
        {
            this.StopCurrentExample();

            //Ejecutar Init
            try
            {
                example.ResetDefaultConfig();
                example.Init();
                this.ExampleLoader.CurrentExample = example;
                this.toolStripStatus.Text = "Ejemplo actual: " + example.Name;
                this.panel3D.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error en Init() de ejemplo: " + example.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void StopCurrentExample()
        {
            if (this.ExampleLoader.CurrentExample != null)
            {
                this.ExampleLoader.CurrentExample.Close();
                this.toolStripStatus.Text = "Ejemplo actual terminado." + this.ExampleLoader.CurrentExample.Name + " terminado";
                this.ExampleLoader.CurrentExample = null;
            }
        }

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        public void ShutDown()
        {
            this.ApplicationRunning = false;

            if (this.ExampleLoader.CurrentExample != null)
            {
                this.ExampleLoader.CurrentExample.Close();
            }

            //Liberar Device al finalizar la aplicacion
            D3DDevice.Instance.Device.Dispose();
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
            var exampleBackup = this.ExampleLoader.CurrentExample;

            if (exampleBackup != null)
            {
                StopCurrentExample();
            }

            this.doResetDevice();

            if (exampleBackup != null)
            {
                ExecuteExample(exampleBackup);
            }
        }
    }
}