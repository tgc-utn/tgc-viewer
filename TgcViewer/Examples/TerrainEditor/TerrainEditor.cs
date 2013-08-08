using System;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using System.Windows.Forms;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.Input;
using Examples.TerrainEditor.Brushes;
using Examples.TerrainEditor.Panel;
using System.Drawing.Imaging;
using Examples.TerrainEditor.Brushes.Level;


namespace Examples.TerrainEditor
{

 
    public class TgcTerrainEditor : TgcExample
    {

        public EditableTerrain terrain;
        TgcPickingRay pickingRay;
        public TerrainBrush Brush {get;set;}
        bool brushOut;
        TerrainEditorModifier modifierPanel;
       
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
             brushOut = true;
           
        }

        void Panel3d_MouseMove(object sender, MouseEventArgs e)
        {
           updateBrushPosition();
             
        }

        private void updateBrushPosition()
        {

            if (modifierPanel.Control.Editing)
            {
                brushOut = false;
                //Actualizar Ray de colisión en base a posición del mouse
                pickingRay.updateRay();
                Vector3 pos;
                if (terrain.intersectRay(pickingRay.Ray, out pos))
                {
                    Brush.Position = pos;

                }
                else brushOut = true;
            }
        }



        public bool ChangesMade { get; set; }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            terrain.Technique = "PositionColoredTextured";

            if (modifierPanel.Control.Editing && !brushOut)
            {
                if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    bool invert = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftAlt);

                    bool oldInvert = Brush.Invert;
                    Brush.Invert ^= invert;
                    if (!Brush.Editing) Brush.beginEdition(terrain);
                    if (Brush.editTerrain())
                    {
                        ChangesMade = true;
                        terrain.updateVertices();
                    }

                    Brush.Invert = oldInvert;
                }
                else { if (Brush.Editing) Brush.endEdition(); }
         
                Brush.configureTerrainEffect(terrain);

            }
           
            
            //Renderizar terreno
            terrain.render();
           
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
        }
        public override void close()
        {
            terrain.dispose();
        }

    }
}
