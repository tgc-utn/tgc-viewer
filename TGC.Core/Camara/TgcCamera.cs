using SharpDX;

namespace TGC.Core.Camara
{
    /// <summary>
    ///     Clase camara estatica, default del Framework.
    /// </summary>
    public class TgcCamera
    {
        protected readonly Vector3 DEFAULT_UP_VECTOR = new Vector3(0.0f, 1.0f, 0.0f);

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
        public Vector3 UpVector { get; protected set; }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     Los vectores son utilizadas por GetViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        public virtual void SetCamera(Vector3 pos, Vector3 lookAt)
        {
            Position = pos;
            LookAt = lookAt;
            UpVector = DEFAULT_UP_VECTOR;
        }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     Los vectores son utilizadas por GetViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        /// <param name="upVector">Vector direccion hacia arriba</param>
        public virtual void SetCamera(Vector3 pos, Vector3 lookAt, Vector3 upVector)
        {
            Position = pos;
            LookAt = lookAt;
            UpVector = upVector;
        }

        /// <summary>
        ///     Permite actualizar el estado interno de la camara si se sobrescribe este metodo. por defecto no realiza ninguna accion.
        ///     Si se realiza procesamiento interno se puede invocar al metodo SetCamera para actualizar posicion, lookAt y upVector.
        /// </summary>
        public virtual void UpdateCamera(float elapsedTime)
        {
            //Esta camara no tienen movimiento, una vez inicializada con posicion y lookAt ya no es actualizada.
            //Se puede invocar a SetCamera para actualizar posicion, lookAt y upVector.
        }

        /// <summary>
        ///     Devuelve la matriz View en base a los valores de la camara. Es invocado en cada update de render.
        /// </summary>
        public virtual Matrix GetViewMatrix()
        {
            return Matrix.LookAtLH(Position, LookAt, UpVector);
        }
    }
}