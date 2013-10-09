namespace Examples.MeshCreator
{
    partial class ObjectBrowser
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonMove = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonHide = new System.Windows.Forms.Button();
            this.buttonShow = new System.Windows.Forms.Button();
            this.buttonNewLayer = new System.Windows.Forms.Button();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.treeViewObjects = new System.Windows.Forms.TreeView();
            this.buttonRename = new System.Windows.Forms.Button();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonUnselectAll = new System.Windows.Forms.Button();
            this.labelSearch = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.labelSearch);
            this.splitContainer1.Panel1.Controls.Add(this.buttonUnselectAll);
            this.splitContainer1.Panel1.Controls.Add(this.buttonSelectAll);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRename);
            this.splitContainer1.Panel1.Controls.Add(this.buttonMove);
            this.splitContainer1.Panel1.Controls.Add(this.buttonDelete);
            this.splitContainer1.Panel1.Controls.Add(this.buttonHide);
            this.splitContainer1.Panel1.Controls.Add(this.buttonShow);
            this.splitContainer1.Panel1.Controls.Add(this.buttonNewLayer);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxSearch);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.treeViewObjects);
            this.splitContainer1.Size = new System.Drawing.Size(407, 553);
            this.splitContainer1.SplitterDistance = 52;
            this.splitContainer1.TabIndex = 0;
            // 
            // buttonMove
            // 
            this.buttonMove.Location = new System.Drawing.Point(223, 26);
            this.buttonMove.Name = "buttonMove";
            this.buttonMove.Size = new System.Drawing.Size(42, 23);
            this.buttonMove.TabIndex = 6;
            this.buttonMove.Text = "Move";
            this.buttonMove.UseVisualStyleBackColor = true;
            this.buttonMove.Click += new System.EventHandler(this.buttonMove_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(40, 26);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(47, 23);
            this.buttonDelete.TabIndex = 5;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonHide
            // 
            this.buttonHide.Location = new System.Drawing.Point(182, 26);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(42, 23);
            this.buttonHide.TabIndex = 4;
            this.buttonHide.Text = "Hide";
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.buttonHide_Click);
            // 
            // buttonShow
            // 
            this.buttonShow.Location = new System.Drawing.Point(140, 26);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(42, 23);
            this.buttonShow.TabIndex = 3;
            this.buttonShow.Text = "Show";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
            // 
            // buttonNewLayer
            // 
            this.buttonNewLayer.Location = new System.Drawing.Point(3, 26);
            this.buttonNewLayer.Name = "buttonNewLayer";
            this.buttonNewLayer.Size = new System.Drawing.Size(37, 23);
            this.buttonNewLayer.TabIndex = 2;
            this.buttonNewLayer.Text = "New";
            this.buttonNewLayer.UseVisualStyleBackColor = true;
            this.buttonNewLayer.Click += new System.EventHandler(this.buttonNewLayer_Click);
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(53, 3);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(338, 20);
            this.textBoxSearch.TabIndex = 1;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
            // 
            // treeViewObjects
            // 
            this.treeViewObjects.CheckBoxes = true;
            this.treeViewObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewObjects.Location = new System.Drawing.Point(0, 0);
            this.treeViewObjects.Name = "treeViewObjects";
            this.treeViewObjects.Size = new System.Drawing.Size(407, 497);
            this.treeViewObjects.TabIndex = 0;
            this.treeViewObjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewObjects_AfterCheck);
            this.treeViewObjects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewObjects_AfterSelect);
            // 
            // buttonRename
            // 
            this.buttonRename.Location = new System.Drawing.Point(86, 26);
            this.buttonRename.Name = "buttonRename";
            this.buttonRename.Size = new System.Drawing.Size(55, 23);
            this.buttonRename.TabIndex = 7;
            this.buttonRename.Text = "Rename";
            this.buttonRename.UseVisualStyleBackColor = true;
            this.buttonRename.Click += new System.EventHandler(this.buttonRename_Click);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Location = new System.Drawing.Point(264, 26);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(58, 23);
            this.buttonSelectAll.TabIndex = 8;
            this.buttonSelectAll.Text = "Select all";
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonUnselectAll
            // 
            this.buttonUnselectAll.Location = new System.Drawing.Point(321, 26);
            this.buttonUnselectAll.Name = "buttonUnselectAll";
            this.buttonUnselectAll.Size = new System.Drawing.Size(70, 23);
            this.buttonUnselectAll.TabIndex = 9;
            this.buttonUnselectAll.Text = "Unselect all";
            this.buttonUnselectAll.UseVisualStyleBackColor = true;
            this.buttonUnselectAll.Click += new System.EventHandler(this.buttonUnselectAll_Click);
            // 
            // labelSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new System.Drawing.Point(3, 6);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(44, 13);
            this.labelSearch.TabIndex = 10;
            this.labelSearch.Text = "Search:";
            // 
            // ObjectBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 553);
            this.Controls.Add(this.splitContainer1);
            this.MinimizeBox = false;
            this.Name = "ObjectBrowser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Object browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ObjectBrowser_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Button buttonNewLayer;
        private System.Windows.Forms.TreeView treeViewObjects;
        private System.Windows.Forms.Button buttonShow;
        private System.Windows.Forms.Button buttonHide;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonMove;
        private System.Windows.Forms.Button buttonRename;
        private System.Windows.Forms.Button buttonUnselectAll;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Label labelSearch;
    }
}