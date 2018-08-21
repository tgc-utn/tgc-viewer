using SharpDX;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Conexi�n unidireccional entre una celda y otra, a travez de un Portal
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
        ///     Pol�gono plano que representa el portal.
        ///     Los v�rtices est�n en clockwise-order seg�n la celda de origen
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