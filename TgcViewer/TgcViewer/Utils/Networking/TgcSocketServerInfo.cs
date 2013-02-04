using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace TgcViewer.Utils.Networking
{
    public class TgcSocketServerInfo
    {
        private IPAddress address;
        /// <summary>
        /// Dirección IP del cliente
        /// </summary>
        public IPAddress Address
        {
            get { return address; }
        }

        private int port;
        /// <summary>
        /// Puerto del servidor
        /// </summary>
        public int Port
        {
            get { return port; }
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


        public TgcSocketServerInfo(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
        }


    }
}
