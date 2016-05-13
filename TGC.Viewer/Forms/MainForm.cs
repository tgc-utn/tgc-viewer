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
using TGC.Viewer.Model;
using TGC.Viewer.Properties;

namespace TGC.Viewer.Forms
{
    /// <summary>
    ///     Formulario principal de la aplicación
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Panel que se utiliza para ejecutar un ejemplo en modo FullScreen
        /// </summary>
        private FullScreenPanel fullScreenPanel;

        /// <summary>
        /// Cargador de ejemplos
        /// </summary>
        private ExampleLoader exampleLoader;

        /// <summary>
        ///     Indica si la aplicacion arranca en modo full screen
        /// </summary>
        public bool fullScreenMode;

        /// <summary>
        ///     En true muestra el panel derecho de modifiers
        /// </summary>
        public bool showModifiersPanel;

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public bool applicationRunning;

        /// <summary>
        ///     Constructor principal de la aplicacion
        /// </summary>
        /// <param name="args">Argumentos de consola</param>
        public MainForm(string[] args)
        {
            this.ParseCommandLineArgs(args);

            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

        #region Eventos de la ventana

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Configuracion de ventana principal
            this.Text = Settings.Default.Title;

            //Modo fullscreen
            if (this.fullScreenMode)
            {
                //Quitar todos los paneles que no queremos que se vean
                this.Controls.Clear();
                this.RemovePanel3DFromMainForm();

                //Acomodar paneles para fullscreen
                if (this.showModifiersPanel)
                {
                    var splitContainer = new SplitContainer();
                    splitContainer.Orientation = Orientation.Vertical;
                    splitContainer.Dock = DockStyle.Fill;
                    splitContainer.SplitterDistance = 600;
                    splitContainer.IsSplitterFixed = true;
                    splitContainer.Panel1.Controls.Add(panel3d);
                    splitContainer.Panel2.Controls.Add(groupBoxModifiers);
                    Controls.Add(splitContainer);
                }
                else
                {
                    Controls.Add(panel3d);
                }
            }

            //Show the App before we Init
            this.Show();
            this.applicationRunning = true;

            this.fullScreenPanel = new FullScreenPanel();
            this.panel3d.Focus();

            //Iniciar graficos
            D3DDevice.Instance.InitializeD3DDevice(this.panel3d);
            D3DDevice.Instance.Device.DeviceReset += this.OnResetDevice;
            this.OnResetDevice(D3DDevice.Instance.Device, null);

            //Iniciar otras herramientas
            TgcD3dInput.Instance.Initialize(this, this.panel3d);
            TgcDirectSound.Instance.InitializeD3DDevice(this);

            //Directorio actual de ejecucion
            string currentDirectory = Environment.CurrentDirectory + "\\";

            //Configuracion
            Settings settings = Settings.Default;

            //Cargar shaders del framework
            TgcShaders.Instance.loadCommonShaders(currentDirectory + settings.ShadersDirectory + settings.CommonShaders);

            //Cargo los ejemplos en el arbol
            this.exampleLoader = new ExampleLoader(currentDirectory + settings.MediaDirectory, currentDirectory + settings.ShadersDirectory, dataGridUserVars, flowLayoutPanelModifiers);
            this.exampleLoader.LoadExamplesInGui(treeViewExamples, currentDirectory);

            //Cargar ejemplo default
            try
            {
                var defaultExample = this.exampleLoader.GetExampleByName(settings.DefaultExampleName, settings.DefaultExampleCategory);
                this.ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.ResetMenuOptions();

            while (this.applicationRunning)
            {
                //Renderizo si es que hay un ejemplo activo
                if (this.exampleLoader.CurrentExample != null)
                {
                    //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                    if (this.ApplicationActive())
                    {
                        this.Render();
                    }
                    //Contemplar también la ventana del modo FullScreen
                    else if (this.ejecutarEnFullScreenToolStripMenuItem.Checked && this.fullScreenPanel.ContainsFocus)
                    {
                        this.Render();
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

            //ShutDown();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShutDown();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wireframeToolStripMenuItem.Checked)
            {
                D3DDevice.Instance.FillModeWireFrame();
            }
            else
            {
                D3DDevice.Instance.FillModeWireSolid();
            }
        }

        private void treeViewExamples_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.treeViewExamples.SelectedNode != null)
            {
                var selectedNode = this.treeViewExamples.SelectedNode;
                textBoxExampleDescription.Text = selectedNode.ToolTipText;
            }
        }

        private void treeViewExamples_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedNode = this.treeViewExamples.SelectedNode;

            if (selectedNode != null && selectedNode.Nodes.Count == 0)
            {
                var example = exampleLoader.GetExampleByTreeNode(selectedNode);

                if (this.ejecutarEnFullScreenToolStripMenuItem.Checked)
                {
                    this.RemovePanel3DFromMainForm();
                    fullScreenPanel.Controls.Add(panel3d);
                    fullScreenPanel.Show(this);
                }

                this.ExecuteExample(example);
            }
        }

        private void contadorFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.exampleLoader.CurrentExample.FPS = this.contadorFPSToolStripMenuItem.Checked;
        }

