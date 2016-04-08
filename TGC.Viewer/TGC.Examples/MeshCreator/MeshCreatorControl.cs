using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Core._2D;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Examples.MeshCreator.Gizmos;
using TGC.Examples.MeshCreator.Primitives;
using TGC.Viewer;
using TGC.Viewer.Utils.Input;
using TGC.Viewer.Utils.Modifiers;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Examples.MeshCreator
{
    /// <summary>
    ///     Control grafico de MeshCreator
    /// </summary>
    public partial class MeshCreatorControl : UserControl
    {
        /// <summary>
        ///     Estado general del editor
        /// </summary>
        public enum State
        {
            SelectObject,
            SelectingObject,
            CreatePrimitiveSelected,
            CreatingPrimitve,
            GizmoActivated,
            EditablePoly
        }

        private readonly string defaultMeshPath;
        private readonly string defaultTexturePath;
        private readonly SaveFileDialog exportSceneSaveDialog;

        private readonly TgcMeshBrowser meshBrowser;

        private readonly ObjectBrowser objectBrowser;
        private readonly TgcText2d objectPositionText;
        private readonly ScaleGizmo scaleGizmo;

        private readonly TgcTextureBrowser textureBrowser;
        private readonly TgcTextureBrowser textureBrowserEPoly;
        private readonly TranslateGizmo translateGizmo;

        private TgcMeshCreator creator;
        private bool fpsCameraEnabled;

        private string lastSavePath;

        private int primitiveNameCounter;
        private Vector3 rotationPivot;

        public MeshCreatorControl(TgcMeshCreator creator)
        {
            InitializeComponent();

            this.creator = creator;
            Meshes = new List<EditorPrimitive>();
            SelectionList = new List<EditorPrimitive>();
            PickingRay = new TgcPickingRay();
            Grid = new Grid(this);
            SelectionRectangle = new SelectionRectangle(this);
            CreatingPrimitive = null;
            primitiveNameCounter = 0;
            CurrentGizmo = null;
            MediaPath = GuiController.Instance.ExamplesMediaDir + "MeshCreator\\";
            defaultTexturePath = MediaPath + "Textures\\Madera\\cajaMadera1.jpg";
            checkBoxShowObjectsBoundingBox.Checked = true;
            PopupOpened = false;
            fpsCameraEnabled = false;
            lastSavePath = null;

            //meshBrowser
            //defaultMeshPath = mediaPath + "Meshes\\Vegetacion\\Arbusto\\Arbusto-TgcScene.xml";
            defaultMeshPath = MediaPath + "\\Meshes\\Vegetacion";
            meshBrowser = new TgcMeshBrowser();
            meshBrowser.setSelectedMesh(defaultMeshPath);

            //Export scene dialog
            exportSceneSaveDialog = new SaveFileDialog();
            exportSceneSaveDialog.DefaultExt = ".xml";
            exportSceneSaveDialog.Filter = ".XML |*.xml";
            exportSceneSaveDialog.AddExtension = true;
            exportSceneSaveDialog.Title = "Export scene to a -TgcScene.xml file";

            //Camara
            Camera = new MeshCreatorCamera();
            Camera.Enable = true;
            Camera.setCamera(new Vector3(0, 0, 0), 500);
            Camera.BaseRotX = -FastMath.PI / 4f;
            CamaraManager.Instance.CurrentCamera.Enable = false;

            //Gizmos
            translateGizmo = new TranslateGizmo(this);
            scaleGizmo = new ScaleGizmo(this);

            //Tab inicial
            tabControl.SelectedTab = tabControl.TabPages["tabPageCreate"];
            CurrentState = State.SelectObject;
            radioButtonSelectObject.Checked = true;

            //Tab Create
            textBoxCreateCurrentLayer.Text = "Default";

            //Tab Modify
            textureBrowser = new TgcTextureBrowser();
            textureBrowser.ShowFolders = true;
            textureBrowser.setSelectedImage(defaultTexturePath);
            textureBrowser.AsyncModeEnable = true;
            textureBrowser.OnSelectImage += textureBrowser_OnSelectImage;
            textureBrowser.OnClose += textureBrowser_OnClose;
            pictureBoxModifyTexture.ImageLocation = defaultTexturePath;
            pictureBoxModifyTexture.Image = MeshCreatorUtils.getImage(defaultTexturePath);
            updateModifyPanel();

            //ObjectPosition Text
            objectPositionText = new TgcText2d();
            objectPositionText.Align = TgcText2d.TextAlign.LEFT;
            objectPositionText.Color = Color.Yellow;
            objectPositionText.Size = new Size(500, 12);
            objectPositionText.Position = new Point(
                GuiController.Instance.Panel3d.Width - objectPositionText.Size.Width,
                GuiController.Instance.Panel3d.Height - 20);

            //Snap to grid
            SnapToGridCellSize = (float)numericUpDownCellSize.Value;

            //ObjectBrowser
            objectBrowser = new ObjectBrowser(this);

            //TextureBrowser para EditablePoly
            textureBrowserEPoly = new TgcTextureBrowser();
            textureBrowserEPoly.ShowFolders = true;
            textureBrowserEPoly.setSelectedImage(defaultTexturePath);
            textureBrowserEPoly.AsyncModeEnable = true;
            textureBrowserEPoly.OnSelectImage += textureBrowserEPoly_OnSelectImage;
            textureBrowserEPoly.OnClose += textureBrowserEPoly_OnClose;

            //Tooltips
            toolTips.SetToolTip(radioButtonSelectObject, "Select object (Q)");
            toolTips.SetToolTip(buttonSelectAll, "Select all objects (CTRL + E)");
            toolTips.SetToolTip(checkBoxShowObjectsBoundingBox, "Show objects BoundingBox");
            toolTips.SetToolTip(checkBoxSnapToGrid, "Toogle snap to grid");
            toolTips.SetToolTip(numericUpDownCellSize, "Snap to grid cell size");
            toolTips.SetToolTip(buttonZoomObject, "Zoom selected object (Z)");
            toolTips.SetToolTip(buttonHideSelected, "Hide selected objects (H)");
            toolTips.SetToolTip(buttonUnhideAll, "Unhide all hidden objects");
            toolTips.SetToolTip(buttonDeleteObject, "Delete selected objects (DEL)");
            toolTips.SetToolTip(buttonCloneObject, "Clone selected objects (CTRL + V)");
            toolTips.SetToolTip(radioButtonFPSCamera, "Toogle First-person camera (C)");
            toolTips.SetToolTip(numericUpDownFPSCameraSpeed, "First-person camera speed factor");
            toolTips.SetToolTip(buttonObjectBrowser, "Open Object Browser (O)");
            toolTips.SetToolTip(buttonTopView, "Set camera in Top-view (T)");
            toolTips.SetToolTip(buttonLeftView, "Set camera in Left-view (L)");
            toolTips.SetToolTip(buttonMergeSelected, "Merge selected objects (G)");
            toolTips.SetToolTip(buttonSaveScene, "Save scene in last used path (CTRL + S)");
            toolTips.SetToolTip(buttonSaveSceneAs, "Save scene in a new path (CTRL + SHIFT + S)");
            toolTips.SetToolTip(checkBoxAttachExport, "If selected all the scene is exported as one single mesh");
            toolTips.SetToolTip(buttonHelp, "Open Help (F1)");

            toolTips.SetToolTip(radioButtonPrimitive_Box, "Create a new Box (B)");
            toolTips.SetToolTip(radioButtonPrimitive_Sphere, "Create a new Sphere");
            toolTips.SetToolTip(radioButtonPrimitive_PlaneXZ, "Create a new XZ-plane (P)");
            toolTips.SetToolTip(radioButtonPrimitive_PlaneXY, "Create a new XY-plane");
            toolTips.SetToolTip(radioButtonPrimitive_PlaneYZ, "Create a new YZ-plane");
            toolTips.SetToolTip(buttonImportMesh, "Import an existing mesh (M)");
            toolTips.SetToolTip(textBoxCreateCurrentLayer, "Default layer for new created objects");

            toolTips.SetToolTip(buttonModifyConvertToMesh, "Conver to Mesh primitive");
            toolTips.SetToolTip(radioButtonModifySelectAndMove, "Move selected objects (W)");
            toolTips.SetToolTip(radioButtonModifySelectAndRotate, "Rotate selected objects (E)");
            toolTips.SetToolTip(radioButtonModifySelectAndScale, "Scale selected objects (R)");
            toolTips.SetToolTip(pictureBoxModifyTexture, "Change primitive texture");
            toolTips.SetToolTip(buttonModifyRecomputeAABB, "Compute a new BoundingBox for the primitive");

            toolTips.SetToolTip(radioButtonEPolyPrimitiveVertex, "Vertex primitve");
            toolTips.SetToolTip(radioButtonEPolyPrimitiveEdge, "Edge primitve");
            toolTips.SetToolTip(radioButtonEPolyPrimitivePolygon, "Polygon primitve");
            toolTips.SetToolTip(radioButtonEPolySelect, "Select primitive (Q)");
            toolTips.SetToolTip(buttonEPolySelectAll, "Select all primitives (CTRL + E)");
            toolTips.SetToolTip(radioButtonEPolyTranslate, "Move selected primitives (W)");
            toolTips.SetToolTip(buttonEPolyDelete, "Delete selected primitives (DEL)");
            toolTips.SetToolTip(buttonEPolyAddTexture, "Add new texture to mesh");
            toolTips.SetToolTip(buttonEPolyDeleteTexture, "Remove current texture");
            toolTips.SetToolTip(pictureBoxEPolyTexture, "Change current texture");
        }

        /// <summary>
        ///     Objetos del escenario
        /// </summary>
        public List<EditorPrimitive> Meshes { get; }

        /// <summary>
        ///     Objetos seleccionados
        /// </summary>
        public List<EditorPrimitive> SelectionList { get; }

        /// <summary>
        ///     Grid
        /// </summary>
        public Grid Grid { get; }

        /// <summary>
        ///     Camara
        /// </summary>
        public MeshCreatorCamera Camera { get; }

        /// <summary>
        ///     PickingRay
        /// </summary>
        public TgcPickingRay PickingRay { get; }

        /// <summary>
        ///     Estado actual
        /// </summary>
        public State CurrentState { get; set; }

        /// <summary>
        ///     Primitiva actual seleccionada para crear
        /// </summary>
        public EditorPrimitive CreatingPrimitive { get; set; }

        /// <summary>
        ///     Gizmo seleccionado actualmente
        /// </summary>
        public EditorGizmo CurrentGizmo { get; set; }

        /// <summary>
        ///     Rectangulo de seleccion
        /// </summary>
        public SelectionRectangle SelectionRectangle { get; }

        /// <summary>
        ///     Archivos de Media propios del editor
        /// </summary>
        public string MediaPath { get; }

        /// <summary>
        ///     Mostrar AABB de los objetos no seleccionados
        /// </summary>
        public bool ShowObjectsAABB
        {
            get { return checkBoxShowObjectsBoundingBox.Checked; }
        }

        /// <summary>
        ///     Hacer Snap to Grid
        /// </summary>
        public bool SnapToGridEnabled
        {
            get { return checkBoxSnapToGrid.Checked; }
        }

        /// <summary>
        ///     Tamaño de celda de Snap to grid
        /// </summary>
        public float SnapToGridCellSize { get; private set; }

        /// <summary>
        ///     True si hay un popup abierto y hay que evitar eventos
        /// </summary>
        public bool PopupOpened { get; set; }

        /// <summary>
        ///     Layer default actual para crear nuevos objetos
        /// </summary>
        public string CurrentLayer
        {
            get { return textBoxCreateCurrentLayer.Text; }
        }

        /// <summary>
        ///     Indica si hay que escalar para ambas direcciones o solo una
        /// </summary>
        public bool ScaleBothDirections
        {
            get { return checkBoxModifyBothDir.Checked; }
        }

        /// <summary>
        ///     Flag para ignorar eventos de UI
        /// </summary>
        public bool IgnoreChangeEvents { get; set; }

        /// <summary>
        ///     Ciclo loop del editor
        /// </summary>
        public void render()
        {
            //Hacer update de estado salvo que haya un popup abierto
            if (!PopupOpened)
            {
                //Modo camara FPS
                if (fpsCameraEnabled)
                {
                    doFpsCameraMode();
                }
                //Modo normal
                else
                {
                    //Procesar shorcuts de teclado
                    processShortcuts();

                    //Actualizar camara
                    updateCamera();

                    //Maquina de estados
                    switch (CurrentState)
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

                        case State.EditablePoly:
                            doEditablePoly();
                            break;
                    }
                }
            }

            //Dibujar objetos del escenario (siempre, aunque no haya foco)
            renderObjects();

            //Dibujar gizmo (sin Z-Buffer, al final de tod)
            if (CurrentGizmo != null && SelectionList.Count > 0)
            {
                CurrentGizmo.render();
            }
        }

        /// <summary>
        ///     Procesar shorcuts de teclado
        /// </summary>
        private void processShortcuts()
        {
            //Solo en estados pasivos
            if (CurrentState == State.SelectObject || CurrentState == State.CreatePrimitiveSelected
                || CurrentState == State.GizmoActivated || CurrentState == State.EditablePoly)
            {
                var input = GuiController.Instance.D3dInput;

                //Acciones que no se pueden hacer si estamos en modo EditablePoly
                if (CurrentState != State.EditablePoly)
                {
                    //Hide
                    if (input.keyPressed(Key.H))
                    {
                        buttonHideSelected_Click(null, null);
                    }
                    //Select
                    else if (input.keyPressed(Key.Q))
                    {
                        radioButtonSelectObject.Checked = true;
                    }
                    //Select all
                    else if (input.keyDown(Key.LeftControl) && input.keyPressed(Key.E))
                    {
                        buttonSelectAll_Click(null, null);
                    }
                    //Delete
                    else if (input.keyPressed(Key.Delete))
                    {
                        buttonDeleteObject_Click(null, null);
                    }
                    //Translate
                    else if (input.keyPressed(Key.W))
                    {
                        if (radioButtonModifySelectAndMove.Checked)
                        {
                            radioButtonModifySelectAndMove_CheckedChanged(null, null);
                        }
                        else
                        {
                            radioButtonModifySelectAndMove.Checked = true;
                        }
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
                    //Camara FPS
                    else if (input.keyPressed(Key.C))
                    {
                        radioButtonFPSCamera.Checked = true;
                    }
                    //Object browser
                    else if (input.keyPressed(Key.O))
                    {
                        buttonObjectBrowser_Click(null, null);
                    }
                    //Merge selected
                    else if (input.keyPressed(Key.G))
                    {
                        buttonMergeSelected_Click(null, null);
                    }
                    //Zoom
                    else if (input.keyPressed(Key.Z))
                    {
                        buttonZoomObject_Click(null, null);
                    }
                    //Top view
                    else if (input.keyPressed(Key.T))
                    {
                        buttonTopView_Click(null, null);
                    }
                    //Left view
                    else if (input.keyPressed(Key.L))
                    {
                        buttonLeftView_Click(null, null);
                    }
                    //Front view
                    else if (input.keyPressed(Key.F))
                    {
                        buttonFrontView_Click(null, null);
                    }
                }

                //Save as
                if (input.keyDown(Key.LeftControl) && input.keyDown(Key.LeftShift) && input.keyPressed(Key.S))
                {
                    buttonSaveSceneAs_Click(null, null);
                }
                //Save
                else if (input.keyDown(Key.LeftControl) && input.keyPressed(Key.S))
                {
                    buttonSaveScene_Click(null, null);
                }
                //Help
                else if (input.keyPressed(Key.F1))
                {
                    buttonHelp_Click(null, null);
                }
            }
        }

        /// <summary>
        ///     Dibujar todos los objetos
        /// </summary>
        private void renderObjects()
        {
            //Objetos opacos
            foreach (var mesh in Meshes)
            {
                if (!mesh.AlphaBlendEnable)
                {
                    if (mesh.Visible)
                    {
                        mesh.render();
                        if ((ShowObjectsAABB || mesh.Selected) && !(CurrentState == State.EditablePoly))
                        {
                            mesh.BoundingBox.render();
                        }
                    }
                }
            }

            //Grid
            Grid.render();

            //Objeto que se esta construyendo actualmente
            if (CurrentState == State.CreatingPrimitve)
            {
                CreatingPrimitive.render();
            }

            //Recuadro de seleccion
            if (CurrentState == State.SelectingObject)
            {
                SelectionRectangle.render();
            }

            //Objetos transparentes
            foreach (var mesh in Meshes)
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
            if (SelectionList.Count > 0)
            {
                var text = SelectionList.Count > 1
                    ? SelectionList[0].Name + " + " + (SelectionList.Count - 1) + " others"
                    : SelectionList[0].Name;
                text += ", Pos: " + TgcParserUtils.printVector3(SelectionList[0].Position);
                objectPositionText.Text = text;
                objectPositionText.render();
            }
        }

        /// <summary>
        ///     Actualizar camara segun movimientos
        /// </summary>
        private void updateCamera()
        {
            //Ajustar velocidad de zoom segun distancia a objeto
            Vector3 q;
            if (SelectionList.Count > 0)
            {
                q = SelectionList[0].BoundingBox.PMin;
            }
            else
            {
                q = Vector3.Empty;
            }
            Camera.ZoomFactor = MeshCreatorUtils.getMouseZoomSpeed(Camera, q);
            var elapsedTime = GuiController.Instance.ElapsedTime;
            Camera.updateCamera(elapsedTime);
            Camera.updateViewMatrix(D3DDevice.Instance.Device);
        }

        /// <summary>
        ///     Modo camara FPS
        /// </summary>
        private void doFpsCameraMode()
        {
            //No hay actualizar la camara FPS, la actualiza GuiController

            //Detectar si hay que salor de este modo
            var input = GuiController.Instance.D3dInput;
            if (input.keyPressed(Key.F))
            {
                radioButtonFPSCamera.Checked = false;
            }
        }

        /// <summary>
        ///     Estado: seleccionar objetos (estado default)
        /// </summary>
        private void doSelectObject()
        {
            SelectionRectangle.doSelectObject();
        }

        /// <summary>
        ///     Estado: Cuando se esta arrastrando el mouse para armar el cuadro de seleccion
        /// </summary>
        private void doSelectingObject()
        {
            SelectionRectangle.render();
        }

        /// <summary>
        ///     Estado: cuando se hizo clic en algun boton de primitiva para crear
        /// </summary>
        private void doCreatePrimitiveSelected()
        {
            var input = GuiController.Instance.D3dInput;

            //Quitar gizmo actual
            CurrentGizmo = null;

            //Si hacen clic con el mouse, iniciar creacion de la primitiva
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                var gridPoint = Grid.getPicking();
                CreatingPrimitive.initCreation(gridPoint);
                CurrentState = State.CreatingPrimitve;
            }
        }

        /// <summary>
        ///     Estado: mientras se esta creando una primitiva
        /// </summary>
        private void doCreatingPrimitve()
        {
            CreatingPrimitive.doCreation();
        }

        /// <summary>
        ///     Estado: se traslada, rota o escala un objeto
        /// </summary>
        private void doGizmoActivated()
        {
            if (SelectionList.Count > 0)
            {
                CurrentGizmo.update();
            }
        }

        /// <summary>
        ///     Estado: hay un objeto en modo editablePoly
        /// </summary>
        private void doEditablePoly()
        {
            var p = (MeshPrimitive)SelectionList[0];
            p.doEditablePolyUpdate();
        }

        /// <summary>
        ///     Agregar mesh creado
        /// </summary>
        public void addMesh(EditorPrimitive mesh)
        {
            Meshes.Add(mesh);
        }

        /// <summary>
        ///     Textura para crear un nuevo objeto
        /// </summary>
        /// <returns></returns>
        public string getCreationTexturePath()
        {
            return pictureBoxModifyTexture.ImageLocation;
        }

        /// <summary>
        ///     Nombre para crear una nueva primitiva
        /// </summary>
        public string getNewPrimitiveName(string type)
        {
            return type + "_" + primitiveNameCounter++;
        }

        /// <summary>
        ///     Eliminar los objetos especificados
        /// </summary>
        public void deleteObjects(List<EditorPrimitive> objectsToDelete)
        {
            foreach (var p in objectsToDelete)
            {
                if (p.Selected)
                {
                    SelectionList.Remove(p);
                }
                Meshes.Remove(p);
                p.dispose();
            }

            //Actualizar panel de modifiy
            updateModifyPanel();

            //Quitar gizmo actual
            CurrentGizmo = null;

            //Pasar a modo seleccion
            CurrentState = State.SelectObject;
        }

        /// <summary>
        ///     Eliminar todos los objetos seleccionados
        /// </summary>
        public void deleteSelectedObjects()
        {
            foreach (var p in SelectionList)
            {
                Meshes.Remove(p);
                p.dispose();
            }

            //Limpiar lista de seleccion
            SelectionList.Clear();
            updateModifyPanel();

            //Quitar gizmo actual
            CurrentGizmo = null;

            //Pasar a modo seleccion
            CurrentState = State.SelectObject;
        }

        /// <summary>
        ///     Mostrar u ocultar una lista de objetos.
        ///     Si estaban seleccionados y se ocultan los quita de la lista de seleccion
        /// </summary>
        public void showHideObjects(List<EditorPrimitive> objects, bool show)
        {
            if (objects.Count > 0)
            {
                foreach (var p in objects)
                {
                    //Mostrar
                    if (show)
                    {
                        p.Visible = true;
                    }
                    //Ocultar
                    else
                    {
                        p.Visible = false;
                        //Si estaba seleccionado entonces quitar seleccion
                        if (p.Selected)
                        {
                            p.setSelected(false);
                            SelectionList.Remove(p);
                        }
                    }
                }

                updateModifyPanel();

                //Quitar gizmo actual
                CurrentGizmo = null;

                //Pasar a modo seleccion
                CurrentState = State.SelectObject;
            }
        }

        /// <summary>
        ///     Cargar Tab de Modify cuando hay un objeto seleccionado
        /// </summary>
        public void updateModifyPanel()
        {
            IgnoreChangeEvents = true;
            if (SelectionList.Count >= 1)
            {
                var onlyOneObjectFlag = SelectionList.Count == 1;
                var isMeshFlag = SelectionList[0].GetType().IsAssignableFrom(typeof(MeshPrimitive));

                //Habilitar paneles
                groupBoxModifyGeneral.Enabled = true;
                groupBoxModifyTexture.Enabled = true;
                groupBoxModifyPosition.Enabled = true;
                groupBoxModifyRotation.Enabled = true;
                groupBoxModifyScale.Enabled = true;
                groupBoxModifyUserProps.Enabled = true;
                buttonModifyConvertToMesh.Enabled = !isMeshFlag;
                groupBoxEPolyPrimitive.Enabled = onlyOneObjectFlag && isMeshFlag;
                groupBoxEPolyCommon.Enabled = false;
                groupBoxEPolyEditVertices.Enabled = false;
                groupBoxEPolyEditEdges.Enabled = false;
                groupBoxEPolyEditPolygons.Enabled = false;

                //Cargar valores generales
                var p = SelectionList[0];
                var caps = p.ModifyCaps;
                textBoxModifyName.Text = p.Name;
                textBoxModifyName.Enabled = onlyOneObjectFlag;
                textBoxModifyLayer.Text = p.Layer;

                //Cargar textura
                if (caps.ChangeTexture)
                {
                    numericUpDownModifyTextureNumber.Enabled = true;
                    numericUpDownModifyTextureNumber.Minimum = 1;
                    numericUpDownModifyTextureNumber.Maximum = caps.TextureNumbers;
                    numericUpDownModifyTextureNumber.Value = 1;
                    pictureBoxModifyTexture.Enabled = true;
                    if (pictureBoxModifyTexture.Image != null)
                    {
                        pictureBoxModifyTexture.Image.Dispose();
                    }
                    pictureBoxModifyTexture.Image = Image.FromFile(p.getTexture(0).FilePath);
                    pictureBoxModifyTexture.ImageLocation = p.getTexture(0).FilePath;
                    //textureBrowser.setSelectedImage(p.getTexture(0).FilePath);
                    checkBoxModifyAlphaBlendEnabled.Enabled = true;
                    checkBoxModifyAlphaBlendEnabled.Checked = p.AlphaBlendEnable;
                }
                else
                {
                    numericUpDownModifyTextureNumber.Enabled = true;
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
                    var scale = p.Scale;
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
                userInfo.Enabled = onlyOneObjectFlag;

                //Rotation pivot (remains the same while the group does not change)
                rotationPivot = SelectionRectangle.getRotationPivot();
            }
            else
            {
                //Deshabilitar paneles
                groupBoxModifyGeneral.Enabled = false;
                groupBoxModifyTexture.Enabled = false;
                groupBoxModifyPosition.Enabled = false;
                groupBoxModifyRotation.Enabled = false;
                groupBoxModifyScale.Enabled = false;
                groupBoxModifyUserProps.Enabled = false;
                buttonModifyConvertToMesh.Enabled = false;
                groupBoxEPolyPrimitive.Enabled = false;
                groupBoxEPolyCommon.Enabled = false;
                groupBoxEPolyEditVertices.Enabled = false;
                groupBoxEPolyEditEdges.Enabled = false;
                groupBoxEPolyEditPolygons.Enabled = false;
            }

            IgnoreChangeEvents = false;
        }

        /// <summary>
        ///     Liberar recursos
        /// </summary>
        public void close()
        {
            foreach (var mesh in Meshes)
            {
                mesh.dispose();
            }
            Grid.dispose();
            SelectionRectangle.dispose();
            if (CreatingPrimitive != null)
            {
                SelectionRectangle.dispose();
            }
            translateGizmo.dipose();
            scaleGizmo.dipose();
            textureBrowser.Close();
            textureBrowserEPoly.Close();
            CamaraManager.Instance.CurrentCamera.Enable = true;
        }

        #region Eventos generales

        /// <summary>
        ///     Desactivar todos los radios
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
        ///     Cambiar de tab
        /// </summary>
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Estabamos en el tab de editablePoly y salimos
            if (CurrentState == State.EditablePoly &&
                tabControl.SelectedTab != tabControl.TabPages["tabPageEditablePoly"])
            {
                setEditablePolyEnable(false, EditablePoly.EditablePoly.PrimitiveType.None);
            }
        }

        /// <summary>
        ///     Clic en "Crear Box"
        /// </summary>
        private void radioButtonPrimitive_Box_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_Box.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_Box);
                CurrentState = State.CreatePrimitiveSelected;
                CreatingPrimitive = new BoxPrimitive(this);
            }
        }

        /// <summary>
        ///     Clic en "Crear Sphere"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButtonPrimitive_Sphere_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_Sphere.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_Sphere);
                CurrentState = State.CreatePrimitiveSelected;
                CreatingPrimitive = new SpherePrimitive(this);
            }
        }

        /// <summary>
        ///     Clic en "Crear Plano XZ"
        /// </summary>
        private void radioButtonPrimitive_PlaneXZ_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneXZ.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneXZ);
                CurrentState = State.CreatePrimitiveSelected;
                CreatingPrimitive = new PlaneXZPrimitive(this);
            }
        }

        /// <summary>
        ///     Clic en "Crear Plano XY"
        /// </summary>
        private void radioButtonPrimitive_PlaneXY_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneXY.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneXY);
                CurrentState = State.CreatePrimitiveSelected;
                CreatingPrimitive = new PlaneXYPrimitive(this);
            }
        }

        /// <summary>
        ///     Clic en "Crear Plano YZ"
        /// </summary>
        private void radioButtonPrimitive_PlaneYZ_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPrimitive_PlaneYZ.Checked)
            {
                untoggleAllRadioButtons(radioButtonPrimitive_PlaneYZ);
                CurrentState = State.CreatePrimitiveSelected;
                CreatingPrimitive = new PlaneYZPrimitive(this);
            }
        }

        /// <summary>
        ///     Clic en "Seleccionar objeto"
        /// </summary>
        private void radioButtonSelectObject_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelectObject.Checked)
            {
                setSelectObjectState();
            }
        }

        /// <summary>
        ///     Setear estado de Seleccion de Objetos
        /// </summary>
        public void setSelectObjectState()
        {
            untoggleAllRadioButtons(radioButtonSelectObject);
            CurrentState = State.SelectObject;
            CreatingPrimitive = null;
            CurrentGizmo = null;
        }

        /// <summary>
        ///     Clic en "Select all"
        /// </summary>
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            SelectionRectangle.selectAll();
        }

        /// <summary>
        ///     Camiar checkbox de "Snap to grid"
        /// </summary>
        private void checkBoxSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownCellSize.Enabled = checkBoxSnapToGrid.Checked;
        }

        /// <summary>
        ///     Cambiar tamaño de "Cell size"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownCellSize_ValueChanged(object sender, EventArgs e)
        {
            SnapToGridCellSize = (float)numericUpDownCellSize.Value;
        }

        /// <summary>
        ///     Clic en "Zoom"
        /// </summary>
        private void buttonZoomObject_Click(object sender, EventArgs e)
        {
            SelectionRectangle.zoomObject();
        }

        /// <summary>
        ///     Clic en "Hide selected"
        /// </summary>
        private void buttonHideSelected_Click(object sender, EventArgs e)
        {
            if (SelectionList.Count > 0)
            {
                var objectsToHide = new List<EditorPrimitive>(SelectionList);
                showHideObjects(objectsToHide, false);
            }
        }

        /// <summary>
        ///     Clic en "Unhide all""
        /// </summary>
        private void buttonUnhideAll_Click(object sender, EventArgs e)
        {
            foreach (var p in Meshes)
            {
                p.Visible = true;
            }
        }

        /// <summary>
        ///     Clic en "Delete object"
        /// </summary>
        private void buttonDeleteObject_Click(object sender, EventArgs e)
        {
            deleteSelectedObjects();
        }

        /// <summary>
        ///     Clic en "Clonar seleccion"
        /// </summary>
        private void buttonCloneObject_Click(object sender, EventArgs e)
        {
            if (SelectionList.Count > 0)
            {
                //Clonar toda la seleccion
                var cloneMeshes = new List<EditorPrimitive>();
                foreach (var p in SelectionList)
                {
                    var pClone = p.clone();
                    cloneMeshes.Add(pClone);
                }

                //Agregar al escenario y seleccionarlas
                SelectionRectangle.clearSelection();
                foreach (var pClone in cloneMeshes)
                {
                    Meshes.Add(pClone);
                    SelectionRectangle.selectObject(pClone);
                }
                CurrentState = State.SelectObject;
                SelectionRectangle.activateCurrentGizmo();
                updateModifyPanel();
            }
        }

        /// <summary>
        ///     Clic en "FPS camera"
        /// </summary>
        private void radioButtonFPSCamera_CheckedChanged(object sender, EventArgs e)
        {
            doClicFPSCamera(radioButtonFPSCamera.Checked);
        }

        /// <summary>
        ///     Accion de clic en "FPS camera"
        /// </summary>
        private void doClicFPSCamera(bool enabled)
        {
            if (enabled)
            {
                //Limpiar lista de seleccion
                SelectionRectangle.clearSelection();
                updateModifyPanel();

                //Quitar gizmo actual
                CurrentGizmo = null;

                //Pasar a modo seleccion
                CurrentState = State.SelectObject;

                //Activar modo FPS
                fpsCameraEnabled = true;
                GuiController.Instance.FpsCamera.Enable = true;
                GuiController.Instance.FpsCamera.setCamera(Camera.getPosition(), Camera.getLookAt());
            }
            else
            {
                //Volver al modo normal
                fpsCameraEnabled = false;
                GuiController.Instance.FpsCamera.Enable = false;

                //Acomodar camara de editor para centrar donde quedo la camara FPS
                Camera.CameraCenter = GuiController.Instance.FpsCamera.getPosition();
                //camera.CameraDistance = 10;
            }
        }

        /// <summary>
        ///     Clic en "Speed de FPS Camera"
        /// </summary>
        private void numericUpDownFPSCameraSpeed_ValueChanged(object sender, EventArgs e)
        {
            //Multiplicar velocidad de camara
            var speed = (float)numericUpDownFPSCameraSpeed.Value;
            GuiController.Instance.FpsCamera.MovementSpeed = TgcFpsCamera.DEFAULT_MOVEMENT_SPEED * speed;
            GuiController.Instance.FpsCamera.JumpSpeed = TgcFpsCamera.DEFAULT_JUMP_SPEED * speed;
        }

        /// <summary>
        ///     Clic en "Importar Mesh"
        /// </summary>
        private void buttonImportMesh_Click(object sender, EventArgs e)
        {
            var r = meshBrowser.ShowDialog();
            if (r == DialogResult.OK)
            {
                try
                {
                    //Cargar scene
                    var path = meshBrowser.SelectedMesh;
                    var loader = new TgcSceneLoader();
                    var scene = loader.loadSceneFromFile(path);

                    //Agregar meshes al escenario y tambien seleccionarlas
                    SelectionRectangle.clearSelection();
                    foreach (var mesh in scene.Meshes)
                    {
                        var p = new MeshPrimitive(this, mesh);
                        Meshes.Add(p);
                        SelectionRectangle.selectObject(p);
                    }
                    CurrentState = State.SelectObject;
                    SelectionRectangle.activateCurrentGizmo();
                    updateModifyPanel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Hubo un error al importar el mesh." + "Error: " + ex.Message + " - " +
                        ex.InnerException.Message,
                        "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        ///     Clic en "Object browser"
        /// </summary>
        private void buttonObjectBrowser_Click(object sender, EventArgs e)
        {
            objectBrowser.loadObjects("");
            PopupOpened = true;
            objectBrowser.Show(this);
        }

        /// <summary>
        ///     Clic en "Top view"
        /// </summary>
        private void buttonTopView_Click(object sender, EventArgs e)
        {
            SelectionRectangle.setTopView();
        }

        /// <summary>
        ///     Clic en "Front view"
        /// </summary>
        private void buttonFrontView_Click(object sender, EventArgs e)
        {
            SelectionRectangle.setFrontView();
        }

        /// <summary>
        ///     Clic en "Left view"
        /// </summary>
        private void buttonLeftView_Click(object sender, EventArgs e)
        {
            SelectionRectangle.setLeftView();
        }

        /// <summary>
        ///     Clic en "Merge Selected"
        /// </summary>
        private void buttonMergeSelected_Click(object sender, EventArgs e)
        {
            //Que haya mas de uno seleccionado
            if (SelectionList.Count > 1)
            {
                //Clonar objetos a mergear
                var objectsToMerge = new List<TgcMesh>();
                foreach (var p in SelectionList)
                {
                    var m = p.createMeshToExport();
                    objectsToMerge.Add(m);
                }

                //Eliminar los originales
                deleteSelectedObjects();

                //Hacer merge
                var exporter = new TgcSceneExporter();
                var mergedMesh = exporter.mergeMeshes(objectsToMerge);

                //Hacer dispose de los objetos clonados para mergear
                foreach (var m in objectsToMerge)
                {
                    m.dispose();
                }

                //Agregar nuevo mesh al escenario y seleccionarlo
                EditorPrimitive pMerged = new MeshPrimitive(this, mergedMesh);
                addMesh(pMerged);
                SelectionRectangle.selectObject(pMerged);
                CurrentState = State.SelectObject;
                SelectionRectangle.activateCurrentGizmo();
                updateModifyPanel();
            }
        }

        /// <summary>
        ///     Clic en "Save"
        /// </summary>
        private void buttonSaveScene_Click(object sender, EventArgs e)
        {
            if (lastSavePath != null)
            {
                exportScene(false, lastSavePath);
            }
            else
            {
                exportScene(true, null);
            }
        }

        /// <summary>
        ///     Clic en "Save as"
        /// </summary>
        private void buttonSaveSceneAs_Click(object sender, EventArgs e)
        {
            exportScene(true, null);
        }

        /// <summary>
        ///     Guardar la escena
        /// </summary>
        private void exportScene(bool askConfirmation, string path)
        {
            FileInfo fInfo = null;
            if (askConfirmation)
            {
                if (exportSceneSaveDialog.ShowDialog() == DialogResult.OK)
                {
                    fInfo = new FileInfo(exportSceneSaveDialog.FileName);
                    lastSavePath = exportSceneSaveDialog.FileName;
                }
            }
            else
            {
                fInfo = new FileInfo(path);
                exportSceneSaveDialog.FileName = path;
            }

            //Obtener directorio y nombre
            if (fInfo == null)
                return;
            var sceneName = fInfo.Name.Split('.')[0];
            sceneName = sceneName.Replace("-TgcScene", "");
            var saveDir = fInfo.DirectoryName;

            //Intentar guardar
            try
            {
                //Convertir todos los objetos del escenario a un TgcMesh y agregarlos a la escena a exportar
                var exportScene = new TgcScene(sceneName, saveDir);
                foreach (var p in Meshes)
                {
                    var m = p.createMeshToExport();
                    exportScene.Meshes.Add(m);
                }

                //Exportar escena
                var exporter = new TgcSceneExporter();
                TgcSceneExporter.ExportResult result;
                if (checkBoxAttachExport.Checked)
                {
                    result = exporter.exportAndAppendSceneToXml(exportScene, saveDir);
                }
                else
                {
                    result = exporter.exportSceneToXml(exportScene, saveDir);
                }

                //Hacer dispose de los objetos clonados para exportar
                exportScene.disposeAll();
                exportScene = null;

                //Mostrar resultado
                if (result.result)
                {
                    if (result.secondaryErrors)
                    {
                        MessageBox.Show(this,
                            "La escena se exportó OK pero hubo errores secundarios. " + result.listErrors(),
                            "Export Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Scene exported OK.", "Export Scene", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, "Ocurrieron errores al intentar exportar la escena. " + result.listErrors(),
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Hubo un error al intenar exportar la escena. Puede ocurrir que esté intentando reemplazar el mismo archivo de escena que tiene abierto ahora. Los archivos de Textura por ejemplo no pueden ser reemplazados si se están utilizando dentro del editor. En ese caso debera guardar en uno nuevo. "
                    + "Error: " + ex.Message + " - " + ex.InnerException.Message,
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///     Clic en "Help"
        /// </summary>
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            //Abrir PDF
            Process.Start(GuiController.Instance.ExamplesDir + "\\MeshCreator\\Guia MeshCreator.pdf");
        }

        #endregion Eventos generales

        #region Eventos de Modify

        /// <summary>
        ///     Clic en "Seleccionar y Mover"
        /// </summary>
        private void radioButtonModifySelectAndMove_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonModifySelectAndMove.Checked)
            {
                untoggleAllRadioButtons(radioButtonModifySelectAndMove);
                CurrentState = State.SelectObject;
                CreatingPrimitive = null;
                CurrentGizmo = translateGizmo;

                //Activar gizmo si hay seleccion
                if (SelectionList.Count > 0)
                {
                    SelectionRectangle.activateCurrentGizmo();
                }
            }
        }

        private void radioButtonModifySelectAndRotate_CheckedChanged(object sender, EventArgs e)
        {
            //TODO: implementar gizmo de rotacion
        }

        /// <summary>
        ///     Clic en "Seleccionar y Escalar"
        /// </summary>
        private void radioButtonModifySelectAndScale_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonModifySelectAndScale.Checked)
            {
                untoggleAllRadioButtons(radioButtonModifySelectAndScale);
                CurrentState = State.SelectObject;
                CreatingPrimitive = null;
                CurrentGizmo = scaleGizmo;

                //Activar gizmo si hay seleccion
                if (SelectionList.Count > 0)
                {
                    SelectionRectangle.activateCurrentGizmo();
                }
            }
        }

        /// <summary>
        ///     Cambiar nombre de mesh
        /// </summary>
        private void textBoxModifyName_Leave(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;
            SelectionList[0].Name = textBoxModifyName.Text;
        }

        /// <summary>
        ///     Cambiar nombre de layer
        /// </summary>
        private void textBoxModifyLayer_Leave(object sender, EventArgs e)
        {
            foreach (var p in SelectionList)
            {
                p.Layer = textBoxModifyLayer.Text;
            }
        }

        /// <summary>
        ///     Elegir otro numero de textura para editar
        /// </summary>
        private void numericUpDownModifyTextureNumber_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;
            var n = (int)numericUpDownModifyTextureNumber.Value - 1;

            //Cambiar imagen del pictureBox
            var img = MeshCreatorUtils.getImage(SelectionList[0].getTexture(n).FilePath);
            var lastImage = pictureBoxModifyTexture.Image;
            pictureBoxModifyTexture.Image = img;
            pictureBoxModifyTexture.ImageLocation = SelectionList[0].getTexture(n).FilePath;
            lastImage.Dispose();
        }

        /// <summary>
        ///     Hacer clic en el recuadro de textura para cambiarla
        /// </summary>
        private void pictureBoxModifyTexture_Click(object sender, EventArgs e)
        {
            PopupOpened = true;

            var n = (int)numericUpDownModifyTextureNumber.Value - 1;
            textureBrowser.setSelectedImage(SelectionList[0].getTexture(n).FilePath);

            textureBrowser.Show(this);
        }

        /// <summary>
        ///     Cuando se selecciona una imagen en el textureBrowser
        /// </summary>
        public void textureBrowser_OnSelectImage(TgcTextureBrowser textureBrowser)
        {
            if (IgnoreChangeEvents) return;

            //Cambiar la textura si es distinta a la que tenia el mesh
            var n = (int)numericUpDownModifyTextureNumber.Value - 1;
            if (textureBrowser.SelectedImage != SelectionList[0].getTexture(n).FilePath)
            {
                var img = MeshCreatorUtils.getImage(textureBrowser.SelectedImage);
                var lastImage = pictureBoxModifyTexture.Image;
                pictureBoxModifyTexture.Image = img;
                pictureBoxModifyTexture.ImageLocation = textureBrowser.SelectedImage;
                lastImage.Dispose();
                foreach (var p in SelectionList)
                {
                    p.setTexture(TgcTexture.createTexture(pictureBoxModifyTexture.ImageLocation), n);
                }
            }
        }

        /// <summary>
        ///     Cuando se cierra el textureBrowser
        /// </summary>
        public void textureBrowser_OnClose(TgcTextureBrowser textureBrowser)
        {
            PopupOpened = false;
        }

        /// <summary>
        ///     Cambiar Alpha Blend
        /// </summary>
        private void checkBoxModifyAlphaBlendEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            foreach (var p in SelectionList)
            {
                p.AlphaBlendEnable = checkBoxModifyAlphaBlendEnabled.Checked;
            }
        }

        /// <summary>
        ///     Cambiar offset U
        /// </summary>
        private void numericUpDownTextureOffsetU_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = (float)numericUpDownTextureOffsetU.Value;
            foreach (var p in SelectionList)
            {
                p.TextureOffset = new Vector2(value, p.TextureOffset.Y);
            }
        }

        /// <summary>
        ///     Cambiar offset V
        /// </summary>
        private void numericUpDownTextureOffsetV_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = (float)numericUpDownTextureOffsetV.Value;
            foreach (var p in SelectionList)
            {
                p.TextureOffset = new Vector2(p.TextureOffset.X, value);
            }
        }

        /// <summary>
        ///     Cambiar tile U
        /// </summary>
        private void numericUpDownTextureTilingU_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = (float)numericUpDownTextureTilingU.Value;
            foreach (var p in SelectionList)
            {
                p.TextureTiling = new Vector2(value, p.TextureTiling.Y);
            }
        }

        /// <summary>
        ///     Cambiar tile V
        /// </summary>
        private void numericUpDownTextureTilingV_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = (float)numericUpDownTextureTilingV.Value;
            foreach (var p in SelectionList)
            {
                p.TextureTiling = new Vector2(p.TextureTiling.X, value);
            }
        }

        /// <summary>
        ///     Cambiar posicion en X
        /// </summary>
        private void numericUpDownModifyPosX_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p = SelectionList[0];
            var oldPos = p.Position;
            p.Position = new Vector3((float)numericUpDownModifyPosX.Value, oldPos.Y, oldPos.Z);
            var movement = p.Position - oldPos;
            if (CurrentGizmo != null)
            {
                CurrentGizmo.move(p, movement);
            }
            for (var i = 1; i < SelectionList.Count; i++)
            {
                SelectionList[i].move(movement);
            }
        }

        /// <summary>
        ///     Cambiar posicion en Y
        /// </summary>
        private void numericUpDownModifyPosY_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p = SelectionList[0];
            var oldPos = p.Position;
            p.Position = new Vector3(oldPos.X, (float)numericUpDownModifyPosY.Value, oldPos.Z);
            var movement = p.Position - oldPos;
            if (CurrentGizmo != null)
            {
                CurrentGizmo.move(p, movement);
            }
            for (var i = 1; i < SelectionList.Count; i++)
            {
                SelectionList[i].move(movement);
            }
        }

        /// <summary>
        ///     Cambiar posicion en Z
        /// </summary>
        private void numericUpDownModifyPosZ_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p = SelectionList[0];
            var oldPos = p.Position;
            p.Position = new Vector3(oldPos.X, oldPos.Y, (float)numericUpDownModifyPosZ.Value);
            var movement = p.Position - oldPos;
            if (CurrentGizmo != null)
            {
                CurrentGizmo.move(p, movement);
            }
            for (var i = 1; i < SelectionList.Count; i++)
            {
                SelectionList[i].move(movement);
            }
        }

        /// <summary>
        ///     Cambiar rotacion en X
        /// </summary>
        private void numericUpDownModifyRotX_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = FastMath.ToRad((float)numericUpDownModifyRotX.Value);
            var pivot = rotationPivot /*selectionRectangle.getRotationPivot()*/;
            foreach (var p in SelectionList)
            {
                p.setRotationFromPivot(new Vector3(value, p.Rotation.Y, p.Rotation.Z), pivot);
            }
        }

        /// <summary>
        ///     Cambiar rotacion en Y
        /// </summary>
        private void numericUpDownModifyRotY_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = FastMath.ToRad((float)numericUpDownModifyRotY.Value);
            var pivot = rotationPivot /*selectionRectangle.getRotationPivot()*/;
            foreach (var p in SelectionList)
            {
                p.setRotationFromPivot(new Vector3(p.Rotation.X, value, p.Rotation.Z), pivot);
            }
        }

        /// <summary>
        ///     Cambiar rotacion en Z
        /// </summary>
        private void numericUpDownModifyRotZ_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var value = FastMath.ToRad((float)numericUpDownModifyRotZ.Value);
            var pivot = rotationPivot /*selectionRectangle.getRotationPivot()*/;
            foreach (var p in SelectionList)
            {
                p.setRotationFromPivot(new Vector3(p.Rotation.X, p.Rotation.Y, value), pivot);
            }
        }

        /// <summary>
        ///     Calcular "AABB"
        /// </summary>
        private void buttonModifyRecomputeAABB_Click(object sender, EventArgs e)
        {
            foreach (var p in SelectionList)
            {
                p.updateBoundingBox();
            }
            updateModifyPanel();
        }

        /// <summary>
        ///     Cambiar escala en X
        /// </summary>
        private void numericUpDownModifyScaleX_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p0 = SelectionList[0];
            var old = p0.Scale;
            var value = (float)numericUpDownModifyScaleX.Value / 100f;
            var diff = new Vector3(value, p0.Scale.Y, p0.Scale.Z) - old;

            foreach (var p in SelectionList)
            {
                if (checkBoxModifyBothDir.Checked)
                {
                    p.Scale += diff;
                }
                else
                {
                    var oldMin = p.BoundingBox.PMin;
                    p.Scale += diff;
                    p.move(oldMin - p.BoundingBox.PMin);
                }
            }
        }

        /// <summary>
        ///     Cambiar escala en Y
        /// </summary>
        private void numericUpDownModifyScaleY_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p0 = SelectionList[0];
            var old = p0.Scale;
            var value = (float)numericUpDownModifyScaleY.Value / 100f;
            var diff = new Vector3(p0.Scale.X, value, p0.Scale.Z) - old;

            foreach (var p in SelectionList)
            {
                if (checkBoxModifyBothDir.Checked)
                {
                    p.Scale += diff;
                }
                else
                {
                    var oldMin = p.BoundingBox.PMin;
                    p.Scale += diff;
                    p.move(oldMin - p.BoundingBox.PMin);
                }
            }
        }

        /// <summary>
        ///     Cambiar escala en Z
        /// </summary>
        private void numericUpDownModifyScaleZ_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p0 = SelectionList[0];
            var old = p0.Scale;
            var value = (float)numericUpDownModifyScaleZ.Value / 100f;
            var diff = new Vector3(p0.Scale.X, p0.Scale.Y, value) - old;

            foreach (var p in SelectionList)
            {
                if (checkBoxModifyBothDir.Checked)
                {
                    p.Scale += diff;
                }
                else
                {
                    var oldMin = p.BoundingBox.PMin;
                    p.Scale += diff;
                    p.move(oldMin - p.BoundingBox.PMin);
                }
            }
        }

        /// <summary>
        ///     Cambiar user properties
        /// </summary>
        private void userInfo_Leave(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;

            var p = SelectionList[0];
            p.UserProperties = MeshCreatorUtils.getUserPropertiesDictionary(userInfo.Text);
        }

        /// <summary>
        ///     Clic en "Convert to Mesh"
        /// </summary>
        private void buttonModifyConvertToMesh_Click(object sender, EventArgs e)
        {
            //Clonar objetos seleccionados
            var newObjects = new List<EditorPrimitive>();
            var objectToDelete = new List<EditorPrimitive>();
            foreach (var p in SelectionList)
            {
                var mesh = p.createMeshToExport();
                EditorPrimitive meshP = new MeshPrimitive(this, mesh);
                newObjects.Add(meshP);

                objectToDelete.Add(p);
            }

            //Eliminar objetos anteriores
            deleteObjects(objectToDelete);

            //Agregar nuevos objetos al escenario y seleccionarlos
            foreach (var p in newObjects)
            {
                addMesh(p);
                SelectionRectangle.selectObject(p);
            }
            CurrentState = State.SelectObject;
            SelectionRectangle.activateCurrentGizmo();
            updateModifyPanel();
        }

        #endregion Eventos de Modify

        #region EditablePoly

        /// <summary>
        ///     Setear estado EditablePoly
        /// </summary>
        /// <param name="enabled"></param>
        public void setEditablePolyEnable(bool enabled, EditablePoly.EditablePoly.PrimitiveType primitiveType)
        {
            if (enabled)
            {
                CurrentState = State.EditablePoly;
                groupBoxEPolyCommon.Enabled = true;
                CreatingPrimitive = null;
                CurrentGizmo = null;
                var m = (MeshPrimitive)SelectionList[0];
                m.enableEditablePoly(true, primitiveType);
            }
            else
            {
                if (CurrentState == State.EditablePoly)
                {
                    var m = (MeshPrimitive)SelectionList[0];
                    m.enableEditablePoly(false, primitiveType);

                    radioButtonEPolyPrimitiveVertex.Checked = false;
                    radioButtonEPolyPrimitiveEdge.Checked = false;
                    radioButtonEPolyPrimitivePolygon.Checked = false;
                    groupBoxEPolyCommon.Enabled = false;
                    groupBoxEPolyEditVertices.Enabled = false;
                    groupBoxEPolyEditEdges.Enabled = false;
                    groupBoxEPolyEditPolygons.Enabled = false;

                    setSelectObjectState();
                }
            }
        }

        /// <summary>
        ///     Clic en Vertex primitive
        /// </summary>
        private void radioButtonEPolyPrimitiveVertex_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEPolyPrimitiveVertex.Checked)
            {
                radioButtonEPolyPrimitiveVertex.BackColor = Color.Red;
                groupBoxEPolyEditVertices.Enabled = true;
                groupBoxEPolyEditEdges.Enabled = false;
                groupBoxEPolyEditPolygons.Enabled = false;
                setEditablePolyEnable(true, EditablePoly.EditablePoly.PrimitiveType.Vertex);
            }
            else
            {
                radioButtonEPolyPrimitiveVertex.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        ///     Clic en Edge primitive
        /// </summary>
        private void radioButtonEPolyPrimitiveEdge_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEPolyPrimitiveEdge.Checked)
            {
                radioButtonEPolyPrimitiveEdge.BackColor = Color.Red;
                groupBoxEPolyEditVertices.Enabled = false;
                groupBoxEPolyEditEdges.Enabled = true;
                groupBoxEPolyEditPolygons.Enabled = false;
                setEditablePolyEnable(true, EditablePoly.EditablePoly.PrimitiveType.Edge);
            }
            else
            {
                radioButtonEPolyPrimitiveEdge.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        ///     Clic en Polygon primitive
        /// </summary>
        private void radioButtonEPolyPrimitivePolygon_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEPolyPrimitivePolygon.Checked)
            {
                radioButtonEPolyPrimitivePolygon.BackColor = Color.Red;
                groupBoxEPolyEditVertices.Enabled = false;
                groupBoxEPolyEditEdges.Enabled = false;
                groupBoxEPolyEditPolygons.Enabled = true;
                setEditablePolyEnable(true, EditablePoly.EditablePoly.PrimitiveType.Polygon);
            }
            else
            {
                radioButtonEPolyPrimitivePolygon.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        ///     Clic en Select de EditablePoly
        /// </summary>
        private void radioButtonEPolySelect_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEPolySelect.Checked)
            {
                var editablePoly = ((MeshPrimitive)SelectionList[0]).EditablePoly;
                editablePoly.setSelectState();
            }
        }

        /// <summary>
        ///     Clic en Select All de EditablePoly
        /// </summary>
        private void buttonEPolySelectAll_Click(object sender, EventArgs e)
        {
            var editablePoly = ((MeshPrimitive)SelectionList[0]).EditablePoly;
            editablePoly.selectAll();
        }

        /// <summary>
        ///     Clic en Translate de EditablePoly
        /// </summary>
        private void radioButtonEPolyTranslate_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEPolyTranslate.Checked)
            {
                var editablePoly = ((MeshPrimitive)SelectionList[0]).EditablePoly;
                editablePoly.activateTranslateGizmo();
            }
        }

        /// <summary>
        ///     Clic en Delete de EditablePoly
        /// </summary>
        private void buttonEPolyDelete_Click(object sender, EventArgs e)
        {
            var editablePoly = ((MeshPrimitive)SelectionList[0]).EditablePoly;
            editablePoly.deleteSelectedPrimitives();
        }

        /// <summary>
        ///     Clic en pasar numero de textura de EditablePoly
        /// </summary>
        private void numericUpDownEPolyTextureNumber_ValueChanged(object sender, EventArgs e)
        {
            if (IgnoreChangeEvents) return;
            var editablePoly = ((MeshPrimitive)SelectionList[0]).EditablePoly;
            var n = (int)numericUpDownEPolyTextureNumber.Value - 1;

            //Cambiar imagen del pictureBox
            var img = MeshCreatorUtils.getImage(SelectionList[0].getTexture(n).FilePath);
            var lastImage = pictureBoxEPolyTexture.Image;
            pictureBoxEPolyTexture.Image = img;
            pictureBoxEPolyTexture.ImageLocation = SelectionList[0].getTexture(n).FilePath;
            lastImage.Dispose();

            //Aplicar cambiod de textura en EditablePoly
            editablePoly.changeTextureId(n);
        }

        /// <summary>
        ///     Agregar una nueva textura a EditablePoly
        /// </summary>
        private void buttonEPolyAddTexture_Click(object sender, EventArgs e)
        {
            //Clonar la primer textura y agregarsela al mesh (al final)
            var p = (MeshPrimitive)SelectionList[0];
            var newTexutre = p.getTexture(0).clone();
            p.addNexTexture(newTexutre);

            //Agregar nuevo slot de textura en UI
            //ignoreChangeEvents = true;
            numericUpDownEPolyTextureNumber.Maximum++;
            numericUpDownEPolyTextureNumber.Value = numericUpDownEPolyTextureNumber.Maximum;
            //ignoreChangeEvents = false;

            //Abrir el popup de seleccion de textura
            pictureBoxEPolyTexture_Click(null, null);
        }

        /// <summary>
        ///     Clic en Delete Texture de EditablePoly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEPolyDeleteTexture_Click(object sender, EventArgs e)
        {
            var p = (MeshPrimitive)SelectionList[0];
            if (p.ModifyCaps.TextureNumbers > 1)
            {
                var n = (int)numericUpDownEPolyTextureNumber.Value - 1;
                p.deleteTexture(n);
                p.EditablePoly.deleteTextureId(n, 0);
                numericUpDownEPolyTextureNumber.Maximum--;
                numericUpDownEPolyTextureNumber.Value = 1;
            }
        }

        /// <summary>
        ///     Clic en icono de Textura de EditablePoly para cambiarla
        /// </summary>
        private void pictureBoxEPolyTexture_Click(object sender, EventArgs e)
        {
            PopupOpened = true;

            var n = (int)numericUpDownEPolyTextureNumber.Value - 1;
            textureBrowserEPoly.setSelectedImage(SelectionList[0].getTexture(n).FilePath);

            textureBrowserEPoly.Show(this);
        }

        /// <summary>
        ///     Cuando se selecciona una imagen en el textureBrowser de EditablePoly
        /// </summary>
        public void textureBrowserEPoly_OnSelectImage(TgcTextureBrowser textureBrowser)
        {
            if (IgnoreChangeEvents) return;
            var p = SelectionList[0];

            //Cambiar la textura si es distinta a la que tenia el mesh
            var n = (int)numericUpDownEPolyTextureNumber.Value - 1;
            if (textureBrowserEPoly.SelectedImage != p.getTexture(n).FilePath)
            {
                var img = MeshCreatorUtils.getImage(textureBrowserEPoly.SelectedImage);
                var lastImage = pictureBoxEPolyTexture.Image;
                pictureBoxEPolyTexture.Image = img;
                pictureBoxEPolyTexture.ImageLocation = textureBrowserEPoly.SelectedImage;
                lastImage.Dispose();

                p.setTexture(TgcTexture.createTexture(pictureBoxEPolyTexture.ImageLocation), n);
            }
        }

        /// <summary>
        ///     Cuando se cierra el textureBrowser de EditablePoly
        /// </summary>
        public void textureBrowserEPoly_OnClose(TgcTextureBrowser textureBrowser)
        {
            PopupOpened = false;
        }

        #endregion EditablePoly
    }
}