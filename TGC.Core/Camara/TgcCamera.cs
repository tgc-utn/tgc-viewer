using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;

namespace TGC.Core.Camara
{
    /// <summary>
    ///     Clase generica para una camara del Framework
    /// </summary>
    public abstract class TgcCamera
    {
        private readonly Vector3 DEFAULT_UP_VECTOR = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        ///     Posicion de la camara
        /// </summary>
        public Vector3 Position { get; private set; }
        
        /// <summary>
        ///     Posición del punto al que mira la cámara
        /// </summary>
        public Vector3 LookAt { get; private set; }

        /// <summary>
        ///     Vector direccional hacia arriba (puede diferir si la camara se invierte).
        /// </summary>
        public Vector3 UpVector { get; private set; }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     estos vectores son utilizadas por getViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        public void setCamera(Vector3 pos, Vector3 lookAt)
        {
            this.Position = pos;
            this.LookAt = lookAt;
            this.UpVector = DEFAULT_UP_VECTOR;
        }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     estos vectores son utilizadas por getViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        public void setCamera(Vector3 pos, Vector3 lookAt, Vector3 upVec)
        {
            this.Position = pos;
            this.LookAt = lookAt;
            this.UpVector = upVec;
        }

        /// <summary>
        ///     Actualizar el estado interno de la camara en cada frame.
        /// </summary>
        public abstract void updateCamera(float elapsedTime);

        /// <summary>
        ///     Devuelve la matriz View en base a los valores de la camara. Es invocado en cada update de render.
        /// </summary>
        public virtual Matrix getViewMatrix()
        {
            return Matrix.LookAtLH(this.Position, this.LookAt, this.UpVector);
        }
    }
}