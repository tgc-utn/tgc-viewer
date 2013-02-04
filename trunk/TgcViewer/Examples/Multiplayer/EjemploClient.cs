using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using System.Net.Sockets;
using System.Net;

namespace Examples
{
    /// <summary>
    /// EjemploClient
    /// </summary>
    public class EjemploClient : TgcExample
    {

        Socket serverSocket;
        float acumulatedTime;
        int mensaje;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            IPAddress[] ipAddress = Dns.GetHostAddresses("localhost");
            IPEndPoint Ipep = new IPEndPoint(ipAddress[0], 4444);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            

            serverSocket.Connect(Ipep);
            serverSocket.Blocking = false;

            acumulatedTime = 0;
            mensaje = 0;
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] data = new byte[1024];
                int recv = serverSocket.Receive(data);
                if (recv > 0)
                {
                    string msg = Encoding.ASCII.GetString(data, 0, recv);
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

                    string msg = "Mensaje Cliente: " + (mensaje++);
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
