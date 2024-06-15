using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Networking;

namespace TGC.Examples.Multiplayer
{
    /// <summary>
    ///     Ejemplo VehiculosMultiplayer:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - GameEngine
    ///     Utiliza la herramienta TgcNetworkingModifier para manejo de Networking.
    ///     Partida multiplayer en la cual hasta 4 jugadores pueden conectarse.
    ///     Cada uno posee un modelo de vehiculo distinto y se puede mover por el escenario.
    ///     Al moverse envia la informaci�n de su posici�n al server y recibe la actualizaci�n
    ///     de las posiciones de los dem�s jugadores.
    ///     El server simplemente recibe y redirige la informaci�n.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class VehiculosMultiplayer : TGCExampleViewerNetworking
    {
        private float acumulatedTime;

        public VehiculosMultiplayer(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Multiplayer";
            Name = "Vehiculos Multiplayer";
            Description = "Multiplayer en la cual hasta 4 jugadores pueden conectarse y utilizar veh�culos sobre un mismo escenario.";
        }

        public override void Init()
        {
            //Crear Modifier de Networking
            Init("Networking", "VehiculosServer", "VehiculosClient", TgcSocketMessages.DEFAULT_PORT);

            acumulatedTime = 0;

            //Informacion inicial del server
            initServerData();

            //Iniciar cliente
            initClient();
        }

        public override void Tick()
        {
            //Sobre escribo el metodo Tick para se corra todo el tiempo el render y el update.
            UnlimitedTick();
        }

        public override void Update()
        {
            //  Se debe escribir toda la l�gica de computo del modelo, as� como tambi�n verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();
            //Actualizar siempre primero todos los valores de red.
            //Esto hace que el cliente y el servidor reciban todos los mensajes y actualicen su estado interno
            updateNetwork();

            //Analizar eventos en el server
            if (Server.Online)
            {
                updateServer();
            }

            //Analizar eventos en el cliente
            if (Client.Online)
            {
                updateClient();
            }
            PostRender();
        }

        public override void Dispose()
        {
            //Cierra todas las conexiones
            base.Dispose();

            piso.Dispose();

            //Renderizar meshPrincipal
            if (meshPrincipal != null)
            {
                meshPrincipal.Dispose();
            }

            //Renderizar otrosMeshes
            foreach (var entry in otrosMeshes)
            {
                entry.Value.Dispose();
            }
        }

        #region Cosas del Server

        /// <summary>
        ///     Tipos de mensajes que envia el server.
        ///     Los Enums son serializables naturalmente.
        /// </summary>
        private enum MyServerProtocol
        {
            InformacionInicial,
            OtroClienteConectado,
            ActualizarUbicaciones,
            OtroClienteDesconectado
        }

        /// <summary>
        ///     Clase para almacenar informaci�n de cada veh�culo.
        ///     Tiene que tener la annotation [Serializable]
        /// </summary>
        [Serializable]
        private class VehiculoData
        {
            public readonly TGCVector3 initialPos;
            public readonly string meshPath;
            public int playerID;

            public VehiculoData(TGCVector3 initialPos, string meshPath)
            {
                playerID = -1;
                this.initialPos = initialPos;
                this.meshPath = meshPath;
            }
        }

        private VehiculoData[] vehiculosData;

        /// <summary>
        ///     Configuracion inicial del server
        /// </summary>
        private void initServerData()
        {
            //Configurar datos para los 4 clientes posibles del servidor
            var mediaPath = MediaDir + "MeshCreator\\Meshes\\Vehiculos\\";
            vehiculosData = new[]
            {
                new VehiculoData(new TGCVector3(0, 0, 0), mediaPath + "TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml"),
                new VehiculoData(new TGCVector3(100, 0, 0), mediaPath + "HelicopteroMilitar\\HelicopteroMilitar-TgcScene.xml"),
                new VehiculoData(new TGCVector3(200, 0, 0), mediaPath + "Auto\\Auto-TgcScene.xml"),
                new VehiculoData(new TGCVector3(0, 0, 200), mediaPath + "AerodeslizadorFuturista\\AerodeslizadorFuturista-TgcScene.xml")
            };
        }

        /// <summary>
        ///     Actualizar l�gica del server
        /// </summary>
        private void updateServer()
        {
            //Iterar sobre todos los nuevos clientes que se conectaron
            for (var i = 0; i < NewClientsCount; i++)
            {
                //Al llamar a nextNewClient() consumimos el aviso de conexion de un nuevo cliente
                var clientInfo = nextNewClient();
                atenderNuevoCliente(clientInfo);
            }

            //Iterar sobre todos los nuevos clientes que se desconectaron
            for (var i = 0; i < DisconnectedClientsCount; i++)
            {
                //Al llamar a nextNewClient() consumimos el aviso de desconexi�n de un nuevo cliente
                var clientInfo = nextDisconnectedClient();
                atenderClienteDesconectado(clientInfo);
            }

            //Atender mensajes recibidos
            for (var i = 0; i < Server.ReceivedMessagesCount; i++)
            {
                //El primer mensaje es el header de nuestro protocolo del ejemplo
                var clientMsg = Server.nextReceivedMessage();
                var msg = clientMsg.Msg;
                var msgType = (MyClientProtocol)msg.readNext();

                switch (msgType)
                {
                    case MyClientProtocol.PosicionActualizada:
                        serverAtenderPosicionActualizada(clientMsg);
                        break;
                }
            }
        }

        /// <summary>
        ///     Aceptar cliente y mandarle informacion inicial
        /// </summary>
        private void atenderNuevoCliente(TgcSocketClientInfo clientInfo)
        {
            //Si el cupo est� lleno, desconectar cliente
            if (Server.Clients.Count > vehiculosData.Length)
            {
                Server.disconnectClient(clientInfo.PlayerId);
            }
            //Darla la informaci�n inicial al cliente
            else
            {
                var currentClientIndex = Server.Clients.Count - 1;
                var data = vehiculosData[currentClientIndex];
                data.playerID = clientInfo.PlayerId;

                //Enviar informaci�n al cliente
                //Primero indicamos que mensaje del protocolo es
                var msg = new TgcSocketSendMsg();
                msg.write(MyServerProtocol.InformacionInicial);
                msg.write(data);
                //Tambi�n le enviamos la informaci�n de los dem�s clientes hasta el momento
                //Cantidad de clientes que hay
                msg.write(Server.Clients.Count - 1);
                //Data de todos los clientes anteriores, salvo el ultimo que es el nuevo agregado recien
                for (var i = 0; i < Server.Clients.Count - 1; i++)
                {
                    msg.write(vehiculosData[i]);
                }

                Server.sendToClient(clientInfo.PlayerId, msg);

                //Avisar a todos los dem�s clientes conectados (excepto este) que hay uno nuevo
                var msg2 = new TgcSocketSendMsg();
                msg2.write(MyServerProtocol.OtroClienteConectado);
                msg2.write(data);
                Server.sendToAllExceptOne(clientInfo.PlayerId, msg2);
            }
        }

        private void atenderClienteDesconectado(TgcSocketClientInfo clientInfo)
        {
            //Enviar info de desconexion a todos los clientes
            var msg = new TgcSocketSendMsg();
            msg.write(MyServerProtocol.OtroClienteDesconectado);
            msg.write(clientInfo.PlayerId);
            Server.sendToClient(clientInfo.PlayerId, msg);

            //Extender para permitir que se conecten nuevos ususarios
        }

        /// <summary>
        ///     Avisar a todos los dem�s clientes sobre la nueva posicion de este cliente
        /// </summary>
        private void serverAtenderPosicionActualizada(TgcSocketClientRecvMesg clientMsg)
        {
            //Nueva posicion del cliente
            var newPos = (TGCMatrix)clientMsg.Msg.readNext();

            //Enviar a todos menos al cliente que nos acaba de informar
            var sendMsg = new TgcSocketSendMsg();
            sendMsg.write(MyServerProtocol.ActualizarUbicaciones);
            sendMsg.write(clientMsg.PlayerId);
            sendMsg.write(newPos);
            Server.sendToAllExceptOne(clientMsg.PlayerId, sendMsg);
        }

        #endregion Cosas del Server

        #region Cosas del Client

        private const float VELODICAD_CAMINAR = 250f;
        private const float VELOCIDAD_ROTACION = 120f;

        /// <summary>
        ///     Tipos de mensajes que envia el cliente
        /// </summary>
        private enum MyClientProtocol
        {
            PosicionActualizada
        }

        private TGCBox piso;
        private TgcMesh meshPrincipal;
        private readonly Dictionary<int, TgcMesh> otrosMeshes = new Dictionary<int, TgcMesh>();
        private TgcThirdPersonCamera camaraInterna;

        /// <summary>
        ///     Iniciar cliente
        /// </summary>
        private void initClient()
        {
            //Crear piso
            var pisoTexture = TGCTexture.CreateTexture(D3DDevice.Instance.Device, MediaDir + "Texturas\\Quake\\TexturePack2\\rock_wall.jpg");
            piso = TGCBox.fromSize(new TGCVector3(0, -60, 0), new TGCVector3(5000, 5, 5000), pisoTexture);
        }

        /// <summary>
        ///     Actualizar l�gicad el cliente
        /// </summary>
        private void updateClient()
        {
            //Analizar los mensajes recibidos
            for (var i = 0; i < Client.ReceivedMessagesCount; i++)
            {
                //El primer mensaje es el header de nuestro protocolo del ejemplo
                var msg = Client.nextReceivedMessage();
                var msgType = (MyServerProtocol)msg.readNext();

                //Ver que tipo de mensaje es
                switch (msgType)
                {
                    case MyServerProtocol.InformacionInicial:
                        clienteAtenderInformacionInicial(msg);
                        break;

                    case MyServerProtocol.OtroClienteConectado:
                        clienteAtenderOtroClienteConectado(msg);
                        break;

                    case MyServerProtocol.ActualizarUbicaciones:
                        clienteAtenderActualizarUbicaciones(msg);
                        break;

                    case MyServerProtocol.OtroClienteDesconectado:
                        clienteAtenderOtroClienteDesconectado(msg);
                        break;
                }
            }

            if (meshPrincipal != null)
            {
                //Renderizar todo
                renderClient();

                //Enviar al server mensaje con posicion actualizada, 10 paquetes por segundo
                acumulatedTime += ElapsedTime;
                if (acumulatedTime > 0.1)
                {
                    acumulatedTime = 0;

                    //Enviar posicion al server
                    var msg = new TgcSocketSendMsg();
                    msg.write(MyClientProtocol.PosicionActualizada);
                    msg.write(meshPrincipal.Transform);
                    Client.send(msg);
                }
            }
        }

        /// <summary>
        ///     Atender mensaje InformacionInicial
        /// </summary>
        private void clienteAtenderInformacionInicial(TgcSocketRecvMsg msg)
        {
            //Recibir data
            var vehiculoData = (VehiculoData)msg.readNext();

            //Cargar mesh
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(vehiculoData.meshPath);
            meshPrincipal = scene.Meshes[0];

            //Ubicarlo en escenario
            meshPrincipal.Position = vehiculoData.initialPos;

            //Camara en 3ra persona
            camaraInterna = new TgcThirdPersonCamera(meshPrincipal.Position, 100, 400);
            camaraInterna.resetValues();
            Camera = camaraInterna;

            //Ver si ya habia mas clientes para cuando nosotros nos conectamos
            var otrosVehiculosCant = (int)msg.readNext();
            for (var i = 0; i < otrosVehiculosCant; i++)
            {
                var vData = (VehiculoData)msg.readNext();
                crearMeshOtroCliente(vData);
            }
        }

        /// <summary>
        ///     Renderizar toda la parte cliente, con el manejo de input
        /// </summary>
        private void renderClient()
        {
            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -VELODICAD_CAMINAR;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = VELODICAD_CAMINAR;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                meshPrincipal.RotateY(Geometry.DegreeToRadian(rotate * ElapsedTime));
                this.camaraInterna.rotateY(rotate);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                meshPrincipal.MoveOrientedY(moveForward * ElapsedTime);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            this.camaraInterna.Target = meshPrincipal.Position;

            //Render piso
            piso.Render();

            //Renderizar meshPrincipal
            if (meshPrincipal != null)
            {
                meshPrincipal.Render();
            }

            //Renderizar otrosMeshes
            foreach (var entry in otrosMeshes)
            {
                entry.Value.Render();
            }
        }

        /// <summary>
        ///     Crear Mesh para el nuevo cliente conectado
        /// </summary>
        private void clienteAtenderOtroClienteConectado(TgcSocketRecvMsg msg)
        {
            //Recibir data
            var vehiculoData = (VehiculoData)msg.readNext();
            crearMeshOtroCliente(vehiculoData);
        }

        /// <summary>
        ///     Crear Mesh para el nuevo cliente conectado
        /// </summary>
        private void crearMeshOtroCliente(VehiculoData vehiculoData)
        {
            //Cargar mesh
            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(vehiculoData.meshPath);
            var mesh = scene.Meshes[0];
            otrosMeshes.Add(vehiculoData.playerID, mesh);

            //Ubicarlo en escenario
            mesh.Transform = TGCMatrix.Translation(vehiculoData.initialPos);
        }

        /// <summary>
        ///     Actualizar posicion de otro cliente
        /// </summary>
        private void clienteAtenderActualizarUbicaciones(TgcSocketRecvMsg msg)
        {
            var playerId = (int)msg.readNext();
            var nextPos = (TGCMatrix)msg.readNext();

            if (otrosMeshes.ContainsKey(playerId))
            {
                otrosMeshes[playerId].Transform = nextPos;
            }
        }

        /// <summary>
        ///     Quitar otro cliente que desconecto
        /// </summary>
        private void clienteAtenderOtroClienteDesconectado(TgcSocketRecvMsg msg)
        {
            var playerId = (int)msg.readNext();
            otrosMeshes[playerId].Dispose();
            otrosMeshes.Remove(playerId);
        }

        #endregion Cosas del Client
    }
}