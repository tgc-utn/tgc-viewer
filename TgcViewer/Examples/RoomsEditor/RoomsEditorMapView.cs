using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using SistPaquetesClient.core;

namespace Examples.RoomsEditor
{
    /// <summary>
    /// Editor de Mapa en 2D
    /// </summary>
    public partial class RoomsEditorMapView : Form
    {
        const int SNAP_TO_GRID_FACTOR = 10;
        

        RoomsEditorControl editorControl;
        EditMode currentMode;
        bool creatingRoomMouseDown = false;
        Point creatingRoomOriginalPos;
        int roomsNameCounter;
        RoomsEditorTexturesEdit texturesEdit;

        internal RoomsEditorRoom selectedRoom;
        internal string defaultTextureDir;
        internal string defaultTextureImage;


        List<RoomsEditorRoom> rooms = new List<RoomsEditorRoom>();
        /// <summary>
        /// Rooms creados
        /// </summary>
        public List<RoomsEditorRoom> Rooms
        {
            get { return rooms; }
        }

        Vector3 mapScale;
        /// <summary>
        /// Escala del mapa
        /// </summary>
        public Vector3 MapScale
        {
            get { return mapScale; }
        }

        /// <summary>
        /// Tamaño del mapa 2D: Width y Length
        /// </summary>
        public Size MapSize
        {
            get { return panel2d.MinimumSize; }
        }



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
        /// Estados de la UI
        /// </summary>
        public enum EditMode
        {
            Nothing,
            CreateRoom,
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
        /// Configura los parametros generales del mapa
        /// </summary>
        public void setMapSettings(Size mapSize, Vector3 mapScale)
        {
            this.panel2d.MinimumSize = mapSize;
            this.numericUpDownMapWidth.Value = (decimal)mapSize.Width;
            this.numericUpDownMapHeight.Value = (decimal)mapSize.Height;

            this.mapScale = mapScale;
            this.numericUpDownMapScaleX.Value = (decimal)mapScale.X;
            this.numericUpDownMapScaleY.Value = (decimal)mapScale.Y;
            this.numericUpDownMapScaleZ.Value = (decimal)mapScale.Z;
        }

        /// <summary>
        /// Elimina todos los Rooms
        /// </summary>
        public void resetRooms(int roomsNameCounter)
        {
            this.roomsNameCounter = roomsNameCounter;
            this.panel2d.Controls.Clear();
            this.rooms.Clear();
        }
        
        

        /// <summary>
        /// Devuelve true si un rectangulo se encuentra dentro de los limites del panel2d
        /// </summary>
        private bool validateRoomBounds(Rectangle bounds)
        {
            return !(bounds.X < 0 || bounds.X + bounds.Width > panel2d.Bounds.Width || bounds.Y < 0 || bounds.Y + bounds.Height > panel2d.Bounds.Height);
        }

        

        /// <summary>
        /// Ajustar valores a la grilla
        /// </summary>
        internal Point snapPointToGrid(int x, int y)
        {
            x = x - x % SNAP_TO_GRID_FACTOR;
            y = y - y % SNAP_TO_GRID_FACTOR;
            return new Point(x, y);
        }

        /// <summary>
        /// Chequea colision con otros rooms.
        /// Devuelve el room contra el cual colisiono o null
        /// </summary>
        internal RoomsEditorRoom testRoomPanelCollision(RoomsEditorRoom room, Rectangle testRect)
        {
            foreach (RoomsEditorRoom r in rooms)
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
        /// Seleccionar un cuarto
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
        /// Clic en eliminar un room
        /// </summary>
        private void radioButtonDeleteRoom_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedRoom != null)
            {
                deleteRoom(selectedRoom);
            }
        }

        /// <summary>
        /// Eliminar un room
        /// </summary>
        internal void deleteRoom(RoomsEditorRoom room)
        {
            this.rooms.Remove(room);
            this.panel2d.Controls.Remove(room.RoomPanel.Label);
        }

        private void panel2d_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }

