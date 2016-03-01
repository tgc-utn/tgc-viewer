namespace TGC.Viewer.Utils.Networking
{
    partial class TgcNetworkingModifierServersDialog
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
            this.dataGridViewAvaliableServers = new System.Windows.Forms.DataGridView();
            this.AvaliableServersColNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvaliableServersColName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvaliableServersColAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonJoin = new System.Windows.Forms.Button();
            this.textBoxClientName = new System.Windows.Forms.TextBox();
            this.labelClientName = new System.Windows.Forms.Label();
            this.buttonAddServer = new System.Windows.Forms.Button();
            this.textBoxAddServer = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAvaliableServers)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewAvaliableServers
            // 
            this.dataGridViewAvaliableServers.AllowUserToAddRows = false;
            this.dataGridViewAvaliableServers.AllowUserToDeleteRows = false;
            this.dataGridViewAvaliableServers.AllowUserToResizeColumns = false;
            this.dataGridViewAvaliableServers.AllowUserToResizeRows = false;
            this.dataGridViewAvaliableServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAvaliableServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.AvaliableServersColNum,
            this.AvaliableServersColName,
            this.AvaliableServersColAddress});
            this.dataGridViewAvaliableServers.Location = new System.Drawing.Point(12, 68);
            this.dataGridViewAvaliableServers.MultiSelect = false;
            this.dataGridViewAvaliableServers.Name = "dataGridViewAvaliableServers";
            this.dataGridViewAvaliableServers.RowHeadersVisible = false;
            this.dataGridViewAvaliableServers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAvaliableServers.Size = new System.Drawing.Size(474, 141);
            this.dataGridViewAvaliableServers.TabIndex = 8;
            this.dataGridViewAvaliableServers.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAvaliableServers_RowEnter);
            // 
            // AvaliableServersColNum
            // 
            this.AvaliableServersColNum.HeaderText = "#";
            this.AvaliableServersColNum.Name = "AvaliableServersColNum";
            this.AvaliableServersColNum.ReadOnly = true;
            this.AvaliableServersColNum.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.AvaliableServersColNum.Width = 20;
            // 
            // AvaliableServersColName
            // 
            this.AvaliableServersColName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.AvaliableServersColName.HeaderText = "Server name";
            this.AvaliableServersColName.Name = "AvaliableServersColName";
            this.AvaliableServersColName.ReadOnly = true;
            this.AvaliableServersColName.Width = 92;
            // 
            // AvaliableServersColAddress
            // 
            this.AvaliableServersColAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.AvaliableServersColAddress.HeaderText = "Address";
            this.AvaliableServersColAddress.Name = "AvaliableServersColAddress";
            this.AvaliableServersColAddress.ReadOnly = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(12, 35);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(131, 27);
            this.buttonRefresh.TabIndex = 9;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefreshServers_Click);
            // 
            // buttonJoin
            // 
            this.buttonJoin.Location = new System.Drawing.Point(204, 1);
            this.buttonJoin.Name = "buttonJoin";
            this.buttonJoin.Size = new System.Drawing.Size(103, 27);
            this.buttonJoin.TabIndex = 10;
            this.buttonJoin.Text = "Join server";
            this.buttonJoin.UseVisualStyleBackColor = true;
            this.buttonJoin.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // textBoxClientName
            // 
            this.textBoxClientName.Location = new System.Drawing.Point(83, 5);
            this.textBoxClientName.Name = "textBoxClientName";
            this.textBoxClientName.Size = new System.Drawing.Size(115, 20);
            this.textBoxClientName.TabIndex = 12;
            this.textBoxClientName.Text = "MyClient";
            this.textBoxClientName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelClientName
            // 
            this.labelClientName.AutoSize = true;
            this.labelClientName.Location = new System.Drawing.Point(12, 8);
            this.labelClientName.Name = "labelClientName";
            this.labelClientName.Size = new System.Drawing.Size(65, 13);
            this.labelClientName.TabIndex = 11;
            this.labelClientName.Text = "Client name:";
            // 
            // buttonAddServer
            // 
            this.buttonAddServer.Location = new System.Drawing.Point(289, 35);
            this.buttonAddServer.Name = "buttonAddServer";
            this.buttonAddServer.Size = new System.Drawing.Size(76, 27);
            this.buttonAddServer.TabIndex = 13;
            this.buttonAddServer.Text = "Add Server";
            this.buttonAddServer.UseVisualStyleBackColor = true;
            this.buttonAddServer.Click += new System.EventHandler(this.buttonAddServer_Click);
            // 
            // textBoxAddServer
            // 
            this.textBoxAddServer.Location = new System.Drawing.Point(371, 39);
            this.textBoxAddServer.Name = "textBoxAddServer";
            this.textBoxAddServer.Size = new System.Drawing.Size(115, 20);
            this.textBoxAddServer.TabIndex = 14;
            this.textBoxAddServer.Text = "198.168.2.100";
            this.textBoxAddServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TgcNetworkingModifierServersDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 226);
            this.Controls.Add(this.textBoxAddServer);
            this.Controls.Add(this.buttonAddServer);
            this.Controls.Add(this.textBoxClientName);
            this.Controls.Add(this.labelClientName);
            this.Controls.Add(this.buttonJoin);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.dataGridViewAvaliableServers);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TgcNetworkingModifierServersDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Avaliable servers";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAvaliableServers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewAvaliableServers;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonJoin;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvaliableServersColNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvaliableServersColName;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvaliableServersColAddress;
        private System.Windows.Forms.TextBox textBoxClientName;
        private System.Windows.Forms.Label labelClientName;
        private System.Windows.Forms.Button buttonAddServer;
        private System.Windows.Forms.TextBox textBoxAddServer;
    }
}