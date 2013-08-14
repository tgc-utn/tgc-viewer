using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Examples.MeshCreator;
using Examples.TerrainEditor.Brushes.Terrain;
using Examples.TerrainEditor.Brushes.Vegetation;
using Examples.TerrainEditor.Vegetation;
using TgcViewer;
using TgcViewer.Utils.Modifiers;
using Microsoft.DirectX;
using System.Collections.Generic;

namespace Examples.TerrainEditor.Panel
{
    public partial class TerrainEditorControl : UserControl
    {
        TgcTerrainEditor terrainEditor;
        TgcTextureBrowser heightmapBrowser;
        TgcTextureBrowser textureBrowser;
        Shovel shovel;
        Steamroller steamroller;
        VegetationBrush vegetationBrush;
   
        public TerrainEditorControl(TgcTerrainEditor terrainEditor)
        {
            // TODO: Complete member initialization
            this.terrainEditor = terrainEditor;

            InitializeComponent();
            //Tab General
            createHeightmapBrowser();

            pictureBoxModifyHeightmap.ImageLocation = heightmapBrowser.SelectedImage;

            textureBrowser = new TgcTextureBrowser();
            textureBrowser.ShowFolders = true;
            textureBrowser.setSelectedImage(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture1-256x256.jpg");
            pictureBoxModifyTexture.ImageLocation = textureBrowser.SelectedImage;
            terrainEditor.Terrain.loadHeightmap(heightmapBrowser.SelectedImage, (float)nudScaleXZ.Value, (float)nudScaleY.Value, new Microsoft.DirectX.Vector3(0, 0, 0));
            terrainEditor.Terrain.loadTexture(textureBrowser.SelectedImage);
            
            shovel = new Shovel();
            vegetationBrush = new VegetationBrush();
            steamroller = new Steamroller();

            //Tooltips
            toolTip1.SetToolTip(rbShovel, "Pala.\nAumenta la altura del terreno.\nShovel sound by adough1@freesound");
            toolTip1.SetToolTip(rbSteamroller, "Aplanadora.\nNivela el terreno\nSteamroller stock image by presterjohn1@deviantArt");
            toolTip1.SetToolTip(tbRadius, "Regula el tamaño del pincel");
            toolTip1.SetToolTip(tbIntensity, "Regula la intesidad del efecto del pincel");
            toolTip1.SetToolTip(tbHardness, "Regula el tamaño del radio interno.\nA medida que los vertices se alejan del radio interno, la intensidad disminuye.");
            toolTip1.SetToolTip(cbRounded, "Cuando se deselecciona, el pincel es cuadrado");
            toolTip1.SetToolTip(cbInvert, "Invierte el efecto del pincel.\n(La pala hunde, la aplanadora aumenta los desniveles)");
            toolTip1.SetToolTip(bChangeFolder, "La carpeta seleccionada debe contener carpetas con\nel mismo nombre que el -TgcScene.xml que llevan dentro.");
            
            //Camera
            terrainEditor.Camera.MovementSpeed = tbCameraMovementSpeed.Value;
            terrainEditor.Camera.JumpSpeed = tbCameraJumpSpeed.Value;
        

            //Info
            setInfo();
            folderBrowserDialog1.SelectedPath = InstancesManager.Location;

            //Vegetation
            fillVegetationList(InstancesManager.Location);
        }

        private void tabControl_TabIndexChanged(object sender, EventArgs e)
        {

            vegetationBrush.removeFloatingVegetation();
            if (tabControl.SelectedTab == pageEdit)
            {
                terrainEditor.PlanePicking = true;
                if (rbShovel.Checked) setBrush(shovel);
                else if (rbSteamroller.Checked) setBrush(steamroller);
            }
            else if (tabControl.SelectedTab == tabVegetation)
            {
                terrainEditor.PlanePicking = false;
                setBrush(vegetationBrush);
            }
            else terrainEditor.Brush = null;

        }





        public void dispose()
        {
            vegetationBrush.dispose();
            shovel.dispose();
            steamroller.dispose();
        }


        #region General

        private void createHeightmapBrowser()
        {
            heightmapBrowser = new TgcTextureBrowser();
            heightmapBrowser.ShowFolders = true;
            heightmapBrowser.setSelectedImage(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap1.jpg");

        }

        private void pictureBoxModifyHeightmap_Click(object sender, EventArgs e)
        {

            string selected = heightmapBrowser.SelectedImage;
            if (heightmapBrowser.ShowDialog() == DialogResult.OK && !selected.Equals(heightmapBrowser.SelectedImage))
            {
                Image img = MeshCreatorUtils.getImage(heightmapBrowser.SelectedImage);
                pictureBoxModifyHeightmap.Image = img;
                pictureBoxModifyHeightmap.ImageLocation = heightmapBrowser.SelectedImage;

                terrainEditor.loadHeightmap(heightmapBrowser.SelectedImage, (float)nudScaleXZ.Value, (float)nudScaleY.Value);

                setInfo();
            }
        }

        private void pictureBoxModifyTexture_Click(object sender, EventArgs e)
        {
            if (textureBrowser.ShowDialog() == DialogResult.OK)
            {
                Image img = MeshCreatorUtils.getImage(textureBrowser.SelectedImage);
                pictureBoxModifyTexture.Image = img;
                pictureBoxModifyTexture.ImageLocation = textureBrowser.SelectedImage;

                terrainEditor.Terrain.loadTexture(pictureBoxModifyTexture.ImageLocation);
            }
        }

        private void buttonNewHeightmap_Click(object sender, EventArgs e)
        {
            pictureBoxModifyHeightmap.Image = null;
            pictureBoxModifyHeightmap.Refresh();
            terrainEditor.loadPlainHeightmap((int)nupWidth.Value, (int)nupHeight.Value, (int)nupLevel.Value, (float)nudScaleXZ.Value, (float)nudScaleY.Value);
            setInfo();
        }



        private void nudScale_ValueChanged(object sender, EventArgs e)
        {
            terrainEditor.setScale((float)nudScaleXZ.Value, (float)nudScaleY.Value);
            setInfo();
        }

        private void bImportVegetation_Click(object sender, EventArgs e)
        {
            openFileVegetation.ShowDialog(this);
        }

        private void openFileVegetation_FileOk(object sender, CancelEventArgs e)
        {
            if (terrainEditor.HasVegetation)
                if (MessageBox.Show("¿Remover vegetacion actual?", "El terreno ya tiene vegetacion", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    terrainEditor.clearVegetation();
                else MessageBox.Show("Por ahora no se pueden importar modelos con nombres iguales a los ya cargados, de darse ese caso, las instancias viejas se eliminaran.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
         
            terrainEditor.addVegetation(InstancesManager.Instance.import(openFileVegetation.FileName));
            terrainEditor.updateVegetationY();
        }

        private void bReload_Click(object sender, EventArgs e)
        {
            terrainEditor.loadHeightmap(heightmapBrowser.SelectedImage, (float)nudScaleXZ.Value, (float)nudScaleY.Value);

        }

        #endregion

        #region Vegetation


        private void setBrush(VegetationBrush brush)
        {

            terrainEditor.Brush = brush;

        }

       
        private void fillVegetationList(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            List<string> names = new List<string>();
            foreach (string folder in folders)
            {

                string name = folder.Substring(folder.LastIndexOf("\\") + 1);
                if (File.Exists(folder + "\\" + name + "-TgcScene.xml")) names.Add(name);
            }

           
            if (names.Count > 0)
            {
                InstancesManager.Location = path;
                lbVegetation.Items.Clear();
                lbVegetation.Items.AddRange(names.ToArray());
                lbVegetation.SelectedIndex = 0;

            }
        }

        private void lbVegetation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbVegetation.SelectedIndex == -1)
            {
                pbVegetationPreview.Image = null;
                pbVegetationPreview.Refresh();
                return;
            }

            pbVegetationPreview.ImageLocation = InstancesManager.Location + lbVegetation.SelectedItem.ToString() + "\\preview.jpg";
            vegetationBrush.setVegetation(lbVegetation.SelectedItem.ToString());

        }

        private void bVegetationClear_Click(object sender, EventArgs e)
        {

            vegetationBrush.removeFloatingVegetation();
            terrainEditor.clearVegetation();
        }

        private void bChangeFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                
                fillVegetationList(folderBrowserDialog1.SelectedPath);

            }
        }


        private void updateVBScaleAxis(object sender, EventArgs e)
        {
            configVegetationBrush();
        }

        private void configVegetationBrush()
        {
            Vector3 scale = new Vector3(0, 0, 0);

            if (cbSx.Checked) scale += new Vector3(1, 0, 0);
            if (cbSy.Checked) scale += new Vector3(0, 1, 0);
            if (cbSz.Checked) scale += new Vector3(0, 0, 1);

            vegetationBrush.ScaleAxis = scale;
        }

        private void updateVBRotationAxis(object sender, EventArgs e)
        {
            if (rbRx.Checked) vegetationBrush.Rotation = VegetationBrush.RotationAxis.X;
            else if (rbRy.Checked) vegetationBrush.Rotation = VegetationBrush.RotationAxis.Y;
            else if (rbRz.Checked) vegetationBrush.Rotation = VegetationBrush.RotationAxis.Z;
        }

        private void bClearTF_Click(object sender, EventArgs e)
        {
            vegetationBrush.setVegetation(lbVegetation.SelectedItem.ToString());
        }
        
        #endregion

        #region Edit

        private void tbBrush_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            String prop = (string)tb.Tag;

            //Reflection para escribir menos :P
            shovel.GetType().GetProperty(prop).SetValue(shovel, tb.Value, null);
            steamroller.GetType().GetProperty(prop).SetValue(steamroller, tb.Value, null);
        }

