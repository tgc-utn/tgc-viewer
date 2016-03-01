namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     Información de animiación de un esqueleto
    /// </summary>
    public class TgcSkeletalAnimationData
    {
        //Info general de la animacion
        public int bonesCount;

        //Frames para cada Bone
        public TgcSkeletalAnimationBoneData[] bonesFrames;

        public int endFrame;
        public int frameRate;

        public int framesCount;
        public string name;

        public float[] pMax;

        //BoundingBox para esta animación
        public float[] pMin;

        public int startFrame;
    }
}