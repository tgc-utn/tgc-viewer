using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Core.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores Int
    /// </summary>
    public class TgcIntModifier : TgcModifierPanel
    {
        private readonly int maxValue;
        private readonly int minValue;
        private readonly NumericUpDown numericUpDown;
        private readonly TrackBar trackBar;

        private bool numericUpDownChange;
        private bool trackBarChange;

        public TgcIntModifier(string varName, int minValue, int maxValue, int defaultValue) : base(varName)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;

            numericUpDown = new NumericUpDown();
            numericUpDown.Size = new Size(100, 20);
            numericUpDown.Margin = new Padding(0);
            numericUpDown.Minimum = minValue;
            numericUpDown.Maximum = maxValue;
            numericUpDown.Value = defaultValue;
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;

            trackBar = new TrackBar();
            trackBar.Size = new Size(100, 20);
            trackBar.Margin = new Padding(0);
            trackBar.Minimum = 0;
            trackBar.ValueChanged += trackBar_ValueChanged;

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

            contentPanel.Controls.Add(numericUpDown);
            contentPanel.Controls.Add(trackBar);
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarChange)
            {
                trackBarChange = false;
                return;
            }

            numericUpDownChange = true;
            if (maxValue - minValue > 10)
            {
                trackBar.Value = (int)numericUpDown.Value * 10 / maxValue;
            }
            else
            {
                trackBar.Value = (int)numericUpDown.Value - minValue;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownChange)
            {
                numericUpDownChange = false;
                return;
            }

            trackBarChange = true;

            if (maxValue - minValue > 10)
            {
                if (trackBar.Value > minValue)
                    numericUpDown.Value = trackBar.Value * maxValue / 10;
                else
                    numericUpDown.Value = minValue;
            }
            else
            {
                numericUpDown.Value = minValue + trackBar.Value;
            }
        }

        public override object getValue()
        {
            return (int)numericUpDown.Value;
        }
    }
}