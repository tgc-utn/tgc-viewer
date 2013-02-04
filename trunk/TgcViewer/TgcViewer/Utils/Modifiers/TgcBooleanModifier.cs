using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Modificador para valores Boolean
    /// </summary>
    public class TgcBooleanModifier : TgcModifierPanel
    {

        CheckBox checkbox;

        public TgcBooleanModifier(string varName, string text, bool defaultValue)
            : base(varName)
        {
            checkbox = new CheckBox();
            checkbox.Checked = defaultValue;
            checkbox.Text = text;
            checkbox.Margin = new Padding(0);

            contentPanel.Controls.Add(checkbox);
        }

        
        public override object getValue()
        {
            return (bool)checkbox.Checked;
        }
    }
}
