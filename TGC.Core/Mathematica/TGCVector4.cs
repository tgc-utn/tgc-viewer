using Microsoft.DirectX;
using System;
using System.Drawing;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a vector in four-dimensional (4-D) space.
    /// </summary>
    public struct TGCVector4
    {
        /// <summary>
        /// Retrieves or sets the DirectX of a 4-D vector.
        /// </summary>
        private Vector4 dxVector4;

        /// <summary>
        /// Initializes a new instance of the TGCVector4 class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <param name="w">The w coordinate.</param>
        public TGCVector4(float x, float y, float z, float w)
        {
            this.dxVector4 = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Initializes a new instance of the TGCVector4 class.
        /// </summary>
        /// <param name="dxVector4">Vector4 from value.</param>
        public TGCVector4(Vector4 dxVector4)
        {
            this.dxVector4 = dxVector4;
        }

        /// <summary>
        /// Retrieves a 4-D vector (0,0,0,0).
        /// </summary>
        public static TGCVector4 Empty
        {
            get { return new TGCVector4(0f, 0f, 0f, 0f); }
        }

        /// <summary>
        /// Retrieves or sets the x component of a 4-D vector.
        /// </summary>
        public float X
        {
            get { return this.dxVector4.X; }
            set { this.dxVector4.X = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Y
        {
            get { return this.dxVector4.Y; }
            set { this.dxVector4.Y = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float Z
        {
            get { return this.dxVector4.Z; }
            set { this.dxVector4.Z = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 4-D vector.
        /// </summary>
        public float W
        {
            get { return this.dxVector4.W; }
            set { this.dxVector4.W = value; }
        }

        /// <summary>
        /// Adds two 4-D vectors.
        /// </summary>
        /// <param name="source">source</param>
        public void Add(TGCVector4 source)
        {
            this.dxVector4.Add(source.ToVector4());
        }

        /// <summary>
        /// Adds two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4.</param>
        /// <param name="right">Source TGCVector4.</param>
        /// <returns>Sum of the two TGCVector4 structures.</returns>
        public static TGCVector4 Add(TGCVector4 left, TGCVector4 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a point in barycentric coordinates, using the specified 4-D vectors.
        /// </summary>
        /// <param name="v1">Source TGCVector4 structure.</param>
        /// <param name="v2">Source TGCVector4 structure.</param>
        /// <param name="v3">Source TGCVector4 structure.</param>
        /// <param name="f">Weighting factor.</param>
        /// <param name="g">Weighting factor.</param>
        /// <returns>A TGCVector4 structure in barycentric coordinates.</returns>
        public static TGCVector4 BaryCentric(TGCVector4 v1, TGCVector4 v2, TGCVector4 v3, float f, float g)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using specified 4-D vectors.
        /// </summary>
        /// <param name="position1">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="position2">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="position3">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="position4">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector4 structure that is the result of the Catmull-Rom interpolation.</returns>
        public static TGCVector4 CatmullRom(TGCVector4 position1, TGCVector4 position2, TGCVector4 position3, TGCVector4 position4, float weightingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the cross product in four dimensions.
        /// </summary>
        /// <param name="v1">Source TGCVector4 structure.</param>
        /// <param name="v2">Source TGCVector4 structure.</param>
        /// <param name="v3">Source TGCVector4 structure.</param>
        /// <returns>A TGCVector4 structure that is the cross product.</returns>
        public static TGCVector4 Cross(TGCVector4 v1, TGCVector4 v2, TGCVector4 v3)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the dot product of two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4 structure.</param>
        /// <param name="right">Source TGCVector4 structure.</param>
        /// <returns>A Single value that represents the dot product.</returns>
        public static float Dot(TGCVector4 left, TGCVector4 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
		/// Returns a value that indicates whether the current instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">Object with which to make the comparison.</param>
		/// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
		public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TGCVector4))
                return false;
            else
                return this == ((TGCVector4)obj);
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return this.dxVector4.GetHashCode();
        }

        /// <summary>
        /// Performs a Hermite spline interpolation using the specified 4-D vectors.
        /// </summary>
        /// <param name="position">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="tangent">Source TGCVector4 structure that is a tangent vector.</param>
        /// <param name="position2">Source TGCVector4 structure that is a position vector.</param>
        /// <param name="tangent2">Source TGCVector4 structure that is a tangent vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector4 structure that is the result of the Hermite spline interpolation.</returns>
        public static TGCVector4 Hermite(TGCVector4 position, TGCVector4 tangent, TGCVector4 position2, TGCVector4 tangent2, float weightingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the length of a 4-D vector.
        /// </summary>
        /// <returns>A Single value that indicates the vector's length.</returns>
        public float Length()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the length of a 4-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <returns>A Single value that indicates the vector's length.</returns>
        public static float Length(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the square of the length of a 4-D vector.
        /// </summary>
        /// <returns>A Single value that indicates the vector's squared length.</returns>
        public float LengthSq()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the square of the length of a 4-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <returns>A Single value that indicates the vector's squared length.</returns>
        public static float LengthSq(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs a linear interpolation between two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4 structure.</param>
        /// <param name="right">Source TGCVector4 structure.</param>
        /// <param name="interpolater"></param>
        /// <returns>A TGCVector4 structure that is the result of the linear interpolation.</returns>
        public static TGCVector4 Lerp(TGCVector4 left, TGCVector4 right, float interpolater)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 4-D vector that is made up of the largest components of two 4-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        public void Maximize(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 4-D vector that is made up of the largest components of two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4 structure.</param>
        /// <param name="right">Source TGCVector4 structure.</param>
        /// <returns>A TGCVector4 structure that is made up of the largest components of the two vectors.</returns>
        public static TGCVector4 Maximize(TGCVector4 left, TGCVector4 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 4-D vector that is made up of the smallest components of two 4-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        public void Minimize(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a 4-D vector that is made up of the smallest components of two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4 structure.</param>
        /// <param name="right">Source TGCVector4 structure.</param>
        /// <returns>A TGCVector4 structure that is made up of the smallest components of the two vectors.</returns>
        public static TGCVector4 Minimize(TGCVector4 left, TGCVector4 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiplies a 4-D vector by a Single value.
        /// </summary>
        /// <param name="s">Source Single value used as a multiplier.</param>
        public void Multiply(float s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiplies a 4-D vector by a Single value.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <param name="f">Source Single value used as a multiplier.</param>
        /// <returns>A TGCVector4 structure that is multiplied by the Single value.</returns>
        public static TGCVector4 Multiply(TGCVector4 source, float f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the normalized version of a 4-D vector.
        /// </summary>
        public void Normalize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the normalized version of a 4-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <returns>A TGCVector4 structure that is the normalized version of the specified vector.</returns>
        public static TGCVector4 Normalize(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4.</param>
        /// <param name="right">Source TGCVector4.</param>
        /// <returns>A TGCVector4 structure that contains the sum of the parameters.</returns>
        public static TGCVector4 operator +(TGCVector4 left, TGCVector4 right)
        {
            return new TGCVector4(left.ToVector4() + right.ToVector4());
        }

        /// <summary>
		/// Compares the current instance of a class to another instance to determine whether they are the same.
		/// </summary>
		/// <param name="left">The TGCVector2 to the left of the equality operator.</param>
		/// <param name="right">The TGCVector2 to the right of the equality operator.</param>
		/// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
		public static bool operator ==(TGCVector4 left, TGCVector4 right)
        {
            return left.ToVector4() == right.ToVector4();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCVector2 to the left of the inequality operator.</param>
        /// <param name="right">The TGCVector2 to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCVector4 left, TGCVector4 right)
        {
            return left.ToVector4() != right.ToVector4();
        }

        /// <summary>
        /// Determines the product of a single value and a 4-D vector.
        /// </summary>
        /// <param name="right">Source Single structure.</param>
        /// <param name="left">Source TGCVector4.</param>
        /// <returns>A TGCVector4 structure that is the product of the right and left parameters.</returns>
        public static TGCVector4 operator *(float right, TGCVector4 left)
        {
            return new TGCVector4(left.ToVector4() * right);
        }

        /// <summary>
        /// Determines the product of a single value and a 4-D vector.
        /// </summary>
        /// <param name="left">Source TGCVector4.</param>
        /// <param name="right">Source Single structure.</param>
        /// <returns>A TGCVector4 structure that is the product of the right and left parameters.</returns>
        public static TGCVector4 operator *(TGCVector4 left, float right)
        {
            return new TGCVector4(left.ToVector4() * right);
        }

        /// <summary>
        /// Subtracts two 4-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector4 to the left of the subtraction operator.</param>
        /// <param name="right">The TGCVector4 to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCVector4 structure.</returns>
        public static TGCVector4 operator -(TGCVector4 left, TGCVector4 right)
        {
            return new TGCVector4(left.ToVector4() - right.ToVector4());
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="vec">Source TGCVector4.</param>
        /// <returns>The TGCVector4 structure that is the result of the operation.</returns>
        public static TGCVector4 operator -(TGCVector4 vec)
        {
            return new TGCVector4(-vec.ToVector4());
        }

        /// <summary>
        /// Scales a 4-D vector.
        /// </summary>
        /// <param name="scalingFactor">Scaling value.</param>
        public void Scale(float scalingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scales a 4-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <param name="scalingFactor">Scaling value.</param>
        /// <returns>A TGCVector4 structure that is the scaled vector.</returns>
        public static TGCVector4 Scale(TGCVector4 source, float scalingFactor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two 4-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure to subtract from the current instance.</param>
        public void Subtract(TGCVector4 source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two 4-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector4 structure to the left of the subtraction operator.</param>
        /// <param name="right">Source TGCVector4 structure to the right of the subtraction operator.</param>
        /// <returns>A TGCVector4 structure that is the result of the operation.</returns>
        public static TGCVector4 Subtract(TGCVector4 left, TGCVector4 right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return this.dxVector4.ToString();
        }

        /// <summary>
        /// Transforms a 4-D vector or an array of 4-D vectors by a given matrix.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix structure.</param>
        public void Transform(TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 4-D vector or an array of 4-D vectors by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector4 structure.</param>
        /// <param name="sourceMatrix">Source TGCMatrix structure.</param>
        /// <returns>A TGCVector4 structure that is the result of the method.</returns>
        public static TGCVector4 Transform(TGCVector4 source, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transforms a 4-D vector or an array of 4-D vectors by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector4 structures.</param>
        /// <param name="sourceMatrix">Source TGCMatrix structure.</param>
        /// <returns></returns>
        public static TGCVector4[] Transform(TGCVector4[] vector, TGCMatrix sourceMatrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the DirectX of a 4-D vector
        /// </summary>
        /// <returns></returns>
        private Vector4 ToVector4()
        {
            return dxVector4;
        }

        /// <summary>
        /// Cast TGCVector4 to DX Vector4
        /// </summary>
        /// <param name="vector">TGCVector4 to become into Vector4</param>
        public static implicit operator Vector4(TGCVector4 vector)
        {
            return vector.ToVector4();
        }

        #region Old TGCVectorUtils

        /// <summary>
        /// Convierte un color base(255,255,255,255) a un Vector4(1f,1f,1f,1f).
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Vector4 ColorToVector4(Color color)
        {
            return Vector4.Normalize(new Vector4(color.R, color.G, color.B, color.A));
        }

        public static Vector4[] TGCVector4ArrayToVector4Array(TGCVector4[] vector)
        {
            Vector4[] ret = new Vector4[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = vector[i].ToVector4();
            }
            return ret;
        }

        #endregion Old TGCVectorUtils
    }
}