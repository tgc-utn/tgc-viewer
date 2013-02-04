using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;

namespace Examples.Optimizacion.KdTree
{
    /// <summary>
    /// Herramienta para crear y utilizar un KdTree para renderizar por Frustum Culling
    /// </summary>
    public class KdTree
    {

        KdTreeNode kdtreeRootNode;
        KdTreeBuilder builder;
        List<TgcMesh> modelos;
        TgcBoundingBox sceneBounds;
        List<TgcDebugBox> debugKdTreeBoxes;

        public KdTree()
        {
            builder = new KdTreeBuilder();
        }

        /// <summary>
        /// Crear nuevo KdTree
        /// </summary>
        /// <param name="modelos">Modelos a optimizar</param>
        /// <param name="sceneBounds">L�mites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //Crear KdTree
            this.kdtreeRootNode = builder.crearKdTree(modelos, sceneBounds);

            //Deshabilitar todos los mesh inicialmente
            foreach (TgcMesh mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Crear meshes para debug
        /// </summary>
        public void createDebugKdTreeMeshes()
        {
            debugKdTreeBoxes = builder.createDebugKdTreeMeshes(kdtreeRootNode, sceneBounds);
        }

        /// <summary>
        /// Renderizar en forma optimizado utilizando el KdTree para hacer FrustumCulling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum, kdtreeRootNode,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z);

            //Renderizar
            foreach (TgcMesh mesh in modelos)
            {
                if (mesh.Enabled)
                {
                    mesh.render();
                    mesh.Enabled = false;
                }
            }

            if (debugEnabled)
            {
                foreach (TgcDebugBox debugBox in debugKdTreeBoxes)
                {
                    debugBox.render();
                }
            }
        }

        /// <summary>
        /// Recorrer recursivamente el KdTree para encontrar los nodos visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum, KdTreeNode node,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            KdTreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }

            //recursividad sobre hijos
            else
            {
                float xCut = node.xCut;
                float yCut = node.yCut;
                float zCut = node.zCut;

                //000
                testChildVisibility(frustum, children[0], xCut, yCut, zCut, boxUpperX, boxUpperY, boxUpperZ);
                //001
                testChildVisibility(frustum, children[1], xCut, yCut, boxLowerZ, boxUpperX, boxUpperY, zCut);

                //010
                testChildVisibility(frustum, children[2], xCut, boxLowerY, zCut, boxUpperX, yCut, boxUpperZ);
                //011
                testChildVisibility(frustum, children[3], xCut, boxLowerY, boxLowerZ, boxUpperX, yCut, zCut);

                //100
                testChildVisibility(frustum, children[4], boxLowerX, yCut, zCut, xCut, boxUpperY, boxUpperZ);
                //101
                testChildVisibility(frustum, children[5], boxLowerX, yCut, boxLowerZ, xCut, boxUpperY, zCut);

                //110
                testChildVisibility(frustum, children[6], boxLowerX, boxLowerY, zCut, xCut, yCut, boxUpperZ);
                //111
                testChildVisibility(frustum, children[7], boxLowerX, boxLowerY, boxLowerZ, xCut, yCut, zCut);
                
            }
        }


        /// <summary>
        /// Hacer visible las meshes de un nodo si es visible por el Frustum
        /// </summary>
        private void testChildVisibility(TgcFrustum frustum, KdTreeNode childNode,
                float boxLowerX, float boxLowerY, float boxLowerZ, float boxUpperX, float boxUpperY, float boxUpperZ)
        {

            //test frustum-box intersection
            TgcBoundingBox caja = new TgcBoundingBox(
                new Vector3(boxLowerX, boxLowerY, boxLowerZ),
                new Vector3(boxUpperX, boxUpperY, boxUpperZ));
            TgcCollisionUtils.FrustumResult c = TgcCollisionUtils.classifyFrustumAABB(frustum, caja);

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
        /// Hacer visibles todas las meshes de un nodo, buscando recursivamente sus hojas
        /// </summary>
        private void addAllLeafMeshes(KdTreeNode node)
        {
            KdTreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);

                //pedir hojas a hijos
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    addAllLeafMeshes(children[i]);
                }
            }
        }


        /// <summary>
        /// Hacer visibles todas las meshes de un nodo
        /// </summary>
        private void selectLeafMeshes(KdTreeNode node)
        {
            TgcMesh[] models = node.models;
            foreach (TgcMesh m in models)
            {
                m.Enabled = true;
            }
        }

        
    }
}
