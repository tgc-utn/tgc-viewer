namespace TGC.Examples.UserControls.Modifier
{
    partial class TGCFileModifier
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.fileButton = new System.Windows.Forms.Button();
            this.contentPanel = new System.Windows.Forms.Panel();
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
            this.tgcModifierTitleBar.Size = new System.Drawing.Size(240, 33);
            this.tgcModifierTitleBar.TabIndex = 0;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Title = "Seleccionar archivo";
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Location = new System.Drawing.Point(4, 10);
            this.fileNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.ReadOnly = true;
            this.fileNameTextBox.Size = new System.Drawing.Size(223, 26);
            this.fileNameTextBox.TabIndex = 1;
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Location = new System.Drawing.Point(4, 46);
            this.filePathTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.ReadOnly = true;
            this.filePathTextBox.Size = new System.Drawing.Size(223, 26);
            this.filePathTextBox.TabIndex = 2;
            // 
            // fileButton
            // 
            this.fileButton.Location = new System.Drawing.Point(4, 82);
            this.fileButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fileButton.Name = "fileButton";
            this.fileButton.Size = new System.Drawing.Size(112, 35);
            this.fileButton.TabIndex = 3;
            this.fileButton.Text = "Examinar...";
            this.fileButton.UseVisualStyleBackColor = true;
            this.fileButton.Click += new System.EventHandler(this.fileButton_Click);
            // 
            // contentPanel
            // 
            this.contentPanel.AutoSize = true;
            this.contentPanel.Controls.Add(this.fileNameTextBox);
            this.contentPanel.Controls.Add(this.fileButton);
            this.contentPanel.Controls.Add(this.filePathTextBox);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 33);
            this.contentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(5);
            this.contentPanel.Size = new System.Drawing.Size(240, 198);
            this.contentPanel.TabIndex = 4;
            // 
            // TGCFileModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.tgcModifierTitleBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TGCFileModifier";
            this.Size = new System.Drawing.Size(240, 231);
            this.contentPanel.ResumeLayout(false);
            this.contentPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TGCModifierTitleBar tgcModifierTitleBar;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.Button fileButton;
        private System.Windows.Forms.Panel contentPanel;
    }
}
