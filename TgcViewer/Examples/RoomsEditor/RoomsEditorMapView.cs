using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TGC.Viewer;
using TGC.Viewer.Utils.Ui;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Editor de Mapa en 2D
    /// </summary>
    public partial class RoomsEditorMapView : Form
    {
        /// <summary>
        ///     Estados de la UI
        /// </summary>
        public enum EditMode
        {
            Nothing,
            CreateRoom
        }

        private const int SNAP_TO_GRID_FACTOR = 10;
        private readonly RoomsEditorTexturesEdit texturesEdit;
        private bool creatingRoomMouseDown;
        private Point creatingRoomOriginalPos;
        private EditMode currentMode;
        internal string defaultTextureDir;
        internal string defaultTextureImage;

        private RoomsEditorControl editorControl;

        private int roomsNameCounter;

        internal RoomsEditorRoom selectedRoom;

        public RoomsEditorMapView(RoomsEditorControl editorControl)
        {
            InitializeComponent();

            this.editorControl = editorControl;
            roomsNameCounter = 0;

            //textura default para los rooms
            defaultTextureDir = GuiController.Instance.ExamplesMediaDir + "Texturas\\";
            defaultTextureImage = defaultTextureDir + "tierra.jpg";
            texturesEdit = new RoomsEditorTexturesEdit(this);

            //Tamaño inicial del panel2
            panel2d.MinimumSize = new Size((int)numericUpDownMapWidth.Value, (int)numericUpDownMapHeight.Value);

            //Estado actual
            radioButtonCreateRoom.Select();
            currentMode = EditMode.CreateRoom;
            groupBoxEditRoom.Enabled = false;
        }

        /// <summary>
        ///     Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms { get; } = new List<RoomsEditorRoom>();

        /// <summary>
        ///     Escala del mapa
        /// </summary>
        public Vector3 MapScale { get; private set; }

        /// <summary>
        ///     Tamaño del mapa 2D: Width y Length
        /// </summary>
        public Size MapSize
        {
            get { return panel2d.MinimumSize; }
        }

        private void radioButtonCreateRoom_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCreateRoom.Checked)
            {
                currentMode = EditMode.CreateRoom;
            }
            else
            {
                currentMode = EditMode.Nothing;
            }
        }

        /// <summary>
        ///     Configura los parametros generales del mapa
        /// </summary>
        public void setMapSettings(Size mapSize, Vector3 mapScale)
        {
            panel2d.MinimumSize = mapSize;
            numericUpDownMapWidth.Value = mapSize.Width;
            numericUpDownMapHeight.Value = mapSize.Height;

            MapScale = mapScale;
            numericUpDownMapScaleX.Value = (decimal)mapScale.X;
            numericUpDownMapScaleY.Value = (decimal)mapScale.Y;
            numericUpDownMapScaleZ.Value = (decimal)mapScale.Z;
        }

        /// <summary>
        ///     Elimina todos los Rooms
        /// </summary>
        public void resetRooms(int roomsNameCounter)
        {
            this.roomsNameCounter = roomsNameCounter;
            panel2d.Controls.Clear();
            Rooms.Clear();
        }

        /// <summary>
        ///     Devuelve true si un rectangulo se encuentra dentro de los limites del panel2d
        /// </summary>
        private bool validateRoomBounds(Rectangle bounds)
        {
            return
                !(bounds.X < 0 || bounds.X + bounds.Width > panel2d.Bounds.Width || bounds.Y < 0 ||
                  bounds.Y + bounds.Height > panel2d.Bounds.Height);
        }

        /// <summary>
        ///     Ajustar valores a la grilla
        /// </summary>
        internal Point snapPointToGrid(int x, int y)
        {
            x = x - x % SNAP_TO_GRID_FACTOR;
            y = y - y % SNAP_TO_GRID_FACTOR;
            return new Point(x, y);
        }

        /// <summary>
        ///     Chequea colision con otros rooms.
        ///     Devuelve el room contra el cual colisiono o null
        /// </summary>
        internal RoomsEditorRoom testRoomPanelCollision(RoomsEditorRoom room, Rectangle testRect)
        {
            foreach (var r in Rooms)
            {
                if (r != room)
                {
                    if (r.RoomPanel.Label.Bounds.IntersectsWith(testRect))
                    {
                        return r;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Seleccionar un cuarto
        /// </summary>
        private void selectRoom(RoomsEditorRoom room)
        {
            if (selectedRoom != null)
            {
                selectedRoom.RoomPanel.setRoomSelected(false);
            }

            selectedRoom = room;
            selectedRoom.RoomPanel.setRoomSelected(true);
            groupBoxEditRoom.Enabled = true;

            //Cargar datos de edicion del Room
            textBoxRoomName.Text = selectedRoom.Name;
            numericUpDownRoomPosX.Value = selectedRoom.RoomPanel.Label.Location.X;
            numericUpDownRoomPosY.Value = selectedRoom.RoomPanel.Label.Location.Y;
            numericUpDownRoomWidth.Value = selectedRoom.RoomPanel.Label.Width;
            numericUpDownRoomLength.Value = selectedRoom.RoomPanel.Label.Height;
            numericUpDownRoomHeight.Value = selectedRoom.Height;
            numericUpDownRoomFloorLevel.Value = selectedRoom.FloorLevel;
        }

        /// <summary>
        ///     Clic en eliminar un room
        /// </summary>
        private void radioButtonDeleteRoom_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedRoom != null)
            {
                deleteRoom(selectedRoom);
            }
        }

        /// <summary>
        ///     Eliminar un room
        /// </summary>
        internal void deleteRoom(RoomsEditorRoom room)
        {
            Rooms.Remove(room);
            panel2d.Controls.Remove(room.RoomPanel.Label);
        }

        private void panel2d_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        private void panel2d_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        ///     Width escenario
        /// </summary>
        private void numericUpDownMapWidth_ValueChanged(object sender, EventArgs e)
        {
            //validar minimo Width de panel2d
            var newWidth = (int)numericUpDownMapWidth.Value;
            if (newWidth < panel2d.MinimumSize.Width)
            {
                //buscar maximo X de todos los rooms
                var maxX = -1;
                foreach (var room in Rooms)
                {
                    var x = room.RoomPanel.Label.Location.X + room.RoomPanel.Label.Width;
                    if (x > maxX)
                    {
                        maxX = x;
                    }
                }

                //ver si el nuevo tamaño de panel2d queda chico
                if (newWidth < maxX)
                {
                    MessageBox.Show(this, "The new map Width is not big enought to contains all the existing rooms.",
                        "Map size", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Actualizar tamaño
            panel2d.MinimumSize = new Size(newWidth, panel2d.MinimumSize.Height);
        }

        /// <summary>
        ///     Height escenario
        /// </summary>
        private void numericUpDownMapHeight_ValueChanged(object sender, EventArgs e)
        {
            //validar minimo Height de panel2d
            var newHeight = (int)numericUpDownMapHeight.Value;
            if (newHeight < panel2d.MinimumSize.Height)
            {
                //buscar maximo Y de todos los rooms
                var maxY = -1;
                foreach (var room in Rooms)
                {
                    var y = room.RoomPanel.Label.Location.Y + room.RoomPanel.Label.Height;
                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }

                //ver si el nuevo tamaño de panel2d queda chico
                if (newHeight < maxY)
                {
                    MessageBox.Show(this, "The new map Height is not big enought to contains all the existing rooms.",
                        "Map size", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Actualizar tamaño
            panel2d.MinimumSize = new Size(panel2d.MinimumSize.Width, newHeight);
        }

        /// <summary>
        ///     Popup de Wall textures
        /// </summary>
        private void buttonWallTextures_Click(object sender, EventArgs e)
        {
            texturesEdit.fillRoomData(selectedRoom);
            texturesEdit.ShowDialog(this);
        }

        /// <summary>
        ///     Click para generar escenario 3D
        /// </summary>
        private void buttonCreate3dMap_Click(object sender, EventArgs e)
        {
            update3dMap();
            Close();
        }

        /// <summary>
        ///     Generar escenario 3D en base a la información del mapa
        /// </summary>
        public void update3dMap()
        {
            //chequear superposicion de rooms
            var sb = new StringBuilder("There are collisions between the following Rooms: ");
            var totalCol = 0;
            foreach (var room in Rooms)
            {
                var collRoom = testRoomPanelCollision(room, room.RoomPanel.Label.Bounds);
                if (collRoom != null)
                {
                    sb.AppendLine(room.Name + " and " + collRoom.Name + "\n");
                    totalCol++;
                }
            }
            if (totalCol > 0)
            {
                MessageBox.Show(this, sb.ToString(), "Collisions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Escala del mapa
            MapScale = new Vector3((float)numericUpDownMapScaleX.Value, (float)numericUpDownMapScaleY.Value,
                (float)numericUpDownMapScaleZ.Value);

            //Construir rooms en 3D
            foreach (var room in Rooms)
            {
                room.buildWalls(Rooms, MapScale);
            }
        }

        #region RoomPanel

        /// <summary>
        ///     Panel 2D que representa un Room
        /// </summary>
        public class RoomPanel
        {
            private readonly Color DEFAULT_ROOM_COLOR = Color.Orange;
            private readonly Size MINIUM_SIZE = new Size(50, 50);
            private readonly Color SELECTED_ROOM_COLOR = Color.BlueViolet;
            private Point initDragP;

            private bool labelDragging;

            internal RoomsEditorMapView mapView;
            private Point oldLocation;

            public RoomPanel(string name, RoomsEditorMapView mapView, Point location, Size size)
            {
                this.mapView = mapView;

                Label = new Label();
                Label.Text = name;
                Label.AutoSize = false;
                Label.Location = location;
                Label.Size = size;
                Label.BorderStyle = BorderStyle.FixedSingle;
                Label.BackColor = DEFAULT_ROOM_COLOR;
                Label.TextAlign = ContentAlignment.MiddleCenter;

                Label.MouseDown += label_MouseDown;
                Label.MouseUp += label_MouseUp;
                Label.MouseMove += label_MouseMove;
                Label.MouseEnter += label_MouseEnter;
                Label.PreviewKeyDown += label_PreviewKeyDown;
            }

            public RoomsEditorRoom Room { get; set; }

            public Label Label { get; }

            /// <summary>
            ///     Adapta al tamaño del label al minimo permitido
            /// </summary>
            internal void adaptaToMiniumSize()
            {
                if (Label.Width < MINIUM_SIZE.Width)
                {
                    Label.Width = MINIUM_SIZE.Width;
                }
                if (Label.Height < MINIUM_SIZE.Height)
                {
                    Label.Height = MINIUM_SIZE.Height;
                }
            }

            /// <summary>
            ///     Seleccionar el cuarto
            /// </summary>
            internal void setRoomSelected(bool selected)
            {
                if (selected)
                {
                    Label.BackColor = SELECTED_ROOM_COLOR;
                    Label.Focus();
                }
                else
                {
                    Label.BackColor = DEFAULT_ROOM_COLOR;
                }
            }

            private void label_MouseDown(object sender, MouseEventArgs e)
            {
                //Se quiere hacer drag del label
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    //seleccionar este cuarto como el actual
                    mapView.selectRoom(Room);

                    labelDragging = true;
                    initDragP = mapView.snapPointToGrid(e.X, e.Y);
                    oldLocation = Label.Location;
                }
            }

            private void label_MouseUp(object sender, MouseEventArgs e)
            {
                //Se termino de hacer drag del label, validar posicion
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    labelDragging = false;

                    //Si hay colision con otros labels, restaurar la posicion original
                    var possibleR = new Rectangle(Label.Location, Label.Size);
                    var collisionRoom = mapView.testRoomPanelCollision(Room, possibleR);
                    if (collisionRoom != null)
                    {
                        Label.Location = oldLocation;
                    }
                }
            }

            private void label_MouseMove(object sender, MouseEventArgs e)
            {
                //Haciendo drag de label, actualizar posicion
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    if (labelDragging)
                    {
                        var x = Label.Location.X - (initDragP.X - e.X);
                        var y = Label.Location.Y - (initDragP.Y - e.Y);
                        var newP = mapView.snapPointToGrid(x, y);

                        //Actualizar posicion solo si no se escapa de los limites del escenario
                        var newRect = new Rectangle(newP, Label.Size);
                        if (mapView.validateRoomBounds(newRect))
                        {
                            Label.Location = newP;
                        }
                    }
                }
            }

            private void label_MouseEnter(object sender, EventArgs e)
            {
                mapView.Cursor = Cursors.SizeAll;
            }

            private void label_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                //Eliminar room
                if (e.KeyCode == Keys.Delete)
                {
                    mapView.selectedRoom = null;
                    mapView.deleteRoom(Room);
                }
            }

            /// <summary>
            ///     Cambia el nombre del Room
            /// </summary>
            internal void changeName(string newName)
            {
                Room.Name = newName;
                Label.Text = newName;
            }

            /// <summary>
            ///     Actualiza el Bounds del label del Room validando limites y colisiones.
            ///     Informa si se pude hacer
            /// </summary>
            internal bool updateLabelBounds(Rectangle newR)
            {
                //Validar limites del escenario
                if (!mapView.validateRoomBounds(newR))
                {
                    return false;
                }

                //Validar colision con otros Rooms
                var collisionRoom = mapView.testRoomPanelCollision(Room, newR);
                if (collisionRoom != null)
                {
                    return false;
                }

                Label.Bounds = newR;
                return true;
            }
        }

        #endregion RoomPanel

        #region Create Room

        private void panel2d_MouseDown(object sender, MouseEventArgs e)
        {
            //Crear Room
            if (currentMode == EditMode.CreateRoom)
            {
                creatingRoomMouseDown = true;
                creatingRoomOriginalPos = snapPointToGrid(e.X, e.Y);
                var name = "Room-" + ++roomsNameCounter;
                createRoom(name, creatingRoomOriginalPos, new Size(1, 1));
            }
        }

        /// <summary>
        ///     Crea un nuevo Room y lo agrega al panel2D
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        public RoomsEditorRoom createRoom(string name, Point pos, Size size)
        {
            var rPanel = new RoomPanel(name, this, pos, size);
            var room = new RoomsEditorRoom(name, rPanel);
            Rooms.Add(room);

            panel2d.Controls.Add(room.RoomPanel.Label);

            return room;
        }

        private void panel2d_MouseMove(object sender, MouseEventArgs e)
        {
            //Modificar tamaño cuando se esta creando un Room
            if (currentMode == EditMode.CreateRoom)
            {
                if (creatingRoomMouseDown)
                {
                    var mPoint = snapPointToGrid(e.X, e.Y);
                    var diffX = mPoint.X - creatingRoomOriginalPos.X;
                    var diffY = mPoint.Y - creatingRoomOriginalPos.Y;

                    //dibujar rectangulo en base a las coordenadas elegidas
                    Rectangle rect;
                    if (diffX > 0)
                    {
                        if (diffY > 0)
                        {
                            rect = new Rectangle(creatingRoomOriginalPos.X, creatingRoomOriginalPos.Y, diffX, diffY);
                        }
                        else
                        {
                            rect = new Rectangle(creatingRoomOriginalPos.X, creatingRoomOriginalPos.Y + diffY, diffX,
                                -diffY);
                        }
                    }
                    else
                    {
                        if (diffY > 0)
                        {
                            rect = new Rectangle(mPoint.X, creatingRoomOriginalPos.Y, -diffX, diffY);
                        }
                        else
                        {
                            rect = new Rectangle(mPoint.X, mPoint.Y, -diffX, -diffY);
                        }
                    }

                    //Actualizar tamaño solo si no se escapa de los limites del escenario
                    if (validateRoomBounds(rect))
                    {
                        var lastRoom = Rooms[Rooms.Count - 1];
                        lastRoom.RoomPanel.Label.Bounds = rect;
                    }
                }
            }
        }

        private void panel2d_MouseUp(object sender, MouseEventArgs e)
        {
            //Termina la creación de un nuevo Room
            if (currentMode == EditMode.CreateRoom)
            {
                creatingRoomMouseDown = false;
                var lastRoom = Rooms[Rooms.Count - 1];
                lastRoom.RoomPanel.adaptaToMiniumSize();

                //Si colisiona no lo borramos
                var collisionRoom = testRoomPanelCollision(lastRoom, lastRoom.RoomPanel.Label.Bounds);
                if (collisionRoom != null)
                {
                    Rooms.Remove(lastRoom);
                    panel2d.Controls.Remove(lastRoom.RoomPanel.Label);
                }

                //Seleccionar room recien creado
                selectRoom(lastRoom);
            }
        }

        #endregion Create Room

        #region Edit Room

        /// <summary>
        ///     Cambiar nombre de room
        /// </summary>
        private void textBoxRoomName_TextChanged(object sender, EventArgs e)
        {
            if (ValidationUtils.validateRequired(textBoxRoomName.Text))
            {
                //controlar repetidos
                foreach (var r in Rooms)
                {
                    if (r != selectedRoom && r.Name == textBoxRoomName.Text)
                    {
                        MessageBox.Show(this, "Ya existe un cuarto con ese nombre: " + textBoxRoomName.Text,
                            "Edición de nombre", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBoxRoomName.Text = selectedRoom.Name;
                        return;
                    }
                }
                selectedRoom.RoomPanel.changeName(textBoxRoomName.Text);
            }
            else
            {
                textBoxRoomName.Text = selectedRoom.Name;
            }
        }

        /// <summary>
        ///     Cambiar X de room
        /// </summary>
        private void numericUpDownRoomPosX_ValueChanged(object sender, EventArgs e)
        {
            var currentR = selectedRoom.RoomPanel.Label.Bounds;
            var newR = new Rectangle((int)numericUpDownRoomPosX.Value, currentR.Y, currentR.Width, currentR.Height);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomPosX.Value = currentR.X;
            }
        }

        /// <summary>
        ///     Cambiar Y de room
        /// </summary>
        private void numericUpDownRoomPosY_ValueChanged(object sender, EventArgs e)
        {
            var currentR = selectedRoom.RoomPanel.Label.Bounds;
            var newR = new Rectangle(currentR.X, (int)numericUpDownRoomPosY.Value, currentR.Width, currentR.Height);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomPosY.Value = currentR.Y;
            }
        }

        /// <summary>
        ///     Cambiar Width de room
        /// </summary>
        private void numericUpDownRoomWidth_ValueChanged(object sender, EventArgs e)
        {
            var currentR = selectedRoom.RoomPanel.Label.Bounds;
            var newR = new Rectangle(currentR.X, currentR.Y, (int)numericUpDownRoomWidth.Value, currentR.Height);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomWidth.Value = currentR.Width;
            }
        }

        /// <summary>
        ///     Cambiar Length de room
        /// </summary>
        private void numericUpDownRoomLength_ValueChanged(object sender, EventArgs e)
        {
            var currentR = selectedRoom.RoomPanel.Label.Bounds;
            var newR = new Rectangle(currentR.X, currentR.Y, currentR.Width, (int)numericUpDownRoomLength.Value);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomLength.Value = currentR.Height;
            }
        }

        /// <summary>
        ///     Variar altura de las paredes de un room
        /// </summary>
        private void numericUpDownRoomHeight_ValueChanged(object sender, EventArgs e)
        {
            var newHeight = (int)numericUpDownRoomHeight.Value;
            selectedRoom.Height = newHeight;
        }

        /// <summary>
        ///     Variar nivel del piso de un room
        /// </summary>
        private void numericUpDownRoomFloorLevel_ValueChanged(object sender, EventArgs e)
        {
            var newFloorLevel = (int)numericUpDownRoomFloorLevel.Value;
            selectedRoom.FloorLevel = newFloorLevel;
        }

        #endregion Edit Room
    }
}