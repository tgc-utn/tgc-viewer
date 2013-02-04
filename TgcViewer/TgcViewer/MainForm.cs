using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Utils;
using System.Diagnostics;
using TgcViewer.Utils.Ui;

namespace TgcViewer
{
    /// <summary>
    /// Formulario principal de la aplicación
    /// </summary>
    public partial class MainForm : Form
    {
        static bool applicationRunning;
        /// <summary>
        /// Obtener o parar el estado del RenderLoop.
        /// </summary>
        public static bool ApplicationRunning
        {
            get { return MainForm.applicationRunning; }
            set { MainForm.applicationRunning = value; }
        }

        /// <summary>
        /// Ventana de About
        /// </summary>
        private AboutWindow aboutWindow;

        private TgcViewerConfig config;
        /// <summary>
        /// Configuracion de arranque de la aplicacion
        /// </summary>
        public TgcViewerConfig Config
        {
            get { return config; }
        }


        /// <summary>
        /// Constructor principal de la aplicacion
        /// </summary>
        /// <param name="args">Argumentos de consola</param>
        public MainForm(string[] args)
        {
            //Cargar configuracion de arranque
            config = new TgcViewerConfig();
            config.parseCommandLineArgs(args);

            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

            aboutWindow = new AboutWindow();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            //Configuracion de ventana principal
            this.Text = config.title;
            if (!config.showTitleBar)
            {
                this.ControlBox = false;
                this.Text = null;
            }

            //Modo fullscreen
            if (this.config.fullScreenMode)
            {
                //Quitar todos los paneles que no queremos que se vean
                this.Controls.Clear();
                this.splitContainerUserVars.Panel1.Controls.Remove(this.panel3d);
                this.splitContainerModifiers.Panel1.Controls.Remove(this.groupBoxModifiers);

                //Acomodar paneles para fullscreen
                if (config.showModifiersPanel)
                {
                    SplitContainer sp = new SplitContainer();
                    sp.Orientation = Orientation.Vertical;
                    sp.Dock = DockStyle.Fill;
                    sp.SplitterDistance = 600;
                    sp.IsSplitterFixed = true;
                    sp.Panel1.Controls.Add(this.panel3d);
                    sp.Panel2.Controls.Add(this.groupBoxModifiers);
                    this.Controls.Add(sp);
                }
                else
                {
                    this.Controls.Add(this.panel3d);
                }
            }


            //Show the App before we init
            this.Show();
            this.panel3d.Focus();

            MainForm.ApplicationRunning = true;
            GuiController.newInstance();
            GuiController guiController = GuiController.Instance;
            guiController.initGraphics(this, panel3d);
            this.resetMenuOptions();

            while (MainForm.ApplicationRunning)
            {
                //Solo renderizamos si la aplicacion tiene foco, para no consumir recursos innecesarios
                if (this.applicationActive())
                {
                    guiController.render();
                }
                //Contemplar también la ventana del modo FullScreen
                else if (this.FullScreenEnable && guiController.FullScreenPanel.ContainsFocus)
                {
                    guiController.render();
                }
                else
                {
                    //Si no tenemos el foco, dormir cada tanto para no consumir gran cantida de CPU
                    System.Threading.Thread.Sleep(100); 
                }
                
                // Process application messages
                Application.DoEvents();
            }

            //shutDown();
        }

        /// <summary>
        /// Finalizar aplicacion
        /// </summary>
        private void shutDown()
        {
            MainForm.ApplicationRunning = false;
            //GuiController.Instance.shutDown();
            //Application.Exit();

            //Matar proceso principal a la fuerza
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }

        /// <summary>
        /// Cerrando el formulario
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            shutDown();
        }

        /// <summary>
        /// Consola de Log
        /// </summary>
        public RichTextBox LogConsole
        {
            get { return logConsole; }
        }

        /// <summary>
        /// TreeView de ejemplos
        /// </summary>
        public TreeView TreeViewExamples
        {
            get { return treeViewExamples; }
        }

        

        /// <summary>
        /// Texto de ToolStripStatusPosition
        /// </summary>
        public void setStatusPosition(string text)
        {
            toolStripStatusPosition.Text = text;
        }

        /// <summary>
        /// Texto de ToolStripStatusCurrentExample
        /// </summary>
        /// <param name="text"></param>
        public void setCurrentExampleStatus(string text)
        {
            toolStripStatusCurrentExample.Text = text;
        }

        /// <summary>
        /// Tabla de variables de usuario
        /// </summary>
        /// <returns></returns>
        public DataGridView getDataGridUserVars()
        {
            return dataGridUserVars;
        }

        /// <summary>
        /// Panel de Modifiers
        /// </summary>
        /// <returns></returns>
        internal Panel getModifiersPanel()
        {
            return flowLayoutPanelModifiers;
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wireframeToolStripMenuItem.Checked)
            {
                GuiController.Instance.D3dDevice.RenderState.FillMode = Microsoft.DirectX.Direct3D.FillMode.WireFrame;
            }
            else
            {
                GuiController.Instance.D3dDevice.RenderState.FillMode = Microsoft.DirectX.Direct3D.FillMode.Solid;
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
            if (treeViewExamples.SelectedNode != null)
            {
                TreeNode selectedNode = treeViewExamples.SelectedNode;
                textBoxExampleDescription.Text = selectedNode.ToolTipText;
            }
        }

        private void treeViewExamples_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeViewExamples.SelectedNode != null)
            {
                TreeNode selectedNode = treeViewExamples.SelectedNode;
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
        /// Setea los valores default de las opciones del menu
        /// </summary>
        internal void resetMenuOptions()
        {
            camaraPrimeraPersonaToolStripMenuItem.Checked = false;
            wireframeToolStripMenuItem.Checked = false;
            contadorFPSToolStripMenuItem.Checked = true;
            ejesCartesianosToolStripMenuItem.Checked = true;
        }

        /// <summary>
        /// Modo FullScreen
        /// </summary>
        internal bool FullScreenEnable
        {
            get { return ejecutarEnFullScreenToolStripMenuItem.Checked; }
            set { ejecutarEnFullScreenToolStripMenuItem.Checked = value; }
        }

        public void removePanel3dFromMainForm()
        {
            this.splitContainerUserVars.Panel1.Controls.Remove(this.panel3d);
        }

        public void addPanel3dToMainForm()
        {
            this.splitContainerUserVars.Panel1.Controls.Add(this.panel3d);
        }

        /// <summary>
        /// Mostrar ventana de About
        /// </summary>
        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutWindow.ShowDialog(this);
        }


        /// <summary>
        /// Mostrar posicion de camara
        /// </summary>
        internal bool MostrarPosicionDeCamaraEnable
        {
            get { return mostrarPosiciónDeCámaraToolStripMenuItem.Checked; }
            set { mostrarPosiciónDeCámaraToolStripMenuItem.Checked = value; }
        }

        private void mostrarPosiciónDeCámaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!mostrarPosiciónDeCámaraToolStripMenuItem.Checked)
            {
                toolStripStatusPosition.Text = "";
            }
        }

        /// <summary>
        /// Indica si la aplicacion esta activa.
        /// Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        private bool applicationActive()
        {
            if (this.ContainsFocus) return true;
            foreach (Form form in this.OwnedForms)
            {
                if (form.ContainsFocus) return true;
            }
            return false;
        }


        
    }
}