using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Optimization.Quadtree
{
    /// <summary>
    ///     Herramienta para construir un Quadtree
    /// </summary>
    internal class QuadtreeBuilder
    {
        //Parametros de corte del QUADTRE
        private readonly int MAX_SECTOR_QUADTREE_RECURSION = 2;

        private readonly int MIN_MESH_PER_LEAVE_THRESHOLD = 5;

        public QuadtreeNode crearQuadtree(List<TgcMesh> TgcMeshs, TgcBoundingAxisAlignBox sceneBounds)
        {
            var rootNode = new QuadtreeNode();

            //Calcular punto medio y centro
            var midSize = sceneBounds.calculateAxisRadius();
            var center = sceneBounds.calculateBoxCenter();

            //iniciar generacion recursiva de octree
            doSectorQuadtreeX(rootNode, center, midSize, 0, TgcMeshs);

            //podar nodos innecesarios
            optimizeSectorQuadtree(rootNode.children);

            //imprimir por consola el octree
            //printDebugQuadtree(rootNode);

            //imprimir estadisticas de debug
            //printEstadisticasQuadtree(rootNode);

            return rootNode;
        }

        /// <summary>
        ///     Corte con plano X
        /// </summary>
        private void doSectorQuadtreeX(QuadtreeNode parent, TGCVector3 center, TGCVector3 size,
            int step, List<TgcMesh> meshes)
        {
            var x = center.X;

            //Crear listas para realizar corte
            var possitiveList = new List<TgcMesh>();
            var negativeList = new List<TgcMesh>();

            //X-cut
            var xCutPlane = new TGCPlane(1, 0, 0, -x);
            splitByPlane(xCutPlane, meshes, possitiveList, negativeList);

            //recursividad de positivos con plano Z, usando resultados positivos y childIndex 0
            doSectorQuadtreeZ(parent, new TGCVector3(x + size.X / 2, center.Y, center.Z),
                new TGCVector3(size.X / 2, size.Y, size.Z),
                step, possitiveList, 0);

            //recursividad de negativos con plano Z, usando resultados negativos y childIndex 4
            doSectorQuadtreeZ(parent, new TGCVector3(x - size.X / 2, center.Y, center.Z),
                new TGCVector3(size.X / 2, size.Y, size.Z),
                step, negativeList, 2);
        }

        /// <summary>
        ///     Corte de plano Z
        /// </summary>
        private void doSectorQuadtreeZ(QuadtreeNode parent, TGCVector3 center, TGCVector3 size, int step,
            List<TgcMesh> meshes, int childIndex)
        {
            var z = center.Z;

            //Crear listas para realizar corte
            var possitiveList = new List<TgcMesh>();
            var negativeList = new List<TgcMesh>();

            //Z-cut
            var zCutPlane = new TGCPlane(0, 0, 1, -z);
            splitByPlane(zCutPlane, meshes, possitiveList, negativeList);

            //obtener lista de children del parent, con iniciacion lazy
            if (parent.children == null)
            {
                parent.children = new QuadtreeNode[4];
            }

            //crear nodo positivo en parent, segun childIndex
            var posNode = new QuadtreeNode();
            parent.children[childIndex] = posNode;

            //cargar nodo negativo en parent, segun childIndex
            var negNode = new QuadtreeNode();
            parent.children[childIndex + 1] = negNode;

            //condicion de corte
            if (step > MAX_SECTOR_QUADTREE_RECURSION || meshes.Count < MIN_MESH_PER_LEAVE_THRESHOLD)
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
                doSectorQuadtreeX(posNode, new TGCVector3(center.X, center.Y, z + size.Z / 2),
                    new TGCVector3(size.X, size.Y, size.Z / 2),
                    step, possitiveList);

                //recursividad de negativos con plano Y, usando resultados negativos
                doSectorQuadtreeX(negNode, new TGCVector3(center.X, center.Y, z - size.Z / 2),
                    new TGCVector3(size.X, size.Y, size.Z / 2),
                    step, negativeList);
            }
        }

        /// <summary>
        ///     Separa los modelos en dos listas, segun el testo contra el plano de corte
        /// </summary>
        private void splitByPlane(TGCPlane cutPlane, List<TgcMesh> modelos,
            List<TgcMesh> possitiveList, List<TgcMesh> negativeList)
        {
            TgcCollisionUtils.PlaneBoxResult c;
            foreach (var modelo in modelos)
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
        ///     Se quitan padres cuyos nodos no tengan ningun triangulo
        /// </summary>
        private void optimizeSectorQuadtree(QuadtreeNode[] children)
        {
            if (children == null)
            {
                return;
            }

            for (var i = 0; i < children.Length; i++)
            {
                var childNode = children[i];
                var childNodeChildren = childNode.children;
                if (childNodeChildren != null && hasEmptyChilds(childNode))
                {
                    childNode.children = null;
                    childNode.models = new TgcMesh[0];
                }
                else
                {
                    optimizeSectorQuadtree(childNodeChildren);
                }
            }
        }

        /// <summary>
        ///     Se fija si los hijos de un nodo no tienen mas hijos y no tienen ningun triangulo
        /// </summary>
        private bool hasEmptyChilds(QuadtreeNode node)
        {
            var children = node.children;
            for (var i = 0; i < children.Length; i++)
            {
                var childNode = children[i];
                if (childNode.children != null || childNode.models.Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Imprime por consola la generacion del Octree
        /// </summary>
        private void printDebugQuadtree(QuadtreeNode rootNode)
        {
            Console.WriteLine("########## Quadtree DEBUG ##########");
            var sb = new StringBuilder();
            doPrintDebugQuadtree(rootNode, 0, sb);
            Console.WriteLine(sb.ToString());
            Console.WriteLine("########## FIN Quadtree DEBUG ##########");
        }

        /// <summary>
        ///     Impresion recursiva
        /// </summary>
        private void doPrintDebugQuadtree(QuadtreeNode node, int index, StringBuilder sb)
        {
            var lineas = "";
            for (var i = 0; i < index; i++)
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
                for (var i = 0; i < node.children.Length; i++)
                {
                    doPrintDebugQuadtree(node.children[i], index, sb);
                }
            }
        }

        /// <summary>
        ///     Dibujar meshes que representan los sectores del Quadtree
        /// </summary>
        public List<TGCBoxDebug> createDebugQuadtreeMeshes(QuadtreeNode rootNode, TgcBoundingAxisAlignBox sceneBounds)
        {
            var pMax = sceneBounds.PMax;
            var pMin = sceneBounds.PMin;

            var debugBoxes = new List<TGCBoxDebug>();
            doCreateQuadtreeDebugBox(rootNode, debugBoxes,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z, 0);

            return debugBoxes;
        }

        private void doCreateQuadtreeDebugBox(QuadtreeNode node, List<TGCBoxDebug> debugBoxes,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ, int step)
        {
            var children = node.children;

            var midX = FastMath.Abs((boxUpperX - boxLowerX) / 2);
            var midZ = FastMath.Abs((boxUpperZ - boxLowerZ) / 2);

            //Crear caja debug
            var box = createDebugBox(boxLowerX, boxLowerY, boxLowerZ, boxUpperX, boxUpperY, boxUpperZ, step);
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
                doCreateQuadtreeDebugBox(children[0], debugBoxes, boxLowerX + midX, boxLowerY, boxLowerZ + midZ,
                    boxUpperX, boxUpperY, boxUpperZ, step);
                //001
                doCreateQuadtreeDebugBox(children[1], debugBoxes, boxLowerX + midX, boxLowerY, boxLowerZ, boxUpperX,
                    boxUpperY, boxUpperZ - midZ, step);

                //100
                doCreateQuadtreeDebugBox(children[2], debugBoxes, boxLowerX, boxLowerY, boxLowerZ + midZ,
                    boxUpperX - midX, boxUpperY, boxUpperZ, step);
                //101
                doCreateQuadtreeDebugBox(children[3], debugBoxes, boxLowerX, boxLowerY, boxLowerZ, boxUpperX - midX,
                    boxUpperY, boxUpperZ - midZ, step);
            }
        }

        /// <summary>
        ///     Construir caja debug
        /// </summary>
        private TGCBoxDebug createDebugBox(float boxLowerX, float boxLowerY, float boxLowerZ,
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
            var box = TGCBoxDebug.fromExtremes(
                new TGCVector3(boxLowerX, boxLowerY, boxLowerZ),
                new TGCVector3(boxUpperX, boxUpperY, boxUpperZ),
                c, thickness);

            return box;
        }

        /// <summary>
        ///     Imprime estadisticas del Octree
        /// </summary>
        private void printEstadisticasQuadtree(QuadtreeNode rootNode)
        {
            Console.WriteLine("*********** Quadtree Statics ***********");

            var minModels = int.MaxValue;
            var maxModels = int.MinValue;

            obtenerEstadisticas(rootNode, ref minModels, ref maxModels);

            Console.WriteLine("Minima cantidad de TgcMeshs en hoja: " + minModels);
            Console.WriteLine("Maxima cantidad de TgcMeshs en hoja: " + maxModels);

            Console.WriteLine("*********** FIN Quadtree Statics ************");
        }

        private void obtenerEstadisticas(QuadtreeNode node, ref int minModels, ref int maxModels)
        {
            if (node.isLeaf())
            {
                var n = node.models.Length;
                if (n < minModels)
                    minModels = n;
                if (n > maxModels)
                    maxModels = n;
            }
            else
            {
                for (var i = 0; i < node.children.Length; i++)
                {
                    obtenerEstadisticas(node.children[i], ref minModels, ref maxModels);
                }
            }
        }
    }
}