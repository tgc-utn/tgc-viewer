using System.Drawing;
using Microsoft.DirectX;

namespace TGC.Core.Mathematica
{
    public class TGCVector4
    {

        #region Old TGCVectorUtils

        /// <summary>
        ///     convierte un color base(255,255,255,255) a un Vector4(1f,1f,1f,1f).
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Vector4 ColorToVector4(Color color)
        {
            return Vector4.Normalize(new Vector4(color.R, color.G, color.B, color.A));
        }

        #endregion
    }
}
