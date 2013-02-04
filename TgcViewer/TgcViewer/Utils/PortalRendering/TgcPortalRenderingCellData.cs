using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.PortalRendering
{
    /// <summary>
    /// Datos de una celda de PortalRendering
    /// </summary>
    public class TgcPortalRenderingCellData
    {
        public int id;
        public string name;
        public float[] facePlanes;
        public float[] boundingVertices;
        public string[] meshes;

    }
}
