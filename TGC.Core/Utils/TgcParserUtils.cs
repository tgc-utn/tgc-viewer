using Microsoft.DirectX;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace TGC.Core.Utils
{
    public class TgcParserUtils
    {
        private static readonly NumberFormatInfo numberInfo = new NumberFormatInfo();
        private static readonly CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        static TgcParserUtils()
        {
            numberInfo.NumberDecimalSeparator = ".";
        }

        /// <summary>
        ///     Parsea un float, con un NumberFormatInfo independiente de la pc
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float parseFloat(string text)
        {
            return float.Parse(text, numberInfo);
        }

        /// <summary>
        ///     Parsea un int
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int parseInt(string text)
        {
            return int.Parse(text);
        }

        /// <summary>
        ///     Parsea el string "[-8.00202,-6.87125,0]" y devuelve un array de 3 floats.
        ///     Sin invertir nada
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float[] parseFloat3Array(string text)
        {
            var aux = text.Substring(1, text.Length - 2);
            var n = aux.Split(',');
            return new[] { parseFloat(n[0]), parseFloat(n[1]), parseFloat(n[2]) };
        }

        /// <summary>
        ///     Parsea el string "[-8.00202,-6.87125,0]" y devuelve un array de 3 floats.
        ///     Invierte las coordenadas (x,y,z) a (x,z,y), de formato 3DsMAX a DirectX
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float[] parseFloat3ArrayAdapted(string text)
        {
            var array = parseFloat3Array(text);
            var aux = array[1];
            array[1] = array[2];
            array[2] = aux;
            return array;
        }

        /// <summary>
        ///     Parsea el string "[4,2,1]" y devuelve un array de 3 int.
        ///     Sin invertir nada
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] parseInt3Array(string text)
        {
            var aux = text.Substring(1, text.Length - 2);
            var n = aux.Split(',');
            return new[] { int.Parse(n[0]), int.Parse(n[1]), int.Parse(n[2]) };
        }

        /// <summary>
        ///     Parsea el string "[4,2,1]" y devuelve un array de 3 int.
        ///     Invierte las coordenadas (x,y,z) a (x,z,y), de formato 3DsMAX a DirectX
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] parseInt3ArrayAdapted(string text)
        {
            var array = parseInt3Array(text);
            var aux = array[1];
            array[1] = array[2];
            array[2] = aux;
            return array;
        }

        /// <summary>
        ///     Parsea el string "[-8.00202,-6.87125]" y devuelve un array de 2 floats.
        ///     Sin invertir nada
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float[] parseFloat2Array(string text)
        {
            var aux = text.Substring(1, text.Length - 2);
            var n = aux.Split(',');
            return new[] { parseFloat(n[0]), parseFloat(n[1]) };
        }

        /// <summary>
        ///     Parsea el string "[-8.00202,-6.87125]" y devuelve un array de 2 floats.
        ///     Invierte las coordenadas (x,y) a (y,x), de formato 3DsMAX a DirectX
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float[] parseFloat2ArrayAdapted(string text)
        {
            var array = parseFloat2Array(text);
            var aux = array[0];
            array[0] = array[1];
            array[1] = aux;
            return array;
        }

        /// <summary>
        ///     Parsea el string "[4,2]" y devuelve un array de 2 int.
        ///     Invierte las coordenadas (x,y,z) a (x,z,y), de formato 3DsMAX a DirectX
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] parseInt2Array(string text)
        {
            var aux = text.Substring(1, text.Length - 2);
            var n = aux.Split(',');
            return new[] { int.Parse(n[0]), int.Parse(n[1]) };
        }

        /// <summary>
        ///     Parsea el string "[4,2]" y devuelve un array de 2 int.
        ///     Sin invertir nada
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] parseInt2ArrayAdapted(string text)
        {
            var array = parseInt2Array(text);
            var aux = array[0];
            array[0] = array[1];
            array[1] = aux;
            return array;
        }

        /// <summary>
        ///     Parsea el string "[-8.00202,-6.87125,0,0.211]" y devuelve un array de 4 floats.
        ///     Sin invertir nada
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float[] parseFloat4Array(string text)
        {
            var aux = text.Substring(1, text.Length - 2);
            var n = aux.Split(',');
            return new[]
            {
                parseFloat(n[0]),
                parseFloat(n[1]),
                parseFloat(n[2]),
                parseFloat(n[3])
            };
        }

        /// <summary>
        ///     Dividir todo el array de floats por el valor especificado
        /// </summary>
        /// <param name="array"></param>
        /// <param name="divValue"></param>
        public static void divFloatArrayValues(ref float[] array, float divValue)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] /= divValue;
            }
        }

        /// <summary>
        ///     Suma a todo el array de floats el valor especificado
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sumValue"></param>
        public static void sumFloatArrayValues(ref float[] array, float sumValue)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] += sumValue;
            }
        }

        /// <summary>
        ///     Suma a todo el array de ints el valor especificado
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sumValue"></param>
        public static void sumIntArrayValues(ref int[] array, int sumValue)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] += sumValue;
            }
        }

        /// <summary>
        ///     Parsea un flujo continuo de ints de la forma: 15 10 16 11 16 10 16 11 17 12 17 11 17 12...
        /// </summary>
        /// <param name="text">flujo de ints</param>
        /// <param name="count">cantidad de valores dentro del flujo</param>
        /// <returns></returns>
        public static int[] parseIntStream(string text, int count)
        {
            var array = new int[count];
            var textArray = text.Split(' ');
            for (var i = 0; i < count; i++)
            {
                array[i] = int.Parse(textArray[i]);
            }
            return array;
        }

        /// <summary>
        ///     Parsea un flujo continuo de floats de la forma: -74.1818 0.0 1.01613 -49.6512 0.0 1.01613...
        /// </summary>
        /// <param name="text">flujo de floats</param>
        /// <param name="count">cantidad de valores dentro del flujo</param>
        /// <returns></returns>
        public static float[] parseFloatStream(string text, int count)
        {
            var array = new float[count];
            var textArray = text.Split(' ');
            for (var i = 0; i < count; i++)
            {
                array[i] = parseFloat(textArray[i]);
            }
            return array;
        }

        /// <summary>
        ///     Convierte un float a un string, con un NumberFormatInfo independiente de la pc
        /// </summary>
        public static string printFloat(float f)
        {
            //return f.ToString(TgcParserUtils.numberInfo);
            return string.Format(cultureInfo, "{0:0.####}", f);
        }

        /// <summary>
        ///     Imprime un Quaternion de la forma [x, y, z, w]
        /// </summary>
        public static string printQuaternion(Quaternion q)
        {
            return "[" + printFloat(q.X) +
                   "," + printFloat(q.Y) +
                   "," + printFloat(q.Z) +
                   "," + printFloat(q.W) +
                   "]";
        }

        /// <summary>
        ///     Imprime un float[2] de la forma [150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string printFloat2Array(float[] array)
        {
            return "[" + printFloat(array[0]) +
                   "," + printFloat(array[1]) + "]";
        }

        /// <summary>
        ///     Imprime un float[3] de la forma [150.0,150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string printFloat3Array(float[] array)
        {
            return "[" + printFloat(array[0]) +
                   "," + printFloat(array[1]) +
                   "," + printFloat(array[2]) + "]";
        }

        /// <summary>
        ///     Imprime un float[4] de la forma [150.0,150.0,150.0,255.0]
        /// </summary>
        /// <returns></returns>
        public static string printFloat4Array(float[] array)
        {
            return "[" + printFloat(array[0]) +
                   "," + printFloat(array[1]) +
                   "," + printFloat(array[2]) +
                   "," + printFloat(array[3]) + "]";
        }

        /// <summary>
        ///     Toma un array de floats y lo imprime en un string de la forma: -74.1818 0.0 1.01613 -49.6512 0.0 1.01613...
        /// </summary>
        public static string printFloatStream(float[] stream)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < stream.Length; i++)
            {
                sb.Append(printFloat(stream[i]));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Toma un array de INT y lo imprime en un string de la forma: -74 -49 0 15 ...
        /// </summary>
        public static string printIntStream(int[] stream)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < stream.Length; i++)
            {
                sb.Append(stream[i]);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Convierte un Quaternion a un float[4]
        /// </summary>
        public static float[] quaternionToFloat4Array(Quaternion q)
        {
            return new[] { q.X, q.Y, q.Z, q.W };
        }
    }
}