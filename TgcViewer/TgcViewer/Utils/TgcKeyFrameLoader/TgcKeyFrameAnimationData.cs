using System;
using System.Collections.Generic;
using System.Text;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    public class TgcKeyFrameAnimationData
    {

        public string name;

        //Info general de la animacion
        public int verticesCount;
        public int framesCount;
        public int keyFramesCount;
        public int frameRate; 
        public int startFrame;
        public int endFrame;

        //KeyFrames de la animacion
        public TgcKeyFrameFrameData[] keyFrames;

        //BoundingBox para esta animación
        public float[] pMin;
        public float[] pMax;
    }
}
