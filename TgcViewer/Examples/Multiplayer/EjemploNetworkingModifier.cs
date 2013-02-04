using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Networking;

namespace Examples
{
    /// <summary>
    /// Ejemplo EjemploNetworkingModifier:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - GameEngine
    /// 
    /// Utiliza la herramienta TgcNetworkingModifier para manejo de Networking.
    /// Aplicación sencilla en la cual el cliente envia un mensaje cada 1 segundo al servidor,
    /// y el servidor lo imprime por consola.... o como hacer el TP de Operativo en 5 min.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploNetworkingModifier : TgcExample
    {
        TgcNetworkingModifier networkingMod;
        float acumulatedTime;

        public override string getCategory()
        {
            return "Multiplayer";
        }

        public override string getName()
        {
            return "NetworkingModifier";
        }

        public override string getDescription()
        {
            return "El cliente envia un mensaje cada 1 segundo al servidor y el servidor lo imprime por consola.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Modifier de Networking
            networkingMod = GuiController.Instance.Modifiers.addNetworking("Networking", "MyServer", "MyClient");

            acumulatedTime = 0;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Actualizar siempre primero todos los valores de red.
            //Esto hace que el cliente y el servidor reciban todos los mensajes y actualicen su es
            networkingMod.updateNetwork();

            //Si el server está online, analizar sus mensajes
            if (networkingMod.Server.Online)
            {
                //Analizar los mensajes recibidos
                for (int i = 0; i < networkingMod.Server.ReceivedMessagesCount; i++)
			    {
                    //Leer el siguiente mensaje, cada vez que llamamos a nextReceivedMessage() consumimos un mensaje pendiente.
                    TgcSocketClientRecvMesg msg = networkingMod.Server.nextReceivedMessage();

                    //Obtenter el primer elemento del mensaje, un string en este caso
                    string strMsg = (string)msg.Msg.readNext();

                    //Mostrar mensaje recibido en consola
                    GuiController.Instance.Logger.log(strMsg, Color.Green);
			    }
                
            }

            //Si el cliente está online, analizar sus mensajes
            if (networkingMod.Client.Online)
            {
                //Mandamos un mensaje al server cada 1 segundo
                acumulatedTime += elapsedTime;
                if (acumulatedTime > 1)
                {
                    acumulatedTime = 0;

                    //Crear nuevo mensaje a enviar
                    TgcSocketSendMsg msg = new TgcSocketSendMsg();

                    //Agregar un dato al mensaje, un string en este caso
                    msg.write("Hello world - ElapsedTime: " + elapsedTime);

                    //Enviar mensaje al server
                    networkingMod.Client.send(msg);
                }
            }
            
            
        }

        public override void close()
        {
            //Cerrar todas las conexiones
            networkingMod.dispose();
        }

    }
}
