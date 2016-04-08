namespace TGC.Examples.RoomsEditor
{
    partial class RoomsEditorMapView
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
            this.panel2d = new System.Windows.Forms.Panel();
            this.groupBoxCursorMode = new System.Windows.Forms.GroupBox();
            this.radioButtonCreateRoom = new System.Windows.Forms.RadioButton();
            this.groupBoxMapSettings = new System.Windows.Forms.GroupBox();
            this.numericUpDownMapHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownMapWidth = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCreate3dMap = new System.Windows.Forms.Button();
            this.panel2dContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxEditRoom = new System.Windows.Forms.GroupBox();
            this.numericUpDownRoomFloorLevel = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.numericUpDownRoomHeight = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonWallTextures = new System.Windows.Forms.Button();
            this.numericUpDownRoomLength = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericUpDownRoomWidth = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownRoomPosY = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownRoomPosX = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxRoomName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMapScaleX = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDownMapScaleZ = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDownMapScaleY = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBoxCursorMode.SuspendLayout();
            this.groupBoxMapSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapWidth)).BeginInit();
            this.panel2dContainer.SuspendLayout();
            this.groupBoxEditRoom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomFloorLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomPosX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleX)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleY)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2d
            // 
            this.panel2d.AutoSize = true;
            this.panel2d.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.panel2d.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2d.Location = new System.Drawing.Point(3, 3);
            this.panel2d.MinimumSize = new System.Drawing.Size(600, 600);
            this.panel2d.Name = "panel2d";
            this.panel2d.Size = new System.Drawing.Size(600, 600);
            this.panel2d.TabIndex = 0;
            this.panel2d.MouseLeave += new System.EventHandler(this.panel2d_MouseLeave);
            this.panel2d.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel2d_MouseMove);
            this.panel2d.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2d_MouseDown);
            this.panel2d.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel2d_MouseUp);
            this.panel2d.MouseEnter += new System.EventHandler(this.panel2d_MouseEnter);
            // 
            // groupBoxCursorMode
            // 
            this.groupBoxCursorMode.Controls.Add(this.radioButtonCreateRoom);
            this.groupBoxCursorMode.Location = new System.Drawing.Point(579, 176);
            this.groupBoxCursorMode.Name = "groupBoxCursorMode";
            this.groupBoxCursorMode.Size = new System.Drawing.Size(149, 54);
            this.groupBoxCursorMode.TabIndex = 1;
            this.groupBoxCursorMode.TabStop = false;
            this.groupBoxCursorMode.Text = "Cursor mode";
            // 
            // radioButtonCreateRoom
            // 
            this.radioButtonCreateRoom.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButtonCreateRoom.Location = new System.Drawing.Point(36, 19);
            this.radioButtonCreateRoom.Name = "radioButtonCreateRoom";
            this.radioButtonCreateRoom.Size = new System.Drawing.Size(74, 23);
            this.radioButtonCreateRoom.TabIndex = 3;
            this.radioButtonCreateRoom.TabStop = true;
            this.radioButtonCreateRoom.Text = "Create room";
            this.radioButtonCreateRoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonCreateRoom.UseVisualStyleBackColor = true;
            this.radioButtonCreateRoom.CheckedChanged += new System.EventHandler(this.radioButtonCreateRoom_CheckedChanged);
            // 
            // groupBoxMapSettings
            // 
            this.groupBoxMapSettings.Controls.Add(this.numericUpDownMapHeight);
            this.groupBoxMapSettings.Controls.Add(this.label2);
            this.groupBoxMapSettings.Controls.Add(this.numericUpDownMapWidth);
            this.groupBoxMapSettings.Controls.Add(this.label1);
            this.groupBoxMapSettings.Location = new System.Drawing.Point(579, 12);
            this.groupBoxMapSettings.Name = "groupBoxMapSettings";
            this.groupBoxMapSettings.Size = new System.Drawing.Size(149, 70);
            this.groupBoxMapSettings.TabIndex = 2;
            this.groupBoxMapSettings.TabStop = false;
            this.groupBoxMapSettings.Text = "Map settings";
            // 
            // numericUpDownMapHeight
            // 
            this.numericUpDownMapHeight.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMapHeight.Location = new System.Drawing.Point(62, 40);
            this.numericUpDownMapHeight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownMapHeight.Minimum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numericUpDownMapHeight.Name = "numericUpDownMapHeight";
            this.numericUpDownMapHeight.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownMapHeight.TabIndex = 3;
            this.numericUpDownMapHeight.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMapHeight.ValueChanged += new System.EventHandler(this.numericUpDownMapHeight_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Height";
            // 
            // numericUpDownMapWidth
            // 
            this.numericUpDownMapWidth.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMapWidth.Location = new System.Drawing.Point(62, 14);
            this.numericUpDownMapWidth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownMapWidth.Minimum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numericUpDownMapWidth.Name = "numericUpDownMapWidth";
            this.numericUpDownMapWidth.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownMapWidth.TabIndex = 1;
            this.numericUpDownMapWidth.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMapWidth.ValueChanged += new System.EventHandler(this.numericUpDownMapWidth_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width";
            // 
            // buttonCreate3dMap
            // 
            this.buttonCreate3dMap.Location = new System.Drawing.Point(591, 480);
            this.buttonCreate3dMap.Name = "buttonCreate3dMap";
            this.buttonCreate3dMap.Size = new System.Drawing.Size(137, 29);
            this.buttonCreate3dMap.TabIndex = 3;
            this.buttonCreate3dMap.Text = "Update 3D Map";
            this.buttonCreate3dMap.UseVisualStyleBackColor = true;
            this.buttonCreate3dMap.Click += new System.EventHandler(this.buttonCreate3dMap_Click);
            // 
            // panel2dContainer
            // 
            this.panel2dContainer.AutoScroll = true;
            this.panel2dContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2dContainer.Controls.Add(this.panel2d);
            this.panel2dContainer.Location = new System.Drawing.Point(12, 12);
            this.panel2dContainer.Name = "panel2dContainer";
            this.panel2dContainer.Size = new System.Drawing.Size(561, 497);
            this.panel2dContainer.TabIndex = 4;
            this.panel2dContainer.WrapContents = false;
            // 
            // groupBoxEditRoom
            // 
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomFloorLevel);
            this.groupBoxEditRoom.Controls.Add(this.label12);
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomHeight);
            this.groupBoxEditRoom.Controls.Add(this.label11);
            this.groupBoxEditRoom.Controls.Add(this.buttonWallTextures);
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomLength);
            this.groupBoxEditRoom.Controls.Add(this.label7);
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomWidth);
            this.groupBoxEditRoom.Controls.Add(this.label6);
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomPosY);
            this.groupBoxEditRoom.Controls.Add(this.label5);
            this.groupBoxEditRoom.Controls.Add(this.numericUpDownRoomPosX);
            this.groupBoxEditRoom.Controls.Add(this.label4);
            this.groupBoxEditRoom.Controls.Add(this.textBoxRoomName);
            this.groupBoxEditRoom.Controls.Add(this.label3);
            this.groupBoxEditRoom.Location = new System.Drawing.Point(579, 236);
            this.groupBoxEditRoom.Name = "groupBoxEditRoom";
            this.groupBoxEditRoom.Size = new System.Drawing.Size(149, 224);
            this.groupBoxEditRoom.TabIndex = 5;
            this.groupBoxEditRoom.TabStop = false;
            this.groupBoxEditRoom.Text = "Edit room";
            // 
            // numericUpDownRoomFloorLevel
            // 
            this.numericUpDownRoomFloorLevel.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomFloorLevel.Location = new System.Drawing.Point(77, 169);
            this.numericUpDownRoomFloorLevel.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownRoomFloorLevel.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDownRoomFloorLevel.Name = "numericUpDownRoomFloorLevel";
            this.numericUpDownRoomFloorLevel.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownRoomFloorLevel.TabIndex = 16;
            this.numericUpDownRoomFloorLevel.ValueChanged += new System.EventHandler(this.numericUpDownRoomFloorLevel_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 171);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Floor level";
            // 
            // numericUpDownRoomHeight
            // 
            this.numericUpDownRoomHeight.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomHeight.Location = new System.Drawing.Point(77, 143);
            this.numericUpDownRoomHeight.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownRoomHeight.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomHeight.Name = "numericUpDownRoomHeight";
            this.numericUpDownRoomHeight.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownRoomHeight.TabIndex = 14;
            this.numericUpDownRoomHeight.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownRoomHeight.ValueChanged += new System.EventHandler(this.numericUpDownRoomHeight_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 145);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "Room height";
            // 
            // buttonWallTextures
            // 
            this.buttonWallTextures.Location = new System.Drawing.Point(6, 195);
            this.buttonWallTextures.Name = "buttonWallTextures";
            this.buttonWallTextures.Size = new System.Drawing.Size(134, 23);
            this.buttonWallTextures.TabIndex = 12;
            this.buttonWallTextures.Text = "Wall textures";
            this.buttonWallTextures.UseVisualStyleBackColor = true;
            this.buttonWallTextures.Click += new System.EventHandler(this.buttonWallTextures_Click);
            // 
            // numericUpDownRoomLength
            // 
            this.numericUpDownRoomLength.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomLength.Location = new System.Drawing.Point(62, 117);
            this.numericUpDownRoomLength.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownRoomLength.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomLength.Name = "numericUpDownRoomLength";
            this.numericUpDownRoomLength.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownRoomLength.TabIndex = 11;
            this.numericUpDownRoomLength.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomLength.ValueChanged += new System.EventHandler(this.numericUpDownRoomLength_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 119);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Length";
            // 
            // numericUpDownRoomWidth
            // 
            this.numericUpDownRoomWidth.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomWidth.Location = new System.Drawing.Point(62, 91);
            this.numericUpDownRoomWidth.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownRoomWidth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomWidth.Name = "numericUpDownRoomWidth";
            this.numericUpDownRoomWidth.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownRoomWidth.TabIndex = 9;
            this.numericUpDownRoomWidth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomWidth.ValueChanged += new System.EventHandler(this.numericUpDownRoomWidth_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 93);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Width";
            // 
            // numericUpDownRoomPosY
            // 
            this.numericUpDownRoomPosY.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomPosY.Location = new System.Drawing.Point(62, 65);
            this.numericUpDownRoomPosY.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownRoomPosY.Name = "numericUpDownRoomPosY";
            this.numericUpDownRoomPosY.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownRoomPosY.TabIndex = 7;
            this.numericUpDownRoomPosY.ValueChanged += new System.EventHandler(this.numericUpDownRoomPosY_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Pos Y";
            // 
            // numericUpDownRoomPosX
            // 
            this.numericUpDownRoomPosX.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownRoomPosX.Location = new System.Drawing.Point(62, 39);
            this.numericUpDownRoomPosX.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownRoomPosX.Name = "numericUpDownRoomPosX";
            this.numericUpDownRoomPosX.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownRoomPosX.TabIndex = 5;
            this.numericUpDownRoomPosX.ValueChanged += new System.EventHandler(this.numericUpDownRoomPosX_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Pos X";
            // 
            // textBoxRoomName
            // 
            this.textBoxRoomName.Location = new System.Drawing.Point(47, 13);
            this.textBoxRoomName.Name = "textBoxRoomName";
            this.textBoxRoomName.Size = new System.Drawing.Size(96, 20);
            this.textBoxRoomName.TabIndex = 1;
            this.textBoxRoomName.TextChanged += new System.EventHandler(this.textBoxRoomName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Name";
            // 
            // numericUpDownMapScaleX
            // 
            this.numericUpDownMapScaleX.DecimalPlaces = 2;
            this.numericUpDownMapScaleX.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleX.Location = new System.Drawing.Point(62, 13);
            this.numericUpDownMapScaleX.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownMapScaleX.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleX.Name = "numericUpDownMapScaleX";
            this.numericUpDownMapScaleX.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownMapScaleX.TabIndex = 5;
            this.numericUpDownMapScaleX.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Scale X";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDownMapScaleZ);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.numericUpDownMapScaleY);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.numericUpDownMapScaleX);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(579, 88);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(149, 81);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Map scale";
            // 
            // numericUpDownMapScaleZ
            // 
            this.numericUpDownMapScaleZ.DecimalPlaces = 2;
            this.numericUpDownMapScaleZ.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleZ.Location = new System.Drawing.Point(62, 55);
            this.numericUpDownMapScaleZ.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownMapScaleZ.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleZ.Name = "numericUpDownMapScaleZ";
            this.numericUpDownMapScaleZ.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownMapScaleZ.TabIndex = 9;
            this.numericUpDownMapScaleZ.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 57);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(44, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Scale Z";
            // 
            // numericUpDownMapScaleY
            // 
            this.numericUpDownMapScaleY.DecimalPlaces = 2;
            this.numericUpDownMapScaleY.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleY.Location = new System.Drawing.Point(62, 34);
            this.numericUpDownMapScaleY.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownMapScaleY.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownMapScaleY.Name = "numericUpDownMapScaleY";
            this.numericUpDownMapScaleY.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownMapScaleY.TabIndex = 7;
            this.numericUpDownMapScaleY.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "Scale Y";
            // 
            // RoomsEditorMapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 521);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxEditRoom);
            this.Controls.Add(this.panel2dContainer);
            this.Controls.Add(this.buttonCreate3dMap);
            this.Controls.Add(this.groupBoxMapSettings);
            this.Controls.Add(this.groupBoxCursorMode);
            this.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RoomsEditorMapView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Map view";
            this.groupBoxCursorMode.ResumeLayout(false);
            this.groupBoxMapSettings.ResumeLayout(false);
            this.groupBoxMapSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapWidth)).EndInit();
            this.panel2dContainer.ResumeLayout(false);
            this.panel2dContainer.PerformLayout();
            this.groupBoxEditRoom.ResumeLayout(false);
            this.groupBoxEditRoom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomFloorLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRoomPosX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleX)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMapScaleY)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2d;
        private System.Windows.Forms.GroupBox groupBoxCursorMode;
        private System.Windows.Forms.RadioButton radioButtonCreateRoom;
        private System.Windows.Forms.GroupBox groupBoxMapSettings;
        private System.Windows.Forms.Button buttonCreate3dMap;
        private System.Windows.Forms.FlowLayoutPanel panel2dContainer;
        private System.Windows.Forms.NumericUpDown numericUpDownMapWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownMapHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxEditRoom;
        private System.Windows.Forms.TextBox textBoxRoomName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomPosX;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomLength;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomPosY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonWallTextures;
        private System.Windows.Forms.NumericUpDown numericUpDownMapScaleX;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numericUpDownMapScaleZ;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDownMapScaleY;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomHeight;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDownRoomFloorLevel;
        private System.Windows.Forms.Label label12;
    }
}