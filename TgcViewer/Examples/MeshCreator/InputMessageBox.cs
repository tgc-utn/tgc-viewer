using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Un MessageBox con un campo de texto para llenar
    /// </summary>
    public partial class InputMessageBox : Form
    {

        /// <summary>
        /// Label
        /// </summary>
        public string InputLabel
        {
            get { return labelText.Text; }
            set { labelText.Text = value; }
        }

        /// <summary>
        /// Input text
        /// </summary>
        public string InputText
        {
            get { return textBoxText.Text; }
            set { textBoxText.Text = value; }
        }

        public InputMessageBox()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            string t = textBoxText.Text;
            t = t.Trim();
            if (t.Length > 0)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void InputMessageBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

    }
}
