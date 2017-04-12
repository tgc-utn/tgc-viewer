using Microsoft.DirectX;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Mathematica;

namespace TGC.Core.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores floats (X,Y) o (U,V) de un vertice
    /// </summary>
    public class TgcVertex2fModifier : TgcModifierPanel
    {
        private readonly NumericUpDown numericUpDownX;
        private readonly NumericUpDown numericUpDownY;
        private readonly TrackBar trackBarX;
        private readonly TrackBar trackBarY;
        private readonly FlowLayoutPanel vertexValuesPanel;
        private TGCVector2 maxValue;
        private TGCVector2 minValue;

        private bool numericUpDownChangeX;
        private bool numericUpDownChangeY;

        private TGCVector2 result = new TGCVector2();
        private bool trackBarChangeX;
        private bool trackBarChangeY;

        public TgcVertex2fModifier(string varName, TGCVector2 minValue, TGCVector2 maxValue, TGCVector2 defaultValue)
            : base(varName)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;

            //numericUpDownX
            numericUpDownX = new NumericUpDown();
            numericUpDownX.Size = new Size(50, 20);
            numericUpDownX.Margin = new Padding(0);
            numericUpDownX.DecimalPlaces = 4;
            numericUpDownX.Minimum = (decimal)minValue.X;
            numericUpDownX.Maximum = (decimal)maxValue.X;
            numericUpDownX.Value = (decimal)defaultValue.X;
            numericUpDownX.Increment = (decimal)(2f * (maxValue.X - minValue.X) / 100f);
            numericUpDownX.ValueChanged += numericUpDownX_ValueChanged;

            //numericUpDownY
            numericUpDownY = new NumericUpDown();
            numericUpDownY.Size = new Size(50, 20);
            numericUpDownY.Margin = new Padding(0);
            numericUpDownY.DecimalPlaces = 4;
            numericUpDownY.Minimum = (decimal)minValue.Y;
            numericUpDownY.Maximum = (decimal)maxValue.Y;
            numericUpDownY.Value = (decimal)defaultValue.Y;
            numericUpDownY.Increment = (decimal)(2f * (maxValue.Y - minValue.Y) / 100f);
            numericUpDownY.ValueChanged += numericUpDownY_ValueChanged;

            //Panel para los dos numericUpDown
            vertexValuesPanel = new FlowLayoutPanel();
            vertexValuesPanel.Margin = new Padding(0);
            vertexValuesPanel.AutoSize = true;
            vertexValuesPanel.FlowDirection = FlowDirection.LeftToRight;

            vertexValuesPanel.Controls.Add(numericUpDownX);
            vertexValuesPanel.Controls.Add(numericUpDownY);

            //trackBarX
            trackBarX = new TrackBar();
            trackBarX.Size = new Size(100, 20);
            trackBarX.Margin = new Padding(0);
            trackBarX.Minimum = 0;
            trackBarX.Maximum = 20;
            trackBarX.Value = (int)((defaultValue.X - minValue.X) * 20 / (maxValue.X - minValue.X));
            trackBarX.ValueChanged += trackBarX_ValueChanged;

            //trackBarY
            trackBarY = new TrackBar();
            trackBarY.Size = new Size(100, 20);
            trackBarY.Margin = new Padding(0);
            trackBarY.Minimum = 0;
            trackBarY.Maximum = 20;
            trackBarY.Value = (int)((defaultValue.Y - minValue.Y) * 20 / (maxValue.Y - minValue.Y));
            trackBarY.ValueChanged += trackBarY_ValueChanged;

            contentPanel.Controls.Add(vertexValuesPanel);
            contentPanel.Controls.Add(trackBarX);
            contentPanel.Controls.Add(trackBarY);
        }

        private void numericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarChangeX)
            {
                trackBarChangeX = false;
                return;
            }

            numericUpDownChangeX = true;
            trackBarX.Value = (int)(((float)numericUpDownX.Value - minValue.X) * 20 / (maxValue.X - minValue.X));

            // TODO GuiController.Instance.focus3dPanel();
        }

        private void numericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarChangeY)
            {
                trackBarChangeY = false;
                return;
            }

            numericUpDownChangeY = true;
            trackBarY.Value = (int)(((float)numericUpDownY.Value - minValue.Y) * 20 / (maxValue.Y - minValue.Y));

            // TODO GuiController.Instance.focus3dPanel();
        }

        private void trackBarX_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownChangeX)
            {
                numericUpDownChangeX = false;
                return;
            }

            trackBarChangeX = true;
            numericUpDownX.Value = (decimal)(minValue.X + trackBarX.Value * (maxValue.X - minValue.X) / 20);

            // TODO GuiController.Instance.focus3dPanel();
        }

        private void trackBarY_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownChangeY)
            {
                numericUpDownChangeY = false;
                return;
            }

            trackBarChangeY = true;
            numericUpDownY.Value = (decimal)(minValue.Y + trackBarY.Value * (maxValue.Y - minValue.Y) / 20);

            // TODO GuiController.Instance.focus3dPanel();
        }

        public override object getValue()
        {
            result.X = (float)numericUpDownX.Value;
            result.Y = (float)numericUpDownY.Value;
            return result;
        }
    }
}