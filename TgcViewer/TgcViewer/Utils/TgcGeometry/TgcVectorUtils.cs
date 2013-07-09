using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Herramientas de manipulación de vectores
    /// </summary>
    public abstract class TgcVectorUtils
    {

        /// <summary>
        /// Longitud al cuadrado del segmento ab
        /// </summary>
        /// <param name="a">Punto inicial del segmento</param>
        /// <param name="b">Punto final del segmento</param>
        /// <returns>Longitud al cuadrado</returns>
        public static float lengthSq(Vector3 a, Vector3 b)
        {
            return Vector3.Subtract(a, b).LengthSq();
        }

        /// <summary>
        /// Multiplicar dos vectores.
        /// Se multiplica cada componente
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector resultante</returns>
        public static Vector3 mul(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        /// <summary>
        /// Dividir dos vectores.
        /// Se divide cada componente
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector resultante</returns>
        public static Vector3 div(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        /// <summary>
        /// Multiplicar un Vector3 por una Matriz.
        /// Devuelve un Vector3 ignorando W.
        /// </summary>
        /// <param name="v">Vector</param>
        /// <param name="m">Matriz</param>
        /// <returns>Vector resultante, sin W</returns>
        public static Vector3 transform(Vector3 v, Matrix m)
        {
            Vector4 t = Vector3.Transform(v, m);
            return new Vector3(t.X, t.Y, t.Z);
        }

        /// <summary>
        /// Aplica el valor absoluto a todos los componentes del vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Vector resultante</returns>
        public static Vector3 abs(Vector3 v)
        {
            return new Vector3(FastMath.Abs(v.X), FastMath.Abs(v.Y), FastMath.Abs(v.Z));
        }

        /// <summary>
        /// Devuelve el menor valor de los 3 componentes del vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Menor valor de los tres</returns>
        public static float min(Vector3 v)
        {
            return FastMath.Min(FastMath.Min(v.X, v.Y), v.Z);
        }

    }
}
