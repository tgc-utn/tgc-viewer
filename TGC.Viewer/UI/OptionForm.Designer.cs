namespace TGC.Viewer.UI
{
    partial class OptionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelShadersDirectory = new System.Windows.Forms.Label();
            this.textBoxShadersDirectory = new System.Windows.Forms.TextBox();
            this.textBoxMediaDirectory = new System.Windows.Forms.TextBox();
            this.labelMediaDirectory = new System.Windows.Forms.Label();
            this.textBoxCommonShaders = new System.Windows.Forms.TextBox();
            this.labelCommonShaders = new System.Windows.Forms.Label();
            this.labelMediaLink = new System.Windows.Forms.Label();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonShadersDirectory = new System.Windows.Forms.Button();
            this.buttonMediaDirectory = new System.Windows.Forms.Button();
            this.buttonCommonShaders = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonOpenCommonShaders = new System.Windows.Forms.Button();
            this.buttonOpenShaders = new System.Windows.Forms.Button();
            this.buttonOpenMedia = new System.Windows.Forms.Button();
            this.buttonOpenMediaLink = new System.Windows.Forms.Button();
            this.buttonMediaLink = new System.Windows.Forms.Button();
            this.textBoxMediaLink = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelShadersDirectory
            // 
            this.labelShadersDirectory.AutoSize = true;
            this.labelShadersDirectory.Location = new System.Drawing.Point(12, 48);
            this.labelShadersDirectory.Name = "labelShadersDirectory";
            this.labelShadersDirectory.Size = new System.Drawing.Size(110, 13);
            this.labelShadersDirectory.TabIndex = 3;
            this.labelShadersDirectory.Text = "Directorio de shaders:";
            // 
            // textBoxShadersDirectory
            // 
            this.textBoxShadersDirectory.Location = new System.Drawing.Point(12, 64);
            this.textBoxShadersDirectory.Name = "textBoxShadersDirectory";
            this.textBoxShadersDirectory.ReadOnly = true;
            this.textBoxShadersDirectory.Size = new System.Drawing.Size(379, 20);
            this.textBoxShadersDirectory.TabIndex = 4;
            // 
            // textBoxMediaDirectory
            // 
            this.textBoxMediaDirectory.Location = new System.Drawing.Point(12, 25);
            this.textBoxMediaDirectory.Name = "textBoxMediaDirectory";
            this.textBoxMediaDirectory.ReadOnly = true;
            this.textBoxMediaDirectory.Size = new System.Drawing.Size(379, 20);
            this.textBoxMediaDirectory.TabIndex = 1;
            // 
            // labelMediaDirectory
            // 
            this.labelMediaDirectory.AutoSize = true;
            this.labelMediaDirectory.Location = new System.Drawing.Point(12, 9);
            this.labelMediaDirectory.Name = "labelMediaDirectory";
            this.labelMediaDirectory.Size = new System.Drawing.Size(113, 13);
            this.labelMediaDirectory.TabIndex = 0;
            this.labelMediaDirectory.Text = "Directorio de recursos:";
            // 
            // textBoxCommonShaders
            // 
            this.textBoxCommonShaders.Location = new System.Drawing.Point(12, 103);
            this.textBoxCommonShaders.Name = "textBoxCommonShaders";
            this.textBoxCommonShaders.ReadOnly = true;
            this.textBoxCommonShaders.Size = new System.Drawing.Size(379, 20);
            this.textBoxCommonShaders.TabIndex = 7;
            // 
            // labelCommonShaders
            // 
            this.labelCommonShaders.AutoSize = true;
            this.labelCommonShaders.Location = new System.Drawing.Point(12, 87);
            this.labelCommonShaders.Name = "labelCommonShaders";
            this.labelCommonShaders.Size = new System.Drawing.Size(166, 13);
            this.labelCommonShaders.TabIndex = 6;
            this.labelCommonShaders.Text = "Subdirectorio de shaders básicos:";
            // 
            // labelMediaLink
            // 
            this.labelMediaLink.AutoSize = true;
            this.labelMediaLink.Location = new System.Drawing.Point(12, 126);
            this.labelMediaLink.Name = "labelMediaLink";
            this.labelMediaLink.Size = new System.Drawing.Size(156, 13);
            this.labelMediaLink.TabIndex = 9;
            this.labelMediaLink.Text = "Enlace de descarga de medios:";
            // 
            // buttonAccept
            // 
            this.buttonAccept.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAccept.Location = new System.Drawing.Point(397, 5);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 23);
            this.buttonAccept.TabIndex = 12;
            this.buttonAccept.Text = "Aceptar";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(478, 5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancelar";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonShadersDirectory
            // 
            this.buttonShadersDirectory.Location = new System.Drawing.Point(397, 62);
            this.buttonShadersDirectory.Name = "buttonShadersDirectory";
            this.buttonShadersDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonShadersDirectory.TabIndex = 5;
            this.buttonShadersDirectory.Text = "Examinar...";
            this.buttonShadersDirectory.UseVisualStyleBackColor = true;
            this.buttonShadersDirectory.Click += new System.EventHandler(this.buttonShadersDirectory_Click);
            // 
            // buttonMediaDirectory
            // 
            this.buttonMediaDirectory.Location = new System.Drawing.Point(397, 23);
            this.buttonMediaDirectory.Name = "buttonMediaDirectory";
            this.buttonMediaDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonMediaDirectory.TabIndex = 2;
            this.buttonMediaDirectory.Text = "Examinar...";
            this.buttonMediaDirectory.UseVisualStyleBackColor = true;
            this.buttonMediaDirectory.Click += new System.EventHandler(this.buttonMediaDirectory_Click);
            // 
            // buttonCommonShaders
            // 
            this.buttonCommonShaders.Location = new System.Drawing.Point(397, 101);
            this.buttonCommonShaders.Name = "buttonCommonShaders";
            this.buttonCommonShaders.Size = new System.Drawing.Size(75, 23);
            this.buttonCommonShaders.TabIndex = 8;
            this.buttonCommonShaders.Text = "Examinar...";
            this.buttonCommonShaders.UseVisualStyleBackColor = true;
            this.buttonCommonShaders.Click += new System.EventHandler(this.buttonCommonShaders_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonAccept);
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 171);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(565, 40);
            this.panel1.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.buttonOpenCommonShaders);
            this.panel2.Controls.Add(this.buttonOpenShaders);
            this.panel2.Controls.Add(this.buttonOpenMedia);
            this.panel2.Controls.Add(this.buttonOpenMediaLink);
            this.panel2.Controls.Add(this.buttonMediaLink);
            this.panel2.Controls.Add(this.textBoxMediaLink);
            this.panel2.Controls.Add(this.labelMediaDirectory);
            this.panel2.Controls.Add(this.labelShadersDirectory);
            this.panel2.Controls.Add(this.textBoxShadersDirectory);
            this.panel2.Controls.Add(this.buttonCommonShaders);
            this.panel2.Controls.Add(this.textBoxMediaDirectory);
            this.panel2.Controls.Add(this.buttonMediaDirectory);
            this.panel2.Controls.Add(this.labelCommonShaders);
            this.panel2.Controls.Add(this.buttonShadersDirectory);
            this.panel2.Controls.Add(this.textBoxCommonShaders);
            this.panel2.Controls.Add(this.labelMediaLink);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(565, 171);
            this.panel2.TabIndex = 15;
            // 
            // buttonOpenCommonShaders
            // 
            this.buttonOpenCommonShaders.Location = new System.Drawing.Point(478, 101);
            this.buttonOpenCommonShaders.Name = "buttonOpenCommonShaders";
            this.buttonOpenCommonShaders.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenCommonShaders.TabIndex = 15;
            this.buttonOpenCommonShaders.Text = "Abrir";
            this.buttonOpenCommonShaders.UseVisualStyleBackColor = true;
            this.buttonOpenCommonShaders.Click += new System.EventHandler(this.buttonOpenCommonShaders_Click);
            // 
            // buttonOpenShaders
            // 
            this.buttonOpenShaders.Location = new System.Drawing.Point(478, 62);
            this.buttonOpenShaders.Name = "buttonOpenShaders";
            this.buttonOpenShaders.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenShaders.TabIndex = 14;
            this.buttonOpenShaders.Text = "Abrir";
            this.buttonOpenShaders.UseVisualStyleBackColor = true;
            this.buttonOpenShaders.Click += new System.EventHandler(this.buttonOpenShaders_Click);
            // 
            // buttonOpenMedia
            // 
            this.buttonOpenMedia.Location = new System.Drawing.Point(478, 23);
            this.buttonOpenMedia.Name = "buttonOpenMedia";
            this.buttonOpenMedia.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenMedia.TabIndex = 13;
            this.buttonOpenMedia.Text = "Abrir";
            this.buttonOpenMedia.UseVisualStyleBackColor = true;
            this.buttonOpenMedia.Click += new System.EventHandler(this.buttonOpenMedia_Click);
            // 
            // buttonOpenMediaLink
            // 
            this.buttonOpenMediaLink.Location = new System.Drawing.Point(478, 140);
            this.buttonOpenMediaLink.Name = "buttonOpenMediaLink";
            this.buttonOpenMediaLink.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenMediaLink.TabIndex = 12;
            this.buttonOpenMediaLink.Text = "Navegar";
            this.buttonOpenMediaLink.UseVisualStyleBackColor = true;
            this.buttonOpenMediaLink.Click += new System.EventHandler(this.buttonOpenMediaLink_Click);
            // 
            // buttonMediaLink
            // 
            this.buttonMediaLink.Location = new System.Drawing.Point(397, 140);
            this.buttonMediaLink.Name = "buttonMediaLink";
            this.buttonMediaLink.Size = new System.Drawing.Size(75, 23);
            this.buttonMediaLink.TabIndex = 11;
            this.buttonMediaLink.Text = "Cambiar";
            this.buttonMediaLink.UseVisualStyleBackColor = true;
            this.buttonMediaLink.Click += new System.EventHandler(this.buttonMediaLink_Click);
            // 
            // textBoxMediaLink
            // 
            this.textBoxMediaLink.Location = new System.Drawing.Point(12, 142);
            this.textBoxMediaLink.Name = "textBoxMediaLink";
            this.textBoxMediaLink.ReadOnly = true;
            this.textBoxMediaLink.Size = new System.Drawing.Size(379, 20);
            this.textBoxMediaLink.TabIndex = 10;
            // 
            // OptionForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(565, 211);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OptionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Opciones";
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelShadersDirectory;
        private System.Windows.Forms.TextBox textBoxShadersDirectory;
        private System.Windows.Forms.TextBox textBoxMediaDirectory;
        private System.Windows.Forms.Label labelMediaDirectory;
        private System.Windows.Forms.TextBox textBoxCommonShaders;
        private System.Windows.Forms.Label labelCommonShaders;
        private System.Windows.Forms.Label labelMediaLink;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonShadersDirectory;
        private System.Windows.Forms.Button buttonMediaDirectory;
        private System.Windows.Forms.Button buttonCommonShaders;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonMediaLink;
        private System.Windows.Forms.TextBox textBoxMediaLink;
        private System.Windows.Forms.Button buttonOpenMediaLink;
        private System.Windows.Forms.Button buttonOpenCommonShaders;
        private System.Windows.Forms.Button buttonOpenShaders;
        private System.Windows.Forms.Button buttonOpenMedia;
    }
}