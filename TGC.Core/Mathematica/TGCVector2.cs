using Microsoft.DirectX;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
    public class TGCVector2
    {

        #region Old TGCVectorUtils

        /// <summary>
        ///     Imprime un Vector2 de la forma [150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string printVector2(float x, float y)
        {
            return "[" + TgcParserUtils.printFloat(x) + "," + TgcParserUtils.printFloat(y) + "]";
        }

        /// <summary>
        ///     Imprime un Vector2 de la forma [150.0,150.0]
        /// </summary>
        public static string printVector2(Vector2 vec)
        {
            return printVector2(vec.X, vec.Y);
        }

        /// <summary>
        ///     Imprime un Vector2 de la forma [150.0,150.0], tomando valores string
        /// </summary>
        public static string printVectorFromString(string x, string y)
        {
            return printVector2(TgcParserUtils.parseFloat(x), TgcParserUtils.parseFloat(y));
        }

        /// <summary>
        ///     Convierte un Vector2 a un float[2]
        /// </summary>
        public static float[] vector2ToFloat2Array(Vector2 v)
        {
            return new[] { v.X, v.Y };
        }

        /// <summary>
        ///     Convierte un array de Vector2 a un array de float
        /// </summary>
        public static float[] vector2ArrayToFloat2Array(Vector2[] values)
        {
            var data = new float[values.Length * 2];
            for (var i = 0; i < values.Length; i++)
            {
                data[i * 2] = values[i].X;
                data[i * 2 + 1] = values[i].Y;
            }
            return data;
        }

        #endregion
    }
}