        private void ejesCartesianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.exampleLoader.CurrentExample.AxisLines.Enable = this.ejesCartesianosToolStripMenuItem.Checked;
        }

        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        private void ShutDown()
        {
            this.applicationRunning = false;

            if (this.exampleLoader.CurrentExample != null)
            {
                this.exampleLoader.CurrentExample.Close();
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
        ///     Setea los valores default de las opciones del menu
        /// </summary>
        public void ResetMenuOptions()
        {
            wireframeToolStripMenuItem.Checked = false;
            contadorFPSToolStripMenuItem.Checked = true;
            ejesCartesianosToolStripMenuItem.Checked = true;
        }

        /// <summary>
        /// Quita el panel 3D de la ventana
        /// </summary>
        public void RemovePanel3DFromMainForm()
        {
            this.Controls.Remove(panel3d);
        }

        /// <summary>
        /// Agregar el panel 3D a la ventana
        /// </summary>
        public void AddPanel3DToMainForm()
        {
            this.Controls.Add(panel3d);
        }

        /// <summary>
        ///     Indica si la aplicacion esta activa.
        ///     Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        private bool ApplicationActive()
        {
            if (ContainsFocus)
            {
                return true;
            }

            foreach (var form in OwnedForms)
            {
                if (form.ContainsFocus)
                {
                    return true;
                }
            }

            return false;
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

                //Ver si abrimos una ventana para modo FullScreen
                if (this.fullScreenMode)
                {
                    this.RemovePanel3DFromMainForm();
                    this.fullScreenPanel.Controls.Add(this.panel3d);
                    this.fullScreenPanel.Show(this);
                }

                this.exampleLoader.CurrentExample = example;
                this.panel3d.Focus();
                this.toolStripStatusCurrentExample.Text = "Ejemplo actual: " + example.Name;
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
            if (this.exampleLoader.CurrentExample != null)
            {
                this.exampleLoader.CurrentExample.Close();
                this.toolStripStatusCurrentExample.Text = "Ejemplo actual terminado." + this.exampleLoader.CurrentExample.Name + " terminado";
                this.exampleLoader.CurrentExample = null;

                if (this.fullScreenMode && this.fullScreenPanel.Visible)
                {
                    this.CloseFullScreenPanel();
                }
            }
        }

        /// <summary>
        ///     Cuando se cierra la ventana de FullScreen
        /// </summary>
        public void CloseFullScreenPanel()
        {
            this.fullScreenPanel.Controls.Remove(this.panel3d);
            this.AddPanel3DToMainForm();
            this.fullScreenPanel.Hide();
            this.panel3d.Focus();
        }

        /// <summary>
        ///     Cuando el Direct3D Device se resetea.
        ///     Se reinica el ejemplo actual, si hay alguno.
        /// </summary>
        public void OnResetDevice()
        {
            var exampleBackup = this.exampleLoader.CurrentExample;

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

        /// <summary>
        ///     Hacer Render del ejemplo
        /// </summary>
        public void Render()
        {
            //Ejecutar Render del ejemplo
            if (this.exampleLoader.CurrentExample != null)
            {
                this.exampleLoader.CurrentExample.Render();
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
        ///     Crea la configuracion a partir de parametros pasados por consola
        ///     Ejemplo: fullScreenMode=true defaultExampleName="Bump Mapping" defaultExampleCategory=Lights
        ///     showModifiersPanel=false title="Mi titulo" showTitleBar=false
        /// </summary>
        public void ParseCommandLineArgs(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var values = args[i].Split('=');
                var key = values[0].Trim();
                var value = values[1].Trim();

                //fullScreenMode=bool
                if (key == "fullScreenMode")
                {
                    fullScreenMode = bool.Parse(value);
                }
                //defaultExampleName=string (si tiene espacios ponerle comillas dobles)
                else if (key == "defaultExampleName")
                {
                    Settings.Default.DefaultExampleName = value;
                }
                //defaultExampleCategory=string (si tiene espacios ponerle comillas dobles)
                else if (key == "defaultExampleCategory")
                {
                    Settings.Default.DefaultExampleCategory = value;
                }
                //showModifiersPanel=string
                else if (key == "showModifiersPanel")
                {
                    showModifiersPanel = bool.Parse(value);
                }
                //title=string (si tiene espacios ponerle comillas dobles)
                else if (key == "title")
                {
                    Settings.Default.Title = value;
                }
            }
            //TODO revisar si se tiene que hacer un Settings.Save o Sincronizar
        }
    }
}