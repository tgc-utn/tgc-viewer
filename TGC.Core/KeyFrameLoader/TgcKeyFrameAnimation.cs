using TGC.Core.BoundingVolumes;

namespace TGC.Core.KeyFrameLoader
{
    /// <summary>
    ///     Animación de una malla animada por KeyFrames
    /// </summary>
    public class TgcKeyFrameAnimation
    {
        public TgcKeyFrameAnimation(TgcKeyFrameAnimationData data, TgcBoundingAxisAlignBox boundingBox)
        {
            Data = data;
            BoundingBox = boundingBox;
        }

        /// <summary>
        ///     BoundingBox de la animación
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox { get; }

        /// <summary>
        ///     Datos de vértices de la animación
        /// </summary>
        public TgcKeyFrameAnimationData Data { get; }
    }
}