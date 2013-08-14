namespace Examples.TerrainEditor.Panel
{
    partial class TerrainEditorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TerrainEditorControl));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageTerrain = new System.Windows.Forms.TabPage();
            this.groupBoxModifyScale = new System.Windows.Forms.GroupBox();
            this.nudScaleY = new System.Windows.Forms.NumericUpDown();
            this.nudScaleXZ = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.bReload = new System.Windows.Forms.Button();
            this.bImportVegetation = new System.Windows.Forms.Button();
            this.pictureBoxModifyHeightmap = new System.Windows.Forms.PictureBox();
            this.pictureBoxModifyTexture = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelModifyTextureImage = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nupLevel = new System.Windows.Forms.NumericUpDown();
            this.nupHeight = new System.Windows.Forms.NumericUpDown();
            this.nupWidth = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pageEdit = new System.Windows.Forms.TabPage();
            this.brushSettings = new System.Windows.Forms.GroupBox();
            this.cbInvert = new System.Windows.Forms.CheckBox();
            this.cbRounded = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbHardness = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.tbIntensity = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.tbRadius = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbSteamroller = new System.Windows.Forms.RadioButton();
            this.rbShovel = new System.Windows.Forms.RadioButton();
            this.tabVegetation = new System.Windows.Forms.TabPage();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.bVegetationClear = new System.Windows.Forms.Button();
            this.bClearTF = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.rbRz = new System.Windows.Forms.RadioButton();
            this.rbRy = new System.Windows.Forms.RadioButton();
            this.rbRx = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cbSz = new System.Windows.Forms.CheckBox();
            this.cbSy = new System.Windows.Forms.CheckBox();
            this.cbSx = new System.Windows.Forms.CheckBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.bChangeFolder = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.pbVegetationPreview = new System.Windows.Forms.PictureBox();
            this.lbVegetation = new System.Windows.Forms.ListBox();
            this.gbMode = new System.Windows.Forms.GroupBox();
            this.rbAddVegetation = new System.Windows.Forms.RadioButton();
            this.rbPickVegetation = new System.Windows.Forms.RadioButton();
            this.pageExport = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lheight = new System.Windows.Forms.Label();
            this.lwidth = new System.Windows.Forms.Label();
            this.lname = new System.Windows.Forms.Label();
            this.gbTerrain = new System.Windows.Forms.GroupBox();
            this.lbScaleY = new System.Windows.Forms.Label();
            this.lbScaleXZ = new System.Windows.Forms.Label();
            this.lbCenter = new System.Windows.Forms.Label();
            this.labelVerticesCount = new System.Windows.Forms.Label();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.buttonSaveVegetation = new System.Windows.Forms.Button();
            this.buttonSaveHeightmap = new System.Windows.Forms.Button();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.cbSound = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tbCameraJumpSpeed = new System.Windows.Forms.TrackBar();
            this.label12 = new System.Windows.Forms.Label();
            this.tbCameraMovementSpeed = new System.Windows.Forms.TrackBar();
            this.label9 = new System.Windows.Forms.Label();
            this.visibleDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.useWaitCursorDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabStopDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabIndexDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rightToLeftDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.minimumSizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.maximumSizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.marginDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.locationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.enabledDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dockDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataBindingsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cursorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.causesValidationDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.backColorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.anchorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.allowDropDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.accessibleRoleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.accessibleNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.accessibleDescriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tickFrequencyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tickStyleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.smallChangeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rightToLeftLayoutDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.orientationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.minimumDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.maximumDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.largeChangeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.autoSizeDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saveFileHeightmap = new System.Windows.Forms.SaveFileDialog();
            this.saveFileVegetation = new System.Windows.Forms.SaveFileDialog();
            this.openFileVegetation = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl.SuspendLayout();
            this.pageTerrain.SuspendLayout();
            this.groupBoxModifyScale.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleXZ)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModifyHeightmap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModifyTexture)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupWidth)).BeginInit();
            this.pageEdit.SuspendLayout();
            this.brushSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbHardness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRadius)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabVegetation.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbVegetationPreview)).BeginInit();
            this.gbMode.SuspendLayout();
            this.pageExport.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbTerrain.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCameraJumpSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbCameraMovementSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageTerrain);
            this.tabControl.Controls.Add(this.pageEdit);
            this.tabControl.Controls.Add(this.tabVegetation);
            this.tabControl.Controls.Add(this.pageExport);
            this.tabControl.Controls.Add(this.tabSettings);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(223, 657);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_TabIndexChanged);
            this.tabControl.TabIndexChanged += new System.EventHandler(this.tabControl_TabIndexChanged);
            // 
            // pageTerrain
            // 
            this.pageTerrain.BackColor = System.Drawing.Color.White;
            this.pageTerrain.Controls.Add(this.groupBoxModifyScale);
            this.pageTerrain.Controls.Add(this.groupBox5);
            this.pageTerrain.Controls.Add(this.groupBox1);
            this.pageTerrain.Location = new System.Drawing.Point(4, 22);
            this.pageTerrain.Name = "pageTerrain";
            this.pageTerrain.Padding = new System.Windows.Forms.Padding(3);
            this.pageTerrain.Size = new System.Drawing.Size(215, 631);
            this.pageTerrain.TabIndex = 0;
            this.pageTerrain.Text = "General";
            // 
            // groupBoxModifyScale
            // 
            this.groupBoxModifyScale.Controls.Add(this.nudScaleY);
            this.groupBoxModifyScale.Controls.Add(this.nudScaleXZ);
            this.groupBoxModifyScale.Controls.Add(this.label10);
            this.groupBoxModifyScale.Controls.Add(this.label11);
            this.groupBoxModifyScale.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxModifyScale.Location = new System.Drawing.Point(3, 457);
            this.groupBoxModifyScale.Name = "groupBoxModifyScale";
            this.groupBoxModifyScale.Size = new System.Drawing.Size(209, 88);
            this.groupBoxModifyScale.TabIndex = 51;
            this.groupBoxModifyScale.TabStop = false;
            this.groupBoxModifyScale.Text = "Scale ";
            // 
            // nudScaleY
            // 
            this.nudScaleY.DecimalPlaces = 2;
            this.nudScaleY.Location = new System.Drawing.Point(48, 53);
            this.nudScaleY.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudScaleY.Name = "nudScaleY";
            this.nudScaleY.Size = new System.Drawing.Size(59, 20);
            this.nudScaleY.TabIndex = 61;
            this.nudScaleY.Value = new decimal(new int[] {
            13,
            0,
            0,
            65536});
            this.nudScaleY.ValueChanged += new System.EventHandler(this.nudScale_ValueChanged);
            // 
            // nudScaleXZ
            // 
            this.nudScaleXZ.DecimalPlaces = 2;
            this.nudScaleXZ.Location = new System.Drawing.Point(47, 22);
            this.nudScaleXZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudScaleXZ.Name = "nudScaleXZ";
            this.nudScaleXZ.Size = new System.Drawing.Size(62, 20);
            this.nudScaleXZ.TabIndex = 60;
            this.nudScaleXZ.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudScaleXZ.ValueChanged += new System.EventHandler(this.nudScale_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 24);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 13);
            this.label10.TabIndex = 58;
            this.label10.Text = "XZ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(31, 56);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(14, 13);
            this.label11.TabIndex = 59;
            this.label11.Text = "Y";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.bReload);
            this.groupBox5.Controls.Add(this.bImportVegetation);
            this.groupBox5.Controls.Add(this.pictureBoxModifyHeightmap);
            this.groupBox5.Controls.Add(this.pictureBoxModifyTexture);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.labelModifyTextureImage);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox5.Location = new System.Drawing.Point(3, 158);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(209, 299);
            this.groupBox5.TabIndex = 50;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Import";
            // 
            // bReload
            // 
            this.bReload.BackColor = System.Drawing.Color.LightCoral;
            this.bReload.Location = new System.Drawing.Point(35, 110);
            this.bReload.Name = "bReload";
            this.bReload.Size = new System.Drawing.Size(80, 23);
            this.bReload.TabIndex = 39;
            this.bReload.Text = "Reload";
            this.bReload.UseVisualStyleBackColor = false;
            this.bReload.Click += new System.EventHandler(this.bReload_Click);
            // 
            // bImportVegetation
            // 
            this.bImportVegetation.BackColor = System.Drawing.Color.Transparent;
            this.bImportVegetation.Location = new System.Drawing.Point(35, 259);
            this.bImportVegetation.Name = "bImportVegetation";
            this.bImportVegetation.Size = new System.Drawing.Size(80, 23);
            this.bImportVegetation.TabIndex = 38;
            this.bImportVegetation.Text = "Vegetation";
            this.bImportVegetation.UseVisualStyleBackColor = false;
            this.bImportVegetation.Click += new System.EventHandler(this.bImportVegetation_Click);
            // 
            // pictureBoxModifyHeightmap
            // 
            this.pictureBoxModifyHeightmap.BackColor = System.Drawing.Color.Black;
            this.pictureBoxModifyHeightmap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxModifyHeightmap.Location = new System.Drawing.Point(35, 27);
            this.pictureBoxModifyHeightmap.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxModifyHeightmap.Name = "pictureBoxModifyHeightmap";
            this.pictureBoxModifyHeightmap.Size = new System.Drawing.Size(80, 80);
            this.pictureBoxModifyHeightmap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxModifyHeightmap.TabIndex = 37;
            this.pictureBoxModifyHeightmap.TabStop = false;
            this.pictureBoxModifyHeightmap.Click += new System.EventHandler(this.pictureBoxModifyHeightmap_Click);
            // 
            // pictureBoxModifyTexture
            // 
            this.pictureBoxModifyTexture.BackColor = System.Drawing.Color.Black;
            this.pictureBoxModifyTexture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxModifyTexture.Location = new System.Drawing.Point(35, 162);
            this.pictureBoxModifyTexture.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxModifyTexture.Name = "pictureBoxModifyTexture";
            this.pictureBoxModifyTexture.Size = new System.Drawing.Size(80, 80);
            this.pictureBoxModifyTexture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxModifyTexture.TabIndex = 37;
            this.pictureBoxModifyTexture.TabStop = false;
            this.pictureBoxModifyTexture.Click += new System.EventHandler(this.pictureBoxModifyTexture_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 149);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 36;
            this.label1.Text = "Texture";
            // 
            // labelModifyTextureImage
            // 
            this.labelModifyTextureImage.AutoSize = true;
            this.labelModifyTextureImage.Location = new System.Drawing.Point(44, 14);
            this.labelModifyTextureImage.Name = "labelModifyTextureImage";
            this.labelModifyTextureImage.Size = new System.Drawing.Size(58, 13);
            this.labelModifyTextureImage.TabIndex = 36;
            this.labelModifyTextureImage.Text = "Heightmap";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nupLevel);
            this.groupBox1.Controls.Add(this.nupHeight);
            this.groupBox1.Controls.Add(this.nupWidth);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 155);
            this.groupBox1.TabIndex = 49;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New";
            // 
            // nupLevel
            // 
            this.nupLevel.Location = new System.Drawing.Point(66, 93);
            this.nupLevel.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nupLevel.Name = "nupLevel";
            this.nupLevel.Size = new System.Drawing.Size(58, 20);
            this.nupLevel.TabIndex = 9;
            // 
            // nupHeight
            // 
            this.nupHeight.Location = new System.Drawing.Point(66, 60);
            this.nupHeight.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.nupHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupHeight.Name = "nupHeight";
            this.nupHeight.Size = new System.Drawing.Size(58, 20);
            this.nupHeight.TabIndex = 8;
            this.nupHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // nupWidth
            // 
            this.nupWidth.Location = new System.Drawing.Point(66, 27);
            this.nupWidth.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.nupWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupWidth.Name = "nupWidth";
            this.nupWidth.Size = new System.Drawing.Size(58, 20);
            this.nupWidth.TabIndex = 7;
            this.nupWidth.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 95);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(36, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Level:";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LightCoral;
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(40, 123);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Create";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.buttonNewHeightmap_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Height:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Width:";
            // 
            // pageEdit
            // 
            this.pageEdit.BackColor = System.Drawing.Color.White;
            this.pageEdit.Controls.Add(this.brushSettings);
            this.pageEdit.Controls.Add(this.groupBox3);
            this.pageEdit.Location = new System.Drawing.Point(4, 22);
            this.pageEdit.Name = "pageEdit";
            this.pageEdit.Padding = new System.Windows.Forms.Padding(3);
            this.pageEdit.Size = new System.Drawing.Size(215, 631);
            this.pageEdit.TabIndex = 1;
            this.pageEdit.Text = "Edit";
            // 
            // brushSettings
            // 
            this.brushSettings.Controls.Add(this.cbInvert);
            this.brushSettings.Controls.Add(this.cbRounded);
            this.brushSettings.Controls.Add(this.label5);
            this.brushSettings.Controls.Add(this.tbHardness);
            this.brushSettings.Controls.Add(this.label4);
            this.brushSettings.Controls.Add(this.tbIntensity);
            this.brushSettings.Controls.Add(this.label3);
            this.brushSettings.Controls.Add(this.tbRadius);
            this.brushSettings.Controls.Add(this.label2);
            this.brushSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.brushSettings.Location = new System.Drawing.Point(3, 86);
            this.brushSettings.Name = "brushSettings";
            this.brushSettings.Size = new System.Drawing.Size(209, 252);
            this.brushSettings.TabIndex = 9;
            this.brushSettings.TabStop = false;
            this.brushSettings.Text = "Brush settings";
            // 
            // cbInvert
            // 
            this.cbInvert.AutoSize = true;
            this.cbInvert.Location = new System.Drawing.Point(11, 213);
            this.cbInvert.Name = "cbInvert";
            this.cbInvert.Size = new System.Drawing.Size(53, 17);
            this.cbInvert.TabIndex = 15;
            this.cbInvert.Tag = "Invert";
            this.cbInvert.Text = "Invert";
            this.cbInvert.UseVisualStyleBackColor = true;
            this.cbInvert.CheckedChanged += new System.EventHandler(this.cbBrush_CheckedChanged);
            // 
            // cbRounded
            // 
            this.cbRounded.AutoSize = true;
            this.cbRounded.Checked = true;
            this.cbRounded.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRounded.Location = new System.Drawing.Point(11, 190);
            this.cbRounded.Name = "cbRounded";
            this.cbRounded.Size = new System.Drawing.Size(70, 17);
            this.cbRounded.TabIndex = 14;
            this.cbRounded.Tag = "Rounded";
            this.cbRounded.Text = "Rounded";
            this.cbRounded.UseVisualStyleBackColor = true;
            this.cbRounded.CheckedChanged += new System.EventHandler(this.cbBrush_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(70, 195);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.label5.Size = new System.Drawing.Size(118, 33);
            this.label5.TabIndex = 13;
            this.label5.Text = "Hold Alt to invert effect.";
            // 
            // tbHardness
            // 
            this.tbHardness.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbHardness.Location = new System.Drawing.Point(3, 145);
            this.tbHardness.Maximum = 100;
            this.tbHardness.Name = "tbHardness";
            this.tbHardness.Size = new System.Drawing.Size(203, 45);
            this.tbHardness.TabIndex = 11;
            this.tbHardness.Tag = "Hardness";
            this.tbHardness.Value = 100;
            this.tbHardness.Scroll += new System.EventHandler(this.tbBrush_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Location = new System.Drawing.Point(3, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Hardness";
            // 
            // tbIntensity
            // 
            this.tbIntensity.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbIntensity.Location = new System.Drawing.Point(3, 87);
            this.tbIntensity.Maximum = 255;
            this.tbIntensity.Name = "tbIntensity";
            this.tbIntensity.Size = new System.Drawing.Size(203, 45);
            this.tbIntensity.TabIndex = 9;
            this.tbIntensity.Tag = "Intensity";
            this.tbIntensity.Value = 125;
            this.tbIntensity.Scroll += new System.EventHandler(this.tbBrush_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Location = new System.Drawing.Point(3, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Intensity";
            // 
            // tbRadius
            // 
            this.tbRadius.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbRadius.Location = new System.Drawing.Point(3, 29);
            this.tbRadius.Maximum = 600;
            this.tbRadius.Minimum = 10;
            this.tbRadius.Name = "tbRadius";
            this.tbRadius.Size = new System.Drawing.Size(203, 45);
            this.tbRadius.TabIndex = 9;
            this.tbRadius.Tag = "Radius";
            this.tbRadius.Value = 100;
            this.tbRadius.Scroll += new System.EventHandler(this.tbBrush_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(3, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Radius";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbSteamroller);
            this.groupBox3.Controls.Add(this.rbShovel);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(209, 83);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Brushes";
            // 
            // rbSteamroller
            // 
            this.rbSteamroller.AutoSize = true;
            this.rbSteamroller.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbSteamroller.ForeColor = System.Drawing.Color.Black;
            this.rbSteamroller.Image = ((System.Drawing.Image)(resources.GetObject("rbSteamroller.Image")));
            this.rbSteamroller.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rbSteamroller.Location = new System.Drawing.Point(71, 16);
            this.rbSteamroller.Name = "rbSteamroller";
            this.rbSteamroller.Padding = new System.Windows.Forms.Padding(10, 0, 0, 2);
            this.rbSteamroller.Size = new System.Drawing.Size(87, 64);
            this.rbSteamroller.TabIndex = 1;
            this.rbSteamroller.Text = "Steamroller";
            this.rbSteamroller.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.rbSteamroller.UseVisualStyleBackColor = true;
            this.rbSteamroller.CheckedChanged += new System.EventHandler(this.bSteamroller_CheckedChanged);
            // 
            // rbShovel
            // 
            this.rbShovel.AutoSize = true;
            this.rbShovel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.rbShovel.Checked = true;
            this.rbShovel.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbShovel.Image = ((System.Drawing.Image)(resources.GetObject("rbShovel.Image")));
            this.rbShovel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rbShovel.Location = new System.Drawing.Point(3, 16);
            this.rbShovel.Margin = new System.Windows.Forms.Padding(0);
            this.rbShovel.Name = "rbShovel";
            this.rbShovel.Padding = new System.Windows.Forms.Padding(10, 0, 0, 2);
            this.rbShovel.Size = new System.Drawing.Size(68, 64);
            this.rbShovel.TabIndex = 0;
            this.rbShovel.TabStop = true;
            this.rbShovel.Text = "Shovel";
            this.rbShovel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.rbShovel.UseVisualStyleBackColor = true;
            this.rbShovel.CheckedChanged += new System.EventHandler(this.rbShovel_CheckedChanged);
            // 
            // tabVegetation
            // 
            this.tabVegetation.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tabVegetation.Controls.Add(this.label17);
            this.tabVegetation.Controls.Add(this.label16);
            this.tabVegetation.Controls.Add(this.bVegetationClear);
            this.tabVegetation.Controls.Add(this.bClearTF);
            this.tabVegetation.Controls.Add(this.groupBox9);
            this.tabVegetation.Controls.Add(this.groupBox6);
            this.tabVegetation.Controls.Add(this.groupBox7);
            this.tabVegetation.Controls.Add(this.gbMode);
            this.tabVegetation.Location = new System.Drawing.Point(4, 22);
            this.tabVegetation.Name = "tabVegetation";
            this.tabVegetation.Padding = new System.Windows.Forms.Padding(3);
            this.tabVegetation.Size = new System.Drawing.Size(215, 631);
            this.tabVegetation.TabIndex = 4;
            this.tabVegetation.Text = "Vegetation";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Top;
            this.label17.Location = new System.Drawing.Point(3, 604);
            this.label17.Name = "label17";
            this.label17.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.label17.Size = new System.Drawing.Size(190, 17);
            this.label17.TabIndex = 7;
            this.label17.Text = "Press Z to select the last one added.";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Top;
            this.label16.Location = new System.Drawing.Point(3, 587);
            this.label16.Name = "label16";
            this.label16.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.label16.Size = new System.Drawing.Size(203, 17);
            this.label16.TabIndex = 3;
            this.label16.Text = "Press Supr to remove the selected one.";
            // 
            // bVegetationClear
            // 
            this.bVegetationClear.BackColor = System.Drawing.Color.LightCoral;
            this.bVegetationClear.Dock = System.Windows.Forms.DockStyle.Top;
            this.bVegetationClear.Location = new System.Drawing.Point(3, 559);
            this.bVegetationClear.Name = "bVegetationClear";
            this.bVegetationClear.Size = new System.Drawing.Size(209, 28);
            this.bVegetationClear.TabIndex = 2;
            this.bVegetationClear.Text = "Remove all ";
            this.bVegetationClear.UseVisualStyleBackColor = false;
            this.bVegetationClear.Click += new System.EventHandler(this.bVegetationClear_Click);
            // 
            // bClearTF
            // 
            this.bClearTF.Dock = System.Windows.Forms.DockStyle.Top;
            this.bClearTF.Location = new System.Drawing.Point(3, 514);
            this.bClearTF.Name = "bClearTF";
            this.bClearTF.Size = new System.Drawing.Size(209, 45);
            this.bClearTF.TabIndex = 5;
            this.bClearTF.Text = "Clear transformations";
            this.bClearTF.UseVisualStyleBackColor = true;
            this.bClearTF.Click += new System.EventHandler(this.bClearTF_Click);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.rbRz);
            this.groupBox9.Controls.Add(this.rbRy);
            this.groupBox9.Controls.Add(this.rbRx);
            this.groupBox9.Controls.Add(this.label14);
            this.groupBox9.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox9.Location = new System.Drawing.Point(3, 449);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(209, 65);
            this.groupBox9.TabIndex = 4;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Rotation";
            // 
            // rbRz
            // 
            this.rbRz.AutoSize = true;
            this.rbRz.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbRz.Location = new System.Drawing.Point(107, 16);
            this.rbRz.Name = "rbRz";
            this.rbRz.Size = new System.Drawing.Size(32, 17);
            this.rbRz.TabIndex = 6;
            this.rbRz.Text = "Z";
            this.rbRz.UseVisualStyleBackColor = true;
            this.rbRz.CheckedChanged += new System.EventHandler(this.updateVBRotationAxis);
            // 
            // rbRy
            // 
            this.rbRy.AutoSize = true;
            this.rbRy.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbRy.Checked = true;
            this.rbRy.Location = new System.Drawing.Point(68, 16);
            this.rbRy.Name = "rbRy";
            this.rbRy.Size = new System.Drawing.Size(32, 17);
            this.rbRy.TabIndex = 5;
            this.rbRy.TabStop = true;
            this.rbRy.Text = "Y";
            this.rbRy.UseVisualStyleBackColor = true;
            this.rbRy.CheckedChanged += new System.EventHandler(this.updateVBRotationAxis);
            // 
            // rbRx
            // 
            this.rbRx.AutoSize = true;
            this.rbRx.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbRx.Location = new System.Drawing.Point(28, 16);
            this.rbRx.Name = "rbRx";
            this.rbRx.Size = new System.Drawing.Size(32, 17);
            this.rbRx.TabIndex = 4;
            this.rbRx.Text = "X";
            this.rbRx.UseVisualStyleBackColor = true;
            this.rbRx.CheckedChanged += new System.EventHandler(this.updateVBRotationAxis);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(16, 36);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(184, 13);
            this.label14.TabIndex = 3;
            this.label14.Text = "Use LEFT and RIGHT arrow to rotate";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.cbSz);
            this.groupBox6.Controls.Add(this.cbSy);
            this.groupBox6.Controls.Add(this.cbSx);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox6.Location = new System.Drawing.Point(3, 383);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(209, 66);
            this.groupBox6.TabIndex = 3;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Scale";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(17, 41);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(172, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Use UP and DOWN arrow to scale";
            // 
            // cbSz
            // 
            this.cbSz.AutoSize = true;
            this.cbSz.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSz.Checked = true;
            this.cbSz.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSz.Location = new System.Drawing.Point(106, 21);
            this.cbSz.Name = "cbSz";
            this.cbSz.Size = new System.Drawing.Size(33, 17);
            this.cbSz.TabIndex = 2;
            this.cbSz.Text = "Z";
            this.cbSz.UseVisualStyleBackColor = true;
            this.cbSz.CheckedChanged += new System.EventHandler(this.updateVBScaleAxis);
            // 
            // cbSy
            // 
            this.cbSy.AutoSize = true;
            this.cbSy.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSy.Checked = true;
            this.cbSy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSy.Location = new System.Drawing.Point(67, 21);
            this.cbSy.Name = "cbSy";
            this.cbSy.Size = new System.Drawing.Size(33, 17);
            this.cbSy.TabIndex = 1;
            this.cbSy.Text = "Y";
            this.cbSy.UseVisualStyleBackColor = true;
            this.cbSy.CheckedChanged += new System.EventHandler(this.updateVBScaleAxis);
            // 
            // cbSx
            // 
            this.cbSx.AutoSize = true;
            this.cbSx.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSx.Checked = true;
            this.cbSx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSx.Location = new System.Drawing.Point(28, 21);
            this.cbSx.Name = "cbSx";
            this.cbSx.Size = new System.Drawing.Size(33, 17);
            this.cbSx.TabIndex = 0;
            this.cbSx.Text = "X";
            this.cbSx.UseVisualStyleBackColor = true;
            this.cbSx.CheckedChanged += new System.EventHandler(this.updateVBScaleAxis);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.bChangeFolder);
            this.groupBox7.Controls.Add(this.groupBox8);
            this.groupBox7.Controls.Add(this.lbVegetation);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox7.Location = new System.Drawing.Point(3, 54);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(209, 329);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Meshes";
            // 
            // bChangeFolder
            // 
            this.bChangeFolder.Location = new System.Drawing.Point(38, 148);
            this.bChangeFolder.Name = "bChangeFolder";
            this.bChangeFolder.Size = new System.Drawing.Size(114, 23);
            this.bChangeFolder.TabIndex = 2;
            this.bChangeFolder.Text = "Change folder";
            this.bChangeFolder.UseVisualStyleBackColor = true;
            this.bChangeFolder.Click += new System.EventHandler(this.bChangeFolder_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.pbVegetationPreview);
            this.groupBox8.Location = new System.Drawing.Point(19, 172);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(152, 157);
            this.groupBox8.TabIndex = 1;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Preview";
            // 
            // pbVegetationPreview
            // 
            this.pbVegetationPreview.BackColor = System.Drawing.Color.Black;
            this.pbVegetationPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pbVegetationPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbVegetationPreview.Location = new System.Drawing.Point(3, 16);
            this.pbVegetationPreview.Margin = new System.Windows.Forms.Padding(0);
            this.pbVegetationPreview.Name = "pbVegetationPreview";
            this.pbVegetationPreview.Size = new System.Drawing.Size(146, 138);
            this.pbVegetationPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbVegetationPreview.TabIndex = 2;
            this.pbVegetationPreview.TabStop = false;
            // 
            // lbVegetation
            // 
            this.lbVegetation.FormattingEnabled = true;
            this.lbVegetation.Location = new System.Drawing.Point(19, 25);
            this.lbVegetation.Name = "lbVegetation";
            this.lbVegetation.Size = new System.Drawing.Size(152, 121);
            this.lbVegetation.TabIndex = 0;
            this.lbVegetation.SelectedIndexChanged += new System.EventHandler(this.lbVegetation_SelectedIndexChanged);
            // 
            // gbMode
            // 
            this.gbMode.Controls.Add(this.rbAddVegetation);
            this.gbMode.Controls.Add(this.rbPickVegetation);
            this.gbMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbMode.Location = new System.Drawing.Point(3, 3);
            this.gbMode.Name = "gbMode";
            this.gbMode.Size = new System.Drawing.Size(209, 51);
            this.gbMode.TabIndex = 6;
            this.gbMode.TabStop = false;
            this.gbMode.Text = "Mode";
            // 
            // rbAddVegetation
            // 
            this.rbAddVegetation.AutoSize = true;
            this.rbAddVegetation.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbAddVegetation.Location = new System.Drawing.Point(79, 16);
            this.rbAddVegetation.Name = "rbAddVegetation";
            this.rbAddVegetation.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.rbAddVegetation.Size = new System.Drawing.Size(74, 32);
            this.rbAddVegetation.TabIndex = 1;
            this.rbAddVegetation.Text = "Add";
            this.rbAddVegetation.UseVisualStyleBackColor = true;
            this.rbAddVegetation.CheckedChanged += new System.EventHandler(this.rbAddVegetation_CheckedChanged);
            // 
            // rbPickVegetation
            // 
            this.rbPickVegetation.AutoSize = true;
            this.rbPickVegetation.Checked = true;
            this.rbPickVegetation.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbPickVegetation.Location = new System.Drawing.Point(3, 16);
            this.rbPickVegetation.Name = "rbPickVegetation";
            this.rbPickVegetation.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
            this.rbPickVegetation.Size = new System.Drawing.Size(76, 32);
            this.rbPickVegetation.TabIndex = 0;
            this.rbPickVegetation.TabStop = true;
            this.rbPickVegetation.Text = "Pick";
            this.rbPickVegetation.UseVisualStyleBackColor = true;
            this.rbPickVegetation.CheckedChanged += new System.EventHandler(this.rbPickVegetation_CheckedChanged);
            // 
            // pageExport
            // 
            this.pageExport.Controls.Add(this.button1);
            this.pageExport.Controls.Add(this.groupBox10);
            this.pageExport.Controls.Add(this.groupBox11);
            this.pageExport.Location = new System.Drawing.Point(4, 22);
            this.pageExport.Name = "pageExport";
            this.pageExport.Padding = new System.Windows.Forms.Padding(3);
            this.pageExport.Size = new System.Drawing.Size(215, 631);
            this.pageExport.TabIndex = 2;
            this.pageExport.Text = "Export";
            this.pageExport.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(65, 397);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "About";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.groupBox2);
            this.groupBox10.Controls.Add(this.gbTerrain);
            this.groupBox10.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox10.Location = new System.Drawing.Point(3, 136);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(209, 255);
            this.groupBox10.TabIndex = 5;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Info";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lheight);
            this.groupBox2.Controls.Add(this.lwidth);
            this.groupBox2.Controls.Add(this.lname);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(3, 141);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(203, 104);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Heightmap";
            // 
            // lheight
            // 
            this.lheight.AutoSize = true;
            this.lheight.Dock = System.Windows.Forms.DockStyle.Top;
            this.lheight.Location = new System.Drawing.Point(3, 62);
            this.lheight.Name = "lheight";
            this.lheight.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lheight.Size = new System.Drawing.Size(46, 23);
            this.lheight.TabIndex = 2;
            this.lheight.Text = "height";
            // 
            // lwidth
            // 
            this.lwidth.AutoSize = true;
            this.lwidth.Dock = System.Windows.Forms.DockStyle.Top;
            this.lwidth.Location = new System.Drawing.Point(3, 39);
            this.lwidth.Name = "lwidth";
            this.lwidth.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lwidth.Size = new System.Drawing.Size(42, 23);
            this.lwidth.TabIndex = 1;
            this.lwidth.Text = "width";
            // 
            // lname
            // 
            this.lname.AutoSize = true;
            this.lname.Dock = System.Windows.Forms.DockStyle.Top;
            this.lname.Location = new System.Drawing.Point(3, 16);
            this.lname.Name = "lname";
            this.lname.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lname.Size = new System.Drawing.Size(43, 23);
            this.lname.TabIndex = 0;
            this.lname.Text = "name";
            // 
            // gbTerrain
            // 
            this.gbTerrain.Controls.Add(this.lbScaleY);
            this.gbTerrain.Controls.Add(this.lbScaleXZ);
            this.gbTerrain.Controls.Add(this.lbCenter);
            this.gbTerrain.Controls.Add(this.labelVerticesCount);
            this.gbTerrain.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbTerrain.Location = new System.Drawing.Point(3, 16);
            this.gbTerrain.Name = "gbTerrain";
            this.gbTerrain.Size = new System.Drawing.Size(203, 125);
            this.gbTerrain.TabIndex = 5;
            this.gbTerrain.TabStop = false;
            this.gbTerrain.Text = "Terrain";
            // 
            // lbScaleY
            // 
            this.lbScaleY.AutoSize = true;
            this.lbScaleY.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbScaleY.Location = new System.Drawing.Point(3, 85);
            this.lbScaleY.Name = "lbScaleY";
            this.lbScaleY.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lbScaleY.Size = new System.Drawing.Size(49, 23);
            this.lbScaleY.TabIndex = 7;
            this.lbScaleY.Text = "scaleY";
            this.lbScaleY.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbScaleXZ
            // 
            this.lbScaleXZ.AutoSize = true;
            this.lbScaleXZ.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbScaleXZ.Location = new System.Drawing.Point(3, 62);
            this.lbScaleXZ.Name = "lbScaleXZ";
            this.lbScaleXZ.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lbScaleXZ.Size = new System.Drawing.Size(56, 23);
            this.lbScaleXZ.TabIndex = 6;
            this.lbScaleXZ.Text = "scaleXZ";
            this.lbScaleXZ.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbCenter
            // 
            this.lbCenter.AutoSize = true;
            this.lbCenter.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbCenter.Location = new System.Drawing.Point(3, 39);
            this.lbCenter.Name = "lbCenter";
            this.lbCenter.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lbCenter.Size = new System.Drawing.Size(47, 23);
            this.lbCenter.TabIndex = 5;
            this.lbCenter.Text = "center";
            this.lbCenter.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelVerticesCount
            // 
            this.labelVerticesCount.AutoSize = true;
            this.labelVerticesCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelVerticesCount.Location = new System.Drawing.Point(3, 16);
            this.labelVerticesCount.Name = "labelVerticesCount";
            this.labelVerticesCount.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.labelVerticesCount.Size = new System.Drawing.Size(54, 23);
            this.labelVerticesCount.TabIndex = 0;
            this.labelVerticesCount.Text = "vertices";
            this.labelVerticesCount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.buttonSaveVegetation);
            this.groupBox11.Controls.Add(this.buttonSaveHeightmap);
            this.groupBox11.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox11.Location = new System.Drawing.Point(3, 3);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(209, 133);
            this.groupBox11.TabIndex = 6;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Export";
            // 
            // buttonSaveVegetation
            // 
            this.buttonSaveVegetation.Location = new System.Drawing.Point(23, 80);
            this.buttonSaveVegetation.Name = "buttonSaveVegetation";
            this.buttonSaveVegetation.Size = new System.Drawing.Size(166, 41);
            this.buttonSaveVegetation.TabIndex = 3;
            this.buttonSaveVegetation.Text = "Vegetation";
            this.buttonSaveVegetation.UseVisualStyleBackColor = true;
            this.buttonSaveVegetation.Click += new System.EventHandler(this.buttonSaveVegetation_Click);
            // 
            // buttonSaveHeightmap
            // 
            this.buttonSaveHeightmap.Location = new System.Drawing.Point(23, 28);
            this.buttonSaveHeightmap.Name = "buttonSaveHeightmap";
            this.buttonSaveHeightmap.Size = new System.Drawing.Size(166, 40);
            this.buttonSaveHeightmap.TabIndex = 2;
            this.buttonSaveHeightmap.Text = "Heightmap";
            this.buttonSaveHeightmap.UseVisualStyleBackColor = true;
            this.buttonSaveHeightmap.Click += new System.EventHandler(this.buttonSaveHeightmap_Click);
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.groupBox12);
            this.tabSettings.Controls.Add(this.groupBox4);
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(215, 631);
            this.tabSettings.TabIndex = 5;
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.cbSound);
            this.groupBox12.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox12.Location = new System.Drawing.Point(3, 167);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(209, 57);
            this.groupBox12.TabIndex = 1;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Sound effects";
            // 
            // cbSound
            // 
            this.cbSound.AutoSize = true;
            this.cbSound.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbSound.Checked = true;
            this.cbSound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSound.Location = new System.Drawing.Point(30, 24);
            this.cbSound.Name = "cbSound";
            this.cbSound.Size = new System.Drawing.Size(65, 17);
            this.cbSound.TabIndex = 0;
            this.cbSound.Text = "Enabled";
            this.cbSound.UseVisualStyleBackColor = true;
            this.cbSound.CheckedChanged += new System.EventHandler(this.cbSound_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.tbCameraJumpSpeed);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.tbCameraMovementSpeed);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(209, 164);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Camera";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(32, 135);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(145, 13);
            this.label15.TabIndex = 3;
            this.label15.Text = "Press F for First Person Mode";
            // 
            // tbCameraJumpSpeed
            // 
            this.tbCameraJumpSpeed.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tbCameraJumpSpeed.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbCameraJumpSpeed.Location = new System.Drawing.Point(3, 87);
            this.tbCameraJumpSpeed.Maximum = 600;
            this.tbCameraJumpSpeed.Minimum = 30;
            this.tbCameraJumpSpeed.Name = "tbCameraJumpSpeed";
            this.tbCameraJumpSpeed.Size = new System.Drawing.Size(203, 45);
            this.tbCameraJumpSpeed.TabIndex = 2;
            this.tbCameraJumpSpeed.Value = 300;
            this.tbCameraJumpSpeed.Scroll += new System.EventHandler(this.tbCameraJumpSpeed_Scroll);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Top;
            this.label12.Location = new System.Drawing.Point(3, 74);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "Jump speed:";
            // 
            // tbCameraMovementSpeed
            // 
            this.tbCameraMovementSpeed.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tbCameraMovementSpeed.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbCameraMovementSpeed.Location = new System.Drawing.Point(3, 29);
            this.tbCameraMovementSpeed.Maximum = 600;
            this.tbCameraMovementSpeed.Minimum = 30;
            this.tbCameraMovementSpeed.Name = "tbCameraMovementSpeed";
            this.tbCameraMovementSpeed.Size = new System.Drawing.Size(203, 45);
            this.tbCameraMovementSpeed.TabIndex = 0;
            this.tbCameraMovementSpeed.Value = 400;
            this.tbCameraMovementSpeed.Scroll += new System.EventHandler(this.tbCameraMovementSpeed_Scroll);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Top;
            this.label9.Location = new System.Drawing.Point(3, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(92, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Movement speed:";
            // 
            // visibleDataGridViewCheckBoxColumn
            // 
            this.visibleDataGridViewCheckBoxColumn.DataPropertyName = "Visible";
            this.visibleDataGridViewCheckBoxColumn.HeaderText = "Visible";
            this.visibleDataGridViewCheckBoxColumn.Name = "visibleDataGridViewCheckBoxColumn";
            // 
            // useWaitCursorDataGridViewCheckBoxColumn
            // 
            this.useWaitCursorDataGridViewCheckBoxColumn.DataPropertyName = "UseWaitCursor";
            this.useWaitCursorDataGridViewCheckBoxColumn.HeaderText = "UseWaitCursor";
            this.useWaitCursorDataGridViewCheckBoxColumn.Name = "useWaitCursorDataGridViewCheckBoxColumn";
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            // 
            // tabStopDataGridViewCheckBoxColumn
            // 
            this.tabStopDataGridViewCheckBoxColumn.DataPropertyName = "TabStop";
            this.tabStopDataGridViewCheckBoxColumn.HeaderText = "TabStop";
            this.tabStopDataGridViewCheckBoxColumn.Name = "tabStopDataGridViewCheckBoxColumn";
            // 
            // tabIndexDataGridViewTextBoxColumn
            // 
            this.tabIndexDataGridViewTextBoxColumn.DataPropertyName = "TabIndex";
            this.tabIndexDataGridViewTextBoxColumn.HeaderText = "TabIndex";
            this.tabIndexDataGridViewTextBoxColumn.Name = "tabIndexDataGridViewTextBoxColumn";
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            this.sizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            this.sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            this.sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            // 
            // rightToLeftDataGridViewTextBoxColumn
            // 
            this.rightToLeftDataGridViewTextBoxColumn.DataPropertyName = "RightToLeft";
            this.rightToLeftDataGridViewTextBoxColumn.HeaderText = "RightToLeft";
            this.rightToLeftDataGridViewTextBoxColumn.Name = "rightToLeftDataGridViewTextBoxColumn";
            // 
            // minimumSizeDataGridViewTextBoxColumn
            // 
            this.minimumSizeDataGridViewTextBoxColumn.DataPropertyName = "MinimumSize";
            this.minimumSizeDataGridViewTextBoxColumn.HeaderText = "MinimumSize";
            this.minimumSizeDataGridViewTextBoxColumn.Name = "minimumSizeDataGridViewTextBoxColumn";
            // 
            // maximumSizeDataGridViewTextBoxColumn
            // 
            this.maximumSizeDataGridViewTextBoxColumn.DataPropertyName = "MaximumSize";
            this.maximumSizeDataGridViewTextBoxColumn.HeaderText = "MaximumSize";
            this.maximumSizeDataGridViewTextBoxColumn.Name = "maximumSizeDataGridViewTextBoxColumn";
            // 
            // marginDataGridViewTextBoxColumn
            // 
            this.marginDataGridViewTextBoxColumn.DataPropertyName = "Margin";
            this.marginDataGridViewTextBoxColumn.HeaderText = "Margin";
            this.marginDataGridViewTextBoxColumn.Name = "marginDataGridViewTextBoxColumn";
            // 
            // locationDataGridViewTextBoxColumn
            // 
            this.locationDataGridViewTextBoxColumn.DataPropertyName = "Location";
            this.locationDataGridViewTextBoxColumn.HeaderText = "Location";
            this.locationDataGridViewTextBoxColumn.Name = "locationDataGridViewTextBoxColumn";
            // 
            // enabledDataGridViewCheckBoxColumn
            // 
            this.enabledDataGridViewCheckBoxColumn.DataPropertyName = "Enabled";
            this.enabledDataGridViewCheckBoxColumn.HeaderText = "Enabled";
            this.enabledDataGridViewCheckBoxColumn.Name = "enabledDataGridViewCheckBoxColumn";
            // 
            // dockDataGridViewTextBoxColumn
            // 
            this.dockDataGridViewTextBoxColumn.DataPropertyName = "Dock";
            this.dockDataGridViewTextBoxColumn.HeaderText = "Dock";
            this.dockDataGridViewTextBoxColumn.Name = "dockDataGridViewTextBoxColumn";
            // 
            // dataBindingsDataGridViewTextBoxColumn
            // 
            this.dataBindingsDataGridViewTextBoxColumn.DataPropertyName = "DataBindings";
            this.dataBindingsDataGridViewTextBoxColumn.HeaderText = "DataBindings";
            this.dataBindingsDataGridViewTextBoxColumn.Name = "dataBindingsDataGridViewTextBoxColumn";
            this.dataBindingsDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // cursorDataGridViewTextBoxColumn
            // 
            this.cursorDataGridViewTextBoxColumn.DataPropertyName = "Cursor";
            this.cursorDataGridViewTextBoxColumn.HeaderText = "Cursor";
            this.cursorDataGridViewTextBoxColumn.Name = "cursorDataGridViewTextBoxColumn";
            // 
            // contextMenuStripDataGridViewTextBoxColumn
            // 
            this.contextMenuStripDataGridViewTextBoxColumn.DataPropertyName = "ContextMenuStrip";
            this.contextMenuStripDataGridViewTextBoxColumn.HeaderText = "ContextMenuStrip";
            this.contextMenuStripDataGridViewTextBoxColumn.Name = "contextMenuStripDataGridViewTextBoxColumn";
            // 
            // causesValidationDataGridViewCheckBoxColumn
            // 
            this.causesValidationDataGridViewCheckBoxColumn.DataPropertyName = "CausesValidation";
            this.causesValidationDataGridViewCheckBoxColumn.HeaderText = "CausesValidation";
            this.causesValidationDataGridViewCheckBoxColumn.Name = "causesValidationDataGridViewCheckBoxColumn";
            // 
            // backColorDataGridViewTextBoxColumn
            // 
            this.backColorDataGridViewTextBoxColumn.DataPropertyName = "BackColor";
            this.backColorDataGridViewTextBoxColumn.HeaderText = "BackColor";
            this.backColorDataGridViewTextBoxColumn.Name = "backColorDataGridViewTextBoxColumn";
            // 
            // anchorDataGridViewTextBoxColumn
            // 
            this.anchorDataGridViewTextBoxColumn.DataPropertyName = "Anchor";
            this.anchorDataGridViewTextBoxColumn.HeaderText = "Anchor";
            this.anchorDataGridViewTextBoxColumn.Name = "anchorDataGridViewTextBoxColumn";
            // 
            // allowDropDataGridViewCheckBoxColumn
            // 
            this.allowDropDataGridViewCheckBoxColumn.DataPropertyName = "AllowDrop";
            this.allowDropDataGridViewCheckBoxColumn.HeaderText = "AllowDrop";
            this.allowDropDataGridViewCheckBoxColumn.Name = "allowDropDataGridViewCheckBoxColumn";
            // 
            // accessibleRoleDataGridViewTextBoxColumn
            // 
            this.accessibleRoleDataGridViewTextBoxColumn.DataPropertyName = "AccessibleRole";
            this.accessibleRoleDataGridViewTextBoxColumn.HeaderText = "AccessibleRole";
            this.accessibleRoleDataGridViewTextBoxColumn.Name = "accessibleRoleDataGridViewTextBoxColumn";
            // 
            // accessibleNameDataGridViewTextBoxColumn
            // 
            this.accessibleNameDataGridViewTextBoxColumn.DataPropertyName = "AccessibleName";
            this.accessibleNameDataGridViewTextBoxColumn.HeaderText = "AccessibleName";
            this.accessibleNameDataGridViewTextBoxColumn.Name = "accessibleNameDataGridViewTextBoxColumn";
            // 
            // accessibleDescriptionDataGridViewTextBoxColumn
            // 
            this.accessibleDescriptionDataGridViewTextBoxColumn.DataPropertyName = "AccessibleDescription";
            this.accessibleDescriptionDataGridViewTextBoxColumn.HeaderText = "AccessibleDescription";
            this.accessibleDescriptionDataGridViewTextBoxColumn.Name = "accessibleDescriptionDataGridViewTextBoxColumn";
            // 
            // valueDataGridViewTextBoxColumn
            // 
            this.valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
            this.valueDataGridViewTextBoxColumn.HeaderText = "Value";
            this.valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
            // 
            // tickFrequencyDataGridViewTextBoxColumn
            // 
            this.tickFrequencyDataGridViewTextBoxColumn.DataPropertyName = "TickFrequency";
            this.tickFrequencyDataGridViewTextBoxColumn.HeaderText = "TickFrequency";
            this.tickFrequencyDataGridViewTextBoxColumn.Name = "tickFrequencyDataGridViewTextBoxColumn";
            // 
            // tickStyleDataGridViewTextBoxColumn
            // 
            this.tickStyleDataGridViewTextBoxColumn.DataPropertyName = "TickStyle";
            this.tickStyleDataGridViewTextBoxColumn.HeaderText = "TickStyle";
            this.tickStyleDataGridViewTextBoxColumn.Name = "tickStyleDataGridViewTextBoxColumn";
            // 
            // smallChangeDataGridViewTextBoxColumn
            // 
            this.smallChangeDataGridViewTextBoxColumn.DataPropertyName = "SmallChange";
            this.smallChangeDataGridViewTextBoxColumn.HeaderText = "SmallChange";
            this.smallChangeDataGridViewTextBoxColumn.Name = "smallChangeDataGridViewTextBoxColumn";
            // 
            // rightToLeftLayoutDataGridViewCheckBoxColumn
            // 
            this.rightToLeftLayoutDataGridViewCheckBoxColumn.DataPropertyName = "RightToLeftLayout";
            this.rightToLeftLayoutDataGridViewCheckBoxColumn.HeaderText = "RightToLeftLayout";
            this.rightToLeftLayoutDataGridViewCheckBoxColumn.Name = "rightToLeftLayoutDataGridViewCheckBoxColumn";
            // 
            // orientationDataGridViewTextBoxColumn
            // 
            this.orientationDataGridViewTextBoxColumn.DataPropertyName = "Orientation";
            this.orientationDataGridViewTextBoxColumn.HeaderText = "Orientation";
            this.orientationDataGridViewTextBoxColumn.Name = "orientationDataGridViewTextBoxColumn";
            // 
            // minimumDataGridViewTextBoxColumn
            // 
            this.minimumDataGridViewTextBoxColumn.DataPropertyName = "Minimum";
            this.minimumDataGridViewTextBoxColumn.HeaderText = "Minimum";
            this.minimumDataGridViewTextBoxColumn.Name = "minimumDataGridViewTextBoxColumn";
            // 
            // maximumDataGridViewTextBoxColumn
            // 
            this.maximumDataGridViewTextBoxColumn.DataPropertyName = "Maximum";
            this.maximumDataGridViewTextBoxColumn.HeaderText = "Maximum";
            this.maximumDataGridViewTextBoxColumn.Name = "maximumDataGridViewTextBoxColumn";
            // 
            // largeChangeDataGridViewTextBoxColumn
            // 
            this.largeChangeDataGridViewTextBoxColumn.DataPropertyName = "LargeChange";
            this.largeChangeDataGridViewTextBoxColumn.HeaderText = "LargeChange";
            this.largeChangeDataGridViewTextBoxColumn.Name = "largeChangeDataGridViewTextBoxColumn";
            // 
            // autoSizeDataGridViewCheckBoxColumn
            // 
            this.autoSizeDataGridViewCheckBoxColumn.DataPropertyName = "AutoSize";
            this.autoSizeDataGridViewCheckBoxColumn.HeaderText = "AutoSize";
            this.autoSizeDataGridViewCheckBoxColumn.Name = "autoSizeDataGridViewCheckBoxColumn";
            // 
            // Value
            // 
            this.Value.DataPropertyName = "Value";
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            // 
            // Tag
            // 
            this.Tag.DataPropertyName = "Tag";
            this.Tag.HeaderText = "Property";
            this.Tag.Name = "Tag";
            this.Tag.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn1.HeaderText = "Property";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // saveFileHeightmap
            // 
            this.saveFileHeightmap.DefaultExt = "jpg";
            this.saveFileHeightmap.Filter = "Images|*.jpg";
            this.saveFileHeightmap.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // saveFileVegetation
            // 
            this.saveFileVegetation.AddExtension = false;
            this.saveFileVegetation.DefaultExt = "xml";
            this.saveFileVegetation.Filter = ".XML |*.xml";
            this.saveFileVegetation.Title = "Export vegetation to a -TgcScene.xml file";
            this.saveFileVegetation.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileVegetation_FileOk);
            // 
            // openFileVegetation
            // 
            this.openFileVegetation.Filter = "-TgcScene.xml |*-TgcScene.xml";
            this.openFileVegetation.Title = "Import vegetation from a -TgcScene.xml file";
            this.openFileVegetation.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileVegetation_FileOk);
            // 
            // TerrainEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.tabControl);
            this.Name = "TerrainEditorControl";
            this.Size = new System.Drawing.Size(223, 657);
            this.tabControl.ResumeLayout(false);
            this.pageTerrain.ResumeLayout(false);
            this.groupBoxModifyScale.ResumeLayout(false);
            this.groupBoxModifyScale.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleXZ)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModifyHeightmap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModifyTexture)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupWidth)).EndInit();
            this.pageEdit.ResumeLayout(false);
            this.brushSettings.ResumeLayout(false);
            this.brushSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbHardness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRadius)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabVegetation.ResumeLayout(false);
            this.tabVegetation.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbVegetationPreview)).EndInit();
            this.gbMode.ResumeLayout(false);
            this.gbMode.PerformLayout();
            this.pageExport.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbTerrain.ResumeLayout(false);
            this.gbTerrain.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbCameraJumpSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbCameraMovementSpeed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pageTerrain;
        private System.Windows.Forms.TabPage pageEdit;
        private System.Windows.Forms.DataGridViewCheckBoxColumn visibleDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn useWaitCursorDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn tabStopDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tabIndexDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rightToLeftDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn minimumSizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn maximumSizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn marginDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn locationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enabledDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dockDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataBindingsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cursorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn contextMenuStripDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn causesValidationDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn backColorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn anchorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn allowDropDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn accessibleRoleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn accessibleNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn accessibleDescriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tickFrequencyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tickStyleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn smallChangeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn rightToLeftLayoutDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn orientationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn minimumDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn maximumDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn largeChangeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn autoSizeDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tag;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.TabPage pageExport;
        private System.Windows.Forms.SaveFileDialog saveFileHeightmap;
        private System.Windows.Forms.GroupBox groupBoxModifyScale;
        private System.Windows.Forms.NumericUpDown nudScaleY;
        private System.Windows.Forms.NumericUpDown nudScaleXZ;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.PictureBox pictureBoxModifyHeightmap;
        private System.Windows.Forms.PictureBox pictureBoxModifyTexture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelModifyTextureImage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nupLevel;
        private System.Windows.Forms.NumericUpDown nupHeight;
        private System.Windows.Forms.NumericUpDown nupWidth;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox brushSettings;
        private System.Windows.Forms.CheckBox cbRounded;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar tbHardness;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar tbIntensity;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar tbRadius;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rbSteamroller;
        private System.Windows.Forms.RadioButton rbShovel;
        private System.Windows.Forms.CheckBox cbInvert;
        private System.Windows.Forms.TabPage tabVegetation;
        private System.Windows.Forms.SaveFileDialog saveFileVegetation;
        private System.Windows.Forms.Button bImportVegetation;
        private System.Windows.Forms.Button bReload;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ListBox lbVegetation;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.PictureBox pbVegetationPreview;
        private System.Windows.Forms.Button bVegetationClear;
        private System.Windows.Forms.OpenFileDialog openFileVegetation;
        private System.Windows.Forms.Button bChangeFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TrackBar tbCameraMovementSpeed;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar tbCameraJumpSpeed;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cbSz;
        private System.Windows.Forms.CheckBox cbSy;
        private System.Windows.Forms.CheckBox cbSx;
        private System.Windows.Forms.RadioButton rbRz;
        private System.Windows.Forms.RadioButton rbRy;
        private System.Windows.Forms.RadioButton rbRx;
        private System.Windows.Forms.Button bClearTF;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Button buttonSaveVegetation;
        private System.Windows.Forms.Button buttonSaveHeightmap;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lheight;
        private System.Windows.Forms.Label lwidth;
        private System.Windows.Forms.Label lname;
        private System.Windows.Forms.GroupBox gbTerrain;
        private System.Windows.Forms.Label lbScaleY;
        private System.Windows.Forms.Label lbScaleXZ;
        private System.Windows.Forms.Label lbCenter;
        private System.Windows.Forms.Label labelVerticesCount;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.CheckBox cbSound;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox gbMode;
        private System.Windows.Forms.RadioButton rbAddVegetation;
        private System.Windows.Forms.RadioButton rbPickVegetation;
        private System.Windows.Forms.Label label17;

    }
}
