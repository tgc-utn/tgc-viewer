using Microsoft.DirectX.Direct3D;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Util.Ui;

namespace TGC.Util
{
    /// <summary>
    ///     Formulario principal de la aplicación
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Ventana de About
        /// </summary>
        private readonly AboutWindow aboutWindow;

        /// <summary>
        ///     Constructor principal de la aplicacion
        /// </summary>
        /// <param name="args">Argumentos de consola</param>
        public MainForm(string[] args)
        {
            //Cargar configuracion de arranque
            Config = new TgcViewerConfig();
            Config.parseCommandLineArgs(args);

            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

            aboutWindow = new AboutWindow();
        }

        /// <summary>
        ///     Obtener o parar el estado del RenderLoop.
        /// </summary>
        public static bool ApplicationRunning { get; set; }

        /// <summary>
        ///     Configuracion de arranque de la aplicacion
        /// </summary>
        public TgcViewerConfig Config { get; }

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
            Text = Config.title;
            if (!Config.showTitleBar)
            {
                ControlBox = false;
                Text = null;
            }

            //Modo fullscreen
            if (Config.fullScreenMode)
            {
                //Quitar todos los paneles que no queremos que se vean
                Controls.Clear();
                splitContainerUserVars.Panel1.Controls.Remove(panel3d);
                splitContainerModifiers.Panel1.Controls.Remove(groupBoxModifiers);

                //Acomodar paneles para fullscreen
                if (Config.showModifiersPanel)
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
            var guiController = GuiController.Instance;
            guiController.initGraphics(this, panel3d);
            resetMenuOptions();

            while (ApplicationRunning)
            {
                //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                if (applicationActive())
                {
                    guiController.render();
                }
                //Contemplar también la ventana del modo FullScreen
                else if (FullScreenEnable && guiController.FullScreenPanel.ContainsFocus)
                {
                    guiController.render();
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
        ///     Texto de ToolStripStatusPosition
        /// </summary>
        public void setStatusPosition(string text)
        {
            toolStripStatusPosition.Text = text;
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

        /// <summary>
        ///     Panel de Modifiers
        /// </summary>
        /// <returns></returns>
        public Panel getModifiersPanel()
        {
            return flowLayoutPanelModifiers;
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
            if (TreeViewExamples.SelectedNode != null)
            {
                var selectedNode = TreeViewExamples.SelectedNode;
                if (selectedNode.Nodes.Count == 0)
                {
                    GuiController.Instance.executeSelectedExample(selectedNode, FullScreenEnable);
                }
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
            aboutWindow.ShowDialog(this);
        }

        private void mostrarPosiciónDeCámaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!mostrarPosiciónDeCámaraToolStripMenuItem.Checked)
            {
                toolStripStatusPosition.Text = "";
            }
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
    }
}