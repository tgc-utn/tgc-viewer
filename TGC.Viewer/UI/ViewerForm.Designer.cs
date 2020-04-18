namespace TGC.Viewer.UI
{
    partial class ViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewerForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.archivoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullExampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.fpsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.axisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateConstanteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wireframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.herramientasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opcionesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ayudaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acercaDeTgcViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusCurrentExample = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainerDerecha = new System.Windows.Forms.SplitContainer();
            this.groupBoxModifiers = new System.Windows.Forms.GroupBox();
            this.panelModifiers = new System.Windows.Forms.Panel();
            this.groupBoxUserVars = new System.Windows.Forms.GroupBox();
            this.dataGridUserVars = new System.Windows.Forms.DataGridView();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainerIzquierda = new System.Windows.Forms.SplitContainer();
            this.groupBoxExamples = new System.Windows.Forms.GroupBox();
            this.treeViewExamples = new System.Windows.Forms.TreeView();
            this.textBoxExampleDescription = new System.Windows.Forms.TextBox();
            this.panel3D = new System.Windows.Forms.Panel();
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
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.archivoToolStripMenuItem,
            this.verToolStripMenuItem,
            this.herramientasToolStripMenuItem,
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
            this.exitToolStripMenuItem});
            this.archivoToolStripMenuItem.Name = "archivoToolStripMenuItem";
            this.archivoToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.archivoToolStripMenuItem.Text = "Archivo";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.application_exit;
            this.exitToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.exitToolStripMenuItem.Text = "Salir";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.salirToolStripMenuItem_Click);
            // 
            // verToolStripMenuItem
            // 
            this.verToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullExampleToolStripMenuItem,
            this.toolStripSeparator1,
            this.fpsToolStripMenuItem,
            this.axisToolStripMenuItem,
            this.updateConstanteToolStripMenuItem,
            this.wireframeToolStripMenuItem,
            this.toolStripSeparator2,
            this.resetToolStripMenuItem});
            this.verToolStripMenuItem.Name = "verToolStripMenuItem";
            this.verToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.verToolStripMenuItem.Text = "Ver";
            // 
            // fullExampleToolStripMenuItem
            // 
            this.fullExampleToolStripMenuItem.CheckOnClick = true;
            this.fullExampleToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.view_fullscreen;
            this.fullExampleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.fullExampleToolStripMenuItem.Name = "fullExampleToolStripMenuItem";
            this.fullExampleToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.fullExampleToolStripMenuItem.Text = "Maximizar ejemplo";
            this.fullExampleToolStripMenuItem.Click += new System.EventHandler(this.fullExampleToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // fpsToolStripMenuItem
            // 
            this.fpsToolStripMenuItem.CheckOnClick = true;
            this.fpsToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.ICON_RENDER_ANIMATION;
            this.fpsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.fpsToolStripMenuItem.Name = "fpsToolStripMenuItem";
            this.fpsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.fpsToolStripMenuItem.Text = "Contador FPS";
            this.fpsToolStripMenuItem.Click += new System.EventHandler(this.fpsToolStripMenuItem_Click);
            // 
            // axisToolStripMenuItem
            // 
            this.axisToolStripMenuItem.CheckOnClick = true;
            this.axisToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.ICON_MANIPUL;
            this.axisToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.axisToolStripMenuItem.Name = "axisToolStripMenuItem";
            this.axisToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.axisToolStripMenuItem.Text = "Ejes cartesianos";
            this.axisToolStripMenuItem.Click += new System.EventHandler(this.axisToolStripMenuItem_Click);
            // 
            // updateConstanteToolStripMenuItem
            // 
            this.updateConstanteToolStripMenuItem.CheckOnClick = true;
            this.updateConstanteToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.appointment_soon;
            this.updateConstanteToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.updateConstanteToolStripMenuItem.Name = "updateConstanteToolStripMenuItem";
            this.updateConstanteToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.updateConstanteToolStripMenuItem.Text = "Update constante";
            this.updateConstanteToolStripMenuItem.Click += new System.EventHandler(this.updateConstanteToolMenuItem_Click);
            // 
            // wireframeToolStripMenuItem
            // 
            this.wireframeToolStripMenuItem.CheckOnClick = true;
            this.wireframeToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.ICON_WIRE;
            this.wireframeToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.wireframeToolStripMenuItem.Name = "wireframeToolStripMenuItem";
            this.wireframeToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.wireframeToolStripMenuItem.Text = "Wireframe";
            this.wireframeToolStripMenuItem.Click += new System.EventHandler(this.wireframeToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.edit_clear_all;
            this.resetToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.resetToolStripMenuItem.Text = "Reiniciar visualización";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // herramientasToolStripMenuItem
            // 
            this.herramientasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.opcionesToolStripMenuItem});
            this.herramientasToolStripMenuItem.Name = "herramientasToolStripMenuItem";
            this.herramientasToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.herramientasToolStripMenuItem.Text = "Herramientas";
            // 
            // opcionesToolStripMenuItem
            // 
            this.opcionesToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.preferences_desktop;
            this.opcionesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.opcionesToolStripMenuItem.Name = "opcionesToolStripMenuItem";
            this.opcionesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.opcionesToolStripMenuItem.Text = "Opciones...";
            this.opcionesToolStripMenuItem.Click += new System.EventHandler(this.opcionesToolStripMenuItem_Click);
            // 
            // ayudaToolStripMenuItem
            // 
            this.ayudaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem,
            this.acercaDeTgcViewerToolStripMenuItem});
            this.ayudaToolStripMenuItem.Name = "ayudaToolStripMenuItem";
            this.ayudaToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.ayudaToolStripMenuItem.Text = "Ayuda";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.help_contents;
            this.helpToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.helpToolStripMenuItem.Text = "Ayuda de TGC";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem1_Click);
            // 
            // acercaDeTgcViewerToolStripMenuItem
            // 
            this.acercaDeTgcViewerToolStripMenuItem.Image = global::TGC.Viewer.Properties.Resources.help_about;
            this.acercaDeTgcViewerToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.acercaDeTgcViewerToolStripMenuItem.Name = "acercaDeTgcViewerToolStripMenuItem";
            this.acercaDeTgcViewerToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.acercaDeTgcViewerToolStripMenuItem.Text = "Acerca de TGC";
            this.acercaDeTgcViewerToolStripMenuItem.Click += new System.EventHandler(this.acercaDeTgcViewerToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusCurrentExample});
            this.statusStrip.Location = new System.Drawing.Point(0, 539);
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
            this.splitContainerDerecha.Size = new System.Drawing.Size(250, 515);
            this.splitContainerDerecha.SplitterDistance = 184;
            this.splitContainerDerecha.TabIndex = 8;
            // 
            // groupBoxModifiers
            // 
            this.groupBoxModifiers.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxModifiers.Controls.Add(this.panelModifiers);
            this.groupBoxModifiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxModifiers.Location = new System.Drawing.Point(0, 0);
            this.groupBoxModifiers.Name = "groupBoxModifiers";
            this.groupBoxModifiers.Size = new System.Drawing.Size(250, 184);
            this.groupBoxModifiers.TabIndex = 0;
            this.groupBoxModifiers.TabStop = false;
            this.groupBoxModifiers.Text = "Modificadores";
            // 
            // panelModifiers
            // 
            this.panelModifiers.AutoScroll = true;
            this.panelModifiers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelModifiers.Location = new System.Drawing.Point(3, 16);
            this.panelModifiers.Margin = new System.Windows.Forms.Padding(2);
            this.panelModifiers.Name = "panelModifiers";
            this.panelModifiers.Size = new System.Drawing.Size(244, 165);
            this.panelModifiers.TabIndex = 0;
            // 
            // groupBoxUserVars
            // 
            this.groupBoxUserVars.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxUserVars.Controls.Add(this.dataGridUserVars);
            this.groupBoxUserVars.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxUserVars.Location = new System.Drawing.Point(0, 0);
            this.groupBoxUserVars.Name = "groupBoxUserVars";
            this.groupBoxUserVars.Size = new System.Drawing.Size(250, 327);
            this.groupBoxUserVars.TabIndex = 0;
            this.groupBoxUserVars.TabStop = false;
            this.groupBoxUserVars.Text = "Variables de usuario";
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
            this.dataGridUserVars.Size = new System.Drawing.Size(244, 308);
            this.dataGridUserVars.TabIndex = 0;
            // 
            // ColumnName
            // 
            this.ColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColumnName.DataPropertyName = "Name";
            this.ColumnName.HeaderText = "Nombre";
            this.ColumnName.Name = "ColumnName";
            this.ColumnName.ReadOnly = true;
            this.ColumnName.Width = 69;
            // 
            // ColumnValue
            // 
            this.ColumnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnValue.DataPropertyName = "Value";
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ColumnValue.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnValue.HeaderText = "Valor";
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
            this.splitContainerIzquierda.Size = new System.Drawing.Size(180, 515);
            this.splitContainerIzquierda.SplitterDistance = 296;
            this.splitContainerIzquierda.TabIndex = 6;
            // 
            // groupBoxExamples
            // 
            this.groupBoxExamples.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxExamples.Controls.Add(this.treeViewExamples);
            this.groupBoxExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxExamples.Location = new System.Drawing.Point(0, 0);
            this.groupBoxExamples.Name = "groupBoxExamples";
            this.groupBoxExamples.Size = new System.Drawing.Size(180, 296);
            this.groupBoxExamples.TabIndex = 0;
            this.groupBoxExamples.TabStop = false;
            this.groupBoxExamples.Text = "Ejemplos";
            // 
            // treeViewExamples
            // 
            this.treeViewExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewExamples.Location = new System.Drawing.Point(3, 16);
            this.treeViewExamples.Name = "treeViewExamples";
            this.treeViewExamples.Size = new System.Drawing.Size(174, 277);
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
            this.textBoxExampleDescription.Size = new System.Drawing.Size(180, 215);
            this.textBoxExampleDescription.TabIndex = 0;
            // 
            // panel3D
            // 
            this.panel3D.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3D.Location = new System.Drawing.Point(180, 24);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new System.Drawing.Size(354, 515);
            this.panel3D.TabIndex = 9;
            // 
            // ViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.panel3D);
            this.Controls.Add(this.splitContainerDerecha);
            this.Controls.Add(this.splitContainerIzquierda);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(798, 594);
            this.Name = "ViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ViewerForm_FormClosing);
            this.Load += new System.EventHandler(this.ViewerForm_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainerDerecha.Panel1.ResumeLayout(false);
            this.splitContainerDerecha.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDerecha)).EndInit();
            this.splitContainerDerecha.ResumeLayout(false);
            this.groupBoxModifiers.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem fpsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCurrentExample;
        private System.Windows.Forms.ToolStripMenuItem axisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullExampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem acercaDeTgcViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerIzquierda;
        private System.Windows.Forms.GroupBox groupBoxExamples;
        private System.Windows.Forms.TreeView treeViewExamples;
        private System.Windows.Forms.TextBox textBoxExampleDescription;
        private System.Windows.Forms.SplitContainer splitContainerDerecha;
        private System.Windows.Forms.GroupBox groupBoxModifiers;
        private System.Windows.Forms.GroupBox groupBoxUserVars;
        private System.Windows.Forms.DataGridView dataGridUserVars;
        private System.Windows.Forms.Panel panel3D;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnValue;
        private System.Windows.Forms.ToolStripMenuItem herramientasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem opcionesToolStripMenuItem;
        private System.Windows.Forms.Panel panelModifiers;
        private System.Windows.Forms.ToolStripMenuItem updateConstanteToolStripMenuItem;
    }
}

