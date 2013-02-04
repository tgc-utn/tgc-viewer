using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using System.IO;
using Microsoft.DirectX;
using SistPaquetesClient.core;
using TgcViewer.Utils.Terrain;
using System.Xml;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Input;

namespace Examples.SceneEditor
{
    /// <summary>
    /// Control de .NET para crear un Modifier personalizado para el ejemplo SceneEditor
    /// </summary>
    public partial class SceneEditorControl : UserControl
    {
        TgcSceneEditor editor;
        OpenFileDialog openMeshDialog;
        OpenFileDialog openHeighmapDialog;
        OpenFileDialog openTextureDialog;
        SaveFileDialog saveSceneDialog;
        TgcSceneLoader sceneLoader;
        TgcSceneParser parser;
        int meshCounter = 1;
        int meshGroupCounter = 0;
        string meshFilePath;
        string meshFolderName;
        string heighmapFilePath;
        string terrainTextureFilePath;
        GuiState currentState;
        TgcPickingRay pickingRay;
        SceneEditorTranslateGizmo translateGizmo;
        SceneEditorHelpWindow helpWindow;
        
        /// <summary>
        /// Terreno del escenario
        /// </summary>
        TgcSimpleTerrain tgcTerrain;
        public TgcSimpleTerrain TgcTerrain
        {
            get { return tgcTerrain; }
        } 

        List<SceneEditorMeshObject> meshObjects = new List<SceneEditorMeshObject>();
        /// <summary>
        /// Lista de todos los objetos del escenario
        /// </summary>
        public List<SceneEditorMeshObject> MeshObjects
        {
            get { return meshObjects; }
        }

        List<SceneEditorMeshObject> selectedMeshList;
        /// <summary>
        /// Lista de objetos del escenario que estan seleccionados 
        /// </summary>
        public List<SceneEditorMeshObject> SelectedMeshList
        {
            get { return selectedMeshList; }
        }




        public SceneEditorControl(TgcSceneEditor editor)
        {
            InitializeComponent();
            
            this.editor = editor;
            parser = new TgcSceneParser();
            sceneLoader = new TgcSceneLoader();
            pickingRay = new TgcPickingRay();
            translateGizmo = new SceneEditorTranslateGizmo();
            helpWindow = new SceneEditorHelpWindow();

            //openMeshDialog
            openMeshDialog = new OpenFileDialog();
            openMeshDialog.CheckFileExists = true;
            openMeshDialog.Title = "Seleccionar malla de formato TGC";
            openMeshDialog.Filter = "-TgcScene.xml |*-TgcScene.xml";
            openMeshDialog.Multiselect = false;
            //openMeshDialog.InitialDirectory = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\";
            openMeshDialog.FileName = GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\Box\\Box-TgcScene.xml";
            fileName.Text = "Box-TgcScene.xml";
            meshFilePath = openMeshDialog.FileName;

            //openHeighmapDialog
            openHeighmapDialog = new OpenFileDialog();
            openHeighmapDialog.CheckFileExists = true;
            openHeighmapDialog.Title = "Seleccionar Heighmap";
            openHeighmapDialog.Filter = ".JPG|*.jpg|.JPEG|*.jpeg|.GIF|*.gif|.PNG|*.png|.BMP|*.bmp | .TGA |*.tga";
            openHeighmapDialog.Multiselect = false;
            openHeighmapDialog.InitialDirectory = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\";

            //openHeighmapDialog
            openTextureDialog = new OpenFileDialog();
            openTextureDialog.CheckFileExists = true;
            openTextureDialog.Title = "Seleccionar textura de terreno";
            openTextureDialog.Filter = ".JPG|*.jpg|.JPEG|*.jpeg|.GIF|*.gif|.PNG|*.png|.BMP|*.bmp | .TGA |*.tga";
            openTextureDialog.Multiselect = false;
            openTextureDialog.InitialDirectory = GuiController.Instance.ExamplesMediaDir + "Heighmaps\\";

            //saveSceneDialog
            saveSceneDialog = new SaveFileDialog();
            saveSceneDialog.DefaultExt = ".xml";
            saveSceneDialog.Filter = ".XML |*.xml";
            saveSceneDialog.AddExtension = true;

            selectedMeshList = new List<SceneEditorMeshObject>();
            
            //Estado inicial
            currentState = GuiState.Nothing;
            tabControl.SelectedTab = tabControl.TabPages["tabPageCreate"];

            //Camara inicial
            GuiController.Instance.FpsCamera.setCamera(new Vector3(50.406f, 185.5353f, -143.7283f), new Vector3(-92.5515f, -567.6361f, 498.3744f));
        }

