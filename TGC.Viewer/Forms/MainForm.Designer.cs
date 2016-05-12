namespace TGC.Viewer.Forms
{
    partial class MainForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.archivoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.salirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contadorFPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ejesCartesianosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ejecutarEnFullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ayudaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acercaDeTgcViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusCurrentExample = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainerDerecha = new System.Windows.Forms.SplitContainer();
            this.groupBoxModifiers = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelModifiers = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxUserVars = new System.Windows.Forms.GroupBox();
            this.dataGridUserVars = new System.Windows.Forms.DataGridView();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainerIzquierda = new System.Windows.Forms.SplitContainer();
            this.groupBoxExamples = new System.Windows.Forms.GroupBox();
            this.treeViewExamples = new System.Windows.Forms.TreeView();
            this.textBoxExampleDescription = new System.Windows.Forms.TextBox();
            this.panel3d = new System.Windows.Forms.Panel();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDerecha)).BeginInit();
            this.splitContainerDerecha.Panel1.SuspendLayout();
            this.splitContainerDerecha.Panel2.SuspendLayout();
            this.splitContainerDerecha.SuspendLayout();
            this.groupBoxModifiers.SuspendLayout();
            this.groupBoxUserVars.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUserVars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerIzquierda)).BeginInit();
            this.splitContainerIzquierda.Panel1.SuspendLayout();
            this.splitContainerIzquierda.Panel2.SuspendLayout();
            this.splitContainerIzquierda.SuspendLayout();
            this.groupBoxExamples.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.archivoToolStripMenuItem,
            this.verToolStripMenuItem,
            this.ayudaToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(784, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // archivoToolStripMenuItem
            // 
            this.archivoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.salirToolStripMenuItem});
            this.archivoToolStripMenuItem.Name = "archivoToolStripMenuItem";
            this.archivoToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.archivoToolStripMenuItem.Text = "Archivo";
            // 
            // salirToolStripMenuItem
            // 
            this.salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            this.salirToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.salirToolStripMenuItem.Text = "Salir";
            this.salirToolStripMenuItem.Click += new System.EventHandler(this.salirToolStripMenuItem_Click);
            // 
            // verToolStripMenuItem
            // 
            this.verToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireframeToolStripMenuItem,
            this.contadorFPSToolStripMenuItem,
            this.ejesCartesianosToolStripMenuItem,
            this.ejecutarEnFullScreenToolStripMenuItem});
            this.verToolStripMenuItem.Name = "verToolStripMenuItem";
            this.verToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.verToolStripMenuItem.Text = "Ver";
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.CheckOnClick = true;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.W)));
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // contadorFPSToolStripMenuItem
            // 
            this.contadorFPSToolStripMenuItem.Checked = true;
            this.contadorFPSToolStripMenuItem.CheckOnClick = true;
            this.contadorFPSToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.contadorFPSToolStripMenuItem.Name = "contadorFPSToolStripMenuItem";
            this.contadorFPSToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.contadorFPSToolStripMenuItem.Text = "Contador FPS";
            this.contadorFPSToolStripMenuItem.Click += new System.EventHandler(this.contadorFPSToolStripMenuItem_Click);
            // 
            // ejesCartesianosToolStripMenuItem
            // 
            this.ejesCartesianosToolStripMenuItem.Checked = true;
            this.ejesCartesianosToolStripMenuItem.CheckOnClick = true;
            this.ejesCartesianosToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ejesCartesianosToolStripMenuItem.Name = "ejesCartesianosToolStripMenuItem";
            this.ejesCartesianosToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.ejesCartesianosToolStripMenuItem.Text = "Ejes cartesianos";
            this.ejesCartesianosToolStripMenuItem.Click += new System.EventHandler(this.ejesCartesianosToolStripMenuItem_Click);
            // 
            // ejecutarEnFullScreenToolStripMenuItem
            // 
            this.ejecutarEnFullScreenToolStripMenuItem.CheckOnClick = true;
            this.ejecutarEnFullScreenToolStripMenuItem.Name = "ejecutarEnFullScreenToolStripMenuItem";
            this.ejecutarEnFullScreenToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.ejecutarEnFullScreenToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.ejecutarEnFullScreenToolStripMenuItem.Text = "Ejecutar en Full Screen (BETA)";
            // 
            // ayudaToolStripMenuItem
            // 
            this.ayudaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.acercaDeTgcViewerToolStripMenuItem});
            this.ayudaToolStripMenuItem.Name = "ayudaToolStripMenuItem";
            this.ayudaToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.ayudaToolStripMenuItem.Text = "Ayuda";
            // 
            // acercaDeTgcViewerToolStripMenuItem
            // 
            this.acercaDeTgcViewerToolStripMenuItem.Name = "acercaDeTgcViewerToolStripMenuItem";
            this.acercaDeTgcViewerToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.acercaDeTgcViewerToolStripMenuItem.Text = "Acerca de TGC Viewer";
            this.acercaDeTgcViewerToolStripMenuItem.Click += new System.EventHandler(this.acercaDeTgcViewerToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusCurrentExample});
            this.statusStrip.Location = new System.Drawing.Point(0, 664);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(784, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusCurrentExample
            // 
            this.toolStripStatusCurrentExample.BackColor = System.Drawing.Color.GreenYellow;
            this.toolStripStatusCurrentExample.Name = "toolStripStatusCurrentExample";
            this.toolStripStatusCurrentExample.Size = new System.Drawing.Size(0, 17);
            // 
            // splitContainerDerecha
            // 
            this.splitContainerDerecha.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitContainerDerecha.Location = new System.Drawing.Point(534, 24);
            this.splitContainerDerecha.Name = "splitContainerDerecha";
            this.splitContainerDerecha.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerDerecha.Panel1
            // 
            this.splitContainerDerecha.Panel1.Controls.Add(this.groupBoxModifiers);
            // 
            // splitContainerDerecha.Panel2
            // 
            this.splitContainerDerecha.Panel2.Controls.Add(this.groupBoxUserVars);
            this.splitContainerDerecha.Size = new System.Drawing.Size(250, 640);
            this.splitContainerDerecha.SplitterDistance = 231;
            this.splitContainerDerecha.TabIndex = 8;
            // 
            // groupBoxModifiers
            // 
            this.groupBoxModifiers.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxModifiers.Controls.Add(this.flowLayoutPanelModifiers);
            this.groupBoxModifiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxModifiers.Location = new System.Drawing.Point(0, 0);
            this.groupBoxModifiers.Name = "groupBoxModifiers";
            this.groupBoxModifiers.Size = new System.Drawing.Size(250, 231);
            this.groupBoxModifiers.TabIndex = 0;
            this.groupBoxModifiers.TabStop = false;
            this.groupBoxModifiers.Text = "Modifiers";
            // 
            // flowLayoutPanelModifiers
            // 
            this.flowLayoutPanelModifiers.AutoScroll = true;
            this.flowLayoutPanelModifiers.AutoSize = true;
            this.flowLayoutPanelModifiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelModifiers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelModifiers.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanelModifiers.Name = "flowLayoutPanelModifiers";
            this.flowLayoutPanelModifiers.Size = new System.Drawing.Size(244, 212);
            this.flowLayoutPanelModifiers.TabIndex = 0;
            this.flowLayoutPanelModifiers.WrapContents = false;
            // 
            // groupBoxUserVars
            // 
            this.groupBoxUserVars.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxUserVars.Controls.Add(this.dataGridUserVars);
            this.groupBoxUserVars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxUserVars.Location = new System.Drawing.Point(0, 0);
            this.groupBoxUserVars.Name = "groupBoxUserVars";
            this.groupBoxUserVars.Size = new System.Drawing.Size(250, 405);
            this.groupBoxUserVars.TabIndex = 0;
            this.groupBoxUserVars.TabStop = false;
            this.groupBoxUserVars.Text = "User variables";
            // 
            // dataGridUserVars
            // 
            this.dataGridUserVars.AllowUserToAddRows = false;
            this.dataGridUserVars.AllowUserToDeleteRows = false;
            this.dataGridUserVars.AllowUserToOrderColumns = true;
            this.dataGridUserVars.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridUserVars.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnName,
            this.ColumnValue});
            this.dataGridUserVars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridUserVars.Location = new System.Drawing.Point(3, 16);
            this.dataGridUserVars.MultiSelect = false;
            this.dataGridUserVars.Name = "dataGridUserVars";
            this.dataGridUserVars.ReadOnly = true;
            this.dataGridUserVars.RowHeadersWidth = 10;
            this.dataGridUserVars.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridUserVars.Size = new System.Drawing.Size(244, 386);
            this.dataGridUserVars.TabIndex = 0;
            // 
            // ColumnName
            // 
            this.ColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnName.DataPropertyName = "Name";
            this.ColumnName.HeaderText = "Name";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            this.ColumnName.Width = 60;
            // 
            // ColumnValue
            // 
            this.ColumnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnValue.DataPropertyName = "Value";
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ColumnValue.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnValue.HeaderText = "Value";
            this.ColumnValue.Name = "ColumnValue";
            this.ColumnValue.ReadOnly = true;
            // 
            // splitContainerIzquierda
            // 
            this.splitContainerIzquierda.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainerIzquierda.Location = new System.Drawing.Point(0, 24);
            this.splitContainerIzquierda.Name = "splitContainerIzquierda";
            this.splitContainerIzquierda.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerIzquierda.Panel1
            // 
            this.splitContainerIzquierda.Panel1.Controls.Add(this.groupBoxExamples);
            // 
            // splitContainerIzquierda.Panel2
            // 
            this.splitContainerIzquierda.Panel2.Controls.Add(this.textBoxExampleDescription);
            this.splitContainerIzquierda.Size = new System.Drawing.Size(180, 640);
            this.splitContainerIzquierda.SplitterDistance = 370;
            this.splitContainerIzquierda.TabIndex = 6;
            // 
            // groupBoxExamples
            // 
            this.groupBoxExamples.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxExamples.Controls.Add(this.treeViewExamples);
            this.groupBoxExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxExamples.Location = new System.Drawing.Point(0, 0);
            this.groupBoxExamples.Name = "groupBoxExamples";
            this.groupBoxExamples.Size = new System.Drawing.Size(180, 370);
            this.groupBoxExamples.TabIndex = 0;
            this.groupBoxExamples.TabStop = false;
            this.groupBoxExamples.Text = "Ejemplos";
            // 
            // treeViewExamples
            // 
            this.treeViewExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewExamples.Location = new System.Drawing.Point(3, 16);
            this.treeViewExamples.Name = "treeViewExamples";
            this.treeViewExamples.Size = new System.Drawing.Size(174, 351);
            this.treeViewExamples.TabIndex = 4;
            this.treeViewExamples.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewExamples_AfterSelect);
            this.treeViewExamples.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeViewExamples_MouseDoubleClick);
            // 
            // textBoxExampleDescription
            // 
            this.textBoxExampleDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxExampleDescription.Location = new System.Drawing.Point(0, 0);
            this.textBoxExampleDescription.Multiline = true;
            this.textBoxExampleDescription.Name = "textBoxExampleDescription";
            this.textBoxExampleDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxExampleDescription.Size = new System.Drawing.Size(180, 266);
            this.textBoxExampleDescription.TabIndex = 0;
            // 
            // panel3d
            // 
            this.panel3d.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3d.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3d.Location = new System.Drawing.Point(180, 24);
            this.panel3d.Name = "panel3d";
            this.panel3d.Size = new System.Drawing.Size(354, 640);
            this.panel3d.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(784, 686);
            this.Controls.Add(this.panel3d);
            this.Controls.Add(this.splitContainerDerecha);
            this.Controls.Add(this.splitContainerIzquierda);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 725);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerDerecha.Panel1.ResumeLayout(false);
            this.splitContainerDerecha.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDerecha)).EndInit();
            this.splitContainerDerecha.ResumeLayout(false);
            this.groupBoxModifiers.ResumeLayout(false);
            this.groupBoxModifiers.PerformLayout();
            this.groupBoxUserVars.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridUserVars)).EndInit();
            this.splitContainerIzquierda.Panel1.ResumeLayout(false);
            this.splitContainerIzquierda.Panel2.ResumeLayout(false);
            this.splitContainerIzquierda.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerIzquierda)).EndInit();
            this.splitContainerIzquierda.ResumeLayout(false);
            this.groupBoxExamples.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem archivoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ayudaToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem verToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wireframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contadorFPSToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCurrentExample;
        private System.Windows.Forms.ToolStripMenuItem ejesCartesianosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ejecutarEnFullScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem acercaDeTgcViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem salirToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerIzquierda;
        private System.Windows.Forms.GroupBox groupBoxExamples;
        private System.Windows.Forms.TreeView treeViewExamples;
        private System.Windows.Forms.TextBox textBoxExampleDescription;
        private System.Windows.Forms.SplitContainer splitContainerDerecha;
        private System.Windows.Forms.GroupBox groupBoxModifiers;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelModifiers;
        private System.Windows.Forms.GroupBox groupBoxUserVars;
        private System.Windows.Forms.DataGridView dataGridUserVars;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnValue;
        private System.Windows.Forms.Panel panel3d;
    }
}

