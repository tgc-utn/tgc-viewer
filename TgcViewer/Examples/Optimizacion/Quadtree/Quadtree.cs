using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;

namespace Examples.Optimizacion.Quadtree
{
    /// <summary>
    /// Herramienta para crear y utilizar un Quadtree para renderizar por Frustum Culling
    /// </summary>
    public class Quadtree
    {

        QuadtreeNode quadtreeRootNode;
        List<TgcMesh> modelos;
        TgcBoundingBox sceneBounds;
        QuadtreeBuilder builder;
        List<TgcDebugBox> debugQuadtreeBoxes;

        public Quadtree()
        {
            builder = new QuadtreeBuilder();
        }

        /// <summary>
        /// Crear nuevo Quadtree
        /// </summary>
        /// <param name="modelos">Modelos a optimizar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //Crear Quadtree
            this.quadtreeRootNode = builder.crearQuadtree(modelos, sceneBounds);

            //Deshabilitar todos los mesh inicialmente
            foreach (TgcMesh mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Crear meshes para debug
        /// </summary>
        public void createDebugQuadtreeMeshes()
        {
            debugQuadtreeBoxes = builder.createDebugQuadtreeMeshes(quadtreeRootNode, sceneBounds);
        }

        /// <summary>
        /// Renderizar en forma optimizado utilizando el Quadtree para hacer FrustumCulling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum, quadtreeRootNode,
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
                foreach (TgcDebugBox debugBox in debugQuadtreeBoxes)
                {
                    debugBox.render();
                }
            }
        }


        /// <summary>
        /// Recorrer recursivamente el Quadtree para encontrar los nodos visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum, QuadtreeNode node,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            QuadtreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }

            //recursividad sobre hijos
            else
            {
                float midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
                float midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

                //00
                testChildVisibility(frustum, children[0], boxLowerX + midX, boxLowerY, boxLowerZ + midZ, boxUpperX, boxUpperY, boxUpperZ);

                //01
                testChildVisibility(frustum, children[1], boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ - midZ);

                //10
                testChildVisibility(frustum, children[2], boxLowerX, boxLowerY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY, boxUpperZ);
                
                //11
                testChildVisibility(frustum, children[3], boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX, boxUpperY, boxUpperZ - midZ);


            }
        }


        /// <summary>
        /// Hacer visible las meshes de un nodo si es visible por el Frustum
        /// </summary>
        private void testChildVisibility(TgcFrustum frustum, QuadtreeNode childNode,
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
        private void addAllLeafMeshes(QuadtreeNode node)
        {
            QuadtreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }
            //pedir hojas a hijos
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
        private void selectLeafMeshes(QuadtreeNode node)
        {
            TgcMesh[] models = node.models;
            foreach (TgcMesh m in models)
            {
                m.Enabled = true;
            }
        }






    }
}
