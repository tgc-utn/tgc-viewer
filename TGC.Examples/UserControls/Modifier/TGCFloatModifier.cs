using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores Float
    /// </summary>
    public partial class TGCFloatModifier : UserControl
    {
        private TGCFloatModifier()
        {
            InitializeComponent();
        }

        public TGCFloatModifier(string modifierName, float minValue, float maxValue, float defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            MinValue = minValue;
            MaxValue = maxValue;

            numericUpDown.Minimum = (decimal)minValue;
            numericUpDown.Maximum = (decimal)maxValue;
            numericUpDown.Value = (decimal)defaultValue;
            numericUpDown.Increment = (decimal)(2f * (maxValue - minValue) / 100f);
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;

            trackBar.Minimum = 0;
            trackBar.Maximum = 20;
            trackBar.Value = (int)((defaultValue - minValue) * 20 / (maxValue - minValue));
            trackBar.ValueChanged += trackBar_ValueChanged;
        }

        private float MaxValue { get; }
        private float MinValue { get; }
        private bool NumericUpDownChange { get; set; }
        private bool TrackBarChange { get; set; }
        public float Value => (float)numericUpDown.Value;

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (TrackBarChange)
            {
                TrackBarChange = false;
                return;
            }

            NumericUpDownChange = true;
            trackBar.Value = (int)(((float)numericUpDown.Value - MinValue) * 20 / (MaxValue - MinValue));
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (NumericUpDownChange)
            {
                NumericUpDownChange = false;
                return;
            }

            TrackBarChange = true;
            numericUpDown.Value = (decimal)(MinValue + trackBar.Value * (MaxValue - MinValue) / 20);
        }
    }
}