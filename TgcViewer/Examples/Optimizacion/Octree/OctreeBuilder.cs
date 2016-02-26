using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using System.Drawing;
using TGC.Core.Utils;

namespace Examples.Optimizacion.Octree
{
    /// <summary>
    /// Herramienta para construir un Octree
    /// </summary>
    class OctreeBuilder
    {
        
        //Parametros de corte del Octree
	    int MAX_SECTOR_OCTREE_RECURSION = 3;
	    int MIN_MESH_PER_LEAVE_THRESHOLD = 5;

        public OctreeNode crearOctree(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            OctreeNode rootNode = new OctreeNode();

            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;

            //Calcular punto medio y centro
            Vector3 midSize = sceneBounds.calculateAxisRadius();
            Vector3 center = sceneBounds.calculateBoxCenter();

            //iniciar generacion recursiva de octree
            doSectorOctreeX(rootNode, center, midSize, 0, modelos);

            //podar nodos innecesarios
            deleteEmptyNodes(rootNode.children);

            //eliminar hijos que subdividen sin necesidad
            //deleteSameMeshCountChilds(rootNode);

            //imprimir por consola el octree
            //printDebugOctree(rootNode);

            //imprimir estadisticas de debug
            //printEstadisticasOctree(rootNode);

            return rootNode;
        }

        

        

        /// <summary>
        /// Corte con plano X
        /// </summary>
        private void doSectorOctreeX(OctreeNode parent, Vector3 center, Vector3 size,
            int step, List<TgcMesh> meshes)
        {

            float x = center.X;

            //Crear listas para realizar corte
            List<TgcMesh> possitiveList = new List<TgcMesh>();
            List<TgcMesh> negativeList = new List<TgcMesh>();

            //X-cut
            Plane xCutPlane = new Plane(1, 0, 0, -x);
            splitByPlane(xCutPlane, meshes, possitiveList, negativeList);

            //recursividad de positivos con plano Y, usando resultados positivos y childIndex 0
            doSectorOctreeY(parent, new Vector3(x + size.X / 2, center.Y, center.Z),
                    new Vector3(size.X / 2, size.Y, size.Z),
                    step, possitiveList, 0);

            //recursividad de negativos con plano Y, usando resultados negativos y childIndex 4
            doSectorOctreeY(parent, new Vector3(x - size.X / 2, center.Y, center.Z),
                    new Vector3(size.X / 2, size.Y, size.Z),
                    step, negativeList , 4);
        }

        /// <summary>
        /// Corte con plano Y
        /// </summary>
        private void doSectorOctreeY(OctreeNode parent, Vector3 center, Vector3 size, int step,
            List<TgcMesh> meshes, int childIndex)
        {
            float y = center.Y;

            //Crear listas para realizar corte
            List<TgcMesh> possitiveList = new List<TgcMesh>();
            List<TgcMesh> negativeList = new List<TgcMesh>();

            //Y-cut
            Plane yCutPlane = new Plane(0, 1, 0, -y);
            splitByPlane(yCutPlane, meshes, possitiveList, negativeList);

            //recursividad de positivos con plano Z, usando resultados positivos y childIndex 0
            doSectorOctreeZ(parent, new Vector3(center.X, y + size.Y / 2, center.Z),
                    new Vector3(size.X, size.Y / 2, size.Z),
                    step, possitiveList, childIndex + 0);

            //recursividad de negativos con plano Z, usando plano X negativo y childIndex 2
            doSectorOctreeZ(parent, new Vector3(center.X, y - size.Y / 2, center.Z),
                    new Vector3(size.X, size.Y / 2, size.Z),
                    step, negativeList, childIndex + 2);
        }

        /// <summary>
        /// Corte de plano Z
        /// </summary>
        private void doSectorOctreeZ(OctreeNode parent, Vector3 center, Vector3 size, int step,
            List<TgcMesh> meshes, int childIndex)
        {
            float z = center.Z;

            //Crear listas para realizar corte
            List<TgcMesh> possitiveList = new List<TgcMesh>();
            List<TgcMesh> negativeList = new List<TgcMesh>();

            //Z-cut
            Plane zCutPlane = new Plane(0, 0, 1, -z);
            splitByPlane(zCutPlane, meshes, possitiveList, negativeList);

            //obtener lista de children del parent, con iniciacion lazy
            if (parent.children == null)
            {
                parent.children = new OctreeNode[8];
            }

            //crear nodo positivo en parent, segun childIndex
            OctreeNode posNode = new OctreeNode();
            parent.children[childIndex] = posNode;

            //cargar nodo negativo en parent, segun childIndex
            OctreeNode negNode = new OctreeNode();
            parent.children[childIndex + 1] = negNode;


            //condicion de corte
            if (step >= MAX_SECTOR_OCTREE_RECURSION || meshes.Count <= MIN_MESH_PER_LEAVE_THRESHOLD)
            {
                //cargar hijos de nodo positivo
                posNode.models = possitiveList.ToArray();

                //cargar hijos de nodo negativo
                negNode.models = negativeList.ToArray();

            //seguir recursividad
            }
            else
            {
                step++;

                //recursividad de positivos con plano X, usando resultados positivos
                doSectorOctreeX(posNode, new Vector3(center.X, center.Y, z + size.Z / 2),
                        new Vector3(size.X, size.Y, size.Z / 2),
                        step, possitiveList);

                //recursividad de negativos con plano Y, usando resultados negativos
                doSectorOctreeX(negNode, new Vector3(center.X, center.Y, z - size.Z / 2),
                        new Vector3(size.X, size.Y, size.Z / 2),
                        step, negativeList);
            }
        }


