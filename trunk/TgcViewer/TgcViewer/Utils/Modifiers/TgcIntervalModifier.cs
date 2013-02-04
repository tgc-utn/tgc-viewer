using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Modificador para un intervalo discreto de valores
    /// </summary>
    public class TgcIntervalModifier : TgcModifierPanel
    {
        ComboBox comboBox;
        int selectedIndex;

        public TgcIntervalModifier(string varName, object[] values, int defaultIndex)
            : base(varName)
        {
            selectedIndex = defaultIndex;

            comboBox = new ComboBox();
            comboBox.Margin = new Padding(0);
            comboBox.Size = new System.Drawing.Size(100, 20);
            comboBox.Items.AddRange(values);
            comboBox.SelectedIndex = selectedIndex;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.SelectionChangeCommitted += new EventHandler(comboBox_SelectionChangeCommitted);

            contentPanel.Controls.Add(comboBox);
        }

        /// <summary>
        /// Cuando el valor del combo cambia y fue confirmado realmente
        /// </summary>
        void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            selectedIndex = comboBox.SelectedIndex;
        }

        public override object getValue()
        {
            return comboBox.Items[selectedIndex];
        }
    }
}
