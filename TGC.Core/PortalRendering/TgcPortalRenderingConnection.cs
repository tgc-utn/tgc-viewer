using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Conexión unidireccional entre una celda y otra, a travez de un Portal
    /// </summary>
    public class TgcPortalRenderingConnection
    {
        public TgcPortalRenderingConnection(TgcPortalRenderingPortal portal, TgcPortalRenderingCell nextCell,
            TgcConvexPolygon polygon, TGCPlane plane)
        {
            Portal = portal;
            NextCell = nextCell;
            Polygon = polygon;
            Plane = plane;
        }

        /// <summary>
        ///     Portal de la conexion
        /// </summary>
        public TgcPortalRenderingPortal Portal { get; }

        /// <summary>
        ///     Celda con la cual comunica
        /// </summary>
        public TgcPortalRenderingCell NextCell { get; set; }

        /// <summary>
        ///     Polígono plano que representa el portal.
        ///     Los vértices están en clockwise-order según la celda de origen
        /// </summary>
        public TgcConvexPolygon Polygon { get; }

        /// <summary>
        ///     Plano del portal, apuntando hacia la celda origen
        /// </summary>
        public TGCPlane Plane { get; }

        public override string ToString()
        {
            return "Connection: [" + Portal + "] - [" + NextCell + "]";
        }
    }
}