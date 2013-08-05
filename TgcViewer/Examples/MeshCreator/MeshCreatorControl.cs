using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Input;
using Examples.MeshCreator.Primitives;
using Examples.MeshCreator.Gizmos;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using System.IO;
using TgcViewer.Utils._2D;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Control grafico de MeshCreator
    /// </summary>
    public partial class MeshCreatorControl : UserControl
    {
        /// <summary>
        /// Estado general del editor
        /// </summary>
        public enum State
        {
            SelectObject,
            SelectingObject,
            CreatePrimitiveSelected,
            CreatingPrimitve,
            GizmoActivated,
        }

        

        /// <summary>
        /// Colores para crear nuevas primitivas
        /// </summary>
        static readonly Color[] CREATION_COLORS = new Color[]{
            Color.Red,
            Color.Blue,
            Color.Brown,
            Color.Coral,
            Color.DarkCyan,
            Color.Green,
            Color.Purple
        };


        
        TgcMeshCreator creator;
        //int creationColorIdx;
        int primitiveNameCounter;
        TranslateGizmo translateGizmo;
        ScaleGizmo scaleGizmo;
        TgcTextureBrowser textureBrowser;
        string defaultTexturePath;
        string defaultMeshPath;
        TgcMeshBrowser meshBrowser;
        SaveFileDialog exportSceneSaveDialog;
        TgcText2d objectPositionText;
        bool doDeleteSelectedObjects;
        


        List<EditorPrimitive> meshes;
        /// <summary>
        /// Objetos del escenario
        /// </summary>
        public List<EditorPrimitive> Meshes
        {
            get { return meshes; }
        }

        List<EditorPrimitive> selectionList;
        /// <summary>
        /// Objetos seleccionados
        /// </summary>
        public List<EditorPrimitive> SelectionList
        {
            get { return selectionList; }
        }

        Grid grid;
        /// <summary>
        /// Grid
        /// </summary>
        public Grid Grid
        {
            get { return grid; }
        }

        MeshCreatorCamera camera;
        /// <summary>
        /// Camara
        /// </summary>
        public MeshCreatorCamera Camera
        {
            get { return camera; }
        }

        TgcPickingRay pickingRay;
        /// <summary>
        /// PickingRay
        /// </summary>
        public TgcPickingRay PickingRay
        {
            get { return pickingRay; }
        }

        State currentState;
        /// <summary>
        /// Estado actual
        /// </summary>
        public State CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        EditorPrimitive creatingPrimitive;
        /// <summary>
        /// Primitiva actual seleccionada para crear
        /// </summary>
        public EditorPrimitive CreatingPrimitive
        {
            get { return creatingPrimitive; }
            set { creatingPrimitive = value; }
        }

        EditorGizmo currentGizmo;
        /// <summary>
        /// Gizmo seleccionado actualmente
        /// </summary>
        public EditorGizmo CurrentGizmo
        {
            get { return currentGizmo; }
            set { currentGizmo = value; }
        }

        SelectionRectangle selectionRectangle;
        /// <summary>
        /// Rectangulo de seleccion
        /// </summary>
        public SelectionRectangle SelectionRectangle
        {
            get { return selectionRectangle; }
        }

        string mediaPath;
        /// <summary>
        /// Archivos de Media propios del editor
        /// </summary>
        public string MediaPath
        {
            get { return mediaPath; }
        }

        /// <summary>
        /// Mostrar AABB de los objetos no seleccionados
        /// </summary>
        public bool ShowObjectsAABB
        {
            get { return checkBoxShowObjectsBoundingBox.Checked; }
        }


        public MeshCreatorControl(TgcMeshCreator creator)
        {
            InitializeComponent();

            this.creator = creator;
            this.meshes = new List<EditorPrimitive>();
            this.selectionList = new List<EditorPrimitive>();
            this.pickingRay = new TgcPickingRay();
            this.grid = new Grid(this);
            this.selectionRectangle = new SelectionRectangle(this);
            creatingPrimitive = null;
            //creationColorIdx = 0;
            primitiveNameCounter = 0;
            currentGizmo = null;
            mediaPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\";
            defaultTexturePath = mediaPath + "Textures\\Madera\\cajaMadera1.jpg";
            checkBoxShowObjectsBoundingBox.Checked = true;
            doDeleteSelectedObjects = false;

            //meshBrowser
            defaultMeshPath = mediaPath + "Meshes\\Vegetacion\\Arbusto\\Arbusto-TgcScene.xml";
            meshBrowser = new TgcMeshBrowser();
            meshBrowser.setSelectedMesh(defaultMeshPath);

            //Export scene dialog
            exportSceneSaveDialog = new SaveFileDialog();
            exportSceneSaveDialog.DefaultExt = ".xml";
            exportSceneSaveDialog.Filter = ".XML |*.xml";
            exportSceneSaveDialog.AddExtension = true;
            exportSceneSaveDialog.Title = "Export scene to a -TgcScene.xml file";

            //Camara
            camera = new MeshCreatorCamera();
            camera.Enable = true;
            camera.setCamera(new Vector3(0, 0, 0), 500);
            camera.BaseRotX = -FastMath.PI / 4f;

            //Gizmos
            translateGizmo = new TranslateGizmo(this);
            scaleGizmo = new ScaleGizmo(this);

            //Tab inicial
            tabControl.SelectedTab = tabControl.TabPages["tabPageCreate"];
            currentState = State.SelectObject;
            radioButtonSelectObject.Checked = true;

            //Tab Modify
            textureBrowser = new TgcTextureBrowser();
            textureBrowser.ShowFolders = true;
            textureBrowser.setSelectedImage(defaultTexturePath);
            pictureBoxModifyTexture.ImageLocation = defaultTexturePath;
            pictureBoxModifyTexture.Image = MeshCreatorUtils.getImage(defaultTexturePath);
            updateModifyPanel();

            //ObjectPosition Text
            objectPositionText = new TgcText2d();
            objectPositionText.Align = TgcText2d.TextAlign.LEFT;
            objectPositionText.Color = Color.Yellow;
            objectPositionText.Size = new Size(300, 12);
            objectPositionText.Position = new Point(GuiController.Instance.Panel3d.Width - 250, GuiController.Instance.Panel3d.Height - 20);
        }

        /// <summary>
        /// Ciclo loop del editor
        /// </summary>
        public void render()
        {
            //Procesar shorcuts de teclado
            processShortcuts();

            //Actualizar camara
            updateCamera();

            //Maquina de estados
            switch (currentState)
            {
                case State.SelectObject:
                    doSelectObject();
                    break;
                case State.SelectingObject:
                    doSelectingObject();
                    break;
                case State.CreatePrimitiveSelected:
                    doCreatePrimitiveSelected();
                    break;
                case State.CreatingPrimitve:
                    doCreatingPrimitve();
                    break;
                case State.GizmoActivated:
                    doGizmoActivated();
                    break;
            }

            //Ver si se pidio que eliminar algun objeto
            if (doDeleteSelectedObjects)
            {
                deleteSelectedObjects();
                doDeleteSelectedObjects = false;
            }


            //Dibujar objetos del escenario
            renderObjects();
            
            //Dibujar gizmo (sin Z-Buffer, al final de tod)
            if (currentGizmo != null)
            {
                currentGizmo.render();
            }
        }

        

        /// <summary>
        /// Procesar shorcuts de teclado
        /// </summary>
        private void processShortcuts()
        {
            //Solo en estados pasivos
            if (currentState == State.SelectObject || currentState == State.CreatePrimitiveSelected || currentState == State.GizmoActivated)
            {
                TgcD3dInput input = GuiController.Instance.D3dInput;

                //Zoom
                if (input.keyPressed(Key.Z))
                {
                    buttonZoomObject_Click(null, null);
                }
                //Select
                else if (input.keyPressed(Key.Q))
                {
                    radioButtonSelectObject.Checked = true;
                }
                //Delete
                else if (input.keyPressed(Key.Delete))
                {
                    buttonDeleteObject_Click(null, null);
                }
                //Translate
                else if (input.keyPressed(Key.W))
                {
                    radioButtonModifySelectAndMove.Checked = true;
                }
                //Scale
                else if (input.keyPressed(Key.R))
                {
                    radioButtonModifySelectAndScale.Checked = true;
                }
                //Clone
                else if (input.keyDown(Key.LeftControl) && input.keyPressed(Key.V))
                {
                    buttonCloneObject_Click(null, null);
                }
                //Create Box
                else if (input.keyPressed(Key.B))
                {
                    radioButtonPrimitive_Box.Checked = true;
                }
                //Create Plane
                else if (input.keyPressed(Key.P))
                {
                    radioButtonPrimitive_PlaneXZ.Checked = true;
                }
                //Import Mesh
                else if (input.keyPressed(Key.M))
                {
                    buttonImportMesh_Click(null, null);
                }
                //Save (Export Scene)
                else if (input.keyDown(Key.LeftControl) && input.keyPressed(Key.S))
                {
                    buttonExportScene_Click(null, null);
                }
                //Help
                else if (input.keyPressed(Key.F1))
                {
                    buttonHelp_Click(null, null);
                }
            }
        }
        

        

        

        /// <summary>
        /// Dibujar todos los objetos
        /// </summary>
        private void renderObjects()
        {
            //Objetos opacos
            foreach (EditorPrimitive mesh in meshes)
            {
                if (!mesh.AlphaBlendEnable)
                {
                    if (mesh.Visible)
                    {
                        mesh.render();
                        if (ShowObjectsAABB || mesh.Selected)
                        {
                            mesh.BoundingBox.render();
                        }
                    }
                }
            }

            //Grid
            grid.render();

            //Objeto que se esta construyendo actualmente
            if (currentState == State.CreatingPrimitve)
            {
                creatingPrimitive.render();
            }

            //Recuadro de seleccion
            if (currentState == State.SelectingObject)
            {
                selectionRectangle.render();
            }

            //Objetos transparentes
            foreach (EditorPrimitive mesh in meshes)
            {
                if (mesh.AlphaBlendEnable)
                {
                    if (mesh.Visible)
                    {
                        mesh.render();
                        if (ShowObjectsAABB || mesh.Selected)
                        {
                            mesh.BoundingBox.render();
                        }
                    }
                }
            }

            //Object position Text
            if (selectionList.Count > 0)
            {
                objectPositionText.Text = "Pos: " + TgcParserUtils.printVector3(selectionList[0].Position);
                objectPositionText.render();
            }
        }

        /// <summary>
        /// Actualizar camara segun movimientos
        /// </summary>
        private void updateCamera()
        {
            //Ajustar velocidad de zoom segun distancia a objeto
            Vector3 q;
            if (selectionList.Count > 0)
            {
                q = selectionList[0].BoundingBox.PMin;
            }
            else
            {
                q = Vector3.Empty;
            }
            camera.ZoomFactor = MeshCreatorUtils.getMouseZoomSpeed(this.camera, q);

            camera.updateCamera();
            camera.updateViewMatrix(GuiController.Instance.D3dDevice);
        }

        /// <summary>
        /// Estado: seleccionar objetos (estado default)
        /// </summary>
        private void doSelectObject()
        {
            selectionRectangle.doSelectObject();
        }

        /// <summary>
        /// Estado: Cuando se esta arrastrando el mouse para armar el cuadro de seleccion
        /// </summary>
        private void doSelectingObject()
        {
            selectionRectangle.render();
        }

        /// <summary>
        /// Estado: cuando se hizo clic en algun boton de primitiva para crear
        /// </summary>
        private void doCreatePrimitiveSelected()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Si hacen clic con el mouse, iniciar creacion de la primitiva
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                Vector3 gridPoint = grid.getPicking();
                creatingPrimitive.initCreation(gridPoint);
                currentState = State.CreatingPrimitve;
            }
        }

        /// <summary>
        /// Estado: mientras se esta creando una primitiva
        /// </summary>
        private void doCreatingPrimitve()
        {
            creatingPrimitive.doCreation();
        }

        /// <summary>
        /// Estado: se traslada, rota o escala un objeto
        /// </summary>
        private void doGizmoActivated()
        {
            currentGizmo.update();
        }

        /// <summary>
        /// Agregar mesh creado
        /// </summary>
        public void addMesh(EditorPrimitive mesh)
        {
            this.meshes.Add(mesh);
            updateMeshesPanel();
        }
        

        /*
        /// <summary>
        /// Siguiente color para crear objeto
        /// </summary>
        public Color getCreationColor()
        {
            creationColorIdx = (creationColorIdx + 1) % CREATION_COLORS.Length;
            return CREATION_COLORS[creationColorIdx];
        }
        */

        /// <summary>
        /// Textura para crear un nuevo objeto
        /// </summary>
        /// <returns></returns>
        public string getCreationTexturePath()
        {
            return pictureBoxModifyTexture.ImageLocation;
        }

        /// <summary>
        /// Nombre para crear una nueva primitiva
        /// </summary>
        public string getNewPrimitiveName(string type)
        {
            return type + "_" + primitiveNameCounter++;
        }

        /// <summary>
        /// Eliminar todos los objetos seleccionados
        /// </summary>
        private void deleteSelectedObjects()
        {
            foreach (EditorPrimitive p in selectionList)
            {
                meshes.Remove(p);
                p.dispose();
            }

            //Limpiar lista de seleccion
            selectionList.Clear();
            updateModifyPanel();
            updateMeshesPanel();

            //Pasar a modo seleccion
            currentState = MeshCreatorControl.State.SelectObject;
        }

        /// <summary>
        /// Cargar Tab de Modify cuando hay un objeto seleccionado
        /// </summary>
        public void updateModifyPanel()
        {
            if (selectionList.Count == 1)
            {
                groupBoxModifyGeneral.Enabled = true;
                groupBoxModifyTexture.Enabled = true;
                groupBoxModifyPosition.Enabled = true;
                groupBoxModifyRotation.Enabled = true;
                groupBoxModifyScale.Enabled = true;
                groupBoxModifyUserProps.Enabled = true;

                //Cargar valores generales
                EditorPrimitive p = selectionList[0];
                EditorPrimitive.ModifyCapabilities caps = p.ModifyCaps;
                textBoxModifyName.Text = p.Name;
                textBoxModifyLayer.Text = p.Layer;

                //Cargar textura
                if (caps.ChangeTexture)
                {
                    pictureBoxModifyTexture.Enabled = true;
                    pictureBoxModifyTexture.Image = Image.FromFile(p.Texture.FilePath);
                    pictureBoxModifyTexture.ImageLocation = p.Texture.FilePath;
                    textureBrowser.setSelectedImage(p.Texture.FilePath);
                    checkBoxModifyAlphaBlendEnabled.Enabled = true;
                    checkBoxModifyAlphaBlendEnabled.Checked = p.AlphaBlendEnable;
                }
                else
                {
                    pictureBoxModifyTexture.Enabled = false;
                    checkBoxModifyAlphaBlendEnabled.Enabled = false;
                }
                
                //Cargar OffsetUV
                if (caps.ChangeOffsetUV)
                {
                    numericUpDownTextureOffsetU.Enabled = true;
                    numericUpDownTextureOffsetV.Enabled = true;
                    numericUpDownTextureOffsetU.Value = (decimal)p.TextureOffset.X;
                    numericUpDownTextureOffsetV.Value = (decimal)p.TextureOffset.Y;
                }
                else
                {
                    numericUpDownTextureOffsetU.Enabled = false;
                    numericUpDownTextureOffsetV.Enabled = false;
                }

                //Cargar TilingUV
                if (caps.ChangeTilingUV)
                {
                    numericUpDownTextureTilingU.Enabled = true;
                    numericUpDownTextureTilingV.Enabled = true;
                    numericUpDownTextureTilingU.Value = (decimal)p.TextureTiling.X;
                    numericUpDownTextureTilingV.Value = (decimal)p.TextureTiling.Y;
                }
                else
                {
                    numericUpDownTextureTilingU.Enabled = false;
                    numericUpDownTextureTilingV.Enabled = false;
                }

                //Cargar Position
                if (caps.ChangePosition)
                {
                    numericUpDownModifyPosX.Enabled = true;
                    numericUpDownModifyPosY.Enabled = true;
                    numericUpDownModifyPosZ.Enabled = true;
                    numericUpDownModifyPosX.Value = (decimal)p.Position.X;
                    numericUpDownModifyPosY.Value = (decimal)p.Position.Y;
                    numericUpDownModifyPosZ.Value = (decimal)p.Position.Z;
                }
                else
                {
                    numericUpDownModifyPosX.Enabled = false;
                    numericUpDownModifyPosY.Enabled = false;
                    numericUpDownModifyPosZ.Enabled = false;
                }

                //Cargar Rotation
                if (caps.ChangeRotation)
                {
                    numericUpDownModifyRotX.Enabled = true;
                    numericUpDownModifyRotY.Enabled = true;
                    numericUpDownModifyRotZ.Enabled = true;
                    numericUpDownModifyRotX.Value = (decimal)FastMath.ToDeg(p.Rotation.X);
                    numericUpDownModifyRotY.Value = (decimal)FastMath.ToDeg(p.Rotation.Y);
                    numericUpDownModifyRotZ.Value = (decimal)FastMath.ToDeg(p.Rotation.Z);
                }
                else
                {
                    numericUpDownModifyRotX.Enabled = false;
                    numericUpDownModifyRotY.Enabled = false;
                    numericUpDownModifyRotZ.Enabled = false;
                }

                //Cargar Scale
                if (caps.ChangeScale)
                {
                    numericUpDownModifyScaleX.Enabled = true;
                    numericUpDownModifyScaleY.Enabled = true;
                    numericUpDownModifyScaleZ.Enabled = true;
                    Vector3 scale = p.Scale;
                    numericUpDownModifyScaleX.Value = (decimal)scale.X * 100;
                    numericUpDownModifyScaleY.Value = (decimal)scale.Y * 100;
                    numericUpDownModifyScaleZ.Value = (decimal)scale.Z * 100;
                }
                else
                {
                    numericUpDownModifyScaleX.Enabled = false;
                    numericUpDownModifyScaleY.Enabled = false;
                    numericUpDownModifyScaleZ.Enabled = false;
                }

                //Cargar userProps
                userInfo.Text = MeshCreatorUtils.getUserPropertiesString(p.UserProperties);
            }
            else
            {
                groupBoxModifyGeneral.Enabled = false;
                groupBoxModifyTexture.Enabled = false;
                groupBoxModifyPosition.Enabled = false;
                groupBoxModifyRotation.Enabled = false;
                groupBoxModifyScale.Enabled = false;
                groupBoxModifyUserProps.Enabled = false;
            }
        }

        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void close()
        {
            foreach (EditorPrimitive mesh in meshes)
            {
                mesh.dispose();
            }
            grid.dispose();
            selectionRectangle.dispose();
            if (creatingPrimitive != null)
            {
                selectionRectangle.dispose();
            }
        }

        /// <summary>
        /// Actualizar panel con grilla de modelos del escenario
        /// </summary>
        public void updateMeshesPanel()
        {
            //Cargar dataGrid con objetos del escenario
            dataGridViewMeshes.Rows.Clear();
            foreach (EditorPrimitive primitive in meshes)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewMeshes, new object[] { 
                        primitive.Name, 
                        primitive.Visible });

                row.Tag = primitive;
                dataGridViewMeshes.Rows.Add(row);
            }
        }




        #region Eventos generales

        /// <summary>
        /// Desactivar todos los radios
        /// </summary>
        private void untoggleAllRadioButtons(RadioButton exceptRadio)
        {
            if (radioButtonPrimitive_Box != exceptRadio)
            {
                radioButtonPrimitive_Box.Checked = false;
            }
            if (radioButtonPrimitive_PlaneXZ != exceptRadio)
            {
                radioButtonPrimitive_PlaneXZ.Checked = false;
            }
            if (radioButtonPrimitive_PlaneXY != exceptRadio)
            {
                radioButtonPrimitive_PlaneXY.Checked = false;
            }
            if (radioButtonPrimitive_PlaneYZ != exceptRadio)
            {
                radioButtonPrimitive_PlaneYZ.Checked = false;
            }
            if (radioButtonSelectObject != exceptRadio)
            {
                radioButtonSelectObject.Checked = false;
            }
            if (radioButtonModifySelectAndMove != exceptRadio)
            {
                radioButtonModifySelectAndMove.Checked = false;
            }
            if (radioButtonModifySelectAndRotate != exceptRadio)
            {
                radioButtonModifySelectAndRotate.Checked = false;
            }
            if (radioButtonModifySelectAndScale != exceptRadio)
            {
                radioButtonModifySelectAndScale.Checked = false;
            }
        }

        /// <summary>
        /// Clic en "Crear Box"
        /// </summary>
        private void radioButtonPrimitive_Box_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_Box.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_Box);
                currentState = State.CreatePrimitiveSelected;
                creatingPrimitive = new BoxPrimitive(this);
            }
        }

        /// <summary>
        /// Clic en "Crear Sphere"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonPrimitive_Sphere_CheckedChanged(object sender, EventArgs e)
        {

            if (radioButtonPrimitive_Sphere.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_Sphere);
                currentState = State.CreatePrimitiveSelected;
                creatingPrimitive = new SpherePrimitive(this);
            }
        }
        /// <summary>
        /// Clic en "Crear Plano XZ"
        /// </summary>
        private void radioButtonPrimitive_PlaneXZ_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneXZ.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneXZ);
                currentState = State.CreatePrimitiveSelected;
                creatingPrimitive = new PlaneXZPrimitive(this);
            }
        }

        /// <summary>
        /// Clic en "Crear Plano XY"
        /// </summary>
        private void radioButtonPrimitive_PlaneXY_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneXY.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneXY);
                currentState = State.CreatePrimitiveSelected;
                creatingPrimitive = new PlaneXYPrimitive(this);
            }
        }

        /// <summary>
        /// Clic en "Crear Plano YZ"
        /// </summary>
        private void radioButtonPrimitive_PlaneYZ_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneYZ.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneYZ);
                currentState = State.CreatePrimitiveSelected;
                creatingPrimitive = new PlaneYZPrimitive(this);
            }
        }

        /// <summary>
        /// Clic en "Seleccionar objeto"
        /// </summary>
        private void radioButtonSelectObject_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelectObject.Checked)
            {
                untoggleAllRadioButtons(radioButtonSelectObject);
                currentState = State.SelectObject;
                creatingPrimitive = null;
                currentGizmo = null;
            }
        }

        /// <summary>
        /// Clic en "Zoom"
        /// </summary>
        private void buttonZoomObject_Click(object sender, EventArgs e)
        {
            selectionRectangle.zoomObject();
        }

        /// <summary>
        /// Clic en "Delete object"
        /// </summary>
        private void buttonDeleteObject_Click(object sender, EventArgs e)
        {
            doDeleteSelectedObjects = true;
        }

        
        /// <summary>
        /// Clic en "Clonar seleccion"
        /// </summary>
        private void buttonCloneObject_Click(object sender, EventArgs e)
        {
            if (selectionList.Count > 0)
            {
                //Clonar toda la seleccion
                List<EditorPrimitive> cloneMeshes = new List<EditorPrimitive>();
                foreach (EditorPrimitive p in selectionList)
                {
                    EditorPrimitive pClone = p.clone();
                    cloneMeshes.Add(pClone);
                }

                //Agregar al escenario y seleccionarlas
                selectionRectangle.clearSelection();
                foreach (EditorPrimitive pClone in cloneMeshes)
                {
                    this.meshes.Add(pClone);
                    selectionRectangle.selectObject(pClone);
                }
                currentState = MeshCreatorControl.State.SelectObject;
                selectionRectangle.activateCurrentGizmo();
                updateModifyPanel();
                updateMeshesPanel();
            }
        }

        /// <summary>
        /// Clic en "Importar Mesh"
        /// </summary>
        private void buttonImportMesh_Click(object sender, EventArgs e)
        {
            if (meshBrowser.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Cargar scene
                    string path = meshBrowser.SelectedMesh;
                    TgcSceneLoader loader = new TgcSceneLoader();
                    TgcScene scene = loader.loadSceneFromFile(path);

                    //Agregar meshes al escenario y tambien seleccionarlas
                    selectionRectangle.clearSelection();
                    foreach (TgcMesh mesh in scene.Meshes)
                    {
                        MeshPrimitive p = new MeshPrimitive(this, mesh);
                        meshes.Add(p);
                        selectionRectangle.selectObject(p);
                    }
                    currentState = MeshCreatorControl.State.SelectObject;
                    selectionRectangle.activateCurrentGizmo();
                    updateModifyPanel();
                    updateMeshesPanel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Hubo un error al importar el mesh." + "Error: " + ex.Message + " - " + ex.InnerException.Message,
                        "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Clic en "Exportar escena"
        /// </summary>
        private void buttonExportScene_Click(object sender, EventArgs e)
        {
            if (exportSceneSaveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Obtener nombre y directorio
                    FileInfo fInfo = new FileInfo(exportSceneSaveDialog.FileName);
                    string sceneName = fInfo.Name.Split('.')[0];
                    sceneName = sceneName.Replace("-TgcScene", "");
                    string saveDir = fInfo.DirectoryName;

                    //Convertir todos los objetos del escenario a un TgcMesh y agregarlos a la escena a exportar
                    TgcScene exportScene = new TgcScene(sceneName, saveDir);
                    foreach (EditorPrimitive p in meshes)
                    {
                        TgcMesh m = p.createMeshToExport();
                        exportScene.Meshes.Add(m);
                    }

                    //Exportar escena
                    TgcSceneExporter exporter = new TgcSceneExporter();
                    if (checkBoxAttachExport.Checked)
                    {
                        exporter.exportAndAppendSceneToXml(exportScene, saveDir);
                    }
                    else
                    {
                        exporter.exportSceneToXml(exportScene, saveDir);
                    }


                    MessageBox.Show(this, "Scene exported OK.", "Export Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Hubo un error al intenar exportar la escena. Puede ocurrir que esté intentando reemplazar el mismo archivo de escena que tiene abierto ahora. Los archivos de Textura por ejemplo no pueden ser reemplazados si se están utilizando dentro del editor. En ese caso debera guardar en uno nuevo. "
                        + "Error: " + ex.Message + " - " + ex.InnerException.Message,
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }

        /// <summary>
        /// Clic en "Help"
        /// </summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            //Abrir PDF
            System.Diagnostics.Process.Start(GuiController.Instance.ExamplesDir + "\\MeshCreator\\Guia MeshCreator.pdf");
        }

        #endregion



        #region Eventos de Modify


        /// <summary>
        /// Clic en "Seleccionar y Mover"
        /// </summary>
        private void radioButtonModifySelectAndMove_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonModifySelectAndMove.Checked)
            {
                untoggleAllRadioButtons(radioButtonModifySelectAndMove);
                currentState = State.SelectObject;
                creatingPrimitive = null;
                currentGizmo = translateGizmo;

                //Activar gizmo si hay seleccion
                if (selectionList.Count > 0)
                {
                    selectionRectangle.activateCurrentGizmo();
                }
            }
        }

        private void radioButtonModifySelectAndRotate_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Clic en "Seleccionar y Escalar"
        /// </summary>
        private void radioButtonModifySelectAndScale_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonModifySelectAndScale.Checked)
            {
                untoggleAllRadioButtons(radioButtonModifySelectAndScale);
                currentState = State.SelectObject;
                creatingPrimitive = null;
                currentGizmo = scaleGizmo;

                //Activar gizmo si hay seleccion
                if (selectionList.Count > 0)
                {
                    selectionRectangle.activateCurrentGizmo();
                }
            }
        }

        /// <summary>
        /// Cambiar nombre de mesh
        /// </summary>
        private void textBoxModifyName_Leave(object sender, EventArgs e)
        {
            selectionList[0].Name = textBoxModifyName.Text;
        }

        /// <summary>
        /// Cambiar nombre de layer
        /// </summary>
        private void textBoxModifyLayer_Leave(object sender, EventArgs e)
        {
            selectionList[0].Layer = textBoxModifyLayer.Text;
        }

        /// <summary>
        /// Hacer clic en el recuadro de textura para cambiarla
        /// </summary>
        private void pictureBoxModifyTexture_Click(object sender, EventArgs e)
        {
            if (textureBrowser.ShowDialog() == DialogResult.OK)
            {
                Image img = MeshCreatorUtils.getImage(textureBrowser.SelectedImage);
                pictureBoxModifyTexture.Image = img;
                pictureBoxModifyTexture.ImageLocation = textureBrowser.SelectedImage;
            }
            else
            {
                pictureBoxModifyTexture.Image = MeshCreatorUtils.getImage(defaultTexturePath);
                pictureBoxModifyTexture.ImageLocation = defaultTexturePath;
            }
            selectionList[0].Texture = TgcTexture.createTexture(pictureBoxModifyTexture.ImageLocation);
        }

        /// <summary>
        /// Cambiar Alpha Blend
        /// </summary>
        private void checkBoxModifyAlphaBlendEnabled_CheckedChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.AlphaBlendEnable = checkBoxModifyAlphaBlendEnabled.Checked;
        }

        /// <summary>
        /// Cambiar offset U
        /// </summary>
        private void numericUpDownTextureOffsetU_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector2 offset = new Vector2((float)numericUpDownTextureOffsetU.Value, p.TextureOffset.Y);
            p.TextureOffset = offset;
        }

        /// <summary>
        /// Cambiar offset V
        /// </summary>
        private void numericUpDownTextureOffsetV_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector2 offset = new Vector2(p.TextureOffset.X, (float)numericUpDownTextureOffsetV.Value);
            p.TextureOffset = offset;
        }

        /// <summary>
        /// Cambiar tile U
        /// </summary>
        private void numericUpDownTextureTilingU_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector2 tiling = new Vector2((float)numericUpDownTextureTilingU.Value, p.TextureTiling.Y);
            p.TextureTiling = tiling;
        }


        /// <summary>
        /// Cambiar tile V
        /// </summary>
        private void numericUpDownTextureTilingV_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector2 tiling = new Vector2(p.TextureTiling.X, (float)numericUpDownTextureTilingV.Value);
            p.TextureTiling = tiling;
        }


        /// <summary>
        /// Cambiar posicion en X
        /// </summary>
        private void numericUpDownModifyPosX_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector3 oldPos = p.Position;
            p.Position = new Vector3((float)numericUpDownModifyPosX.Value, oldPos.Y, oldPos.Z);
            if (currentGizmo != null)
            {
                currentGizmo.move(p, p.Position - oldPos);
            }
        }

        /// <summary>
        /// Cambiar posicion en Y
        /// </summary>
        private void numericUpDownModifyPosY_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector3 oldPos = p.Position;
            p.Position = new Vector3(oldPos.X, (float)numericUpDownModifyPosY.Value, oldPos.Z);
            if (currentGizmo != null)
            {
                currentGizmo.move(p, p.Position - oldPos);
            }
        }

        /// <summary>
        /// Cambiar posicion en Z
        /// </summary>
        private void numericUpDownModifyPosZ_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            Vector3 oldPos = p.Position;
            p.Position = new Vector3(oldPos.X, oldPos.Y, (float)numericUpDownModifyPosZ.Value);
            if (currentGizmo != null)
            {
                currentGizmo.move(p, p.Position - oldPos);
            }
        }

        /// <summary>
        /// Cambiar rotacion en X
        /// </summary>
        private void numericUpDownModifyRotX_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Rotation = new Vector3(FastMath.ToRad((float)numericUpDownModifyRotX.Value), p.Rotation.Y, p.Rotation.Z);
        }

        /// <summary>
        /// Cambiar rotacion en Y
        /// </summary>
        private void numericUpDownModifyRotY_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Rotation = new Vector3(p.Rotation.X, FastMath.ToRad((float)numericUpDownModifyRotY.Value), p.Rotation.Z);
        }

        /// <summary>
        /// Cambiar rotacion en Z
        /// </summary>
        private void numericUpDownModifyRotZ_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Rotation = new Vector3(p.Rotation.X, p.Rotation.Y, FastMath.ToRad((float)numericUpDownModifyRotZ.Value));
        }

        /// <summary>
        /// Cambiar escala en X
        /// </summary>
        private void numericUpDownModifyScaleX_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Scale = new Vector3((float)numericUpDownModifyScaleX.Value / 100f, p.Scale.Y, p.Scale.Z);
        }

        /// <summary>
        /// Cambiar escala en Y
        /// </summary>
        private void numericUpDownModifyScaleY_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Scale = new Vector3(p.Scale.X, (float)numericUpDownModifyScaleY.Value / 100f, p.Scale.Z);
        }

        /// <summary>
        /// Cambiar escala en Z
        /// </summary>
        private void numericUpDownModifyScaleZ_ValueChanged(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.Scale = new Vector3(p.Scale.X, p.Scale.Y, (float)numericUpDownModifyScaleZ.Value / 100f);
        }

        /// <summary>
        /// Cambiar user properties
        /// </summary>
        private void userInfo_Leave(object sender, EventArgs e)
        {
            EditorPrimitive p = selectionList[0];
            p.UserProperties = MeshCreatorUtils.getUserPropertiesDictionary(userInfo.Text);
        }
        

        #endregion



        #region Eventos de Selection


        /// <summary>
        /// Clic en Visible de la tabla de meshes
        /// </summary>
        private void dataGridViewMeshes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewMeshes.SelectedCells.Count > 0)
            {
                DataGridViewCell cell = dataGridViewMeshes.SelectedCells[0];
                EditorPrimitive p = (EditorPrimitive)dataGridViewMeshes.Rows[cell.RowIndex].Tag;
                p.Visible = bool.Parse((string)cell.Value);
            }
        }


        #endregion

      

































    }
}
