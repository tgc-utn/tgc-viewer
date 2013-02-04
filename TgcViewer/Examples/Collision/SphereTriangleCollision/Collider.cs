using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;

namespace Examples.Collision.SphereTriangleCollision
{
    /// <summary>
    /// Colisionador abstracto
    /// </summary>
    public abstract class Collider
    {

        protected bool enable;
        /// <summary>
        /// Indica si esta habilitado.
        /// Solo se tienen en cuenta para colisionar los objetos habilitados.
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        protected TgcBoundingSphere boundingSphere;
        /// <summary>
        /// BoundingSphere
        /// </summary>
        public TgcBoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }



        public Collider()
        {
            this.enable = true;
        }

        /// <summary>
        /// Colisiona un BoundingSphere en movimiento contra el objeto colisionador.
        /// Si hay colision devuelve el instante t de colision, el punto q de colision y el vector normal n de la superficie contra la que
        /// se colisiona
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="movementVector">movimiento del BoundingSphere</param>
        /// <param name="movementSphere">BoundingSphere que abarca el sphere en su punto de origen mas el sphere en su punto final deseado</param>
        /// <param name="t">Menor instante de colision</param>
        /// <param name="q">Punto mas cercano de colision</param>
        /// <param name="n">Vector normal de la superficie contra la que se colisiona</param>
        /// <returns>True si hay colision</returns>
        public abstract bool intersectMovingSphere(TgcBoundingSphere sphere, Vector3 movementVector, TgcBoundingSphere movementSphere, out float t, out Vector3 q, out Vector3 n);

    }
}
