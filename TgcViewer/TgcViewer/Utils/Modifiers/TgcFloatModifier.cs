using System;
using System.Drawing;
using System.Windows.Forms;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    ///     Modificador para valores Float
    /// </summary>
    public class TgcFloatModifier : TgcModifierPanel
    {
        private readonly float maxValue;
        private readonly float minValue;
        private readonly NumericUpDown numericUpDown;

        private bool numericUpDownChange;
        private readonly TrackBar trackBar;
        private bool trackBarChange;

        public TgcFloatModifier(string varName, float minValue, float maxValue, float defaultValue) : base(varName)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;

            numericUpDown = new NumericUpDown();
            numericUpDown.Size = new Size(100, 20);
            numericUpDown.Margin = new Padding(0);
            numericUpDown.DecimalPlaces = 4;
            numericUpDown.Minimum = (decimal) minValue;
            numericUpDown.Maximum = (decimal) maxValue;
            numericUpDown.Value = (decimal) defaultValue;
            numericUpDown.Increment = (decimal) (2f*(maxValue - minValue)/100f);
            numericUpDown.ValueChanged += numericUpDown_ValueChanged;

            trackBar = new TrackBar();
            trackBar.Size = new Size(100, 20);
            trackBar.Margin = new Padding(0);
            trackBar.Minimum = 0;
            trackBar.Maximum = 20;
            trackBar.Value = (int) ((defaultValue - minValue)*20/(maxValue - minValue));
            trackBar.ValueChanged += trackBar_ValueChanged;

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
            trackBar.Value = (int) (((float) numericUpDown.Value - minValue)*20/(maxValue - minValue));

            GuiController.Instance.focus3dPanel();
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownChange)
            {
                numericUpDownChange = false;
                return;
            }

            trackBarChange = true;
            numericUpDown.Value = (decimal) (minValue + trackBar.Value*(maxValue - minValue)/20);

            GuiController.Instance.focus3dPanel();
        }

        public override object getValue()
        {
            return (float) numericUpDown.Value;
        }
    }
}