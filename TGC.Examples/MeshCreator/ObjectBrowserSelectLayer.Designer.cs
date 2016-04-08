namespace TGC.Examples.MeshCreator
{
    partial class ObjectBrowserSelectLayer
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
            this.treeViewLayers = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLayers.Location = new System.Drawing.Point(0, 0);
            this.treeViewLayers.Name = "treeViewLayers";
            this.treeViewLayers.Size = new System.Drawing.Size(240, 400);
            this.treeViewLayers.TabIndex = 0;
            this.treeViewLayers.DoubleClick += new System.EventHandler(this.treeViewLayers_DoubleClick);
            // 
            // ObjectBrowserSelectLayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(240, 400);
            this.Controls.Add(this.treeViewLayers);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ObjectBrowserSelectLayer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select layer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewLayers;
    }
}