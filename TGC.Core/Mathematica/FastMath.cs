using System;
using SharpDX;

namespace TGC.Core.Mathematica
{
    /// <summary>
    ///     Funciones matem�ticas r�pidas y optimizadas
    /// </summary>
    public abstract class FastMath
    {
        /// <summary>
        ///     Representa la relaci�n entre la longitud de la circunferencia de un c�rculo
        ///     y su di�metro, especificada por la constante PI.
        /// </summary>
        public static readonly float PI = (float)Math.PI;

        /// <summary>
        ///     PI / 2
        /// </summary>
        public static readonly float PI_HALF = PI / 2.0f;

        /// <summary>
        ///     2 PI
        /// </summary>
        public static readonly float TWO_PI = 2.0f * PI;

        /// <summary>
        ///     PI / 4
        /// </summary>
        public static readonly float QUARTER_PI = PI / 4;

        /// <summary>
        ///     Representa la base logar�tmica natural, especificada por la constante, e.
        /// </summary>
        public static readonly float E = (float)Math.E;

        private static readonly int precision = 0x100000;

        private static float[] sinTable,
            cosTable,
            tanTable;

        private static int radToIndex(float radians)
        {
            return (int)(radians / TWO_PI * precision) & (precision - 1);
        }

        /// <summary>
        ///     Devuelve el seno del �ngulo especificado.
        ///     Utiliza valores precalculados con una precisi�n fija para obtener el resultado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Seno de x</returns>
        public static float Sin(float x)
        {
            if (sinTable == null)
            {
                sinTable = new float[precision];

                var rad_slice = TWO_PI / precision;

                for (var i = 0; i < precision; i++)
                {
                    sinTable[i] = (float)Math.Sin(i * rad_slice);
                }
            }

            return sinTable[radToIndex(x)];
        }

        /// <summary>
        ///     Devuelve el �ngulo cuyo seno es el n�mero especificado.
        /// </summary>
        /// <param name="x">
        ///     N�mero que representa un seno, donde -1 es menor a x que es menor a 1
        /// </param>
        /// <returns>Arcoseno de x</returns>
        public static float Asin(float x)
        {
            return (float)Math.Asin(x);
        }

        /// <summary>
        ///     Devuelve el seno hiperb�lico del �ngulo especificado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Seno hiperb�lico de x</returns>
        public static float Sinh(float x)
        {
            return (float)Math.Sinh(x);
        }

        /// <summary>
        ///     Devuelve el coseno del �ngulo especificado.
        ///     Utiliza valores precalculados con una precisi�n fija para obtener el resultado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Coseno de x</returns>
        public static float Cos(float x)
        {
            if (cosTable == null)
            {
                cosTable = new float[precision];

                var rad_slice = TWO_PI / precision;

                for (var i = 0; i < precision; i++)
                {
                    cosTable[i] = (float)Math.Cos(i * rad_slice);
                }
            }

            return cosTable[radToIndex(x)];
        }

        /// <summary>
        ///     Devuelve el �ngulo cuyo coseno es el n�mero especificado.
        /// </summary>
        /// <param name="x">
        ///     N�mero que representa un coseno, donde -1 menor igual a x menor igual a 1.
        /// </param>
        /// <returns>Arcocoseno de x</returns>
        public static float Acos(float x)
        {
            return (float)Math.Acos(x);
        }

        /// <summary>
        ///     Devuelve el coseno hiperb�lico del �ngulo especificado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Coseno hiperb�lico de x</returns>
        public static float Cosh(float x)
        {
            return (float)Math.Cosh(x);
        }

        /// <summary>
        ///     Devuelve la tangente del �ngulo especificado.
        ///     Utiliza valores precalculados con una precisi�n fija para obtener el resultado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Tangente de x</returns>
        public static float Tan(float x)
        {
            if (tanTable == null)
            {
                tanTable = new float[precision];

                var rad_slice = TWO_PI / precision;

                for (var i = 0; i < precision; i++)
                {
                    tanTable[i] = (float)Math.Tan(i * rad_slice);
                }
            }

            return tanTable[radToIndex(x)];
        }

