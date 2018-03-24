namespace TGC.Examples.UserControls.Modifier
{
    partial class TGCVertex2fModifier
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
            this.numericUpDownX = new System.Windows.Forms.NumericUpDown();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.trackBarX = new System.Windows.Forms.TrackBar();
            this.numericUpDownY = new System.Windows.Forms.NumericUpDown();
            this.trackBarY = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX)).BeginInit();
            this.contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).BeginInit();
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
            // numericUpDownX
            // 
            this.numericUpDownX.DecimalPlaces = 4;
            this.numericUpDownX.Dock = System.Windows.Forms.DockStyle.Top;
            this.numericUpDownX.Location = new System.Drawing.Point(0, 0);
            this.numericUpDownX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownX.Name = "numericUpDownX";
            this.numericUpDownX.Size = new System.Drawing.Size(225, 26);
            this.numericUpDownX.TabIndex = 1;
            // 
            // contentPanel
            // 
            this.contentPanel.AutoSize = true;
            this.contentPanel.Controls.Add(this.trackBarY);
            this.contentPanel.Controls.Add(this.numericUpDownY);
            this.contentPanel.Controls.Add(this.trackBarX);
            this.contentPanel.Controls.Add(this.numericUpDownX);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 33);
            this.contentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(225, 198);
            this.contentPanel.TabIndex = 5;
            // 
            // trackBarX
            // 
            this.trackBarX.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackBarX.Location = new System.Drawing.Point(0, 26);
            this.trackBarX.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarX.Name = "trackBarX";
            this.trackBarX.Size = new System.Drawing.Size(225, 69);
            this.trackBarX.TabIndex = 5;
            // 
            // numericUpDownY
            // 
            this.numericUpDownY.DecimalPlaces = 4;
            this.numericUpDownY.Dock = System.Windows.Forms.DockStyle.Top;
            this.numericUpDownY.Location = new System.Drawing.Point(0, 95);
            this.numericUpDownY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownY.Name = "numericUpDownY";
            this.numericUpDownY.Size = new System.Drawing.Size(225, 26);
            this.numericUpDownY.TabIndex = 6;
            // 
            // trackBarY
            // 
            this.trackBarY.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackBarY.Location = new System.Drawing.Point(0, 121);
            this.trackBarY.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.trackBarY.Name = "trackBarY";
            this.trackBarY.Size = new System.Drawing.Size(225, 69);
            this.trackBarY.TabIndex = 7;
            // 
            // TGCVertex2fModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.tgcModifierTitleBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TGCVertex2fModifier";
            this.Size = new System.Drawing.Size(225, 231);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX)).EndInit();
            this.contentPanel.ResumeLayout(false);
            this.contentPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TGCModifierTitleBar tgcModifierTitleBar;
        private System.Windows.Forms.NumericUpDown numericUpDownX;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.TrackBar trackBarY;
        private System.Windows.Forms.NumericUpDown numericUpDownY;
        private System.Windows.Forms.TrackBar trackBarX;
    }
}
