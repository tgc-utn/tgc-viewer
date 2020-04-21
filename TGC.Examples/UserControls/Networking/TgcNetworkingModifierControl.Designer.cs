namespace TGC.Examples.UserControls.Networking
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIp = new System.Windows.Forms.TextBox();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.groupBoxClient = new System.Windows.Forms.GroupBox();
            this.textBoxServerIp = new System.Windows.Forms.TextBox();
            this.buttonJoinServer = new System.Windows.Forms.Button();
            this.textBoxPlayerId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBoxCurrentServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxServer = new System.Windows.Forms.GroupBox();
            this.buttonConnectedClients = new System.Windows.Forms.Button();
            this.buttonCloseServer = new System.Windows.Forms.Button();
            this.textBoxServerName = new System.Windows.Forms.TextBox();
            this.buttonCreateServer = new System.Windows.Forms.Button();
            this.labelServerName = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tgcModifierTitleBar = new TGC.Examples.UserControls.Modifier.TGCModifierTitleBar();
            this.contentPanel.SuspendLayout();
            this.groupBoxClient.SuspendLayout();
            this.groupBoxServer.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP:";
            // 
            // textBoxIp
            // 
            this.textBoxIp.Location = new System.Drawing.Point(29, 27);
            this.textBoxIp.Name = "textBoxIp";
            this.textBoxIp.ReadOnly = true;
            this.textBoxIp.Size = new System.Drawing.Size(150, 20);
            this.textBoxIp.TabIndex = 4;
            this.textBoxIp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // contentPanel
            // 
            this.contentPanel.Controls.Add(this.groupBoxClient);
            this.contentPanel.Controls.Add(this.groupBoxServer);
            this.contentPanel.Controls.Add(this.panel1);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(0, 0);
            this.contentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(200, 400);
            this.contentPanel.TabIndex = 6;
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
            this.groupBoxClient.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxClient.Location = new System.Drawing.Point(0, 185);
            this.groupBoxClient.Name = "groupBoxClient";
            this.groupBoxClient.Size = new System.Drawing.Size(200, 180);
            this.groupBoxClient.TabIndex = 7;
            this.groupBoxClient.TabStop = false;
            this.groupBoxClient.Text = "Client";
            // 
            // textBoxServerIp
            // 
            this.textBoxServerIp.Location = new System.Drawing.Point(9, 87);
            this.textBoxServerIp.Name = "textBoxServerIp";
            this.textBoxServerIp.ReadOnly = true;
            this.textBoxServerIp.Size = new System.Drawing.Size(170, 20);
            this.textBoxServerIp.TabIndex = 15;
            this.textBoxServerIp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonJoinServer
            // 
            this.buttonJoinServer.Location = new System.Drawing.Point(6, 19);
            this.buttonJoinServer.Name = "buttonJoinServer";
            this.buttonJoinServer.Size = new System.Drawing.Size(173, 23);
            this.buttonJoinServer.TabIndex = 4;
            this.buttonJoinServer.Text = "Join server";
            this.buttonJoinServer.UseVisualStyleBackColor = true;
            this.buttonJoinServer.Click += new System.EventHandler(this.buttonJoinServer_Click);
            // 
            // textBoxPlayerId
            // 
            this.textBoxPlayerId.Location = new System.Drawing.Point(9, 126);
            this.textBoxPlayerId.Name = "textBoxPlayerId";
            this.textBoxPlayerId.ReadOnly = true;
            this.textBoxPlayerId.Size = new System.Drawing.Size(170, 20);
            this.textBoxPlayerId.TabIndex = 14;
            this.textBoxPlayerId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label3.Location = new System.Drawing.Point(6, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Player ID";
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(9, 152);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(170, 21);
            this.buttonDisconnect.TabIndex = 12;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // textBoxCurrentServer
            // 
            this.textBoxCurrentServer.BackColor = System.Drawing.Color.Red;
            this.textBoxCurrentServer.ForeColor = System.Drawing.SystemColors.Menu;
            this.textBoxCurrentServer.Location = new System.Drawing.Point(9, 61);
            this.textBoxCurrentServer.Name = "textBoxCurrentServer";
            this.textBoxCurrentServer.ReadOnly = true;
            this.textBoxCurrentServer.Size = new System.Drawing.Size(170, 20);
            this.textBoxCurrentServer.TabIndex = 11;
            this.textBoxCurrentServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.label2.Location = new System.Drawing.Point(6, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Current server:";
            // 
            // groupBoxServer
            // 
            this.groupBoxServer.Controls.Add(this.buttonConnectedClients);
            this.groupBoxServer.Controls.Add(this.buttonCloseServer);
            this.groupBoxServer.Controls.Add(this.textBoxServerName);
            this.groupBoxServer.Controls.Add(this.buttonCreateServer);
            this.groupBoxServer.Controls.Add(this.labelServerName);
            this.groupBoxServer.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxServer.Location = new System.Drawing.Point(0, 55);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(200, 130);
            this.groupBoxServer.TabIndex = 6;
            this.groupBoxServer.TabStop = false;
            this.groupBoxServer.Text = "Server";
            // 
            // buttonConnectedClients
            // 
            this.buttonConnectedClients.Location = new System.Drawing.Point(6, 87);
            this.buttonConnectedClients.Name = "buttonConnectedClients";
            this.buttonConnectedClients.Size = new System.Drawing.Size(173, 23);
            this.buttonConnectedClients.TabIndex = 3;
            this.buttonConnectedClients.Text = "Connected clients";
            this.buttonConnectedClients.UseVisualStyleBackColor = true;
            this.buttonConnectedClients.Click += new System.EventHandler(this.buttonConnectedClients_Click);
            // 
            // buttonCloseServer
            // 
            this.buttonCloseServer.Location = new System.Drawing.Point(104, 58);
            this.buttonCloseServer.Name = "buttonCloseServer";
            this.buttonCloseServer.Size = new System.Drawing.Size(75, 23);
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
            this.textBoxServerName.Size = new System.Drawing.Size(173, 20);
            this.textBoxServerName.TabIndex = 1;
            this.textBoxServerName.Text = "MyServer";
            this.textBoxServerName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonCreateServer
            // 
            this.buttonCreateServer.Location = new System.Drawing.Point(6, 58);
            this.buttonCreateServer.Name = "buttonCreateServer";
            this.buttonCreateServer.Size = new System.Drawing.Size(92, 23);
            this.buttonCreateServer.TabIndex = 0;
            this.buttonCreateServer.Text = "Create Server";
            this.buttonCreateServer.UseVisualStyleBackColor = true;
            this.buttonCreateServer.Click += new System.EventHandler(this.buttonCreateServer_Click);
            // 
            // labelServerName
            // 
            this.labelServerName.AutoSize = true;
            this.labelServerName.Location = new System.Drawing.Point(3, 16);
            this.labelServerName.Name = "labelServerName";
            this.labelServerName.Size = new System.Drawing.Size(70, 13);
            this.labelServerName.TabIndex = 0;
            this.labelServerName.Text = "Server name:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBoxIp);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 55);
            this.panel1.TabIndex = 5;
            // 
            // tgcModifierTitleBar
            // 
            this.tgcModifierTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.tgcModifierTitleBar.Location = new System.Drawing.Point(0, 0);
            this.tgcModifierTitleBar.Name = "tgcModifierTitleBar";
            this.tgcModifierTitleBar.Size = new System.Drawing.Size(200, 21);
            this.tgcModifierTitleBar.TabIndex = 0;
            // 
            // TgcNetworkingModifierControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tgcModifierTitleBar);
            this.Controls.Add(this.contentPanel);
            this.Name = "TgcNetworkingModifierControl";
            this.Size = new System.Drawing.Size(200, 400);
            this.contentPanel.ResumeLayout(false);
            this.groupBoxClient.ResumeLayout(false);
            this.groupBoxClient.PerformLayout();
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIp;
        private Modifier.TGCModifierTitleBar tgcModifierTitleBar;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.GroupBox groupBoxClient;
        private System.Windows.Forms.TextBox textBoxServerIp;
        private System.Windows.Forms.Button buttonJoinServer;
        private System.Windows.Forms.TextBox textBoxPlayerId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBoxCurrentServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxServer;
        private System.Windows.Forms.Button buttonConnectedClients;
        private System.Windows.Forms.Button buttonCloseServer;
        private System.Windows.Forms.TextBox textBoxServerName;
        private System.Windows.Forms.Button buttonCreateServer;
        private System.Windows.Forms.Label labelServerName;
        private System.Windows.Forms.Panel panel1;
    }
}