        /// <summary>
        ///     Devuelve el �ngulo cuya tangente corresponde al n�mero especificado.
        /// </summary>
        /// <param name="x">N�mero que representa una tangente.</param>
        /// <returns>Arcotangente de x</returns>
        public static float Atan(float x)
        {
            return (float)Math.Atan(x);
        }

        /// <summary>
        ///     Devuelve el �ngulo cuya tangente es el cociente de dos n�meros especificados.
        /// </summary>
        /// <param name="y">Coordenada y de un punto.</param>
        /// <param name="x">Coordenada x de un punto.</param>
        /// <returns>Arcotangente</returns>
        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        /// <summary>
        ///     Devuelve la tangente hiperb�lica del �ngulo especificado.
        /// </summary>
        /// <param name="x">�ngulo, medido en radianes.</param>
        /// <returns>Tangente hiperb�lica de x</returns>
        public static float Tanh(float x)
        {
            return (float)Math.Tanh(x);
        }

        /// <summary>
        ///     Devuelve la ra�z cuadrada de un n�mero especificado.
        /// </summary>
        /// <param name="x">Un n�mero.</param>
        /// <returns>Ra�z cuadrada</returns>
        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        /// <summary>
        ///     Inversa de la ra�z cuadrada de un n�mero
        /// </summary>
        /// <param name="x">Un n�mero.</param>
        /// <returns>Inversa de la ra�z</returns>
        public static float InvSqrt(float x)
        {
            return 1.0f / (float)Math.Sqrt(x);
        }

        /// <summary>
        ///     Devuelve el siguiente entero mayor o igual que el n�mero decimal especificado.
        /// </summary>
        /// <param name="x">N�mero decimal.</param>
        /// <returns>El n�mero entero m�s peque�o mayor o igual que x.</returns>
        public static float Ceiling(float x)
        {
            return (float)Math.Ceiling(x);
        }

        /// <summary>
        ///     Devuelve el siguiente entero menor o igual que el n�mero de punto flotante
        ///     de precisi�n doble especificado.
        /// </summary>
        /// <param name="x">N�mero decimal.</param>
        /// <returns>N�mero entero m�s grande menor o igual que x.</returns>
        public static float Floor(float x)
        {
            return (float)Math.Floor(x);
        }

        /// <summary>
        ///     Devuelve el logaritmo natural (en base e) de un n�mero especificado.
        /// </summary>
        /// <param name="x">N�mero cuyo logaritmo hay que calcular.</param>
        /// <returns>Logaritmo natural de x</returns>
        public static float Log(float x)
        {
            return (float)Math.Log(x);
        }

        /// <summary>
        ///     Devuelve el logaritmo en base 10 de un n�mero especificado.
        /// </summary>
        /// <param name="x">N�mero cuyo logaritmo hay que calcular.</param>
        /// <returns>Logaritmo en base 10 de x</returns>
        public static float Log10(float x)
        {
            return (float)Math.Log10(x);
        }

        /// <summary>
        ///     Convierte radianes en grados
        /// </summary>
        /// <param name="x">Valor en radianes</param>
        /// <returns>Valor en grados</returns>
        public static float ToDeg(float x)
        {
            return MathUtil.RadiansToDegrees(x);
        }

        /// <summary>
        ///     Convierte grados en radiantes
        /// </summary>
        /// <param name="x">Valor en grados</param>
        /// <returns>Valor en radianes</returns>
        public static float ToRad(float x)
        {
            return MathUtil.DegreesToRadians(x);
        }

        /// <summary>
        ///     Devuelve un n�mero especificado elevado a la potencia especificada.
        /// </summary>
        /// <param name="x">N�mero que especifica una potencia.</param>
        /// <param name="y">N�mero que se desea elevar a una potencia.</param>
        /// <returns>N�mero x elevado a la potencia y</returns>
        public static float Pow(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }

