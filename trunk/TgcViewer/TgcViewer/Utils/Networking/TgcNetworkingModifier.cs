using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.Modifiers;
using Microsoft.DirectX.DirectPlay;
using System.Windows.Forms;

namespace TgcViewer.Utils.Networking
{
    /// <summary>
    /// Modifier para Networking.
    /// Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
    /// Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
    /// </summary>
    public class TgcNetworkingModifier : TgcModifierPanel
    {
        TgcNetworkingModifierControl networkingControl;
        int port;
        Queue<TgcSocketClientInfo> newConnectedClients;
        Queue<TgcSocketClientInfo> disconnectedClients;
        bool clientConnected;

        List<TgcSocketClient.TgcAvaliableServer> avaliableServers;
        /// <summary>
        /// Servidores disponibles
        /// </summary>
        internal List<TgcSocketClient.TgcAvaliableServer> AvaliableServers
        {
            get { return avaliableServers; }
        }

        TgcSocketServer server;
        /// <summary>
        /// Servidor de la conexion. 
        /// Solo estara Online si creó una nueva sesion de servidor en la aplicacion
        /// </summary>
        public TgcSocketServer Server
        {
            get { return server; }
        }

        TgcSocketClient client;
        /// <summary>
        /// Cliente de la conexion.
        /// Solo estara Conectado si se especificó desde la aplicacion a que servidor conectarse 
        /// </summary>
        public TgcSocketClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// Crea el modificador de Networking
        /// </summary>
        /// <param name="varName">Identificador del modifier</param>
        /// <param name="serverName">Nombre default que va a usar el servidor</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente</param>
        /// <param name="port">Puerto en el cual se va a crear y buscar conexiones</param>
        public TgcNetworkingModifier(string varName, string serverName, string clientName, int port)
            : base(varName)
        {
            server = new TgcSocketServer();
            client = new TgcSocketClient();
            this.port = port;
            clientConnected = false;
            networkingControl = new TgcNetworkingModifierControl(this, serverName, clientName);
            avaliableServers = new List<TgcSocketClient.TgcAvaliableServer>();

            newConnectedClients = new Queue<TgcSocketClientInfo>();
            disconnectedClients = new Queue<TgcSocketClientInfo>(); ;

            contentPanel.Controls.Add(networkingControl);
        }


        public override object getValue()
        {
            return null;
        }

        /// <summary>
        /// Intenta crear un nuevo servidor
        /// </summary>
        internal bool createServer(string serverName)
        {
            return server.initializeServer(serverName, this.port);
        }

        /// <summary>
        /// Actualiza todo el estado de la red, tanto para clientes como servidores.
        /// Debe llamarse en cada cuadro de render, antes de comenzar a analizar
        /// los mensajes de la red.
        /// </summary>
        public void updateNetwork()
        {
            updateServer();
            updateClient();
        }

        private void updateServer()
        {
            if (server.Online)
            {
                server.updateNetwork();

                //Nuevos clientes conectados, leer sin consumir
                newConnectedClients.Clear();
                for (int i = 0; i < server.NewClientsCount; i++)
                {
                    TgcSocketClientInfo clientInfo = server.nextNewClient();
                    newConnectedClients.Enqueue(clientInfo);
                    networkingControl.addClient(clientInfo);
                }

                //Clientes desconectados
                disconnectedClients.Clear();
                for (int i = 0; i < server.DisconnectedClientsCount; i++)
                {
                    TgcSocketClientInfo clientInfo = server.nextDisconnectedClient();
                    disconnectedClients.Enqueue(clientInfo);
                    networkingControl.onClientDisconnected(clientInfo);
                }
            }

            
        }

        /// <summary>
        /// Cantidad de clientes nuevos que se han conectado recientemente y que aún
        /// están pendientes de lectura.
        /// </summary>
        public int NewClientsCount
        {
            get { return newConnectedClients.Count; }
        }

        /// <summary>
        /// Devuelve el próximo cliente que se ha conectado recientemente y que aún
        /// está pendiente de lectura.
        /// Cada vez que se llama al método se consume el aviso y ese cliente ya no se
        /// considera más pendietne de lectura.
        /// </summary>
        /// <returns>Información del cliente recientemente conectado</returns>
        public TgcSocketClientInfo nextNewClient()
        {
            if (newConnectedClients.Count == 0)
            {
                throw new Exception("No hay nuevos clientes a evaluar");
            }
            return newConnectedClients.Dequeue();
        }

        /// <summary>
        /// Cantidad de clientes que se han desconectado recientemente y que aún
        /// están pendientes de lectura.
        /// </summary>
        public int DisconnectedClientsCount
        {
            get { return disconnectedClients.Count; }
        }

        /// <summary>
        /// Devuelve el próximo cliente que se ha desconectado recientemente y que aún
        /// está pendiente de lectura.
        /// Cada vez que se llama al método se consume el aviso y ese cliente ya no se
        /// considera más pendietne de lectura.
        /// </summary>
        /// <returns>Información del cliente recientemente desconectado</returns>
        public TgcSocketClientInfo nextDisconnectedClient()
        {
            if (disconnectedClients.Count == 0)
            {
                throw new Exception("No hay más clientes desconectados a evaluar");
            }
            return disconnectedClients.Dequeue();
        }




        private void updateClient()
        {
            if (client.Status != TgcSocketClientInfo.ClientStatus.Disconnected)
            {
                client.updateNetwork();
            }
            
            if (clientConnected && client.Status == TgcSocketClientInfo.ClientStatus.Disconnected)
            {
                clientConnected = false;
                networkingControl.serverDisconnected();
            }
            else if (!clientConnected && client.Status == TgcSocketClientInfo.ClientStatus.Connected)
            {
                clientConnected = true;
                networkingControl.clientConnectedToServer(client.ServerInfo, client.PlayerId);
            }
        }



        /// <summary>
        /// Cerrar el server
        /// </summary>
        internal void closeServer()
        {
            server.disconnectServer();
        }

        /// <summary>
        /// Eliminar un cliente por parte del server
        /// </summary>
        internal void deleteClient(int playerId)
        {
            //server.disconnectClient(playerId, null);
        }

        /// <summary>
        /// Buscar servers el puerto especificado
        /// </summary>
        internal void searchServers()
        {
            avaliableServers = client.findLanServers(this.port);
            foreach (TgcSocketClient.TgcAvaliableServer server in avaliableServers)
            {
                networkingControl.addServerToList(server);
            }
        }

        /// <summary>
        /// Conectarse a un server en particular
        /// </summary>
        internal bool connectToServer(int serverIndex, string clientName)
        {
            TgcSocketClient.TgcAvaliableServer server = avaliableServers[serverIndex];
            client.initializeClient(clientName);
            return client.connect(server.Ip, this.port);
        }

        /// <summary>
        /// Desconectar el cliente del server
        /// </summary>
        internal void disconnectFromServer()
        {
            client.disconnectClient();
        }

        /// <summary>
        /// Limpia todas las conexiones que se hayan abierto
        /// </summary>
        public void dispose()
        {
            server.disconnectServer();
        }

    }
}
