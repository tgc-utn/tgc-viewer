using Microsoft.DirectX;
using System;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a vector in two-dimensional (2-D) space.
    /// </summary>
    public class TGCVector2
    {
        /// <summary>
        /// Initializes a new instance of the TGCVector2 class.
        /// </summary>
        public TGCVector2()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the TGCVector2 class.
        /// </summary>
        /// <param name="valueX">Initial X value.</param>
        /// <param name="valueY">Initial Y value.</param>
        public TGCVector2(float valueX, float valueY)
        {
            throw new NotImplementedException();
        }

        public Vector2 DXVector2 { get; set; }

        /// <summary>
        /// Retrieves or sets the x component of a 2-D vector.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Retrieves or sets the y component of a 2-D vector.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Retrieves a 2-D vector (0,0).
        /// </summary>
        public static TGCVector2 Empty { get; }

        /// <summary>
        /// Retrieves a 2-D vector (1,1).
        /// </summary>
        public static TGCVector2 One { get; }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="v">Source TGCVector2.</param>
        public void Add(TGCVector2 v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>Sum of the two source TGCVector2.</returns>
        public static TGCVector2 Add(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a point in barycentric coordinates, using specified 2-D vectors.
        /// </summary>
        /// <param name="v1">Source TGCVector2.</param>
        /// <param name="v2">Source TGCVector2.</param>
        /// <param name="v3">Source TGCVector2.</param>
        /// <param name="f">Weighting factor.</param>
        /// <param name="g">Weighting factor.</param>
        /// <returns>A TGCVector2 in barycentric coordinates.</returns>
        public static TGCVector2 BaryCentric(TGCVector2 v1, TGCVector2 v2, TGCVector2 v3, float f, float g)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using specified 2-D vectors.
        /// </summary>
        /// <param name="position1">Source TGCVector2 that is a position vector.</param>
        /// <param name="position2">Source TGCVector2 that is a position vector.</param>
        /// <param name="position3">Source TGCVector2 that is a position vector.</param>
        /// <param name="position4">Source TGCVector2 that is a position vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns></returns>
        public static TGCVector2 CatmullRom(TGCVector2 position1, TGCVector2 position2, TGCVector2 position3,
            TGCVector2 position4, float weightingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the z component by calculating the cross product of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>The z component.</returns>
        public static float Ccw(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the dot product of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>Dot product.</returns>
        public static float Dot(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="compare">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object compare)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs a Hermite spline interpolation using specified 2-D vectors.
        /// </summary>
        /// <param name="position">Source TGCVector2 that is a position vector.</param>
        /// <param name="tangent">Source TGCVector2 that is a tangent vector.</param>
        /// <param name="position2">Source TGCVector2 that is a position vector.</param>
        /// <param name="tangent2">Source TGCVector2 that is a tangent vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector2 that is the result of the Hermite spline interpolation.</returns>
        public static TGCVector2 Hermite(TGCVector2 position, TGCVector2 tangent, TGCVector2 position2,
            TGCVector2 tangent2, float weightingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the length of a 2-D vector.
        /// </summary>
        /// <returns>Vector length.</returns>
        public float Length()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the length of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>Vector length.</returns>
        public static float Length(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the square of the length of a 2-D vector.
        /// </summary>
        /// <returns>Vector's squared length.</returns>
        public float LengthSq()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the square of the length of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>Vector's squared length.</returns>
        public static float LengthSq(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs a linear interpolation between two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <param name="interpolater">Parameter that linearly interpolates between the vectors.</param>
        /// <returns></returns>
        public static TGCVector2 Lerp(TGCVector2 left, TGCVector2 right, float interpolater)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the largest components of two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        public void Maximize(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the largest components of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>A TGCVector2 structure that is made up of the largest components of the two vectors.</returns>
        public static TGCVector2 Maximize(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the smallest components of two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        public void Minimize(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 2-D vector that is made up of the smallest components of two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source TGCVector2.</param>
        /// <returns>A TGCVector2 that is made up of the smallest components of the two vectors.</returns>
        public static TGCVector2 Minimize(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiplies the current 2-D vector with a single value.
        /// </summary>
        /// <param name="s">Source float value.</param>
        public void Multiply(float s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiplies the current 2-D vector with a single value.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="s">Source float value.</param>
        /// <returns>A TGCVector2 that is the result of the source parameter multiplied by the s parameter.</returns>
        public static TGCVector2 Multiply(TGCVector2 source, float s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the normalized version of a 2-D vector.
        /// </summary>
        public void Normalize()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns the normalized version of a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <returns>A Vector2 that is the normalized version of the vector.</returns>
        public static TGCVector2 Normalize(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds two 2-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the addition operator.</param>
        /// <param name="right">The TGCVector2 to the right of the addition operator.</param>
        /// <returns>Resulting TGCVector2.</returns>
        public static TGCVector2 operator +(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the equality operator.</param>
        /// <param name="right">The TGCVector2 to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the inequality operator.</param>
        /// <param name="right">The TGCVector2 to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the product of a single value and a 2-D vector.
        /// </summary>
        /// <param name="right">Source float structure.</param>
        /// <param name="left">Source TGCVector2.</param>
        /// <returns>A TGCVector2 structure that is the product of the right and left parameters.</returns>
        public static TGCVector2 operator *(float right, TGCVector2 left)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the product of a single value and a 2-D vector.
        /// </summary>
        /// /// <param name="left">Source TGCVector2.</param>
        /// <param name="right">Source float structure.</param>
        /// <returns>A TGCVector2 that is the product of the right and left parameters.</returns>
        public static TGCVector2 operator *(TGCVector2 left, float right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the subtraction operator.</param>
        /// <param name="right">The TGCVector2 to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCVector2.</returns>
        public static TGCVector2 operator -(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="vec">Source TGCVector2.</param>
        /// <returns>The TGCVector2 structure that is the result of the negation operation.</returns>
        public static TGCVector2 operator -(TGCVector2 vec)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scales a 2-D vector.
        /// </summary>
        /// <param name="scalingFactor">Scaling value.</param>
        public void Scale(float scalingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scales a 2-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="scalingFactor">Scaling value.</param>
        /// <returns>A TGCVector2 that is the scaled vector.</returns>
        public static TGCVector2 Scale(TGCVector2 source, float scalingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector2 to subtract from the current TGCVector2 instance.</param>
        public void Subtract(TGCVector2 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two 2-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector2 to the left of the subtraction operator.</param>
        /// <param name="right">Source TGCVector2 to the right of the subtraction operator.</param>
        /// <returns>A TGCVector2 that is the result of the operation.</returns>
        public static TGCVector2 Subtract(TGCVector2 left, TGCVector2 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A Vector4 structure that is the result of the method.</returns>
        public static Vector4 Transform(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of Vector4 structures that are the result of the method.</returns>
        public static Vector4[] Transform(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformCoordinate(TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector2 that represents the results of the method.</returns>
        public static TGCVector2 TransformCoordinate(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 2-D vector or an array of 2-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of TGCVector2 that represent the results of the method.</returns>
        public static TGCVector2[] TransformCoordinate(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformNormal(TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector2 that contains the results of this method.</returns>
        public static TGCVector2 TransformNormal(TGCVector2 source, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms the 2-D vector normal by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector2.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of TGCVector2 that contain the results of this method.</returns>
        public static TGCVector2[] TransformNormal(TGCVector2[] vector, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the DX Vector3 wrapped to be use in DX primitives.
        /// </summary>
        /// <returns>The DX Vector3 wrapped</returns>
        public Vector2 ToVector2()
        {
            return this.DXVector2;
        }

        /// <summary>
        /// Transform TGCVector2[] to DX Vector2[]
        /// </summary>
        /// <param name="am">Source TGCVector2.</param>
        /// <returns>A Vector2[] with all de wrapped Matrix in TGCVector2.</returns>
        public static Vector2[] ToVector2Array(TGCVector2[] am)
        {
            Vector2[] m = new Vector2[am.Length];

            for (int i = 0; i < am.Length; i++)
            {
                m[i] = am[i].ToVector2();
            }

            return m;
        }

        #region Old TGCVectorUtils

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string PrintVector2(float x, float y)
        {
            return "[" + TgcParserUtils.printFloat(x) + "," + TgcParserUtils.printFloat(y) + "]";
        }

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0]
        /// </summary>
        public static string PrintVector2(TGCVector2 vec)
        {
            return PrintVector2(vec.X, vec.Y);
        }

        /// <summary>
        ///     Imprime un TGCVector2 de la forma [150.0,150.0], tomando valores string
        /// </summary>
        public static string PrintVector2FromString(string x, string y)
        {
            return PrintVector2(TgcParserUtils.parseFloat(x), TgcParserUtils.parseFloat(y));
        }

        /// <summary>
        ///     Convierte un TGCVector2 a un float[2]
        /// </summary>
        public static float[] Vector2ToFloat2Array(TGCVector2 v)
        {
            return new[] { v.X, v.Y };
        }

        /// <summary>
        ///     Convierte un array de TGCVector2 a un array de float
        /// </summary>
        public static float[] Vector2ArrayToFloat2Array(TGCVector2[] values)
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