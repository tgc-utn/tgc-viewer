using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;

namespace TGC.Core.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores floats (X,Y,Z) de un vertice
    /// </summary>
    public class TgcVertex3fModifier : TgcModifierPanel
    {
        private readonly NumericUpDown numericUpDownX;
        private readonly NumericUpDown numericUpDownY;
        private readonly NumericUpDown numericUpDownZ;
        private readonly TrackBar trackBarX;
        private readonly TrackBar trackBarY;
        private readonly TrackBar trackBarZ;
        private readonly FlowLayoutPanel vertexValuesPanel;
        private Vector3 maxValue;
        private Vector3 minValue;

        private bool numericUpDownChangeX;
        private bool numericUpDownChangeY;
        private bool numericUpDownChangeZ;

        private Vector3 result = new Vector3();
        private bool trackBarChangeX;
        private bool trackBarChangeY;
        private bool trackBarChangeZ;

        public TgcVertex3fModifier(string varName, Vector3 minValue, Vector3 maxValue, Vector3 defaultValue)
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

            //numericUpDownZ
            numericUpDownZ = new NumericUpDown();
            numericUpDownZ.Size = new Size(50, 20);
            numericUpDownZ.Margin = new Padding(0);
            numericUpDownZ.DecimalPlaces = 4;
            numericUpDownZ.Minimum = (decimal)minValue.Z;
            numericUpDownZ.Maximum = (decimal)maxValue.Z;
            numericUpDownZ.Value = (decimal)defaultValue.Z;
            numericUpDownZ.Increment = (decimal)(2f * (maxValue.Z - minValue.Z) / 100f);
            numericUpDownZ.ValueChanged += numericUpDownZ_ValueChanged;

            //Panel para los tres numericUpDown
            vertexValuesPanel = new FlowLayoutPanel();
            vertexValuesPanel.Margin = new Padding(0);
            vertexValuesPanel.AutoSize = true;
            vertexValuesPanel.FlowDirection = FlowDirection.LeftToRight;

            vertexValuesPanel.Controls.Add(numericUpDownX);
            vertexValuesPanel.Controls.Add(numericUpDownY);
            vertexValuesPanel.Controls.Add(numericUpDownZ);

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

            //trackBarZ
            trackBarZ = new TrackBar();
            trackBarZ.Size = new Size(100, 20);
            trackBarZ.Margin = new Padding(0);
            trackBarZ.Minimum = 0;
            trackBarZ.Maximum = 20;
            trackBarZ.Value = (int)((defaultValue.Z - minValue.Z) * 20 / (maxValue.Z - minValue.Z));
            trackBarZ.ValueChanged += trackBarZ_ValueChanged;

            contentPanel.Controls.Add(vertexValuesPanel);
            contentPanel.Controls.Add(trackBarX);
            contentPanel.Controls.Add(trackBarY);
            contentPanel.Controls.Add(trackBarZ);
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

        private void numericUpDownZ_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarChangeZ)
            {
                trackBarChangeZ = false;
                return;
            }

            numericUpDownChangeZ = true;
            trackBarZ.Value = (int)(((float)numericUpDownZ.Value - minValue.Z) * 20 / (maxValue.Z - minValue.Z));

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

        private void trackBarZ_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownChangeZ)
            {
                numericUpDownChangeZ = false;
                return;
            }

            trackBarChangeZ = true;
            numericUpDownZ.Value = (decimal)(minValue.Z + trackBarZ.Value * (maxValue.Z - minValue.Z) / 20);

            // TODO GuiController.Instance.focus3dPanel();
        }

        public override object getValue()
        {
            result.X = (float)numericUpDownX.Value;
            result.Y = (float)numericUpDownY.Value;
            result.Z = (float)numericUpDownZ.Value;
            return result;
        }
    }
}