namespace TGC.Examples.UserControls.Modifier
{
    partial class TgcMeshBrowser
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelItems = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonHome = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBoxPath = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonBrowse = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelItems
            // 
            this.panelItems.AutoScroll = true;
            this.panelItems.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelItems.Location = new System.Drawing.Point(5, 5);
            this.panelItems.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelItems.Name = "panelItems";
            this.panelItems.Size = new System.Drawing.Size(1373, 861);
            this.panelItems.TabIndex = 3;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonUp,
            this.toolStripButtonHome,
            this.toolStripTextBoxPath,
            this.toolStripButtonBrowse});
            this.toolStrip1.Location = new System.Drawing.Point(5, 5);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1373, 39);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonUp
            // 
            this.toolStripButtonUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUp.Image = global::TGC.Examples.Properties.Resources.go_up;
            this.toolStripButtonUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonUp.Name = "toolStripButtonUp";
            this.toolStripButtonUp.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonUp.Text = "toolStripButton1";
            this.toolStripButtonUp.Click += new System.EventHandler(this.toolStripButtonUp_Click);
            // 
            // toolStripButtonHome
            // 
            this.toolStripButtonHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonHome.Image = global::TGC.Examples.Properties.Resources.go_home;
            this.toolStripButtonHome.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHome.Name = "toolStripButtonHome";
            this.toolStripButtonHome.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonHome.Text = "toolStripButton2";
            this.toolStripButtonHome.Click += new System.EventHandler(this.toolStripButtonHome_Click);
            // 
            // toolStripTextBoxPath
            // 
            this.toolStripTextBoxPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolStripTextBoxPath.Name = "toolStripTextBoxPath";
            this.toolStripTextBoxPath.Size = new System.Drawing.Size(600, 39);
            // 
            // toolStripButtonBrowse
            // 
            this.toolStripButtonBrowse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonBrowse.Image = global::TGC.Examples.Properties.Resources.folder1;
            this.toolStripButtonBrowse.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonBrowse.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBrowse.Name = "toolStripButtonBrowse";
            this.toolStripButtonBrowse.Size = new System.Drawing.Size(36, 36);
            this.toolStripButtonBrowse.Text = "toolStripButton3";
            this.toolStripButtonBrowse.Click += new System.EventHandler(this.toolStripButtonBrowse_Click);
            // 
            // TgcMeshBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1383, 871);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panelItems);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "TgcMeshBrowser";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mesh Browser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TgcMeshBrowser_FormClosed);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel panelItems;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonUp;
        private System.Windows.Forms.ToolStripButton toolStripButtonHome;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxPath;
        private System.Windows.Forms.ToolStripButton toolStripButtonBrowse;
    }
}