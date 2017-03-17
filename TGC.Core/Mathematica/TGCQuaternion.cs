using Microsoft.DirectX;
using System;
using TGC.Core.Utils;

namespace TGC.Core.Mathematica
{
	/// <summary>
	/// Describes a quaternion.
	/// </summary>
	public class TGCQuaternion
	{
		/// <summary>
		/// Initializes a new instance of the Quaternion class.
		/// </summary>
		public TGCQuaternion()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Initializes a new instance of the Quaternion class.
		/// </summary>
		/// <param name="valueX">Initial value for the Quaternion.X member.</param>
		/// <param name="valueY">Initial value for the Quaternion.Y member.</param>
		/// <param name="valueZ">Initial value for the Quaternion.Z member.</param>
		/// <param name="valueW">Initial value for the Quaternion.W member.</param>
		public TGCQuaternion(float valueX, float valueY, float valueZ, float valueW)
		{
			this.DXQuaternion = new Quaternion(valueX, valueY, valueZ, valueW);
		}

		private Quaternion DXQuaternion;

		/// <summary>
		/// Retrieves or sets the w component of the quaternion..
		/// </summary>
		public float W
		{
			get { return this.DXQuaternion.W; }
			set { this.DXQuaternion.W = value; }
		}

		/// <summary>
		/// Retrieves or sets the x component of the quaternion.
		/// </summary>
		public float X
		{
			get { return this.DXQuaternion.X; }
			set { this.DXQuaternion.X = value; }
		}

		/// <summary>
		/// Retrieves or sets the y component of the quaternion.
		/// </summary>
		public float Y
		{
			get { return this.DXQuaternion.Y; }
			set { this.DXQuaternion.Y = value; }
		}

		/// <summary>
		/// Retrieves or sets the z component of the quaternion.
		/// </summary>
		public float Z
		{
			get { return this.DXQuaternion.Z; }
			set { this.DXQuaternion.Z = value; }
		}

		/// <summary>
		/// Retrieves the identity quaternion.
		/// </summary>
		public static TGCQuaternion Identity { get; }

		/// <summary>
		/// Retrieves an empty quaternion.
		/// </summary>
		public static TGCQuaternion Zero { get; }

