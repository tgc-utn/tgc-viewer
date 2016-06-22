using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Optimization.KdtTree
{
    /// <summary>
    ///     Herramienta para construir un KdTree
    /// </summary>
    internal class KdTreeBuilder
    {
        //Parametros de generacion de planos de corte
        private readonly float D_DESPLAZAMIENTO = 1;

        //Parametros de corte del KdTree
        private readonly int MAX_SECTOR_KDTREE_RECURSION = 3;

        private readonly int MIN_MESH_PER_LEAVE_THRESHOLD = 10;

        private readonly float MIN_VOL = 10;

        public KdTreeNode crearKdTree(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            var rootNode = new KdTreeNode();

            //iniciar generacion recursiva de KdTree
            doSectorKdTreeX(rootNode, sceneBounds.PMin, sceneBounds.PMax, 0, modelos);

            //podar nodos innecesarios
            optimizeSectorKdTree(rootNode.children);

            //imprimir por consola el KdTree
            //printDebugKdTree(rootNode);

            //imprimir estadisticas de debug
            //printEstadisticasKdTree(rootNode);

            return rootNode;
        }

        /// <summary>
        ///     Corte con plano X
        /// </summary>
        private void doSectorKdTreeX(KdTreeNode parent, Vector3 pMin, Vector3 pMax,
            int step, List<TgcMesh> meshes)
        {
            //Crear listas para realizar corte
            var possitiveList = new List<TgcMesh>();
            var negativeList = new List<TgcMesh>();

            //X-cut
            float cutValue = 0;
            var xCutPlane = getCutPlane(meshes, new Vector3(1, 0, 0), pMin.X, pMax.X, ref cutValue);
            splitByPlane(xCutPlane, meshes, possitiveList, negativeList);

            //recursividad de positivos con plano Y, usando resultados positivos y childIndex 0
            doSectorKdTreeY(parent,
                new Vector3(cutValue, pMin.Y, pMin.Z),
                pMax,
                step, possitiveList, 0, cutValue);

            //recursividad de negativos con plano Y, usando resultados negativos y childIndex 4
            doSectorKdTreeY(parent,
                pMin,
                new Vector3(cutValue, pMax.Y, pMax.Z),
                step, negativeList, 4, cutValue);
        }

        /// <summary>
        ///     Corte con plano Y
        /// </summary>
        private void doSectorKdTreeY(KdTreeNode parent, Vector3 pMin, Vector3 pMax, int step,
            List<TgcMesh> meshes, int childIndex, float xCutValue)
        {
            //Crear listas para realizar corte
            var possitiveList = new List<TgcMesh>();
            var negativeList = new List<TgcMesh>();

            //Y-cut
            float cutValue = 0;
            var yCutPlane = getCutPlane(meshes, new Vector3(0, 1, 0), pMin.Y, pMax.Y, ref cutValue);
            splitByPlane(yCutPlane, meshes, possitiveList, negativeList);

            //recursividad de positivos con plano Z, usando resultados positivos y childIndex 0
            doSectorKdTreeZ(parent,
                new Vector3(pMin.X, cutValue, pMin.Z),
                pMax,
                step, possitiveList, childIndex + 0, xCutValue, cutValue);

            //recursividad de negativos con plano Z, usando plano X negativo y childIndex 2
            doSectorKdTreeZ(parent,
                pMin,
                new Vector3(pMax.X, cutValue, pMax.Z),
                step, negativeList, childIndex + 2, xCutValue, cutValue);
        }

        /// <summary>
        ///     Corte de plano Z
        /// </summary>
        private void doSectorKdTreeZ(KdTreeNode parent, Vector3 pMin, Vector3 pMax, int step,
            List<TgcMesh> meshes, int childIndex, float xCutValue, float yCutValue)
        {
            //Crear listas para realizar corte
            var possitiveList = new List<TgcMesh>();
            var negativeList = new List<TgcMesh>();

            //Z-cut
            float cutValue = 0;
            var zCutPlane = getCutPlane(meshes, new Vector3(0, 0, 1), pMin.Z, pMax.Z, ref cutValue);
            splitByPlane(zCutPlane, meshes, possitiveList, negativeList);

            //obtener lista de children del parent, con iniciacion lazy
            if (parent.children == null)
            {
                parent.children = new KdTreeNode[8];
            }

            //crear nodo positivo en parent, segun childIndex
            var posNode = new KdTreeNode();
            parent.children[childIndex] = posNode;

            //cargar nodo negativo en parent, segun childIndex
            var negNode = new KdTreeNode();
            parent.children[childIndex + 1] = negNode;

            //cargar cortes en parent
            parent.xCut = xCutValue;
            parent.yCut = yCutValue;
            parent.zCut = cutValue;

            //nuevos limites
            var v1 = new Vector3(pMax.X - pMin.X, pMax.Y - pMin.Y, pMax.Z - cutValue);
            var v2 = new Vector3(pMax.X - pMin.X, pMax.Y - pMin.Y, cutValue - pMin.Z);

            //condicion de corte
            if (step >= MAX_SECTOR_KDTREE_RECURSION || meshes.Count <= MIN_MESH_PER_LEAVE_THRESHOLD
                || v1.X < MIN_VOL || v1.Y < MIN_VOL || v1.Z < MIN_VOL
                || v2.X < MIN_VOL || v2.Y < MIN_VOL || v2.Z < MIN_VOL
                )
            {
                //cargar hijos de nodo positivo
                posNode.models = possitiveList.ToArray();

                //cargar hijos de nodo negativo
                negNode.models = negativeList.ToArray();
            }
            //seguir recursividad
            else
            {
                step++;

                //recursividad de positivos con plano X, usando resultados positivos
                doSectorKdTreeX(posNode,
                    new Vector3(pMin.X, pMin.Y, cutValue),
                    pMax,
                    step, possitiveList);

                //recursividad de negativos con plano Y, usando resultados negativos
                doSectorKdTreeX(negNode,
                    pMin,
                    new Vector3(pMax.X, pMax.Y, cutValue),
                    step, negativeList);
            }
        }

        /// <summary>
        ///     Obtiene el mejor plano de corte recto, en el volumen dado, en la direccion dada
        /// </summary>
        private Plane getCutPlane(List<TgcMesh> modelos, Vector3 n, float pMin, float pMax, ref float cutValue)
        {
            var vueltas = (int)((pMax - pMin) / D_DESPLAZAMIENTO);
            var bestBalance = int.MaxValue;
            var bestPlane = Plane.Empty;
            cutValue = 0;

            for (var i = 0; i < vueltas; i++)
            {
                //crear plano de corte
                var currentCutValue = pMin + D_DESPLAZAMIENTO * i;
                var p = new Plane(n.X, n.Y, n.Z, -currentCutValue);

                //clasificar todos los modelos contra ese plano
                var possitiveList = new List<TgcMesh>();
                var negativeList = new List<TgcMesh>();
                splitByPlane(p, modelos, possitiveList, negativeList);

                //calcular balance
                var balance = Math.Abs(possitiveList.Count - negativeList.Count);

                //guardar mejor
                if (balance < bestBalance)
                {
                    bestBalance = balance;
                    bestPlane = p;
                    cutValue = currentCutValue;
                }
            }

            return bestPlane;
        }

        /// <summary>
        ///     Separa los modelos en dos listas, segun el testo contra el plano de corte
        /// </summary>
        private void splitByPlane(Plane cutPlane, List<TgcMesh> modelos,
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
        ///     Separa los modelos en dos listas, segun el testo contra el plano de corte.
        ///     No tiene en cuenta los que estan atravezando
        /// </summary>
        private void splitByPlaneWithoutRepeating(Plane cutPlane, List<TgcMesh> modelos,
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
            }
        }

        /// <summary>
        ///     Se quitan padres cuyos nodos no tengan ningun triangulo
        /// </summary>
        private void optimizeSectorKdTree(KdTreeNode[] children)
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
                    optimizeSectorKdTree(childNodeChildren);
                }
            }
        }

        /// <summary>
        ///     Se fija si los hijos de un nodo no tienen mas hijos y no tienen ningun triangulo
        /// </summary>
        private bool hasEmptyChilds(KdTreeNode node)
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
        ///     Imprime por consola la generacion del KdTree
        /// </summary>
        private void printDebugKdTree(KdTreeNode rootNode)
        {
            Console.WriteLine("########## KdTree DEBUG ##########");
            var sb = new StringBuilder();
            doPrintDebugKdTree(rootNode, 0, sb);
            Console.WriteLine(sb.ToString());
            Console.WriteLine("########## FIN KdTree DEBUG ##########");
        }

        /// <summary>
        ///     Impresion recursiva
        /// </summary>
        private void doPrintDebugKdTree(KdTreeNode node, int index, StringBuilder sb)
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
                    doPrintDebugKdTree(node.children[i], index, sb);
                }
            }
        }

        /// <summary>
        ///     Dibujar meshes que representan los sectores del KdTree
        /// </summary>
        public List<TgcDebugBox> createDebugKdTreeMeshes(KdTreeNode rootNode, TgcBoundingBox sceneBounds)
        {
            var pMax = sceneBounds.PMax;
            var pMin = sceneBounds.PMin;

            var debugBoxes = new List<TgcDebugBox>();
            doCreateKdTreeDebugBox(rootNode, debugBoxes,
                pMin.X, pMin.Y, pMin.Z,
                pMax.X, pMax.Y, pMax.Z, 0);

            return debugBoxes;
        }

        private void doCreateKdTreeDebugBox(KdTreeNode node, List<TgcDebugBox> debugBoxes,
            float boxLowerX, float boxLowerY, float boxLowerZ,
            float boxUpperX, float boxUpperY, float boxUpperZ, int step)
        {
            var children = node.children;

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

                var xCut = node.xCut;
                var yCut = node.yCut;
                var zCut = node.zCut;

                //000
                doCreateKdTreeDebugBox(children[0], debugBoxes, xCut, yCut, zCut, boxUpperX, boxUpperY, boxUpperZ, step);
                //001
                doCreateKdTreeDebugBox(children[1], debugBoxes, xCut, yCut, boxLowerZ, boxUpperX, boxUpperY, zCut, step);

                //010
                doCreateKdTreeDebugBox(children[2], debugBoxes, xCut, boxLowerY, zCut, boxUpperX, yCut, boxUpperZ, step);
                //011
                doCreateKdTreeDebugBox(children[3], debugBoxes, xCut, boxLowerY, boxLowerZ, boxUpperX, yCut, zCut, step);

                //100
                doCreateKdTreeDebugBox(children[4], debugBoxes, boxLowerX, yCut, zCut, xCut, boxUpperY, boxUpperZ, step);
                //101
                doCreateKdTreeDebugBox(children[5], debugBoxes, boxLowerX, yCut, boxLowerZ, xCut, boxUpperY, zCut, step);

                //110
                doCreateKdTreeDebugBox(children[6], debugBoxes, boxLowerX, boxLowerY, zCut, xCut, yCut, boxUpperZ, step);
                //111
                doCreateKdTreeDebugBox(children[7], debugBoxes, boxLowerX, boxLowerY, boxLowerZ, xCut, yCut, zCut, step);
            }
        }

        /// <summary>
        ///     Construir caja debug
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
            var box = TgcDebugBox.fromExtremes(
                new Vector3(boxLowerX, boxLowerY, boxLowerZ),
                new Vector3(boxUpperX, boxUpperY, boxUpperZ),
                c, thickness);

            return box;
        }

        /// <summary>
        ///     Imprime estadisticas del KdTree
        /// </summary>
        private void printEstadisticasKdTree(KdTreeNode rootNode)
        {
            Console.WriteLine("*********** KdTree Statics ***********");

            var minModels = int.MaxValue;
            var maxModels = int.MinValue;

            obtenerEstadisticas(rootNode, ref minModels, ref maxModels);

            Console.WriteLine("Minima cantidad de modelos en hoja: " + minModels);
            Console.WriteLine("Maxima cantidad de modelos en hoja: " + maxModels);

            Console.WriteLine("*********** FIN Octree Statics ************");
        }

        private void obtenerEstadisticas(KdTreeNode node, ref int minModels, ref int maxModels)
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