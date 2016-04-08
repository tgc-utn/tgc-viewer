using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Viewer.Utils.Modifiers
{
    /// <summary>
    ///     Modificador para un intervalo discreto de valores creados con una estructura Enum
    /// </summary>
    public class TgcEnumModifier : TgcModifierPanel
    {
        private readonly ComboBox comboBox;
        private readonly Type enumType;
        private object selectValue;

        public TgcEnumModifier(string varName, Type enumType, object defaultValue)
            : base(varName)
        {
            this.enumType = enumType;
            selectValue = defaultValue;

            //Buscar indice del defaultValue dentro del enum
            var enumNames = Enum.GetNames(enumType);
            var currentName = Enum.GetName(enumType, defaultValue);
            var selectedIndex = -1;
            for (var i = 0; i < enumNames.Length; i++)
            {
                if (currentName.Equals(enumNames[i]))
                {
                    selectedIndex = i;
                    break;
                }
            }

            comboBox = new ComboBox();
            comboBox.Margin = new Padding(0);
            comboBox.Size = new Size(100, 20);
            comboBox.Items.AddRange(enumNames);
            comboBox.SelectedIndex = selectedIndex;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.SelectionChangeCommitted += comboBox_SelectionChangeCommitted;

            contentPanel.Controls.Add(comboBox);
        }

        /// <summary>
        ///     Cuando el valor del combo cambia y fue confirmado realmente
        /// </summary>
        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedIndex = comboBox.SelectedIndex;
            var itemString = (string)comboBox.Items[selectedIndex];
            selectValue = Enum.Parse(enumType, itemString);
        }

        public override object getValue()
        {
            return selectValue;
        }
    }
}