using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TGC.Core._2D;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Examples.TerrainEditor.Brushes;
using TGC.Examples.TerrainEditor.Instances;
using TGC.Examples.TerrainEditor.Panel;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.TgcGeometry;
using Font = System.Drawing.Font;

namespace TGC.Examples.TerrainEditor
{
    /// <summary>
    ///     Ejemplo TgcTerrainEditor:
    ///     Unidades Involucradas:
    ///     # Unidad 7 - Optimización - Heightmap
    ///     Herramienta para crear terrenos por Heightmaps, ubicar objetos sobre el terreno
    ///     y luego exportarlos para ser usados en otro ejemplo.
    ///     Autor: Daniela Kazarian
    /// </summary>
    public class TgcTerrainEditor : TgcExample
    {
        public bool RenderBoundingBoxes { get; set; }

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
Modo primera persona: F (Rotacion con boton izquierdo)
Mostar AABBs: B";
        }

        public override void init()
        {
            Camera = new TerrainFpsCamera();
            Terrain = new SmartTerrain();
            brush = new DummyBrush();
            Vegetation = new List<TgcMesh>();
            modifierPanel = new TerrainEditorModifier("Panel", this);

            GuiController.Instance.Modifiers.add(modifierPanel);

            pickingRay = new TgcPickingRay();
            ShowVegetation = true;
            mouseMove = Panel3d_MouseMove;
            mouseLeave = Panel3d_MouseLeave;
            noBrush = new DummyBrush();
            GuiController.Instance.Panel3d.MouseMove += mouseMove;
            GuiController.Instance.Panel3d.MouseLeave += mouseLeave;

            //Configurar FPS Camara
            Camera.Enable = true;
            Camera.RotateMouseButton = cameraRotationButton;
            Camera.setCamera(new Vector3(-722.6171f, 495.0046f, -31.2611f), new Vector3(164.9481f, 35.3185f, -61.5394f));

            labelFPS = new TgcText2d();
            labelFPS.Text = "Press F to go back to edition mode";
            labelFPS.changeFont(new Font("Arial", 12, FontStyle.Bold));
            labelFPS.Color = Color.Red;
            labelFPS.Align = TgcText2d.TextAlign.RIGHT;

            labelVegetationHidden = new TgcText2d();
            labelVegetationHidden.Text = "Press V to show vegetation";
            labelVegetationHidden.changeFont(new Font("Arial", 12, FontStyle.Bold));
            labelVegetationHidden.Color = Color.GreenYellow;
            labelVegetationHidden.Format = DrawTextFormat.Bottom | DrawTextFormat.Center;
        }

        private void Panel3d_MouseLeave(object sender, EventArgs e)
        {
            Brush.mouseLeave(this);
        }

        private void Panel3d_MouseMove(object sender, MouseEventArgs e)
        {
            Brush.mouseMove(this);
        }

        /// <summary>
        ///     Retorna la posicion del mouse en el terreno, usando el metodo de picking configurado.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool mousePositionInTerrain(out Vector3 position)
        {
            pickingRay.updateRay();

            if (PlanePicking)
                return Terrain.intersectRayPlane(pickingRay.Ray, out position);
            return Terrain.intersectRay(pickingRay.Ray, out position);
        }

        /// <summary>
        ///     Configura la escala del terreno.
        /// </summary>
        /// <param name="scaleXZ"></param>
        /// <param name="scaleY"></param>
        public void setScale(float scaleXZ, float scaleY)
        {
            var scaleRatioXZ = scaleXZ / Terrain.ScaleXZ;
            Terrain.ScaleXZ = scaleXZ;
            Terrain.ScaleY = scaleY;

            updateVegetationScale(scaleRatioXZ);
            updateVegetationY();
            Camera.updateCamera(GuiController.Instance.ElapsedTime);
        }

        /// <summary>
        ///     Carga el heightmap a partir de la textura del path.
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
        ///     Carga un heightmap plano.
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
        ///     Exporta el heightmap a un jpg.
        /// </summary>
        /// <param name="path"></param>
        public void save(string path)
        {
            var width = Terrain.HeightmapData.GetLength(0);
            var height = Terrain.HeightmapData.GetLength(1);
            var bitmap = new Bitmap(height, width);

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var intensity = (int)Terrain.HeightmapData[i, j];
                    var pixel = Color.FromArgb(intensity, intensity, intensity);
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
            labelFPS.dispose();
            labelVegetationHidden.dispose();
            GuiController.Instance.Panel3d.MouseMove -= mouseMove;
            GuiController.Instance.Panel3d.MouseLeave -= mouseLeave;
        }

        #region Fields

        private TgcPickingRay pickingRay;
        private ITerrainEditorBrush brush;
        private bool showVegetation;
        private bool mustUpdateVegetationPosition;
        private TerrainEditorModifier modifierPanel;
        private DummyBrush noBrush;
        private float previousSpeed;
        private readonly TgcD3dInput.MouseButtons cameraRotationButton = TgcD3dInput.MouseButtons.BUTTON_RIGHT;
        private readonly TgcD3dInput.MouseButtons cameraRotationFPSButton = TgcD3dInput.MouseButtons.BUTTON_LEFT;
        private MouseEventHandler mouseMove;
        private EventHandler mouseLeave;
        private SmartTerrain terrain;
        private TgcText2d labelFPS;
        private TgcText2d labelVegetationHidden;

        #endregion Fields

        #region Properties

        public List<TgcMesh> Vegetation { get; private set; }

        /// <summary>
        ///     Obtiene o configura el terreno a editar
        /// </summary>
        public SmartTerrain Terrain
        {
            get { return terrain; }
            set
            {
                terrain = value;
                Camera.Terrain = value;
            }
        }

