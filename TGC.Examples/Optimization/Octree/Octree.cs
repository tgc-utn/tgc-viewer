using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Optimization.Octree
{
    /// <summary>
    ///     Herramienta para crear y utilizar un Octree para renderizar por Frustum Culling
    /// </summary>
    public class Octree
    {
        private readonly OctreeBuilder builder;
        private List<TgcBoxDebug> debugOctreeBoxes;
        private List<TgcMesh> modelos;
        private OctreeNode octreeRootNode;
        private TgcBoundingAxisAlignBox sceneBounds;

        public Octree()
        {
            builder = new OctreeBuilder();
        }

        /// <summary>
        ///     Crear nuevo Octree
        /// </summary>
        /// <param name="modelos">Modelos a optimizar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingAxisAlignBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //Crear Octree
            octreeRootNode = builder.crearOctree(modelos, sceneBounds);

            //Deshabilitar todos los mesh inicialmente
            foreach (var mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        ///     Crear meshes para debug
        /// </summary>
        public void createDebugOctreeMeshes()
        {
            debugOctreeBoxes = builder.createDebugOctreeMeshes(octreeRootNode, sceneBounds);
        }

        /// <summary>
        ///     Renderizar en forma optimizado utilizando el Octree para hacer FrustumCulling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled)
        {
            var pMax = sceneBounds.PMax;
            var pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum, octreeRootNode,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z);

            //Renderizar
            foreach (var mesh in modelos)
            {
                if (mesh.Enabled)
                {
                    mesh.Render();
                    mesh.Enabled = false;
                }
            }

            if (debugEnabled)
            {
                foreach (var debugBox in debugOctreeBoxes)
                {
                    debugBox.Render();
                }
            }
        }

        /// <summary>
        ///     Recorrer recursivamente el Octree para encontrar los nodos visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum, OctreeNode node,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            var children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }

            //recursividad sobre hijos
            else
            {
                var midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
                var midY = FastMath.Abs((boxUpperY - boxLowerY) / 2);
                var midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

                //000
                testChildVisibility(frustum, children[0], boxLowerX + midX, boxLowerY + midY, boxLowerZ + midZ,
                    boxUpperX, boxUpperY, boxUpperZ);
                //001
                testChildVisibility(frustum, children[1], boxLowerX + midX, boxLowerY + midY, boxLowerZ, boxUpperX,
                    boxUpperY, boxUpperZ - midZ);

                //010
                testChildVisibility(frustum, children[2], boxLowerX + midX, boxLowerY, boxLowerZ + midZ, boxUpperX,
                    boxUpperY - midY, boxUpperZ);
                //011
                testChildVisibility(frustum, children[3], boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX,
                    boxUpperY - midY, boxUpperZ - midZ);

                //100
                testChildVisibility(frustum, children[4], boxLowerX, boxLowerY + midY, boxLowerZ + midZ,
                    boxUpperX - midX, boxUpperY, boxUpperZ);
                //101
                testChildVisibility(frustum, children[5], boxLowerX, boxLowerY + midY, boxLowerZ, boxUpperX - midX,
                    boxUpperY, boxUpperZ - midZ);

                //110
                testChildVisibility(frustum, children[6], boxLowerX, boxLowerY, boxLowerZ + midZ, boxUpperX - midX,
                    boxUpperY - midY, boxUpperZ);
                //111
                testChildVisibility(frustum, children[7], boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX,
                    boxUpperY - midY, boxUpperZ - midZ);
            }
        }

        /// <summary>
        ///     Hacer visible las meshes de un nodo si es visible por el Frustum
        /// </summary>
        private void testChildVisibility(TgcFrustum frustum, OctreeNode childNode,
            float boxLowerX, float boxLowerY, float boxLowerZ, float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            if (childNode == null)
            {
                return;
            }

            //test frustum-box intersection
            var caja = new TgcBoundingAxisAlignBox(
                new TGCVector3(boxLowerX, boxLowerY, boxLowerZ),
                new TGCVector3(boxUpperX, boxUpperY, boxUpperZ));
            var c = TgcCollisionUtils.classifyFrustumAABB(frustum, caja);

            //complementamente adentro: cargar todos los hijos directamente, sin testeos
            if (c == TgcCollisionUtils.FrustumResult.INSIDE)
            {
                addAllLeafMeshes(childNode);
            }

            //parte adentro: seguir haciendo testeos con hijos
            else if (c == TgcCollisionUtils.FrustumResult.INTERSECT)
            {
                findVisibleMeshes(frustum, childNode, boxLowerX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ);
            }
        }

        /// <summary>
        ///     Hacer visibles todas las meshes de un nodo, buscando recursivamente sus hojas
        /// </summary>
        private void addAllLeafMeshes(OctreeNode node)
        {
            if (node == null)
            {
                return;
            }

            var children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);

                //pedir hojas a hijos
            }
            else
            {
                for (var i = 0; i < children.Length; i++)
                {
                    addAllLeafMeshes(children[i]);
                }
            }
        }

        /// <summary>
        ///     Hacer visibles todas las meshes de un nodo
        /// </summary>
        private void selectLeafMeshes(OctreeNode node)
        {
            var models = node.models;
            foreach (var m in models)
            {
                m.Enabled = true;
            }
        }
    }
}