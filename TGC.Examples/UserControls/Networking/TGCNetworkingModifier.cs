using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TGC.Examples.UserControls.Networking
{
    /// <summary>
    ///     Modifier para Networking.
    ///     Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
    ///     Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
    /// </summary>
    public partial class TGCNetworkingModifier : UserControl
    {
        private readonly Queue<TgcSocketClientInfo> disconnectedClients;
        private readonly TgcNetworkingModifierControl networkingControl;
        private readonly Queue<TgcSocketClientInfo> newConnectedClients;
        private readonly int port;
        private bool clientConnected;

        /// <summary>
        ///     Crea el modificador de Networking
        /// </summary>
        /// <param name="varName">Identificador del modifier</param>
        /// <param name="serverName">Nombre default que va a usar el servidor</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente</param>
        /// <param name="port">Puerto en el cual se va a crear y buscar conexiones</param>
        public TGCNetworkingModifier(string varName, string serverName, string clientName, int port)
        {
            InitializeComponent();

            Server = new TgcSocketServer();
            Client = new TgcSocketClient();
            this.port = port;
            clientConnected = false;
            networkingControl = new TgcNetworkingModifierControl(this, serverName, clientName);
            AvaliableServers = new List<TgcSocketClient.TgcAvaliableServer>();

            newConnectedClients = new Queue<TgcSocketClientInfo>();
            disconnectedClients = new Queue<TgcSocketClientInfo>();

            //FIXME este modifier no fue migrado a la nueva forma.
            //contentPanel.Controls.Add(networkingControl);
        }

        /// <summary>
        ///     Servidores disponibles
        /// </summary>
        internal List<TgcSocketClient.TgcAvaliableServer> AvaliableServers { get; private set; }

        /// <summary>
        ///     Servidor de la conexion.
        ///     Solo estara Online si creó una nueva sesion de servidor en la aplicacion
        /// </summary>
        public TgcSocketServer Server { get; }

        /// <summary>
        ///     Cliente de la conexion.
        ///     Solo estara Conectado si se especificó desde la aplicacion a que servidor conectarse
        /// </summary>
        public TgcSocketClient Client { get; }

        /// <summary>
        ///     Cantidad de clientes nuevos que se han conectado recientemente y que aún
        ///     están pendientes de lectura.
        /// </summary>
        public int NewClientsCount => newConnectedClients.Count;

        /// <summary>
        ///     Cantidad de clientes que se han desconectado recientemente y que aún
        ///     están pendientes de lectura.
        /// </summary>
        public int DisconnectedClientsCount => disconnectedClients.Count;

        public object getValue()
        {
            return null;
        }

        /// <summary>
        ///     Intenta crear un nuevo servidor
        /// </summary>
        internal bool createServer(string serverName)
        {
            return Server.initializeServer(serverName, port);
        }

        /// <summary>
        ///     Actualiza todo el estado de la red, tanto para clientes como servidores.
        ///     Debe llamarse en cada cuadro de Render, antes de comenzar a analizar
        ///     los mensajes de la red.
        /// </summary>
        public void updateNetwork()
        {
            updateServer();
            updateClient();
        }

        private void updateServer()
        {
            if (Server.Online)
            {
                Server.updateNetwork();

                //Nuevos clientes conectados, leer sin consumir
                newConnectedClients.Clear();
                for (var i = 0; i < Server.NewClientsCount; i++)
                {
                    var clientInfo = Server.nextNewClient();
                    newConnectedClients.Enqueue(clientInfo);
                    networkingControl.addClient(clientInfo);
                }

                //Clientes desconectados
                disconnectedClients.Clear();
                for (var i = 0; i < Server.DisconnectedClientsCount; i++)
                {
                    var clientInfo = Server.nextDisconnectedClient();
                    disconnectedClients.Enqueue(clientInfo);
                    networkingControl.onClientDisconnected(clientInfo);
                }
            }
        }

        /// <summary>
        ///     Devuelve el próximo cliente que se ha conectado recientemente y que aún
        ///     está pendiente de lectura.
        ///     Cada vez que se llama al método se consume el aviso y ese cliente ya no se
        ///     considera más pendietne de lectura.
        /// </summary>
        /// <returns>Información del cliente recientemente conectado</returns>
        public TgcSocketClientInfo nextNewClient()
        {
            if (newConnectedClients.Count == 0) throw new Exception("No hay nuevos clientes a evaluar");
            return newConnectedClients.Dequeue();
        }

        /// <summary>
        ///     Devuelve el próximo cliente que se ha desconectado recientemente y que aún
        ///     está pendiente de lectura.
        ///     Cada vez que se llama al método se consume el aviso y ese cliente ya no se
        ///     considera más pendietne de lectura.
        /// </summary>
        /// <returns>Información del cliente recientemente desconectado</returns>
        public TgcSocketClientInfo nextDisconnectedClient()
        {
            if (disconnectedClients.Count == 0) throw new Exception("No hay más clientes desconectados a evaluar");
            return disconnectedClients.Dequeue();
        }

        private void updateClient()
        {
            if (Client.Status != TgcSocketClientInfo.ClientStatus.Disconnected) Client.updateNetwork();

            if (clientConnected && Client.Status == TgcSocketClientInfo.ClientStatus.Disconnected)
            {
                clientConnected = false;
                networkingControl.serverDisconnected();
            }
            else if (!clientConnected && Client.Status == TgcSocketClientInfo.ClientStatus.Connected)
            {
                clientConnected = true;
                networkingControl.clientConnectedToServer(Client.ServerInfo, Client.PlayerId);
            }
        }

        /// <summary>
        ///     Cerrar el server
        /// </summary>
        internal void closeServer()
        {
            Server.disconnectServer();
        }

        /// <summary>
        ///     Eliminar un cliente por parte del server
        /// </summary>
        internal void deleteClient(int playerId)
        {
            //server.disconnectClient(playerId, null);
        }

        /// <summary>
        ///     Buscar servers el puerto especificado
        /// </summary>
        internal void searchServers()
        {
            AvaliableServers = Client.findLanServers(port);
            foreach (var server in AvaliableServers) networkingControl.addServerToList(server);
        }

        /// <summary>
        ///     Conectarse a un server en particular
        /// </summary>
        internal bool connectToServer(int serverIndex, string clientName)
        {
            var server = AvaliableServers[serverIndex];
            Client.initializeClient(clientName);
            return Client.connect(server.Ip, port);
        }

        /// <summary>
        ///     Desconectar el cliente del server
        /// </summary>
        internal void disconnectFromServer()
        {
            Client.disconnectClient();
        }

        /// <summary>
        ///     Limpia todas las conexiones que se hayan abierto
        /// </summary>
        public void dispose()
        {
            Server.disconnectServer();
        }
    }
}