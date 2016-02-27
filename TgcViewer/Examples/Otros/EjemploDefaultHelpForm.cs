using System.Windows.Forms;

namespace Examples.Otros
{
    public partial class EjemploDefaultHelpForm : Form
    {
        public EjemploDefaultHelpForm(string helpRtf)
        {
            InitializeComponent();

            richTextBoxHelp.Rtf = helpRtf;
        }
    }
}