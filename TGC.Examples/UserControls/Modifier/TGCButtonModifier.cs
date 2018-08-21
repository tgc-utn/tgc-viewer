using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador con un Boton
    /// </summary>
    public partial class TGCButtonModifier : UserControl
    {
        private TGCButtonModifier()
        {
            InitializeComponent();
        }

        public TGCButtonModifier(string modifierName, string text, EventHandler clickEventHandler) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);
            button.Text = text;
            button.Click += clickEventHandler;
        }
    }
}