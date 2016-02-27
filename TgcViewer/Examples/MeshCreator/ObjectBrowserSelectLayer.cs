using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Examples.MeshCreator
{
    /// <summary>
    ///     Popup para elegir un layer
    /// </summary>
    public partial class ObjectBrowserSelectLayer : Form
    {
        private string selectedLayer;

        public ObjectBrowserSelectLayer()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     El layer elegido
        /// </summary>
        public string SelectedLayer
        {
            get { return selectedLayer; }
            set
            {
                foreach (TreeNode node in treeViewLayers.Nodes)
                {
                    if (node.Text == value)
                    {
                        treeViewLayers.SelectedNode = node;
                        selectedLayer = value;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Cargar layers
        /// </summary>
        public void loadLayers(List<string> layers)
        {
            treeViewLayers.Nodes.Clear();
            selectedLayer = "";
            foreach (var layerName in layers)
            {
                var layerNode = new TreeNode(layerName);
                layerNode.BackColor = Color.LightBlue;
                treeViewLayers.Nodes.Add(layerNode);
            }
        }

        private void treeViewLayers_DoubleClick(object sender, EventArgs e)
        {
            var node = treeViewLayers.SelectedNode;
            if (node != null)
            {
                DialogResult = DialogResult.OK;
                selectedLayer = node.Text;
                Close();
            }
        }
    }
}