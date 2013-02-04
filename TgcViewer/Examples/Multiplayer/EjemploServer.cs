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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Examples
{
    /// <summary>
    /// EjemploServer
    /// </summary>
    public class EjemploServer : TgcExample
    {

        Socket serverSocket;
        List<Socket> clients;
        int recibido = 0;

        public override string getCategory()
        {
            return "Multiplayer";
        }

        public override string getName()
        {
            return "EjemploServer";
        }

        public override string getDescription()
        {
            return "EjemploServer.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            IPAddress[] ipAddress = Dns.GetHostAddresses("localhost");
            IPEndPoint Ipep = new IPEndPoint(ipAddress[0], 4444);
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


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            if (serverSocket.Poll(0, SelectMode.SelectRead))
            {
                Socket clientSocket = serverSocket.Accept();
                clients.Add(clientSocket);
            }

            foreach (Socket clientSocket in clients)
            {
                if (clientSocket.Poll(0, SelectMode.SelectRead))
                {
                    byte[] data = new byte[1024];
                    try
                    {
                        int recv = clientSocket.Receive(data);
                        if (recv > 0)
                        {
                            string msg = Encoding.ASCII.GetString(data, 0, recv);
                            GuiController.Instance.Logger.log(msg);

                            string msgRta = "Recibido" + (recibido++);
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
                        SocketError error = ex.SocketErrorCode;

                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        //clients.Remove(clientSocket);
                    }
                    

                }
            }

        }

        public override void close()
        {
            foreach (Socket clientSocket in clients)
            {
                clientSocket.Close();
            }
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }

    }
}
