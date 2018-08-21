using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para un intervalo discreto de valores
    /// </summary>
    public partial class TGCIntervalModifier : UserControl
    {
        public TGCIntervalModifier()
        {
            InitializeComponent();
        }

        public TGCIntervalModifier(string modifierName, object[] values, int defaultIndex) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            SelectedIndex = defaultIndex;

            comboBox.Items.AddRange(values);
            comboBox.SelectedIndex = SelectedIndex;
            comboBox.SelectionChangeCommitted += comboBox_SelectionChangeCommitted;
        }

        public object Value => comboBox.Items[SelectedIndex];
        private int SelectedIndex { get; set; }

        /// <summary>
        ///     Cuando el valor del combo cambia y fue confirmado realmente
        /// </summary>
        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectedIndex = comboBox.SelectedIndex;
        }
    }
}