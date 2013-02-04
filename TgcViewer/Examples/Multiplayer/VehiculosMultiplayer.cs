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
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.TgcGeometry;

namespace Examples
{
    /// <summary>
    /// Ejemplo VehiculosMultiplayer:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - GameEngine
    /// 
    /// Utiliza la herramienta TgcNetworkingModifier para manejo de Networking.
    /// Partida multiplayer en la cual hasta 4 jugadores pueden conectarse.
    /// Cada uno posee un modelo de vehiculo distinto y se puede mover por el escenario.
    /// Al moverse envia la información de su posición al server y recibe la actualización
    /// de las posiciones de los demás jugadores.
    /// El server simplemente recibe y redirige la información.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class VehiculosMultiplayer : TgcExample
    {
        TgcNetworkingModifier networkingMod;
        float acumulatedTime;


        public override string getCategory()
        {
            return "Multiplayer";
        }

        public override string getName()
        {
            return "Vehiculos Multiplayer";
        }

        public override string getDescription()
        {
            return "Partida multiplayer en la cual hasta 4 jugadores pueden conectarse y utilizar vehículos sobre un mismo escenario.";
        }


        public override void init()
        {
            //Crear Modifier de Networking
            networkingMod = GuiController.Instance.Modifiers.addNetworking("Networking", "VehiculosServer", "VehiculosClient");

            acumulatedTime = 0;

            //Informacion inicial del server
            initServerData();

            //Iniciar cliente
            initClient();
        }


        public override void render(float elapsedTime)
        {
            //Actualizar siempre primero todos los valores de red.
            //Esto hace que el cliente y el servidor reciban todos los mensajes y actualicen su estado interno
            networkingMod.updateNetwork();


            //Analizar eventos en el server
            if (networkingMod.Server.Online)
            {
                updateServer();
            }

            //Analizar eventos en el cliente
            if (networkingMod.Client.Online)
            {
                updateClient();
            }
        }

        public override void close()
        {
            //Cierra todas las conexiones
            networkingMod.dispose();

            piso.dispose();

            //Renderizar meshPrincipal
            if (meshPrincipal != null)
            {
                meshPrincipal.dispose();
            }

            //Renderizar otrosMeshes
            foreach (KeyValuePair<int, TgcMesh> entry in otrosMeshes)
            {
                entry.Value.dispose();
            }
        }


        #region Cosas del Server

        /// <summary>
        /// Tipos de mensajes que envia el server.
        /// Los Enums son serializables naturalmente.
        /// </summary>
        enum MyServerProtocol
        {
            InformacionInicial,
            OtroClienteConectado,
            ActualizarUbicaciones,
            OtroClienteDesconectado,
        }

        /// <summary>
        /// Clase para almacenar información de cada vehículo.
        /// Tiene que tener la annotation [Serializable]
        /// </summary>
        [Serializable]
        class VehiculoData
        {
            public VehiculoData(Vector3 initialPos, string meshPath)
            {
                this.playerID = -1;
                this.initialPos = initialPos;
                this.meshPath = meshPath;
            }

            public Vector3 initialPos;
            public string meshPath;
            public int playerID;
        }

        VehiculoData[] vehiculosData;

        /// <summary>
        /// Configuracion inicial del server
        /// </summary>
        private void initServerData()
        {
            //Configurar datos para los 4 clientes posibles del servidor
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\";
            vehiculosData = new VehiculoData[]
            {
                new VehiculoData(new Vector3(0,0,0), mediaPath + "TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml"),
                new VehiculoData(new Vector3(100,0,0), mediaPath + "HelicopteroMilitar\\HelicopteroMilitar-TgcScene.xml"),
                new VehiculoData(new Vector3(200,0,0), mediaPath + "Auto\\Auto-TgcScene.xml"),
                new VehiculoData(new Vector3(0,0,200), mediaPath + "AerodeslizadorFuturista\\AerodeslizadorFuturista-TgcScene.xml"),
            };
        }
 

        /// <summary>
        /// Actualizar lógica del server
        /// </summary>
        private void updateServer()
        {
            //Iterar sobre todos los nuevos clientes que se conectaron
            for (int i = 0; i < networkingMod.NewClientsCount; i++)
            {
                //Al llamar a nextNewClient() consumimos el aviso de conexion de un nuevo cliente
                TgcSocketClientInfo clientInfo = networkingMod.nextNewClient();
                atenderNuevoCliente(clientInfo);
            }

            //Iterar sobre todos los nuevos clientes que se desconectaron
            for (int i = 0; i < networkingMod.DisconnectedClientsCount; i++)
            {
                //Al llamar a nextNewClient() consumimos el aviso de desconexión de un nuevo cliente
                TgcSocketClientInfo clientInfo = networkingMod.nextDisconnectedClient();
                atenderClienteDesconectado(clientInfo);
            }

            //Atender mensajes recibidos
            for (int i = 0; i < networkingMod.Server.ReceivedMessagesCount; i++)
            {
                //El primer mensaje es el header de nuestro protocolo del ejemplo
                TgcSocketClientRecvMesg clientMsg = networkingMod.Server.nextReceivedMessage();
                TgcSocketRecvMsg msg = clientMsg.Msg;
                MyClientProtocol msgType = (MyClientProtocol)msg.readNext();

                switch (msgType)
                {
                    case MyClientProtocol.PosicionActualizada:
                        serverAtenderPosicionActualizada(clientMsg);
                        break;
                }
            }
        }

        /// <summary>
        /// Aceptar cliente y mandarle informacion inicial
        /// </summary>
        private void atenderNuevoCliente(TgcSocketClientInfo clientInfo)
        {
            //Si el cupo está lleno, desconectar cliente
            if (networkingMod.Server.Clients.Count > vehiculosData.Length)
            {
                networkingMod.Server.disconnectClient(clientInfo.PlayerId);
            }
            //Darla la información inicial al cliente
            else
            {
                int currentClientIndex = networkingMod.Server.Clients.Count - 1;
                VehiculoData data = vehiculosData[currentClientIndex];
                data.playerID = clientInfo.PlayerId;

                //Enviar información al cliente
                //Primero indicamos que mensaje del protocolo es
                TgcSocketSendMsg msg = new TgcSocketSendMsg();
                msg.write(MyServerProtocol.InformacionInicial);
                msg.write(data);
                //También le enviamos la información de los demás clientes hasta el momento
                //Cantidad de clientes que hay
                msg.write(networkingMod.Server.Clients.Count - 1);
                //Data de todos los clientes anteriores, salvo el ultimo que es el nuevo agregado recien
                for (int i = 0; i < networkingMod.Server.Clients.Count - 1; i++)
                {
                    msg.write(vehiculosData[i]);
                }

                networkingMod.Server.sendToClient(clientInfo.PlayerId, msg);

                //Avisar a todos los demás clientes conectados (excepto este) que hay uno nuevo
                TgcSocketSendMsg msg2 = new TgcSocketSendMsg();
                msg2.write(MyServerProtocol.OtroClienteConectado);
                msg2.write(data);
                networkingMod.Server.sendToAllExceptOne(clientInfo.PlayerId, msg2);
            }
        }

        private void atenderClienteDesconectado(TgcSocketClientInfo clientInfo)
        {
            //Enviar info de desconexion a todos los clientes
            TgcSocketSendMsg msg = new TgcSocketSendMsg();
            msg.write(MyServerProtocol.OtroClienteDesconectado);
            msg.write(clientInfo.PlayerId);
            networkingMod.Server.sendToClient(clientInfo.PlayerId, msg);


            //Extender para permitir que se conecten nuevos ususarios
        }

        /// <summary>
        /// Avisar a todos los demás clientes sobre la nueva posicion de este cliente
        /// </summary>
        private void serverAtenderPosicionActualizada(TgcSocketClientRecvMesg clientMsg)
        {
            //Nueva posicion del cliente
            Matrix newPos = (Matrix)clientMsg.Msg.readNext();

            //Enviar a todos menos al cliente que nos acaba de informar
            TgcSocketSendMsg sendMsg = new TgcSocketSendMsg();
            sendMsg.write(MyServerProtocol.ActualizarUbicaciones);
            sendMsg.write(clientMsg.PlayerId);
            sendMsg.write(newPos);
            networkingMod.Server.sendToAllExceptOne(clientMsg.PlayerId, sendMsg);
        }

        

        #endregion



        #region Cosas del Client

        const float VELODICAD_CAMINAR = 250f;
        const float VELOCIDAD_ROTACION = 120f;

        /// <summary>
        /// Tipos de mensajes que envia el cliente
        /// </summary>
        enum MyClientProtocol
        {
            PosicionActualizada,
        }

        TgcBox piso;
        TgcMesh meshPrincipal;
        Dictionary<int, TgcMesh> otrosMeshes = new Dictionary<int, TgcMesh>();

        /// <summary>
        /// Iniciar cliente
        /// </summary>
        private void initClient()
        {
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice,
                GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\rock_wall.jpg");
            piso = TgcBox.fromSize(new Vector3(0, -60, 0), new Vector3(5000, 5, 5000), pisoTexture);

            //Camara en 3ra persona
            GuiController.Instance.ThirdPersonCamera.Enable = true;
        }

        /// <summary>
        /// Actualizar lógicad el cliente
        /// </summary>
        private void updateClient()
        {
            //Analizar los mensajes recibidos
            for (int i = 0; i < networkingMod.Client.ReceivedMessagesCount; i++)
            {
                //El primer mensaje es el header de nuestro protocolo del ejemplo
                TgcSocketRecvMsg msg = networkingMod.Client.nextReceivedMessage();
                MyServerProtocol msgType = (MyServerProtocol)msg.readNext();

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
                acumulatedTime += GuiController.Instance.ElapsedTime;
                if (acumulatedTime > 0.1)
                {
                    acumulatedTime = 0;

                    //Enviar posicion al server
                    TgcSocketSendMsg msg = new TgcSocketSendMsg();
                    msg.write(MyClientProtocol.PosicionActualizada);
                    msg.write(meshPrincipal.Transform);
                    networkingMod.Client.send(msg);
                }
            }
        }

        /// <summary>
        /// Atender mensaje InformacionInicial
        /// </summary>
        private void clienteAtenderInformacionInicial(TgcSocketRecvMsg msg)
        {
            //Recibir data
            VehiculoData vehiculoData = (VehiculoData)msg.readNext();

            //Cargar mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(vehiculoData.meshPath);
            meshPrincipal = scene.Meshes[0];

            //Ubicarlo en escenario
            meshPrincipal.Position = vehiculoData.initialPos;

            //Camara
            GuiController.Instance.ThirdPersonCamera.resetValues();
            GuiController.Instance.ThirdPersonCamera.setCamera(meshPrincipal.Position, 100, 400);

            //Ver si ya habia mas clientes para cuando nosotros nos conectamos
            int otrosVehiculosCant = (int)msg.readNext();
            for (int i = 0; i < otrosVehiculosCant; i++)
            {
                VehiculoData vData = (VehiculoData)msg.readNext();
                crearMeshOtroCliente(vData);
            }
        }

        /// <summary>
        /// Renderizar toda la parte cliente, con el manejo de input
        /// </summary>
        private void renderClient()
        {
            //Calcular proxima posicion de personaje segun Input
            float elapsedTime = GuiController.Instance.ElapsedTime;
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                moveForward = -VELODICAD_CAMINAR;
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = VELODICAD_CAMINAR;
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                meshPrincipal.rotateY(Geometry.DegreeToRadian(rotate * elapsedTime));
                GuiController.Instance.ThirdPersonCamera.rotateY(rotate);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                meshPrincipal.moveOrientedY(moveForward * elapsedTime);
            }

            //Hacer que la camara siga al personaje en su nueva posicion
            GuiController.Instance.ThirdPersonCamera.Target = meshPrincipal.Position;


            //Render piso
            piso.render();

            //Renderizar meshPrincipal
            if (meshPrincipal != null)
            {
                meshPrincipal.render();
            }

            //Renderizar otrosMeshes
            foreach (KeyValuePair<int, TgcMesh> entry in otrosMeshes)
            {
                entry.Value.render();
            }
        }

