using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TgcViewer.Utils.Modifiers
{
    /// <summary>
    /// Panel generico para un Modifier
    /// </summary>
    public abstract class TgcModifierPanel
    {
        protected FlowLayoutPanel mainPanel;
        /// <summary>
        /// Control gráfico principal del Modifier
        /// </summary>
        public FlowLayoutPanel MainPanel
        {
            get { return mainPanel; }
        }

        private string varName;
        /// <summary>
        /// Nombre de la variable del Modifier
        /// </summary>
        public string VarName
        {
            get { return varName; }
        }

        protected Button showHideButton;
        protected Label title;
        protected FlowLayoutPanel titleBar;
        protected FlowLayoutPanel contentPanel;

        public TgcModifierPanel(string varName)
        {
            this.varName = varName;

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
            titleBar.BackColor = System.Drawing.SystemColors.ButtonShadow;
            titleBar.BorderStyle = BorderStyle.FixedSingle;

            showHideButton = new Button();
            showHideButton.Margin = new Padding(0);
            showHideButton.Click += new EventHandler(showHideButton_click);
            showHideButton.AutoSize = false;
            showHideButton.Size = new Size(15, 15);
            showHideButton.Text = "-";
            showHideButton.FlatStyle = FlatStyle.System;
            showHideButton.Name = "showHideButton";
            showHideButton.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            titleBar.Controls.Add(showHideButton);

            title = new Label();
            title.AutoSize = true;
            title.Text = varName;
            title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            title.Name = "title";
            title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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


        private void showHideButton_click(object sender, EventArgs e)
        {
            contentPanel.Visible = !contentPanel.Visible;
            showHideButton.Text = contentPanel.Visible ? "-" : "+"; 
            GuiController.Instance.focus3dPanel();
        }

        /// <summary>
        /// Devuelve el valor del variable del modificador.
        /// Se debe castear al tipo que corresponda.
        /// </summary>
        public abstract object getValue();


    }
}
