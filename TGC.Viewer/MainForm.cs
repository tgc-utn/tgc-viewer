using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Util;
using TGC.Util.Ui;
using Microsoft.DirectX;

namespace TGC.Viewer
{
    /// <summary>
    ///     Formulario principal de la aplicación
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Constructor principal de la aplicacion
        /// </summary>
        /// <param name="args">Argumentos de consola</param>
        public MainForm(string[] args)
        {
            TgcViewerConfig.Instance.parseCommandLineArgs(args);

            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public static bool ApplicationRunning { get; set; }

        /// <summary>
        ///     Consola de Log
        /// </summary>
        public RichTextBox LogConsole { get { return this.logConsole; } }

        /// <summary>
        ///     TreeView de ejemplos
        /// </summary>
        public TreeView TreeViewExamples
        {
            get { return this.treeViewExamples; }
        }

        /// <summary>
        ///     Modo FullScreen
        /// </summary>
        public bool FullScreenEnable
        {
            get { return ejecutarEnFullScreenToolStripMenuItem.Checked; }
            set { ejecutarEnFullScreenToolStripMenuItem.Checked = value; }
        }

        /// <summary>
        ///     Mostrar posicion de camara
        /// </summary>
        public bool MostrarPosicionDeCamaraEnable
        {
            get { return mostrarPosiciónDeCámaraToolStripMenuItem.Checked; }
            set { mostrarPosiciónDeCámaraToolStripMenuItem.Checked = value; }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Configuracion de ventana principal
            Text = TgcViewerConfig.Instance.title;

            if (!TgcViewerConfig.Instance.showTitleBar)
            {
                ControlBox = false;
                Text = null;
            }

            //Modo fullscreen
            if (TgcViewerConfig.Instance.fullScreenMode)
            {
                //Quitar todos los paneles que no queremos que se vean
                Controls.Clear();
                splitContainerUserVars.Panel1.Controls.Remove(panel3d);
                splitContainerModifiers.Panel1.Controls.Remove(groupBoxModifiers);

                //Acomodar paneles para fullscreen
                if (TgcViewerConfig.Instance.showModifiersPanel)
                {
                    var sp = new SplitContainer();
                    sp.Orientation = Orientation.Vertical;
                    sp.Dock = DockStyle.Fill;
                    sp.SplitterDistance = 600;
                    sp.IsSplitterFixed = true;
                    sp.Panel1.Controls.Add(panel3d);
                    sp.Panel2.Controls.Add(groupBoxModifiers);
                    Controls.Add(sp);
                }
                else
                {
                    Controls.Add(panel3d);
                }
            }

            //Show the App before we init
            Show();
            panel3d.Focus();

            ApplicationRunning = true;

            GuiController.Instance.initGraphics(this, this.panel3d, this.logConsole, this.dataGridUserVars, this.flowLayoutPanelModifiers, this.treeViewExamples);
            //Cargar ejemplo default
            var defaultExample = GuiController.Instance.ExampleLoader.getExampleByName(TgcViewerConfig.Instance.defaultExampleName,
               TgcViewerConfig.Instance.defaultExampleCategory);
            this.executeExample(defaultExample);

            resetMenuOptions();

            while (ApplicationRunning)
            {
                //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                if (applicationActive())
                {
                    GuiController.Instance.render();
                }
                //Contemplar también la ventana del modo FullScreen
                else if (FullScreenEnable && GuiController.Instance.FullScreenPanel.ContainsFocus)
                {
                    GuiController.Instance.render();
                }
                else
                {
                    //Si no tenemos el foco, dormir cada tanto para no consumir gran cantida de CPU
                    Thread.Sleep(100);
                }

                // Process application messages
                Application.DoEvents();
            }

            //shutDown();
        }

        /// <summary>
        ///     Finalizar aplicacion
        /// </summary>
        private void shutDown()
        {
            ApplicationRunning = false;
            //GuiController.Instance.shutDown();
            //Application.Exit();

            //Matar proceso principal a la fuerza
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }

        /// <summary>
        ///     Cerrando el formulario
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            shutDown();
        }

        /// <summary>
        ///     Texto de ToolStripStatusCurrentExample
        /// </summary>
        /// <param name="text"></param>
        public void setCurrentExampleStatus(string text)
        {
            toolStripStatusCurrentExample.Text = text;
        }

