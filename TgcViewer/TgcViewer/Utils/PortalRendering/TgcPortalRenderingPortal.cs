using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Portal de PortalRendering que comunica dos celdas
    /// </summary>
    public class TgcPortalRenderingPortal
    {
        public TgcPortalRenderingPortal(string name, TgcBoundingBox boundingBox)
        {
            this.name = name;
            this.boundingBox = boundingBox;
        }

        private string name;
        /// <summary>
        /// Nombre del portal
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox del Portal
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        private bool visited;
        /// <summary>
        /// Indica si la celda ya fue visitada por el algoritmo de visibilidad
        /// </summary>
        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }

        public override string ToString()
        {
            return "Portal: " + name;
        }

    }
}
