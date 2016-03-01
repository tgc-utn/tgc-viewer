namespace TGC.Viewer.Utils.TgcSkeletalAnimation
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