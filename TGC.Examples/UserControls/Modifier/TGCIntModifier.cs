using System;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores Int
    /// </summary>
    public partial class TGCIntModifier : UserControl
    {
        private TGCIntModifier()
        {
            InitializeComponent();
        }

        public TGCIntModifier(string modifierName, int minValue, int maxValue, int defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            MinValue = minValue;
            MaxValue = maxValue;

            numericUpDown.Minimum = minValue;
            numericUpDown.Maximum = maxValue;
            numericUpDown.Value = defaultValue;
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;

            trackBar.Minimum = 0;

            if (maxValue - minValue > 10)
            {
                numericUpDown.Increment = (maxValue - minValue) / 10;
                trackBar.Value = defaultValue * 10 / maxValue;
                trackBar.Maximum = 10;
            }
            else
            {
                numericUpDown.Increment = 1;
                trackBar.Value = defaultValue - minValue;
                trackBar.Maximum = maxValue - minValue;
            }

            trackBar.ValueChanged += trackBar_ValueChanged;
        }

        private int MinValue { get; }
        private int MaxValue { get; }
        private bool NumericUpDownChange { get; set; }
        private bool TrackBarChange { get; set; }
        public int Value => (int)numericUpDown.Value;

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (TrackBarChange)
            {
                TrackBarChange = false;
                return;
            }

            NumericUpDownChange = true;
            if (MaxValue - MinValue > 10)
                trackBar.Value = (int)numericUpDown.Value * 10 / MaxValue;
            else
                trackBar.Value = (int)numericUpDown.Value - MinValue;
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (NumericUpDownChange)
            {
                NumericUpDownChange = false;
                return;
            }

            TrackBarChange = true;

            if (MaxValue - MinValue > 10)
                if (trackBar.Value > MinValue)
                    numericUpDown.Value = trackBar.Value * MaxValue / 10;
                else
                    numericUpDown.Value = MinValue;
            else
                numericUpDown.Value = MinValue + trackBar.Value;
        }
    }
}