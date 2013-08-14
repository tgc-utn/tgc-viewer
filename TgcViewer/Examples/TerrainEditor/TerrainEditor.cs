using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Examples.TerrainEditor.Brushes;
using Examples.TerrainEditor.Panel;
using Examples.TerrainEditor.Vegetation;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;


namespace Examples.TerrainEditor
{

 
    public class TgcTerrainEditor : TgcExample
    {
        #region Fields
        private TgcPickingRay pickingRay;
        private ITerrainEditorBrush brush;
        private bool showVegetation;
        private bool mustUpdateVegetationPosition;
        private TerrainEditorModifier modifierPanel;
        private List<TgcMesh> vegetation;
        private TerrainFpsCamera camera;
        private DummyBrush noBrush;
        private float previousSpeed;
        private TgcD3dInput.MouseButtons cameraRotationButton = TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_RIGHT;
        private TgcD3dInput.MouseButtons cameraRotationFPSButton = TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT;
        private MouseEventHandler mouseMove;
        private EventHandler mouseLeave;
        private SmartTerrain terrain;
        #endregion


        #region Properties
        

        /// <summary>
        /// Obtiene o configura el terreno a editar
        /// </summary>
        public SmartTerrain Terrain { get { return terrain; } set { terrain = value; Camera.Terrain = value; } }

        /// <summary>
        /// Obtiene o configura el brush a utilizar
        /// </summary>
        public ITerrainEditorBrush Brush { get { if (Camera.FpsModeEnable) return noBrush; else return brush; } set { if (value == null) brush = noBrush; else brush = value; } }
        
        /// <summary>
        /// Determina si se renderizaran los objetos sobre el terreno
        /// </summary>
        public bool ShowVegetation { get { return showVegetation; } set { showVegetation = value; if (showVegetation && mustUpdateVegetationPosition) updateVegetationY(); } }
        
        /// <summary>
        /// Determina el tipo de picking a utilizar. Cuando esta en true, se hace picking contra el plano del terreno (no depende de la altura). En false hace picking contra las montanias.
        /// </summary>
        protected bool planePicking;
        public bool PlanePicking { get { return planePicking; } set { planePicking = value; Brush.mouseMove(this); } }

        /// <summary>
        /// Obtiene la camara del editor.
        /// </summary>
        public TerrainFpsCamera Camera { get { return camera; } }

        /// <summary>
        /// Setea el modo FPS de la camara. Cuando ese modo esta activo no se puede editar el terreno.
        /// </summary>
        public bool FpsModeEnable
        {
            get { return camera.FpsModeEnable; }
            set
            {
                if (value && !camera.FpsModeEnable)
                {
                    camera.RotateMouseButton = cameraRotationFPSButton;
                    previousSpeed = camera.MovementSpeed;
                    camera.MovementSpeed /= 2;
                }
                else if (!value && camera.FpsModeEnable)
                {
                    camera.RotateMouseButton = cameraRotationButton;
                    camera.MovementSpeed = previousSpeed;
                }
                camera.FpsModeEnable = value;

            }
        }

        #endregion
        
      
        
    
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
            return @"Terrain editor.
Camara: 
    Desplazamiento: W A S D Ctrl Space
    Para rotar mantener presionado el boton derecho.
Ocultar vegetacion: V 
Cambiar modo de picking: P
Modo primera persona: F (Rotacion con boton izquierdo)";
        }
        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            camera = new TerrainFpsCamera();
            Terrain = new SmartTerrain();
            this.brush = new DummyBrush();
            this.vegetation = new List<TgcMesh>();
            modifierPanel = new TerrainEditorModifier("Panel", this);

            GuiController.Instance.Modifiers.add(modifierPanel);

            pickingRay = new TgcPickingRay();
            ShowVegetation = true;
            mouseMove = new MouseEventHandler(Panel3d_MouseMove);
            mouseLeave = new EventHandler(Panel3d_MouseLeave);
            noBrush = new DummyBrush();
            GuiController.Instance.Panel3d.MouseMove += mouseMove;
            GuiController.Instance.Panel3d.MouseLeave += mouseLeave;

