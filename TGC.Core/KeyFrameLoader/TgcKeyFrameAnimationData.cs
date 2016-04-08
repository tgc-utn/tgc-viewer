namespace TGC.Core.KeyFrameLoader
{
    public class TgcKeyFrameAnimationData
    {
        public int endFrame;
        public int frameRate;

        public int framesCount;

        //KeyFrames de la animacion
        public TgcKeyFrameFrameData[] keyFrames;

        public int keyFramesCount;
        public string name;

        public float[] pMax;

        //BoundingBox para esta animación
        public float[] pMin;

        public int startFrame;

        //Info general de la animacion
        public int verticesCount;
    }
}