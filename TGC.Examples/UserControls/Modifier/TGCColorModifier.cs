using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para elegir un color
    /// </summary>
    public partial class TGCColorModifier : UserControl
    {
        private TGCColorModifier()
        {
            InitializeComponent();
        }

        public TGCColorModifier(string modifierName, Color defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);
            colorPanel.BackColor = defaultValue;
            colorDialog.Color = defaultValue;
            colorDialog.AnyColor = true;
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
        }

        public Color Value => colorPanel.BackColor;

        private void colorPanel_Click(object sender, EventArgs e)
        {
            colorDialog.ShowDialog();
            colorPanel.BackColor = colorDialog.Color;
        }
    }
}