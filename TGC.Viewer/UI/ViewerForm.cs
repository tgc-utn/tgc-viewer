using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Examples.Others;
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

            //Generic TGC shaders
            var commonShaders = Settings.Default.ShadersDirectory + Settings.Default.CommonShaders;

            if (!Directory.Exists(commonShaders))
            {
                //TODO mejorar esta valicacion ya que si esta la carpeta pero no los shaders necesarios pasaria lo mismo.
                MessageBox.Show("Debe configurar correctamente el directorio con los shaders comunes y reiniciar la aplicación.", "No se encontro la carpeta de shaders", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Iniciar graficos
            Modelo.InitGraphics(this, panel3D, commonShaders);

            try
            {
                //Cargo los ejemplos en el arbol
                Modelo.LoadExamples(treeViewExamples, panelModifiers, dataGridUserVars, settings.MediaDirectory, settings.ShadersDirectory);
                var defaultExample = Modelo.ExampleLoader.GetExampleByName(settings.DefaultExampleName, settings.DefaultExampleCategory);
                ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo " + settings.DefaultExampleName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Modelo.InitRenderLoop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error en RenderLoop del ejemplo: " + Modelo.ExampleLoader.CurrentExample.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
            var pathMedia = Settings.Default.MediaDirectory;

            if (!Directory.Exists(pathMedia))
            {
                //modelo.DownloadMediaFolder();
                Process.Start(Settings.Default.MediaLink);
                Process.Start(Environment.CurrentDirectory);
                MessageBox.Show("No se encuentra disponible la carpeta Media en: " + pathMedia + Environment.NewLine + Environment.NewLine + "A continuación se abrira la dirección donde se encuentra la carpeta comprimida.");

                //Fuerzo el cierre de la aplicacion.
                Environment.Exit(0);
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
            splitContainerIzquierda.Visible = true;
            splitContainerDerecha.Visible = true;
            statusStrip.Visible = true;

            Modelo.UpdateAspectRatio(panel3D);
            Modelo.Wireframe(wireframeToolStripMenuItem.Checked);
            Modelo.ContadorFPS(fpsToolStripMenuItem.Checked);
            Modelo.AxisLines(axisToolStripMenuItem.Checked);
        }

        private void OpenHelp()
        {
            //Help form
            var helpRtf = File.ReadAllText(Settings.Default.MediaDirectory + "\\help.rtf");
            new EjemploDefaultHelpForm(helpRtf).ShowDialog();
        }

        private void OpenAbout()
        {
            new AboutForm().ShowDialog(this);
        }

        private void OpenOption()
        {
            new OptionForm().ShowDialog(this);
            Modelo.UpdateMediaAndShaderDirectories(Settings.Default.MediaDirectory, Settings.Default.ShadersDirectory);
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
            try
            {
                Modelo.ExecuteExample(example);
                ContadorFPS();
                AxisLines();
                Wireframe();

                toolStripStatusCurrentExample.Text = "Ejemplo actual: " + example.Name;
                panel3D.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo " + example.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);

                //TODO mejorar esta llamada con un metodo de model
                Modelo.ExampleLoader.CurrentExample = null;
            }
        }

        private void FullExample()
        {
            splitContainerIzquierda.Visible = !splitContainerIzquierda.Visible;
            splitContainerDerecha.Visible = !splitContainerDerecha.Visible;
            statusStrip.Visible = !statusStrip.Visible;

            Modelo.UpdateAspectRatio(panel3D);
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

        private void opcionesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenOption();
        }

        #endregion Eventos del form
    }
}