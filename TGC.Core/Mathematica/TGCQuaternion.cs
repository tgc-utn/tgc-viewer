using Microsoft.DirectX;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
    public class TGCQuaternion
    {
        #region Old TGCVectorUtils && TGCParserUtils
        /// <summary>
        ///     Convierte un float[4] a un Quaternion
        /// </summary>
        public static Quaternion Float4ArrayToQuaternion(float[] f)
        {
            return new Quaternion(f[0], f[1], f[2], f[3]);
        }

		/// <summary>
		///     Convierte un Quaternion a un float[4]
		/// </summary>
		public static float[] QuaternionToFloat4Array(Quaternion q)
		{
			return new[] { q.X, q.Y, q.Z, q.W };
		}

		/// <summary>
		///     Imprime un Quaternion de la forma [x, y, z, w]
		/// </summary>
		public static string PrintQuaternion(Quaternion q)
		{
			return "[" + TgcParserUtils.printFloat(q.X) +
				   "," + TgcParserUtils.printFloat(q.Y) +
				   "," + TgcParserUtils.printFloat(q.Z) +
				   "," + TgcParserUtils.printFloat(q.W) +
				   "]";
		}

        #endregion
    }
}