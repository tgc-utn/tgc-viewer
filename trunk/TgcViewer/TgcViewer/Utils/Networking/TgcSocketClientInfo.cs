using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TgcViewer.Utils.Networking
{
    public class TgcSocketClientInfo
    {

        private Socket socket;
        /// <summary>
        /// Socket del cliente abierto por el server
        /// </summary>
        internal Socket Socket
        {
            get { return socket; }
        }

        private IPAddress address;
        /// <summary>
        /// Dirección IP del cliente
        /// </summary>
        public IPAddress Address
        {
            get { return address; }
        }

        private string name;
        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int playerId;
        /// <summary>
        /// ID que identifica al cliente unívocamente
        /// </summary>
        public int PlayerId
        {
            get { return playerId; }
            set { playerId = value; }
        }


        /// <summary>
        /// Estados del cliente respecto de como los ve el servidor
        /// </summary>
        public enum ClientStatus
        {
            HandshakePending,
            RequireInitialInfo,
            WaitingClientOk,
            Connected,
            Disconnected,
        }

        private ClientStatus status;
        /// <summary>
        /// Estado del cliente respecto de como lo ve el servidor
        /// </summary>
        internal ClientStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public TgcSocketClientInfo(Socket clientSocket)
        {
            this.socket = clientSocket;
            this.status = ClientStatus.HandshakePending;

            //IP Address
            IPEndPoint endPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            this.address = endPoint.Address;
        }

        


    }
}
