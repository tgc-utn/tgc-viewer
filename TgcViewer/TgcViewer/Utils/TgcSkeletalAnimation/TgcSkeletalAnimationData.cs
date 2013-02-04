using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    /// Información de animiación de un esqueleto
    /// </summary>
    public class TgcSkeletalAnimationData
    {
        public string name;

        //Info general de la animacion
        public int bonesCount;
        public int framesCount;
        public int frameRate;
        public int startFrame;
        public int endFrame;

        //Frames para cada Bone
        public TgcSkeletalAnimationBoneData[] bonesFrames;

        //BoundingBox para esta animación
        public float[] pMin;
        public float[] pMax;
    }
}
