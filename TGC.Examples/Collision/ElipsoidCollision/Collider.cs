using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;

namespace TGC.Examples.Collision.ElipsoidCollision
{
    /// <summary>
    ///     Colisionador abstracto
    /// </summary>
    public abstract class Collider
    {
        protected TgcBoundingSphere boundingSphere;
        protected bool enable;

        public Collider()
        {
            enable = true;
        }

        /// <summary>
        ///     Indica si esta habilitado.
        ///     Solo se tienen en cuenta para colisionar los objetos habilitados.
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        /// <summary>
        ///     BoundingSphere
        /// </summary>
        public TgcBoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }

        /// <summary>
        ///     Colisiona un Elipsoide en movimiento contra el objeto colisionador.
        ///     Si hay colision devuelve el instante t de colision, el punto q de colision y el vector normal n de la superficie
        ///     contra la que
        ///     se colisiona.
        ///     Todo se devuelve en Elipsoid space.
        /// </summary>
        /// <param name="eSphere">BoundingSphere de radio 1 en Elipsoid space</param>
        /// <param name="eMovementVector">movimiento en Elipsoid space</param>
        /// <param name="eRadius">radio del Elipsoide</param>
        /// <param name="movementSphere">
        ///     BoundingSphere que abarca el sphere en su punto de origen mas el sphere en su punto final
        ///     deseado
        /// </param>
        /// <param name="t">Menor instante de colision, en Elipsoid space</param>
        /// <param name="q">Punto mas cercano de colision, en Elipsoid space</param>
        /// <param name="n">Vector normal de la superficie contra la que se colisiona</param>
        /// <returns>True si hay colision</returns>
        public abstract bool intersectMovingElipsoid(TgcBoundingSphere eSphere, TGCVector3 eMovementVector, TGCVector3 eRadius,
            TgcBoundingSphere movementSphere, out float t, out TGCVector3 q, out TGCVector3 n);
    }
}