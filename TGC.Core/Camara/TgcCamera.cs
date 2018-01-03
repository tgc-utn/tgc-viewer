using TGC.Core.Mathematica;

namespace TGC.Core.Camara
{
    /// <summary>
    ///     Clase camara estatica, default del Framework.
    /// </summary>
    public class TgcCamera
    {
        protected readonly TGCVector3 DEFAULT_UP_VECTOR = new TGCVector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        ///     Posicion de la camara
        /// </summary>
        public TGCVector3 Position { get; protected set; }

        /// <summary>
        ///     Posición del punto al que mira la cámara
        /// </summary>
        public TGCVector3 LookAt { get; protected set; }

        /// <summary>
        ///     Vector direccional hacia arriba (puede diferir si la camara se invierte).
        /// </summary>
        public TGCVector3 UpVector { get; protected set; }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     Los vectores son utilizadas por GetViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        public virtual void SetCamera(TGCVector3 pos, TGCVector3 lookAt)
        {
            //Direccion efectiva de la vista.
            TGCVector3 direction = lookAt - pos;

            //Se busca el vector que es producto del (0,1,0)Up y la direccion de vista. 
            TGCVector3 crossDirection = TGCVector3.Cross(TGCVector3.Up, direction);

            //El vector de Up correcto dependiendo del LookAt
            TGCVector3 finalUp = TGCVector3.Cross(direction,crossDirection);

            Position = pos;
            LookAt = lookAt;
            UpVector = finalUp;
        }

        /// <summary>
        ///     Configura la posicion de la camara, punto de entrada para todas las camaras, con los mismos se calcula la matriz de view.
        ///     Los vectores son utilizadas por GetViewMatrix.
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        /// <param name="upVector">Vector direccion hacia arriba</param>
        public virtual void SetCamera(TGCVector3 pos, TGCVector3 lookAt, TGCVector3 upVector)
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
        public virtual TGCMatrix GetViewMatrix()
        {
            return TGCMatrix.LookAtLH(Position, LookAt, UpVector);
        }
    }
}