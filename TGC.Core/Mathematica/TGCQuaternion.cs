using Microsoft.DirectX;
using System;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes a quaternion.
    /// </summary>
    [Serializable]
    public struct TGCQuaternion
    {
        /// <summary>
        /// Retrieves or sets the DirectX of quaternion.
        /// </summary>
        private Quaternion dxQuaternion;

        /// <summary>
        /// Initializes a new instance of the Quaternion class.
        /// </summary>
        /// <param name="valueX">Initial value for the Quaternion.X member.</param>
        /// <param name="valueY">Initial value for the Quaternion.Y member.</param>
        /// <param name="valueZ">Initial value for the Quaternion.Z member.</param>
        /// <param name="valueW">Initial value for the Quaternion.W member.</param>
        public TGCQuaternion(float valueX, float valueY, float valueZ, float valueW)
        {
            this.dxQuaternion = new Quaternion(valueX, valueY, valueZ, valueW);
        }

        /// <summary>
        /// Initializes a new instance of the TGCVector2 class.
        /// </summary>
        /// <param name="quaternion">Vector2 from value.</param>
        public TGCQuaternion(Quaternion quaternion) : this()
        {
            this.dxQuaternion = quaternion;
        }

        /// <summary>
        /// Retrieves or sets the w component of the quaternion..
        /// </summary>
        public float W
        {
            get { return this.dxQuaternion.W; }
            set { this.dxQuaternion.W = value; }
        }

        /// <summary>
        /// Retrieves or sets the x component of the quaternion.
        /// </summary>
        public float X
        {
            get { return this.dxQuaternion.X; }
            set { this.dxQuaternion.X = value; }
        }

        /// <summary>
        /// Retrieves or sets the y component of the quaternion.
        /// </summary>
        public float Y
        {
            get { return this.dxQuaternion.Y; }
            set { this.dxQuaternion.Y = value; }
        }

        /// <summary>
        /// Retrieves or sets the z component of the quaternion.
        /// </summary>
        public float Z
        {
            get { return this.dxQuaternion.Z; }
            set { this.dxQuaternion.Z = value; }
        }

        /// <summary>
        /// Retrieves the identity quaternion.
        /// </summary>
        public static TGCQuaternion Identity
        {
            get { return new TGCQuaternion(Quaternion.Identity); }
        }

        /// <summary>
        /// Retrieves an empty quaternion.
        /// </summary>
        public static TGCQuaternion Zero
        {
            get { return new TGCQuaternion(0f, 0f, 0f, 0f); }
        }

        /// <summary>
        /// Adds two quaternions.
        /// </summary>
        /// <param name="m1">Source Quaternion.</param>
        /// <param name="m2">Source Quaternion.</param>
        /// <returns>Sum of the two source Quaternion.</returns>
        public static TGCQuaternion Add(TGCQuaternion m1, TGCQuaternion m2)
        {
            return new TGCQuaternion(Quaternion.Add(m1.ToQuaternion(), m2.ToQuaternion()));
        }

        /// <summary>
        /// Returns a quaternion in barycentric coordinates.
        /// </summary>
        /// <param name="q1">Source Quaternion.</param>
        /// <param name="q2">Source Quaternion.</param>
        /// <param name="q3">Source Quaternion.</param>
        /// <param name="f">Weighting factor.</param>
        /// <param name="g">Weighting factor.</param>
        /// <returns>Resulting Quaternion in barycentric coordinates.</returns>
        public static TGCQuaternion BaryCentric(TGCQuaternion q1, TGCQuaternion q2, TGCQuaternion q3, float f, float g)
        {
            return new TGCQuaternion(Quaternion.BaryCentric(q1.ToQuaternion(), q2.ToQuaternion(), q3.ToQuaternion(), f, g));
        }

        /// <summary>
        /// Returns the conjugate of a quaternion.
        /// </summary>
        /// <param name="q">Returns the conjugate of a quaternion.</param>
        /// <returns>A Quaternion that is the conjugate of the q parameter.</returns>
        public static TGCQuaternion Conjugate(TGCQuaternion q)
        {
            return new TGCQuaternion(Quaternion.Conjugate(q.ToQuaternion()));
        }

        /// <summary>
        /// Returns the dot product of two quaternions.
        /// </summary>
        /// <param name="v1">Source Quaternion.</param>
        /// <param name="v2">Source Quaternion.</param>
        /// <returns>Dot product of two quaternions.</returns>
        public static float Dot(TGCQuaternion v1, TGCQuaternion v2)
        {
            return Quaternion.Dot(v1.ToQuaternion(), v2.ToQuaternion());
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TGCQuaternion))
                return false;
            else
                return this == ((TGCQuaternion)obj);
        }

        /// <summary>
        /// Calculates the exponential.
        /// </summary>
        public void Exp()
        {
            this.dxQuaternion.Exp();
        }

        /// <summary>
        /// Calculates the exponential.
        /// </summary>
        /// <param name="q">Source Quaternion.</param>
        /// <returns>The Quaternion that is the exponential of the q parameter.</returns>
        public static TGCQuaternion Exp(TGCQuaternion q)
        {
            return new TGCQuaternion(Quaternion.Exp(q.ToQuaternion()));
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return this.dxQuaternion.GetHashCode();
        }

        /// <summary>
        /// Conjugates and re-normalizes a quaternion.
        /// </summary>
        public void Invert()
        {
            this.dxQuaternion.Invert();
        }

        /// <summary>
        /// Conjugates and re-normalizes a quaternion.
        /// </summary>
        /// <param name="q">Source Quaternion.</param>
        /// <returns>A Quaternion that is the inverse of the quaternion passed into the q parameter.</returns>
        public static TGCQuaternion Invert(TGCQuaternion q)
        {
            return new TGCQuaternion(Quaternion.Invert(q.ToQuaternion()));
        }

        /// <summary>
        /// Returns the length of a quaternion.
        /// </summary>
        /// <returns>A float value that represents the quaternion's length.</returns>
        public float Length()
        {
            return this.dxQuaternion.Length();
        }

        /// <summary>
        /// Returns the length of a quaternion.
        /// </summary>
        /// <param name="v">Source Quaternion.</param>
        /// <returns>A float that represents the quaternion's length.</returns>
        public static float Length(TGCQuaternion v)
        {
            return Quaternion.Length(v.ToQuaternion());
        }

        /// <summary>
        /// Returns the square of a quaternion's length.
        /// </summary>
        /// <returns>A float that represents the quaternion's squared length.</returns>
        public float LengthSq()
        {
            return this.dxQuaternion.LengthSq();
        }

        /// <summary>
        /// Returns the square of a quaternion's length.
        /// </summary>
        /// <param name="v">Source Quaternion.</param>
        /// <returns>A float that represents the quaternion's squared length.</returns>
        public static float LengthSq(TGCQuaternion v)
        {
            return Quaternion.LengthSq(v.ToQuaternion());
        }

        /// <summary>
        /// Calculates the natural logarithm.
        /// </summary>
        public void Ln()
        {
            this.dxQuaternion.Ln();
        }

        /// <summary>
        /// Calculates the natural logarithm.
        /// </summary>
        /// <param name="q">Source Quaternion.</param>
        /// <returns>A Quaternion that is the natural logarithm of the quaternion passed into the q parameter.</returns>
        public static TGCQuaternion Ln(TGCQuaternion q)
        {
            return new TGCQuaternion(Quaternion.Ln(q.ToQuaternion()));
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="q">Source Quaternion.</param>
        public void Multiply(TGCQuaternion q)
        {
            this.dxQuaternion.Multiply(q.ToQuaternion());
        }

        /// <summary>
        /// Multiplies two quaternions.
        /// </summary>
        /// <param name="m1">Source Quaternion.</param>
        /// <param name="m2">Source Quaternion.</param>
        /// <returns>A Quaternion that is the product of two quaternions.</returns>
        public static TGCQuaternion Multiply(TGCQuaternion m1, TGCQuaternion m2)
        {
            return new TGCQuaternion(Quaternion.Multiply(m1.ToQuaternion(), m2.ToQuaternion()));
        }

        /// <summary>
        /// Returns the normal of a quaternion.
        /// </summary>
        public void Normalize()
        {
            this.dxQuaternion.Normalize();
        }

        /// <summary>
        /// Returns the normal of a quaternion.
        /// </summary>
        /// <param name="q">Source Quaternion.</param>
        /// <returns>A Quaternion that is the normal of the quaternion.</returns>
        public static TGCQuaternion Normalize(TGCQuaternion q)
        {
            return new TGCQuaternion(Quaternion.Normalize(q.ToQuaternion()));
        }

        /// <summary>
        /// Cast TGCQuaternion to DX Quaternion
        /// </summary>
        /// <param name="quaternion">TGCQuaternion to become into Quaternion</param>
        public static implicit operator Quaternion(TGCQuaternion quaternion)
        {
            return quaternion.ToQuaternion();
        }

        /// <summary>
        /// Adds two quaternions.
        /// </summary>
        /// <param name="left">Source TGCQuaternion.</param>
        /// <param name="right">Source TGCQuaternion.</param>
        /// <returns>A TGCQuaternion structure that contains the sum of the parameters.</returns>
        public static TGCQuaternion operator +(TGCQuaternion left, TGCQuaternion right)
        {
            return new TGCQuaternion(left.ToQuaternion() + right.ToQuaternion());
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCQuaternion to the left of the equality operator.</param>
        /// <param name="right">The TGCQuaternion to the right of the equality operator.</param>
        /// <returns>Compares the current instance of a class to another instance to determine whether they are the same.</returns>
        public static bool operator ==(TGCQuaternion left, TGCQuaternion right)
        {
            return left.ToQuaternion() == right.ToQuaternion();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCQuaternion to the left of the inequality operator.</param>
        /// <param name="right">The TGCQuaternion to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCQuaternion left, TGCQuaternion right)
        {
            return left.ToQuaternion() != right.ToQuaternion();
        }

        /// <summary>
        /// Determines the product of two quaternions.
        /// </summary>
        /// <param name="left">Source TGCQuaternion.</param>
        /// <param name="right">Source TGCQuaternion.</param>
        /// <returns>A TGCQuaternion structure that is the product of the right and left parameters.</returns>
        public static TGCQuaternion operator *(TGCQuaternion left, TGCQuaternion right)
        {
            return new TGCQuaternion(left.ToQuaternion() * right.ToQuaternion());
        }

        /// <summary>
        /// Subtracts two quaternions.
        /// </summary>
        /// <param name="left">The TGCQuaternion structure to the left of the subtraction operator.</param>
        /// <param name="right">The TGCQuaternion structure to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCQuaternion structure.</returns>
        public static TGCQuaternion operator -(TGCQuaternion left, TGCQuaternion right)
        {
            return new TGCQuaternion(left.ToQuaternion() - right.ToQuaternion());
        }

        /// <summary>
        /// Rotates a quaternion around an arbitrary axis.
        /// </summary>
        /// <param name="v">A TGCVector3 that identifies the axis about which to rotate the quaternion.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateAxis(TGCVector3 v, float angle)
        {
            this.dxQuaternion.RotateAxis(v, angle);
        }

        /// <summary>
        /// Builds a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="m">Source Matrix that defines the rotation.</param>
        public void RotateMatrix(TGCMatrix m)
        {
            this.dxQuaternion.RotateMatrix(m);
        }

        /// <summary>
        /// Builds a quaternion with the given yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        public void RotateYawPitchRoll(float yaw, float pitch, float roll)
        {
            this.dxQuaternion.RotateYawPitchRoll(yaw, pitch, roll);
        }

        /// <summary>
        /// Builds a quaternion that is rotated around an arbitrary axis.
        /// </summary>
        /// <param name="v">A Vector3 that identifies the axis about which to rotate the quaternion.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>A Quaternion that is rotated around the specified axis.</returns>
        public static TGCQuaternion RotationAxis(TGCVector3 v, float angle)
        {
            return new TGCQuaternion(Quaternion.RotationAxis(v, angle));
        }

        /// <summary>
        /// Builds a quaternion from a rotation matrix.
        /// </summary>
        /// <param name="m">Source Matrix structure that defines the rotation.</param>
        /// <returns>A Quaternion structure built from a rotation matrix.</returns>
        public static TGCQuaternion RotationMatrix(TGCMatrix m)
        {
            return new TGCQuaternion(Quaternion.RotationMatrix(m));
        }

        /// <summary>
        /// Builds a quaternion with the given yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <returns>A Quaternion structure with the specified yaw, pitch, and roll.</returns>
        public static TGCQuaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            return new TGCQuaternion(Quaternion.RotationYawPitchRoll(yaw, pitch, roll));
        }

        /// <summary>
        /// Interpolates between two quaternions, using spherical linear interpolation.
        /// </summary>
        /// <param name="q1">Source TGCQuaternion structure.</param>
        /// <param name="q2">Source TGCQuaternion structure.</param>
        /// <param name="t">A float value that indicates how far to interpolate between the quaternions.</param>
        /// <returns>A TGCQuaternion structure that is the result of the interpolation.</returns>
        public static TGCQuaternion Slerp(TGCQuaternion q1, TGCQuaternion q2, float t)
        {
            return new TGCQuaternion(Quaternion.Slerp(q1.ToQuaternion(), q2.ToQuaternion(), t));
        }

        /// <summary>
        /// Interpolates between quaternions, using spherical quadrangle interpolation.
        /// </summary>
        /// <param name="q1">Source TGCQuaternion structure.</param>
        /// <param name="a">Source TGCQuaternion structure.</param>
        /// <param name="b">Source TGCQuaternion structure.</param>
        /// <param name="c">Source TGCQuaternion structure.</param>
        /// <param name="t">A float value that indicates how far to interpolate between the quaternions.</param>
        /// <returns></returns>
        public static TGCQuaternion Squad(TGCQuaternion q1, TGCQuaternion a, TGCQuaternion b, TGCQuaternion c, float t)
        {
            return new TGCQuaternion(Quaternion.Squad(q1.ToQuaternion(), a.ToQuaternion(), b.ToQuaternion(), c.ToQuaternion(), t));
        }

        /// <summary>
        /// Sets up control points for spherical quadrangle interpolation.
        /// </summary>
        /// <param name="outA">A TGCQuaternion instance that represents the outA value.</param>
        /// <param name="outB">A TGCQuaternion instance that represents the outB value.</param>
        /// <param name="outC">A TGCQuaternion instance that represents the outC value.</param>
        /// <param name="q0">A TGCQuaternion instance that represents the q0 input control point.</param>
        /// <param name="q1">A TGCQuaternion instance that represents the q1 input control point.</param>
        /// <param name="q2">A TGCQuaternion instance that represents the q2 input control point.</param>
        /// <param name="q3">A TGCQuaternion instance that represents the q3 input control point.</param>
        public static void SquadSetup(ref TGCQuaternion outA, ref TGCQuaternion outB, ref TGCQuaternion outC, TGCQuaternion q0, TGCQuaternion q1, TGCQuaternion q2, TGCQuaternion q3)
        {
            Quaternion tempA = outA;
            Quaternion tempB = outB;
            Quaternion tempC = outC;
            Quaternion.SquadSetup(ref tempA, ref tempA, ref tempA, q0, q1, q2, q3);
            outA = new TGCQuaternion(tempA);
            outB = new TGCQuaternion(tempB);
            outC = new TGCQuaternion(tempC);
        }

        /// <summary>
        /// Subtracts two quaternion instances.
        /// </summary>
        /// <param name="m1">Source Quaternion structure to the left of the subtraction operator.</param>
        /// <param name="m2">Source Quaternion structure to the right of the subtraction operator.</param>
        /// <returns>A Quaternion structure that is the result of the operation.</returns>
        public static TGCQuaternion Subtract(TGCQuaternion m1, TGCQuaternion m2)
        {
            return new TGCQuaternion(Quaternion.Subtract(m1.ToQuaternion(), m1.ToQuaternion()));
        }

        /// <summary>
        /// Computes a quaternion's axis and angle of rotation.
        /// </summary>
        /// <param name="q">Source TGCQuaternion structure. See Remarks.</param>
        /// <param name="axis">A TGCVector3 structure that identifies the quaternion's axis of rotation.</param>
        /// <param name="angle">A float value that identifies the quaternion's angle of rotation, in radians.</param>
        public static void ToAxisAngle(TGCQuaternion q, ref TGCVector3 axis, ref float angle)
        {
            //TODO no deberia depender de Vector3
            Vector3 tempAxis = axis;
            Quaternion.ToAxisAngle(q.ToQuaternion(), ref tempAxis, ref angle);
            axis = new TGCVector3(tempAxis);
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return this.dxQuaternion.ToString();
        }

        /// <summary>
        /// Get the DX Quaternion wrapped to be use in DX primitives.
        /// </summary>
        /// <returns>The DX Vector3 wrapped</returns>
        private Quaternion ToQuaternion()
        {
            return this.dxQuaternion;
        }

        #region Old TGCVectorUtils && TGCParserUtils

        /// <summary>
        ///     Convierte un float[4] a un TGCQuaternion
        /// </summary>
        public static TGCQuaternion Float4ArrayToTGCQuaternion(float[] f)
        {
            return new TGCQuaternion(f[0], f[1], f[2], f[3]);
        }

        /// <summary>
        ///     Convierte un TGCQuaternion a un float[4]
        /// </summary>
        public static float[] TGCQuaternionToFloat4Array(TGCQuaternion q)
        {
            return new[] { q.X, q.Y, q.Z, q.W };
        }

        #endregion Old TGCVectorUtils && TGCParserUtils
    }
}