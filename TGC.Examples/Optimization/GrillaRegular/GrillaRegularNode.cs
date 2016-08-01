using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Optimization.GrillaRegular
{
    /// <summary>
    ///     Celda de una grilla regular
    /// </summary>
    public class GrillaRegularNode
    {
        /// <summary>
        ///     Modelos de la celda
        /// </summary>
        public List<TgcMesh> Models { get; set; }

        /// <summary>
        ///     BoundingBox de la celda
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox { get; set; }

        /// <summary>
        ///     Activar todos los modelos de la celda
        /// </summary>
        public void activateCellMeshes()
        {
            foreach (var mesh in Models)
            {
                mesh.Enabled = true;
            }
        }
    }
}