using System.Windows.Forms;

namespace TGC.Viewer.Utils.Modifiers
{
    /// <summary>
    ///     Modificador para valores Boolean
    /// </summary>
    public class TgcBooleanModifier : TgcModifierPanel
    {
        private readonly CheckBox checkbox;

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
            return checkbox.Checked;
        }
    }
}