        private void panel2d_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Width escenario
        /// </summary>
        private void numericUpDownMapWidth_ValueChanged(object sender, EventArgs e)
        {
            //validar minimo Width de panel2d
            int newWidth = (int)numericUpDownMapWidth.Value;
            if (newWidth < panel2d.MinimumSize.Width)
            {
                //buscar maximo X de todos los rooms
                int maxX = -1;
                foreach (RoomsEditorRoom room in rooms)
                {
                    int x = room.RoomPanel.Label.Location.X + room.RoomPanel.Label.Width;
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
        /// Height escenario
        /// </summary>
        private void numericUpDownMapHeight_ValueChanged(object sender, EventArgs e)
        {
            //validar minimo Height de panel2d
            int newHeight = (int)numericUpDownMapHeight.Value;
            if (newHeight < panel2d.MinimumSize.Height)
            {
                //buscar maximo Y de todos los rooms
                int maxY = -1;
                foreach (RoomsEditorRoom room in rooms)
                {
                    int y = room.RoomPanel.Label.Location.Y + room.RoomPanel.Label.Height;
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
        /// Popup de Wall textures
        /// </summary>
        private void buttonWallTextures_Click(object sender, EventArgs e)
        {
            texturesEdit.fillRoomData(selectedRoom);
            texturesEdit.ShowDialog(this);
        }

        /// <summary>
        /// Click para generar escenario 3D
        /// </summary>
        private void buttonCreate3dMap_Click(object sender, EventArgs e)
        {
            update3dMap();
            this.Close();
        }

        /// <summary>
        /// Generar escenario 3D en base a la información del mapa
        /// </summary>
        public void update3dMap()
        {
            //chequear superposicion de rooms
            StringBuilder sb = new StringBuilder("There are collisions between the following Rooms: ");
            int totalCol = 0;
            foreach (RoomsEditorRoom room in rooms)
            {
                RoomsEditorRoom collRoom = testRoomPanelCollision(room, room.RoomPanel.Label.Bounds);
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
            mapScale = new Vector3((float)numericUpDownMapScaleX.Value, (float)numericUpDownMapScaleY.Value, (float)numericUpDownMapScaleZ.Value);


            //Construir rooms en 3D
            foreach (RoomsEditorRoom room in rooms)
            {
                room.buildWalls(rooms, mapScale);
            }
        }





        #region Create Room

        private void panel2d_MouseDown(object sender, MouseEventArgs e)
        {
            //Crear Room
            if (currentMode == EditMode.CreateRoom)
            {
                creatingRoomMouseDown = true;
                creatingRoomOriginalPos = snapPointToGrid(e.X, e.Y);
                string name = "Room-" + (++roomsNameCounter);
                createRoom(name, creatingRoomOriginalPos, new Size(1, 1));
            }
        }

        /// <summary>
        /// Crea un nuevo Room y lo agrega al panel2D
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        public RoomsEditorRoom createRoom(string name, Point pos, Size size)
        {
            RoomPanel rPanel = new RoomPanel(name, this, pos, size);
            RoomsEditorRoom room = new RoomsEditorRoom(name, rPanel);
            rooms.Add(room);

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
                    Point mPoint = snapPointToGrid(e.X, e.Y);
                    int diffX = mPoint.X - creatingRoomOriginalPos.X;
                    int diffY = mPoint.Y - creatingRoomOriginalPos.Y;

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
                            rect = new Rectangle(creatingRoomOriginalPos.X, creatingRoomOriginalPos.Y + diffY, diffX, -diffY);
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
                        RoomsEditorRoom lastRoom = rooms[rooms.Count - 1];
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
                RoomsEditorRoom lastRoom = rooms[rooms.Count - 1];
                lastRoom.RoomPanel.adaptaToMiniumSize();

                //Si colisiona no lo borramos
                RoomsEditorRoom collisionRoom = testRoomPanelCollision(lastRoom, lastRoom.RoomPanel.Label.Bounds);
                if (collisionRoom != null)
                {
                    rooms.Remove(lastRoom);
                    panel2d.Controls.Remove(lastRoom.RoomPanel.Label);
                }

                //Seleccionar room recien creado
                selectRoom(lastRoom);
            }

        }

        #endregion


        #region Edit Room


        /// <summary>
        /// Cambiar nombre de room
        /// </summary>
        private void textBoxRoomName_TextChanged(object sender, EventArgs e)
        {
            if (ValidationUtils.validateRequired(textBoxRoomName.Text))
            {
                //controlar repetidos
                foreach (RoomsEditorRoom r in rooms)
                {
                    if (r != selectedRoom && r.Name == textBoxRoomName.Text)
                    {
                        MessageBox.Show(this, "Ya existe un cuarto con ese nombre: " + textBoxRoomName.Text, "Edición de nombre", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Cambiar X de room
        /// </summary>
        private void numericUpDownRoomPosX_ValueChanged(object sender, EventArgs e)
        {
            Rectangle currentR = selectedRoom.RoomPanel.Label.Bounds;
            Rectangle newR = new Rectangle((int)numericUpDownRoomPosX.Value, currentR.Y, currentR.Width, currentR.Height);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomPosX.Value = currentR.X;
            }
        }


        /// <summary>
        /// Cambiar Y de room
        /// </summary>
        private void numericUpDownRoomPosY_ValueChanged(object sender, EventArgs e)
        {
            Rectangle currentR = selectedRoom.RoomPanel.Label.Bounds;
            Rectangle newR = new Rectangle(currentR.X, (int)numericUpDownRoomPosY.Value, currentR.Width, currentR.Height);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomPosY.Value = currentR.Y;
            }
        }

        /// <summary>
        /// Cambiar Width de room
        /// </summary>
        private void numericUpDownRoomWidth_ValueChanged(object sender, EventArgs e)
        {
            Rectangle currentR = selectedRoom.RoomPanel.Label.Bounds;
            Rectangle newR = new Rectangle(currentR.X, currentR.Y, (int)numericUpDownRoomWidth.Value, currentR.Height);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomWidth.Value = currentR.Width;
            }
        }

        /// <summary>
        /// Cambiar Length de room
        /// </summary>
        private void numericUpDownRoomLength_ValueChanged(object sender, EventArgs e)
        {
            Rectangle currentR = selectedRoom.RoomPanel.Label.Bounds;
            Rectangle newR = new Rectangle(currentR.X, currentR.Y, currentR.Width, (int)numericUpDownRoomLength.Value);
            selectedRoom.RoomPanel.updateLabelBounds(newR);
            if (!selectedRoom.RoomPanel.updateLabelBounds(newR))
            {
                numericUpDownRoomLength.Value = currentR.Height;
            }
        }

        /// <summary>
        /// Variar altura de las paredes de un room
        /// </summary>
        private void numericUpDownRoomHeight_ValueChanged(object sender, EventArgs e)
        {
            int newHeight = (int)numericUpDownRoomHeight.Value;
            selectedRoom.Height = newHeight;
        }

        /// <summary>
        /// Variar nivel del piso de un room
        /// </summary>
        private void numericUpDownRoomFloorLevel_ValueChanged(object sender, EventArgs e)
        {
            int newFloorLevel = (int)numericUpDownRoomFloorLevel.Value;
            selectedRoom.FloorLevel = newFloorLevel;
        }

        

        #endregion


        #region RoomPanel


        /// <summary>
        /// Panel 2D que representa un Room
        /// </summary>
        public class RoomPanel
        {
            readonly Color DEFAULT_ROOM_COLOR = Color.Orange;
            readonly Color SELECTED_ROOM_COLOR = Color.BlueViolet;
            readonly Size MINIUM_SIZE = new Size(50, 50);

            
            internal RoomsEditorMapView mapView;
            bool labelDragging = false;
            Point initDragP;
            Point oldLocation;

            RoomsEditorRoom room;
            public RoomsEditorRoom Room
            {
                get { return room; }
                set { room = value; }
            }

            Label label;
            public Label Label
            {
                get { return label; }
            }

            public RoomPanel(string name, RoomsEditorMapView mapView, Point location, Size size)
            {
                this.mapView = mapView;

                label = new Label();
                label.Text = name;
                label.AutoSize = false;
                label.Location = location;
                label.Size = size;
                label.BorderStyle = BorderStyle.FixedSingle;
                label.BackColor = DEFAULT_ROOM_COLOR;
                label.TextAlign = ContentAlignment.MiddleCenter;

                label.MouseDown += new MouseEventHandler(label_MouseDown);
                label.MouseUp += new MouseEventHandler(label_MouseUp);
                label.MouseMove += new MouseEventHandler(label_MouseMove);
                label.MouseEnter += new EventHandler(label_MouseEnter);
                label.PreviewKeyDown += new PreviewKeyDownEventHandler(label_PreviewKeyDown);
            }

            

            

            /// <summary>
            /// Adapta al tamaño del label al minimo permitido
            /// </summary>
            internal void adaptaToMiniumSize()
            {
                if (label.Width < MINIUM_SIZE.Width)
                {
                    label.Width = MINIUM_SIZE.Width;
                }
                if (label.Height < MINIUM_SIZE.Height)
                {
                    label.Height = MINIUM_SIZE.Height;
                }
            }

            /// <summary>
            /// Seleccionar el cuarto
            /// </summary>
            internal void setRoomSelected(bool selected)
            {
                if (selected)
                {
                    label.BackColor = SELECTED_ROOM_COLOR;
                    label.Focus();
                }
                else
                {
                    label.BackColor = DEFAULT_ROOM_COLOR;
                }
            }


            void label_MouseDown(object sender, MouseEventArgs e)
            {
                //Se quiere hacer drag del label
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    //seleccionar este cuarto como el actual
                    mapView.selectRoom(room);

                    labelDragging = true;
                    initDragP = mapView.snapPointToGrid(e.X, e.Y);
                    oldLocation = label.Location;
                }
            }

            void label_MouseUp(object sender, MouseEventArgs e)
            {
                //Se termino de hacer drag del label, validar posicion
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    labelDragging = false;

                    //Si hay colision con otros labels, restaurar la posicion original
                    Rectangle possibleR = new Rectangle(label.Location, label.Size);
                    RoomsEditorRoom collisionRoom = mapView.testRoomPanelCollision(this.room, possibleR);
                    if (collisionRoom != null)
                    {
                        label.Location = oldLocation;
                    }
                }

            }

            void label_MouseMove(object sender, MouseEventArgs e)
            {
                //Haciendo drag de label, actualizar posicion
                if (mapView.currentMode == EditMode.CreateRoom)
                {
                    if (labelDragging)
                    {
                        int x = label.Location.X - (initDragP.X - e.X);
                        int y = label.Location.Y - (initDragP.Y - e.Y);
                        Point newP = mapView.snapPointToGrid(x, y);

                        //Actualizar posicion solo si no se escapa de los limites del escenario
                        Rectangle newRect = new Rectangle(newP, label.Size);
                        if (mapView.validateRoomBounds(newRect))
                        {
                            label.Location = newP;
                        }

                    }
                }
            }

            void label_MouseEnter(object sender, EventArgs e)
            {
                mapView.Cursor = Cursors.SizeAll;
            }

            void label_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                //Eliminar room
                if (e.KeyCode == Keys.Delete)
                {
                    mapView.selectedRoom = null;
                    mapView.deleteRoom(this.room);
                }
            }

            /// <summary>
            /// Cambia el nombre del Room
            /// </summary>
            internal void changeName(string newName)
            {
                room.Name = newName;
                label.Text = newName;
            }

            /// <summary>
            /// Actualiza el Bounds del label del Room validando limites y colisiones.
            /// Informa si se pude hacer
            /// </summary>
            internal bool updateLabelBounds(Rectangle newR)
            {
                //Validar limites del escenario
                if (!mapView.validateRoomBounds(newR))
                {
                    return false;
                }

                //Validar colision con otros Rooms
                RoomsEditorRoom collisionRoom = mapView.testRoomPanelCollision(this.room, newR);
                if (collisionRoom != null)
                {
                    return false;
                }

                label.Bounds = newR;
                return true;
            }
        }


        #endregion

        

        

        

        

        

        

        

        





    }
}