        /// <summary>
        /// Crear Mesh para el nuevo cliente conectado
        /// </summary>
        private void clienteAtenderOtroClienteConectado(TgcSocketRecvMsg msg)
        {
            //Recibir data
            VehiculoData vehiculoData = (VehiculoData)msg.readNext();
            crearMeshOtroCliente(vehiculoData);
            
        }

        /// <summary>
        /// Crear Mesh para el nuevo cliente conectado
        /// </summary>
        private void crearMeshOtroCliente(VehiculoData vehiculoData)
        {
            //Cargar mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(vehiculoData.meshPath);
            TgcMesh mesh = scene.Meshes[0];
            otrosMeshes.Add(vehiculoData.playerID, mesh);

            //Ubicarlo en escenario
            mesh.AutoTransformEnable = false;
            mesh.Transform = Matrix.Translation(vehiculoData.initialPos);
        }

        /// <summary>
        /// Actualizar posicion de otro cliente
        /// </summary>
        private void clienteAtenderActualizarUbicaciones(TgcSocketRecvMsg msg)
        {
            int playerId = (int)msg.readNext();
            Matrix nextPos = (Matrix)msg.readNext();

            if (otrosMeshes.ContainsKey(playerId))
            {
                otrosMeshes[playerId].Transform = nextPos;
            }
        }

        /// <summary>
        /// Quitar otro cliente que desconecto
        /// </summary>
        private void clienteAtenderOtroClienteDesconectado(TgcSocketRecvMsg msg)
        {
            int playerId = (int)msg.readNext();
            otrosMeshes[playerId].dispose();
            otrosMeshes.Remove(playerId);
        }

        #endregion


        

    }
}
