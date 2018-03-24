using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para un intervalo discreto de valores creados con una estructura Enum
    /// </summary>
    public partial class TGCEnumModifier : UserControl
    {
        private TGCEnumModifier()
        {
            InitializeComponent();
        }

        public TGCEnumModifier(string modifierName, Type enumType, object defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            EnumType = enumType;
            SelectValue = defaultValue;

            //Buscar indice del defaultValue dentro del enum
            var enumNames = Enum.GetNames(enumType);
            var currentName = Enum.GetName(enumType, defaultValue);
            var selectedIndex = -1;

            //TODO revisar esta logica si es necesaria.
            for (var i = 0; i < enumNames.Length; i++)
                if (currentName.Equals(enumNames[i]))
                {
                    selectedIndex = i;
                    break;
                }

            comboBox.Items.AddRange(enumNames);
            comboBox.SelectedIndex = selectedIndex;
        }

        private Type EnumType { get; }
        private object SelectValue { get; set; }

        public object Value => SelectValue;

        /// <summary>
        ///     Cuando el valor del combo cambia y fue confirmado realmente
        /// </summary>
        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedIndex = comboBox.SelectedIndex;
            var itemString = comboBox.Items[selectedIndex].ToString();
            SelectValue = (int)Enum.Parse(EnumType, itemString);
        }
    }
}