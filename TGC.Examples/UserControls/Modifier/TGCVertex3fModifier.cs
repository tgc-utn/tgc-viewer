using System;
using System.Windows.Forms;
using TGC.Core.Mathematica;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores floats (X,Y,Z) de un vertice
    /// </summary>
    public partial class TGCVertex3fModifier : UserControl
    {
        public TGCVertex3fModifier()
        {
            InitializeComponent();
        }

        public TGCVertex3fModifier(string modifierName, TGCVector3 minValue, TGCVector3 maxValue,
            TGCVector3 defaultValue) : this()
        {
            tgcModifierTitleBar.setModifierName(modifierName);
            tgcModifierTitleBar.setContentPanel(contentPanel);

            MinValue = minValue;
            MaxValue = maxValue;

            //numericUpDownX
            numericUpDownX.Minimum = (decimal)minValue.X;
            numericUpDownX.Maximum = (decimal)maxValue.X;
            numericUpDownX.Value = (decimal)defaultValue.X;
            numericUpDownX.Increment = (decimal)(2f * (maxValue.X - minValue.X) / 100f);
            numericUpDownX.ValueChanged += numericUpDownX_ValueChanged;

            //numericUpDownY
            numericUpDownY.Minimum = (decimal)minValue.Y;
            numericUpDownY.Maximum = (decimal)maxValue.Y;
            numericUpDownY.Value = (decimal)defaultValue.Y;
            numericUpDownY.Increment = (decimal)(2f * (maxValue.Y - minValue.Y) / 100f);
            numericUpDownY.ValueChanged += numericUpDownY_ValueChanged;

            //numericUpDownZ
            numericUpDownZ.Minimum = (decimal)minValue.Z;
            numericUpDownZ.Maximum = (decimal)maxValue.Z;
            numericUpDownZ.Value = (decimal)defaultValue.Z;
            numericUpDownZ.Increment = (decimal)(2f * (maxValue.Z - minValue.Z) / 100f);
            numericUpDownZ.ValueChanged += numericUpDownZ_ValueChanged;

            //trackBarX
            trackBarX.Minimum = 0;
            trackBarX.Maximum = 20;
            trackBarX.Value = (int)((defaultValue.X - minValue.X) * 20 / (maxValue.X - minValue.X));
            trackBarX.ValueChanged += trackBarX_ValueChanged;

            //trackBarY
            trackBarY.Minimum = 0;
            trackBarY.Maximum = 20;
            trackBarY.Value = (int)((defaultValue.Y - minValue.Y) * 20 / (maxValue.Y - minValue.Y));
            trackBarY.ValueChanged += trackBarY_ValueChanged;

            //trackBarZ
            trackBarZ.Minimum = 0;
            trackBarZ.Maximum = 20;
            trackBarZ.Value = (int)((defaultValue.Z - minValue.Z) * 20 / (maxValue.Z - minValue.Z));
            trackBarZ.ValueChanged += trackBarZ_ValueChanged;
        }

        public TGCVector3 Value => new TGCVector3((float)numericUpDownX.Value, (float)numericUpDownY.Value,
            (float)numericUpDownZ.Value);

        private TGCVector3 MinValue { get; }
        private TGCVector3 MaxValue { get; }
        private bool NumericUpDownChangeX { get; set; }
        private bool NumericUpDownChangeY { get; set; }
        private bool NumericUpDownChangeZ { get; set; }
        private bool TrackBarChangeX { get; set; }
        private bool TrackBarChangeY { get; set; }
        private bool TrackBarChangeZ { get; set; }

        private void numericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if (TrackBarChangeX)
            {
                TrackBarChangeX = false;
                return;
            }

            NumericUpDownChangeX = true;
            trackBarX.Value = (int)(((float)numericUpDownX.Value - MinValue.X) * 20 / (MaxValue.X - MinValue.X));
        }

        private void numericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (TrackBarChangeY)
            {
                TrackBarChangeY = false;
                return;
            }

            NumericUpDownChangeY = true;
            trackBarY.Value = (int)(((float)numericUpDownY.Value - MinValue.Y) * 20 / (MaxValue.Y - MinValue.Y));
        }

        private void numericUpDownZ_ValueChanged(object sender, EventArgs e)
        {
            if (TrackBarChangeZ)
            {
                TrackBarChangeZ = false;
                return;
            }

            NumericUpDownChangeZ = true;
            trackBarZ.Value = (int)(((float)numericUpDownZ.Value - MinValue.Z) * 20 / (MaxValue.Z - MinValue.Z));
        }

        private void trackBarX_ValueChanged(object sender, EventArgs e)
        {
            if (NumericUpDownChangeX)
            {
                NumericUpDownChangeX = false;
                return;
            }

            TrackBarChangeX = true;
            numericUpDownX.Value = (decimal)(MinValue.X + trackBarX.Value * (MaxValue.X - MinValue.X) / 20);
        }

        private void trackBarY_ValueChanged(object sender, EventArgs e)
        {
            if (NumericUpDownChangeY)
            {
                NumericUpDownChangeY = false;
                return;
            }

            TrackBarChangeY = true;
            numericUpDownY.Value = (decimal)(MinValue.Y + trackBarY.Value * (MaxValue.Y - MinValue.Y) / 20);
        }

        private void trackBarZ_ValueChanged(object sender, EventArgs e)
        {
            if (NumericUpDownChangeZ)
            {
                NumericUpDownChangeZ = false;
                return;
            }

            TrackBarChangeZ = true;
            numericUpDownZ.Value = (decimal)(MinValue.Z + trackBarZ.Value * (MaxValue.Z - MinValue.Z) / 20);
        }
    }
}