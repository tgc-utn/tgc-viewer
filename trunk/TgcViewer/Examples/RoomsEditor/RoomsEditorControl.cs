using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using System.IO;
using TgcViewer;
using System.Xml;
using Microsoft.DirectX;

namespace Examples.RoomsEditor
{
    /// <summary>
    /// Control gráfico principal del Modifier TgcRoomsEditor
    /// </summary>
    public partial class RoomsEditorControl : UserControl
    {

        const string DEFAULT_TEXTURES_DIR = "Textures";


        TgcRoomsEditor editor;
        RoomsEditorMapView mapView;
        SaveFileDialog saveDialog;
        OpenFileDialog openMapDialog;
        RoomsEditorHelpWindow helpWindow;

        /// <summary>
        /// Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms
        {
            get { return mapView.Rooms; }
        }

        public RoomsEditorControl(TgcRoomsEditor editor)
        {
            InitializeComponent();

            this.editor = editor;
            this.mapView = new RoomsEditorMapView(this);
            this.helpWindow = new RoomsEditorHelpWindow();

            //openMapDialog
            openMapDialog = new OpenFileDialog();
            openMapDialog.CheckFileExists = true;
            openMapDialog.Title = "Select a Map file";
            openMapDialog.Filter = ".XML |*.xml";
            openMapDialog.Multiselect = false;
            openMapDialog.InitialDirectory = GuiController.Instance.ExamplesMediaDir;

            //saveDialog
            saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = ".xml";
            saveDialog.Filter = ".XML |*.xml";
            saveDialog.AddExtension = true;
        }

        /// <summary>
        /// Editar mapa 2D
        /// </summary>
        private void buttonEdit2dMap_Click(object sender, EventArgs e)
        {
            mapView.ShowDialog(this);
        }

        private void buttonOpenMap_Click(object sender, EventArgs e)
        {
            if (openMapDialog.ShowDialog(this) == DialogResult.OK)
            {
                openMap(openMapDialog.FileName);
            }
        }
        
        private void buttonSaveMap_Click(object sender, EventArgs e)
        {
            saveDialog.Title = "Save Map";
            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileInfo fInfo = new FileInfo(saveDialog.FileName);
                saveMap(fInfo.DirectoryName, saveDialog.FileName);
            }
        }

        /// <summary>
        /// Graba todo el mapa a un XML propio del Modifier para que luego el escenario pueda
        /// ser restaurado y seguir trabajando.
        /// Copia todas las texturas utilizadas a una subcarpeta relativa al directorio donde se guarda
        /// </summary>
        private void saveMap(string saveDir, string savePath)
        {
            //Crear directorio de texturas, respetar si ya existe porque puede ser la fuente de las texturas de ahora
            string texturesDir = saveDir + "\\" + DEFAULT_TEXTURES_DIR;
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }
            

            //Crear XML
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("roomsEditor-Map");

            //mapSettings
            XmlElement mapSettingsNode = doc.CreateElement("mapSettings");
            mapSettingsNode.SetAttribute("width", mapView.MapSize.Width.ToString());
            mapSettingsNode.SetAttribute("height", mapView.MapSize.Height.ToString());
            mapSettingsNode.SetAttribute("scaleX", TgcParserUtils.printFloat(mapView.MapScale.X));
            mapSettingsNode.SetAttribute("scaleY", TgcParserUtils.printFloat(mapView.MapScale.Y));
            mapSettingsNode.SetAttribute("scaleZ", TgcParserUtils.printFloat(mapView.MapScale.Z));
            root.AppendChild(mapSettingsNode);

            //rooms
            XmlElement roomsNode = doc.CreateElement("rooms");
            roomsNode.SetAttribute("count", Rooms.Count.ToString());
            foreach (RoomsEditorRoom room in Rooms)
            {
                //room
                XmlElement roomNode = doc.CreateElement("room");
                roomNode.SetAttribute("name", room.Name);
                roomNode.SetAttribute("x", room.RoomPanel.Label.Location.X.ToString());
                roomNode.SetAttribute("y", room.RoomPanel.Label.Location.Y.ToString());
                roomNode.SetAttribute("width", room.RoomPanel.Label.Width.ToString());
                roomNode.SetAttribute("length", room.RoomPanel.Label.Height.ToString());
                roomNode.SetAttribute("height", room.Height.ToString());
                roomNode.SetAttribute("floorLevel", room.FloorLevel.ToString());

                foreach (RoomsEditorWall wall in room.Walls)
                {
                    //wall
                    XmlElement wallNode = doc.CreateElement("wall");
                    wallNode.SetAttribute("name", wall.Name);
                    wallNode.SetAttribute("textureName", wall.Texture.FileName);
                    wallNode.SetAttribute("autoAdjustUv", wall.AutoAdjustUv.ToString());
                    wallNode.SetAttribute("uTile", wall.UTile.ToString());
                    wallNode.SetAttribute("vTile", wall.VTile.ToString());

                    roomNode.AppendChild(wallNode);

                    //copiar textura, respetar si ya existe porque puede ser la fuente actual
                    string textDest = texturesDir + "\\" + wall.Texture.FileName;
                    if (!File.Exists(textDest))
                    {
                        File.Copy(wall.Texture.FilePath, textDest, true);
                    }
                    
                }

                roomsNode.AppendChild(roomNode);
            }
            root.AppendChild(roomsNode);

