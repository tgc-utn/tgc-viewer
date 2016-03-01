using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Viewer.Utils.Modifiers
{
    /// <summary>
    ///     Panel generico para un Modifier
    /// </summary>
    public abstract class TgcModifierPanel
    {
        protected FlowLayoutPanel contentPanel;
        protected FlowLayoutPanel mainPanel;

        protected Button showHideButton;
        protected Label title;
        protected FlowLayoutPanel titleBar;

        public TgcModifierPanel(string varName)
        {
            VarName = varName;

            mainPanel = new FlowLayoutPanel();
            mainPanel.Margin = new Padding(3);
            mainPanel.AutoSize = true;
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.FlowDirection = FlowDirection.TopDown;
            mainPanel.BorderStyle = BorderStyle.Fixed3D;

            titleBar = new FlowLayoutPanel();
            titleBar.Margin = new Padding(2);
            titleBar.AutoSize = true;
            titleBar.Dock = DockStyle.Top;
            titleBar.FlowDirection = FlowDirection.LeftToRight;
            titleBar.Name = "titleBar";
            titleBar.BackColor = SystemColors.ButtonShadow;
            titleBar.BorderStyle = BorderStyle.FixedSingle;

            showHideButton = new Button();
            showHideButton.Margin = new Padding(0);
            showHideButton.Click += showHideButton_click;
            showHideButton.AutoSize = false;
            showHideButton.Size = new Size(15, 15);
            showHideButton.Text = "-";
            showHideButton.FlatStyle = FlatStyle.System;
            showHideButton.Name = "showHideButton";
            showHideButton.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            titleBar.Controls.Add(showHideButton);

            title = new Label();
            title.AutoSize = true;
            title.Text = varName;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Name = "title";
            title.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            titleBar.Controls.Add(title);

            contentPanel = new FlowLayoutPanel();
            contentPanel.Margin = new Padding(0);
            contentPanel.AutoSize = true;
            contentPanel.FlowDirection = FlowDirection.TopDown;
            contentPanel.Name = "contentPanel";
            contentPanel.Dock = DockStyle.Top;

            mainPanel.Controls.Add(titleBar);
            mainPanel.Controls.Add(contentPanel);
        }

        /// <summary>
        ///     Control gráfico principal del Modifier
        /// </summary>
        public FlowLayoutPanel MainPanel
        {
            get { return mainPanel; }
        }

        /// <summary>
        ///     Nombre de la variable del Modifier
        /// </summary>
        public string VarName { get; }

        private void showHideButton_click(object sender, EventArgs e)
        {
            contentPanel.Visible = !contentPanel.Visible;
            showHideButton.Text = contentPanel.Visible ? "-" : "+";
            GuiController.Instance.focus3dPanel();
        }

        /// <summary>
        ///     Devuelve el valor del variable del modificador.
        ///     Se debe castear al tipo que corresponda.
        /// </summary>
        public abstract object getValue();
    }
}