using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TgcViewer.Utils.Networking
{
    public class TgcSocketClient
    {
        public const String CLIENT_HANDSHAKE = "TgcClient";

        private string clientName;
        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string ClientName
        {
            get { return clientName; }
        }

        private TgcSocketClientInfo.ClientStatus status;
        /// <summary>
        /// Estado del cliente
        /// </summary>
        public TgcSocketClientInfo.ClientStatus Status
        {
            get { return status; }
        }

        private int playerId;
        /// <summary>
        /// ID que identifica unívocamente al cliente
        /// </summary>
        public int PlayerId
        {
            get { return playerId; }
        }

        private TgcSocketServerInfo serverInfo;
        /// <summary>
        /// Información del server al cual se conectó
        /// </summary>
        public TgcSocketServerInfo ServerInfo
        {
            get { return serverInfo; }
        }

        /// <summary>
        /// Indica si el cliente está online
        /// </summary>
        public bool Online
        {
            get { return status == TgcSocketClientInfo.ClientStatus.Connected; }
        }

        private Socket clientSocket;
        private Queue<TgcSocketRecvMsg> receivedMessages;



        public TgcSocketClient()
        {
            this.receivedMessages = new Queue<TgcSocketRecvMsg>();
        }

        public void initializeClient(string clientName)
        {
            this.clientName = clientName;
            this.receivedMessages.Clear();
            this.status = TgcSocketClientInfo.ClientStatus.Disconnected;
        }

        /// <summary>
        /// Se conecta a un nuevo servidor. Pero todavía no lo toma como definitivo hasta
        /// que no se hace la selección final.
        /// </summary>
        /// <param name="ip">IP del server</param>
        /// <param name="port">Puerto del server</param>
        /// <returns>True si todo salio bien</returns>
        public bool connect(string ip, int port)
        {
            try
            {
                //Conectar con el server, en forma no bloqueante
                IPAddress address = IPAddress.Parse(ip);
                IPEndPoint Ipep = new IPEndPoint(address, port);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(Ipep);
                clientSocket.Blocking = false;

                serverInfo = new TgcSocketServerInfo(address, port);
                
                //Enviar mensaje de Handshake
                clientSocket.Send(Encoding.ASCII.GetBytes(CLIENT_HANDSHAKE));
                status = TgcSocketClientInfo.ClientStatus.HandshakePending;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Desconecta el cliente
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
                status = TgcSocketClientInfo.ClientStatus.Disconnected;
                clientSocket = null;
            }
        }


        /// <summary>
        /// Recibe todos los mensajes pendientes de la red y actualiza todos los estados
        /// </summary>
        public void updateNetwork()
        {
            if (clientSocket == null)
            {
                return;
            }

            if (!clientSocket.Connected)
            {
                disconnectClient();
                return;
            }

            //Ver si recibimos algún mensaje
            if(clientSocket.Poll(0, SelectMode.SelectRead))
            {
                bool result;

                switch (status)
                {
                    //Hanshake para aprobación del servidor
                    case TgcSocketClientInfo.ClientStatus.HandshakePending:
                        result = doHandShake();
                        if (!result)
                        {
                            disconnectClient();
                        }

                        break;

                    //Recibir info inicial del server
                    case TgcSocketClientInfo.ClientStatus.RequireInitialInfo:
                        result = getServerInitialInfo();
                        if (!result)
                        {
                            disconnectClient();
                        }
                        break;

                    case TgcSocketClientInfo.ClientStatus.Connected:
                        result = getReceiveMessage();
                        if (!result)
                        {
                            disconnectClient();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Enviar un mensaje al server
        /// </summary>
        /// <param name="msg">Mensaje a enviar</param>
        public void send(TgcSocketSendMsg msg)
        {
            TgcSocketMessages.sendMessage(clientSocket, msg, TgcSocketMessageHeader.MsgType.RegularMessage);
        }

        public int ReceivedMessagesCount
        {
            get { return receivedMessages.Count; }
        }

        public TgcSocketRecvMsg nextReceivedMessage()
        {
            if (receivedMessages.Count == 0)
            {
                throw new Exception("No hay nuevos mensajes a recibir");
            }
            return receivedMessages.Dequeue();
        }

        /// <summary>
        /// Recibir mensaje del servidor
        /// </summary>
        /// <returns>True si todo salio bien, False en caso de problemas en la conexión</returns>
        private bool getReceiveMessage()
        {
            try
            {
                //Recibir mensaje
                TgcSocketRecvMsg msg = TgcSocketMessages.receiveMessage(clientSocket, TgcSocketMessageHeader.MsgType.RegularMessage);
                if (msg == null)
                {
                    return false;
                }

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
        /// Recibir informacion inicial del server
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool getServerInitialInfo()
        {
            try
            {
                //Recibir info inicial del server
                TgcSocketRecvMsg msg = TgcSocketMessages.receiveMessage(clientSocket, TgcSocketMessageHeader.MsgType.InitialMessage);
                if (msg == null)
                {
                    return false;
                }

                //Guardar sus datos y cambiar su estado
                TgcSocketInitialInfoServer serverInitInfo = (TgcSocketInitialInfoServer)msg.readNext();
                serverInfo.Name = serverInitInfo.serverName;
                playerId = serverInitInfo.playerId;

                //Enviar OK final
                this.status = TgcSocketClientInfo.ClientStatus.Connected;
                TgcSocketSendMsg sendMsg = new TgcSocketSendMsg();
                sendMsg.write(true);
                return TgcSocketMessages.sendMessage(clientSocket, sendMsg, TgcSocketMessageHeader.MsgType.InitialMessage);
            }
            catch (SocketException)
            {
                return false;
            }
        }


        /// <summary>
        /// Empezar el handshake con el server
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool doHandShake()
        {
            try
            {
                int serverHandshakeLength = Encoding.ASCII.GetBytes(TgcSocketServer.SERVER_HANDSHAKE).Length;
                byte[] data = new byte[serverHandshakeLength];
                int recv = clientSocket.Receive(data, data.Length, SocketFlags.None);
                if (recv > 0 && recv == serverHandshakeLength)
                {
                    string msg = Encoding.ASCII.GetString(data, 0, recv);
                    if (msg.Equals(TgcSocketServer.SERVER_HANDSHAKE))
                    {
                        //Server correcto, enviar informacion inicial
                        status = TgcSocketClientInfo.ClientStatus.RequireInitialInfo;
                        TgcSocketInitialInfoClient clientInitInfo = new TgcSocketInitialInfoClient();
                        clientInitInfo.clientName = clientName;

                        TgcSocketSendMsg sendMsg = new TgcSocketSendMsg();
                        sendMsg.write(clientInitInfo);
                        TgcSocketMessages.sendMessage(clientSocket, sendMsg, TgcSocketMessageHeader.MsgType.InitialMessage);

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
        /// Buscar todos los servers disponibles de la LAN bajo el puerto especificado
        /// </summary>
        public List<TgcAvaliableServer> findLanServers(int port)
        {
            TgcLanBrowser lanBrowser = new TgcLanBrowser();
            List<string> computersDomains = lanBrowser.getNetworkComputers();
            List<TgcAvaliableServer> servers = new List<TgcAvaliableServer>();

            foreach (string computerDomain in computersDomains)
            {
                //Intentar conectarse
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(computerDomain);
                    IPAddress address = addresses[0];
                    servers.Add(new TgcAvaliableServer(computerDomain, address.ToString()));
                }
                catch (Exception)
                {
                    //Hubo algún problema un una IP, ignorar en la lista
                }
                
            }

            return servers;
        }

        /// <summary>
        /// Representa un servidor de la LAN disponible para conectarse
        /// </summary>
        public class TgcAvaliableServer
        {
            private string hostName;
            /// <summary>
            /// HostName
            /// </summary>
            public string HostName
            {
                get { return hostName; }
            }

            private string ip;
            /// <summary>
            /// IP
            /// </summary>
            public string Ip
            {
                get { return ip; }
            }

            public TgcAvaliableServer(string hostName, string ip)
            {
                this.hostName = hostName;
                this.ip = ip;
            }
        }

    }

    
}
