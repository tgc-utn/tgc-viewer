using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TGC.Examples.UserControls.Networking
{
    public class TgcSocketClient
    {
        public const string CLIENT_HANDSHAKE = "TgcClient";

        private readonly Queue<TgcSocketRecvMsg> receivedMessages;

        private Socket clientSocket;

        public TgcSocketClient()
        {
            receivedMessages = new Queue<TgcSocketRecvMsg>();
        }

        /// <summary>
        ///     Nombre del cliente
        /// </summary>
        public string ClientName { get; private set; }

        /// <summary>
        ///     Estado del cliente
        /// </summary>
        public TgcSocketClientInfo.ClientStatus Status { get; private set; }

        /// <summary>
        ///     ID que identifica unívocamente al cliente
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        ///     Información del server al cual se conectó
        /// </summary>
        public TgcSocketServerInfo ServerInfo { get; private set; }

        /// <summary>
        ///     Indica si el cliente está online
        /// </summary>
        public bool Online => Status == TgcSocketClientInfo.ClientStatus.Connected;

        public int ReceivedMessagesCount => receivedMessages.Count;

        public void initializeClient(string clientName)
        {
            ClientName = clientName;
            receivedMessages.Clear();
            Status = TgcSocketClientInfo.ClientStatus.Disconnected;
        }

        /// <summary>
        ///     Se conecta a un nuevo servidor. Pero todavía no lo toma como definitivo hasta
        ///     que no se hace la selección final.
        /// </summary>
        /// <param name="ip">IP del server</param>
        /// <param name="port">Puerto del server</param>
        /// <returns>True si todo salio bien</returns>
        public bool connect(string ip, int port)
        {
            try
            {
                //Conectar con el server, en forma no bloqueante
                var address = IPAddress.Parse(ip);
                var Ipep = new IPEndPoint(address, port);
                clientSocket = new Socket(Ipep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(Ipep);
                clientSocket.Blocking = false;

                ServerInfo = new TgcSocketServerInfo(address, port);

                //Enviar mensaje de Handshake
                clientSocket.Send(Encoding.ASCII.GetBytes(CLIENT_HANDSHAKE));
                Status = TgcSocketClientInfo.ClientStatus.HandshakePending;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Desconecta el cliente
        /// </summary>
        public void disconnectClient()
        {
            if (clientSocket != null)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch (Exception)
                {
                }

                Status = TgcSocketClientInfo.ClientStatus.Disconnected;
                clientSocket = null;
            }
        }

        /// <summary>
        ///     Recibe todos los mensajes pendientes de la red y actualiza todos los estados
        /// </summary>
        public void updateNetwork()
        {
            if (clientSocket == null) return;

            if (!clientSocket.Connected)
            {
                disconnectClient();
                return;
            }

            //Ver si recibimos algún mensaje
            if (clientSocket.Poll(0, SelectMode.SelectRead))
            {
                bool result;

                switch (Status)
                {
                    //Hanshake para aprobación del servidor
                    case TgcSocketClientInfo.ClientStatus.HandshakePending:
                        result = doHandShake();
                        if (!result) disconnectClient();

                        break;

                    //Recibir info inicial del server
                    case TgcSocketClientInfo.ClientStatus.RequireInitialInfo:
                        result = getServerInitialInfo();
                        if (!result) disconnectClient();
                        break;

                    case TgcSocketClientInfo.ClientStatus.Connected:
                        result = getReceiveMessage();
                        if (!result) disconnectClient();
                        break;
                }
            }
        }

        /// <summary>
        ///     Enviar un mensaje al server
        /// </summary>
        /// <param name="msg">Mensaje a enviar</param>
        public void send(TgcSocketSendMsg msg)
        {
            TgcSocketMessages.sendMessage(clientSocket, msg, TgcSocketMessageHeader.MsgType.RegularMessage);
        }

        public TgcSocketRecvMsg nextReceivedMessage()
        {
            if (receivedMessages.Count == 0) throw new Exception("No hay nuevos mensajes a recibir");
            return receivedMessages.Dequeue();
        }

        /// <summary>
        ///     Recibir mensaje del servidor
        /// </summary>
        /// <returns>True si todo salio bien, False en caso de problemas en la conexión</returns>
        private bool getReceiveMessage()
        {
            try
            {
                //Recibir mensaje
                var msg = TgcSocketMessages.receiveMessage(clientSocket, TgcSocketMessageHeader.MsgType.RegularMessage);
                if (msg == null) return false;

                //Agregar a la lista de mensajes pendientes
                receivedMessages.Enqueue(msg);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Recibir informacion inicial del server
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool getServerInitialInfo()
        {
            try
            {
                //Recibir info inicial del server
                var msg = TgcSocketMessages.receiveMessage(clientSocket, TgcSocketMessageHeader.MsgType.InitialMessage);
                if (msg == null) return false;

                //Guardar sus datos y cambiar su estado
                var serverInitInfo = (TgcSocketInitialInfoServer)msg.readNext();
                ServerInfo.Name = serverInitInfo.serverName;
                PlayerId = serverInitInfo.playerId;

                //Enviar OK final
                Status = TgcSocketClientInfo.ClientStatus.Connected;
                var sendMsg = new TgcSocketSendMsg();
                sendMsg.write(true);
                return TgcSocketMessages.sendMessage(clientSocket, sendMsg,
                    TgcSocketMessageHeader.MsgType.InitialMessage);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Empezar el handshake con el server
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool doHandShake()
        {
            try
            {
                var serverHandshakeLength = Encoding.ASCII.GetBytes(TgcSocketServer.SERVER_HANDSHAKE).Length;
                var data = new byte[serverHandshakeLength];
                var recv = clientSocket.Receive(data, data.Length, SocketFlags.None);
                if (recv > 0 && recv == serverHandshakeLength)
                {
                    var msg = Encoding.ASCII.GetString(data, 0, recv);
                    if (msg.Equals(TgcSocketServer.SERVER_HANDSHAKE))
                    {
                        //Server correcto, enviar informacion inicial
                        Status = TgcSocketClientInfo.ClientStatus.RequireInitialInfo;
                        var clientInitInfo = new TgcSocketInitialInfoClient();
                        clientInitInfo.clientName = ClientName;

                        var sendMsg = new TgcSocketSendMsg();
                        sendMsg.write(clientInitInfo);
                        TgcSocketMessages.sendMessage(clientSocket, sendMsg,
                            TgcSocketMessageHeader.MsgType.InitialMessage);

                        return true;
                    }
                }
            }
            catch (SocketException)
            {
                //Handshake incorrecto
                return false;
            }

            return false;
        }

        /// <summary>
        ///     Buscar todos los servers disponibles de la LAN bajo el puerto especificado
        /// </summary>
        public List<TgcAvaliableServer> findLanServers(int port)
        {
            var lanBrowser = new TgcLanBrowser();
            var computersDomains = lanBrowser.getNetworkComputers();
            var servers = new List<TgcAvaliableServer>();

            foreach (var computerDomain in computersDomains)
                //Intentar conectarse
                try
                {
                    var addresses = Dns.GetHostAddresses(computerDomain);
                    var address = addresses[0];
                    servers.Add(new TgcAvaliableServer(computerDomain, address.ToString()));
                }
                catch (Exception)
                {
                    //Hubo algún problema un una IP, ignorar en la lista
                }

            return servers;
        }

        /// <summary>
        ///     Representa un servidor de la LAN disponible para conectarse
        /// </summary>
        public class TgcAvaliableServer
        {
            public TgcAvaliableServer(string hostName, string ip)
            {
                HostName = hostName;
                Ip = ip;
            }

            /// <summary>
            ///     HostName
            /// </summary>
            public string HostName { get; }

            /// <summary>
            ///     IP
            /// </summary>
            public string Ip { get; }
        }
    }
}