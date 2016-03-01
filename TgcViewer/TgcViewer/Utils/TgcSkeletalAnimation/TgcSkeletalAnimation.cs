using System.Collections.Generic;
using TGC.Viewer.Utils.TgcGeometry;

namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Animacion para una malla con animacion esqueletica
    /// </summary>
    public class TgcSkeletalAnimation
    {
        public TgcSkeletalAnimation(string name, int frameRate, int framesCount,
            List<TgcSkeletalAnimationFrame>[] boneFrames, TgcBoundingBox boundingBox)
        {
            Name = name;
            FrameRate = frameRate;
            FramesCount = framesCount;
            BoneFrames = boneFrames;
            BoundingBox = boundingBox;
        }

        /// <summary>
        ///     Nombre de la animacion
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Velocidad de refresco de la animacion
        /// </summary>
        public int FrameRate { get; }

        /// <summary>
        ///     Total de cuadros que tiene la animacion
        /// </summary>
        public int FramesCount { get; }

        /// <summary>
        ///     Frames de animacion por cada uno de los huesos.
        ///     El array se encuentra en el mismo orden que la jerarquia de huesos del esquelto.
        ///     Están todos los huesos, aunque no tengan ningún KeyFrame.
        ///     Si no tienen ningun frame tienen la lista en null
        /// </summary>
        public List<TgcSkeletalAnimationFrame>[] BoneFrames { get; }

        /// <summary>
        ///     BoundingBox para esta animacion particular
        /// </summary>
        public TgcBoundingBox BoundingBox { get; }

        /// <summary>
        ///     Indica si el hueso tiene algun KeyFrame
        /// </summary>
        public bool hasFrames(int boneIdx)
        {
            return BoneFrames[boneIdx] != null && BoneFrames[boneIdx].Count > 0;
        }
    }
}