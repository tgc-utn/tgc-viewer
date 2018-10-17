using System;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Examples.Others;
using TGC.Viewer.Model;
using TGC.Viewer.Properties;

namespace TGC.Viewer.UI
{
    /// <summary>
    /// Formulario principal de la aplicación.
    /// </summary>
    public partial class ViewerForm : Form
    {
        /// <summary>
        /// Constructor principal de la aplicacion.
        /// </summary>
        public ViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Modelo del Viewer.
        /// </summary>
        private ViewerModel Model { get; set; }

        /// <summary>
        /// Inicializacion de los componentes principales y carga de ejemplos.
        /// </summary>
        private void InitAplication()
        {
            //Archivo de configuracion
            var settings = Settings.Default;

            //Titulo de la ventana principal
            Text = settings.Title;

            //Herramientas basicas.
            fpsToolStripMenuItem.Checked = true;
            axisToolStripMenuItem.Checked = true;

            //Modelo de la aplicacion
            Model = new ViewerModel();

            //Verificamos la carpeta Media y la de TGC shaders basicos
            if (Model.CheckFolder(settings.MediaDirectory) || Model.CheckFolder(settings.CommonShaders))
            {
                if (OpenOption() == DialogResult.Cancel)
                {
                    //Fuerzo el cierre de la aplicacion.
                    Environment.Exit(0);
                }
            }

            //Iniciar graficos
            Model.InitGraphics(this, panel3D, settings.CommonShaders);

            try
            {
                //Cargo los ejemplos en el arbol
                Model.LoadExamples(treeViewExamples, panelModifiers, dataGridUserVars, settings.MediaDirectory, settings.ShadersDirectory);
                var defaultExample = Model.GetExampleByName(settings.DefaultExampleName, settings.DefaultExampleCategory);
                ExecuteExample(defaultExample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo " + settings.DefaultExampleName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Model.InitRenderLoop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error en RenderLoop del ejemplo: " + Model.CurrentExample().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panel3D.Focus();
        }

        /// <summary>
        /// Indica si la aplicacion esta activa.
        /// Busca si la ventana principal tiene foco o si alguna de sus hijas tiene.
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

        /// <summary>
        /// Dialogo de confirmacion para cerrar la aplicacion.
        /// </summary>
        /// <returns> Si hay o no la aplicacion.</returns>
        public bool CloseAplication()
        {
            var result = MessageBox.Show("¿Esta seguro que desea cerrar la aplicación?", Text, MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                if (Model.ApplicationRunning)
                {
                    Model.Dispose();
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Vuelve al estado inicial los valores del menu.
        /// </summary>
        private void ResetMenuValues()
        {
            wireframeToolStripMenuItem.Checked = false;
            fpsToolStripMenuItem.Checked = true;
            axisToolStripMenuItem.Checked = true;
            fullExampleToolStripMenuItem.Checked = false;
            splitContainerIzquierda.Visible = true;
            splitContainerDerecha.Visible = true;
            statusStrip.Visible = true;

            Model.UpdateAspectRatio(panel3D);
            Model.Wireframe(wireframeToolStripMenuItem.Checked);
            Model.ContadorFPS(fpsToolStripMenuItem.Checked);
            Model.AxisLines(axisToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Abre un dialogo con ayuda.
        /// </summary>
        private void OpenHelp()
        {
            //Help form
            var helpRtf = File.ReadAllText(Settings.Default.MediaDirectory + "\\help.rtf");
            new EjemploDefaultHelpForm(helpRtf).ShowDialog();
        }

        /// <summary>
        /// Abre un dialogo con informacion de la aplicacion.
        /// </summary>
        private void OpenAbout()
        {
            new AboutForm().ShowDialog(this);
        }

        /// <summary>
        /// Abre las opciones de la aplicacion.
        /// </summary>
        private DialogResult OpenOption()
        {
            OptionForm option = new OptionForm();
            DialogResult result = option.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                Model.UpdateMediaAndShaderDirectories(Settings.Default.MediaDirectory, Settings.Default.ShadersDirectory);
            }

            return result;
        }

        /// <summary>
        /// Activa o desactiva la opcion de wireframe en el ejemplo.
        /// </summary>
        private void Wireframe()
        {
            Model.Wireframe(wireframeToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Activa o desactiva la opcion del contador de fps.
        /// </summary>
        private void ContadorFPS()
        {
            Model.ContadorFPS(fpsToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Activa o desactiva la opcion de los ejes cartesianos.
        /// </summary>
        private void AxisLines()
        {
            Model.AxisLines(axisToolStripMenuItem.Checked);
        }

        /// <summary>
        /// Ejecuta un ejemplo particular.
        /// </summary>
        /// <param name="example">Ejemplo a ejecutar.</param>
        private void ExecuteExample(TgcExample example)
        {
            try
            {
                Model.ExecuteExample(example);
                ContadorFPS();
                AxisLines();
                Wireframe();

                toolStripStatusCurrentExample.Text = "Ejemplo actual: " + example.Name;
                panel3D.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se pudo cargar el ejemplo " + example.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);

                Model.ClearCurrentExample();
            }
        }

        /// <summary>
        /// Ejecuta el ejemplo en pantalla completa.
        /// </summary>
        private void FullExample()
        {
            splitContainerIzquierda.Visible = !splitContainerIzquierda.Visible;
            splitContainerDerecha.Visible = !splitContainerDerecha.Visible;
            statusStrip.Visible = !statusStrip.Visible;

            Model.UpdateAspectRatio(panel3D);
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
                ExecuteExample(Model.GetExampleByTreeNode(selectedNode));
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