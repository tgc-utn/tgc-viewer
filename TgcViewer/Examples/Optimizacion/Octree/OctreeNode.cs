using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Optimizacion.Octree
{
    /// <summary>
    /// Nodo del árbol Octree
    /// </summary>
    class OctreeNode
    {
        public OctreeNode[] children;
        public TgcMesh[] models;

        public bool isLeaf()
        {
            return children == null;
        }

    }
}
