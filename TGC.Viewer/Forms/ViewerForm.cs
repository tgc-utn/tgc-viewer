using System;
using System.Windows.Forms;
using TGC.Viewer.Model;
using TGC.Viewer.Properties;

namespace TGC.Viewer.Forms
{
    /// <summary>
    ///     Formulario principal de la aplicación
    /// </summary>
    public partial class ViewerForm : Form
    {
        private ViewerModel modelo;

        /// <summary>
        ///     Constructor principal de la aplicacion
        /// </summary>
        public ViewerForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Titulo de la ventana principal
            this.Text = Settings.Default.Title;

            this.modelo = new ViewerModel();

            //Iniciar graficos
            this.modelo.InitGraphics(this, this.treeViewExamples, this.panel3d, this.flowLayoutPanelModifiers, this.dataGridUserVars, this.toolStripStatusCurrentExample);
            this.modelo.InitRenderLoop();

            //Focus panel3D
            this.panel3d.Focus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.modelo.ShutDown();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.modelo.WireFrame(wireframeToolStripMenuItem.Checked);
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
                this.modelo.ExecuteExample(selectedNode);
            }
        }

        private void contadorFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.modelo.ContadorFPS(this.contadorFPSToolStripMenuItem.Checked);
        }

        private void ejesCartesianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.modelo.AxisLines(this.ejesCartesianosToolStripMenuItem.Checked);
        }

        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///     Setea los valores default de las opciones del menu
        /// </summary>
        public void ResetMenuOptions()
        {
            this.wireframeToolStripMenuItem.Checked = false;
            this.contadorFPSToolStripMenuItem.Checked = true;
            this.ejesCartesianosToolStripMenuItem.Checked = true;

            this.modelo.WireFrame(this.wireframeToolStripMenuItem.Checked);
            this.modelo.ContadorFPS(this.contadorFPSToolStripMenuItem.Checked);
            this.modelo.AxisLines(this.ejesCartesianosToolStripMenuItem.Checked);
        }

        /// <summary>
        ///     Indica si la aplicacion esta activa.
        ///     Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
        /// </summary>
        public bool ApplicationActive()
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
    }
}