            //Guardar XML
            doc.AppendChild(root);
            doc.Save(savePath);

            MessageBox.Show(this, "Map saved OK", "Save Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Carga un archivo XML con los datos de una mapa guardado anteriormente
        /// </summary>
        private void openMap(string filePath)
        {
            try
            {
                FileInfo fInfo = new FileInfo(filePath);
                string directory = fInfo.DirectoryName;
                string texturesDir = directory + "\\" + DEFAULT_TEXTURES_DIR;

                XmlDocument dom = new XmlDocument();
                string xmlString = File.ReadAllText(filePath);
                dom.LoadXml(xmlString);
                XmlElement root = dom.DocumentElement;

                //mapSettings
                XmlNode mapSettingsNode = root.GetElementsByTagName("mapSettings")[0];
                Size mapSize = new Size();
                mapSize.Width = TgcParserUtils.parseInt(mapSettingsNode.Attributes["width"].InnerText);
                mapSize.Height = TgcParserUtils.parseInt(mapSettingsNode.Attributes["height"].InnerText);

                Vector3 mapScale = new Vector3();
                mapScale.X = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleX"].InnerText);
                mapScale.Y = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleY"].InnerText);
                mapScale.Z = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleZ"].InnerText);

                mapView.setMapSettings(mapSize, mapScale);


                //rooms
                XmlNode roomsNode = root.GetElementsByTagName("rooms")[0];
                mapView.resetRooms(roomsNode.ChildNodes.Count * 2);
                foreach (XmlNode roomNode in roomsNode.ChildNodes)
                {
                    string roomName = roomNode.Attributes["name"].InnerText;
                    Point pos = new Point();
                    pos.X = TgcParserUtils.parseInt(roomNode.Attributes["x"].InnerText);
                    pos.Y = TgcParserUtils.parseInt(roomNode.Attributes["y"].InnerText);
                    Size size = new Size();
                    size.Width = TgcParserUtils.parseInt(roomNode.Attributes["width"].InnerText);
                    size.Height = TgcParserUtils.parseInt(roomNode.Attributes["length"].InnerText);
                    int roomHeight = TgcParserUtils.parseInt(roomNode.Attributes["height"].InnerText);
                    int roomFloorLevel = TgcParserUtils.parseInt(roomNode.Attributes["floorLevel"].InnerText);

                    RoomsEditorRoom room = mapView.createRoom(roomName, pos, size);
                    room.Height = roomHeight;
                    room.FloorLevel = roomFloorLevel;

                    //walls
                    int wIdx = 0;
                    foreach (XmlNode wallNode in roomNode.ChildNodes)
	                {
                        string wallName = wallNode.Attributes["name"].InnerText;
                        string textureName = wallNode.Attributes["textureName"].InnerText;
                        bool autoAdjustUv = bool.Parse(wallNode.Attributes["autoAdjustUv"].InnerText);
                        float uTile = TgcParserUtils.parseFloat(wallNode.Attributes["uTile"].InnerText);
                        float vTile = TgcParserUtils.parseFloat(wallNode.Attributes["vTile"].InnerText);

                        RoomsEditorWall wall = room.Walls[wIdx++];
                        if (wall.Texture != null)
                        {
                            wall.Texture.dispose();
                        }
                        wall.Texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, texturesDir + "\\" + textureName);
                        wall.AutoAdjustUv = autoAdjustUv;
                        wall.UTile = uTile;
                        wall.VTile = vTile;
	                } 
                }

                //Crear escenario 3D
                mapView.update3dMap();


                MessageBox.Show(this, "Map openned OK", "Open Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                GuiController.Instance.Logger.logError("Cannot open RoomsEditor Map file", ex);
            }
            
        }

        
        private void buttonExportScene_Click(object sender, EventArgs e)
        {
            saveDialog.Title = "Export Scene";
            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileInfo fInfo = new FileInfo(saveDialog.FileName);
                string name = fInfo.Name.Split('.')[0];
                int n = name.IndexOf("-TgcScene");
                if (n >= 0)
                {
                    name = name.Substring(0, n);
                }
                exportScene(fInfo.DirectoryName, name);
            }
        }

        /// <summary>
        /// Exporta todos los cuartos y paredes a un xml de TgcScene
        /// </summary>
        private void exportScene(string saveDir, string sceneName)
        {
            TgcScene scene = new TgcScene(sceneName, saveDir);
            foreach (RoomsEditorRoom room in Rooms)
            {
                foreach (RoomsEditorWall wall in room.Walls)
                {
                    int wallSegId = 0;
                    foreach (TgcPlaneWall wall3d in wall.WallSegments)
                    {
                        scene.Meshes.Add(wall3d.toMesh(room.Name + "-" + wall.Name + "-" + wallSegId));
                        wallSegId++;
                    }  
                }
            }

            TgcSceneExporter exporter = new TgcSceneExporter();
            exporter.exportSceneToXml(scene, saveDir);

            MessageBox.Show(this, "Scene export OK", "Export Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        /// <summary>
        /// Exportación customizada
        /// </summary>
        private void buttonCustomExport_Click(object sender, EventArgs e)
        {
            saveDialog.Title = "Elegir destino para Custom Export";
            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                editor.customExport(saveDialog.FileName);
            }
        }

        /// <summary>
        /// Mostrar ventana de ayuda
        /// </summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            this.helpWindow.ShowDialog(this);
        }

        
    }
}
