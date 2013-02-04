using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Celda de PortalRendering
    /// </summary>
    public class TgcPortalRenderingCell
    {
        public TgcPortalRenderingCell(string name, TgcConvexPolyhedron convexPolyhedron)
        {
            this.name = name;
            this.convexPolyhedron = convexPolyhedron;
            this.meshes = new List<TgcMesh>();
            this.connections = new List<TgcPortalRenderingConnection>();
        }

        private TgcConvexPolyhedron convexPolyhedron;
        /// <summary>
        /// Vol�men convexo de la celda
        /// </summary>
        public TgcConvexPolyhedron ConvexPolyhedron
        {
            get { return convexPolyhedron; }
        }

        private string name;
        /// <summary>
        /// Nombre de la celda
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private List<TgcMesh> meshes;
        /// <summary>
        /// Modelos dentro de la celda
        /// </summary>
        public List<TgcMesh> Meshes
        {
            get { return meshes; }
        }

        private List<TgcPortalRenderingConnection> connections;
        /// <summary>
        /// Conexiones con otras celdas, a trav�s de portales
        /// </summary>
        public List<TgcPortalRenderingConnection> Connections
        {
            get { return connections; }
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
            return "Cell: " + name;
        }
    }
}
