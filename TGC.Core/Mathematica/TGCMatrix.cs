using Microsoft.DirectX;
using System;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a matrix.
    /// </summary>
    public class TGCMatrix
    {
        /// <summary>
        /// Initializes a new instance of the TGCMatrix class.
        /// </summary>
        public TGCMatrix()
        {
        }

        private Matrix DXMatrix { get; set; }

        /// <summary>
        /// Retrieves the determinant of the matrix.
        /// </summary>
        public float Determinant { get; }

        /// <summary>
        /// Retrieves the identity of the matrix.
        /// </summary>
        public static TGCMatrix Identity { get; }

        /// <summary>
        /// Retrieves an empty matrix.
        /// </summary>
        public static TGCMatrix Zero { get; }

        /// <summary>
        /// Retrieves or sets the element in the first row and the first column of the matrix.
        /// </summary>
        public float M11 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the first row and the second column of the matrix.
        /// </summary>
        public float M12 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the first row and the third column of the matrix.
        /// </summary>
        public float M13 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the first row and the fourth column of the matrix.
        /// </summary>
        public float M14 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the second row and the first column of the matrix.
        /// </summary>
        public float M21 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the second row and the second column of the matrix.
        /// </summary>
        public float M22 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the second row and the third column of the matrix.
        /// </summary>
        public float M23 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the second row and the fourth column of the matrix.
        /// </summary>
        public float M24 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the third row and the first column of the matrix.
        /// </summary>
        public float M31 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the third row and the second column of the matrix.
        /// </summary>
        public float M32 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the third row and the third column of the matrix.
        /// </summary>
        public float M33 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the third row and the fourth column of the matrix.
        /// </summary>
        public float M34 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the first column of the matrix.
        /// </summary>
        public float M41 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the second column of the matrix.
        /// </summary>
        public float M42 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the third column of the matrix.
        /// </summary>
        public float M43 { get; set; }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the fourth column of the matrix.
        /// </summary>
        public float M44 { get; set; }

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="left">A TGCMatrix instance on the left side of the addition operator.</param>
        /// <param name="right">A TGCMatrix instance on the right side of the addition operator.</param>
        /// <returns>A TGCMatrix instance that represents the result of the addition.</returns>
        public static TGCMatrix Add(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a 3-D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor. A value of zero indicates no scaling.</param>
        /// <param name="rotationCenter">A TGCVector3 that indicates the point at the center of rotation.</param>
        /// <param name="rotation">A 
        /// structure that specifies the rotation. Use Quaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use Vector2.Empty to specify no translation.</param>
        public void AffineTransformation(float scaling, TGCVector3 rotationCenter, Quaternion rotation, TGCVector3 translation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a 2-D affine transformation matrix in the xy plane.
        /// </summary>
        /// <param name="scaling">Scaling factor. A value of zero indicates no scaling.</param>
        /// <param name="rotationCenter">A Vector2 structure that represents a point identifying the center of rotation. Use an empty Vector2 for no rotation.</param>
        /// <param name="rotation">Angle of rotation. A value of zero indicates no rotation.</param>
        /// <param name="translation">A Vector2 structure that represents the translation. Use Vector2.Empty to specify no translation.</param>
        /// <returns>A TGCMatrix that is an affine transformation matrix.</returns>
        public static TGCMatrix AffineTransformation2D(float scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        {
            throw new NotImplementedException();
        }

        //TODO ver si hace falta redefinir o no el equals y hashcode
        //public override bool Equals(object compare);
        //public override int GetHashCode();

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        public void Invert()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        /// <param name="source">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the result of the operation.</returns>
        public static TGCMatrix Invert(TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        /// <param name="determinant">A Single value that contains the determinant of the matrix. If the determinant is not needed, the parameter is omitted.</param>
        /// <param name="source">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the result of the operation.</returns>
        public static TGCMatrix Invert(ref float determinant, TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a left-handed look-at matrix.
        /// </summary>
        /// <param name="cameraPosition">A TGCVector3 that defines the camera point. This value is used in translation.</param>
        /// <param name="cameraTarget">A TGCVector3 that defines the camera look-at target.</param>
        /// <param name="cameraUpVector">A TGCVector3 that defines the up direction of the current world, usually [0, 1, 0].</param>
        /// <returns>A TGCMatrix that is a left-handed look-at matrix.</returns>
        public static TGCMatrix LookAtLH(TGCVector3 cameraPosition, TGCVector3 cameraTarget, TGCVector3 cameraUpVector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a right-handed look-at matrix.
        /// </summary>
        /// <param name="cameraPosition">A TGCVector3 that defines the camera point. This value is used in translation.</param>
        /// <param name="cameraTarget">A TGCVector3 that defines the camera look-at target.</param>
        /// <param name="cameraUpVector">A TGCVector3 that defines the up direction of the current world, usually [0, 1, 0].</param>
        /// <returns>A TGCMatrix that is a right-handed look-at matrix.</returns>
        public static TGCMatrix LookAtRH(TGCVector3 cameraPosition, TGCVector3 cameraTarget, TGCVector3 cameraUpVector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="source">Source TGCMatrix to multiply by the current instance.</param>
        public void Multiply(TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">Source TGCMatrix.</param>
        /// <param name="right">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the product of two matrices.</returns>
        public static TGCMatrix Multiply(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the transposed product of two matrices.
        /// </summary>
        /// <param name="source">Source TGCMatrix to multiply and transpose with the current instance.</param>
        public void MultiplyTranspose(TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the transposed product of two matrices.
        /// </summary>
        /// <param name="left">Source TGCMatrix.</param>
        /// <param name="right">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the product and transposition of two matrices.</returns>
        public static TGCMatrix MultiplyTranspose(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds two instances of TGCMatrix.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the addition operator.</param>
        /// <param name="right">The TGCMatrix to the right of the addition operator.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix operator +(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the equality operator.</param>
        /// <param name="right">The TGCMatrix to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the inequality operator.</param>
        /// <param name="right">The TGCMatrix to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">Source Matrix structure.</param>
        /// <param name="right">Source Matrix structure.</param>
        /// <returns>A Matrix structure that is the product of two matrices.</returns>
        public static TGCMatrix operator *(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts two instances of the TGCMatrix.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the subtraction operator.</param>
        /// <param name="right">The TGCMatrix to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix operator -(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a left-handed orthogonal projection matrix.
        /// </summary>
        /// <param name="width">Width of the view volume.</param>
        /// <param name="height">Height of the view volume.</param>
        /// <param name="znearPlane">Minimum z-value of the view volume, which is referred to as z-near.</param>
        /// <param name="zfarPlane">Maximum z-value of the view volume, which is referred to as z-far.</param>
        /// <returns>Pointer to a TGCMatrix that is a left-handed orthogonal projection matrix.</returns>
        public static TGCMatrix OrthoLH(float width, float height, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a customized, left-handed orthogonal projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the view volume.</param>
        /// <param name="right">Maximum x-value of the view volume.</param>
        /// <param name="bottom">Minimum y-value of the view volume.</param>
        /// <param name="top">Maximum y-value of the view volume.</param>
        /// <param name="znearPlane">Minimum z-value of the view volume.</param>
        /// <param name="zfarPlane">Maximum z-value of the view volume.</param>
        /// <returns>A TGCMatrix that is a customized, left-handed orthogonal projection matrix.</returns>
        public static TGCMatrix OrthoOffCenterLH(float left, float right, float bottom, float top, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a customized, right-handed orthogonal projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the view volume.</param>
        /// <param name="right">Maximum x-value of the view volume.</param>
        /// <param name="bottom">Maximum y-value of the view volume.</param>
        /// <param name="top">Minimum y-value of the view volume.</param>
        /// <param name="znearPlane">Minimum z-value of the view volume.</param>
        /// <param name="zfarPlane">Maximum z-value of the view volume.</param>
        /// <returns>A TGCMatrix that is a customized, right-handed orthogonal projection matrix.</returns>
        public static TGCMatrix OrthoOffCenterRH(float left, float right, float bottom, float top, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a left-handed perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fieldOfViewY">Field of view in the y direction, in radians.</param>
        /// <param name="aspectRatio">Aspect ratio, defined as the view space width divided by height.</param>
        /// <param name="znearPlane">Z-value of the near view plane.</param>
        /// <param name="zfarPlane">Z-value of the far view plane.</param>
        /// <returns>A TGCMatrix that is a left-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveFovLH(float fieldOfViewY, float aspectRatio, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a right-handed perspective projection matrix based on a field of view (FOV).
        /// </summary>
        /// <param name="fieldOfViewY">Field of view in the y direction, in radians.</param>
        /// <param name="aspectRatio">Aspect ratio, defined as the view space width divided by height.</param>
        /// <param name="znearPlane">Z-value of the near view plane.</param>
        /// <param name="zfarPlane">Z-value of the far view plane.</param>
        /// <returns>A TGCMatrix that is a right-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveFovRH(float fieldOfViewY, float aspectRatio, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Builds a left-handed perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the view volume at the near view plane.</param>
        /// <param name="height">Height of the view volume at the near view plane.</param>
        /// <param name="znearPlane">Z-value of the near view plane.</param>
        /// <param name="zfarPlane">Z-value of the far view plane.</param>
        /// <returns>A TGCMatrix that is a left-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveLH(float width, float height, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a customized, left-handed perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the view volume.</param>
        /// <param name="right">Maximum x-value of the view volume.</param>
        /// <param name="bottom">Minimum y-value of the view volume.</param>
        /// <param name="top">Maximum y-value of the view volume.</param>
        /// <param name="znearPlane">Minimum z-value of the view volume.</param>
        /// <param name="zfarPlane">Maximum z-value of the view volume.</param>
        /// <returns>A TGCMatrix that is a customized, left-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveOffCenterLH(float left, float right, float bottom, float top, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a customized, right-handed perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the view volume.</param>
        /// <param name="right">Maximum x-value of the view volume.</param>
        /// <param name="bottom">Minimum y-value of the view volume.</param>
        /// <param name="top">Maximum y-value of the view volume.</param>
        /// <param name="znearPlane">Minimum z-value of the view volume.</param>
        /// <param name="zfarPlane">Maximum z-value of the view volume.</param>
        /// <returns>A TGCMatrix that is a customized, right-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveOffCenterRH(float left, float right, float bottom, float top, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a right-handed perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the view volume at the near view plane.</param>
        /// <param name="height">Height of the view volume at the near view plane.</param>
        /// <param name="znearPlane">Z-value of the near view plane.</param>
        /// <param name="zfarPlane">Z-value of the far view plane.</param>
        /// <returns>Pointer to a TGCMatrix that is a right-handed perspective projection matrix.</returns>
        public static TGCMatrix PerspectiveRH(float width, float height, float znearPlane, float zfarPlane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that reflects the coordinate system about a plane.
        /// </summary>
        /// <param name="plane">Source TGCPlane structure.</param>
        public void Reflect(TGCPlane plane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates the matrix around an arbitrary axis.
        /// </summary>
        /// <param name="axisRotation">A TGCVector3 structure that identifies the axis about which to rotate the matrix.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateAxis(TGCVector3 axisRotation, float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates a matrix from a quaternion.
        /// </summary>
        /// <param name="quat">Source Quaternion structure that defines the rotation.</param>
        public void RotateQuaternion(Quaternion quat)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates a matrix around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateX(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates a matrix around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateY(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates a matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        public void RotateYawPitchRoll(float yaw, float pitch, float roll)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rotates the matrix around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when viewed from the rotation axis (positive side) toward the origin.</param>
        public void RotateZ(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axisRotation">A TGCVector3 that identifies the axis about which to rotate the matrix.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationAxis(TGCVector3 axisRotation, float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix from a quaternion.
        /// </summary>
        /// <param name="quat">Source Quaternion structure.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationQuaternion(Quaternion quat)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationX(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationY(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Builds a matrix with a specified yaw, pitch, and roll.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when viewed from the rotation axis (positive side) toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationZ(float angle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scales the matrix along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        public void Scale(float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Scales the matrix along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="v">A TGCVector3 containing three values that represent the scaling factors applied along the x-axis, y-axis, and z-axis.</param>
        public void Scale(TGCVector3 v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that scales along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <returns>Scaled TGCMatrix.</returns>
        public static TGCMatrix Scaling(float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that scales along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="v">A TGCVector3 containing three values that represent the scaling factors applied along the x-axis, y-axis, and z-axis.</param>
        /// <returns>Scaled TGCMatrix.</returns>
        public static TGCMatrix Scaling(TGCVector3 v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix that flattens geometry into a plane.
        /// </summary>
        /// <param name="light">A Vector4 structure that describes the light's position.</param>
        /// <param name="plane">Source TGCPlane structure.</param>
        public void Shadow(Vector4 light, TGCPlane plane)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Subtracts one matrix from another.
        /// </summary>
        /// <param name="left">A TGCMatrix instance on the left side of the subtraction operation.</param>
        /// <param name="right">A TGCMatrix instance on the right side of the subtraction operation.</param>
        /// <returns>A TGCMatrix instance that represents the result of the subtraction.</returns>
        public static TGCMatrix Subtract(TGCMatrix left, TGCMatrix right)
        {
            throw new NotImplementedException();
        }

        //TODO ver si hace falta redefinir toString
        //public override string ToString();

        /// <summary>
        /// Transforms the matrix.
        /// </summary>
        /// <param name="scalingCenter">A TGCVector3 that identifies the scaling center point.</param>
        /// <param name="scalingRotation">A Quaternion structure that specifies the scaling rotation. Use Quaternion.Identity to specify no scaling.</param>
        /// <param name="scalingFactor">A TGCVector3 that is the scaling vector.</param>
        /// <param name="rotationCenter">A TGCVector3 that is a point that identifies the center of rotation.</param>
        /// <param name="rotation">A Quaternion structure that specifies the rotation. Use Quaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use Vector3.Empty to specify no translation.</param>
        public void Transform(TGCVector3 scalingCenter, Quaternion scalingRotation, TGCVector3 scalingFactor, TGCVector3 rotationCenter, Quaternion rotation, TGCVector3 translation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">A TGCVector3 that identifies the scaling center point.</param>
        /// <param name="scalingRotation">A Quaternion structure that specifies the scaling rotation. Use Quaternion.Identity to specify no scaling.</param>
        /// <param name="scalingFactor">A TGCVector3 that is the scaling vector.</param>
        /// <param name="rotationCenter">A TGCVector3 that is a point that identifies the center of rotation.</param>
        /// <param name="rotation">A Quaternion structure that specifies the rotation. Use Quaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use Vector3.Empty to specify no translation.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix Transformation(TGCVector3 scalingCenter, Quaternion scalingRotation, TGCVector3 scalingFactor, TGCVector3 rotationCenter, Quaternion rotation, TGCVector3 translation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a 2-D transformation matrix in the xy plane.
        /// </summary>
        /// <param name="scalingCenter">A Vector2 structure that is a point identifying the scaling center.</param>
        /// <param name="scalingRotation">Scaling rotation factor. Use a zero value to specify no rotation.</param>
        /// <param name="scaling">A Vector2 structure that is a point identifying the scale. Use Vector2.Empty to specify no scaling.</param>
        /// <param name="rotationCenter">A Vector2 structure that is a point identifying the rotation center.</param>
        /// <param name="rotation">Angle of rotation, in radians.</param>
        /// <param name="translation">A Vector2 structure that identifies the translation. Use Vector2.Empty to specify no translation.</param>
        /// <returns>A TGCMatrix that contains the transformation matrix.</returns>
        public static TGCMatrix Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Translates the matrix using specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        public void Translate(float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Translates the matrix using specified offsets.
        /// </summary>
        /// <param name="v">A TGCVector3 that contains the x-coordinate, y-coordinate, and z-coordinate offsets.</param>
        public void Translate(TGCVector3 v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix using specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <returns>A TGCMatrix that contains a translated transformation matrix.</returns>
        public static TGCMatrix Translation(float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds a matrix using specified offsets.
        /// </summary>
        /// <param name="v">A TGCVector3 that contains the x-coordinate, y-coordinate, and z-coordinate offsets.</param>
        /// <returns>A TGCMatrix that contains a translated transformation matrix.</returns>
        public static TGCMatrix Translation(TGCVector3 v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transposes the matrix using a source matrix.
        /// </summary>
        /// <param name="source">Source TGCMatrix.</param>
        public void Transpose(TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the matrix transpose of a given matrix.
        /// </summary>
        /// <param name="source">Source Matrix.</param>
        /// <returns>A TGCMatrix object that is the matrix transpose of the matrix.</returns>
        public static TGCMatrix TransposeMatrix(TGCMatrix source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the DX Matrix wrapped to be use in DX primitives.
        /// </summary>
        /// <returns>The DX Matrix wrapped</returns>
        public Matrix ToMatrix()
        {
            return this.DXMatrix;
        }

        /// <summary>
        /// Transform TGCMatrix[] to DX Matrix[]
        /// </summary>
        /// <param name="am">Source TGCMatrix.</param>
        /// <returns>A Matrix[] with all de wrapped Matrix in TGCMatrix.</returns>
        public static Matrix[] ToMatrixArray(TGCMatrix[] am)
        {
            Matrix[] m = new Matrix[am.Length];

            for (int i = 0; i < am.Length; i++)
            {
                m[i] = am[i].ToMatrix();
            }

            return m;
        }

        /// <summary>
        /// New TGCMatrix from DX Matrix
        /// </summary>
        /// <param name="matrix">Source Matrix.</param>
        /// <returns>Initializes a new instance of the TGCMatrix class.</returns>
        public static TGCMatrix FromMatrix(Matrix matrix)
        {
            return new TGCMatrix();
        }
    }
}