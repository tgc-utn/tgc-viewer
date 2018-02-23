using System;
using System.Windows.Forms;
using TGC.Viewer.Properties;

namespace TGC.Viewer.UI
{
    public partial class OptionForm : Form
    {
        private FolderBrowserDialog folderBrowserDialog;

        public OptionForm()
        {
            InitializeComponent();
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            //Revisar si Settings.Default tiene que estar como esta en ViewerModel y ViewerForm
            this.textBoxShadersDirectory.Text = Settings.Default.ShadersDirectory;
            this.textBoxMediaDirectory.Text = Settings.Default.MediaDirectory;
            this.textBoxCommonShaders.Text = Settings.Default.CommonShaders;
            this.textBoxMediaLink.Text = Settings.Default.MediaLink;
            this.folderBrowserDialog = new FolderBrowserDialog();
        }

        private void buttonShadersDirectory_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxShadersDirectory.Text = folderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonMediaDirectory_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxMediaDirectory.Text = folderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonCommonShaders_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog.ShowDialog().Equals(DialogResult.OK))
            {
                this.textBoxCommonShaders.Text = folderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            Settings.Default.ShadersDirectory = this.textBoxShadersDirectory.Text;
            Settings.Default.MediaDirectory = this.textBoxMediaDirectory.Text;
            Settings.Default.CommonShaders = this.textBoxCommonShaders.Text;
            Settings.Default.MediaLink = this.textBoxMediaLink.Text;
            Settings.Default.Save();
        }

        private void buttonMediaLink_Click(object sender, EventArgs e)
        {
            this.textBoxMediaLink.ReadOnly = !this.textBoxMediaLink.ReadOnly;
        }
    }
}