		/// <summary>
		/// Adds two quaternions.
		/// </summary>
		/// <param name="m1">Source Quaternion.</param>
		/// <param name="m2">Source Quaternion.</param>
		/// <returns>Sum of the two source Quaternion.</returns>
		public static TGCQuaternion Add(TGCQuaternion m1, TGCQuaternion m2)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the conjugate of a quaternion.
		/// </summary>
		/// <param name="q">Returns the conjugate of a quaternion.</param>
		/// <returns>A Quaternion that is the conjugate of the q parameter.</returns>
		public static TGCQuaternion Conjugate(TGCQuaternion q)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the dot product of two quaternions.
		/// </summary>
		/// <param name="v1">Source Quaternion.</param>
		/// <param name="v2">Source Quaternion.</param>
		/// <returns>Dot product of two quaternions.</returns>
		public static float Dot(TGCQuaternion v1, TGCQuaternion v2)
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
		/// Calculates the exponential.
		/// </summary>
		public void Exp()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calculates the exponential.
		/// </summary>
		/// <param name="q">Source Quaternion.</param>
		/// <returns>The Quaternion that is the exponential of the q parameter.</returns>
		public static TGCQuaternion Exp(TGCQuaternion q)
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
		/// Conjugates and re-normalizes a quaternion.
		/// </summary>
		public void Invert()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Conjugates and re-normalizes a quaternion.
		/// </summary>
		/// <param name="q">Source Quaternion.</param>
		/// <returns>A Quaternion that is the inverse of the quaternion passed into the q parameter.</returns>
		public static TGCQuaternion Invert(TGCQuaternion q)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the length of a quaternion.
		/// </summary>
		/// <returns>A float value that represents the quaternion's length.</returns>
		public float Length()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the length of a quaternion.
		/// </summary>
		/// <param name="v">Source Quaternion.</param>
		/// <returns>A float that represents the quaternion's length.</returns>
		public static float Length(TGCQuaternion v)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the square of a quaternion's length.
		/// </summary>
		/// <returns>A float that represents the quaternion's squared length.</returns>
		public float LengthSq()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the square of a quaternion's length.
		/// </summary>
		/// <param name="v">Source Quaternion.</param>
		/// <returns>A float that represents the quaternion's squared length.</returns>
		public static float LengthSq(TGCQuaternion v)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calculates the natural logarithm.
		/// </summary>
		public void Ln()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calculates the natural logarithm.
		/// </summary>
		/// <param name="q">Source Quaternion.</param>
		/// <returns>A Quaternion that is the natural logarithm of the quaternion passed into the q parameter.</returns>
		public static TGCQuaternion Ln(TGCQuaternion q)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Multiplies two quaternions.
		/// </summary>
		/// <param name="q">Source Quaternion.</param>
		public void Multiply(TGCQuaternion q)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Multiplies two quaternions.
		/// </summary>
		/// <param name="m1">Source Quaternion.</param>
		/// <param name="m2">Source Quaternion.</param>
		/// <returns>A Quaternion that is the product of two quaternions.</returns>
		public static TGCQuaternion Multiply(TGCQuaternion m1, TGCQuaternion m2)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the normal of a quaternion.
		/// </summary>
		public void Normalize()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the normal of a quaternion.
		/// </summary>
		/// <param name="q">Source Quaternion.</param>
		/// <returns>A Quaternion that is the normal of the quaternion.</returns>
		public static TGCQuaternion Normalize(TGCQuaternion q)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds two quaternions.
		/// </summary>
		/// <param name="left">Source TGCQuaternion.</param>
		/// <param name="right">Source TGCQuaternion.</param>
		/// <returns>A TGCQuaternion structure that contains the sum of the parameters.</returns>
		public static TGCQuaternion operator +(TGCQuaternion left, TGCQuaternion right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Compares the current instance of a class to another instance to determine whether they are the same.
		/// </summary>
		/// <param name="left">The TGCQuaternion to the left of the equality operator.</param>
		/// <param name="right">The TGCQuaternion to the right of the equality operator.</param>
		/// <returns>Compares the current instance of a class to another instance to determine whether they are the same.</returns>
		public static bool operator ==(TGCQuaternion left, TGCQuaternion right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Compares the current instance of a class to another instance to determine whether they are different.   
		/// </summary>
		/// <param name="left">The TGCQuaternion to the left of the inequality operator.</param>
		/// <param name="right">The TGCQuaternion to the right of the inequality operator.</param>
		/// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
		public static bool operator !=(TGCQuaternion left, TGCQuaternion right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determines the product of two quaternions.
		/// </summary>
		/// <param name="left">Source TGCQuaternion.</param>
		/// <param name="right">Source TGCQuaternion.</param>
		/// <returns>A TGCQuaternion structure that is the product of the right and left parameters.</returns>
		public static TGCQuaternion operator *(TGCQuaternion left, TGCQuaternion right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Subtracts two quaternions.
		/// </summary>
		/// <param name="left">The TGCQuaternion structure to the left of the subtraction operator.</param>
		/// <param name="right">The TGCQuaternion structure to the right of the subtraction operator.</param>
		/// <returns>Resulting TGCQuaternion structure.</returns>
		public static TGCQuaternion operator -(TGCQuaternion left, TGCQuaternion right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Rotates a quaternion around an arbitrary axis.
		/// </summary>
		/// <param name="v">A TGCVector3 that identifies the axis about which to rotate the quaternion.</param>
		/// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		public void RotateAxis(TGCVector3 v, float angle)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Builds a quaternion from a rotation matrix.
		/// </summary>
		/// <param name="m">Source Matrix that defines the rotation.</param>
		public void RotateMatrix(TGCMatrix m)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Builds a quaternion with the given yaw, pitch, and roll.
		/// </summary>
		/// <param name="yaw">Yaw around the y-axis, in radians.</param>
		/// <param name="pitch">Pitch around the x-axis, in radians.</param>
		/// <param name="roll">Roll around the z-axis, in radians.</param>
		public void RotateYawPitchRoll(float yaw, float pitch, float roll)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Builds a quaternion that is rotated around an arbitrary axis.
		/// </summary>
		/// <param name="v">A Vector3 that identifies the axis about which to rotate the quaternion.</param>
		/// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
		/// <returns>A Quaternion that is rotated around the specified axis.</returns>
		public static TGCQuaternion RotationAxis(TGCVector3 v, float angle)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Builds a quaternion from a rotation matrix.
		/// </summary>
		/// <param name="m">Source Matrix structure that defines the rotation.</param>
		/// <returns>A Quaternion structure built from a rotation matrix.</returns>
		public static TGCQuaternion RotationMatrix(TGCMatrix m)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Subtracts two quaternion instances.
		/// </summary>
		/// <param name="m1">Source Quaternion structure to the left of the subtraction operator.</param>
		/// <param name="m2">Source Quaternion structure to the right of the subtraction operator.</param>
		/// <returns>A Quaternion structure that is the result of the operation.</returns>
		public static TGCQuaternion Subtract(TGCQuaternion m1, TGCQuaternion m2)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Computes a quaternion's axis and angle of rotation.
		/// </summary>
		/// <param name="q">Source TGCQuaternion structure. See Remarks.</param>
		/// <param name="axis">A TGCVector3 structure that identifies the quaternion's axis of rotation.</param>
		/// <param name="angle">A float value that identifies the quaternion's angle of rotation, in radians.</param>
		public static void ToAxisAngle(TGCQuaternion q, ref TGCVector3 axis, ref float angle)
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
		/// Get the DX Vector3 wrapped to be use in DX primitives.
		/// </summary>
		/// <returns>The DX Vector3 wrapped</returns>
		public Quaternion ToQuaternion()
		{
			return this.DXQuaternion;
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

		/// <summary>
		///     Imprime un TGCQuaternion de la forma [x, y, z, w]
		/// </summary>
		public static string PrintTGCQuaternion(TGCQuaternion q)
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