            //Configurar FPS Camara
            camera.Enable = true;
            camera.RotateMouseButton = cameraRotationButton;
            camera.setCamera(new Vector3(-722.6171f, 495.0046f, -31.2611f), new Vector3(164.9481f, 35.3185f, -61.5394f));
          

            
        }

      

        void Panel3d_MouseLeave(object sender, EventArgs e)
        {
            Brush.mouseLeave(this);
            
        }

        void Panel3d_MouseMove(object sender, MouseEventArgs e)
        {
            Brush.mouseMove(this);      
        }

    
        /// <summary>
        /// Retorna la posicion del mouse en el terreno, usando el metodo de picking configurado.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool mousePositionInTerrain(out Vector3 position)
        {
            pickingRay.updateRay();

            if (PlanePicking)
               return Terrain.intersectRayPlane(pickingRay.Ray, out position);
            else
                return Terrain.intersectRay(pickingRay.Ray, out position);
        }



        #region Render
        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.V))
               ShowVegetation ^= true;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.P))
                PlanePicking ^= true;

            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
                FpsModeEnable ^= true;

            Terrain.Technique = "PositionColoredTextured";

            Brush.update(this);

            Brush.render(this);
            
           
        }

        public void doRender()
        {
            Terrain.render();
            if(ShowVegetation)renderVegetation();

        }

        public void renderVegetation()
        {
            foreach (TgcMesh v in vegetation) v.render();

        }

        #endregion

        #region Vegetation

        /// <summary>
        /// Actualiza la altura de la vegetacion segun la altura del terreno.
        /// </summary>
        public void updateVegetationY()
        {
            if (ShowVegetation)
            {
                foreach (TgcMesh v in vegetation) terrain.setObjectPosition(v);
                
                mustUpdateVegetationPosition = false;
            }
            else mustUpdateVegetationPosition = true;

        }

        /// <summary>
        /// Elimina toda la vegetacion.
        /// </summary>
        public void clearVegetation()
        {
            vegetation.Clear();
            InstancesManager.Clear();

        }

        /// <summary>
        /// Actualiza la escala de la vegetacion segun la variacion de escala scaleRatioXZ
        /// </summary>
        /// <param name="scaleRatioXZ"></param>
        private void updateVegetationScale(float scaleRatioXZ)
        {
            foreach (TgcMesh v in vegetation)
            {
                v.Scale = new Vector3(v.Scale.X * scaleRatioXZ, v.Scale.Y * scaleRatioXZ, v.Scale.Z * scaleRatioXZ);
                v.Position = Vector3.TransformCoordinate(v.Position, Matrix.Scaling(scaleRatioXZ, 1, scaleRatioXZ));
            }
        }

        /// <summary>
        /// Exporta la vegetacion a un -TgcScene.xml
        /// </summary>
        /// <param name="name"></param>
        /// <param name="saveFolderPath"></param>
        public void saveVegetation(string name, string saveFolderPath)
        {
            InstancesManager.Instance.export(vegetation, name, saveFolderPath);
        }

        public void addVegetation(TgcMesh v)
        {
            this.vegetation.Add(v);
        }

        /// <summary>
        /// Remueve y retorna el ultimo mesh
        /// </summary>
        /// <returns></returns>
        public TgcMesh vegetationPop()
        {
            int count = vegetation.Count;

            if (count == 0) return null;

            TgcMesh last = vegetation[count - 1];

            removeVegetation(last);

            return last;

        }

        public bool HasVegetation { get { return vegetation.Count > 0; } }
        /// <summary>
        /// Remueve el mesh, no le hace dispose.
        /// </summary>
        /// <param name="v"></param>
        public void removeVegetation(TgcMesh v)
        {
            this.vegetation.Remove(v);  
        }

        public void addVegetation(List<TgcMesh> list)
        {
            this.vegetation.AddRange(list);
        }

     
        #endregion

        /// <summary>
        /// Configura la escala del terreno.
        /// </summary>
        /// <param name="scaleXZ"></param>
        /// <param name="scaleY"></param>
        public void setScale(float scaleXZ, float scaleY)
        {
            float scaleRatioXZ = scaleXZ / Terrain.ScaleXZ;
            Terrain.ScaleXZ = scaleXZ;
            Terrain.ScaleY = scaleY;

            updateVegetationScale(scaleRatioXZ);
            updateVegetationY();
            Camera.updateCamera();

        }

        /// <summary>
        /// Carga el heightmap a partir de la textura del path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scaleXZ"></param>
        /// <param name="scaleY"></param>
        public void loadHeightmap(string path, float scaleXZ, float scaleY)
        {
            Terrain.loadHeightmap(path, scaleXZ, scaleY, new Vector3(0, 0, 0));
            clearVegetation();
        }

        /// <summary>
        /// Carga un heightmap plano.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="level"></param>
        /// <param name="scaleXZ"></param>
        /// <param name="scaleY"></param>
        public void loadPlainHeightmap(int width, int length, int level, float scaleXZ, float scaleY)
        {
            Terrain.loadPlainHeightmap(width, length, level, scaleXZ, scaleY, new Vector3(0, 0, 0));
            clearVegetation();

        }

        /// <summary>
        /// Exporta el heightmap a un jpg.
        /// </summary>
        /// <param name="path"></param>
        public void save(String path)
        {

            int width = Terrain.HeightmapData.GetLength(0);
            int height = Terrain.HeightmapData.GetLength(1);
            Bitmap bitmap = new Bitmap(height, width);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    int intensity = (int)Terrain.HeightmapData[i, j];
                    Color pixel = Color.FromArgb(intensity, intensity, intensity); 
                    bitmap.SetPixel(j, i, pixel);


                }

            }

          
            bitmap.Save(path, ImageFormat.Jpeg);
            bitmap.Dispose();
        }

      
        public override void close()
        {
            Terrain.dispose();
            clearVegetation();
            modifierPanel.dispose();

            GuiController.Instance.Panel3d.MouseMove -= mouseMove;
            GuiController.Instance.Panel3d.MouseLeave -= mouseLeave;
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
            editor.doRender();
        }

        public void dispose()
        {
          
        }
    }
}
