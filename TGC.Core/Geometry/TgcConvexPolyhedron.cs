using Microsoft.DirectX;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Representa un volúmen de un cuerpo convexo 3D, delimitado por planos.
    ///     Las normales de los planos apuntan hacia adentro.
    /// </summary>
    public class TgcConvexPolyhedron
    {
        /// <summary>
        ///     Planos que definen el cuerpo convexo.
        ///     Apuntan hacia adentro del cuerpo
        /// </summary>
        public Plane[] Planes { get; set; }

        /// <summary>
        ///     Vertices que definen el contorno del cuerpo convexo
        /// </summary>
        public Vector3[] BoundingVertices { get; set; }
    }
}