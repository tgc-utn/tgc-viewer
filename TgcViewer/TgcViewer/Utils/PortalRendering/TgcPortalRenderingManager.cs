using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Herramienta para optimizar la visibilidad de objetos mediante PortalRendering.
    /// Determina que objetos se ven y cuales no en base a celdas y portales.
    /// Esta herramienta implementa una estrategia de Portal Rendering muy básica.
    /// Aún posee muchos aspectos para pulir y optimizar.
    /// </summary>
    public class TgcPortalRenderingManager
    {
        private TgcConvexPolyhedron frustumConvexPolyhedon;
        private TgcScene scene;


        /// <summary>
        /// Crear Manager
        /// </summary>
        /// <param name="scene">Escenario a administrar</param>
        public TgcPortalRenderingManager(TgcScene scene)
        {
            this.scene = scene;
            this.cells = new List<TgcPortalRenderingCell>();
            this.portals = new List<TgcPortalRenderingPortal>();
            this.frustumConvexPolyhedon = new TgcConvexPolyhedron();
        }

        private List<TgcPortalRenderingCell> cells;
        /// <summary>
        /// Celdas
        /// </summary>
        public List<TgcPortalRenderingCell> Cells
        {
            get { return cells; }
        }

        private List<TgcPortalRenderingPortal> portals;
        /// <summary>
        /// Portales
        /// </summary>
        public List<TgcPortalRenderingPortal> Portals
        {
            get { return portals; }
        }

        /// <summary>
        /// Actualiza la visibilidad de todos los modelos de las celdas.
        /// Las modelos visibles se cargan como Enable = true, mientras que el
        /// resto se deshabilita.
        /// </summary>
        /// <param name="cameraPos">Posición de la cámara</param>
        public void updateVisibility(Vector3 cameraPos)
        {
            //Armar Frustum para uso internor, en base al Frustum actual
            TgcFrustum frustum = GuiController.Instance.Frustum;
            Plane[] currentFrustumPlanes = new Plane[6];
            currentFrustumPlanes = new Plane[6];
            currentFrustumPlanes[0] = frustum.NearPlane;
            currentFrustumPlanes[1] = frustum.FarPlane;
            currentFrustumPlanes[2] = frustum.LeftPlane;
            currentFrustumPlanes[3] = frustum.RightPlane;
            currentFrustumPlanes[4] = frustum.BottomPlane;
            currentFrustumPlanes[5] = frustum.TopPlane;

            //Deshabilitar todas las celdas
            foreach (TgcPortalRenderingCell cell in cells)
            {
                cell.Visited = false;
                foreach (TgcPortalRenderingConnection connection in cell.Connections)
                {
                    connection.Portal.Visited = false;
                }
            }

            //Buscar la celda actual en la que se encuentra la cámara
            TgcPortalRenderingCell currentCell = findCellFromPoint(cameraPos);
            if (currentCell == null)
            {
                return;
            }

            //Recorrer grafo de celdas desde la celda actual
            currentCell.Visited = true;
            traverseCellGraph(cameraPos, currentFrustumPlanes, currentCell);
        }

        /// <summary>
        /// Renderiza todos los meshes habilitados, y los vuelve a marcar como inhabilitados
        /// para el próximo cuadro.
        /// Debe ejecutarse luego de haber llamado a updateVisibility()
        /// </summary>
        public void render()
        {
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.render();
                mesh.Enabled = false;
            }
        }

        /// <summary>
        /// Recorrer el grafo de celdas y portales
        /// </summary>
        private void traverseCellGraph(Vector3 cameraPos, Plane[] currentFrustumPlanes, TgcPortalRenderingCell cell)
        {
            //Habilitar modelos visibles de esta celda
            findVisibleMeshes(cell, currentFrustumPlanes);

            //Recorrer todas la conexiones de esta celda
            foreach (TgcPortalRenderingConnection connection in cell.Connections)
            {
                //Si el portal ya fue visitado, ignorar
                //TODO: Hay una configuración extrema de celdas y portales que no es tenida en cuenta con este atajo. Analizar en más detalle.
                if (connection.Portal.Visited)
                    continue;

                //TODO: quizás convendria hacer un test Frustum-BoundingSphere del Portal para descartar más rápido los que no se ven

                //Hacer clipping entre el Frustum y el polígono del portal
                Vector3[] clippedPortalVerts = doPortalClipping(currentFrustumPlanes, connection.Polygon);

                //Si quedó algún remanente luego de hacer clipping, avanzar hacia esa celda
                if (clippedPortalVerts != null)
                {
                    //Crear nuevo Frustum recortado por el portal
                    Plane[] clippedFrustumPlanes = createFrustumPlanes(cameraPos, currentFrustumPlanes, clippedPortalVerts, connection.Plane);

                    //Avanzar sobre la celda que conecta este portal, utilizando el Frustum recortado
                    connection.NextCell.Visited = true;
                    connection.Portal.Visited = true;
                    traverseCellGraph(cameraPos, clippedFrustumPlanes, connection.NextCell);
                }
            }
        }

        /// <summary>
        /// Crear un nuevo Frustum acotado usando como base el portal recorado.
        /// La cantidad de planos del nuevo Frustum no tiene por qué ser 6.
        /// Depende de la forma que haya quedado en el portal recortado.
        /// </summary>
        private Plane[] createFrustumPlanes(Vector3 cameraPos, Plane[] currentFrustumPlanes, Vector3[] portalVerts, Plane portalPlane)
        {
            //Hay un plano por cada vértice del polígono + 2 por el near y far plane
            Plane[] frustumPlanes = new Plane[2 + portalVerts.Length];

            //Cargar near y far plane originales
            //TODO: habria que usar el portalPlane para acercar el NearPlane hasta el portal
            frustumPlanes[0] = currentFrustumPlanes[0];
            frustumPlanes[1] = currentFrustumPlanes[1];

            //Generar los planos laterales en base al polígono remanente del portal
            //Vamos tomando de a dos puntos del polígono + la posición de la cámara y creamos un plano
            Vector3 lastP = portalVerts[portalVerts.Length - 1];
            for (int i = 0; i < portalVerts.Length; i++)
            {
                Vector3 nextP = portalVerts[i];

                //Armar el plano para que la normal apunte hacia adentro del Frustum
                Vector3 a = lastP - cameraPos;
                Vector3 b = nextP - cameraPos;
                Plane plane = Plane.FromPointNormal(cameraPos, Vector3.Cross(b, a));

                //Guardar después del near y far plane
                frustumPlanes[i + 2] = plane;

                lastP = nextP;
            }

            return frustumPlanes;
        }

        /// <summary>
        /// Recorta el portal en base al frustum.
        /// Este método se realiza haciendo un clipping del Frustum contra la cara del portal.
        /// El recorte se hace en 3D.
        /// Existen técnicas más eficientes para realizar el clipping en 2D, utilizando la proyección del BoundingBox del portal.
        /// Ver Capítulo 13 - Portal Rendering, del libro Core Techniques and Algorithms in Game Programming, para optimizar la estrategia.
        /// </summary>
        private Vector3[] doPortalClipping(Plane[] frustumPlanes, TgcConvexPolygon portalPoly)
        {
            Vector3[] clippedPortalVerts = portalPoly.BoundingVertices;
            foreach (Plane plane in frustumPlanes)
            {
                //Clipping con algoritmo de Sutherland-Hodgman
                if (!TgcCollisionUtils.clipConvexPolygon(clippedPortalVerts, plane, out clippedPortalVerts))
                {
                    return null;
                }
            }

            return clippedPortalVerts;
        }

        /// <summary>
        /// Habilitar los modelos visibles de esta celda, según el Frustum restringido
        /// </summary>
        private void findVisibleMeshes(TgcPortalRenderingCell cell, Plane[] currentFrustumPlanes)
        {
            //El Frustum puede tener más de 6 planos, asi que lo tratamos como un cuerpo convexo general.
            frustumConvexPolyhedon.Planes = currentFrustumPlanes;
            foreach (TgcMesh mesh in cell.Meshes)
            {
                if(mesh.Enabled == false)
                {
                    if (TgcCollisionUtils.classifyConvexPolyhedronAABB(frustumConvexPolyhedon, mesh.BoundingBox) != TgcCollisionUtils.ConvexPolyhedronResult.OUTSIDE)
                    {
                        mesh.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Busca y devuelve la celda que contenga al punto q.
        /// Devuelve null en caso de que ninguna celda lo contenga.
        /// </summary>
        /// <param name="q">Punto buscado</param>
        /// <returns>Celda que lo contiene o null</returns>
        public TgcPortalRenderingCell findCellFromPoint(Vector3 q)
        {
            foreach (TgcPortalRenderingCell cell in cells)
            {
                TgcCollisionUtils.ConvexPolyhedronResult c = TgcCollisionUtils.classifyPointConvexPolyhedron(q, cell.ConvexPolyhedron);
                if (c == TgcCollisionUtils.ConvexPolyhedronResult.INSIDE)
                {
                    return cell;
                }
            }

            return null;
        }

        /// <summary>
        /// Crear meshes de debug para renderizar portales
        /// </summary>
        /// <param name="portalColor">Color de portales</param>
        public void createDebugPortals(Color portalColor)
        {
            foreach (TgcPortalRenderingCell cell in cells)
            {
                foreach (TgcPortalRenderingConnection connection in cell.Connections)
                {
                    connection.Polygon.Color = portalColor;
                    connection.Polygon.updateValues();
                }
            }
        }

        /// <summary>
        /// Renderizar meshes de debug de Portales visitados
        /// </summary>
        public void renderPortals()
        {
            foreach (TgcPortalRenderingCell cell in cells)
            {
                foreach (TgcPortalRenderingConnection connection in cell.Connections)
                {
                    if (connection.Portal.Visited)
                    {
                        connection.Polygon.render();
                    }
                }
            }
        }

        /// <summary>
        /// Libera todos los recursos
        /// </summary>
        public void dispose()
        {
            foreach (TgcPortalRenderingCell cell in cells)
            {
                foreach (TgcPortalRenderingConnection connection in cell.Connections)
                {
                    connection.Polygon.dispose();
                }
            }
        }

    }
}

