using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Utils;

namespace TGC.Util.Networking
{
    /// <summary>
    ///     Control grafico de Modifier para Networking
    /// </summary>
    public partial class TgcNetworkingModifierControl : UserControl
    {
        private readonly TgcNetworkingModifierClientsDialog clientsDialog;
        private readonly TgcNetworkingModifier modifier;
        private readonly TgcNetworkingModifierServersDialog serversDialog;
        private int selectedPlayerId;
        private int selectedServer;

        public TgcNetworkingModifierControl(TgcNetworkingModifier modifier, string serverName, string clientName)
        {
            InitializeComponent();

            this.modifier = modifier;
            textBoxServerName.Text = serverName;
            selectedPlayerId = -1;

            buttonCloseServer.Enabled = false;
            buttonConnectedClients.Enabled = false;

            buttonDisconnect.Enabled = false;

            //Cargar IP local
            textBoxIp.Text = TgcSocketServer.getHostAddress().ToString();

            clientsDialog = new TgcNetworkingModifierClientsDialog(this);
            serversDialog = new TgcNetworkingModifierServersDialog(this, clientName);
        }

        /// <summary>
        ///     Servidores disponibles
        /// </summary>
        public List<TgcSocketClient.TgcAvaliableServer> AvaliableServers
        {
            get { return modifier.AvaliableServers; }
        }

        public int SelectedPlayerId
        {
            get { return selectedPlayerId; }
            set { selectedPlayerId = value; }
        }

        public int SelectedServer
        {
            get { return selectedServer; }
            set { selectedServer = value; }
        }

        /// <summary>
        ///     Crear nuevo server
        /// </summary>
        private void buttonCreateServer_Click(object sender, EventArgs e)
        {
            var serverName = getServerName();
            var result = modifier.createServer(serverName);
            if (result)
            {
                buttonCloseServer.Enabled = true;
                buttonConnectedClients.Enabled = true;
                buttonCreateServer.Enabled = false;
                textBoxServerName.Enabled = false;
                textBoxServerName.BackColor = Color.Green;
                clientsDialog.onServerCreated();
            }
            else
            {
                MessageBox.Show(this,
                    "No se ha podido crear el servidor.\n" +
                    "Compruebe que el puerto no esté siendo utilizado.\n" +
                    "Si se ha cerrado una conexión recientemente, es necesario esperar unos segundos hasta que se libere el puerto.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Obtener nombre de server
        /// </summary>
        private string getServerName()
        {
            var name = textBoxServerName.Text;
            if (!ValidationUtils.validateRequired(name))
            {
                name = "MyServer";
            }
            return name;
        }

        /// <summary>
        ///     Agregar un cliente a la lista de conectados
        /// </summary>
        public void addClient(TgcSocketClientInfo clientInfo)
        {
            clientsDialog.addClient(clientInfo);
        }

        /// <summary>
        ///     Mostrar ventana de clientes conectados
        /// </summary>
        private void buttonConnectedClients_Click(object sender, EventArgs e)
        {
            clientsDialog.ShowDialog(GuiController.Instance.MainForm);
        }

        /// <summary>
        ///     Eliminar un cliente conectado de la lista que se acaba de desconectar
        /// </summary>
        public void onClientDisconnected(TgcSocketClientInfo clientInfo)
        {
            //Si la ventana de clientes estaba abierta, cerrarla primero, para que no haya lio de Threas de refresco
            clientsDialog.Hide();
            clientsDialog.onClientDisconnected(clientInfo);
        }

        /// <summary>
        ///     Cerrar el servidor
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
        ///     Eliminar por decision del server un cliente conectado
        /// </summary>
        public void deleteClient(int playerId)
        {
            modifier.deleteClient(playerId);
        }

        /// <summary>
        ///     Abrir la ventana para buscar servidores
        /// </summary>
        private void buttonJoinServer_Click(object sender, EventArgs e)
        {
            serversDialog.prepareDialog();
            serversDialog.ShowDialog(GuiController.Instance.MainForm);
        }

        /// <summary>
        ///     Buscar servidores
        /// </summary>
        public void searchServers()
        {
            modifier.searchServers();
        }

        /// <summary>
        ///     Agregar server encontrado a la lista de servers disponibles
        /// </summary>
        public void addServerToList(TgcSocketClient.TgcAvaliableServer server)
        {
            serversDialog.addServerToList(server);
        }

        /// <summary>
        ///     Conectarse al server elegido
        /// </summary>
        public bool connectToServer(int selectedServer, string clientName)
        {
            return modifier.connectToServer(selectedServer, clientName);
        }

        /// <summary>
        ///     Cuando el cliente se conecto finalmente con el server
        /// </summary>
        public void clientConnectedToServer(TgcSocketServerInfo serverInfo, int playerId)
        {
            buttonJoinServer.Enabled = false;
            buttonDisconnect.Enabled = true;
            textBoxCurrentServer.Text = serverInfo.Name;
            textBoxCurrentServer.BackColor = Color.Green;
            textBoxPlayerId.Text = playerId.ToString();
            textBoxServerIp.Text = modifier.Client.ServerInfo.Address.ToString();
        }

        /// <summary>
        ///     Desconectarse del server
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

        public void serverDisconnected()
        {
            buttonDisconnect.Enabled = false;
            textBoxCurrentServer.Text = "";
            textBoxCurrentServer.BackColor = Color.Red;
            textBoxPlayerId.Text = "";
            buttonJoinServer.Enabled = true;
            textBoxServerIp.Text = "";
        }
    }
}