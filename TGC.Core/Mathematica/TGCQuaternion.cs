using System;
using Microsoft.DirectX;

namespace TGC.Core.Mathematica
{
    public class TGCQuaternion
    {
        #region Old TGCVectorUtils
        /// <summary>
        ///     Convierte un float[4] a un Quaternion
        /// </summary>
        public static Quaternion Float4ArrayToQuaternion(float[] f)
        {
            return new Quaternion(f[0], f[1], f[2], f[3]);
        }

        #endregion
    }
}
