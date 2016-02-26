using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TGC.Core.Utils;

namespace Examples.Optimizacion.Octree
{
    /// <summary>
    /// Herramienta para crear y utilizar un Octree para renderizar por Frustum Culling
    /// </summary>
    public class Octree
    {

        OctreeNode octreeRootNode;
        List<TgcMesh> modelos;
        TgcBoundingBox sceneBounds;
        OctreeBuilder builder;
        List<TgcDebugBox> debugOctreeBoxes;

        public Octree()
        {
            builder = new OctreeBuilder();
        }

        /// <summary>
        /// Crear nuevo Octree
        /// </summary>
        /// <param name="modelos">Modelos a optimizar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //Crear Octree
            this.octreeRootNode = builder.crearOctree(modelos, sceneBounds);

            //Deshabilitar todos los mesh inicialmente
            foreach (TgcMesh mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Crear meshes para debug
        /// </summary>
        public void createDebugOctreeMeshes()
        {
            debugOctreeBoxes = builder.createDebugOctreeMeshes(octreeRootNode, sceneBounds);
        }

        /// <summary>
        /// Renderizar en forma optimizado utilizando el Octree para hacer FrustumCulling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum, octreeRootNode, 
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
                foreach (TgcDebugBox debugBox in debugOctreeBoxes)
                {
                    debugBox.render();
                }
            }
        }

        /// <summary>
        /// Recorrer recursivamente el Octree para encontrar los nodos visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum, OctreeNode node, 
            float boxLowerX, float boxLowerY, float boxLowerZ, 
            float boxUpperX, float boxUpperY, float boxUpperZ) 
        {
            OctreeNode[] children = node.children;

            //es hoja, cargar todos los meshes
            if (children == null)
            {
                selectLeafMeshes(node);
            }

            //recursividad sobre hijos
            else
            {
                float midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
                float midY = FastMath.Abs((boxUpperY - boxLowerY) / 2);
                float midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

                //000
                testChildVisibility(frustum, children[0], boxLowerX + midX, boxLowerY + midY, boxLowerZ + midZ, boxUpperX, boxUpperY, boxUpperZ);
                //001
                testChildVisibility(frustum, children[1], boxLowerX + midX, boxLowerY + midY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ - midZ);

                //010
                testChildVisibility(frustum, children[2], boxLowerX + midX, boxLowerY, boxLowerZ + midZ, boxUpperX, boxUpperY - midY, boxUpperZ);
                //011
                testChildVisibility(frustum, children[3], boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY - midY, boxUpperZ - midZ);

                //100
                testChildVisibility(frustum, children[4], boxLowerX, boxLowerY + midY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY, boxUpperZ);
                //101
                testChildVisibility(frustum, children[5], boxLowerX, boxLowerY + midY, boxLowerZ, boxUpperX - midX, boxUpperY, boxUpperZ - midZ);

                //110
                testChildVisibility(frustum, children[6], boxLowerX, boxLowerY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY - midY, boxUpperZ);
                //111
                testChildVisibility(frustum, children[7], boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX, boxUpperY - midY, boxUpperZ - midZ);
            }
        }


        /// <summary>
        /// Hacer visible las meshes de un nodo si es visible por el Frustum
        /// </summary>
        private void testChildVisibility(TgcFrustum frustum, OctreeNode childNode,
                float boxLowerX, float boxLowerY, float boxLowerZ, float boxUpperX, float boxUpperY, float boxUpperZ)
        {
            if (childNode == null)
            {
                return;
            }

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
	    private void addAllLeafMeshes(OctreeNode node) {
            if (node == null)
            {
                return;
            }

            OctreeNode[] children = node.children;

		    //es hoja, cargar todos los meshes
		    if( children == null ) 
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
        private void selectLeafMeshes(OctreeNode node)
        {
            TgcMesh[] models = node.models;
            foreach (TgcMesh m in models)
	        {
        		 m.Enabled = true;
	        }
        }
         





    }
}
