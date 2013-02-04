using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Optimizacion.GrillaRegular
{
    /// <summary>
    /// Celda de una grilla regular
    /// </summary>
    public class GrillaRegularNode
    {
        private List<TgcMesh> models;
        /// <summary>
        /// Modelos de la celda
        /// </summary>
        public List<TgcMesh> Models
        {
          get { return models; }
          set { models = value; }
        }

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de la celda
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }


        /// <summary>
        /// Activar todos los modelos de la celda
        /// </summary>
        public void activateCellMeshes()
        {
            foreach (TgcMesh mesh in models)
            {
                mesh.Enabled = true;
            }
        }
    }
}
