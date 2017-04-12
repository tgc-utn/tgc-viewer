using Microsoft.DirectX;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Loader de información de PortalRendering de una escena
    /// </summary>
    public class TgcPortalRenderingLoader
    {
        /// <summary>
        ///     Cargar información de PortalRendering
        /// </summary>
        public TgcPortalRenderingManager loadFromData(TgcScene scene, TgcPortalRenderingData portalRenderingData)
        {
            var manager = new TgcPortalRenderingManager(scene);

            //Crear dictionary de nombres de los meshes
            var meshDictionary = new Dictionary<string, TgcMesh>();
            foreach (var mesh in scene.Meshes)
            {
                meshDictionary.Add(mesh.Name, mesh);
            }

            //Cargar celdas
            foreach (var cellData in portalRenderingData.cells)
            {
                //Crear cuerpo Convexo
                var convexPoly = new TgcConvexPolyhedron();
                convexPoly.Planes = new TGCPlane[cellData.facePlanes.Length / 4];
                for (var i = 0; i < convexPoly.Planes.Length; i++)
                {
                    convexPoly.Planes[i] = new TGCPlane(
                        cellData.facePlanes[i * 4],
                        cellData.facePlanes[i * 4 + 1],
                        cellData.facePlanes[i * 4 + 2],
                        cellData.facePlanes[i * 4 + 3]
                        );
                }
                convexPoly.BoundingVertices = new TGCVector3[cellData.boundingVertices.Length / 3];
                for (var i = 0; i < convexPoly.BoundingVertices.Length; i++)
                {
                    convexPoly.BoundingVertices[i] = new TGCVector3(
                        cellData.boundingVertices[i * 3],
                        cellData.boundingVertices[i * 3 + 1],
                        cellData.boundingVertices[i * 3 + 2]
                        );
                }

                //Crear celda
                var cell = new TgcPortalRenderingCell(cellData.name, convexPoly);
                manager.Cells.Add(cell);

                //Cargar meshes en celda
                for (var i = 0; i < cellData.meshes.Length; i++)
                {
                    var mesh = meshDictionary[cellData.meshes[i]];
                    cell.Meshes.Add(mesh);
                }
            }

            //Cargar portales
            foreach (var portalData in portalRenderingData.portals)
            {
                //BoundingBox del portal
                var boundingBox = new TgcBoundingAxisAlignBox(
                    new TGCVector3(portalData.pMin[0], portalData.pMin[1], portalData.pMin[2]),
                    new TGCVector3(portalData.pMax[0], portalData.pMax[1], portalData.pMax[2])
                    );

                //Crear portal
                var portal = new TgcPortalRenderingPortal(portalData.name, boundingBox);
                manager.Portals.Add(portal);

                //Cargar conexiones para celdas A y B
                var cellA = manager.Cells[portalData.cellA];
                var cellB = manager.Cells[portalData.cellB];

                //Poligono del portal para la celda A
                var polygonA = new TgcConvexPolygon();
                polygonA.BoundingVertices = new TGCVector3[portalData.boundingVerticesA.Length / 3];
                for (var i = 0; i < polygonA.BoundingVertices.Length; i++)
                {
                    polygonA.BoundingVertices[i] = new TGCVector3(
                        portalData.boundingVerticesA[i * 3],
                        portalData.boundingVerticesA[i * 3 + 1],
                        portalData.boundingVerticesA[i * 3 + 2]
                        );
                }

                //Plano del portal para la celda A
                var planeA = TGCPlane.Float4ArrayToPlane(portalData.planeA);

                //Crear conexion A
                var connectionA = new TgcPortalRenderingConnection(portal, cellB, polygonA, planeA);
                cellA.Connections.Add(connectionA);

                //Poligono del portal para la celda B
                var polygonB = new TgcConvexPolygon();
                polygonB.BoundingVertices = new TGCVector3[portalData.boundingVerticesB.Length / 3];
                for (var i = 0; i < polygonB.BoundingVertices.Length; i++)
                {
                    polygonB.BoundingVertices[i] = new TGCVector3(
                        portalData.boundingVerticesB[i * 3],
                        portalData.boundingVerticesB[i * 3 + 1],
                        portalData.boundingVerticesB[i * 3 + 2]
                        );
                }

                //Plano del portal para la celda B
                var planeB = TGCPlane.Float4ArrayToPlane(portalData.planeB);

                //Crear conexion B
                var connectionB = new TgcPortalRenderingConnection(portal, cellA, polygonB, planeB);
                cellB.Connections.Add(connectionB);
            }

            return manager;
        }
    }
}