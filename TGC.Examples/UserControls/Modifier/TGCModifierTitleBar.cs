using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    public partial class TGCModifierTitleBar : UserControl
    {
        public TGCModifierTitleBar()
        {
            InitializeComponent();
        }

        //TODO reemplazar este get por un metodo que haga lo que los controlers necesiten
        private Panel ContentPanel { get; set; }

        public void setContentPanel(Panel contentPanel)
        {
            ContentPanel = contentPanel;
        }

        public void setModifierName(string modifierName)
        {
            title.Text = modifierName;
        }

        private void showHideButton_Click(object sender, EventArgs e)
        {
            if (ContentPanel != null)
            {
                ContentPanel.Visible = !ContentPanel.Visible;
                showHideButton.Text = ContentPanel.Visible ? "-" : "+";
            }
        }
    }
}