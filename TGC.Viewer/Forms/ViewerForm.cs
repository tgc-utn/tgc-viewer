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
            Text = Settings.Default.Title;

            modelo = new ViewerModel();

            //Iniciar graficos
            modelo.InitGraphics(this, treeViewExamples, panel3d, flowLayoutPanelModifiers, dataGridUserVars,
                toolStripStatusCurrentExample);
            modelo.InitRenderLoop();

            //Focus panel3D
            panel3d.Focus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            modelo.ShutDown();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modelo.WireFrame(wireframeToolStripMenuItem.Checked);
        }

        private void treeViewExamples_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeViewExamples.SelectedNode != null)
            {
                var selectedNode = treeViewExamples.SelectedNode;
                textBoxExampleDescription.Text = selectedNode.ToolTipText;
            }
        }

        private void treeViewExamples_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedNode = treeViewExamples.SelectedNode;

            if (selectedNode != null && selectedNode.Nodes.Count == 0)
            {
                modelo.ExecuteExample(selectedNode);
            }
        }

        private void contadorFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modelo.ContadorFPS(contadorFPSToolStripMenuItem.Checked);
        }

        private void ejesCartesianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modelo.AxisLines(ejesCartesianosToolStripMenuItem.Checked);
        }

        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Setea los valores default de las opciones del menu
        /// </summary>
        public void ResetMenuOptions()
        {
            wireframeToolStripMenuItem.Checked = false;
            contadorFPSToolStripMenuItem.Checked = true;
            ejesCartesianosToolStripMenuItem.Checked = true;

            modelo.WireFrame(wireframeToolStripMenuItem.Checked);
            modelo.ContadorFPS(contadorFPSToolStripMenuItem.Checked);
            modelo.AxisLines(ejesCartesianosToolStripMenuItem.Checked);
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