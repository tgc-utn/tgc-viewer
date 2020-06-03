using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TGC.Core.Text;
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
        private Socket listener;
        private TgcText2D text;

        public EjemploServer(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Multiplayer";
            Name = "EjemploServer";
            Description = "Ejemplo Server.";
        }

        public override void Init()
        {
            BackgroundColor = Color.Black;

            text = new TgcText2D();
            text.Text = "";
            text.Align = TgcText2D.TextAlign.RIGHT;
            text.Position = new Point(50, 75);
            text.Size = new Size(300, 100);
            text.Color = Color.Green;

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var localEP = new IPEndPoint(ipHostInfo.AddressList[0], 4444);
            listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Blocking = false;
            listener.Bind(localEP);
            listener.Listen(32);

            clients = new List<Socket>();

            /*
            BinaryFormatter f = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            f.Serialize(stream, new object());
            listener.Send(stream.ToArray());
            */
        }

        public override void Tick()
        {
            //Sobre escribo el metodo Tick para se corra todo el tiempo el render y el update.
            UnlimitedTick();
        }

        public override void Update()
        {
            if (listener.Poll(0, SelectMode.SelectRead))
            {
                var clientSocket = listener.Accept();
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
                            text.Text += msg + "\n";

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

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Este ejemplo solo es el Server, debe conectar clientes, una vez conectado un cliente espere uno instantes para ver los mensajes.", 5, 50, Color.DarkCyan);
            text.render();
            PostRender();
        }

        public override void Dispose()
        {
            foreach (var clientSocket in clients)
            {
                clientSocket.Close();
            }
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }
    }
}