using System;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Textures;
using TGC.Util.Modifiers;

namespace TGC.Examples.RoomsEditor
{
    /// <summary>
    ///     Ventana que permite editar las texturas de cada pared de un Room
    /// </summary>
    public partial class RoomsEditorTexturesEdit : Form
    {
        private readonly RoomsEditorMapView mapView;
        private readonly TgcTextureBrowser textureBrowser;

        public RoomsEditorTexturesEdit(RoomsEditorMapView mapView)
        {
            InitializeComponent();

            this.mapView = mapView;
            textureBrowser = new TgcTextureBrowser();
            textureBrowser.CurrentDir = mapView.defaultTextureDir;

            //Cargar imagenes default
            var defaultTextureImage = mapView.defaultTextureImage;
            roofImage.ImageLocation = defaultTextureImage;
            floorImage.ImageLocation = defaultTextureImage;
            eastWallImage.ImageLocation = defaultTextureImage;
            westWallImage.ImageLocation = defaultTextureImage;
            northWallImage.ImageLocation = defaultTextureImage;
            southWallImage.ImageLocation = defaultTextureImage;
        }

        /// <summary>
        ///     Aplicar valores de North Wall a todo el resto
        /// </summary>
        private void buttonApplyToAll_Click(object sender, EventArgs e)
        {
            //Replicar textura de Nort Wall a todos
            var imagePath = northWallImage.ImageLocation;
            roofImage.ImageLocation = imagePath;
            floorImage.ImageLocation = imagePath;
            eastWallImage.ImageLocation = imagePath;
            westWallImage.ImageLocation = imagePath;
            southWallImage.ImageLocation = imagePath;

            //Replicar valor de auto UV
            var autoUv = northWallAutoUv.Checked;
            roofAutoUv.Checked = autoUv;
            floorAutoUv.Checked = autoUv;
            eastWallAutoUv.Checked = autoUv;
            westWallAutoUv.Checked = autoUv;
            southWallAutoUv.Checked = autoUv;

            //Replicar valor de U
            var u = northWallUTile.Value;
            roofUTile.Value = u;
            floorUTile.Value = u;
            eastWallUTile.Value = u;
            westWallUTile.Value = u;
            southWallUTile.Value = u;

            //Replicar valor de v
            var v = northWallVTile.Value;
            roofVTile.Value = v;
            floorVTile.Value = v;
            eastWallVTile.Value = v;
            westWallVTile.Value = v;
            southWallVTile.Value = v;
        }

        /// <summary>
        ///     Actualizar datos de una pared
        /// </summary>
        private void updateWallData(RoomsEditorWall wall, PictureBox image, CheckBox autoUv, NumericUpDown uTile,
            NumericUpDown vTile)
        {
            if (wall.Texture != null)
            {
                wall.Texture.dispose();
            }
            wall.Texture = TgcTexture.createTexture(D3DDevice.Instance.Device, image.ImageLocation);
            wall.AutoAdjustUv = autoUv.Checked;
            wall.UTile = (float)uTile.Value;
            wall.VTile = (float)vTile.Value;
        }

        /// <summary>
        ///     Actualiza datos de la UI en base a una pared
        /// </summary>
        private void updateUiData(RoomsEditorWall wall, PictureBox image, CheckBox autoUv, NumericUpDown uTile,
            NumericUpDown vTile)
        {
            image.ImageLocation = wall.Texture.FilePath;
            autoUv.Checked = wall.AutoAdjustUv;
            uTile.Value = (decimal)wall.UTile;
            vTile.Value = (decimal)wall.VTile;
        }

        /// <summary>
        ///     Carga todos los valores en base a la información del Room
        /// </summary>
        public void fillRoomData(RoomsEditorRoom room)
        {
            updateUiData(mapView.selectedRoom.Walls[0], roofImage, roofAutoUv, roofUTile, roofVTile);
            updateUiData(mapView.selectedRoom.Walls[1], floorImage, floorAutoUv, floorUTile, floorVTile);
            updateUiData(mapView.selectedRoom.Walls[2], eastWallImage, eastWallAutoUv, eastWallUTile, eastWallVTile);
            updateUiData(mapView.selectedRoom.Walls[3], westWallImage, westWallAutoUv, westWallUTile, westWallVTile);
            updateUiData(mapView.selectedRoom.Walls[4], northWallImage, northWallAutoUv, northWallUTile, northWallVTile);
            updateUiData(mapView.selectedRoom.Walls[5], southWallImage, southWallAutoUv, southWallUTile, southWallVTile);
        }

        /// <summary>
        ///     Actualizar datos al cerrar ventana
        /// </summary>
        private void RoomsEditorTexturesEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            updateWallData(mapView.selectedRoom.Walls[0], roofImage, roofAutoUv, roofUTile, roofVTile);
            updateWallData(mapView.selectedRoom.Walls[1], floorImage, floorAutoUv, floorUTile, floorVTile);
            updateWallData(mapView.selectedRoom.Walls[2], eastWallImage, eastWallAutoUv, eastWallUTile, eastWallVTile);
            updateWallData(mapView.selectedRoom.Walls[3], westWallImage, westWallAutoUv, westWallUTile, westWallVTile);
            updateWallData(mapView.selectedRoom.Walls[4], northWallImage, northWallAutoUv, northWallUTile,
                northWallVTile);
            updateWallData(mapView.selectedRoom.Walls[5], southWallImage, southWallAutoUv, southWallUTile,
                southWallVTile);
        }

        /// <summary>
        ///     Abrir el TextureBrowser para elegir una textura, al hacer click sobre un PictureBox
        /// </summary>
        private void changeImage(PictureBox image)
        {
            textureBrowser.setSelectedImage(image.ImageLocation);
            if (textureBrowser.ShowDialog(this) == DialogResult.OK)
            {
                image.ImageLocation = textureBrowser.SelectedImage;
            }
        }

        private void northWallImage_Click(object sender, EventArgs e)
        {
            changeImage(northWallImage);
        }

        private void westWallImage_Click(object sender, EventArgs e)
        {
            changeImage(westWallImage);
        }

        private void floorImage_Click(object sender, EventArgs e)
        {
            changeImage(floorImage);
        }

        private void eastWallImage_Click(object sender, EventArgs e)
        {
            changeImage(eastWallImage);
        }

        private void southWallImage_Click(object sender, EventArgs e)
        {
            changeImage(southWallImage);
        }

        private void roofImage_Click(object sender, EventArgs e)
        {
            changeImage(roofImage);
        }
    }
}