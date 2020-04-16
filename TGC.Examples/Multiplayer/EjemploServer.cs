using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     EjemploServer
    /// </summary>
    public class EjemploServer : TGCExampleViewer
    {
        private List<Socket> clients;
        private int recibido;
        private Socket serverSocket;

        public EjemploServer(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Multiplayer";
            Name = "EjemploServer";
            Description = "EjemploServer.";
        }

        public override void Init()
        {
            var ipAddress = Dns.GetHostAddresses("localhost");
            var Ipep = new IPEndPoint(ipAddress[0], 4444);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Blocking = false;
            serverSocket.Bind(Ipep);
            serverSocket.Listen(32);

            clients = new List<Socket>();

            /*
            BinaryFormatter f = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            f.Serialize(stream, new object());
            serverSocket.Send(stream.ToArray());
            */
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                var clientSocket = serverSocket.Accept();
                clients.Add(clientSocket);
            }

            foreach (var clientSocket in clients)
            {
                if (clientSocket.Poll(0, SelectMode.SelectRead))
                {
                    var data = new byte[1024];
                    try
                    {
                        var recv = clientSocket.Receive(data);
                        if (recv > 0)
                        {
                            var msg = Encoding.ASCII.GetString(data, 0, recv);
                            Debug.WriteLine(msg);

                            var msgRta = "Recibido" + recibido++;
                            clientSocket.Send(Encoding.ASCII.GetBytes(msgRta));
                        }
                        else
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                            //clients.Remove(clientSocket);
                        }
                    }
                    catch (SocketException ex)
                    {
                        var error = ex.SocketErrorCode;

                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        //clients.Remove(clientSocket);
                    }
                }
            }
        }

        public override void Dispose()
        {
            foreach (var clientSocket in clients)
            {
                clientSocket.Close();
            }
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }
    }
}