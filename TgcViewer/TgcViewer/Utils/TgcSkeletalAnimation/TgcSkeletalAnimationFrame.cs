using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    /// Key frame de animacion para un hueso particular
    /// </summary>
    public class TgcSkeletalAnimationFrame
    {
        public TgcSkeletalAnimationFrame(int frame, Vector3 position, Quaternion rotation)
        {
            this.frame = frame;
            this.position = position;
            this.rotation = rotation;
        }

        private int frame;
        /// <summary>
        /// Numero de frame en el cual transcurre esta rotacion y traslacion.
        /// </summary>
        public int Frame
        {
            get { return frame; }
        }

        private Vector3 position;
        /// <summary>
        /// Posicion del hueso para este frame
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
        }

        private Quaternion rotation;
        /// <summary>
        /// Rotacion del hueso para este frame en Quaternion
        /// </summary>
        public Quaternion Rotation
        {
            get { return rotation; }
        }


    }
}
