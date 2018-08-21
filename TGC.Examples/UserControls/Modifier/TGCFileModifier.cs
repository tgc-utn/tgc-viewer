using System;
using System.IO;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para elegir un archivo del FileSystem
    /// </summary>
    public partial class TGCFileModifier : UserControl
    {
        private TGCFileModifier()
        {
            InitializeComponent();
        }

        public TGCFileModifier(string modifierName, string defaultPath, string fileFilter) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            DefaultPath = defaultPath;

            fileNameTextBox.Text = new FileInfo(defaultPath).Name;
            filePathTextBox.Text = defaultPath;

            openFileDialog.Filter = fileFilter;
            openFileDialog.FileName = defaultPath;
        }

        private string DefaultPath { get; }
        public string Value => filePathTextBox.Text;

        private void fileButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                filePathTextBox.Text = openFileDialog.FileName;
            else
                filePathTextBox.Text = DefaultPath;
        }
    }
}