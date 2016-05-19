using System.Windows.Forms;

namespace TGC.Examples.Others
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