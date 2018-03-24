using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Networking;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     Ejemplo EjemploNetworkingModifier:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - GameEngine
    ///     Utiliza la herramienta TgcNetworkingModifier para manejo de Networking.
    ///     Aplicaci�n sencilla en la cual el cliente envia un mensaje cada 1 segundo al servidor,
    ///     y el servidor lo imprime por consola.... o como hacer el TP de Operativo en 5 min.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploNetworkingModifier : TGCExampleViewer
    {
        private float acumulatedTime;
        private TGCNetworkingModifier networkingMod;

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
            networkingMod = AddNetworking("Networking", "MyServer", "MyClient");

            acumulatedTime = 0;
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            //Actualizar siempre primero todos los valores de red.
            //Esto hace que el cliente y el servidor reciban todos los mensajes y actualicen su es
            networkingMod.updateNetwork();

            //Si el server est� online, analizar sus mensajes
            if (networkingMod.Server.Online)
            {
                //Analizar los mensajes recibidos
                for (var i = 0; i < networkingMod.Server.ReceivedMessagesCount; i++)
                {
                    //Leer el siguiente mensaje, cada vez que llamamos a nextReceivedMessage() consumimos un mensaje pendiente.
                    var msg = networkingMod.Server.nextReceivedMessage();

                    //Obtenter el primer elemento del mensaje, un string en este caso
                    var strMsg = (string)msg.Msg.readNext();

                    //Mostrar mensaje recibido en consola
                    Debug.WriteLine(strMsg, Color.Green);
                }
            }

            //Si el cliente est� online, analizar sus mensajes
            if (networkingMod.Client.Online)
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
                    networkingMod.Client.send(msg);
                }
            }
        }

        public override void Dispose()
        {
            //Cerrar todas las conexiones
            networkingMod.dispose();
        }
    }
}