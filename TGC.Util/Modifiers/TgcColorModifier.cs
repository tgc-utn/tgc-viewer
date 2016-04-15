using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Util.Modifiers
{
    /// <summary>
    ///     Modificador para elegir un color
    /// </summary>
    public class TgcColorModifier : TgcModifierPanel
    {
        private readonly ColorDialog colorDialog;
        private readonly Label colorLabel;
        private readonly FlowLayoutPanel colorPanel;

        public TgcColorModifier(string varName, Color defaultValue)
            : base(varName)
        {
            colorPanel = new FlowLayoutPanel();
            colorPanel.Margin = new Padding(0);
            colorPanel.AutoSize = true;
            colorPanel.FlowDirection = FlowDirection.LeftToRight;

            colorLabel = new Label();
            colorLabel.Margin = new Padding(0);
            colorLabel.Size = new Size(80, 40);
            colorLabel.BackColor = defaultValue;
            colorLabel.BorderStyle = BorderStyle.FixedSingle;
            colorLabel.Click += colorButton_click;

            colorPanel.Controls.Add(colorLabel);

            colorDialog = new ColorDialog();
            colorDialog.Color = defaultValue;
            colorDialog.AllowFullOpen = true;
            colorDialog.AnyColor = true;
            colorDialog.FullOpen = true;

            contentPanel.Controls.Add(colorPanel);
        }

        private void colorButton_click(object sender, EventArgs e)
        {
            colorDialog.ShowDialog();
            colorLabel.BackColor = colorDialog.Color;
        }

        public override object getValue()
        {
            return colorLabel.BackColor;
        }
    }
}