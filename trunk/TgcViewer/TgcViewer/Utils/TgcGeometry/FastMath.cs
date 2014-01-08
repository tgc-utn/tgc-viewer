using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Funciones matemáticas rápidas y optimizadas
    /// </summary>
    public abstract class FastMath
    {

        /// <summary>
        /// Representa la relación entre la longitud de la circunferencia de un círculo
        /// y su diámetro, especificada por la constante PI.
        /// </summary>
        public static readonly float PI = (float)Math.PI;

        /// <summary>
        /// PI / 2
        /// </summary>
        public static readonly float PI_HALF = PI / 2.0f;

        /// <summary>
        /// 2 PI
        /// </summary>
        public static readonly float TWO_PI = 2.0f * PI;

        /// <summary>
        /// PI / 4
        /// </summary>
        public static readonly float QUARTER_PI = PI / 4;

        /// <summary>
        /// Representa la base logarítmica natural, especificada por la constante, e.
        /// </summary>
        public static readonly float E = (float)Math.E;
        
        private static int precision = 0x100000;
        private static float[] sinTable = null,
                               cosTable = null,
                               tanTable = null;
        
        
        private static int radToIndex( float radians )
        {
            return( (int)( ( radians / TWO_PI ) * (float)precision ) & ( precision - 1 ) );
        }
        
        /// <summary>
        /// Devuelve el seno del ángulo especificado.
        /// Utiliza valores precalculados con una precisión fija para obtener el resultado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Seno de x</returns>
        public static float Sin( float x )
        {
            if ( sinTable == null )
            {
                sinTable = new float[ precision ];
                
                float rad_slice = TWO_PI / (float)precision;

                for ( int i = 0; i < precision; i++ )
                {
                    sinTable[ i ] = (float)Math.Sin( (float)i * rad_slice );
                }
            }
            
            return( sinTable[ radToIndex( x ) ] );
        }
        
        /// <summary>
        /// Devuelve el ángulo cuyo seno es el número especificado.
        /// </summary>
        /// <param name="x">Número que representa un seno, donde -1 <=x<= 1.</param>
        /// <returns>Arcoseno de x</returns>
        public static float Asin( float x )
        {
            return( (float)Math.Asin( x ) );
        }
        
        /// <summary>
        /// Devuelve el seno hiperbólico del ángulo especificado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Seno hiperbólico de x</returns>
        public static float Sinh( float x )
        {
            return( (float)Math.Sinh( x ) );
        }

        /// <summary>
        /// Devuelve el coseno del ángulo especificado.
        /// Utiliza valores precalculados con una precisión fija para obtener el resultado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Coseno de x</returns>
        public static float Cos( float x )
        {
            if ( cosTable == null )
            {
                cosTable = new float[ precision ];
                
                float rad_slice = TWO_PI / (float)precision;

                for ( int i = 0; i < precision; i++ )
                {
                    cosTable[ i ] = (float)Math.Cos( (float)i * rad_slice );
                }
            }
            
            return( cosTable[ radToIndex( x ) ] );
        }
        
        /// <summary>
        /// Devuelve el ángulo cuyo coseno es el número especificado.
        /// </summary>
        /// <param name="x">Número que representa un coseno, donde -1 <=x<= 1.</param>
        /// <returns>Arcocoseno de x</returns>
        public static float Acos( float x )
        {
            return( (float)Math.Acos( x ) );
        }
        
        /// <summary>
        /// Devuelve el coseno hiperbólico del ángulo especificado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Coseno hiperbólico de x</returns>
        public static float Cosh( float x )
        {
            return( (float)Math.Cosh( x ) );
        }

        /// <summary>
        /// Devuelve la tangente del ángulo especificado.
        /// Utiliza valores precalculados con una precisión fija para obtener el resultado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Tangente de x</returns>
        public static float Tan( float x )
        {
            if ( tanTable == null )
            {
                tanTable = new float[ precision ];
                
                float rad_slice = TWO_PI / (float)precision;

                for ( int i = 0; i < precision; i++ )
                {
                    tanTable[ i ] = (float)Math.Tan( (float)i * rad_slice );
                }
            }
            
            return( tanTable[ radToIndex( x ) ] );
        }
        
        /// <summary>
        /// Devuelve el ángulo cuya tangente corresponde al número especificado.
        /// </summary>
        /// <param name="x">Número que representa una tangente.</param>
        /// <returns>Arcotangente de x</returns>
        public static float Atan( float x )
        {
            return( (float)Math.Atan( x ) );
        }
        
        /// <summary>
        /// Devuelve el ángulo cuya tangente es el cociente de dos números especificados.
        /// </summary>
        /// <param name="y">Coordenada y de un punto.</param>
        /// <param name="x">Coordenada x de un punto.</param>
        /// <returns>Arcotangente</returns>
        public static float Atan2( float y, float x )
        {
            return( (float)Math.Atan2( y, x ) );
        }
        
        /// <summary>
        /// Devuelve la tangente hiperbólica del ángulo especificado.
        /// </summary>
        /// <param name="x">Ángulo, medido en radianes.</param>
        /// <returns>Tangente hiperbólica de x</returns>
        public static float Tanh( float x )
        {
            return( (float)Math.Tanh( x ) );
        }
        
        /// <summary>
        /// Devuelve la raíz cuadrada de un número especificado.
        /// </summary>
        /// <param name="x">Un número.</param>
        /// <returns>Raíz cuadrada</returns>
        public static float Sqrt( float x )
        {
            return( (float)Math.Sqrt( x ) );
        }
        
        /// <summary>
        /// Inversa de la raíz cuadrada de un número
        /// </summary>
        /// <param name="x">Un número.</param>
        /// <returns>Inversa de la raíz</returns>
        public static float InvSqrt( float x )
        {
            return( 1.0f / (float)Math.Sqrt( x ) );
        }
        
        /// <summary>
        /// Devuelve el siguiente entero mayor o igual que el número decimal especificado.
        /// </summary>
        /// <param name="x">Número decimal.</param>
        /// <returns>El número entero más pequeño mayor o igual que x.</returns>
        public static float Ceiling(float x)
        {
            return( (float)Math.Ceiling( x ) );
        }
        
        /// <summary>
        /// Devuelve el siguiente entero menor o igual que el número de punto flotante
        /// de precisión doble especificado.
        /// </summary>
        /// <param name="x">Número decimal.</param>
        /// <returns>Número entero más grande menor o igual que x.</returns>
        public static float Floor( float x )
        {
            return( (float)Math.Floor( x ) );
        }
        
        /// <summary>
        /// Devuelve el logaritmo natural (en base e) de un número especificado.
        /// </summary>
        /// <param name="x">Número cuyo logaritmo hay que calcular.</param>
        /// <returns>Logaritmo natural de x</returns>
        public static float Log( float x )
        {
            return( (float)Math.Log( x ) );
        }

        /// <summary>
        /// Devuelve el logaritmo en base 10 de un número especificado.
        /// </summary>
        /// <param name="x">Número cuyo logaritmo hay que calcular.</param>
        /// <returns>Logaritmo en base 10 de x</returns>
        public static float Log10( float x )
        {
            return( (float)Math.Log10( x ) );
        }
        
        /// <summary>
        /// Convierte radianes en grados
        /// </summary>
        /// <param name="x">Valor en radianes</param>
        /// <returns>Valor en grados</returns>
        public static float ToDeg( float x )
        {
            return ((float)Geometry.RadianToDegree(x));
        }

        /// <summary>
        /// Convierte grados en radiantes
        /// </summary>
        /// <param name="x">Valor en grados</param>
        /// <returns>Valor en radianes</returns>
        public static float ToRad( float x )
        {
            return Geometry.DegreeToRadian(x);
        }
        
        /// <summary>
        /// Devuelve un número especificado elevado a la potencia especificada.
        /// </summary>
        /// <param name="x">Número que especifica una potencia.</param>
        /// <param name="y">Número que se desea elevar a una potencia.</param>
        /// <returns>Número x elevado a la potencia y</returns>
        public static float Pow( float x, float y )
        {
            return( (float)Math.Pow( x, y ) );
        }
        
        /// <summary>
        /// Devuelve un número especificado elevado al cuadrado.
        /// </summary>
        /// <param name="x">Número que especifica una potencia.</param>
        /// <returns>Número x elevado al cuadrado</returns>
        public static float Pow2( float x )
        {
            return( Pow( x, 2f ) );
        }
        
        /// <summary>
        /// Devuelve un número especificado elevado a la potencia especificada, como Integer.
        /// </summary>
        /// <param name="x">Número que especifica una potencia.</param>
        /// <param name="y">Número que se desea elevar a una potencia.</param>
        /// <returns>Número x elevado a la potencia y</returns>
        public static int Pow( int x, int y )
        {
            return( (int)Math.Pow( x, y ) );
        }
        
        /// <summary>
        /// Compara la igualdad de dos números teniendo en cuenta una diferencia Epsilon
        /// </summary>
        /// <param name="v1">Número 1</param>
        /// <param name="v2">Número 2</param>
        /// <returns>Resultado de la comparación</returns>
        public static bool EpsilonEquals( float v1, float v2 )
        {
    	    return( Abs( v1 - v2 ) < float.Epsilon );
        }

        /// <summary>
        /// Devuelve el valor absoluto de un número
        /// </summary>
        public static float Abs(float v)
        {
            return (float)Math.Abs(v);
        }

        /// <summary>
        /// Devuelve el valor mínimo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>Mínimo valor</returns>
        public static float Min(float v1, float v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        /// <summary>
        /// Devuelve el valor mínimo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>Mínimo valor</returns>
        public static int Min(int v1, int v2)
        {
            return v1 < v2 ? v1 : v2;
        }

        /// <summary>
        /// Devuelve el valor máximo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>Máximo valor</returns>
        public static float Max(float v1, float v2)
        {
            return v1 > v2 ? v1 : v2;
        }

        /// <summary>
        /// Devuelve el valor máximo entre v1 y v2
        /// </summary>
        /// <param name="v1">Valor 1</param>
        /// <param name="v2">Valor 2</param>
        /// <returns>Máximo valor</returns>
        public static int Max(int v1, int v2)
        {
            return v1 > v2 ? v1 : v2;
        }

        /// <summary>
        /// Devuelve E^n
        /// </summary>
        /// <param name="n">Exponente</param>
        /// <returns>E elevado a la n</returns>
        public static float Exp(float n)
        {
            return (float)Math.Exp(n);
        }

        /// <summary>
        /// Calculo de funcion de Gauss
        /// </summary>
        /// <param name="x">valor de X</param>
        /// <param name="y">valor de Y</param>
        /// <param name="rho">standard deviation</param>
        /// <returns>valor de la funcion de distribucion de Gauss</returns>
        public static float GaussianDistribution(float x, float y, float rho)
        {
            float g = 1.0f / FastMath.Sqrt(2.0f * FastMath.PI * rho * rho);
            g *= FastMath.Exp(-(x * x + y * y) / (2 * rho * rho));
            return g;
        }

        /// <summary>
        /// Calculo de funcion de Gauss
        /// </summary>
        /// <param name="x">valor de X</param>
        /// <param name="rho">standard deviation</param>
        /// <returns>valor de la funcion de distribucion de Gauss</returns>
        public static float GaussianDistribution(float x, float rho)
        {
            float g = 1.0f / FastMath.Sqrt(2.0f * FastMath.PI * rho * rho);
            g *= FastMath.Exp(-(x * x) / (2 * rho * rho));
            return g;
        }

    }

    
}
