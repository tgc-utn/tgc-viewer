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
            this.richTextBoxMediaLink = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // labelShadersDirectory
            // 
            this.labelShadersDirectory.AutoSize = true;
            this.labelShadersDirectory.Location = new System.Drawing.Point(12, 41);
            this.labelShadersDirectory.Name = "labelShadersDirectory";
            this.labelShadersDirectory.Size = new System.Drawing.Size(107, 13);
            this.labelShadersDirectory.TabIndex = 4;
            this.labelShadersDirectory.Text = "Directorio de shaders";
            // 
            // textBoxShadersDirectory
            // 
            this.textBoxShadersDirectory.Location = new System.Drawing.Point(171, 38);
            this.textBoxShadersDirectory.Name = "textBoxShadersDirectory";
            this.textBoxShadersDirectory.ReadOnly = true;
            this.textBoxShadersDirectory.Size = new System.Drawing.Size(250, 20);
            this.textBoxShadersDirectory.TabIndex = 5;
            // 
            // textBoxMediaDirectory
            // 
            this.textBoxMediaDirectory.Location = new System.Drawing.Point(171, 12);
            this.textBoxMediaDirectory.Name = "textBoxMediaDirectory";
            this.textBoxMediaDirectory.ReadOnly = true;
            this.textBoxMediaDirectory.Size = new System.Drawing.Size(250, 20);
            this.textBoxMediaDirectory.TabIndex = 1;
            // 
            // labelMediaDirectory
            // 
            this.labelMediaDirectory.AutoSize = true;
            this.labelMediaDirectory.Location = new System.Drawing.Point(12, 15);
            this.labelMediaDirectory.Name = "labelMediaDirectory";
            this.labelMediaDirectory.Size = new System.Drawing.Size(103, 13);
            this.labelMediaDirectory.TabIndex = 0;
            this.labelMediaDirectory.Text = "Directorio de medias";
            // 
            // textBoxCommonShaders
            // 
            this.textBoxCommonShaders.Location = new System.Drawing.Point(171, 64);
            this.textBoxCommonShaders.Name = "textBoxCommonShaders";
            this.textBoxCommonShaders.ReadOnly = true;
            this.textBoxCommonShaders.Size = new System.Drawing.Size(250, 20);
            this.textBoxCommonShaders.TabIndex = 8;
            // 
            // labelCommonShaders
            // 
            this.labelCommonShaders.AutoSize = true;
            this.labelCommonShaders.Location = new System.Drawing.Point(12, 67);
            this.labelCommonShaders.Name = "labelCommonShaders";
            this.labelCommonShaders.Size = new System.Drawing.Size(153, 13);
            this.labelCommonShaders.TabIndex = 7;
            this.labelCommonShaders.Text = "Directorio de shaders comunes";
            // 
            // labelMediaLink
            // 
            this.labelMediaLink.AutoSize = true;
            this.labelMediaLink.Location = new System.Drawing.Point(14, 93);
            this.labelMediaLink.Name = "labelMediaLink";
            this.labelMediaLink.Size = new System.Drawing.Size(151, 13);
            this.labelMediaLink.TabIndex = 10;
            this.labelMediaLink.Text = "Link de descarga de los media";
            // 
            // buttonAccept
            // 
            this.buttonAccept.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.buttonAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAccept.Location = new System.Drawing.Point(346, 176);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 23);
            this.buttonAccept.TabIndex = 12;
            this.buttonAccept.Text = "Aceptar";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(430, 176);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancelar";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonShadersDirectory
            // 
            this.buttonShadersDirectory.Location = new System.Drawing.Point(427, 36);
            this.buttonShadersDirectory.Name = "buttonShadersDirectory";
            this.buttonShadersDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonShadersDirectory.TabIndex = 6;
            this.buttonShadersDirectory.Text = "Examinar...";
            this.buttonShadersDirectory.UseVisualStyleBackColor = true;
            this.buttonShadersDirectory.Click += new System.EventHandler(this.buttonShadersDirectory_Click);
            // 
            // buttonMediaDirectory
            // 
            this.buttonMediaDirectory.Location = new System.Drawing.Point(427, 10);
            this.buttonMediaDirectory.Name = "buttonMediaDirectory";
            this.buttonMediaDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonMediaDirectory.TabIndex = 3;
            this.buttonMediaDirectory.Text = "Examinar...";
            this.buttonMediaDirectory.UseVisualStyleBackColor = true;
            this.buttonMediaDirectory.Click += new System.EventHandler(this.buttonMediaDirectory_Click);
            // 
            // buttonCommonShaders
            // 
            this.buttonCommonShaders.Location = new System.Drawing.Point(427, 62);
            this.buttonCommonShaders.Name = "buttonCommonShaders";
            this.buttonCommonShaders.Size = new System.Drawing.Size(75, 23);
            this.buttonCommonShaders.TabIndex = 9;
            this.buttonCommonShaders.Text = "Examinar...";
            this.buttonCommonShaders.UseVisualStyleBackColor = true;
            this.buttonCommonShaders.Click += new System.EventHandler(this.buttonCommonShaders_Click);
            // 
            // richTextBoxMediaLink
            // 
            this.richTextBoxMediaLink.Location = new System.Drawing.Point(171, 90);
            this.richTextBoxMediaLink.Multiline = false;
            this.richTextBoxMediaLink.Name = "richTextBoxMediaLink";
            this.richTextBoxMediaLink.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBoxMediaLink.Size = new System.Drawing.Size(250, 20);
            this.richTextBoxMediaLink.TabIndex = 11;
            this.richTextBoxMediaLink.Text = "";
            // 
            // OptionForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(517, 211);
            this.Controls.Add(this.richTextBoxMediaLink);
            this.Controls.Add(this.buttonCommonShaders);
            this.Controls.Add(this.buttonMediaDirectory);
            this.Controls.Add(this.buttonShadersDirectory);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.labelMediaLink);
            this.Controls.Add(this.textBoxCommonShaders);
            this.Controls.Add(this.labelCommonShaders);
            this.Controls.Add(this.textBoxMediaDirectory);
            this.Controls.Add(this.labelMediaDirectory);
            this.Controls.Add(this.textBoxShadersDirectory);
            this.Controls.Add(this.labelShadersDirectory);
            this.Name = "OptionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Opciones";
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.RichTextBox richTextBoxMediaLink;
    }
}