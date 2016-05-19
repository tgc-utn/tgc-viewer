using System.Collections.Generic;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;

namespace TGC.Core.PortalRendering
{
    /// <summary>
    ///     Celda de PortalRendering
    /// </summary>
    public class TgcPortalRenderingCell
    {
        public TgcPortalRenderingCell(string name, TgcConvexPolyhedron convexPolyhedron)
        {
            Name = name;
            ConvexPolyhedron = convexPolyhedron;
            Meshes = new List<TgcMesh>();
            Connections = new List<TgcPortalRenderingConnection>();
        }

        /// <summary>
        ///     Volúmen convexo de la celda
        /// </summary>
        public TgcConvexPolyhedron ConvexPolyhedron { get; }

        /// <summary>
        ///     Nombre de la celda
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Modelos dentro de la celda
        /// </summary>
        public List<TgcMesh> Meshes { get; }

        /// <summary>
        ///     Conexiones con otras celdas, a través de portales
        /// </summary>
        public List<TgcPortalRenderingConnection> Connections { get; }

        /// <summary>
        ///     Indica si la celda ya fue visitada por el algoritmo de visibilidad
        /// </summary>
        public bool Visited { get; set; }

        public override string ToString()
        {
            return "Cell: " + Name;
        }
    }
}