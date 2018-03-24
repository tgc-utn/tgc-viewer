using System.Net;

namespace TGC.Examples.UserControls.Networking
{
    public class TgcSocketServerInfo
    {
        public TgcSocketServerInfo(IPAddress address, int port)
        {
            Address = address;
            Port = port;
        }

        /// <summary>
        ///     Direcci�n IP del cliente
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