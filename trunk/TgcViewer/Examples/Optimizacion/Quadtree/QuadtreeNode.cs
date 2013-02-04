using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Optimizacion.Quadtree
{
    /// <summary>
    /// Nodo del árbol Quadtree
    /// </summary>
    class QuadtreeNode
    {
        public QuadtreeNode[] children;
        public TgcMesh[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}
