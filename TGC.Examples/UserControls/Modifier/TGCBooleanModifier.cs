using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores Boolean
    /// </summary>
    public partial class TGCBooleanModifier : UserControl
    {
        private TGCBooleanModifier()
        {
            InitializeComponent();
        }

        public TGCBooleanModifier(string modifierName, string text, bool defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);
            checkBox.Checked = defaultValue;
            checkBox.Text = text;
        }

        public bool Value => checkBox.Checked;
    }
}