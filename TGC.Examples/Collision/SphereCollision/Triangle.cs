using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;

namespace TGC.Examples.Collision.SphereCollision
{
    /// <summary>
    ///     Triangulo del Collider
    /// </summary>
    public class Triangle
    {
        /// <summary>
        ///     Crear triangulo.
        ///     Calcula su plano y BoundingSphere
        /// </summary>
        public Triangle(TGCVector3 a, TGCVector3 b, TGCVector3 c)
        {
            A = a;
            B = b;
            C = c;
            Plane = TGCPlane.FromPoints(a, b, c);
            BoundingSphere = TgcBoundingSphere.computeFromPoints(new[] { a, b, c }).toClass();
        }

        /// <summary>
        ///     Vertice A
        /// </summary>
        public TGCVector3 A { get; set; }

        /// <summary>
        ///     Vertice B
        /// </summary>
        public TGCVector3 B { get; set; }

        /// <summary>
        ///     Vertice C
        /// </summary>
        public TGCVector3 C { get; set; }

        /// <summary>
        ///     Ecuacion del plano del triangulo
        /// </summary>
        public TGCPlane Plane { get; set; }

        /// <summary>
        ///     BoundingSphere
        /// </summary>
        public TgcBoundingSphere BoundingSphere { get; set; }
    }
}
