namespace TGC.Examples.UserControls.Modifier
{
    partial class TGCTextureModifier
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.tgcModifierTitleBar = new TGC.Examples.UserControls.Modifier.TGCModifierTitleBar();
            this.textureBox = new System.Windows.Forms.PictureBox();
            this.contentPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.textureBox)).BeginInit();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tgcModifierTitleBar
            // 
            this.tgcModifierTitleBar.AutoSize = true;
            this.tgcModifierTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.tgcModifierTitleBar.Location = new System.Drawing.Point(0, 0);
            this.tgcModifierTitleBar.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.tgcModifierTitleBar.Name = "tgcModifierTitleBar";
            this.tgcModifierTitleBar.Size = new System.Drawing.Size(225, 33);
            this.tgcModifierTitleBar.TabIndex = 0;
            // 
            // textureBox
            // 
            this.textureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textureBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.textureBox.Location = new System.Drawing.Point(5, 5);
            this.textureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textureBox.Name = "textureBox";
            this.textureBox.Size = new System.Drawing.Size(215, 150);
            this.textureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.textureBox.TabIndex = 1;
            this.textureBox.TabStop = false;
            this.textureBox.Click += new System.EventHandler(this.textureBox_Click);
            // 
            // contentPanel
            // 
            this.contentPanel.AutoSize = true;
            this.contentPanel.Controls.Add(this.textureBox);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 33);
            this.contentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(5);
            this.contentPanel.Size = new System.Drawing.Size(225, 198);
            this.contentPanel.TabIndex = 2;
            // 
            // TGCTextureModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.tgcModifierTitleBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TGCTextureModifier";
            this.Size = new System.Drawing.Size(225, 231);
            ((System.ComponentModel.ISupportInitialize)(this.textureBox)).EndInit();
            this.contentPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TGCModifierTitleBar tgcModifierTitleBar;
        private System.Windows.Forms.PictureBox textureBox;
        private System.Windows.Forms.Panel contentPanel;
    }
}
