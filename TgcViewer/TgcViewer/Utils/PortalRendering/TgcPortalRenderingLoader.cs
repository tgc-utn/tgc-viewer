using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TGC.Core.Geometries;
using TGC.Core.Utils;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Loader de información de PortalRendering de una escena
    /// </summary>
    public class TgcPortalRenderingLoader
    {

        public TgcPortalRenderingLoader()
        {
        }

        /// <summary>
        /// Cargar información de PortalRendering
        /// </summary>
        public TgcPortalRenderingManager loadFromData(TgcScene scene, TgcPortalRenderingData portalRenderingData)
        {
            TgcPortalRenderingManager manager = new TgcPortalRenderingManager(scene);

            //Crear dictionary de nombres de los meshes
            Dictionary<string, TgcMesh> meshDictionary = new Dictionary<string, TgcMesh>();
            foreach (TgcMesh mesh in scene.Meshes)
            {
                meshDictionary.Add(mesh.Name, mesh);
            }

            //Cargar celdas
            foreach (TgcPortalRenderingCellData cellData in portalRenderingData.cells)
            {
                //Crear cuerpo Convexo
                TgcConvexPolyhedron convexPoly = new TgcConvexPolyhedron();
                convexPoly.Planes = new Plane[cellData.facePlanes.Length / 4];
                for (int i = 0; i < convexPoly.Planes.Length; i++)
                {
                    convexPoly.Planes[i] = new Plane(
                        cellData.facePlanes[i * 4],
                        cellData.facePlanes[i * 4 + 1],
                        cellData.facePlanes[i * 4 + 2],
                        cellData.facePlanes[i * 4 + 3]
                        );
                }
                convexPoly.BoundingVertices = new Vector3[cellData.boundingVertices.Length / 3];
                for (int i = 0; i < convexPoly.BoundingVertices.Length; i++)
                {
                    convexPoly.BoundingVertices[i] = new Vector3(
                        cellData.boundingVertices[i * 3],
                        cellData.boundingVertices[i * 3 + 1],
                        cellData.boundingVertices[i * 3 + 2]
                        );
                }

                //Crear celda
                TgcPortalRenderingCell cell = new TgcPortalRenderingCell(cellData.name, convexPoly);
                manager.Cells.Add(cell);

                //Cargar meshes en celda
                for (int i = 0; i < cellData.meshes.Length; i++)
                {
                    TgcMesh mesh = meshDictionary[cellData.meshes[i]];
                    cell.Meshes.Add(mesh);
                }
            }

            //Cargar portales
            foreach (TgcPortalRenderingPortalData portalData in portalRenderingData.portals)
            {
                //BoundingBox del portal
                TgcBoundingBox boundingBox = new TgcBoundingBox(
                    new Vector3(portalData.pMin[0], portalData.pMin[1], portalData.pMin[2]),
                    new Vector3(portalData.pMax[0], portalData.pMax[1], portalData.pMax[2])
                    );

                //Crear portal
                TgcPortalRenderingPortal portal = new TgcPortalRenderingPortal(portalData.name, boundingBox);
                manager.Portals.Add(portal);


                //Cargar conexiones para celdas A y B
                TgcPortalRenderingCell cellA = manager.Cells[portalData.cellA];
                TgcPortalRenderingCell cellB = manager.Cells[portalData.cellB];


                //Poligono del portal para la celda A
                TgcConvexPolygon polygonA = new TgcConvexPolygon();
                polygonA.BoundingVertices = new Vector3[portalData.boundingVerticesA.Length / 3];
                for (int i = 0; i < polygonA.BoundingVertices.Length; i++)
                {
                    polygonA.BoundingVertices[i] = new Vector3(
                        portalData.boundingVerticesA[i * 3],
                        portalData.boundingVerticesA[i * 3 + 1],
                        portalData.boundingVerticesA[i * 3 + 2]
                        );
                }

                //Plano del portal para la celda A
                Plane planeA = TgcParserUtils.float4ArrayToPlane(portalData.planeA);

                //Crear conexion A
                TgcPortalRenderingConnection connectionA = new TgcPortalRenderingConnection(portal, cellB, polygonA, planeA);
                cellA.Connections.Add(connectionA);


                //Poligono del portal para la celda B
                TgcConvexPolygon polygonB = new TgcConvexPolygon();
                polygonB.BoundingVertices = new Vector3[portalData.boundingVerticesB.Length / 3];
                for (int i = 0; i < polygonB.BoundingVertices.Length; i++)
                {
                    polygonB.BoundingVertices[i] = new Vector3(
                        portalData.boundingVerticesB[i * 3],
                        portalData.boundingVerticesB[i * 3 + 1],
                        portalData.boundingVerticesB[i * 3 + 2]
                        );
                }

                //Plano del portal para la celda B
                Plane planeB = TgcParserUtils.float4ArrayToPlane(portalData.planeB);

                //Crear conexion B
                TgcPortalRenderingConnection connectionB = new TgcPortalRenderingConnection(portal, cellA, polygonB, planeB);
                cellB.Connections.Add(connectionB);
            }

            return manager;
        }

    }
}
