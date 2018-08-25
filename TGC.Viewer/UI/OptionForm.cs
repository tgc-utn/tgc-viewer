using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TGC.Viewer.Properties;

namespace TGC.Viewer.UI
{
    public partial class OptionForm : Form
    {
        private Settings Settings { get; set; }
        private FolderBrowserDialog FolderBrowserDialog { get; set; }

        public OptionForm()
        {
            //Revisar si Settings.Default tiene que estar como esta en ViewerModel y ViewerForm
            this.Settings = Settings.Default;
            InitializeComponent();
        }

        /// <summary>
        /// Abre la la carpeta si es una ruta, si no existe abre la carpeta actual del proyecto.
        /// </summary>
        /// <param name="path"> Ruta que se quiere abrir.</param>
        private void ProcessStart(string path)
        {
            if (this.CheckFolder(path))
            {
                Process.Start(Environment.CurrentDirectory);
            }
            else
            {
                Process.Start(path);
            }
        }

        /// <summary>
        /// Verifica si existe la carpeta.
        /// </summary>
        /// <param name="pathMedia"></param>
        private bool CheckFolder(string path)
        {
            return !Directory.Exists(path);
        }

        /// <summary>
        /// Verifica que existan las carpetas necesarias para la aplicacion.
        /// </summary>
        /// <returns></returns>
        private bool CheckAllFolders()
        {
            //TODO mejorar esta valicacion ya que si esta la carpeta pero no los shaders/media necesarios pasaria lo mismo.
            return this.CheckFolder(this.textBoxMediaDirectory.Text) || this.CheckFolder(this.textBoxShadersDirectory.Text)
                || this.CheckFolder(this.textBoxShadersDirectory.Text + this.textBoxCommonShaders.Text);
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            this.textBoxShadersDirectory.Text = this.Settings.ShadersDirectory;
            this.textBoxMediaDirectory.Text = this.Settings.MediaDirectory;
            this.textBoxCommonShaders.Text = this.Settings.CommonShaders;
            this.textBoxMediaLink.Text = this.Settings.MediaLink;
            this.FolderBrowserDialog = new FolderBrowserDialog();
        }

        private void buttonShadersDirectory_Click(object sender, EventArgs e)
        {
            if (this.FolderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxShadersDirectory.Text = FolderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonMediaDirectory_Click(object sender, EventArgs e)
        {
            if (this.FolderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxMediaDirectory.Text = FolderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonCommonShaders_Click(object sender, EventArgs e)
        {
            if (this.FolderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxCommonShaders.Text = FolderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            if (this.CheckAllFolders())
            {
                this.DialogResult = DialogResult.None;
                return;
            }

            this.Settings.ShadersDirectory = this.textBoxShadersDirectory.Text;
            this.Settings.MediaDirectory = this.textBoxMediaDirectory.Text;
            this.Settings.CommonShaders = this.textBoxCommonShaders.Text;
            this.Settings.MediaLink = this.textBoxMediaLink.Text;
            this.Settings.Save();
        }

        private void buttonMediaLink_Click(object sender, EventArgs e)
        {
            this.textBoxMediaLink.ReadOnly = !this.textBoxMediaLink.ReadOnly;
        }

        private void buttonOpenMediaLink_Click(object sender, EventArgs e)
        {
            Process.Start(this.textBoxMediaLink.Text);
        }

        private void buttonOpenCommonShaders_Click(object sender, EventArgs e)
        {
            this.ProcessStart(this.textBoxShadersDirectory.Text + this.textBoxCommonShaders.Text);
        }

        private void buttonOpenShaders_Click(object sender, EventArgs e)
        {
            this.ProcessStart(this.textBoxShadersDirectory.Text);
        }

        private void buttonOpenMedia_Click(object sender, EventArgs e)
        {
            this.ProcessStart(this.textBoxMediaDirectory.Text);
        }
    }
}