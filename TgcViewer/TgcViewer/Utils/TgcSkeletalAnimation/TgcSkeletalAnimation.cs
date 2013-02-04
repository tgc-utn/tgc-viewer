using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace TgcViewer.Utils.TgcSkeletalAnimation
{
    /// <summary>
    /// Animacion para una malla con animacion esqueletica
    /// </summary>
    public class TgcSkeletalAnimation
    {
        public TgcSkeletalAnimation(string name, int frameRate, int framesCount, List<TgcSkeletalAnimationFrame>[] boneFrames, TgcBoundingBox boundingBox)
        {
            this.name = name;
            this.frameRate = frameRate;
            this.framesCount = framesCount;
            this.boneFrames = boneFrames;
            this.boundingBox = boundingBox;
        }

        private string name;
        /// <summary>
        /// Nombre de la animacion
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private int frameRate;
        /// <summary>
        /// Velocidad de refresco de la animacion
        /// </summary>
        public int FrameRate
        {
            get { return frameRate; }
        }

        private int framesCount;
        /// <summary>
        /// Total de cuadros que tiene la animacion
        /// </summary>
        public int FramesCount
        {
            get { return framesCount; }
        }

        private List<TgcSkeletalAnimationFrame>[] boneFrames;
        /// <summary>
        /// Frames de animacion por cada uno de los huesos.
        /// El array se encuentra en el mismo orden que la jerarquia de huesos del esquelto.
        /// Están todos los huesos, aunque no tengan ningún KeyFrame.
        /// Si no tienen ningun frame tienen la lista en null
        /// </summary>
        public List<TgcSkeletalAnimationFrame>[] BoneFrames
        {
            get { return boneFrames; }
        }

        /// <summary>
        /// Indica si el hueso tiene algun KeyFrame
        /// </summary>
        public bool hasFrames(int boneIdx)
        {
            return boneFrames[boneIdx] != null && boneFrames[boneIdx].Count > 0;
        }

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox para esta animacion particular
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        
    }
}
