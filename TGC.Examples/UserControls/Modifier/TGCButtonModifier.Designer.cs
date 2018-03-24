namespace TGC.Examples.UserControls.Modifier
{
    partial class TGCButtonModifier
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
            this.button = new System.Windows.Forms.Button();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.tgcModifierTitleBar = new TGC.Examples.UserControls.Modifier.TGCModifierTitleBar();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // button
            // 
            this.button.Location = new System.Drawing.Point(4, 5);
            this.button.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(112, 35);
            this.button.TabIndex = 1;
            this.button.Text = "button";
            this.button.UseVisualStyleBackColor = true;
            // 
            // contentPanel
            // 
            this.contentPanel.AutoSize = true;
            this.contentPanel.Controls.Add(this.button);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 33);
            this.contentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(5);
            this.contentPanel.Size = new System.Drawing.Size(225, 198);
            this.contentPanel.TabIndex = 2;
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
            // TGCButtonModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.tgcModifierTitleBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TGCButtonModifier";
            this.Size = new System.Drawing.Size(225, 231);
            this.contentPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TGCModifierTitleBar tgcModifierTitleBar;
        private System.Windows.Forms.Button button;
        private System.Windows.Forms.Panel contentPanel;
    }
}
