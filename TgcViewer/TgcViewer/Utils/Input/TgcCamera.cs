using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils.Input
{
    /// <summary>
    ///     Interfaz general para una cámara del Framework
    /// </summary>
    public interface TgcCamera
    {
        /// <summary>
        ///     Activar o desactivar la camara
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        ///     Posición de la cámara
        /// </summary>
        Vector3 getPosition();

        /// <summary>
        ///     Posición del punto al que mira la cámara
        /// </summary>
        Vector3 getLookAt();

        /// <summary>
        ///     Actualizar el estado interno de la cámara en cada frame
        /// </summary>
        void updateCamera();

        /// <summary>
        ///     Actualizar la matriz View en base a los valores de la cámara
        /// </summary>
        void updateViewMatrix(Device d3dDevice);
    }
}