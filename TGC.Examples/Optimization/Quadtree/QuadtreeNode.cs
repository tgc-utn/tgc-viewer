using TGC.Core.SceneLoader;

namespace TGC.Examples.Optimization.Quadtree
{
    /// <summary>
    ///     Nodo del árbol Quadtree
    /// </summary>
    internal class QuadtreeNode
    {
        public QuadtreeNode[] children;
        public TgcMesh[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}