        /// <summary>
        /// Mostrar ventana de Help
        /// </summary>
        private void buttonHelp_Click_1(object sender, EventArgs e)
        {
            helpWindow.ShowDialog(this);
        }



        #region GUI States

        /// <summary>
        /// Estados de la interfaz grafica
        /// </summary>
        enum GuiState
        {
            Nothing,
            SelectionMode,
            CameraMode,
            TranslateMode,
        }


        /// <summary>
        /// Modo: SelectionMode
        /// </summary>
        private void radioButtonSelectionMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSelectionMode.Checked)
            {
                unselectAllModes(radioButtonSelectionMode);
                currentState = GuiState.SelectionMode;
            }
            else
            {
            }
        }

        /// <summary>
        /// Modo: CameraMode
        /// </summary>
        private void radioButtonCameraMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCameraMode.Checked)
            {
                unselectAllModes(radioButtonCameraMode);
                currentState = GuiState.CameraMode;
                GuiController.Instance.FpsCamera.Enable = true;
                cameraSpeed.Enabled = true;
            }
            else
            {
                GuiController.Instance.FpsCamera.Enable = false;
                cameraSpeed.Enabled = false;
            }
        }

        /// <summary>
        /// Modo: TranslateMode
        /// </summary>
        private void radioButtonTranslateMode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonTranslateMode.Checked)
            {
                unselectAllModes(radioButtonTranslateMode);
                currentState = GuiState.TranslateMode;
                if (selectedMeshList.Count > 0)
                {
                    translateGizmo.setMesh(selectedMeshList[0]);
                }
            }
            else
            {
                translateGizmo.hide();
            }
        }

        /// <summary>
        /// Desactiva todos los modos
        /// </summary>
        private void unselectAllModes(Control c)
        {
            currentState = GuiState.Nothing;

            if (!radioButtonSelectionMode.Equals(c))
                radioButtonSelectionMode.Checked = false;

            if (!radioButtonCameraMode.Equals(c))
                radioButtonCameraMode.Checked = false;

            if (!radioButtonTranslateMode.Equals(c))
                radioButtonTranslateMode.Checked = false;
        }


        #endregion


        #region Render Loop

        /// <summary>
        /// Render Loop
        /// </summary>
        internal void render()
        {
            //Frustum culling
            doFrustumCulling();

            //Input
            handleInput();

            //Renderizar terreno
            if (tgcTerrain != null)
            {
                tgcTerrain.render();
            }

            //Renderizar modelos visibles
            foreach (SceneEditorMeshObject meshObj in meshObjects)
            {
                if (meshObj.visible)
                {
                    meshObj.mesh.render();
                }
            }

            //Mostrar BoundingBox de modelos seleccionados
            foreach (SceneEditorMeshObject selectedMeshObj in selectedMeshList)
            {
                selectedMeshObj.mesh.BoundingBox.render();
            }

            //Renderizar gizmo
            translateGizmo.render();
        }

        /// <summary>
        /// Hacer FrustumCulling por fuerza bruta para detectar que modelos se ven
        /// </summary>
        private void doFrustumCulling()
        {
            TgcFrustum frustum = GuiController.Instance.Frustum;
            foreach (SceneEditorMeshObject meshObj in meshObjects)
            {
                TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(frustum, meshObj.mesh.BoundingBox);
                if (r == TgcCollisionUtils.FrustumResult.OUTSIDE)
                {
                    meshObj.visible = false;
                }
                else
                {
                    meshObj.visible = true;
                }
            }
        }

        /// <summary>
        /// Se cierra el ejemplo. Liberar recursos
        /// </summary>
        internal void close()
        {
            if (tgcTerrain != null)
            {
                tgcTerrain.dispose();
            }

            foreach (SceneEditorMeshObject meshObj in meshObjects)
            {
                meshObj.mesh.dispose();
            }
        }

        /// <summary>
        /// Manejar eventos de Input
        /// </summary>
        private void handleInput()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Seleccionar modelo
            if (currentState == GuiState.SelectionMode)
            {
                //Picking con mouse Izquierdo
                if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    findSelectedMeshWithPicking();
                }
            }
            //Trasladar modelo seleccionado
            else if(currentState == GuiState.TranslateMode)
            {
                if (selectedMeshList.Count > 0)
                {
                    //Comenzar a trasladar
                    if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        initGizmoDragging();
                    }
                    //Arrastrando gizmo
                    else if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        doGizmoTranslate();
                    }
                    //Terminar el traslado
                    else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                    {
                        endGizmoDragging();
                    }
                }
            }
        }

        #endregion
    

        #region CreateMesh

        /// <summary>
        /// Abrir archivo de Mesh, pero todavia no se parsea
        /// </summary>
        private void openFile_Click(object sender, EventArgs e)
        {
            if (openMeshDialog.ShowDialog() == DialogResult.OK)
            {
                meshFilePath = openMeshDialog.FileName;
                FileInfo fInfo = new FileInfo(meshFilePath);
                fileName.Text = fInfo.Name;
                meshFolderName = fInfo.Directory.Name;
            }
        }

        /// <summary>
        /// Crear el mesh y cargarlo en la tabla
        /// </summary>
        private void meshCreate_Click(object sender, EventArgs e)
        {
            if (meshFilePath == null || meshFilePath.Equals(""))
            {
                return;
            }

            try
            {
                //Parsear XML de mesh
                string mediaPath = meshFilePath.Substring(0, meshFilePath.LastIndexOf('\\') + 1);
                string xmlString = File.ReadAllText(meshFilePath);
                TgcSceneData sceneData = parser.parseSceneFromString(xmlString);


                //Crear el mesh tantas veces como se haya pedido
                int cantidad = (int)amountToCreate.Value;
                Vector3 initialPos = getInitialPos();
                for (int i = 0; i < cantidad; i++)
                {
                    TgcScene scene = sceneLoader.loadScene(sceneData, mediaPath);
                    float radius = scene.Meshes[0].BoundingBox.calculateBoxRadius();
                    float posOffsetX = radius * 2;

                    //cargar meshes en dataGrid
                    foreach (TgcMesh mesh in scene.Meshes)
                    {
                        SceneEditorMeshObject mo = new SceneEditorMeshObject();
                        mo.mesh = mesh;
                        mo.name = mesh.Name + meshCounter++;
                        mo.userInfo = "";
                        mo.index = meshObjects.Count;
                        mo.fileName = fileName.Text;
                        mo.groupIndex = meshGroupCounter;
                        mo.folderName = meshFolderName;

                        //Mover mesh a la posicion correcta
                        mesh.Position = new Vector3(initialPos.X + i * posOffsetX, initialPos.Y, initialPos.Z);

                        meshObjects.Add(mo);
                        dataGridMeshList.Rows.Add(dataGridMeshList.Rows.Count + 1, meshGroupCounter, mo.name);
                    }

                    meshGroupCounter++;
                }

                //seleccionar el primer mesh en la grilla
                seleccionarPrimerElementoDataGrid();

                //pasar a modo camara con edicion de mesh
                radioButtonCameraMode.Checked = true;
                tabControl.SelectedTab = tabControl.TabPages["tabPageModify"];

            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al cargar el archivo " + fileName.Text, "Error al cargar Mesh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GuiController.Instance.Logger.logError("Error al cargar Mesh de TGC", ex);
            }
            
        }

        /// <summary>
        /// Parsea la posicion inicial en donde ubicar los mesh creados
        /// </summary>
        private Vector3 getInitialPos()
        {
            if (!ValidationUtils.validateFloat(createPosX.Text))
            {
                createPosX.Text = "0";
            }
            if (!ValidationUtils.validateFloat(createPosY.Text))
            {
                createPosY.Text = "0";
            }
            if (!ValidationUtils.validateFloat(createPosZ.Text))
            {
                createPosZ.Text = "0";
            }

            return new Vector3(float.Parse(createPosX.Text), float.Parse(createPosY.Text), float.Parse(createPosZ.Text));
        }

        #endregion


        #region Scene


        /// <summary>
        /// Velocidad de camara FPS
        /// </summary>
        private void cameraSpeed_ValueChanged(object sender, EventArgs e)
        {
            GuiController.Instance.FpsCamera.MovementSpeed = (float)cameraSpeed.Value;
            GuiController.Instance.FpsCamera.JumpSpeed = (float)cameraSpeed.Value;
        }

        /// <summary>
        /// Exportar escena a XML
        /// </summary>
        private void exportSceneButton_Click(object sender, EventArgs e)
        {
            saveSceneDialog.Title = "Export Scene";
            if (saveSceneDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo fInfo = new FileInfo(saveSceneDialog.FileName);
                string sceneName = fInfo.Name.Split('.')[0];
                string saveDir = fInfo.DirectoryName;

                TgcScene exportScene = new TgcScene(sceneName, saveDir);
                foreach (SceneEditorMeshObject meshObject in meshObjects)
                {
                    exportScene.Meshes.Add(meshObject.mesh);
                }
                TgcSceneExporter exporter = new TgcSceneExporter();
                exporter.exportSceneToXml(exportScene, saveDir);

                MessageBox.Show(this, "Escena exportada a formato TGC satisfactoriamente.\n" + "El terreno no fue exportado", 
                    "Export Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Exportación customizada
        /// </summary>
        private void buttonCustomExport_Click(object sender, EventArgs e)
        {
            saveSceneDialog.Title = "Exportación customizada a archivo XML";
            if (saveSceneDialog.ShowDialog() == DialogResult.OK)
            {
                string savePath = saveSceneDialog.FileName;
                editor.exportScene(savePath);
                MessageBox.Show("Escena exportada satisfactoriamente", "Exportación");
            }
        }

        /// <summary>
        /// Devuelve los objetos del escenario ordenados por numero de grupo
        /// </summary>
        public List<SceneEditorMeshObject> getMeshObjectsOrderByGroup()
        {
            List<SceneEditorMeshObject> sortedList = new List<SceneEditorMeshObject>();
            sortedList.AddRange(meshObjects);
            sortedList.Sort(new ComparadorGrupoMeshObject());
            return sortedList;
        }

        /// <summary>
        /// Comparador que ordena por numero de grupo
        /// </summary>
        private class ComparadorGrupoMeshObject : IComparer<SceneEditorMeshObject>
        {
            public int Compare(SceneEditorMeshObject x, SceneEditorMeshObject y)
            {
                return x.groupIndex.CompareTo(y.groupIndex);
            }
        }


        



        #endregion


        #region Translaste Gizmo

        private void initGizmoDragging()
        {
            translateGizmo.detectSelectedAxis(pickingRay);
        }

        private void doGizmoTranslate()
        {
            if (translateGizmo.SelectedAxis != SceneEditorTranslateGizmo.Axis.None)
            {
                translateGizmo.updateMove();
                updateEditMeshValues(translateGizmo.MeshObj);
            }
        }

        private void endGizmoDragging()
        {
            translateGizmo.endDragging();
        }

        #endregion


        #region SelectionMode

        /// <summary>
        /// Hacer picking y seleccionar modelo si hay colision
        /// </summary>
        private void findSelectedMeshWithPicking()
        {
            pickingRay.updateRay();
            Vector3 collisionP;

            //Hacer picking sobre todos los modelos y quedarse con el mas cerca
            float minDistance = float.MaxValue;
            SceneEditorMeshObject collisionMeshObj = null;
            foreach (SceneEditorMeshObject meshObj in meshObjects)
            {
                if (!meshObj.visible)
                {
                    continue;
                }

                //Si ya está seleccionado ignorar
                if (isMeshObjectSelected(meshObj))
                {
                    continue;
                }

                TgcBoundingBox aabb = meshObj.mesh.BoundingBox;
                bool result = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionP);
                if (result)
                {
                    float lengthSq = Vector3.Subtract(collisionP, pickingRay.Ray.Origin).LengthSq();
                    if (lengthSq < minDistance)
                    {
                        minDistance = lengthSq;
                        collisionMeshObj = meshObj;
                    }
                }
            }

            //Seleccionar objeto
            if (collisionMeshObj != null)
            {
                //Deseleccionar todo
                for (int i = 0; i < dataGridMeshList.Rows.Count; i++)
                {
                    dataGridMeshList.Rows[i].Selected = false;
                }

                //Seleccionar objeto del dataGrid
                dataGridMeshList.Rows[collisionMeshObj.index].Selected = true;
                dataGridMeshList_RowEnter(null, null);
            }
        }


        /// <summary>
        /// Informa si un objeto esta actualmente seleccionado
        /// </summary>
        private bool isMeshObjectSelected(SceneEditorMeshObject meshObj)
        {
            foreach (SceneEditorMeshObject selectMeshObj in selectedMeshList)
            {
                if (selectMeshObj == meshObj)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion


        #region MeshEdit-General

        /// <summary>
        /// Cuando seleccionan un mesh del dataGrid.
        /// Obtener info y cargarla en el panel de edicion
        /// </summary>
        private void dataGridMeshList_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridMeshList.SelectedRows.Count > 0)
            {
                //cargar lista de seleccion, al revez
                selectedMeshList.Clear();
                for (int i = dataGridMeshList.SelectedRows.Count - 1; i >= 0; i--)
                {
                    int index = dataGridMeshList.SelectedRows[i].Index;
                    selectedMeshList.Add(meshObjects[index]);
                }

                //Seleccionar primero de la lista
                SceneEditorMeshObject selectedMeshObj = selectedMeshList[0];
                selectMeshObject(selectedMeshObj);
            }
        }

        /// <summary>
        /// Seleccionar objeto
        /// </summary>
        private void selectMeshObject(SceneEditorMeshObject selectedMeshObj)
        {
            //Cargar valores del panel de edicion
            updateEditMeshValues(selectedMeshObj);
            enableEditMeshPanels(true);
        }

        /// <summary>
        /// Habilitar paneles de edicion de Mesh
        /// </summary>
        private void enableEditMeshPanels(bool flag)
        {
            groupBoxEditMeshGeneral.Enabled = flag;
            groupBoxEditMeshPosition.Enabled = flag;
            groupBoxEditMeshRotation.Enabled = flag;
            groupBoxEditMeshScale.Enabled = flag;
            groupBoxEditMeshUserInfo.Enabled = flag;
        }

        /// <summary>
        /// Actualizar valores de edicion de mesh de la interfaz
        /// </summary>
        private void updateEditMeshValues(SceneEditorMeshObject meshObj)
        {
            meshName.Text = meshObj.name;

            Vector3 pos = meshObj.mesh.Position;
            meshPosX.Text = pos.X.ToString();
            meshPosY.Text = pos.Y.ToString();
            meshPosZ.Text = pos.Z.ToString();

            Vector3 rot = meshObj.mesh.Rotation;
            meshRotX.Text = rot.X.ToString();
            meshRotY.Text = rot.Y.ToString();
            meshRotZ.Text = rot.Z.ToString();

            Vector3 scale = meshObj.mesh.Scale;
            meshScaleX.Text = scale.X.ToString();
            meshScaleY.Text = scale.Y.ToString();
            meshScaleZ.Text = scale.Z.ToString();

            userInfo.Text = meshObj.userInfo;
        }

        /// <summary>
        /// Remover un mesh
        /// </summary>
        private void meshRemove_Click(object sender, EventArgs e)
        {
            //Ordenar seleccion en forma descendente segun Index en la dataGrid
            selectedMeshList.Sort(new ComparadorMeshObjectEliminar());
            
            //clonar lista de seleccion
            List<SceneEditorMeshObject> clonedList = new List<SceneEditorMeshObject>();
            foreach (SceneEditorMeshObject selectedMeshObj in selectedMeshList)
            {
                clonedList.Add(selectedMeshObj);
            }

            //Eliminar en base a lista clonada
            foreach (SceneEditorMeshObject selectedMeshObj in clonedList)
            {
                dataGridMeshList.Rows.RemoveAt(selectedMeshObj.index);
                meshObjects.RemoveAt(selectedMeshObj.index);
                selectedMeshObj.mesh.dispose();
            }
            selectedMeshList.Clear();
            seleccionarPrimerElementoDataGrid();

            //arreglar indices de objetos
            int index = 0;
            foreach (SceneEditorMeshObject selectedMeshObj in meshObjects)
            {
                selectedMeshObj.index = index++;
            }
            
        }

        /// <summary>
        /// Seleccionar el primer elemento de la dataGrid, si quedo algo
        /// </summary>
        private void seleccionarPrimerElementoDataGrid()
        {
            if (dataGridMeshList.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridMeshList.Rows)
                {
                    row.Selected = false;
                }
                dataGridMeshList.Rows[0].Selected = true;
                dataGridMeshList_RowEnter(null, null);
            }
            else
            {
                enableEditMeshPanels(false);
                translateGizmo.hide();
            }
        }

        /// <summary>
        /// Ordena los objetos en forma descendente segun Index en la dataGrid
        /// </summary>
        private class ComparadorMeshObjectEliminar : IComparer<SceneEditorMeshObject>
        {
            public int Compare(SceneEditorMeshObject x, SceneEditorMeshObject y)
            {
                return y.index.CompareTo(x.index);
            }
        }

        /// <summary>
        /// Cambiar nombre de mesh
        /// </summary>
        private void meshName_Leave(object sender, EventArgs e)
        {
            SceneEditorMeshObject selectedMeshObj = selectedMeshList[0];

            //cambiarlo en mesh
            selectedMeshObj.name = meshName.Text;

            //cambiarlo en dataGrid
            dataGridMeshList.Rows[selectedMeshObj.index].Cells["MeshColumnMeshName"].Value = selectedMeshObj.name;
        }

        /// <summary>
        /// Cargar UserInfo
        /// </summary>
        private void userInfo_TextChanged(object sender, EventArgs e)
        {
            selectedMeshList[0].userInfo = userInfo.Text;
        }

        #endregion


        #region MeshEdit - Position

        /// <summary>
        /// Actualizar posicion de malla seleccionada en base a los Inputs
        /// </summary>
        private void updateMeshPositionFromGui()
        {
            SceneEditorMeshObject meshObj = selectedMeshList[0];
            Vector3 newPos = meshObj.mesh.Position;

            //Nueva posicion en base a input de usuario
            if (ValidationUtils.validateFloat(meshPosX.Text))
            {
                newPos.X = float.Parse(meshPosX.Text);
            }
            else
            {
                meshPosX.Text = newPos.X.ToString();
            }
            if (ValidationUtils.validateFloat(meshPosY.Text))
            {
                newPos.Y = float.Parse(meshPosY.Text);
            }
            {
                meshPosY.Text = newPos.Y.ToString();
            }
            if (ValidationUtils.validateFloat(meshPosZ.Text))
            {
                newPos.Z = float.Parse(meshPosZ.Text);
            }
            else
            {
                meshPosZ.Text = newPos.Z.ToString();
            }

            //Actualizar posicion
            meshObj.mesh.Position = newPos;

            //Actualizar gizmo si corresponde
            if (currentState == GuiState.TranslateMode)
            {
                translateGizmo.setMesh(meshObj);
            }
        }

        /// <summary>
        /// Actualizar la posicion X
        /// </summary>
        private void meshPosX_Leave(object sender, EventArgs e)
        {
            updateMeshPositionFromGui();
        }

        /// <summary>
        /// Actualizar posicion Y
        /// </summary>
        private void meshPosY_Leave(object sender, EventArgs e)
        {
            updateMeshPositionFromGui();
        }

        /// <summary>
        /// Actualizar posicion Z
        /// </summary>
        private void meshPosZ_Leave(object sender, EventArgs e)
        {
            updateMeshPositionFromGui();
        }

        #endregion


        #region MeshEdit - Rotation


        /// <summary>
        /// Actualizar rotación de malla seleccionada en base a los Inputs
        /// </summary>
        private void updateMeshRotationFromGui()
        {
            SceneEditorMeshObject meshObj = selectedMeshList[0];
            Vector3 newRot = meshObj.mesh.Rotation;
            float minValue = 0;
            float maxValue = 360;

            //Nueva rotación en base a input de usuario
            if (ValidationUtils.validateFloatRange(meshRotX.Text, minValue, maxValue))
            {
                newRot.X = float.Parse(meshRotX.Text);
            }
            else
            {
                meshRotX.Text = newRot.X.ToString();
            }
            if (ValidationUtils.validateFloatRange(meshRotY.Text, minValue, maxValue))
            {
                newRot.Y = float.Parse(meshRotY.Text);
            }
            else
            {
                meshRotY.Text = newRot.Y.ToString();
            }
            if (ValidationUtils.validateFloatRange(meshRotZ.Text, minValue, maxValue))
            {
                newRot.Z = float.Parse(meshRotZ.Text);
            }
            else
            {
                meshRotZ.Text = newRot.Z.ToString();
            }

            //Actualizar las 3 trackbars de rotacion
            trackBarRotationX.Value = (int)newRot.X;
            trackBarRotationY.Value = (int)newRot.Y;
            trackBarRotationZ.Value = (int)newRot.Z;

            //Actualizar rotacion, pasar a Radianes
            newRot.X = Geometry.DegreeToRadian(newRot.X);
            newRot.Y = Geometry.DegreeToRadian(newRot.Y);
            newRot.Z = Geometry.DegreeToRadian(newRot.Z);
            meshObj.mesh.Rotation = newRot;
        }

        /// <summary>
        /// Actualizar rotacion X
        /// </summary>
        private void meshRotX_Leave(object sender, EventArgs e)
        {
            updateMeshRotationFromGui();
        }

        /// <summary>
        /// Actualizar rotacion Y
        /// </summary>
        private void meshRotY_Leave(object sender, EventArgs e)
        {
            updateMeshRotationFromGui();
        }

        /// <summary>
        /// Actualizar rotacion Z
        /// </summary>
        private void meshRotZ_Leave(object sender, EventArgs e)
        {
            updateMeshRotationFromGui();
        }

        /// <summary>
        /// Actualizar rotacion de mesh en base a los trackbars de rotacion
        /// </summary>
        private void updateMeshRotationFromTrackbars()
        {
            SceneEditorMeshObject meshObj = selectedMeshList[0];
            Vector3 newRot = meshObj.mesh.Rotation;

            //Actualizar rotacion en base a los trackbars
            newRot.X = (float)trackBarRotationX.Value;
            newRot.Y = (float)trackBarRotationY.Value;
            newRot.Z = (float)trackBarRotationZ.Value;

            //Actualizar inputs de rotacion
            meshRotX.Text = newRot.X.ToString();
            meshRotY.Text = newRot.Y.ToString();
            meshRotZ.Text = newRot.Z.ToString();

            //pasar a Radianes
            newRot.X = Geometry.DegreeToRadian(newRot.X);
            newRot.Y = Geometry.DegreeToRadian(newRot.Y);
            newRot.Z = Geometry.DegreeToRadian(newRot.Z);
            meshObj.mesh.Rotation = newRot;
        }

        /// <summary>
        /// Trackbar de rotación X
        /// </summary>
        private void trackBarRotationX_ValueChanged(object sender, EventArgs e)
        {
            updateMeshRotationFromTrackbars();
        }

        /// <summary>
        /// Trackbar de rotación Y
        /// </summary>
        private void trackBarRotationY_Scroll(object sender, EventArgs e)
        {
            updateMeshRotationFromTrackbars();
        }

        /// <summary>
        /// Trackbar de rotación Z
        /// </summary>
        private void trackBarRotationZ_Scroll(object sender, EventArgs e)
        {
            updateMeshRotationFromTrackbars();
        }

        /// <summary>
        /// Rotar BoundingBox
        /// </summary>
        private void buttonEditMeshRotateBoundingBox_Click(object sender, EventArgs e)
        {
            TgcMesh mesh = selectedMeshList[0].mesh;
            mesh.BoundingBox.transform(mesh.Transform);

            //meshObj.mesh.BoundingBox.rotate(meshObj.mesh.Rotation);
        }


        #endregion


        #region MeshEdit - Scale

        /// <summary>
        /// Actualizar escala de mesh en base a valores de inputs
        /// </summary>
        private void updateMeshScaleFromGui()
        {
            SceneEditorMeshObject meshObj = selectedMeshList[0];
            Vector3 newScale = meshObj.mesh.Scale;

            //Nueva posicion en base a input de usuario
            if (ValidationUtils.validatePossitiveFloat(meshScaleX.Text))
            {
                newScale.X = float.Parse(meshScaleX.Text);
            }
            else
            {
                meshScaleX.Text = newScale.X.ToString();
            }
            if (ValidationUtils.validatePossitiveFloat(meshScaleY.Text))
            {
                newScale.Y = float.Parse(meshScaleY.Text);
            }
            {
                meshScaleY.Text = newScale.Y.ToString();
            }
            if (ValidationUtils.validatePossitiveFloat(meshScaleZ.Text))
            {
                newScale.Z = float.Parse(meshScaleZ.Text);
            }
            else
            {
                meshScaleZ.Text = newScale.Z.ToString();
            }

            //Actualizar escala
            meshObj.mesh.Scale = newScale;
        }

        /// <summary>
        /// Escalar X
        /// </summary>
        private void meshScaleX_Leave(object sender, EventArgs e)
        {
            updateMeshScaleFromGui();
        }

        /// <summary>
        /// Escalar Y
        /// </summary>
        private void meshScaleY_Leave(object sender, EventArgs e)
        {
            updateMeshScaleFromGui();
        }

        /// <summary>
        /// Escalar Z
        /// </summary>
        private void meshScaleZ_Leave(object sender, EventArgs e)
        {
            updateMeshScaleFromGui();
        }


        #endregion


        #region Terreno

        /// <summary>
        /// Abrir Heightmap
        /// </summary>
        private void openHeightmap_Click(object sender, EventArgs e)
        {
            if (openHeighmapDialog.ShowDialog() == DialogResult.OK)
            {
                heighmapFilePath = openHeighmapDialog.FileName;
                string[] array = heighmapFilePath.Split('\\');
                heighmap.Text = array[array.Length - 1];
            }
        }

        /// <summary>
        /// Abrir textura de terreno
        /// </summary>
        private void openTerrainTexture_Click(object sender, EventArgs e)
        {
            if (openTextureDialog.ShowDialog() == DialogResult.OK)
            {
                terrainTextureFilePath = openTextureDialog.FileName;
                string[] array = terrainTextureFilePath.Split('\\');
                terrainTexture.Text = array[array.Length - 1];
            }
        }

        /// <summary>
        /// Crear Terrain
        /// </summary>
        private void terrainCreate_Click(object sender, EventArgs e)
        {
            if (heighmapFilePath == null || terrainTextureFilePath == null)
            {
                MessageBox.Show("No se especificó un Heightmap y una Textura para el terreno", "Creación de Terreno");
                return;
            }

            try
            {
                //quitar terreno, si hay
                terrainRemove_Click(sender, e);

                //Crear nuevo
                tgcTerrain = new TgcSimpleTerrain();
                tgcTerrain.loadHeightmap(heighmapFilePath, (float)terrainXZscale.Value, (float)terrainYscale.Value, getTerrainCenter());
                tgcTerrain.loadTexture(terrainTextureFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al crea un terreno desde el Heightmap " + heighmap.Text, "Error al cargar Heightmap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GuiController.Instance.Logger.logError("Error al cargar Heightmap", ex);
            }
        }

        /// <summary>
        /// Elimina el terreno, si hay
        /// </summary>
        private void terrainRemove_Click(object sender, EventArgs e)
        {
            if (tgcTerrain != null)
            {
                tgcTerrain.dispose();
                tgcTerrain = null;
            }
        }

        /// <summary>
        /// Parsea la posicion del centro del terreno
        /// </summary>
        private Vector3 getTerrainCenter()
        {
            if (!ValidationUtils.validateFloat(terrainCenterX.Text))
            {
                terrainCenterX.Text = "0";
            }
            if (!ValidationUtils.validateFloat(terrainCenterY.Text))
            {
                terrainCenterY.Text = "0";
            }
            if (!ValidationUtils.validateFloat(terrainCenterZ.Text))
            {
                terrainCenterZ.Text = "0";
            }

            return new Vector3(float.Parse(terrainCenterX.Text), float.Parse(terrainCenterY.Text), float.Parse(terrainCenterZ.Text));
        }


        #endregion

        

       

        

        

        





        

        
        











































        
    }
}
