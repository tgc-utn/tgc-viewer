using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Examples.MeshCreator
{
    /// <summary>
    /// Popup para elegir un layer
    /// </summary>
    public partial class ObjectBrowserSelectLayer : Form
    {
        string selectedLayer;
        /// <summary>
        /// El layer elegido
        /// </summary>
        public string SelectedLayer
        {
            get { return selectedLayer; }
            set { 
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


        public ObjectBrowserSelectLayer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cargar layers
        /// </summary>
        public void loadLayers(List<string> layers)
        {
            treeViewLayers.Nodes.Clear();
            this.selectedLayer = "";
            foreach (string layerName in layers)
            {
                TreeNode layerNode = new TreeNode(layerName);
                layerNode.BackColor = Color.LightBlue;
                treeViewLayers.Nodes.Add(layerNode);
            }
        }

        private void treeViewLayers_DoubleClick(object sender, EventArgs e)
        {
            TreeNode node = treeViewLayers.SelectedNode;
            if (node != null)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.selectedLayer = node.Text;
                this.Close();
            }
        }
    }
}
