using System;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using System.Windows.Forms;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;
using Examples.TerrainEditor.Brushes;
using Examples.TerrainEditor.Panel;
using System.Drawing.Imaging;


namespace Examples.TerrainEditor
{

 
    public class TgcTerrainEditor : TgcExample
    {

        public EditableTerrain terrain;
        private TgcPickingRay pickingRay;
        private ITerrainEditorBrush brush;
        public ITerrainEditorBrush Brush { get { return brush; } set { if (value == null) brush = new DummyBrush(); else brush = value; } }
        private TerrainEditorModifier modifierPanel;
        public bool PlanePicking { get; set; }

        public override string getCategory()
        {
            return "Utils";
        }

        public override string getName()
        {
            return "TerrainEditor";
        }

        public override string getDescription()
        {
            return "Terrain editor.\n\nCamara: \nDesplazamiento: W A S D Ctrl Space\n Para rotar mantener presionado el boton derecho.\n";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
          
            terrain = new EditableTerrain();
           

            //Shader
            terrain.Effect = TgcShaders.loadEffect(GuiController.Instance.ExamplesDir + "TerrainEditor\\Shaders\\EditableHeightmap.fx");
            terrain.Technique = "PositionTextured";

            this.brush = new DummyBrush();

            modifierPanel = new TerrainEditorModifier("Panel", this);
            GuiController.Instance.Modifiers.add(modifierPanel);

            pickingRay = new TgcPickingRay();
          

            GuiController.Instance.Panel3d.MouseMove += new MouseEventHandler(Panel3d_MouseMove);
            GuiController.Instance.Panel3d.MouseLeave += new EventHandler(Panel3d_MouseLeave);
            
            //Configurar FPS Camara
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 300f;
            GuiController.Instance.FpsCamera.JumpSpeed = 300f;
            GuiController.Instance.FpsCamera.RotateMouseButton = TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(-722.6171f, 495.0046f, -31.2611f), new Vector3(164.9481f, 35.3185f, -61.5394f));
            
        }
   

      

        void Panel3d_MouseLeave(object sender, EventArgs e)
        {
            if (Brush.mouseLeave(this)) ChangesMade = true;
            
        }

        void Panel3d_MouseMove(object sender, MouseEventArgs e)
        {
            if (Brush.mouseMove(this)) ChangesMade = true;       
        }

    

        public bool mousePositionInTerrain(out Vector3 position)
        {
            pickingRay.updateRay();

            if (PlanePicking)
               return terrain.intersectRayPlane(pickingRay.Ray, out position);
            else
                return terrain.intersectRay(pickingRay.Ray, out position);
        }


        public bool ChangesMade { get; set; }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            
            terrain.Technique = "PositionColoredTextured";

            if (Brush.update(this)) ChangesMade = true;

            Brush.render(this);
            
           
        }               
        

        public void save(String path)
        {

            int width = terrain.HeightmapData.GetLength(0);
            int height = terrain.HeightmapData.GetLength(1);
            Bitmap bitmap = new Bitmap(height, width);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    int intensity = (int)terrain.HeightmapData[i, j];
                    Color pixel = Color.FromArgb(intensity, intensity, intensity); //Si R=G=B el color es blanco, gris o negro
                    bitmap.SetPixel(j, i, pixel);


                }

            }

            bitmap.Save(path, ImageFormat.Jpeg);
            bitmap.Dispose();
            ChangesMade = false;
        }

        public override void close()
        {
            terrain.dispose();
            Brush.dispose();
        }

    }

    public class DummyBrush : ITerrainEditorBrush
    {
        public bool mouseMove(TgcTerrainEditor editor)
        {
            return false;
        }

        public bool mouseLeave(TgcTerrainEditor editor)
        {
            return false;
        }

        public bool update(TgcTerrainEditor editor)
        {
            return false;
        }

        public void render(TgcTerrainEditor editor)
        {
            editor.terrain.render();
        }

        public void dispose()
        {
            return;
        }
    }
}