        /// <summary>
        /// Separa los modelos en dos listas, segun el testo contra el plano de corte
        /// </summary>
        private void splitByPlane(Plane cutPlane, List<TgcMesh> modelos,
            List<TgcMesh> possitiveList, List<TgcMesh> negativeList) 
        {
            TgcCollisionUtils.PlaneBoxResult c;
            foreach (TgcMesh modelo in modelos)
	        {
                c = TgcCollisionUtils.classifyPlaneAABB(cutPlane, modelo.BoundingBox);

                 //possitive side
                if (c == TgcCollisionUtils.PlaneBoxResult.IN_FRONT_OF)
                {
                    possitiveList.Add(modelo);
                }

                //negative side
                else if (c == TgcCollisionUtils.PlaneBoxResult.BEHIND)
                {
                    negativeList.Add(modelo);
                }

                //both sides
                else
                {
                    possitiveList.Add(modelo);
                    negativeList.Add(modelo);
                }
	        }
	    }


     
        /// <summary>
        /// Se quitan padres cuyos nodos no tengan ningun triangulo
        /// </summary>
        private void deleteEmptyNodes(OctreeNode[] children)
        {
            if (children == null)
            {
                return;
            }

            for (int i = 0; i < children.Length; i++)
            {
                OctreeNode childNode = children[i];
                OctreeNode[] childNodeChildren = childNode.children;
                if (childNodeChildren != null && hasEmptyChilds(childNode))
                {
                    childNode.children = null;
                    childNode.models = new TgcMesh[0];
                }
                else
                {
                    deleteEmptyNodes(childNodeChildren);
                }
            }
        }