        /// <summary>
        ///     Obtiene o configura el brush a utilizar
        /// </summary>
        public ITerrainEditorBrush Brush
        {
            get
            {
                if (Camera.FpsModeEnable) return noBrush;
                return brush;
            }
            set
            {
                if (value == null) brush = noBrush;
                else brush = value;
            }
        }

        /// <summary>
        ///     Determina si se renderizaran los objetos sobre el terreno
        /// </summary>
        public bool ShowVegetation
        {
            get { return showVegetation; }
            set
            {
                showVegetation = value;
                if (showVegetation && mustUpdateVegetationPosition) updateVegetationY();
            }
        }

        /// <summary>
        ///     Determina el tipo de picking a utilizar. Cuando esta en true, se hace picking contra el plano del terreno (no
        ///     depende de la altura). En false hace picking contra las montanias.
        /// </summary>
        protected bool planePicking;

        public bool PlanePicking
        {
            get { return planePicking; }
            set
            {
                planePicking = value;
                Brush.mouseMove(this);
            }
        }

        /// <summary>
        ///     Obtiene la camara del editor.
        /// </summary>
        public TerrainFpsCamera Camera { get; private set; }

        /// <summary>
        ///     Setea el modo FPS de la camara. Cuando ese modo esta activo no se puede editar el terreno.
        /// </summary>
        public bool FpsModeEnable
        {
            get { return Camera.FpsModeEnable; }
            set
            {
                if (value && !Camera.FpsModeEnable)
                {
                    Camera.RotateMouseButton = cameraRotationFPSButton;
                    previousSpeed = Camera.MovementSpeed;
                    Camera.MovementSpeed /= 2;
                }
                else if (!value && Camera.FpsModeEnable)
                {
                    Camera.RotateMouseButton = cameraRotationButton;
                    Camera.MovementSpeed = previousSpeed;
                }
                Camera.FpsModeEnable = value;
            }
        }

        #endregion Properties

        #region Render

        public override void render(float elapsedTime)
        {
            if (GuiController.Instance.D3dInput.keyPressed(Key.V))
                ShowVegetation ^= true;

            if (GuiController.Instance.D3dInput.keyPressed(Key.P))
                PlanePicking ^= true;

            if (GuiController.Instance.D3dInput.keyPressed(Key.F))
                FpsModeEnable ^= true;

            if (GuiController.Instance.D3dInput.keyPressed(Key.B))
                RenderBoundingBoxes ^= true;

            if (FpsModeEnable) labelFPS.render();

            if (!ShowVegetation) labelVegetationHidden.render();

            Terrain.Technique = "PositionColoredTextured";

            Brush.update(this);

            Brush.render(this);
        }

        public void doRender()
        {
            Terrain.render();
            if (ShowVegetation) renderVegetation();
        }

        public void renderVegetation()
        {
            foreach (var v in Vegetation)
            {
                v.render();
                if (RenderBoundingBoxes) v.BoundingBox.render();
            }
        }

        #endregion Render

        #region Vegetation

        /// <summary>
        ///     Actualiza la altura de la vegetacion segun la altura del terreno.
        /// </summary>
        public void updateVegetationY()
        {
            if (ShowVegetation)
            {
                foreach (var v in Vegetation) Terrain.setObjectPosition(v);

                mustUpdateVegetationPosition = false;
            }
            else mustUpdateVegetationPosition = true;
        }

        /// <summary>
        ///     Elimina toda la vegetacion.
        /// </summary>
        public void clearVegetation()
        {
            Vegetation.Clear();
            InstancesManager.Clear();
        }

        /// <summary>
        ///     Actualiza la escala de la vegetacion segun la variacion de escala scaleRatioXZ
        /// </summary>
        /// <param name="scaleRatioXZ"></param>
        private void updateVegetationScale(float scaleRatioXZ)
        {
            foreach (var v in Vegetation)
            {
                v.Scale = new Vector3(v.Scale.X * scaleRatioXZ, v.Scale.Y * scaleRatioXZ, v.Scale.Z * scaleRatioXZ);
                v.Position = Vector3.TransformCoordinate(v.Position, Matrix.Scaling(scaleRatioXZ, 1, scaleRatioXZ));
            }
        }

        /// <summary>
        ///     Exporta la vegetacion a un -TgcScene.xml
        /// </summary>
        /// <param name="name"></param>
        /// <param name="saveFolderPath"></param>
        public void saveVegetation(string name, string saveFolderPath)
        {
            InstancesManager.Instance.export(Vegetation, name, saveFolderPath);
        }

        public void addVegetation(TgcMesh v)
        {
            Vegetation.Add(v);
        }

        /// <summary>
        ///     Remueve y retorna el ultimo mesh
        /// </summary>
        /// <returns></returns>
        public TgcMesh vegetationPop()
        {
            var count = Vegetation.Count;

            if (count == 0) return null;

            var last = Vegetation[count - 1];

            removeVegetation(last);

            return last;
        }

        public bool HasVegetation
        {
            get { return Vegetation.Count > 0; }
        }

        /// <summary>
        ///     Remueve el mesh, no le hace dispose.
        /// </summary>
        /// <param name="v"></param>
        public void removeVegetation(TgcMesh v)
        {
            Vegetation.Remove(v);
        }

        public void addVegetation(List<TgcMesh> list)
        {
            Vegetation.AddRange(list);
        }

        public void removeDisposedVegetation()
        {
            var aux = new List<TgcMesh>();
            foreach (var m in Vegetation) if (m.D3dMesh != null) aux.Add(m);
            Vegetation = aux;
        }

        #endregion Vegetation
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