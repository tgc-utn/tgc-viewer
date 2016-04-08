namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Información de animación de un hueso para una animación particular
    /// </summary>
    public class TgcSkeletalAnimationBoneData
    {
        public int id;

        public TgcSkeletalAnimationBoneFrameData[] keyFrames;
        public int keyFramesCount;
    }
}