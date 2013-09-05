using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace TgcViewer.Utils.Input
{
    /// <summary>
    /// Interfaz general para una cámara del Framework
    /// </summary>
    public interface TgcCamera
    {
        /// <summary>
        /// Posición de la cámara
        /// </summary>
        Vector3 getPosition();

        /// <summary>
        /// Posición del punto al que mira la cámara
        /// </summary>
        Vector3 getLookAt();

        /// <summary>
        /// Actualizar el estado interno de la cámara en cada frame
        /// </summary>
        void updateCamera();

        /// <summary>
        /// Actualizar la matriz View en base a los valores de la cámara
        /// </summary>
        void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice);

        /// <summary>
        /// Activar o desactivar la camara
        /// </summary>
        bool Enable
        {
            get;
            set;
        }
    }
}
