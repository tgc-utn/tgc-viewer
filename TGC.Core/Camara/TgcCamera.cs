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
		public abstract Vector3 getPosition();

        /// <summary>
        ///     Posición del punto al que mira la cámara
        /// </summary>
        public abstract Vector3 getLookAt();

        /// <summary>
        ///     Configura la posicion de la camara
        /// </summary>
        /// <param name="pos">Posicion de la camara</param>
        /// <param name="lookAt">Punto hacia el cual se quiere ver</param>
        public void setCamera(Vector3 pos, Vector3 lookAt)
        {
            D3DDevice.Instance.Device.Transform.View = Matrix.LookAtLH(pos, lookAt, DEFAULT_UP_VECTOR);
        }

        /// <summary>
        ///     Actualizar el estado interno de la camara en cada frame
        /// </summary>
        public abstract void updateCamera(float elapsedTime);

        /// <summary>
        ///     Actualizar la matriz View en base a los valores de la camara
        /// </summary>
        public abstract void updateViewMatrix(Device d3dDevice);
    }
}