        /// <summary>
        ///     Tabla de variables de usuario
        /// </summary>
        /// <returns></returns>
        public DataGridView getDataGridUserVars()
        {
            return dataGridUserVars;
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wireframeToolStripMenuItem.Checked)
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.WireFrame;
            }
            else
            {
                D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
            }
        }

        private void camaraPrimeraPersonaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GuiController.Instance.FpsCamera.Enable = camaraPrimeraPersonaToolStripMenuItem.Checked;
        }

        private void contadorFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GuiController.Instance.FpsCounterEnable = contadorFPSToolStripMenuItem.Checked;
        }

        private void treeViewExamples_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (TreeViewExamples.SelectedNode != null)
            {
                var selectedNode = TreeViewExamples.SelectedNode;
                textBoxExampleDescription.Text = selectedNode.ToolTipText;
            }
        }

        private void treeViewExamples_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedNode = TreeViewExamples.SelectedNode;

            if (selectedNode != null && selectedNode.Nodes.Count == 0)
            {
                var example = GuiController.Instance.ExampleLoader.getExampleByTreeNode(selectedNode);
                this.executeExample(example);
            }
        }

        private void ejesCartesianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GuiController.Instance.AxisLines.Enable = ejesCartesianosToolStripMenuItem.Checked;
        }

        /// <summary>
        ///     Setea los valores default de las opciones del menu
        /// </summary>
        public void resetMenuOptions()
        {
            camaraPrimeraPersonaToolStripMenuItem.Checked = false;
            wireframeToolStripMenuItem.Checked = false;
            contadorFPSToolStripMenuItem.Checked = true;
            ejesCartesianosToolStripMenuItem.Checked = true;
        }

        public void removePanel3dFromMainForm()
        {
            splitContainerUserVars.Panel1.Controls.Remove(panel3d);
        }

        public void addPanel3dToMainForm()
        {
            splitContainerUserVars.Panel1.Controls.Add(panel3d);
        }

        /// <summary>
        ///     Mostrar ventana de About
        /// </summary>
        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutWindow().ShowDialog(this);
        }

        private void mostrarPosiciónDeCámaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO ya no se muestra mas la posicion de la camara... si se quiere hay que implementarlo mejor que antes
        }

        /// <summary>
        ///     Indica si la aplicacion esta activa.
        ///     Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        private bool applicationActive()
        {
            if (ContainsFocus) return true;
            foreach (var form in OwnedForms)
            {
                if (form.ContainsFocus) return true;
            }
            return false;
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///     Arranca a ejecutar un ejemplo.
        ///     Para el ejemplo anterior, si hay alguno.
        /// </summary>
        /// <param name="example"></param>
        public void executeExample(TgcExample example)
        {
            this.stopCurrentExample();
            GuiController.Instance.UserVars.clearVars();
            GuiController.Instance.Modifiers.clear();
            GuiController.Instance.resetDefaultConfig();
            GuiController.Instance.FpsCamera.resetValues();
            GuiController.Instance.RotCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.resetValues();

            //Ejecutar init
            try
            {
                example.init();

                //Ver si abrimos una ventana para modo FullScreen
                if (TgcViewerConfig.Instance.fullScreenMode)
                {
                    this.removePanel3dFromMainForm();
                    GuiController.Instance.FullScreenPanel.Controls.Add(GuiController.Instance.Panel3d);
                    GuiController.Instance.FullScreenPanel.Show(this);
                }

                GuiController.Instance.CurrentExample = example;
                GuiController.Instance.Panel3d.Focus();
                this.setCurrentExampleStatus("Ejemplo actual: " + example.getName());
                GuiController.Instance.Logger.log("Ejecutando ejemplo: " + example.getName(), Color.Blue);
            }
            catch (Exception e)
            {
                GuiController.Instance.Logger.logError("Error en init() de ejemplo: " + example.getName(), e);
            }
        }

        /// <summary>
        ///     Deja de ejecutar el ejemplo actual
        /// </summary>
        public void stopCurrentExample()
        {
            if (GuiController.Instance.CurrentExample != null)
            {
                GuiController.Instance.CurrentExample.close();
                D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
                GuiController.Instance.Logger.log("Ejemplo " + GuiController.Instance.CurrentExample.getName() + " terminado");
                GuiController.Instance.CurrentExample = null;
                GuiController.Instance.ElapsedTime = -1;

                if (TgcViewerConfig.Instance.fullScreenMode && GuiController.Instance.FullScreenPanel.Visible)
                {
                    closeFullScreenPanel();
                }
            }
        }

        /// <summary>
        ///     Cuando se cierra la ventana de FullScreen
        /// </summary>
        public void closeFullScreenPanel()
        {
            GuiController.Instance.FullScreenPanel.Controls.Remove(GuiController.Instance.Panel3d);
            this.addPanel3dToMainForm();
            GuiController.Instance.FullScreenPanel.Hide();
            GuiController.Instance.Panel3d.Focus();
        }

        /// <summary>
        ///     Cuando el Direct3D Device se resetea.
        ///     Se reinica el ejemplo actual, si hay alguno.
        /// </summary>
        public void onResetDevice()
        {
            var exampleBackup = GuiController.Instance.CurrentExample;

            if (exampleBackup != null)
            {
                stopCurrentExample();
            }

            GuiController.Instance.TgcD3DDevice.doResetDevice();

            if (exampleBackup != null)
            {
                executeExample(exampleBackup);
            }
        }
    }
}