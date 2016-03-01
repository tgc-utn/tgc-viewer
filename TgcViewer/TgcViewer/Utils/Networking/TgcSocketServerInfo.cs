using System.Net;

namespace TGC.Viewer.Utils.Networking
{
    public class TgcSocketServerInfo
    {
        public TgcSocketServerInfo(IPAddress address, int port)
        {
            Address = address;
            Port = port;
        }

        /// <summary>
        ///     Dirección IP del cliente
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        ///     Puerto del servidor
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///     Nombre del cliente
        /// </summary>
        public string Name { get; set; }
    }
}