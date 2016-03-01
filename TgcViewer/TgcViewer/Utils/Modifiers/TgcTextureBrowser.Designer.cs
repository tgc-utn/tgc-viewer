namespace TGC.Viewer.Utils.Modifiers
{
    partial class TgcTextureBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TgcTextureBrowser));
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxFolderPath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.panelImages = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBoxDirIcon = new System.Windows.Forms.PictureBox();
            this.pictureBoxUpDir = new System.Windows.Forms.PictureBox();
            this.pictureBoxHomeDir = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDirIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUpDir)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHomeDir)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(122, 9);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(63, 13);
            this.labelPath.TabIndex = 0;
            this.labelPath.Text = "Folder path:";
            // 
            // textBoxFolderPath
            // 
            this.textBoxFolderPath.Location = new System.Drawing.Point(191, 6);
            this.textBoxFolderPath.Name = "textBoxFolderPath";
            this.textBoxFolderPath.ReadOnly = true;
            this.textBoxFolderPath.Size = new System.Drawing.Size(638, 20);
            this.textBoxFolderPath.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(835, 6);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // panelImages
            // 
            this.panelImages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelImages.AutoScroll = true;
            this.panelImages.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelImages.Location = new System.Drawing.Point(12, 35);
            this.panelImages.Name = "panelImages";
            this.panelImages.Size = new System.Drawing.Size(898, 519);
            this.panelImages.TabIndex = 3;
            // 
            // pictureBoxDirIcon
            // 
            this.pictureBoxDirIcon.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxDirIcon.Image")));
            this.pictureBoxDirIcon.Location = new System.Drawing.Point(779, 554);
            this.pictureBoxDirIcon.Name = "pictureBoxDirIcon";
            this.pictureBoxDirIcon.Size = new System.Drawing.Size(10, 11);
            this.pictureBoxDirIcon.TabIndex = 0;
            this.pictureBoxDirIcon.TabStop = false;
            this.pictureBoxDirIcon.Visible = false;
            // 
            // pictureBoxUpDir
            // 
            this.pictureBoxUpDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxUpDir.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxUpDir.Image")));
            this.pictureBoxUpDir.Location = new System.Drawing.Point(12, 1);
            this.pictureBoxUpDir.Name = "pictureBoxUpDir";
            this.pictureBoxUpDir.Size = new System.Drawing.Size(35, 35);
            this.pictureBoxUpDir.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxUpDir.TabIndex = 4;
            this.pictureBoxUpDir.TabStop = false;
            this.pictureBoxUpDir.Click += new System.EventHandler(this.pictureBoxUpDir_Click);
            // 
            // pictureBoxHomeDir
            // 
            this.pictureBoxHomeDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxHomeDir.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxHomeDir.Image")));
            this.pictureBoxHomeDir.Location = new System.Drawing.Point(53, 1);
            this.pictureBoxHomeDir.Name = "pictureBoxHomeDir";
            this.pictureBoxHomeDir.Size = new System.Drawing.Size(34, 34);
            this.pictureBoxHomeDir.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxHomeDir.TabIndex = 5;
            this.pictureBoxHomeDir.TabStop = false;
            this.pictureBoxHomeDir.Click += new System.EventHandler(this.pictureBoxHomeDir_Click);
            // 
            // TgcTextureBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 566);
            this.Controls.Add(this.pictureBoxHomeDir);
            this.Controls.Add(this.pictureBoxUpDir);
            this.Controls.Add(this.pictureBoxDirIcon);
            this.Controls.Add(this.panelImages);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxFolderPath);
            this.Controls.Add(this.labelPath);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TgcTextureBrowser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Texture Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TgcTextureBrowser_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TgcTextureBrowser_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDirIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxUpDir)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHomeDir)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textBoxFolderPath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.FlowLayoutPanel panelImages;
        private System.Windows.Forms.PictureBox pictureBoxDirIcon;
        private System.Windows.Forms.PictureBox pictureBoxUpDir;
        private System.Windows.Forms.PictureBox pictureBoxHomeDir;
    }
}