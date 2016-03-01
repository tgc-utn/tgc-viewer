using Microsoft.DirectX;

namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Key frame de animacion para un hueso particular
    /// </summary>
    public class TgcSkeletalAnimationFrame
    {
        public TgcSkeletalAnimationFrame(int frame, Vector3 position, Quaternion rotation)
        {
            Frame = frame;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        ///     Numero de frame en el cual transcurre esta rotacion y traslacion.
        /// </summary>
        public int Frame { get; }

        /// <summary>
        ///     Posicion del hueso para este frame
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        ///     Rotacion del hueso para este frame en Quaternion
        /// </summary>
        public Quaternion Rotation { get; }
    }
}