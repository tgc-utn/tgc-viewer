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
    ///     EjemploClient
    /// </summary>
    public class EjemploClient : TGCExampleViewer
    {
        private float acumulatedTime;
        private int mensaje;
        private Socket listener;
        private TGCText2D text;

        public EjemploClient(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Multiplayer";
            Name = "EjemploClient";
            Description = "Ejemplo Client.";
        }

        public override void Init()
        {
            BackgroundColor = Color.Black;

            text = new TGCText2D();
            text.Text = "";
            text.Align = TGCText2D.TextAlign.RIGHT;
            text.Position = new Point(50, 75);
            text.Size = new Size(300, 100);
            text.Color = Color.Green;

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var localEP = new IPEndPoint(ipHostInfo.AddressList[0], 4444);
            listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Connect(localEP);
            listener.Blocking = false;

            acumulatedTime = 0;
            mensaje = 0;
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
                var data = new byte[1024];
                var recv = listener.Receive(data);
                if (recv > 0)
                {
                    var msg = Encoding.ASCII.GetString(data, 0, recv);
                    text.Text += msg + "\n";
                }
                else
                {
                    listener.Close();
                }
            }

            if (listener.Connected)
            {
                acumulatedTime += ElapsedTime;
                if (acumulatedTime > 2)
                {
                    acumulatedTime = 0;

                    var msg = "Mensaje Cliente: " + mensaje++;
                    listener.Send(Encoding.ASCII.GetBytes(msg));
                }
            }
        }

        public override void Render()
        {
            PreRender();
            DrawText.drawText("Este ejemplo solo es el Client, debe tener primero levantando un server...", 5, 50, Color.DarkCyan);
            text.Render();
            PostRender();
        }

        public override void Dispose()
        {
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }
    }
}