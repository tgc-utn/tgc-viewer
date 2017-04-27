using TGC.Core.Mathematica;

namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Key frame de animacion para un hueso particular
    /// </summary>
    public class TgcSkeletalAnimationFrame
    {
        public TgcSkeletalAnimationFrame(int frame, TGCVector3 position, TGCQuaternion rotation)
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
        public TGCVector3 Position { get; }

        /// <summary>
        ///     Rotacion del hueso para este frame en TGCQuaternion
        /// </summary>
        public TGCQuaternion Rotation { get; }
    }
}