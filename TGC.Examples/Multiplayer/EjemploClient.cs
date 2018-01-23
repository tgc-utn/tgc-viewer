using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     EjemploClient
    /// </summary>
    public class EjemploClient : TGCExampleViewer
    {
        private float acumulatedTime;
        private int mensaje;
        private Socket serverSocket;

        public EjemploClient(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers) : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Multiplayer";
            Name = "EjemploClient";
            Description = "EjemploClient.";
        }

        public override void Init()
        {
            var ipAddress = Dns.GetHostAddresses("localhost");
            var Ipep = new IPEndPoint(ipAddress[0], 4444);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Connect(Ipep);
            serverSocket.Blocking = false;

            acumulatedTime = 0;
            mensaje = 0;
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                var data = new byte[1024];
                var recv = serverSocket.Receive(data);
                if (recv > 0)
                {
                    var msg = Encoding.ASCII.GetString(data, 0, recv);
                    Debug.WriteLine(msg);
                }
                else
                {
                    serverSocket.Close();
                }
            }

            if (serverSocket.Connected)
            {
                acumulatedTime += ElapsedTime;
                if (acumulatedTime > 2)
                {
                    acumulatedTime = 0;

                    var msg = "Mensaje Cliente: " + mensaje++;
                    serverSocket.Send(Encoding.ASCII.GetBytes(msg));
                }
            }
        }

        public override void Dispose()
        {
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }
    }
}