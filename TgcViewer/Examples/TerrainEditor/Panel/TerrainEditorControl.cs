using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TgcViewer.Utils.Modifiers;
using TgcViewer;
using Examples.MeshCreator;
using Examples.TerrainEditor.Brushes;
using Examples.TerrainEditor.Brushes.Level;
using Examples.TerrainEditor.Brushes.LevelBrush;

namespace Examples.TerrainEditor.Panel
{
    public partial class TerrainEditorControl : UserControl
    {
        private TgcTerrainEditor terrainEditor;

        TgcTextureBrowser heightmapBrowser;
        TgcTextureBrowser textureBrowser;

        
        public TerrainBrush SelectedBrush
        {
            get { return terrainEditor.Brush; }
            set{setBrush(value);}

        }
       
        
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
            terrainEditor.terrain.loadHeightmap(heightmapBrowser.SelectedImage, (float)nudScaleXZ.Value, (float)nudScaleY.Value, new Microsoft.DirectX.Vector3(0, 0, 0));
            terrainEditor.terrain.loadTexture(textureBrowser.SelectedImage);

            //Pincel
            SelectedBrush = new Shovel();

            //Info
            setInfo();

            
          

        }

        private void setInfo()
        {
            labelVerticesCount.Text = "Vertices: " + terrainEditor.terrain.TotalVertices.ToString();
            lbCenter.Text = String.Format("Center:  ({0},{1},{2})", terrainEditor.terrain.Center.X, terrainEditor.terrain.Center.Y,terrainEditor.terrain.Center.Z);
            lbScaleXZ.Text = "ScaleXZ:  " + terrainEditor.terrain.ScaleXZ.ToString();
            lbScaleY.Text = "ScaleY:  " + terrainEditor.terrain.ScaleY.ToString();
            lwidth.Text = "Width:  "+terrainEditor.terrain.HeightmapData.GetLength(1).ToString();
            lheight.Text = "Height:  "+terrainEditor.terrain.HeightmapData.GetLength(0).ToString();
            lname.Text = "Name: " + heightmapBrowser.SelectedImage.Substring(heightmapBrowser.SelectedImage.LastIndexOf("\\")+1);
        }

        private void createHeightmapBrowser()
        {
            heightmapBrowser = new TgcTextureBrowser();
            heightmapBrowser.ShowFolders = true;
            heightmapBrowser.setSelectedImage(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "Heightmap1.jpg");

        }

        private void pictureBoxModifyHeightmap_Click(object sender, EventArgs e)
        {
            if (terrainEditor.ChangesMade)
            {
                if (MessageBox.Show("¿Desea guardar los cambios?", "El heightmap ha sido editado.", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    saveFileDialog1.ShowDialog(this);
                }

            }
            if (heightmapBrowser.ShowDialog() == DialogResult.OK)
            {
                Image img = MeshCreatorUtils.getImage(heightmapBrowser.SelectedImage);
                pictureBoxModifyHeightmap.Image = img;
                pictureBoxModifyHeightmap.ImageLocation = heightmapBrowser.SelectedImage;

                terrainEditor.terrain.loadHeightmap(heightmapBrowser.SelectedImage, (float)nudScaleXZ.Value, (float)nudScaleY.Value, new Microsoft.DirectX.Vector3(0, 0, 0));
                setInfo();
            }

            terrainEditor.ChangesMade = false;

        }

        private void pictureBoxModifyTexture_Click(object sender, EventArgs e)
        {
            if (textureBrowser.ShowDialog() == DialogResult.OK)
            {
                Image img = MeshCreatorUtils.getImage(textureBrowser.SelectedImage);
                pictureBoxModifyTexture.Image = img;
                pictureBoxModifyTexture.ImageLocation = textureBrowser.SelectedImage;

                terrainEditor.terrain.loadTexture(pictureBoxModifyTexture.ImageLocation);
            }
        }

     

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog(this);
           
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            terrainEditor.save(saveFileDialog1.FileName);
            heightmapBrowser.setSelectedImage(saveFileDialog1.FileName);
            pictureBoxModifyHeightmap.ImageLocation = heightmapBrowser.SelectedImage;
            terrainEditor.ChangesMade = false;
        }

        public bool Editing { get { return tabControl.SelectedTab.Name.Equals("pageEdit"); } }
      

        private void buttonNewHeightmap_Click(object sender, EventArgs e)
        {
            pictureBoxModifyHeightmap.Image = null;
            pictureBoxModifyHeightmap.Refresh();
            terrainEditor.ChangesMade = false;
            terrainEditor.terrain.loadPlainHeightmap((int)nupWidth.Value, (int)nupHeight.Value,(int)nupLevel.Value, (float)nudScaleXZ.Value, (float)nudScaleY.Value, new Microsoft.DirectX.Vector3(0,0,0));
            setInfo();
        }

        private void nudScaleXZ_ValueChanged(object sender, EventArgs e)
        {
            terrainEditor.terrain.ScaleXZ = (float)nudScaleXZ.Value;
            setInfo();
        }

        private void nudScaleY_ValueChanged(object sender, EventArgs e)
        {
            terrainEditor.terrain.ScaleY = (float)nudScaleY.Value;
            setInfo();
        }

        private void tbBrsuh_Scroll(object sender, EventArgs e)
        {
            TrackBar tb = (TrackBar)sender;
            String prop = (string)tb.Tag;
            terrainEditor.Brush.GetType().GetProperty(prop).SetValue(terrainEditor.Brush, (float)tb.Value, null);
        }

        private void cbInvert_CheckedChanged(object sender, EventArgs e)
        {
            terrainEditor.Brush.Invert = cbInvert.Checked;
        }

        private void cbRounded_CheckedChanged(object sender, EventArgs e)
        {
            terrainEditor.Brush.Rounded = cbRounded.Checked;
        }

       


        private void rbShovel_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShovel.Checked) SelectedBrush = new Shovel();
           

        }

        private void bSteamroller_CheckedChanged(object sender, EventArgs e)
        {
            if (bSteamroller.Checked) SelectedBrush = new Steamroller();
           
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

        private void rbPPlane_CheckedChanged(object sender, EventArgs e)
        {
            terrainEditor.PlanePicking = rbPPlane.Checked;
        }

     

    
       

     

     

      
    }
}
