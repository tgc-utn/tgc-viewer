using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace TgcViewer.Utils.Networking
{
    public class TgcSocketMessages
    {
        public static readonly int DEFAULT_PORT = 4774;

        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Enviar un mensaje de TGC por un socket
        /// </summary>
        /// <param name="socket">Socket por el cual enviar</param>
        /// <param name="msg">Mensaje a enviar</param>
        /// <param name="msgType">Tipo de mensaje a enviar</param>
        /// <returns>True si lo pudo hacer bien</returns>
        public static bool sendMessage(Socket socket, TgcSocketSendMsg msg, TgcSocketMessageHeader.MsgType msgType)
        {
            try
            {
                //Enviar Header
                byte[] msgBytes = msg.getBytes();
                byte[] data = BitConverter.GetBytes(msgBytes.Length);
                socket.Send(data);

                //Enviar msg
                socket.Send(msgBytes);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Recibe un mensaje de TGC por un socket
        /// </summary>
        /// <param name="socket">Socket del cual recibir</param>
        /// <param name="msgType">Tipo de mensaje esperado</param>
        /// <returns>Mensaje recibido o null si recibió mal</returns>
        public static TgcSocketRecvMsg receiveMessage(Socket socket, TgcSocketMessageHeader.MsgType msgType)
        {
            try
            {
                //Recibir header
                byte[] headerData = new byte[TgcSocketMessageHeader.HEADER_SIZE];
                int recv = socket.Receive(headerData, headerData.Length, SocketFlags.None);
                if (recv == TgcSocketMessageHeader.HEADER_SIZE)
                {
                    int msgLength = BitConverter.ToInt32(headerData, 0);

                    //Recibir cuerpo del mensaje
                    byte[] msgData = new byte[msgLength];
                    recv = socket.Receive(msgData, msgData.Length, SocketFlags.None);
                    if (recv == msgLength)
                    {
                        TgcSocketRecvMsg recvMsg = new TgcSocketRecvMsg(msgData);
                        return recvMsg;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

    }

    /// <summary>
    /// Tipos de header de los mensajes enviados
    /// </summary>
    [Serializable]
    public struct TgcSocketMessageHeader
    {
        public enum MsgType
        {
            InitialMessage = 0,
            RegularMessage = 1,
        }

        public int length;

        public static readonly int HEADER_SIZE = 4;
    }

    /// <summary>
    /// Información inicial enviada por el Cliente al Servidor
    /// </summary>
    [Serializable]
    public struct TgcSocketInitialInfoClient
    {
        public string clientName;
    }

    [Serializable]
    public struct TgcSocketInitialInfoServer
    {
        public string serverName;
        public int playerId;
    }

    /// <summary>
    /// Mensaje para enviar información
    /// </summary>
    public class TgcSocketSendMsg
    {
        BinaryFormatter binaryFormatter;
        List<byte[]> data;

        public TgcSocketSendMsg()
        {
            binaryFormatter = new BinaryFormatter();
            data = new List<byte[]>();
        }

        public TgcSocketSendMsg(object value)
            : this()
        {
            write(value);
        }

        /// <summary>
        /// Agregar un nuevo objeto/valor al mensaje
        /// </summary>
        /// <param name="value">objeto a agregar</param>
        public void write(object value)
        {
            if (!value.GetType().IsSerializable)
            {
                throw new Exception("El objeto que se quiere enviar no es Serializable");
            }
            try
            {
                MemoryStream objStream = new MemoryStream();
                binaryFormatter.Serialize(objStream, value);
                data.Add(objStream.GetBuffer());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al serializar objeto", ex);
            }
        }

        internal byte[] getBytes()
        {
            MemoryStream stream = new MemoryStream();
            binaryFormatter.Serialize(stream, data);
            return stream.GetBuffer();
        }
    }

    /// <summary>
    /// Mensaje para recibir información
    /// </summary>
    public class TgcSocketRecvMsg
    {
        BinaryFormatter binaryFormatter;
        List<byte[]> data;
        int dataIndex;

        public TgcSocketRecvMsg(byte[] bytesData)
        {
            MemoryStream stream = new MemoryStream(bytesData, false);
            binaryFormatter = new BinaryFormatter();
            data = (List<byte[]>)binaryFormatter.Deserialize(stream);
            dataIndex = 0;
        }

        /// <summary>
        /// Leer un objeto del mensaje y avanza a la siguiente posición
        /// </summary>
        /// <returns>Objeto leido</returns>
        public object readNext()
        {
            try
            {
                byte[] bytes = data[dataIndex];
                dataIndex++;
                MemoryStream objStream = new MemoryStream(bytes, false);
                return binaryFormatter.Deserialize(objStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al deserializar objeto de Mensaje", ex);
            }
        }
    }

    /// <summary>
    /// Mensajes enviados por un cliente y recibidos por el server
    /// </summary>
    public class TgcSocketClientRecvMesg
    {
        private TgcSocketRecvMsg msg;
        /// <summary>
        /// Mensaje recibido
        /// </summary>
        public TgcSocketRecvMsg Msg
        {
            get { return msg; }
        }

        private int playerId;
        /// <summary>
        /// ID del cliente que lo envio
        /// </summary>
        public int PlayerId
        {
            get { return playerId; }
        }

        public TgcSocketClientRecvMesg(TgcSocketRecvMsg msg, int playerId)
        {
            this.msg = msg;
            this.playerId = playerId;
        }
    }
}
