using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.DirectPlay;
using SistPaquetesClient.core;
using System.Net;

namespace TgcViewer.Utils.Networking
{
    /// <summary>
    /// Ventana para buscar servidores
    /// </summary>
    public partial class TgcNetworkingModifierServersDialog : Form
    {
        TgcNetworkingModifierControl networkingControl;

        public TgcNetworkingModifierServersDialog(TgcNetworkingModifierControl networkingControl, string clientName)
        {
            InitializeComponent();

            this.networkingControl = networkingControl;
            this.textBoxClientName.Text = clientName;
        }

        /// <summary>
        /// Limpiar las cosas al principio antes de mostrar
        /// </summary>
        internal void prepareDialog()
        {
            this.dataGridViewAvaliableServers.Enabled = false;
            this.dataGridViewAvaliableServers.Rows.Clear();
            this.buttonJoin.Enabled = false;
            this.buttonRefresh.Enabled = true;

            //auto-refresh
            //buttonRefreshServers_Click(null, null);
        }

        /// <summary>
        /// Buscar servidores
        /// </summary>
        private void buttonRefreshServers_Click(object sender, EventArgs e)
        {
            dataGridViewAvaliableServers.Rows.Clear();
            networkingControl.searchServers();
        }

        /// <summary>
        /// Se encontro un server
        /// </summary>
        internal void addServerToList(TgcSocketClient.TgcAvaliableServer server)
        {
            dataGridViewAvaliableServers.Rows.Add(dataGridViewAvaliableServers.Rows.Count, 
                server.HostName,
                server.Ip);

            //seleccionar el primer elemento de la tabla
            dataGridViewAvaliableServers.Enabled = true;
            dataGridViewAvaliableServers.Rows[0].Selected = true;
            dataGridViewAvaliableServers_RowEnter(null, null);
        }

        /// <summary>
        /// Eligieron un server de la lista
        /// </summary>
        private void dataGridViewAvaliableServers_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewAvaliableServers.SelectedRows.Count > 0)
            {
                networkingControl.selectedServer = dataGridViewAvaliableServers.SelectedRows[0].Index;
                buttonJoin.Enabled = true;
            }
            else
            {
                buttonJoin.Enabled = false;
            }
        }

        
        /// <summary>
        /// Conectarse a un server
        /// </summary>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            bool result = networkingControl.connectToServer(networkingControl.selectedServer, getClientName());
            if (!result)
            {
                MessageBox.Show(this, "No se ha podido establecer conexión con el Servidor", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Hide();
        }

        /// <summary>
        /// Obtener nombre de cliente
        /// </summary>
        private string getClientName()
        {
            string name = this.textBoxClientName.Text;
            if (!ValidationUtils.validateRequired(name))
            {
                name = "MyClient";
            }
            return name;
        }

        /// <summary>
        /// Agregar un server a mano
        /// </summary>
        private void buttonAddServer_Click(object sender, EventArgs e)
        {
            string textIp = textBoxAddServer.Text;
            IPAddress ip;
            bool result = IPAddress.TryParse(textIp, out ip);
            if (result)
            {
                TgcSocketClient.TgcAvaliableServer server = new TgcSocketClient.TgcAvaliableServer("Manual", ip.ToString());
                networkingControl.AvaliableServers.Add(server);
                dataGridViewAvaliableServers.Rows.Add(new object[] { dataGridViewAvaliableServers.Rows.Count, server.HostName, server.Ip });
                dataGridViewAvaliableServers.Enabled = true;
                dataGridViewAvaliableServers.Rows[0].Selected = true;
                dataGridViewAvaliableServers_RowEnter(null, null);
            }
            else
            {
                MessageBox.Show(this, "La IP ingresada es incorrecta", "Add Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxAddServer.Text = "";
            }
        }

        
    }
}