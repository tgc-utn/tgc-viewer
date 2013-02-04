using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.Optimizacion.KdTree
{
    /// <summary>
    /// Nodo del árbol KdTree
    /// </summary>
    class KdTreeNode
    {
        public KdTreeNode[] children;
        public TgcMesh[] models;

        //Corte realizado
        public float xCut;
        public float yCut;
        public float zCut;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}
