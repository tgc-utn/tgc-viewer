using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.Optimizacion.Octree
{
    /// <summary>
    ///     Nodo del árbol Octree
    /// </summary>
    internal class OctreeNode
    {
        public OctreeNode[] children;
        public TgcMesh[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}