        /// <summary>
        ///     Devuelve un n�mero especificado elevado al cuadrado.
        /// </summary>
        /// <param name="x">N�mero que especifica una potencia.</param>
        /// <returns>N�mero x elevado al cuadrado</returns>
        public static float Pow2(float x)
        {
            return Pow(x, 2f);
        }

        /// <summary>
        ///     Devuelve un n�mero especificado elevado a la potencia especificada, como Integer.
        /// </summary>
        /// <param name="x">N�mero que especifica una potencia.</param>
        /// <param name="y">N�mero que se desea elevar a una potencia.</param>
        /// <returns>N�mero x elevado a la potencia y</returns>
        public static int Pow(int x, int y)
        {
            return (int)Math.Pow(x, y);
        }

        /// <summary>
        ///     Compara la igualdad de dos n�meros teniendo en cuenta una diferencia Epsilon
        /// </summary>
        /// <param name="v1">N�mero 1</param>
        /// <param name="v2">N�mero 2</param>
        /// <returns>Resultado de la comparaci�n</returns>
        public static bool EpsilonEquals(float v1, float v2)
        {
            return Abs(v1 - v2) < float.Epsilon;
        }

        /// <summary>
        ///     Devuelve el valor absoluto de un n�mero
        /// </summary>
        public static float Abs(float v)
        {
            return Math.Abs(v);
        }

        /// <summary>
        ///     Devuelve el valor m�nimo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>M�nimo valor</returns>
        public static float Min(float v1, float v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        /// <summary>
        ///     Devuelve el valor m�nimo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>M�nimo valor</returns>
        public static int Min(int v1, int v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        /// <summary>
        ///     Devuelve el valor m�ximo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>M�ximo valor</returns>
        public static float Max(float v1, float v2)
        {
            return v1 > v2 ? v1 : v2;
        }

        /// <summary>
        ///     Devuelve el valor m�ximo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>M�ximo valor</returns>
        public static int Max(int v1, int v2)
        {
            return v1 > v2 ? v1 : v2;
        }

        /// <summary>
        ///     Clampear x al intervalo [min, max]
        /// </summary>
        /// <param name="x">Valor a hacer clamp</param>
        /// <param name="min">Maximo inclusive</param>
        /// <param name="max">Minimo inclusive</param>
        /// <returns>Valor campleado</returns>
        public static float Clamp(float x, float min, float max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }

        /// <summary>
        ///     Clampear x al intervalo [min, max]
        /// </summary>
        /// <param name="x">Valor a hacer clamp</param>
        /// <param name="min">Maximo inclusive</param>
        /// <param name="max">Minimo inclusive</param>
        /// <returns>Valor campleado</returns>
        public static int Clamp(int x, int min, int max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }

        /// <summary>
        ///     Devuelve E^n
        /// </summary>
        /// <param name="n">Exponente</param>
        /// <returns>E elevado a la n</returns>
        public static float Exp(float n)
        {
            return (float)Math.Exp(n);
        }

        /// <summary>
        ///     Calculo de funcion de Gauss
        /// </summary>
        /// <param name="x">valor de X</param>
        /// <param name="y">valor de Y</param>
        /// <param name="rho">standard deviation</param>
        /// <returns>valor de la funcion de distribucion de Gauss</returns>
        public static float GaussianDistribution(float x, float y, float rho)
        {
            var g = 1.0f / Sqrt(2.0f * PI * rho * rho);
            g *= Exp(-(x * x + y * y) / (2 * rho * rho));
            return g;
        }

        /// <summary>
        ///     Calculo de funcion de Gauss
        /// </summary>
        /// <param name="x">valor de X</param>
        /// <param name="rho">standard deviation</param>
        /// <returns>valor de la funcion de distribucion de Gauss</returns>
        public static float GaussianDistribution(float x, float rho)
        {
            var g = 1.0f / Sqrt(2.0f * PI * rho * rho);
            g *= Exp(-(x * x) / (2 * rho * rho));
            return g;
        }
    }
}