        private void cbBrush_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            String prop = (string)cb.Tag;

            //Reflection para escribir menos :P
            shovel.GetType().GetProperty(prop).SetValue(shovel, cb.Checked, null);
            steamroller.GetType().GetProperty(prop).SetValue(steamroller, cb.Checked, null);

        }


        private void rbShovel_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShovel.Checked) setBrush(shovel);

        }

        private void bSteamroller_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSteamroller.Checked) setBrush(steamroller);

        }

        private void setBrush(TerrainBrush brush)
        {
            brush.Hardness = tbHardness.Value;
            brush.Intensity = tbIntensity.Value;
            brush.Radius = tbRadius.Value;
            brush.Invert = cbInvert.Checked;
            brush.Rounded = cbRounded.Checked;

            terrainEditor.Brush = brush;

        }

        #endregion

        #region Export
        private void setInfo()
        {
            labelVerticesCount.Text = "Vertices: " + terrainEditor.Terrain.TotalVertices.ToString();
            lbCenter.Text = String.Format("Center:  ({0},{1},{2})", terrainEditor.Terrain.Center.X, terrainEditor.Terrain.Center.Y,terrainEditor.Terrain.Center.Z);
            lbScaleXZ.Text = "ScaleXZ:  " + terrainEditor.Terrain.ScaleXZ.ToString();
            lbScaleY.Text = "ScaleY:  " + terrainEditor.Terrain.ScaleY.ToString();
            lwidth.Text = "Width:  "+terrainEditor.Terrain.HeightmapData.GetLength(1).ToString();
            lheight.Text = "Height:  "+terrainEditor.Terrain.HeightmapData.GetLength(0).ToString();
            lname.Text = "Name: " + heightmapBrowser.SelectedImage.Substring(heightmapBrowser.SelectedImage.LastIndexOf("\\")+1);
        }

      
        private void buttonSaveHeightmap_Click(object sender, EventArgs e)
        {
            saveFileHeightmap.ShowDialog(this);           
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                terrainEditor.save(saveFileHeightmap.FileName);
                heightmapBrowser.setSelectedImage(saveFileHeightmap.FileName);
                pictureBoxModifyHeightmap.ImageLocation = heightmapBrowser.SelectedImage;

            }
            catch (Exception ex)
            {

                MessageBox.Show(this, "Hubo un error al intentar exportar la textura. Puede ocurrir que este intentando reemplazar una textura que se encuentra en la misma carpeta de la textura que tiene abierta. Intente crear una nueva.\n"
                       + "Error: " + ex.Message + " - " + ex.InnerException.Message,
                       "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void buttonSaveVegetation_Click(object sender, EventArgs e)
        {
            saveFileVegetation.ShowDialog(this);
        }

        private void saveFileVegetation_FileOk(object sender, CancelEventArgs e)
        {
            FileInfo fInfo = new FileInfo(saveFileVegetation.FileName);
            string sceneName = fInfo.Name.Split('.')[0];
            sceneName = sceneName.Replace("-TgcScene", "");

            try
            {

                terrainEditor.saveVegetation(sceneName, fInfo.DirectoryName);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Hubo un error al intentar exportar la escena. Puede ocurrir que esté intentando reemplazar el mismo archivo de escena que tiene abierto ahora. Los archivos de Textura por ejemplo no pueden ser reemplazados si se están utilizando dentro del editor. En ese caso debera guardar en uno nuevo. "
                       + "Error: " + ex.Message + " - " + ex.InnerException.Message,
                       "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

   


        #endregion

        #region Settings


        private void tbCameraMovementSpeed_Scroll(object sender, EventArgs e)
        {
            terrainEditor.Camera.MovementSpeed = tbCameraMovementSpeed.Value;
        }

        private void tbCameraJumpSpeed_Scroll(object sender, EventArgs e)
        {
            terrainEditor.Camera.JumpSpeed = tbCameraJumpSpeed.Value;
        }

        private void cbSound_CheckedChanged(object sender, EventArgs e)
        {
            vegetationBrush.SoundEnabled = cbSound.Checked;
            steamroller.SoundEnabled = cbSound.Checked;
            shovel.SoundEnabled = cbSound.Checked;
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Desarrollado por Daniela Kazarian.\n","About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       


    

       


     

    }
}
