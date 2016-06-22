using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Viewer.Model;
using TGC.Viewer.Properties;

namespace TGC.Viewer.UI
{
    /// <summary>
    ///     Formulario principal de la aplicación
    /// </summary>
    public partial class ViewerForm : Form
    {
        /// <summary>
        ///     Constructor principal de la aplicacion
        /// </summary>
        public ViewerForm()
        {
            InitializeComponent();
        }

        private ViewerModel Modelo { get; set; }

        private void InitAplication()
        {
            //Configuracion
            var settings = Settings.Default;

            //Titulo de la ventana principal
            Text = settings.Title;

            fpsToolStripMenuItem.Checked = true;
            axisToolStripMenuItem.Checked = true;

            Modelo = new ViewerModel();

            CheckMediaFolder();

            //Iniciar graficos
            Modelo.InitGraphics(this, treeViewExamples, panel3D, toolStripStatusCurrentExample);

            try
            {
                //Cargo los ejemplos en el arbol
                Modelo.LoadExamples(treeViewExamples, flowLayoutPanelModifiers, dataGridUserVars);
                var defaultExample = Modelo.ExampleLoader.GetExampleByName(settings.DefaultExampleName,
                    settings.DefaultExampleCategory);
                ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Modelo.InitRenderLoop();

            panel3D.Focus();
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

        private void CheckMediaFolder()
        {
            //Verificamos la carpeta Media
            var pathMedia = Environment.CurrentDirectory + "\\" + Settings.Default.MediaDirectory;

            if (!Directory.Exists(pathMedia))
            {
                //modelo.DownloadMediaFolder();
                MessageBox.Show("No se encuentra disponible la carpeta Media en: " + pathMedia + Environment.NewLine +
                                Environment.NewLine +
                                "A continuación se abrira la dirección donde se encuentra la carpeta comprimida.");
                Process.Start(Settings.Default.MediaLink);
            }
        }

        public bool CloseAplication()
        {
            var result = MessageBox.Show("¿Esta seguro que desea cerrar la aplicación?", Text, MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                if (Modelo.ApplicationRunning)
                {
                    Modelo.Dispose();
                }

                return false;
            }

            return true;
        }

        private void ResetMenuValues()
        {
            wireframeToolStripMenuItem.Checked = false;
            fpsToolStripMenuItem.Checked = true;
            axisToolStripMenuItem.Checked = true;
            fullExampleToolStripMenuItem.Checked = false;

            Modelo.Wireframe(wireframeToolStripMenuItem.Checked);
            Modelo.ContadorFPS(fpsToolStripMenuItem.Checked);
            Modelo.AxisLines(axisToolStripMenuItem.Checked);
        }

        private void OpenHelp()
        {
            throw new NotImplementedException();
        }

        private void OpenAbout()
        {
            new AboutForm().ShowDialog(this);
        }

        private void Wireframe()
        {
            Modelo.Wireframe(wireframeToolStripMenuItem.Checked);
        }

        private void ContadorFPS()
        {
            Modelo.ContadorFPS(fpsToolStripMenuItem.Checked);
        }

        private void AxisLines()
        {
            Modelo.AxisLines(axisToolStripMenuItem.Checked);
        }

        private TgcExample GetExample(TreeNode selectedNode)
        {
            return Modelo.ExampleLoader.GetExampleByTreeNode(selectedNode);
        }

        private void ExecuteExample(TgcExample example)
        {
            Modelo.ExecuteExample(example);
            ContadorFPS();
            AxisLines();
            Wireframe();

            toolStripStatusCurrentExample.Text = "Ejemplo actual: " + example.Name;
            panel3D.Focus();
        }

        private void FullExample()
        {
            splitContainerIzquierda.Visible = !splitContainerIzquierda.Visible;
            splitContainerDerecha.Visible = !splitContainerDerecha.Visible;
            statusStrip.Visible = !statusStrip.Visible;
        }

        #region Eventos del form

        private void ViewerForm_Load(object sender, EventArgs e)
        {
            InitAplication();
        }

        private void ViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CloseAplication();
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
                ExecuteExample(GetExample(selectedNode));
            }
        }

        private void fpsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContadorFPS();
        }

        private void axisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AxisLines();
        }

        private void fullExampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FullExample();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Wireframe();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void acercaDeTgcViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenAbout();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetMenuValues();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenHelp();
        }

        #endregion Eventos del form
    }
}