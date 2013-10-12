using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Examples.MeshCreator.Primitives;

namespace Examples.MeshCreator
{
    public partial class ObjectBrowser : Form
    {

        MeshCreatorControl control;
        InputMessageBox inputBox;
        ObjectBrowserSelectLayer selectLayerDialog;


        public ObjectBrowser(MeshCreatorControl control)
        {
            InitializeComponent();
            this.control = control;
            this.inputBox = new InputMessageBox();
            this.selectLayerDialog = new ObjectBrowserSelectLayer();
        }

        /// <summary>
        /// Cargar todos los mesh
        /// </summary>
        public void loadObjects(string searchQuery)
        {
            searchQuery = searchQuery.ToLower().Trim();

            treeViewObjects.Nodes.Clear();
            Dictionary<string, TreeNode> layerNodes = new Dictionary<string, TreeNode>();

            //Agregar meshes al tree
            foreach (EditorPrimitive p in control.Meshes)
            {
                //Busqueda por nombre
                string meshName = p.Name.ToLower();
                if (searchQuery == "" || meshName.Contains(searchQuery))
                {
                    //Ver si ya existe el nodo para el layer de este mesh
                    TreeNode layerNode;
                    if (layerNodes.ContainsKey(p.Layer))
                    {
                        //Crear nodo de layer
                        layerNode = layerNodes[p.Layer];
                    }
                    else
                    {
                        //Crear nodo de layer
                        layerNode = new TreeNode(p.Layer);
                        layerNode.BackColor = Color.LightBlue;
                        treeViewObjects.Nodes.Add(layerNode);
                        layerNodes.Add(p.Layer, layerNode);
                    }

                    //Crear nodo de mesh
                    TreeNode node = new TreeNode(p.Name);
                    node.Tag = p;
                    node.Checked = p.Selected;
                    node.BackColor = getMeshNodeColor(node);
                    layerNode.Nodes.Add(node);
                }
            }

            if (searchQuery != "")
            {
                treeViewObjects.ExpandAll();
            }

            //Ver si hay que tildar los layers si todos sus hijos estan tildados
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                bool allChecked = true;
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (!childNode.Checked)
                    {
                        allChecked = false;
                        break;
                    }
                }
                if (allChecked)
                {
                    node.Checked = true;
                }
            }
        }

        private Color getMeshNodeColor(TreeNode n)
        {
            EditorPrimitive p = (EditorPrimitive)n.Tag;
            if (p.Selected)
            {
                return Color.Yellow;
            }
            else
            {
                return p.Visible ? Color.White : Color.LightGray;
            }
        }

        /// <summary>
        /// Cuando se cierra el form
        /// </summary>
        private void ObjectBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            control.PopupOpened = false;

            //Evitar hacer dispose del formulario, solo ocultarlo
            e.Cancel = true;
            this.Parent = null;
            this.Hide();
        }

        /// <summary>
        /// Clic en "Search"
        /// </summary>
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            loadObjects(textBoxSearch.Text);
        }

        /// <summary>
        /// Cuando cambia el  texto de textBoxSearch
        /// </summary>
        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            loadObjects(textBoxSearch.Text);
        }

        /// <summary>
        /// Luego de tildar el checkbox de un objeto
        /// </summary>
        private void treeViewObjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //Si es un layer, tildar todos sus hijos
            TreeNode node = e.Node;
            if (node.Tag == null)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    childNode.Checked = node.Checked;
                }
            }

            //Actualizar seleccion (solo si fue producido por el usuario y no por la propia aplicacion)
            if (e.Action == TreeViewAction.ByMouse)
            {
                updateSelection();
                treeViewObjects.SelectedNode = node;
            }
        }

        /// <summary>
        /// Actualizar que objetos estan seleccionados y cuales no
        /// </summary>
        public void updateSelection()
        {
            //Limpiar seleccion
            control.SelectionRectangle.clearSelection();

            //Marcar como seleccionados todos los objetos del tree con check
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Checked)
                    {
                        EditorPrimitive p = (EditorPrimitive)childNode.Tag;
                        if (p.Visible)
                        {
                            control.SelectionRectangle.selectObject(p);
                        }
                    }
                    childNode.BackColor = getMeshNodeColor(childNode);
                }
            }

            //Actualizar paneles
            control.updateModifyPanel();

            //Quitar gizmo actual
            control.CurrentGizmo = null;

            //Pasar a modo seleccion
            control.CurrentState = MeshCreatorControl.State.SelectObject;
        }

        /// <summary>
        /// Clic en "Show"
        /// </summary>
        private void buttonShow_Click(object sender, EventArgs e)
        {
            showHideObjects(true);
        }

        /// <summary>
        /// Clic en "Hide"
        /// </summary>
        private void buttonHide_Click(object sender, EventArgs e)
        {
            showHideObjects(false);
        }

        /// <summary>
        /// Mostrar u ocultar los objetos con check
        /// </summary>
        public void showHideObjects(bool show)
        {
            List<EditorPrimitive> objectsToShowHide = new List<EditorPrimitive>();
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Checked)
                    {
                        EditorPrimitive p = (EditorPrimitive)childNode.Tag;
                        objectsToShowHide.Add(p);
                    }
                }
            }
            control.showHideObjects(objectsToShowHide, show);
            updateSelection();
        }

        /// <summary>
        /// Seleccionar elemento del TreeView
        /// </summary>
        private void treeViewObjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //e.Node.Checked = !e.Node.Checked;
        }

        /// <summary>
        /// Clic en "New layer"
        /// </summary>
        private void buttonNewLayer_Click(object sender, EventArgs e)
        {
            inputBox.Text = "New layer";
            inputBox.InputLabel = "Layer name: ";
            inputBox.InputText = "layer1";
            if (inputBox.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                //Chequear que ese nombre ya no exista
                string layerName = inputBox.InputText;
                TreeNode repetedLayer = getLayerWithName(layerName);
                if (repetedLayer != null)
                {
                    MessageBox.Show(this, "There is already another layer with the name: " + layerName, "Duplicated layer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    //Agregar layer
                    TreeNode layerNode = new TreeNode(inputBox.InputText);
                    layerNode.BackColor = Color.LightBlue;
                    treeViewObjects.Nodes.Add(layerNode);
                }
            }
        }

        /// <summary>
        /// Clic en "Delete"
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            //Obtener objetos a borrar
            List<EditorPrimitive> objectsToDelete = new List<EditorPrimitive>();
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Checked)
                    {
                        EditorPrimitive p = (EditorPrimitive)childNode.Tag;
                        objectsToDelete.Add(p);
                    }
                }
            }

            //Confirmacion
            if (objectsToDelete.Count > 0)
            {
                if (MessageBox.Show(this, "¿Do you want to delete " + objectsToDelete.Count + " objects?", "Delete objects", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    //Borrar meshes
                    control.deleteObjects(objectsToDelete);

                    //Borrar layers y objetos seleccionados del tree
                    for (int i = 0; i < treeViewObjects.Nodes.Count; i++)
                    {
                        TreeNode node = treeViewObjects.Nodes[i];
                        for (int j = 0; j < node.Nodes.Count; j++)
                        {
                            TreeNode childNode = node.Nodes[j];
                            if (childNode.Checked)
                            {
                                node.Nodes.RemoveAt(j);
                                j--;
                            }
                        }
                        if (node.Checked)
                        {
                            treeViewObjects.Nodes.RemoveAt(i);
                            i--;
                        }
                    }

                    updateSelection();
                }
            }
        }

        /// <summary>
        /// Clic en "Move"
        /// </summary>
        private void buttonMove_Click(object sender, EventArgs e)
        {
            //Mostrar popup para elegir layer destino
            List<string> layerNames = new List<string>();
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                layerNames.Add(node.Text);
            }
            selectLayerDialog.loadLayers(layerNames);

            if (selectLayerDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                //Buscar el nodo layer destino elegido
                TreeNode dstLayer = getLayerWithName(selectLayerDialog.SelectedLayer);
                if (dstLayer != null)
                {
                    //Recorrer todos los nodos chequeados y moverlos al layer destino
                    foreach (TreeNode node in treeViewObjects.Nodes)
                    {
                        for (int i = 0; i < node.Nodes.Count; i++)
                        {
                            //Evitar moverlo al mismo destino
                            TreeNode childNode = node.Nodes[i];
                            if (childNode.Checked && node.Text != dstLayer.Text)
                            {
                                //Cambiar layer del mesh
                                EditorPrimitive p = (EditorPrimitive)childNode.Tag;
                                p.Layer = dstLayer.Text;

                                //Mover de nodo del tree
                                node.Nodes.RemoveAt(i);
                                i--;

                                //Agregar al nuevo nodo
                                dstLayer.Nodes.Add(childNode);
                            }
                        }
                    }

                    updateSelection();
                }

            }
        }

        /// <summary>
        /// Buscar nodo de layer por nombre
        /// </summary>
        private TreeNode getLayerWithName(string text)
        {
            TreeNode layerNode = null;
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                if (node.Text == text)
                {
                    layerNode = node;
                    break;
                }
            }
            return layerNode;
        }

        /// <summary>
        /// Clic en "Rename"
        /// </summary>
        private void buttonRename_Click(object sender, EventArgs e)
        {
            TreeNode node = treeViewObjects.SelectedNode;
            if (node != null)
            {
                inputBox.Text = "Rename";
                inputBox.InputLabel = "Object name:";
                inputBox.InputText = node.Text;
                if (inputBox.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    //Es un layer
                    if (node.Tag == null)
                    {
                        //Cambiar nombre de layer
                        node.Text = inputBox.InputText;

                        //Cambiar layer a todos sus hijos
                        foreach (TreeNode childNode in node.Nodes)
                        {
                            EditorPrimitive p = (EditorPrimitive)childNode.Tag;
                            p.Layer = node.Text;
                        }
                    }
                    //Es un mesh
                    else
                    {
                        EditorPrimitive p = (EditorPrimitive)node.Tag;
                        p.Name = inputBox.InputText;
                        node.Text = p.Name;
                    }
                    
                    updateSelection();
                }

                treeViewObjects.Focus();
            }
        }

        /// <summary>
        /// Clic en "Select All"
        /// </summary>
        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    childNode.Checked = true;
                }
                node.Checked = true;
            }
            updateSelection();
        }

        /// <summary>
        /// Clic en "Unselect All"
        /// </summary>
        private void buttonUnselectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeViewObjects.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    childNode.Checked = false;
                }
                node.Checked = false;
            }
            updateSelection();
        }



    }
}
