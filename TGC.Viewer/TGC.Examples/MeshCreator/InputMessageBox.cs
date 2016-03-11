using System;
using System.Windows.Forms;

namespace TGC.Examples.MeshCreator
{
    /// <summary>
    ///     Un MessageBox con un campo de texto para llenar
    /// </summary>
    public partial class InputMessageBox : Form
    {
        public InputMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Label
        /// </summary>
        public string InputLabel
        {
            get { return labelText.Text; }
            set { labelText.Text = value; }
        }

        /// <summary>
        ///     Input text
        /// </summary>
        public string InputText
        {
            get { return textBoxText.Text; }
            set { textBoxText.Text = value; }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var t = textBoxText.Text;
            t = t.Trim();
            if (t.Length > 0)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InputMessageBox_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void InputMessageBox_Load(object sender, EventArgs e)
        {
            textBoxText.Focus();
            textBoxText.SelectAll();
        }

        private void textBoxText_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Enter
            if (e.KeyChar == (char)13)
            {
                buttonOk_Click(null, null);
            }
        }
    }
}