namespace TGC.Examples.RoomsEditor
{
    partial class RoomsEditorControl
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

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxMapView = new System.Windows.Forms.GroupBox();
            this.buttonEdit2dMap = new System.Windows.Forms.Button();
            this.groupBoxExport = new System.Windows.Forms.GroupBox();
            this.buttonCustomExport = new System.Windows.Forms.Button();
            this.buttonExportScene = new System.Windows.Forms.Button();
            this.buttonSaveMap = new System.Windows.Forms.Button();
            this.groupBoxProject = new System.Windows.Forms.GroupBox();
            this.buttonOpenMap = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.groupBoxMapView.SuspendLayout();
            this.groupBoxExport.SuspendLayout();
            this.groupBoxProject.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMapView
            // 
            this.groupBoxMapView.Controls.Add(this.buttonEdit2dMap);
            this.groupBoxMapView.Location = new System.Drawing.Point(3, 3);
            this.groupBoxMapView.Name = "groupBoxMapView";
            this.groupBoxMapView.Size = new System.Drawing.Size(100, 58);
            this.groupBoxMapView.TabIndex = 0;
            this.groupBoxMapView.TabStop = false;
            this.groupBoxMapView.Text = "Map view";
            // 
            // buttonEdit2dMap
            // 
            this.buttonEdit2dMap.Location = new System.Drawing.Point(6, 19);
            this.buttonEdit2dMap.Name = "buttonEdit2dMap";
            this.buttonEdit2dMap.Size = new System.Drawing.Size(88, 23);
            this.buttonEdit2dMap.TabIndex = 0;
            this.buttonEdit2dMap.Text = "Edit 2D map";
            this.buttonEdit2dMap.UseVisualStyleBackColor = true;
            this.buttonEdit2dMap.Click += new System.EventHandler(this.buttonEdit2dMap_Click);
            // 
            // groupBoxExport
            // 
            this.groupBoxExport.Controls.Add(this.buttonCustomExport);
            this.groupBoxExport.Controls.Add(this.buttonExportScene);
            this.groupBoxExport.Location = new System.Drawing.Point(3, 150);
            this.groupBoxExport.Name = "groupBoxExport";
            this.groupBoxExport.Size = new System.Drawing.Size(100, 80);
            this.groupBoxExport.TabIndex = 1;
            this.groupBoxExport.TabStop = false;
            this.groupBoxExport.Text = "Export";
            // 
            // buttonCustomExport
            // 
            this.buttonCustomExport.Location = new System.Drawing.Point(6, 48);
            this.buttonCustomExport.Name = "buttonCustomExport";
            this.buttonCustomExport.Size = new System.Drawing.Size(88, 23);
            this.buttonCustomExport.TabIndex = 2;
            this.buttonCustomExport.Text = "Custom export";
            this.buttonCustomExport.UseVisualStyleBackColor = true;
            this.buttonCustomExport.Click += new System.EventHandler(this.buttonCustomExport_Click);
            // 
            // buttonExportScene
            // 
            this.buttonExportScene.Location = new System.Drawing.Point(6, 19);
            this.buttonExportScene.Name = "buttonExportScene";
            this.buttonExportScene.Size = new System.Drawing.Size(88, 23);
            this.buttonExportScene.TabIndex = 1;
            this.buttonExportScene.Text = "Export scene";
            this.buttonExportScene.UseVisualStyleBackColor = true;
            this.buttonExportScene.Click += new System.EventHandler(this.buttonExportScene_Click);
            // 
            // buttonSaveMap
            // 
            this.buttonSaveMap.Location = new System.Drawing.Point(6, 48);
            this.buttonSaveMap.Name = "buttonSaveMap";
            this.buttonSaveMap.Size = new System.Drawing.Size(88, 23);
            this.buttonSaveMap.TabIndex = 0;
            this.buttonSaveMap.Text = "Save map";
            this.buttonSaveMap.UseVisualStyleBackColor = true;
            this.buttonSaveMap.Click += new System.EventHandler(this.buttonSaveMap_Click);
            // 
            // groupBoxProject
            // 
            this.groupBoxProject.Controls.Add(this.buttonOpenMap);
            this.groupBoxProject.Controls.Add(this.buttonSaveMap);
            this.groupBoxProject.Location = new System.Drawing.Point(3, 67);
            this.groupBoxProject.Name = "groupBoxProject";
            this.groupBoxProject.Size = new System.Drawing.Size(100, 77);
            this.groupBoxProject.TabIndex = 2;
            this.groupBoxProject.TabStop = false;
            this.groupBoxProject.Text = "Project";
            // 
            // buttonOpenMap
            // 
            this.buttonOpenMap.Location = new System.Drawing.Point(6, 19);
            this.buttonOpenMap.Name = "buttonOpenMap";
            this.buttonOpenMap.Size = new System.Drawing.Size(88, 23);
            this.buttonOpenMap.TabIndex = 1;
            this.buttonOpenMap.Text = "Open map";
            this.buttonOpenMap.UseVisualStyleBackColor = true;
            this.buttonOpenMap.Click += new System.EventHandler(this.buttonOpenMap_Click);
            // 
            // buttonHelp
            // 
            this.buttonHelp.Location = new System.Drawing.Point(27, 235);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(45, 23);
            this.buttonHelp.TabIndex = 3;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // RoomsEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.groupBoxProject);
            this.Controls.Add(this.groupBoxExport);
            this.Controls.Add(this.groupBoxMapView);
            this.Name = "RoomsEditorControl";
            this.Size = new System.Drawing.Size(106, 552);
            this.groupBoxMapView.ResumeLayout(false);
            this.groupBoxExport.ResumeLayout(false);
            this.groupBoxProject.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxMapView;
        private System.Windows.Forms.Button buttonEdit2dMap;
        private System.Windows.Forms.GroupBox groupBoxExport;
        private System.Windows.Forms.Button buttonSaveMap;
        private System.Windows.Forms.Button buttonExportScene;
        private System.Windows.Forms.Button buttonCustomExport;
        private System.Windows.Forms.GroupBox groupBoxProject;
        private System.Windows.Forms.Button buttonOpenMap;
        private System.Windows.Forms.Button buttonHelp;
    }
}
