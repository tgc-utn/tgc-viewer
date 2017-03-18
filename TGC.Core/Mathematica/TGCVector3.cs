using Microsoft.DirectX;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a vector in three-dimensional (3-D) space.
    /// </summary>
    public struct TGCVector3
    {
        private static TGCVector3 ZERO = new TGCVector3(0f, 0f, 0f);
        private static TGCVector3 ONE = new TGCVector3(1f, 1f, 1f);
        private static TGCVector3 UP = new TGCVector3(0f, 1f, 0f);
        private static TGCVector3 DOWN = new TGCVector3(0f, -1f, 0f);

        /// <summary>
        /// Retrieves or sets the DirectX of a 3-D vector.
        /// </summary>
        private Vector3 dxVector3;

        /// <summary>
        /// Initializes a new instance of the TGCVector3 class.
        /// </summary>
        /// <param name="x">Initial X value.</param>
        /// <param name="y">Initial Y value.</param>
        /// <param name="z">Initial Z value.</param>
        public TGCVector3(float x, float y, float z)
        {
            this.dxVector3 = new Vector3(x, y, z);
        }

        /// <summary>
        /// Initializes a new instance of the TGCVector3 class.
        /// </summary>
        /// <param name="dxVector3">Vector3 from value.</param>
        public TGCVector3(Vector3 dxVector3)
        {
            this.dxVector3 = dxVector3;
        }

        /// <summary>
        /// Retrieves or sets the x component of a 3-D vector.
        /// </summary>
        public float X
        {
            get { return this.dxVector3.X; }
            set { this.dxVector3.X = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of a 3-D vector.
        /// </summary>
        public float Y
        {
            get { return this.dxVector3.Y; }
            set { this.dxVector3.Y = value; }
        }

        /// <summary>
        /// Retrieves or sets the z component of a 3-D vector.
        /// </summary>
        public float Z
        {
            get { return this.dxVector3.Z; }
            set { this.dxVector3.Z = value; }
        }

        /// <summary>
        /// Retrieves a 3-D vector (0,0,0).
        /// </summary>
        public static TGCVector3 Empty
        {
            get { return ZERO; }
        }

        /// <summary>
        /// Retrieves a 3-D vector (1,1,1).
        /// </summary>
        public static TGCVector3 One
        {
            get { return ONE; }
        }

        /// <summary>
        /// Retrieves a 3-D vector (0,1,0).
        /// </summary>
        public static TGCVector3 Up
        {
            get
            { return UP; }
        }

        /// <summary>
        /// Retrieves a 3-D vector (0,-1,0).
        /// </summary>
        public static TGCVector3 Down
        {
            get { return DOWN; }
        }

        /// <summary>
        /// Adds two 3-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        public void Add(TGCVector3 source)
        {
            this.dxVector3.Add(source.ToVector3());
        }

        /// <summary>
        /// Adds two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>Sum of the two TGCVector3 structures.</returns>
        public static TGCVector3 Add(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(Vector3.Add(left.ToVector3(), right.ToVector3()));
        }

        /// <summary>
        /// Returns a point in barycentric coordinates, using specified 3-D vectors.
        /// </summary>
        /// <param name="v1">Source TGCVector3.</param>
        /// <param name="v2">Source TGCVector3.</param>
        /// <param name="v3">Source TGCVector3.</param>
        /// <param name="f">Weighting factor.</param>
        /// <param name="g">Weighting factor.</param>
        /// <returns>A TGCVector3 structure in barycentric coordinates.</returns>
        public static TGCVector3 BaryCentric(TGCVector3 v1, TGCVector3 v2, TGCVector3 v3, float f, float g)
        {
            return new TGCVector3(Vector3.BaryCentric(v1.ToVector3(), v2.ToVector3(), v3.ToVector3(), f, g));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using specified 3-D vectors.
        /// </summary>
        /// <param name="position1">Source TGCVector3 that is a position vector.</param>
        /// <param name="position2">Source TGCVector3 that is a position vector.</param>
        /// <param name="position3">Source TGCVector3 that is a position vector.</param>
        /// <param name="position4">Source TGCVector3 that is a position vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector3 structure that is the result of the Catmull-Rom interpolation.</returns>
        public static TGCVector3 CatmullRom(TGCVector3 position1, TGCVector3 position2, TGCVector3 position3, TGCVector3 position4, float weightingFactor)
        {
            return new TGCVector3(Vector3.CatmullRom(position1.ToVector3(), position2.ToVector3(), position3.ToVector3(), position4.ToVector3(), weightingFactor));
        }

        /// <summary>
        /// Determines the cross product of two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>A Vector3 structure that is the cross product of two 3-D vectors.</returns>
        public static TGCVector3 Cross(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(Vector3.Cross(left.ToVector3(), right.ToVector3()));
        }

        /// <summary>
        /// Determines the dot product of two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>A Single value that is the dot product.</returns>
        public static float Dot(TGCVector3 left, TGCVector3 right)
        {
            return Vector3.Dot(left.ToVector3(), right.ToVector3());
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="compare">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object compare)
        {
            return this.dxVector3.Equals(compare);
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return this.dxVector3.GetHashCode();
        }

        /// <summary>
        /// Performs a Hermite spline interpolation using the specified 3-D vectors.
        /// </summary>
        /// <param name="position">Source TGCVector3 that is a position vector.</param>
        /// <param name="tangent">Source TGCVector3 that is a position vector.</param>
        /// <param name="position2">Source TGCVector3 that is a position vector.</param>
        /// <param name="tangent2">Source TGCVector3 that is a position vector.</param>
        /// <param name="weightingFactor">Weighting factor.</param>
        /// <returns>A TGCVector3 structure that is the result of the Hermite spline interpolation.</returns>
        public static TGCVector3 Hermite(TGCVector3 position, TGCVector3 tangent, TGCVector3 position2, TGCVector3 tangent2, float weightingFactor)
        {
            return new TGCVector3(Vector3.Hermite(position.ToVector3(), tangent.ToVector3(), position2.ToVector3(), tangent2.ToVector3(), weightingFactor));
        }

        /// <summary>
        /// Returns the length of a 3-D vector.
        /// </summary>
        /// <returns>A Single value that contains the vector's length.</returns>
        public float Length()
        {
            return dxVector3.Length();
        }

        /// <summary>
        /// Returns the length of a 3-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <returns>Returns the length of a 3-D vector.</returns>
        public static float Length(TGCVector3 source)
        {
            return Vector3.Length(source.ToVector3());
        }

        /// <summary>
        /// Returns the square of the length of a 3-D vector.
        /// </summary>
        /// <returns>A Single value that contains the vector's squared length.</returns>
        public float LengthSq()
        {
            return dxVector3.LengthSq();
        }

        /// <summary>
        /// Returns the square of the length of a 3-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <returns>A Single value that contains the vector's squared length.</returns>
        public static float LengthSq(TGCVector3 source)
        {
            return Vector3.LengthSq(source.ToVector3());
        }

        /// <summary>
        /// Performs a linear interpolation between two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <param name="interpolater">Parameter that linearly interpolates between the vectors.</param>
        /// <returns>A TGCVector3 structure that is the result of the linear interpolation.</returns>
        public static TGCVector3 Lerp(TGCVector3 left, TGCVector3 right, float interpolater)
        {
            return new TGCVector3(Vector3.Lerp(left.ToVector3(), right.ToVector3(), interpolater));
        }

        /// <summary>
        /// Returns a 3-D vector that is made up of the largest components of two 3-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        public void Maximize(TGCVector3 source)
        {
            this.dxVector3.Maximize(source.ToVector3());
        }

        /// <summary>
        /// Returns a 3-D vector that is made up of the largest components of two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>A Vector3 structure that is made up of the largest components of the two vectors.</returns>
        public static TGCVector3 Maximize(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(Vector3.Maximize(left.ToVector3(), right.ToVector3()));
        }

        /// <summary>
        /// Returns a 3-D vector that is made up of the smallest components of two 3-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        public void Minimize(TGCVector3 source)
        {
            this.dxVector3.Minimize(source.ToVector3());
        }

        /// <summary>
        /// Returns a 3-D vector that is made up of the smallest components of two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>A Vector3 structure that is made up of the smallest components of the two vectors.</returns>
        public static TGCVector3 Minimize(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(Vector3.Minimize(left.ToVector3(), right.ToVector3()));
        }

        /// <summary>
        /// Multiplies a 3-D vector by a Single value.
        /// </summary>
        /// <param name="s">Source Single value used as a multiplier.</param>
        public void Multiply(float s)
        {
            this.dxVector3.Multiply(s);
        }

        /// <summary>
        /// Multiplies a 3-D vector by a Single value.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <param name="f">Source Single value used as a multiplier.</param>
        /// <returns>A Vector3 structure that is multiplied by the Single value.</returns>
        public static TGCVector3 Multiply(TGCVector3 source, float f)
        {
            return new TGCVector3(Vector3.Multiply(source.ToVector3(), f));
        }

        /// <summary>
        /// Returns the normalized version of a 3-D vector.
        /// </summary>
        public void Normalize()
        {
            this.dxVector3.Normalize();
        }

        /// <summary>
        /// Returns the normalized version of a 3-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <returns>A TGCVector3 structure that is the normalized version of the specified vector.</returns>
        public static TGCVector3 Normalize(TGCVector3 source)
        {
            return new TGCVector3(Vector3.Normalize(source.ToVector3()));
        }

        /// <summary>
        /// Cast TGCVector3 to DX Vector3
        /// </summary>
        /// <param name="vector">TGCVector3 to become into Vector3</param>
        public static implicit operator Vector3(TGCVector3 vector)
        {
            return vector.ToVector3();
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source TGCVector3.</param>
        /// <returns>A TGCVector3 structure that contains the sum of the parameters.</returns>
        public static TGCVector3 operator +(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(left.ToVector3() + right.ToVector3());
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCVector3 to the left of the equality operator.</param>
        /// <param name="right">The TGCVector3 to the right of the equality operator.</param>
        /// <returns>Compares the current instance of a class to another instance to determine whether they are the same.</returns>
        public static bool operator ==(TGCVector3 left, TGCVector3 right)
        {
            return left.ToVector3() == right.ToVector3();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCVector3 to the left of the inequality operator.</param>
        /// <param name="right">The TGCVector3 to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCVector3 left, TGCVector3 right)
        {
            return left.ToVector3() != right.ToVector3();
        }

        /// <summary>
        /// Determines the product of a single value and a 3-D vector.
        /// </summary>
        /// <param name="right">Source Single structure.</param>
        /// <param name="left">Source TGCVector3.</param>
        /// <returns>A TGCVector3 structure that is the product of the right and left parameters.</returns>
        public static TGCVector3 operator *(float right, TGCVector3 left)
        {
            return new TGCVector3(left.ToVector3() * right);
        }

        /// <summary>
        /// Determines the product of a single value and a 3-D vector.
        /// </summary>
        /// <param name="left">Source TGCVector3.</param>
        /// <param name="right">Source Single structure.</param>
        /// <returns>A TGCVector3 structure that is the product of the right and left parameters.</returns>
        public static TGCVector3 operator *(TGCVector3 left, float right)
        {
            return new TGCVector3(left.ToVector3() * right);
        }

        /// <summary>
        /// Subtracts two 3-D vectors.
        /// </summary>
        /// <param name="left">The TGCVector3 to the left of the subtraction operator.</param>
        /// <param name="right">The TGCVector3 to the right of the subtraction operator.</param>
        /// <returns>Resulting Vector3 structure.</returns>
        public static TGCVector3 operator -(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(left.ToVector3() - right.ToVector3());
        }

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="vec">Source TGCVector3.</param>
        /// <returns>The Vector3 structure that is the result of the operation.</returns>
        public static TGCVector3 operator -(TGCVector3 vec)
        {
            return new TGCVector3(-vec.ToVector3());
        }

        /// <summary>
        /// Projects a vector from object space into screen space.
        /// </summary>
        /// <param name="viewport">An Object structure that represents the viewport. Only Viewport structures are valid for this parameter.</param>
        /// <param name="projection">A TGCMatrix structure that represents the projection matrix.</param>
        /// <param name="view">A TGCMatrix structure that represents the view matrix.</param>
        /// <param name="world">A TGCMatrix structure that represents the world matrix.</param>
        public void Project(object viewport, TGCMatrix projection, TGCMatrix view, TGCMatrix world)
        {
            this.dxVector3.Project(viewport, projection.ToMatrix(), view.ToMatrix(), world.ToMatrix());
        }

        /// <summary>
        /// Projects a vector from object space into screen space.
        /// </summary>
        /// <param name="v">Source TGCVector3.</param>
        /// <param name="viewport">An Object structure that represents the viewport. Only Viewport structures are valid for this parameter.</param>
        /// <param name="projection">A TGCMatrix structure that represents the projection matrix.</param>
        /// <param name="view">A TGCMatrix structure that represents the view matrix.</param>
        /// <param name="world">A TGCMatrix structure that represents the world matrix.</param>
        /// <returns>A TGCVector3 structure that is the vector projected from object space into screen space.</returns>
        public static TGCVector3 Project(TGCVector3 v, object viewport, TGCMatrix projection, TGCMatrix view, TGCMatrix world)
        {
            return new TGCVector3(Vector3.Project(v.ToVector3(), viewport, projection.ToMatrix(), view.ToMatrix(), world.ToMatrix()));
        }

        /// <summary>
        /// Scales a 3-D vector.
        /// </summary>
        /// <param name="scalingFactor">Scaling value.</param>
        public void Scale(float scalingFactor)
        {
            this.dxVector3.Scale(scalingFactor);
        }

        /// <summary>
        /// Scales a 3-D vector.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <param name="scalingFactor">Scaling value.</param>
        /// <returns>A Vector3 structure that is the scaled vector.</returns>
        public static TGCVector3 Scale(TGCVector3 source, float scalingFactor)
        {
            return new TGCVector3(Vector3.Scale(source.ToVector3(), scalingFactor));
        }

        /// <summary>
        /// Subtracts two 3-D vectors.
        /// </summary>
        /// <param name="source">Source TGCVector3 structure to subtract from the current instance.</param>
        public void Subtract(TGCVector3 source)
        {
            this.dxVector3.Subtract(source.ToVector3());
        }

        /// <summary>
        /// Subtracts two 3-D vectors.
        /// </summary>
        /// <param name="left">Source TGCVector3 structure to the left of the subtraction operator.</param>
        /// <param name="right">Source TGCVector3 structure to the right of the subtraction operator.</param>
        /// <returns>A Vector3 structure that is the result of the operation.</returns>
        public static TGCVector3 Subtract(TGCVector3 left, TGCVector3 right)
        {
            return new TGCVector3(Vector3.Subtract(left.ToVector3(), right.ToVector3()));
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return "[" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", X) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", Y) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", X) + "]";
        }

        /// <summary>
        /// Transforms a 3-D vector or an array of 3-D vectors by a given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A Vector4 structure that is the result of the method.</returns>
        public static TGCVector4 Transform(TGCVector3 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector4(Vector3.Transform(source.ToVector3(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms a 3-D vector or an array of 3-D vectors by a given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector3 structures.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of Vector4 structures that are the result of the method.</returns>
        public static TGCVector4[] Transform(TGCVector3[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector4[] ret = new TGCVector4[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector4(Vector3.Transform(vector[i].ToVector3(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Transforms a 3-D vector or an array of 3-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformCoordinate(TGCMatrix sourceMatrix)
        {
            this.dxVector3.TransformCoordinate(sourceMatrix.ToMatrix());
        }

        /// <summary>
        /// Transforms a 3-D vector or an array of 3-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A TGCVector3 structure that represents the results of the method.</returns>
        public static TGCVector3 TransformCoordinate(TGCVector3 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector3(Vector3.TransformCoordinate(source.ToVector3(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms a 3-D vector or an array of 3-D vectors by a given matrix, projecting the result back into w = 1.
        /// </summary>
        /// <param name="vector">Array of source TGCVector3 structures.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of TGCVector3 structures that represent the results of the method.</returns>
        public static TGCVector3[] TransformCoordinate(TGCVector3[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector3[] ret = new TGCVector3[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector3(Vector3.TransformCoordinate(vector[i].ToVector3(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Transforms a 3-D vector normal by the given matrix.
        /// </summary>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        public void TransformNormal(TGCMatrix sourceMatrix)
        {
            this.dxVector3.TransformNormal(sourceMatrix.ToMatrix());
        }

        /// <summary>
        /// Transforms a 3-D vector normal by the given matrix.
        /// </summary>
        /// <param name="source">Source TGCVector3.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>A Vector3 structure that contains the results of this method.</returns>
        public static TGCVector3 TransformNormal(TGCVector3 source, TGCMatrix sourceMatrix)
        {
            return new TGCVector3(Vector3.TransformNormal(source.ToVector3(), sourceMatrix.ToMatrix()));
        }

        /// <summary>
        /// Transforms a 3-D vector normal by the given matrix.
        /// </summary>
        /// <param name="vector">Array of source TGCVector3 structures.</param>
        /// <param name="sourceMatrix">Source TGCMatrix.</param>
        /// <returns>Array of Vector3 structures that contain the results of this method.</returns>
        public static TGCVector3[] TransformNormal(TGCVector3[] vector, TGCMatrix sourceMatrix)
        {
            TGCVector3[] ret = new TGCVector3[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                ret[i] = new TGCVector3(Vector3.TransformNormal(vector[i].ToVector3(), sourceMatrix.ToMatrix()));
            }
            return ret;
        }

        /// <summary>
        /// Projects a vector from screen space into object space.
        /// </summary>
        /// <param name="viewport">An Object structure that represents the viewport. Only Viewport structures are valid for this parameter.</param>
        /// <param name="projection">A TGCMatrix structure that represents the projection matrix.</param>
        /// <param name="view">A TGCMatrix structure that represents the view matrix.</param>
        /// <param name="world">A TGCMatrix structure that represents the world matrix.</param>
        public void Unproject(object viewport, TGCMatrix projection, TGCMatrix view, TGCMatrix world)
        {
            this.dxVector3.Unproject(viewport, projection.ToMatrix(), view.ToMatrix(), world.ToMatrix());
        }

        /// <summary>
        /// Projects a vector from screen space into object space.
        /// </summary>
        /// <param name="v">Source TGCVector3.</param>
        /// <param name="viewport">An Object structure that represents the viewport. Only Viewport structures are valid for this parameter.</param>
        /// <param name="projection">A TGCMatrix structure that represents the projection matrix.</param>
        /// <param name="view">A TGCMatrix structure that represents the view matrix.</param>
        /// <param name="world">A TGCMatrix structure that represents the world matrix.</param>
        /// <returns>A TGCVector3 structure that is the vector projected from screen space into object space.</returns>
        public static TGCVector3 Unproject(TGCVector3 v, object viewport, TGCMatrix projection, TGCMatrix view, TGCMatrix world)
        {
            return new TGCVector3(Vector3.Unproject(v.ToVector3(), viewport, projection.ToMatrix(), view.ToMatrix(), world.ToMatrix()));
        }

        /// <summary>
        /// Get the DX Vector3 wrapped to be use in DX primitives.
        /// </summary>
        /// <returns>The DX Vector3 wrapped</returns>
        private Vector3 ToVector3()
        {
            return this.dxVector3;
        }

        /// <summary>
        /// (DEPRECATED)New TGCVector3 from DX Vector3
        /// </summary>
        /// <param name="vector">Source Vector3.</param>
        /// <returns>Initializes a new instance of the TGCVector3 class.</returns>
        public static TGCVector3 FromVector3(Vector3 vector)
        {
            //TODO DEPRECAR
            return new TGCVector3(vector);
        }

        #region Old TGCVectorUtils

        /// <summary>
        ///     Longitud al cuadrado del segmento ab
        /// </summary>
        /// <param name="a">Punto inicial del segmento</param>
        /// <param name="b">Punto final del segmento</param>
        /// <returns>Longitud al cuadrado</returns>
        public static float LengthSq(TGCVector3 a, TGCVector3 b)
        {
            return TGCVector3.Subtract(a, b).LengthSq();
        }

        /// <summary>
        ///     Multiplicar dos vectores.
        ///     Se multiplica cada componente
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector resultante</returns>
        public static TGCVector3 Mul(TGCVector3 v1, TGCVector3 v2)
        {
            return new TGCVector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        /// <summary>
        ///     Dividir dos vectores.
        ///     Se divide cada componente
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector resultante</returns>
        public static TGCVector3 Div(TGCVector3 v1, TGCVector3 v2)
        {
            return new TGCVector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        /// <summary>
        ///     Multiplicar un TGCVector3 por una Matriz.
        ///     Devuelve un TGCVector3 ignorando W.
        /// </summary>
        /// <param name="v">Vector</param>
        /// <param name="m">Matriz</param>
        /// <returns>Vector resultante, sin W</returns>
        public static TGCVector3 transform(TGCVector3 v, TGCMatrix m)
        {
            //TODO este metodo no deberia volar?
            var t = TGCVector3.Transform(v, m);
            return new TGCVector3(t.X, t.Y, t.Z);
        }

        /// <summary>
        ///     Aplica el valor absoluto a todos los componentes del vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Vector resultante</returns>
        public static TGCVector3 Abs(TGCVector3 v)
        {
            return new TGCVector3(FastMath.Abs(v.X), FastMath.Abs(v.Y), FastMath.Abs(v.Z));
        }

        /// <summary>
        ///     Devuelve el menor valor de los 3 componentes del vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <returns>Menor valor de los tres</returns>
        public static float Min(TGCVector3 v)
        {
            return FastMath.Min(FastMath.Min(v.X, v.Y), v.Z);
        }

        /// <summary>
        ///     Imprime un TGCVector3 de la forma [150.0,150.0,150.0]
        /// </summary>
        public static string PrintVector3(TGCVector3 vec)
        {
            return PrintVector3(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        ///     Imprime un TGCVector3 de la forma [150.0,150.0,150.0], tomando valores string
        /// </summary>
        public static string PrintVector3FromString(string x, string y, string z)
        {
            return PrintVector3(TgcParserUtils.parseFloat(x), TgcParserUtils.parseFloat(y), TgcParserUtils.parseFloat(z));
        }

        /// <summary>
        ///     Convierte un TGCVector3 a un float[3]
        /// </summary>
        public static float[] Vector3ToFloat3Array(TGCVector3 v)
        {
            return new[] { v.X, v.Y, v.Z };
        }

        /// <summary>
        ///     Convierte un TGCVector3 a un float[4] con w = 1
        /// </summary>
        public static float[] Vector3ToFloat4Array(TGCVector3 v)
        {
            return new[] { v.X, v.Y, v.Z, 1f };
        }

        /// <summary>
        ///     Convierte un TGCVector3 a un Vector4 con w = 1
        /// </summary>
        public static Vector4 Vector3ToVector4(TGCVector3 v)
        {
            return new Vector4(v.X, v.Y, v.Z, 1f);
        }

        /// <summary>
        ///     Convierte un float[3] a un TGCVector3
        /// </summary>
        public static TGCVector3 Float3ArrayToVector3(float[] f)
        {
            return new TGCVector3(f[0], f[1], f[2]);
        }

        /// <summary>
        ///     Imprime un TGCVector3 de la forma [150.0,150.0,150.0]
        /// </summary>
        /// <returns></returns>
        public static string PrintVector3(float x, float y, float z)
        {
            return "[" + TgcParserUtils.printFloat(x) +
                   "," + TgcParserUtils.printFloat(y) +
                   "," + TgcParserUtils.printFloat(z) + "]";
        }

        #endregion Old TGCVectorUtils
    }
}