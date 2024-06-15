using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Text;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Networking;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     Ejemplo EjemploNetworkingModifier:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - GameEngine
    ///     Utiliza la herramienta TgcNetworkingModifier para manejo de Networking.
    ///     Aplicación sencilla en la cual el cliente envia un mensaje cada 1 segundo al servidor,
    ///     y el servidor lo imprime por consola.... o como hacer el TP de Operativo en 5 min.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploNetworkingModifier : TGCExampleViewerNetworking
    {
        private float acumulatedTime;
        private TGCText2D text;

        public EjemploNetworkingModifier(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Multiplayer";
            Name = "NetworkingModifier";
            Description = "El cliente envia un mensaje cada 1 segundo al servidor y el servidor lo imprime por consola.";
        }

        public override void Init()
        {
            //Crear Modifier de Networking
            Init("Networking", "MyServer", "MyClient", TgcSocketMessages.DEFAULT_PORT);

            acumulatedTime = 0;
            BackgroundColor = Color.Black;

            text = new TGCText2D();
            text.Text = "";
            text.Align = TGCText2D.TextAlign.RIGHT;
            text.Position = new Point(50, 50);
            text.Size = new Size(300, 100);
            text.Color = Color.Green;
        }

        public override void Tick()
        {
            //Sobre escribo el metodo Tick para se corra todo el tiempo el render y el update.
            UnlimitedTick();
        }

        public override void Update()
        {
            //Actualizar siempre primero todos los valores de red.
            //Esto hace que el cliente y el servidor reciban todos los mensajes y actualicen su es
            updateNetwork();

            //Si el server está online, analizar sus mensajes
            if (Server.Online)
            {
                //Analizar los mensajes recibidos
                for (var i = 0; i < Server.ReceivedMessagesCount; i++)
                {
                    //Leer el siguiente mensaje, cada vez que llamamos a nextReceivedMessage() consumimos un mensaje pendiente.
                    var msg = Server.nextReceivedMessage();

                    //Obtenter el primer elemento del mensaje, un string en este caso
                    text.Text += msg.Msg.readNext().ToString();
                }
            }

            //Si el cliente está online, analizar sus mensajes
            if (Client.Online)
            {
                //Mandamos un mensaje al server cada 1 segundo
                acumulatedTime += ElapsedTime;
                if (acumulatedTime > 1)
                {
                    acumulatedTime = 0;

                    //Crear nuevo mensaje a enviar
                    var msg = new TgcSocketSendMsg();

                    //Agregar un dato al mensaje, un string en este caso
                    msg.write("Hello world - ElapsedTime: " + ElapsedTime);

                    //Enviar mensaje al server
                    Client.send(msg);
                }
            }
        }

        public override void Render()
        {
            PreRender();
            text.Render();
            PostRender();
        }

        public override void Dispose()
        {
            //Cerrar todas las conexiones
            base.Dispose();
        }
    }
}