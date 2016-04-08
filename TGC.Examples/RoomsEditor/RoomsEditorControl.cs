using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Viewer;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Control gráfico principal del Modifier TgcRoomsEditor
    /// </summary>
    public partial class RoomsEditorControl : UserControl
    {
        private const string DEFAULT_TEXTURES_DIR = "Textures";

        private readonly TgcRoomsEditor editor;
        private readonly RoomsEditorHelpWindow helpWindow;
        private readonly RoomsEditorMapView mapView;
        private readonly OpenFileDialog openMapDialog;
        private readonly SaveFileDialog saveDialog;

        public RoomsEditorControl(TgcRoomsEditor editor)
        {
            InitializeComponent();

            this.editor = editor;
            mapView = new RoomsEditorMapView(this);
            helpWindow = new RoomsEditorHelpWindow();

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
        ///     Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms
        {
            get { return mapView.Rooms; }
        }

        /// <summary>
        ///     Editar mapa 2D
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
                var fInfo = new FileInfo(saveDialog.FileName);
                saveMap(fInfo.DirectoryName, saveDialog.FileName);
            }
        }

        /// <summary>
        ///     Graba todo el mapa a un XML propio del Modifier para que luego el escenario pueda
        ///     ser restaurado y seguir trabajando.
        ///     Copia todas las texturas utilizadas a una subcarpeta relativa al directorio donde se guarda
        /// </summary>
        private void saveMap(string saveDir, string savePath)
        {
            //Crear directorio de texturas, respetar si ya existe porque puede ser la fuente de las texturas de ahora
            var texturesDir = saveDir + "\\" + DEFAULT_TEXTURES_DIR;
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }

            //Crear XML
            var doc = new XmlDocument();
            XmlNode root = doc.CreateElement("roomsEditor-Map");

            //mapSettings
            var mapSettingsNode = doc.CreateElement("mapSettings");
            mapSettingsNode.SetAttribute("width", mapView.MapSize.Width.ToString());
            mapSettingsNode.SetAttribute("height", mapView.MapSize.Height.ToString());
            mapSettingsNode.SetAttribute("scaleX", TgcParserUtils.printFloat(mapView.MapScale.X));
            mapSettingsNode.SetAttribute("scaleY", TgcParserUtils.printFloat(mapView.MapScale.Y));
            mapSettingsNode.SetAttribute("scaleZ", TgcParserUtils.printFloat(mapView.MapScale.Z));
            root.AppendChild(mapSettingsNode);

            //rooms
            var roomsNode = doc.CreateElement("rooms");
            roomsNode.SetAttribute("count", Rooms.Count.ToString());
            foreach (var room in Rooms)
            {
                //room
                var roomNode = doc.CreateElement("room");
                roomNode.SetAttribute("name", room.Name);
                roomNode.SetAttribute("x", room.RoomPanel.Label.Location.X.ToString());
                roomNode.SetAttribute("y", room.RoomPanel.Label.Location.Y.ToString());
                roomNode.SetAttribute("width", room.RoomPanel.Label.Width.ToString());
                roomNode.SetAttribute("length", room.RoomPanel.Label.Height.ToString());
                roomNode.SetAttribute("height", room.Height.ToString());
                roomNode.SetAttribute("floorLevel", room.FloorLevel.ToString());

                foreach (var wall in room.Walls)
                {
                    //wall
                    var wallNode = doc.CreateElement("wall");
                    wallNode.SetAttribute("name", wall.Name);
                    wallNode.SetAttribute("textureName", wall.Texture.FileName);
                    wallNode.SetAttribute("autoAdjustUv", wall.AutoAdjustUv.ToString());
                    wallNode.SetAttribute("uTile", wall.UTile.ToString());
                    wallNode.SetAttribute("vTile", wall.VTile.ToString());

                    roomNode.AppendChild(wallNode);

                    //copiar textura, respetar si ya existe porque puede ser la fuente actual
                    var textDest = texturesDir + "\\" + wall.Texture.FileName;
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
        ///     Carga un archivo XML con los datos de una mapa guardado anteriormente
        /// </summary>
        private void openMap(string filePath)
        {
            try
            {
                var fInfo = new FileInfo(filePath);
                var directory = fInfo.DirectoryName;
                var texturesDir = directory + "\\" + DEFAULT_TEXTURES_DIR;

                var dom = new XmlDocument();
                var xmlString = File.ReadAllText(filePath);
                dom.LoadXml(xmlString);
                var root = dom.DocumentElement;

                //mapSettings
                var mapSettingsNode = root.GetElementsByTagName("mapSettings")[0];
                var mapSize = new Size();
                mapSize.Width = TgcParserUtils.parseInt(mapSettingsNode.Attributes["width"].InnerText);
                mapSize.Height = TgcParserUtils.parseInt(mapSettingsNode.Attributes["height"].InnerText);

                var mapScale = new Vector3();
                mapScale.X = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleX"].InnerText);
                mapScale.Y = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleY"].InnerText);
                mapScale.Z = TgcParserUtils.parseFloat(mapSettingsNode.Attributes["scaleZ"].InnerText);

                mapView.setMapSettings(mapSize, mapScale);

                //rooms
                var roomsNode = root.GetElementsByTagName("rooms")[0];
                mapView.resetRooms(roomsNode.ChildNodes.Count * 2);
                foreach (XmlNode roomNode in roomsNode.ChildNodes)
                {
                    var roomName = roomNode.Attributes["name"].InnerText;
                    var pos = new Point();
                    pos.X = TgcParserUtils.parseInt(roomNode.Attributes["x"].InnerText);
                    pos.Y = TgcParserUtils.parseInt(roomNode.Attributes["y"].InnerText);
                    var size = new Size();
                    size.Width = TgcParserUtils.parseInt(roomNode.Attributes["width"].InnerText);
                    size.Height = TgcParserUtils.parseInt(roomNode.Attributes["length"].InnerText);
                    var roomHeight = TgcParserUtils.parseInt(roomNode.Attributes["height"].InnerText);
                    var roomFloorLevel = TgcParserUtils.parseInt(roomNode.Attributes["floorLevel"].InnerText);

                    var room = mapView.createRoom(roomName, pos, size);
                    room.Height = roomHeight;
                    room.FloorLevel = roomFloorLevel;

                    //walls
                    var wIdx = 0;
                    foreach (XmlNode wallNode in roomNode.ChildNodes)
                    {
                        var wallName = wallNode.Attributes["name"].InnerText;
                        var textureName = wallNode.Attributes["textureName"].InnerText;
                        var autoAdjustUv = bool.Parse(wallNode.Attributes["autoAdjustUv"].InnerText);
                        var uTile = TgcParserUtils.parseFloat(wallNode.Attributes["uTile"].InnerText);
                        var vTile = TgcParserUtils.parseFloat(wallNode.Attributes["vTile"].InnerText);

                        var wall = room.Walls[wIdx++];
                        if (wall.Texture != null)
                        {
                            wall.Texture.dispose();
                        }
                        wall.Texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                            texturesDir + "\\" + textureName);
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
                var fInfo = new FileInfo(saveDialog.FileName);
                var name = fInfo.Name.Split('.')[0];
                var n = name.IndexOf("-TgcScene");
                if (n >= 0)
                {
                    name = name.Substring(0, n);
                }
                exportScene(fInfo.DirectoryName, name);
            }
        }

        /// <summary>
        ///     Exporta todos los cuartos y paredes a un xml de TgcScene
        /// </summary>
        private void exportScene(string saveDir, string sceneName)
        {
            var scene = new TgcScene(sceneName, saveDir);
            foreach (var room in Rooms)
            {
                foreach (var wall in room.Walls)
                {
                    var wallSegId = 0;
                    foreach (var wall3d in wall.WallSegments)
                    {
                        scene.Meshes.Add(wall3d.toMesh(room.Name + "-" + wall.Name + "-" + wallSegId));
                        wallSegId++;
                    }
                }
            }

            var exporter = new TgcSceneExporter();
            exporter.exportSceneToXml(scene, saveDir);

            MessageBox.Show(this, "Scene export OK", "Export Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        ///     Exportación customizada
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
        ///     Mostrar ventana de ayuda
        /// </summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            helpWindow.ShowDialog(this);
        }
    }
}