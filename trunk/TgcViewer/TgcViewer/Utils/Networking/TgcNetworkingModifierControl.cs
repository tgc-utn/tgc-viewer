using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SistPaquetesClient.core;

namespace TgcViewer.Utils.Networking
{
    /// <summary>
    /// Control grafico de Modifier para Networking
    /// </summary>
    public partial class TgcNetworkingModifierControl : UserControl
    {
        TgcNetworkingModifier modifier;
        internal int selectedPlayerId;
        internal int selectedServer;
        TgcNetworkingModifierClientsDialog clientsDialog;
        TgcNetworkingModifierServersDialog serversDialog;

        public TgcNetworkingModifierControl(TgcNetworkingModifier modifier, string serverName, string clientName)
        {
            InitializeComponent();

            this.modifier = modifier;
            this.textBoxServerName.Text = serverName;
            this.selectedPlayerId = -1;

            this.buttonCloseServer.Enabled = false;
            this.buttonConnectedClients.Enabled = false;

            this.buttonDisconnect.Enabled = false;

            //Cargar IP local
            this.textBoxIp.Text = TgcSocketServer.getHostAddress().ToString();
            
            clientsDialog = new TgcNetworkingModifierClientsDialog(this);
            serversDialog = new TgcNetworkingModifierServersDialog(this, clientName);
        }

        /// <summary>
        /// Crear nuevo server
        /// </summary>
        private void buttonCreateServer_Click(object sender, EventArgs e)
        {
            string serverName = getServerName();
            bool result = modifier.createServer(serverName);
            if (result)
            {
                this.buttonCloseServer.Enabled = true;
                this.buttonConnectedClients.Enabled = true;
                this.buttonCreateServer.Enabled = false;
                this.textBoxServerName.Enabled = false;
                this.textBoxServerName.BackColor = Color.Green;
                this.clientsDialog.onServerCreated();
            }
            else
            {
                MessageBox.Show(this, 
                    "No se ha podido crear el servidor.\n" +
                    "Compruebe que el puerto no est� siendo utilizado.\n" +
                    "Si se ha cerrado una conexi�n recientemente, es necesario esperar unos segundos hasta que se libere el puerto.",
                    "Error de conexi�n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Obtener nombre de server
        /// </summary>
        private string getServerName()
        {
            string name = this.textBoxServerName.Text;
            if (!ValidationUtils.validateRequired(name))
            {
                name = "MyServer";
            }
            return name;
        }

        /// <summary>
        /// Agregar un cliente a la lista de conectados
        /// </summary>
        internal void addClient(TgcSocketClientInfo clientInfo)
        {
            clientsDialog.addClient(clientInfo);
        }

        /// <summary>
        /// Mostrar ventana de clientes conectados
        /// </summary>
        private void buttonConnectedClients_Click(object sender, EventArgs e)
        {
            clientsDialog.ShowDialog(GuiController.Instance.MainForm);
        }
   

        /// <summary>
        /// Eliminar un cliente conectado de la lista que se acaba de desconectar
        /// </summary>
        internal void onClientDisconnected(TgcSocketClientInfo clientInfo)
        {
            //Si la ventana de clientes estaba abierta, cerrarla primero, para que no haya lio de Threas de refresco
            clientsDialog.Hide();
            clientsDialog.onClientDisconnected(clientInfo);
        }

        /// <summary>
        /// Cerrar el servidor
        /// </summary>
        private void buttonCloseServer_Click(object sender, EventArgs e)
        {
            modifier.closeServer();

            buttonConnectedClients.Enabled = false;
            buttonCloseServer.Enabled = false;
            buttonCreateServer.Enabled = true;
            textBoxServerName.Enabled = true;
            textBoxServerName.BackColor = Color.Red;
        }

        
        /// <summary>
        /// Eliminar por decision del server un cliente conectado 
        /// </summary>
        internal void deleteClient(int playerId)
        {
            modifier.deleteClient(playerId);
        }

        /// <summary>
        /// Abrir la ventana para buscar servidores
        /// </summary>
        private void buttonJoinServer_Click(object sender, EventArgs e)
        {
            serversDialog.prepareDialog();
            serversDialog.ShowDialog(GuiController.Instance.MainForm);
        }

        /// <summary>
        /// Buscar servidores
        /// </summary>
        internal void searchServers()
        {
            modifier.searchServers();
        }

        

        /// <summary>
        /// Agregar server encontrado a la lista de servers disponibles
        /// </summary>
        internal void addServerToList(TgcSocketClient.TgcAvaliableServer server)
        {
            serversDialog.addServerToList(server);
        }

        /// <summary>
        /// Conectarse al server elegido
        /// </summary>
        internal bool connectToServer(int selectedServer, string clientName)
        {
            return modifier.connectToServer(selectedServer, clientName);
        }

        /// <summary>
        /// Cuando el cliente se conecto finalmente con el server
        /// </summary>
        internal void clientConnectedToServer(TgcSocketServerInfo serverInfo, int playerId)
        {
            buttonJoinServer.Enabled = false;
            buttonDisconnect.Enabled = true;
            textBoxCurrentServer.Text = serverInfo.Name;
            textBoxCurrentServer.BackColor = Color.Green;
            textBoxPlayerId.Text = playerId.ToString();
            textBoxServerIp.Text = modifier.Client.ServerInfo.Address.ToString();
        }

        /// <summary>
        /// Desconectarse del server
        /// </summary>
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            modifier.disconnectFromServer();
            buttonDisconnect.Enabled = false;
            textBoxCurrentServer.Text = "";
            textBoxCurrentServer.BackColor = Color.Red;
            textBoxPlayerId.Text = "";
            buttonJoinServer.Enabled = true;
            textBoxServerIp.Text = "";
        }

        internal void serverDisconnected()
        {
            buttonDisconnect.Enabled = false;
            textBoxCurrentServer.Text = "";
            textBoxCurrentServer.BackColor = Color.Red;
            textBoxPlayerId.Text = "";
            buttonJoinServer.Enabled = true;
            textBoxServerIp.Text = "";
        }

        /// <summary>
        /// Servidores disponibles
        /// </summary>
        internal List<TgcSocketClient.TgcAvaliableServer> AvaliableServers
        {
            get { return modifier.AvaliableServers; }
        }

        

        
    }
}