        /// <summary>
        /// Se fija si los hijos de un nodo no tienen mas hijos y no tienen ningun triangulo
        /// </summary>
        private bool hasEmptyChilds(OctreeNode node)
        {
            OctreeNode[] children = node.children;
            for (int i = 0; i < children.Length; i++)
            {
                OctreeNode childNode = children[i];
                if (childNode.children != null || childNode.models.Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Se quitan nodos cuyos hijos siguen teniendo la misma cantidad de modelos que el padre
        /// </summary>
        private void deleteSameMeshCountChilds(OctreeNode node)
        {
            if (node == null || node.children == null)
            {
                return;
            }

            int nodeCount = getTotalNodeMeshCount(node);
            for (int i = 0; i < node.children.Length; i++)
            {
                OctreeNode childNode = node.children[i];
                int childCount = getTotalNodeMeshCount(childNode);
                if (childCount == nodeCount)
                {
                    node.children[i] = null;
                }
                else
                {
                    deleteSameMeshCountChilds(node.children[i]);
                }

            }
        }

        /// <summary>
        /// Cantidad de meshes que tiene un nodo, contando todos los de sus hijos recursivamente
        /// </summary>
        private int getTotalNodeMeshCount(OctreeNode node)
        {
            if (node.children == null)
            {
                return node.models.Length;
            }

            int meshCount = 0;
            for (int i = 0; i < node.children.Length; i++)
            {
                meshCount += getTotalNodeMeshCount(node.children[i]);
            }
            return meshCount;
        }


        /// <summary>
        /// Imprime por consola la generacion del Octree
        /// </summary>
        private void printDebugOctree(OctreeNode rootNode)
        {
            Console.WriteLine("########## Octree DEBUG ##########");
            StringBuilder sb = new StringBuilder();
            doPrintDebugOctree(rootNode, 0, sb);
            Console.WriteLine(sb.ToString());
            Console.WriteLine("########## FIN Octree DEBUG ##########");
        }

        /// <summary>
        /// Impresion recursiva
        /// </summary>
        private void doPrintDebugOctree(OctreeNode node, int index, StringBuilder sb)
        {
            if (node == null)
            {
                return;
            }

            String lineas = "";
            for (int i = 0; i < index; i++)
            {
                lineas += "-";
            }

            if (node.isLeaf())
            {
                if (node.models.Length > 0)
                {
                    sb.Append(lineas + "Models [" + node.models.Length + "]" + "\n");
                }
                else
                {
                    sb.Append(lineas + "[0]" + "\n");
                }
                
            }
            else
            {
                sb.Append(lineas + "\n");
                index++;
                for (int i = 0; i < node.children.Length; i++)
			    {
    			    doPrintDebugOctree(node.children[i], index, sb);
			    }
            }
        }

        /// <summary>
        /// Dibujar meshes que representan los sectores del Octree
        /// </summary>
        public List<TgcDebugBox> createDebugOctreeMeshes(OctreeNode rootNode, TgcBoundingBox sceneBounds)
        {
            Vector3 pMax = sceneBounds.PMax;
            Vector3 pMin = sceneBounds.PMin;

            List<TgcDebugBox> debugBoxes = new List<TgcDebugBox>();
            doCreateOctreeDebugBox(rootNode, debugBoxes,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z, 0);

            return debugBoxes;
        }

        private void doCreateOctreeDebugBox(OctreeNode node, List<TgcDebugBox> debugBoxes,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ, int step)
        {
            if (node == null)
            {
                return;
            }

            OctreeNode[] children = node.children;

		    float midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
            float midY = FastMath.Abs((boxUpperY - boxLowerY) / 2);
            float midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

            //Crear caja debug
            TgcDebugBox box = createDebugBox(boxLowerX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ, step);
            debugBoxes.Add(box);

            //es hoja, dibujar caja
            if (children == null)
            {

            }

            //recursividad sobre hijos
            else
            {
                step++;

                //000
                doCreateOctreeDebugBox(children[0], debugBoxes, boxLowerX + midX, boxLowerY + midY, boxLowerZ + midZ, boxUpperX, boxUpperY, boxUpperZ, step);
                //001
                doCreateOctreeDebugBox(children[1], debugBoxes, boxLowerX + midX, boxLowerY + midY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ - midZ, step);

                //010
                doCreateOctreeDebugBox(children[2], debugBoxes, boxLowerX + midX, boxLowerY, boxLowerZ + midZ, boxUpperX, boxUpperY - midY, boxUpperZ, step);
                //011
                doCreateOctreeDebugBox(children[3], debugBoxes, boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY - midY, boxUpperZ - midZ, step);

                //100
                doCreateOctreeDebugBox(children[4], debugBoxes, boxLowerX, boxLowerY + midY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY, boxUpperZ, step);
                //101
                doCreateOctreeDebugBox(children[5], debugBoxes, boxLowerX, boxLowerY + midY, boxLowerZ, boxUpperX - midX, boxUpperY, boxUpperZ - midZ, step);

                //110
                doCreateOctreeDebugBox(children[6], debugBoxes, boxLowerX, boxLowerY, boxLowerZ + midZ, boxUpperX - midX, boxUpperY - midY, boxUpperZ, step);
                //111
                doCreateOctreeDebugBox(children[7], debugBoxes, boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX, boxUpperY - midY, boxUpperZ - midZ, step);
            }
			    

        }

        /// <summary>
        /// Construir caja debug
        /// </summary>
        private TgcDebugBox createDebugBox(float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ, int step)
        {
            //Determinar color y grosor según profundidad
            Color c;
            float thickness;
            switch (step)
            {
                case 0:
                    c = Color.Red;
                    thickness = 4f;
                    break;
                case 1:
                    c = Color.Violet;
                    thickness = 3f;
                    break;
                case 2:
                    c = Color.Brown;
                    thickness = 2f;
                    break;
                case 3:
                    c = Color.Gold;
                    thickness = 1f;
                    break;
                default:
                    c = Color.Orange;
                    thickness = 0.5f;
                    break;
            }

            //Crear caja Debug
            TgcDebugBox box = TgcDebugBox.fromExtremes(
                new Vector3(boxLowerX, boxLowerY, boxLowerZ),
                new Vector3(boxUpperX, boxUpperY, boxUpperZ), 
                c, thickness);

            return box;
        }



        /// <summary>
        /// Imprime estadisticas del Octree
        /// </summary>
        private void printEstadisticasOctree(OctreeNode rootNode)
        {
            Console.WriteLine("*********** Octree Statics ***********");

            int minModels = int.MaxValue;
            int maxModels = int.MinValue;

            obtenerEstadisticas(rootNode, ref minModels, ref maxModels);

            Console.WriteLine("Minima cantidad de modelos en hoja: " + minModels);
            Console.WriteLine("Maxima cantidad de modelos en hoja: " + maxModels);


            Console.WriteLine("*********** FIN Octree Statics ************");
        }

        private void obtenerEstadisticas(OctreeNode node, ref int minModels, ref int maxModels)
        {
            if (node.isLeaf())
            {
                int n = node.models.Length;
                if (n < minModels)
                    minModels = n;
                if (n > maxModels)
                    maxModels = n;
            }
            else
            {
                for (int i = 0; i < node.children.Length; i++)
                {
                    obtenerEstadisticas(node.children[i], ref minModels, ref maxModels);
                }
            }
        }
    }
}
