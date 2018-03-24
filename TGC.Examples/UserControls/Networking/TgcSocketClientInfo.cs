using System.Net;
using System.Net.Sockets;

namespace TGC.Examples.UserControls.Networking
{
    public class TgcSocketClientInfo
    {
        /// <summary>
        ///     Estados del cliente respecto de como los ve el servidor
        /// </summary>
        public enum ClientStatus
        {
            HandshakePending,
            RequireInitialInfo,
            WaitingClientOk,
            Connected,
            Disconnected
        }

        public TgcSocketClientInfo(Socket clientSocket)
        {
            Socket = clientSocket;
            Status = ClientStatus.HandshakePending;

            //IP Address
            var endPoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            Address = endPoint.Address;
        }

        /// <summary>
        ///     Socket del cliente abierto por el server
        /// </summary>
        internal Socket Socket { get; }

        /// <summary>
        ///     Direcci�n IP del cliente
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        ///     Nombre del cliente
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     ID que identifica al cliente un�vocamente
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        ///     Estado del cliente respecto de como lo ve el servidor
        /// </summary>
        internal ClientStatus Status { get; set; }
    }
}