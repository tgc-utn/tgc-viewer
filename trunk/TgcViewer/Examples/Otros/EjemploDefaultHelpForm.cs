using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Examples.Otros
{
    public partial class EjemploDefaultHelpForm : Form
    {
        public EjemploDefaultHelpForm(string helpRtf)
        {
            InitializeComponent();

            this.richTextBoxHelp.Rtf = helpRtf;
        }
    }
}
