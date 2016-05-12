namespace TGC.Core.UserControls.Networking
{
    partial class TgcNetworkingModifierClientsDialog
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
            this.dataGridViewConnectedClients = new System.Windows.Forms.DataGridView();
            this.labelConnectClients = new System.Windows.Forms.Label();
            this.buttonDeleteClient = new System.Windows.Forms.Button();
            this.ConnectedClientColumnNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectedClientColId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectedClientsColName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnectedClientsColAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConnectedClients)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewConnectedClients
            // 
            this.dataGridViewConnectedClients.AllowUserToAddRows = false;
            this.dataGridViewConnectedClients.AllowUserToDeleteRows = false;
            this.dataGridViewConnectedClients.AllowUserToResizeColumns = false;
            this.dataGridViewConnectedClients.AllowUserToResizeRows = false;
            this.dataGridViewConnectedClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewConnectedClients.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ConnectedClientColumnNum,
            this.ConnectedClientColId,
            this.ConnectedClientsColName,
            this.ConnectedClientsColAddress});
            this.dataGridViewConnectedClients.Location = new System.Drawing.Point(12, 26);
            this.dataGridViewConnectedClients.MultiSelect = false;
            this.dataGridViewConnectedClients.Name = "dataGridViewConnectedClients";
            this.dataGridViewConnectedClients.ReadOnly = true;
            this.dataGridViewConnectedClients.RowHeadersVisible = false;
            this.dataGridViewConnectedClients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewConnectedClients.Size = new System.Drawing.Size(474, 141);
            this.dataGridViewConnectedClients.TabIndex = 6;
            this.dataGridViewConnectedClients.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewConnectedClients_RowEnter);
            // 
            // labelConnectClients
            // 
            this.labelConnectClients.AutoSize = true;
            this.labelConnectClients.Location = new System.Drawing.Point(12, 9);
            this.labelConnectClients.Name = "labelConnectClients";
            this.labelConnectClients.Size = new System.Drawing.Size(95, 13);
            this.labelConnectClients.TabIndex = 7;
            this.labelConnectClients.Text = "Connected clients:";
            // 
            // buttonDeleteClient
            // 
            this.buttonDeleteClient.Location = new System.Drawing.Point(12, 172);
            this.buttonDeleteClient.Name = "buttonDeleteClient";
            this.buttonDeleteClient.Size = new System.Drawing.Size(92, 24);
            this.buttonDeleteClient.TabIndex = 8;
            this.buttonDeleteClient.Text = "Delete client";
            this.buttonDeleteClient.UseVisualStyleBackColor = true;
            this.buttonDeleteClient.Click += new System.EventHandler(this.buttonDeleteClient_Click);
            // 
            // ConnectedClientColumnNum
            // 
            this.ConnectedClientColumnNum.HeaderText = "#";
            this.ConnectedClientColumnNum.Name = "ConnectedClientColumnNum";
            this.ConnectedClientColumnNum.ReadOnly = true;
            this.ConnectedClientColumnNum.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ConnectedClientColumnNum.Width = 20;
            // 
            // ConnectedClientColId
            // 
            this.ConnectedClientColId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ConnectedClientColId.HeaderText = "Player ID";
            this.ConnectedClientColId.Name = "ConnectedClientColId";
            this.ConnectedClientColId.ReadOnly = true;
            this.ConnectedClientColId.Width = 75;
            // 
            // ConnectedClientsColName
            // 
            this.ConnectedClientsColName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ConnectedClientsColName.HeaderText = "Client name";
            this.ConnectedClientsColName.Name = "ConnectedClientsColName";
            this.ConnectedClientsColName.ReadOnly = true;
            this.ConnectedClientsColName.Width = 87;
            // 
            // ConnectedClientsColAddress
            // 
            this.ConnectedClientsColAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ConnectedClientsColAddress.HeaderText = "Address";
            this.ConnectedClientsColAddress.Name = "ConnectedClientsColAddress";
            this.ConnectedClientsColAddress.ReadOnly = true;
            // 
            // TgcNetworkingModifierClientsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 207);
            this.Controls.Add(this.buttonDeleteClient);
            this.Controls.Add(this.labelConnectClients);
            this.Controls.Add(this.dataGridViewConnectedClients);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TgcNetworkingModifierClientsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connected Clients";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConnectedClients)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewConnectedClients;
        private System.Windows.Forms.Label labelConnectClients;
        private System.Windows.Forms.Button buttonDeleteClient;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectedClientColumnNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectedClientColId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectedClientsColName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectedClientsColAddress;
    }
}