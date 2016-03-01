namespace TGC.Viewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    ///     KeyFrame de animación de un hueso, para una animación particular
    /// </summary>
    public class TgcSkeletalAnimationBoneFrameData
    {
        //Numero de frame en el que ocurre la animacion
        public int frame;

        public float[] position;
        public float[] rotation;
    }
}