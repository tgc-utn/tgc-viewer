using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    /// <summary>
    /// Animaci�n de una malla animada por KeyFrames
    /// </summary>
    public class TgcKeyFrameAnimation
    {
        public TgcKeyFrameAnimation(TgcKeyFrameAnimationData data, TgcBoundingBox boundingBox)
        {
            this.data = data;
            this.boundingBox = boundingBox;
        }

        TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de la animaci�n
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        TgcKeyFrameAnimationData data;
        /// <summary>
        /// Datos de v�rtices de la animaci�n
        /// </summary>
        public TgcKeyFrameAnimationData Data
        {
            get { return data; }
        }

    }
}
