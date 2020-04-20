﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Networking;

namespace TGC.Examples.Example
{
    /// <summary>
    ///     Modifier para Networking.
    ///     Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
    ///     Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
    /// </summary>
    public partial class TGCExampleViewerNetworking : TGCExampleViewer
    {
        private readonly Queue<TgcSocketClientInfo> disconnectedClients;
        private readonly Queue<TgcSocketClientInfo> newConnectedClients;
        private TgcNetworkingModifierControl networkingModifier;
        private int port;
        private bool clientConnected;

        public TGCExampleViewerNetworking(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Server = new TgcSocketServer();
            Client = new TgcSocketClient();
            AvaliableServers = new List<TgcSocketClient.TgcAvaliableServer>();
            newConnectedClients = new Queue<TgcSocketClientInfo>();
            disconnectedClients = new Queue<TgcSocketClientInfo>();
        }

        /// <summary>
        ///     Crea el modificador de Networking
        /// </summary>
        /// <param name="varName">Identificador del modifier</param>
        /// <param name="serverName">Nombre default que va a usar el servidor</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente</param>
        /// <param name="port">Puerto en el cual se va a crear y buscar conexiones</param>
        public void Init(string varName, string serverName, string clientName, int port)
        {
            this.port = port;
            clientConnected = false;
            AddNetworking(varName, serverName, clientName, port);
        }

        /// <summary>
        /// Modifier para Networking.
        /// Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
        /// Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
        /// </summary>
        /// <param name="varName">Identificador del modifier.</param>
        /// <param name="serverName">Nombre default que va a usar el servidor.</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente.</param>
        /// <param name="port">Puerto en el cual se va a crear y buscar conexiones.</param>
        /// <returns>Modificador creado.</returns>
        public TgcNetworkingModifierControl AddNetworking(string varName, string serverName, string clientName, int port)
        {
            // TODO no deberia pasar this
            networkingModifier = new TgcNetworkingModifierControl(varName, serverName, clientName, port, this);
            AddModifier(networkingModifier);
            return networkingModifier;
        }

        /// <summary>
        /// Modifier para Networking.
        /// Permite crear servidores y conectarse a estos como cliente, mediante conexiones TCP/IP utilizando DirectPlay.
        /// Abstrae todo el manejo interno de DirectPlay para el manejo de conexiones.
        /// Utiliza el puerto default del framework.
        /// </summary>
        /// <param name="varName">Identificador del modifier.</param>
        /// <param name="serverName">Nombre default que va a usar el servidor.</param>
        /// <param name="clientName">Nombre default que va a usar cada cliente.</param>
        /// <returns>Modificador creado.</returns>
        public TgcNetworkingModifierControl AddNetworking(string varName, string serverName, string clientName)
        {
            // TODO no deberia pasar this
            networkingModifier = new TgcNetworkingModifierControl(varName, serverName, clientName, TgcSocketMessages.DEFAULT_PORT, this);
            AddModifier(networkingModifier);
            return networkingModifier;
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
                    networkingModifier.addClient(clientInfo);
                }

                //Clientes desconectados
                disconnectedClients.Clear();
                for (var i = 0; i < Server.DisconnectedClientsCount; i++)
                {
                    var clientInfo = Server.nextDisconnectedClient();
                    disconnectedClients.Enqueue(clientInfo);
                    networkingModifier.onClientDisconnected(clientInfo);
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
                networkingModifier.serverDisconnected();
            }
            else if (!clientConnected && Client.Status == TgcSocketClientInfo.ClientStatus.Connected)
            {
                clientConnected = true;
                networkingModifier.clientConnectedToServer(Client.ServerInfo, Client.PlayerId);
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
            Server.disconnectClient(playerId);
        }

        /// <summary>
        ///     Buscar servers el puerto especificado
        /// </summary>
        internal void searchServers()
        {
            AvaliableServers = Client.findLanServers(port);
            foreach (var server in AvaliableServers) networkingModifier.addServerToList(server);
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

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Limpia todas las conexiones que se hayan abierto
        /// </summary>
        public override void Dispose()
        {
            Server.disconnectServer();
        }
    }
}