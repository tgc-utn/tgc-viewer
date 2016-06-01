using System;
using System.IO;
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
        private ViewerModel Modelo { get; set; }

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

            Modelo = new ViewerModel();

            CheckMediaFolder();

            //Iniciar graficos
            Modelo.InitGraphics(this, treeViewExamples, panel3d, flowLayoutPanelModifiers, dataGridUserVars, toolStripStatusCurrentExample);
            Modelo.InitRenderLoop();

            //Focus panel3D
            panel3d.Focus();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Modelo.ApplicationRunning)
            {
                Modelo.ShutDown();
            }
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Modelo.WireFrame(wireframeToolStripMenuItem.Checked);
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
                Modelo.ExecuteExample(selectedNode);
            }
        }

        private void contadorFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Modelo.ContadorFPS(contadorFPSToolStripMenuItem.Checked);
        }

        private void ejesCartesianosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Modelo.AxisLines(ejesCartesianosToolStripMenuItem.Checked);
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

            Modelo.WireFrame(wireframeToolStripMenuItem.Checked);
            Modelo.ContadorFPS(contadorFPSToolStripMenuItem.Checked);
            Modelo.AxisLines(ejesCartesianosToolStripMenuItem.Checked);
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

        public void CheckMediaFolder()
        {
            //Verificamos la carpeta Media
            string pathMedia = Environment.CurrentDirectory + "\\" + Settings.Default.MediaDirectory;

            if (!Directory.Exists(pathMedia))
            {
                //modelo.DownloadMediaFolder();
                MessageBox.Show("No se encuentra disponible la carpeta Media en: " + pathMedia + Environment.NewLine + Environment.NewLine +
                    "A continuación se abrira la dirección donde se encuentra la carpeta comprimida.");
                System.Diagnostics.Process.Start(Settings.Default.MediaLink);
            }
        }
    }
}