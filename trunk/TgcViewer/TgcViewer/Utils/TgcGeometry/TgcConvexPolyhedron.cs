using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Representa un volúmen de un cuerpo convexo 3D, delimitado por planos.
    /// Las normales de los planos apuntan hacia adentro.
    /// </summary>
    public class TgcConvexPolyhedron
    {
        public TgcConvexPolyhedron()
        {
        }

        private Plane[] planes;
        /// <summary>
        /// Planos que definen el cuerpo convexo.
        /// Apuntan hacia adentro del cuerpo
        /// </summary>
        public Plane[] Planes
        {
            get { return planes; }
            set { planes = value; }
        }

        private Vector3[] boundingVertices;
        /// <summary>
        /// Vertices que definen el contorno del cuerpo convexo
        /// </summary>
        public Vector3[] BoundingVertices
        {
            get { return boundingVertices; }
            set { boundingVertices = value; }
        }



    }
}
