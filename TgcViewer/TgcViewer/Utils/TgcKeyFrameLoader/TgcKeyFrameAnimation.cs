using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Viewer.Utils.TgcKeyFrameLoader
{
    /// <summary>
    ///     Animación de una malla animada por KeyFrames
    /// </summary>
    public class TgcKeyFrameAnimation
    {
        public TgcKeyFrameAnimation(TgcKeyFrameAnimationData data, TgcBoundingBox boundingBox)
        {
            Data = data;
            BoundingBox = boundingBox;
        }

        /// <summary>
        ///     BoundingBox de la animación
        /// </summary>
        public TgcBoundingBox BoundingBox { get; }

        /// <summary>
        ///     Datos de vértices de la animación
        /// </summary>
        public TgcKeyFrameAnimationData Data { get; }
    }
}