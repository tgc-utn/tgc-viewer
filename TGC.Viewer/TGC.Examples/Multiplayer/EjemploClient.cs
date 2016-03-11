using System.Net;
using System.Net.Sockets;
using System.Text;
using TGC.Core.Example;
using TGC.Viewer;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     EjemploClient
    /// </summary>
    public class EjemploClient : TgcExample
    {
        private float acumulatedTime;
        private int mensaje;
        private Socket serverSocket;

        public override string getCategory()
        {
            return "Multiplayer";
        }

        public override string getName()
        {
            return "EjemploClient";
        }

        public override string getDescription()
        {
            return "EjemploClient.";
        }

        public override void init()
        {
            var ipAddress = Dns.GetHostAddresses("localhost");
            var Ipep = new IPEndPoint(ipAddress[0], 4444);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Connect(Ipep);
            serverSocket.Blocking = false;

            acumulatedTime = 0;
            mensaje = 0;
        }

        public override void render(float elapsedTime)
        {
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                var data = new byte[1024];
                var recv = serverSocket.Receive(data);
                if (recv > 0)
                {
                    var msg = Encoding.ASCII.GetString(data, 0, recv);
                    GuiController.Instance.Logger.log(msg);
                }
                else
                {
                    serverSocket.Close();
                }
            }

            if (serverSocket.Connected)
            {
                acumulatedTime += elapsedTime;
                if (acumulatedTime > 2)
                {
                    acumulatedTime = 0;

                    var msg = "Mensaje Cliente: " + mensaje++;
                    serverSocket.Send(Encoding.ASCII.GetBytes(msg));
                }
            }
        }

        public override void close()
        {
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }
    }
}