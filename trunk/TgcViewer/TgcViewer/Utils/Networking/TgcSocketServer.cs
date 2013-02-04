using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace TgcViewer.Utils.Networking
{
    public class TgcSocketServer
    {
        const int MAX_PENDING_CONNECTIONS = 32;
        public const String SERVER_HANDSHAKE = "TgcServer";

        Socket serverSocket;
        int playerIdCounter;
        Queue<TgcSocketClientRecvMesg> receivedMessages;
        Queue<TgcSocketClientInfo> newConnectedClients;
        Queue<TgcSocketClientInfo> disconnectedClients;
        List<TgcSocketClientInfo> allClients;

        List<TgcSocketClientInfo> connectedClients;
        /// <summary>
        /// Clientes conectados al servidor.
        /// </summary>
        public List<TgcSocketClientInfo> Clients
        {
            get { return connectedClients; }
        }

        string serverName;
        /// <summary>
        /// Nombre del servidor
        /// </summary>
        public string ServerName
        {
            get { return serverName; }
        }

        private bool online;
        /// <summary>
        /// Indica si el servidor está online
        /// </summary>
        public bool Online
        {
            get { return online; }
        }

        private IPAddress serverAddress;
        /// <summary>
        /// IP del servidor
        /// </summary>
        public IPAddress ServerAddress
        {
            get { return serverAddress; }
        }

        /// <summary>
        /// Devuelve la IP de la PC local
        /// </summary>
        /// <returns>IP local</returns>
        public static IPAddress getHostAddress()
        {
            string hostname = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostname);
            return ipAddresses[0];
        }

        public TgcSocketServer()
        {
        }

        /// <summary>
        /// Inicia una nueva conexion TCP/IP con el nombre de sesion especificado
        /// en la IP local y el puerto default
        /// </summary>
        public void initializeServer(string serverName)
        {
            initializeServer(serverName, TgcSocketMessages.DEFAULT_PORT);
        }

        /// <summary>
        /// Inicia una nueva conexion TCP/IP con el nombre de sesion especificado
        /// en la IP local y el puerto especificado
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        public bool initializeServer(string serverName, int port)
        {
            try
            {
                this.serverName = serverName;
                this.allClients = new List<TgcSocketClientInfo>();
                this.connectedClients = new List<TgcSocketClientInfo>();
                this.playerIdCounter = 1;
                this.receivedMessages = new Queue<TgcSocketClientRecvMesg>();
                this.newConnectedClients = new Queue<TgcSocketClientInfo>();
                this.disconnectedClients = new Queue<TgcSocketClientInfo>();

                //Crear un socket en la IP local con el puerto especificado
                serverAddress = TgcSocketServer.getHostAddress();
                IPEndPoint Ipep = new IPEndPoint(serverAddress, port);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //Hacerlo no bloqueante
                serverSocket.LingerState = new LingerOption(false, 0); 
                serverSocket.Blocking = false;
                serverSocket.Bind(Ipep);
                serverSocket.Listen(MAX_PENDING_CONNECTIONS);

                this.online = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Desconecta el servidor
        /// </summary>
        public void disconnectServer()
        {
            foreach (TgcSocketClientInfo clientInfo in allClients)
            {
                try
                {
                    clientInfo.Socket.Shutdown(SocketShutdown.Both);
                    clientInfo.Socket.Close();
                }
                catch (Exception)
                {
                }
            }

            this.receivedMessages.Clear();
            this.newConnectedClients.Clear();
            this.disconnectedClients.Clear();
            this.allClients.Clear();
            this.connectedClients.Clear();

            if (serverSocket != null)
            {
                try
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                    serverSocket.Close();
                }
                catch (Exception)
                {
                }
                this.online = false;
                serverSocket = null;
            }
            
        }

        /// <summary>
        /// Recibe todos los mensajes pendientes de la red y actualiza todos los estados
        /// </summary>
        public void updateNetwork()
        {
            //Escuchar nuevas conexiones
            acceptNewClients();


            //Recibir mensajes de clientes
            for (int i = 0; i < allClients.Count; i++)
            {
                TgcSocketClientInfo clientInfo = allClients[i];
                Socket socket = clientInfo.Socket;

                //Ver si envió algún mensaje
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    bool result;

                    switch (clientInfo.Status)
                    {
                        //Handshake para clientes pendientes de aprobación
                        case TgcSocketClientInfo.ClientStatus.HandshakePending:
                            result = doHandshake(clientInfo, socket);
                            if (!result)
                            {
                                //Handshake incorrecto, eliminar cliente
                                allClients.RemoveAt(i);
                                i--;
                            }
                            break;

                        //Recibir información inicial del cliente
                        case TgcSocketClientInfo.ClientStatus.RequireInitialInfo:
                            result = getClientInitialInfo(clientInfo, socket);
                            if (!result)
                            {
                                //Error al recibir información inicial, eliminar cliente
                                allClients.RemoveAt(i);
                                i--;
                            }
                            break;

                        //Recibir aprobación final del cliente
                        case TgcSocketClientInfo.ClientStatus.WaitingClientOk:
                            result = getClientOkResponse(clientInfo, socket);
                            if (!result)
                            {
                                //El cliente no aceptó la conexión, eliminar
                                allClients.RemoveAt(i);
                                i--;
                            }
                            break;

                        //Recibir mensajes de clientes ya aprobados
                        case TgcSocketClientInfo.ClientStatus.Connected:
                            result = getReceiveMessage(clientInfo, socket);
                            if (!result)
                            {
                                //Si hubo algún problema entonces el cliente se desconectó, eliminar
                                allClients.RemoveAt(i);
                                connectedClients.Remove(clientInfo);
                                i--;
                            }
                            break;  
                    }
                }
            }
        }

        /// <summary>
        /// Busca la información de un cliente en base a su playerId
        /// </summary>
        /// <returns>Null si no lo encuentra</returns>
        public TgcSocketClientInfo getClientInfo(int playerId)
        {
            foreach (TgcSocketClientInfo clientInfo in allClients)
            {
                if (clientInfo.PlayerId == playerId)
                {
                    return clientInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Envia un mensaje a un cliente particular
        /// </summary>
        /// <param name="playerId">ID del cliente</param>
        /// <param name="msg">Mensaje a enviar</param>
        public void sendToClient(int playerId, TgcSocketSendMsg msg)
        {
            TgcSocketClientInfo clientInfo = getClientInfo(playerId);
            if (clientInfo != null)
            {
                sendToClient(clientInfo, msg);
            }
        }

        /// <summary>
        /// Envia un mensaje a todos los clientes conectados al servidor
        /// </summary>
        /// <param name="msg">Mensaje a enviar</param>
        public void sendToAll(TgcSocketSendMsg msg)
        {
            foreach (TgcSocketClientInfo clientInfo in allClients)
            {
                sendToClient(clientInfo, msg);
            }
        }

        /// <summary>
        /// Envia un mensaje a todos los clientes conectados al servidor, a excepción
        /// de uno.
        /// </summary>
        /// <param name="playerIdException">ID de cliente al que no se le va a mandar el mensaje</param>
        /// <param name="msg">Mensaje a enviar</param>
        public void sendToAllExceptOne(int playerIdException, TgcSocketSendMsg msg)
        {
            foreach (TgcSocketClientInfo clientInfo in allClients)
            {
                if (clientInfo.PlayerId != playerIdException)
                {
                    sendToClient(clientInfo, msg);
                }
            }
        }

        /// <summary>
        /// Enviar mensaje a cliente, solo si esta conectado
        /// </summary>
        private void sendToClient(TgcSocketClientInfo clientInfo, TgcSocketSendMsg msg)
        {
            if (clientInfo.Status == TgcSocketClientInfo.ClientStatus.Connected)
            {
                TgcSocketMessages.sendMessage(clientInfo.Socket, msg, TgcSocketMessageHeader.MsgType.RegularMessage);
            }
        }

        /// <summary>
        /// Cantidad de clientes nuevos que se han conectado recientemente y que aún
        /// están pendientes de lectura.
        /// </summary>
        internal int NewClientsCount
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
        internal TgcSocketClientInfo nextNewClient()
        {
            if (newConnectedClients.Count == 0)
            {
                throw new Exception("No hay nuevos clientes a evaluar");
            }
            return newConnectedClients.Dequeue();
        }

        /// <summary>
        /// Cantidad de nuevos mensajes recibidos pendientes de lectura
        /// </summary>
        public int ReceivedMessagesCount
        {
            get { return receivedMessages.Count; }
        }

        /// <summary>
        /// Devuelve el próximo mensaje recibido y que aún está pendiente de lectura.
        /// Cada vez que se llama al método se consume el aviso y ese mensaje ya no se
        /// considera más pendietne de lectura.
        /// </summary>
        /// <returns>Mensaje recibido</returns>
        public TgcSocketClientRecvMesg nextReceivedMessage()
        {
            if (receivedMessages.Count == 0)
            {
                throw new Exception("No hay nuevos mensajes a recibir");
            }
            return receivedMessages.Dequeue();
        }

        /// <summary>
        /// Cantidad de clientes que se han desconectado recientemente y que aún
        /// están pendientes de lectura.
        /// </summary>
        internal int DisconnectedClientsCount
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
        internal TgcSocketClientInfo nextDisconnectedClient()
        {
            if (disconnectedClients.Count == 0)
            {
                throw new Exception("No hay más clientes desconectados a evaluar");
            }
            return disconnectedClients.Dequeue();
        }



        /// <summary>
        /// Lee un mensaje ordinario recibido por el cliente
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool getReceiveMessage(TgcSocketClientInfo clientInfo, Socket socket)
        {
            try
            {
                //Recibir mensaje
                TgcSocketRecvMsg msg = TgcSocketMessages.receiveMessage(socket, TgcSocketMessageHeader.MsgType.RegularMessage);
                if (msg == null)
                {
                    return false;
                }

                //Agregar a la lista de mensajes pendientes
                receivedMessages.Enqueue(new TgcSocketClientRecvMesg(msg, clientInfo.PlayerId));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Recibir la confirmación final del cliente para empezar a recibir mensajes de la aplicación
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool getClientOkResponse(TgcSocketClientInfo clientInfo, Socket socket)
        {
            try
            {
                //Recibir respuesta final del cliente
                TgcSocketRecvMsg msg = TgcSocketMessages.receiveMessage(socket, TgcSocketMessageHeader.MsgType.InitialMessage);
                if (msg == null)
                {
                    return false;
                }

                //Respuesta del cliente
                //bool userOk = bool.Parse(msg.readString());
                bool userOk = (bool)msg.readNext();
                if (userOk)
                {
                    //Habilitar estado de cliente para escuchar mensajes
                    clientInfo.Status = TgcSocketClientInfo.ClientStatus.Connected;
                    connectedClients.Add(clientInfo);

                    //Guardar en la lista de nuevos clientes conectados
                    newConnectedClients.Enqueue(clientInfo);
                }

                return userOk;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        /// <summary>
        /// Recibir información inicial del cliente
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool getClientInitialInfo(TgcSocketClientInfo clientInfo, Socket socket)
        {
            try
            {
                //Recibir info inicial del cliente
                TgcSocketRecvMsg msg = TgcSocketMessages.receiveMessage(socket, TgcSocketMessageHeader.MsgType.InitialMessage);
                if (msg == null)
                {
                    return false;
                }

                //Guardar sus datos y cambiar su estado
                TgcSocketInitialInfoClient clientInitInfo = (TgcSocketInitialInfoClient)msg.readNext();
                clientInfo.Name = clientInitInfo.clientName;
                clientInfo.Status = TgcSocketClientInfo.ClientStatus.WaitingClientOk;

                //Asignar Player ID a cliente
                TgcSocketInitialInfoServer serverInitInfo = new TgcSocketInitialInfoServer();
                serverInitInfo.serverName = serverName;
                serverInitInfo.playerId = playerIdCounter;
                clientInfo.PlayerId = serverInitInfo.playerId;
                playerIdCounter++;

                //Enviar info inicial del server
                TgcSocketSendMsg sendMsg = new TgcSocketSendMsg();
                sendMsg.write(serverInitInfo);
                TgcSocketMessages.sendMessage(socket, sendMsg, TgcSocketMessageHeader.MsgType.InitialMessage);

                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }


        /// <summary>
        /// Hacer Handshake con el cliente para ver si es un cliente correcto
        /// </summary>
        /// <returns>True si todo salio bien</returns>
        private bool doHandshake(TgcSocketClientInfo clientInfo, Socket socket)
        {
            try
            {
                int clientHandshakeLength = Encoding.ASCII.GetBytes(TgcSocketClient.CLIENT_HANDSHAKE).Length;
                byte[] data = new byte[clientHandshakeLength];
                int recv = socket.Receive(data, data.Length, SocketFlags.None);
                if (recv > 0 && recv == clientHandshakeLength)
                {
                    string msg = Encoding.ASCII.GetString(data, 0, recv);
                    if (msg.Equals(TgcSocketClient.CLIENT_HANDSHAKE))
                    {
                        //Cliente aceptado, falta recibir datos iniciales
                        clientInfo.Status = TgcSocketClientInfo.ClientStatus.RequireInitialInfo;

                        //Enviar respuesta de handshake
                        socket.Send(Encoding.ASCII.GetBytes(SERVER_HANDSHAKE));

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
        /// Atender nuevas conexiones de clientes
        /// </summary>
        private void acceptNewClients()
        {
            //Ver si hay alguna conexion pendiente
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                //Aceptar nuevo cliente
                Socket clientSocket = serverSocket.Accept();

                //Ver si es un tipo de conexion valido
                if (clientSocket.AddressFamily == AddressFamily.InterNetwork &&
                    clientSocket.SocketType == SocketType.Stream &&
                    clientSocket.ProtocolType == ProtocolType.Tcp)
                {
                    //Agrear cliente válido a la lista
                    TgcSocketClientInfo clientInfo = new TgcSocketClientInfo(clientSocket);
                    allClients.Add(clientInfo);
                }
                //Si no es válido, desconectar
                else
                {
                    try
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                    }
                    catch (Exception)
                    {
                    }
                    
                }
            }
        }

        /// <summary>
        /// Desconectar a un cliente particular  del servidor
        /// </summary>
        /// <param name="playerId">ID del cliente a desconectar</param>
        public void disconnectClient(int playerId)
        {
            TgcSocketClientInfo clientInfo = getClientInfo(playerId);
            if (clientInfo != null)
            {
                allClients.Remove(clientInfo);
                connectedClients.Remove(clientInfo);
                try
                {
                    clientInfo.Socket.Shutdown(SocketShutdown.Both);
                    clientInfo.Socket.Close();
                }
                catch (Exception)
                {
                }
            }
        }


    }
}
