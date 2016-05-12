namespace TGC.Core.UserControls.Networking
{
    partial class TgcNetworkingModifierControl
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
            this.buttonCreateServer = new System.Windows.Forms.Button();
            this.groupBoxServer = new System.Windows.Forms.GroupBox();
            this.buttonConnectedClients = new System.Windows.Forms.Button();
            this.buttonCloseServer = new System.Windows.Forms.Button();
            this.textBoxServerName = new System.Windows.Forms.TextBox();
            this.groupBoxClient = new System.Windows.Forms.GroupBox();
            this.buttonJoinServer = new System.Windows.Forms.Button();
            this.textBoxPlayerId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBoxCurrentServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelServerName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIp = new System.Windows.Forms.TextBox();
            this.textBoxServerIp = new System.Windows.Forms.TextBox();
            this.groupBoxServer.SuspendLayout();
            this.groupBoxClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateServer
            // 
            this.buttonCreateServer.Location = new System.Drawing.Point(8, 57);
            this.buttonCreateServer.Name = "buttonCreateServer";
            this.buttonCreateServer.Size = new System.Drawing.Size(86, 23);
            this.buttonCreateServer.TabIndex = 0;
            this.buttonCreateServer.Text = "Create Server";
            this.buttonCreateServer.UseVisualStyleBackColor = true;
            this.buttonCreateServer.Click += new System.EventHandler(this.buttonCreateServer_Click);
            // 
            // groupBoxServer
            // 
            this.groupBoxServer.Controls.Add(this.buttonConnectedClients);
            this.groupBoxServer.Controls.Add(this.buttonCloseServer);
            this.groupBoxServer.Controls.Add(this.textBoxServerName);
            this.groupBoxServer.Controls.Add(this.buttonCreateServer);
            this.groupBoxServer.Controls.Add(this.labelServerName);
            this.groupBoxServer.Location = new System.Drawing.Point(3, 26);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(100, 133);
            this.groupBoxServer.TabIndex = 1;
            this.groupBoxServer.TabStop = false;
            this.groupBoxServer.Text = "Server";
            // 
            // buttonConnectedClients
            // 
            this.buttonConnectedClients.Location = new System.Drawing.Point(1, 105);
            this.buttonConnectedClients.Name = "buttonConnectedClients";
            this.buttonConnectedClients.Size = new System.Drawing.Size(100, 23);
            this.buttonConnectedClients.TabIndex = 3;
            this.buttonConnectedClients.Text = "Connected clients";
            this.buttonConnectedClients.UseVisualStyleBackColor = true;
            this.buttonConnectedClients.Click += new System.EventHandler(this.buttonConnectedClients_Click);
            // 
            // buttonCloseServer
            // 
            this.buttonCloseServer.Location = new System.Drawing.Point(29, 81);
            this.buttonCloseServer.Name = "buttonCloseServer";
            this.buttonCloseServer.Size = new System.Drawing.Size(41, 23);
            this.buttonCloseServer.TabIndex = 2;
            this.buttonCloseServer.Text = "Close";
            this.buttonCloseServer.UseVisualStyleBackColor = true;
            this.buttonCloseServer.Click += new System.EventHandler(this.buttonCloseServer_Click);
            // 
            // textBoxServerName
            // 
            this.textBoxServerName.BackColor = System.Drawing.Color.Red;
            this.textBoxServerName.ForeColor = System.Drawing.SystemColors.Menu;
            this.textBoxServerName.Location = new System.Drawing.Point(6, 32);
            this.textBoxServerName.Name = "textBoxServerName";
            this.textBoxServerName.Size = new System.Drawing.Size(91, 20);
            this.textBoxServerName.TabIndex = 1;
            this.textBoxServerName.Text = "MyServer";
            this.textBoxServerName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBoxClient
            // 
            this.groupBoxClient.Controls.Add(this.textBoxServerIp);
            this.groupBoxClient.Controls.Add(this.buttonJoinServer);
            this.groupBoxClient.Controls.Add(this.textBoxPlayerId);
            this.groupBoxClient.Controls.Add(this.label3);
            this.groupBoxClient.Controls.Add(this.buttonDisconnect);
            this.groupBoxClient.Controls.Add(this.textBoxCurrentServer);
            this.groupBoxClient.Controls.Add(this.label2);
            this.groupBoxClient.Location = new System.Drawing.Point(3, 165);
            this.groupBoxClient.Name = "groupBoxClient";
            this.groupBoxClient.Size = new System.Drawing.Size(100, 175);
            this.groupBoxClient.TabIndex = 2;
            this.groupBoxClient.TabStop = false;
            this.groupBoxClient.Text = "Client";
            // 
            // buttonJoinServer
            // 
            this.buttonJoinServer.Location = new System.Drawing.Point(1, 16);
            this.buttonJoinServer.Name = "buttonJoinServer";
            this.buttonJoinServer.Size = new System.Drawing.Size(100, 23);
            this.buttonJoinServer.TabIndex = 4;
            this.buttonJoinServer.Text = "Join server";
            this.buttonJoinServer.UseVisualStyleBackColor = true;
            this.buttonJoinServer.Click += new System.EventHandler(this.buttonJoinServer_Click);
            // 
            // textBoxPlayerId
            // 
            this.textBoxPlayerId.Location = new System.Drawing.Point(6, 121);
            this.textBoxPlayerId.Name = "textBoxPlayerId";
            this.textBoxPlayerId.ReadOnly = true;
            this.textBoxPlayerId.Size = new System.Drawing.Size(91, 20);
            this.textBoxPlayerId.TabIndex = 14;
            this.textBoxPlayerId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label3.Location = new System.Drawing.Point(6, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Player ID";
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(11, 144);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(74, 21);
            this.buttonDisconnect.TabIndex = 12;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // textBoxCurrentServer
            // 
            this.textBoxCurrentServer.BackColor = System.Drawing.Color.Red;
            this.textBoxCurrentServer.ForeColor = System.Drawing.SystemColors.Menu;
            this.textBoxCurrentServer.Location = new System.Drawing.Point(6, 58);
            this.textBoxCurrentServer.Name = "textBoxCurrentServer";
            this.textBoxCurrentServer.ReadOnly = true;
            this.textBoxCurrentServer.Size = new System.Drawing.Size(91, 20);
            this.textBoxCurrentServer.TabIndex = 11;
            this.textBoxCurrentServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label2.Location = new System.Drawing.Point(6, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Current server:";
            // 
            // labelServerName
            // 
            this.labelServerName.AutoSize = true;
            this.labelServerName.Location = new System.Drawing.Point(6, 16);
            this.labelServerName.Name = "labelServerName";
            this.labelServerName.Size = new System.Drawing.Size(70, 13);
            this.labelServerName.TabIndex = 0;
            this.labelServerName.Text = "Server name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP:";
            // 
            // textBoxIp
            // 
            this.textBoxIp.Location = new System.Drawing.Point(29, 7);
            this.textBoxIp.Name = "textBoxIp";
            this.textBoxIp.ReadOnly = true;
            this.textBoxIp.Size = new System.Drawing.Size(74, 20);
            this.textBoxIp.TabIndex = 4;
            this.textBoxIp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxServerIp
            // 
            this.textBoxServerIp.Location = new System.Drawing.Point(6, 80);
            this.textBoxServerIp.Name = "textBoxServerIp";
            this.textBoxServerIp.ReadOnly = true;
            this.textBoxServerIp.Size = new System.Drawing.Size(91, 20);
            this.textBoxServerIp.TabIndex = 15;
            this.textBoxServerIp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TgcNetworkingModifierControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxIp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBoxClient);
            this.Controls.Add(this.groupBoxServer);
            this.Name = "TgcNetworkingModifierControl";
            this.Size = new System.Drawing.Size(106, 350);
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.groupBoxClient.ResumeLayout(false);
            this.groupBoxClient.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCreateServer;
        private System.Windows.Forms.GroupBox groupBoxServer;
        private System.Windows.Forms.Button buttonCloseServer;
        private System.Windows.Forms.TextBox textBoxServerName;
        private System.Windows.Forms.GroupBox groupBoxClient;
        private System.Windows.Forms.TextBox textBoxCurrentServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBoxPlayerId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonConnectedClients;
        private System.Windows.Forms.Button buttonJoinServer;
        private System.Windows.Forms.Label labelServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIp;
        private System.Windows.Forms.TextBox textBoxServerIp;
    }
}
