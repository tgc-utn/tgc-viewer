using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Utils;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.Optimizacion.GrillaRegular
{
    /// <summary>
    ///     Herramienta para crear y usar la Grilla Regular
    /// </summary>
    public class GrillaRegular
    {
        private readonly float CELL_HEIGHT = 400;
        private readonly float CELL_LENGTH = 400;

        //Tamaños de celda de la grilla
        private readonly float CELL_WIDTH = 400;

        private List<TgcDebugBox> debugBoxes;
        private GrillaRegularNode[,,] grid;

        private List<TgcMesh> modelos;
        private TgcBoundingBox sceneBounds;

        /// <summary>
        ///     Crear una nueva grilla
        /// </summary>
        /// <param name="modelos">Modelos a contemplar</param>
        /// <param name="sceneBounds">Límites del escenario</param>
        public void create(List<TgcMesh> modelos, TgcBoundingBox sceneBounds)
        {
            this.modelos = modelos;
            this.sceneBounds = sceneBounds;

            //build
            grid = buildGrid(modelos, sceneBounds, new Vector3(CELL_WIDTH, CELL_HEIGHT, CELL_LENGTH));

            foreach (var mesh in modelos)
            {
                mesh.Enabled = false;
            }
        }

        /// <summary>
        ///     Construye la grilla
        /// </summary>
        private GrillaRegularNode[,,] buildGrid(List<TgcMesh> modelos, TgcBoundingBox sceneBounds, Vector3 cellDim)
        {
            var sceneSize = sceneBounds.calculateSize();

            var gx = (int)FastMath.Ceiling(sceneSize.X / cellDim.X) + 1;
            var gy = (int)FastMath.Ceiling(sceneSize.Y / cellDim.Y) + 1;
            var gz = (int)FastMath.Ceiling(sceneSize.Z / cellDim.Z) + 1;

            var grid = new GrillaRegularNode[gx, gy, gz];

            //Construir grilla
            for (var x = 0; x < gx; x++)
            {
                for (var y = 0; y < gy; y++)
                {
                    for (var z = 0; z < gz; z++)
                    {
                        //Crear celda
                        var node = new GrillaRegularNode();

                        //Crear BoundingBox de celda
                        var pMin = new Vector3(sceneBounds.PMin.X + x * cellDim.X, sceneBounds.PMin.Y + y * cellDim.Y,
                            sceneBounds.PMin.Z + z * cellDim.Z);
                        var pMax = Vector3.Add(pMin, cellDim);
                        node.BoundingBox = new TgcBoundingBox(pMin, pMax);

                        //Cargar modelos en celda
                        node.Models = new List<TgcMesh>();
                        addModelsToCell(node, modelos);

                        grid[x, y, z] = node;
                    }
                }
            }

            return grid;
        }

        /// <summary>
        ///     Agregar modelos a una celda
        /// </summary>
        private void addModelsToCell(GrillaRegularNode node, List<TgcMesh> modelos)
        {
            foreach (var mesh in modelos)
            {
                if (TgcCollisionUtils.testAABBAABB(node.BoundingBox, mesh.BoundingBox))
                {
                    node.Models.Add(mesh);
                }
            }
        }

        /// <summary>
        ///     Crear meshes debug
        /// </summary>
        public void createDebugMeshes()
        {
            debugBoxes = new List<TgcDebugBox>();

            for (var x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (var y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (var z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        var node = grid[x, y, z];
                        var box = TgcDebugBox.fromExtremes(node.BoundingBox.PMin, node.BoundingBox.PMax, Color.Red);

                        debugBoxes.Add(box);
                    }
                }
            }
        }

        /// <summary>
        ///     Dibujar objetos de la isla en forma optimizada, utilizando la grilla para Frustm Culling
        /// </summary>
        public void render(TgcFrustum frustum, bool debugEnabled)
        {
            var pMax = sceneBounds.PMax;
            var pMin = sceneBounds.PMin;
            findVisibleMeshes(frustum);

            //Renderizar
            foreach (var mesh in modelos)
            {
                if (mesh.Enabled)
                {
                    mesh.render();
                    mesh.Enabled = false;
                }
            }

            if (debugEnabled)
            {
                foreach (var debugBox in debugBoxes)
                {
                    debugBox.render();
                }
            }
        }

        /// <summary>
        ///     Activar modelos dentro de celdas visibles
        /// </summary>
        private void findVisibleMeshes(TgcFrustum frustum)
        {
            for (var x = 0; x < grid.GetUpperBound(0); x++)
            {
                for (var y = 0; y < grid.GetUpperBound(1); y++)
                {
                    for (var z = 0; z < grid.GetUpperBound(2); z++)
                    {
                        var node = grid[x, y, z];
                        var r = TgcCollisionUtils.classifyFrustumAABB(frustum, node.BoundingBox);

                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            node.activateCellMeshes();
                        }
                    }
                }
            }
        }
    }
}