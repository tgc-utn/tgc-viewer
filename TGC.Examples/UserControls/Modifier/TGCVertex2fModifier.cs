using System;
using System.Windows.Forms;
using TGC.Core.Mathematica;

namespace TGC.Examples.UserControls.Modifier
{
    /// <summary>
    ///     Modificador para valores floats (X,Y) o (U,V) de un vertice
    /// </summary>
    public partial class TGCVertex2fModifier : UserControl
    {
        public TGCVertex2fModifier()
        {
            InitializeComponent();
        }

        public TGCVertex2fModifier(string modifierName, TGCVector2 minValue, TGCVector2 maxValue,
            TGCVector2 defaultValue) : this()
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
        }

        public TGCVector2 Value => new TGCVector2((float)numericUpDownX.Value, (float)numericUpDownY.Value);
        private TGCVector2 MinValue { get; }
        private TGCVector2 MaxValue { get; }
        private bool NumericUpDownChangeX { get; set; }
        private bool NumericUpDownChangeY { get; set; }
        private bool TrackBarChangeX { get; set; }
        private bool TrackBarChangeY { get; set; }

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
    }
}