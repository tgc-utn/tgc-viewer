using Microsoft.DirectX;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a matrix.
    /// </summary>
    public struct TGCMatrix
    {
        private static TGCMatrix IDENTITY = new TGCMatrix(Matrix.Identity);
        private static TGCMatrix ZERO = new TGCMatrix(Matrix.Zero);

        /// <summary>
        /// Initializes a new instance of the TGCMatrix class.
        /// </summary>
        /// <param name="dxMatrix">Matrix from value.</param>
        public TGCMatrix(Matrix dxMatrix)
        {
            this.DXMatrix = dxMatrix;
        }

        private Matrix DXMatrix;

        /// <summary>
        /// Retrieves the determinant of the matrix.
        /// </summary>
        public float Determinant { get { return DXMatrix.Determinant; } }

        /// <summary>
        /// Retrieves the identity of the matrix.
        /// </summary>
        public static TGCMatrix Identity { get { return IDENTITY; } }

        /// <summary>
        /// Retrieves an empty matrix.
        /// </summary>
        public static TGCMatrix Zero { get { return ZERO; } }

        /// <summary>
        /// Retrieves or sets the element in the first row and the first column of the matrix.
        /// </summary>
        public float M11
        {
            get { return this.DXMatrix.M11; }
            set { this.DXMatrix.M11 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the first row and the second column of the matrix.
        /// </summary>
        public float M12
        {
            get { return this.DXMatrix.M12; }
            set { this.DXMatrix.M12 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the first row and the third column of the matrix.
        /// </summary>
        public float M13
        {
            get { return this.DXMatrix.M13; }
            set { this.DXMatrix.M13 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the first row and the fourth column of the matrix.
        /// </summary>
        public float M14
        {
            get { return this.DXMatrix.M14; }
            set { this.DXMatrix.M14 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the second row and the first column of the matrix.
        /// </summary>
        public float M21
        {
            get { return this.DXMatrix.M21; }
            set { this.DXMatrix.M21 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the second row and the second column of the matrix.
        /// </summary>
        public float M22
        {
            get { return this.DXMatrix.M22; }
            set { this.DXMatrix.M22 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the second row and the third column of the matrix.
        /// </summary>
        public float M23
        {
            get { return this.DXMatrix.M23; }
            set { this.DXMatrix.M23 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the second row and the fourth column of the matrix.
        /// </summary>
        public float M24
        {
            get { return this.DXMatrix.M24; }
            set { this.DXMatrix.M24 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the third row and the first column of the matrix.
        /// </summary>
        public float M31
        {
            get { return this.DXMatrix.M31; }
            set { this.DXMatrix.M31 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the third row and the second column of the matrix.
        /// </summary>
        public float M32
        {
            get { return this.DXMatrix.M32; }
            set { this.DXMatrix.M32 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the third row and the third column of the matrix.
        /// </summary>
        public float M33
        {
            get { return this.DXMatrix.M33; }
            set { this.DXMatrix.M33 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the third row and the fourth column of the matrix.
        /// </summary>
        public float M34
        {
            get { return this.DXMatrix.M34; }
            set { this.DXMatrix.M34 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the first column of the matrix.
        /// </summary>
        public float M41
        {
            get { return this.DXMatrix.M41; }
            set { this.DXMatrix.M41 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the second column of the matrix.
        /// </summary>
        public float M42
        {
            get { return this.DXMatrix.M42; }
            set { this.DXMatrix.M42 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the third column of the matrix.
        /// </summary>
        public float M43
        {
            get { return this.DXMatrix.M43; }
            set { this.DXMatrix.M43 = value; }
        }

        /// <summary>
        /// Retrieves or sets the element in the fourth row and the fourth column of the matrix.
        /// </summary>
        public float M44
        {
            get { return this.DXMatrix.M44; }
            set { this.DXMatrix.M44 = value; }
        }

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="left">A TGCMatrix instance on the left side of the addition operator.</param>
        /// <param name="right">A TGCMatrix instance on the right side of the addition operator.</param>
        /// <returns>A TGCMatrix instance that represents the result of the addition.</returns>
        public static TGCMatrix Add(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(Matrix.Add(left.ToMatrix(), right.ToMatrix()));
        }

        /// <summary>
        /// Builds a 3-D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor. A value of zero indicates no scaling.</param>
        /// <param name="rotationCenter">A TGCVector3 that indicates the point at the center of rotation.</param>
        /// <param name="rotation">A
        /// structure that specifies the rotation. Use TGCQuaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use TGCVector3.Empty to specify no translation.</param>
        public void AffineTransformation(float scaling, TGCVector3 rotationCenter, TGCQuaternion rotation, TGCVector3 translation)
        {
            this.DXMatrix.AffineTransformation(scaling, rotationCenter, rotation, translation);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Luego de cambiar valores de DXMatrix deberian llamar a este metodo.
        /// </summary>
        private void AssingAllDXMatrix()
        {
            this.M11 = DXMatrix.M11;
            this.M12 = DXMatrix.M12;
            this.M13 = DXMatrix.M13;
            this.M14 = DXMatrix.M14;
            this.M21 = DXMatrix.M21;
            this.M22 = DXMatrix.M22;
            this.M23 = DXMatrix.M23;
            this.M24 = DXMatrix.M24;
            this.M31 = DXMatrix.M31;
            this.M32 = DXMatrix.M32;
            this.M33 = DXMatrix.M33;
            this.M34 = DXMatrix.M34;
            this.M41 = DXMatrix.M41;
            this.M42 = DXMatrix.M42;
            this.M43 = DXMatrix.M43;
            this.M44 = DXMatrix.M44;
        }

        /// <summary>
        /// Builds a 2-D affine transformation matrix in the xy plane.
        /// </summary>
        /// <param name="scaling">Scaling factor. A value of zero indicates no scaling.</param>
        /// <param name="rotationCenter">A TGCVector2 structure that represents a point identifying the center of rotation. Use an empty TGCVector2 for no rotation.</param>
        /// <param name="rotation">Angle of rotation. A value of zero indicates no rotation.</param>
        /// <param name="translation">A TGCVector2 structure that represents the translation. Use TGCVector2.Empty to specify no translation.</param>
        /// <returns>A TGCMatrix that is an affine transformation matrix.</returns>
        public static TGCMatrix AffineTransformation2D(float scaling, TGCVector2 rotationCenter, float rotation, TGCVector2 translation)
        {
            return new TGCMatrix(Matrix.AffineTransformation2D(scaling, rotationCenter, rotation, translation));
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="compare">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object compare)
        {
            if (compare is TGCMatrix)
            {
                TGCMatrix other = ((TGCMatrix)compare);
                return this.DXMatrix.Equals(other.DXMatrix);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return DXMatrix.GetHashCode();
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        public void Invert()
        {
            DXMatrix.Invert();
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        /// <param name="source">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the result of the operation.</returns>
        public static TGCMatrix Invert(TGCMatrix source)
        {
            return new TGCMatrix(Matrix.Invert(source.ToMatrix()));
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        /// <param name="determinant">A Single value that contains the determinant of the matrix. If the determinant is not needed, the parameter is omitted.</param>
        /// <param name="source">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the result of the operation.</returns>
        public static TGCMatrix Invert(ref float determinant, TGCMatrix source)
        {
            return new TGCMatrix(Matrix.Invert(ref determinant, source.ToMatrix()));
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
            return new TGCMatrix(Matrix.LookAtLH(cameraPosition, cameraTarget, cameraUpVector));
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
            return new TGCMatrix(Matrix.LookAtRH(cameraPosition, cameraTarget, cameraUpVector));
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="source">Source TGCMatrix to multiply by the current instance.</param>
        public void Multiply(TGCMatrix source)
        {
            this.DXMatrix.Multiply(source.ToMatrix());
            this.AssingAllDXMatrix();
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">Source TGCMatrix.</param>
        /// <param name="right">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the product of two matrices.</returns>
        public static TGCMatrix Multiply(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(Matrix.Multiply(left.ToMatrix(), right.ToMatrix()));
        }

        /// <summary>
        /// Calculates the transposed product of two matrices.
        /// </summary>
        /// <param name="source">Source TGCMatrix to multiply and transpose with the current instance.</param>
        public void MultiplyTranspose(TGCMatrix source)
        {
            this.DXMatrix.MultiplyTranspose(source.ToMatrix());
            this.AssingAllDXMatrix();
        }

        /// <summary>
        /// Calculates the transposed product of two matrices.
        /// </summary>
        /// <param name="left">Source TGCMatrix.</param>
        /// <param name="right">Source TGCMatrix.</param>
        /// <returns>A TGCMatrix that is the product and transposition of two matrices.</returns>
        public static TGCMatrix MultiplyTranspose(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(Matrix.MultiplyTranspose(left.ToMatrix(), right.ToMatrix()));
        }

        /// <summary>
        /// Cast TGCMatrix to DX Matrix
        /// </summary>
        /// <param name="vector">TGCMatrix to become into Matrix</param>
        public static implicit operator Matrix(TGCMatrix matrix)
        {
            return matrix.ToMatrix();
        }

        /// <summary>
        /// Adds two instances of TGCMatrix
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the addition operator.</param>
        /// <param name="right">The TGCMatrix to the right of the addition operator.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix operator +(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(left.ToMatrix() + right.ToMatrix());
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the equality operator.</param>
        /// <param name="right">The TGCMatrix to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCMatrix left, TGCMatrix right)
        {
            //TODO validar NPE????
            return left.ToMatrix() == right.ToMatrix();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the inequality operator.</param>
        /// <param name="right">The TGCMatrix to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCMatrix left, TGCMatrix right)
        {
            return left.ToMatrix() != right.ToMatrix();
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">Source Matrix structure.</param>
        /// <param name="right">Source Matrix structure.</param>
        /// <returns>A Matrix structure that is the product of two matrices.</returns>
        public static TGCMatrix operator *(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(left.ToMatrix() * right.ToMatrix());
        }

        /// <summary>
        /// Subtracts two instances of the TGCMatrix.
        /// </summary>
        /// <param name="left">The TGCMatrix to the left of the subtraction operator.</param>
        /// <param name="right">The TGCMatrix to the right of the subtraction operator.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix operator -(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(left.ToMatrix() - right.ToMatrix());
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
            return new TGCMatrix(Matrix.OrthoLH(width, height, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.OrthoOffCenterLH(left, right, bottom, top, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.OrthoOffCenterRH(left, right, bottom, top, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveFovLH(fieldOfViewY, aspectRatio, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveFovRH(fieldOfViewY, aspectRatio, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveLH(width, height, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveOffCenterLH(left, right, bottom, top, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveOffCenterRH(left, right, bottom, top, znearPlane, zfarPlane));
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
            return new TGCMatrix(Matrix.PerspectiveRH(width, height, znearPlane, zfarPlane));
        }

        /// <summary>
        /// Builds a matrix that reflects the coordinate system about a plane.
        /// </summary>
        /// <param name="plane">Source TGCPlane structure.</param>
        public void Reflect(TGCPlane plane)
        {
            this.DXMatrix.Reflect(plane);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates the matrix around an arbitrary axis.
        /// </summary>
        /// <param name="axisRotation">A TGCVector3 structure that identifies the axis about which to rotate the matrix.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateAxis(TGCVector3 axisRotation, float angle)
        {
            this.DXMatrix.RotateAxis(axisRotation, angle);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates a matrix from a quaternion.
        /// </summary>
        /// <param name="quat">Source TGCQuaternion structure that defines the rotation.</param>
        public void RotateTGCQuaternion(TGCQuaternion quat)
        {
            this.DXMatrix.RotateQuaternion(quat);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates a matrix around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateX(float angle)
        {
            this.DXMatrix.RotateX(angle);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates a matrix around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        public void RotateY(float angle)
        {
            this.DXMatrix.RotateY(angle);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates a matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        public void RotateYawPitchRoll(float yaw, float pitch, float roll)
        {
            this.DXMatrix.RotateYawPitchRoll(yaw, pitch, roll);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Rotates the matrix around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when viewed from the rotation axis (positive side) toward the origin.</param>
        public void RotateZ(float angle)
        {
            this.DXMatrix.RotateZ(angle);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Builds a matrix that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axisRotation">A TGCVector3 that identifies the axis about which to rotate the matrix.</param>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationAxis(TGCVector3 axisRotation, float angle)
        {
            return new TGCMatrix(Matrix.RotationAxis(axisRotation, angle));
        }

        /// <summary>
        /// Builds a matrix from a quaternion.
        /// </summary>
        /// <param name="quat">Source TGCQuaternion structure.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationTGCQuaternion(TGCQuaternion quat)
        {
            return new TGCMatrix(Matrix.RotationQuaternion(quat));
        }

        /// <summary>
        /// Builds a matrix that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationX(float angle)
        {
            return new TGCMatrix(Matrix.RotationX(angle));
        }

        /// <summary>
        /// Builds a matrix that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationY(float angle)
        {
            return new TGCMatrix(Matrix.RotationY(angle));
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
            return new TGCMatrix(Matrix.RotationYawPitchRoll(yaw, pitch, roll));
        }

        /// <summary>
        /// Builds a matrix that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation, in radians. Angles are measured clockwise when viewed from the rotation axis (positive side) toward the origin.</param>
        /// <returns>Rotated TGCMatrix.</returns>
        public static TGCMatrix RotationZ(float angle)
        {
            return new TGCMatrix(Matrix.RotationZ(angle));
        }

        /// <summary>
        /// Scales the matrix along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        public void Scale(float x, float y, float z)
        {
            this.DXMatrix.Scale(x, y, z);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Scales the matrix along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="v">A TGCVector3 containing three values that represent the scaling factors applied along the x-axis, y-axis, and z-axis.</param>
        public void Scale(TGCVector3 v)
        {
            this.DXMatrix.Scale(v);
            AssingAllDXMatrix();
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
            return new TGCMatrix(Matrix.Scaling(x, y, z));
        }

        /// <summary>
        /// Builds a matrix that scales along the x-axis, y-axis, and z-axis.
        /// </summary>
        /// <param name="v">A TGCVector3 containing three values that represent the scaling factors applied along the x-axis, y-axis, and z-axis.</param>
        /// <returns>Scaled TGCMatrix.</returns>
        public static TGCMatrix Scaling(TGCVector3 v)
        {
            return new TGCMatrix(Matrix.Scaling(v));
        }

        /// <summary>
        /// Builds a matrix that flattens geometry into a plane.
        /// </summary>
        /// <param name="light">A Vector4 structure that describes the light's position.</param>
        /// <param name="plane">Source TGCPlane structure.</param>
        public void Shadow(TGCVector4 light, TGCPlane plane)
        {
            this.DXMatrix.Shadow(light, plane);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Subtracts one matrix from another.
        /// </summary>
        /// <param name="left">A TGCMatrix instance on the left side of the subtraction operation.</param>
        /// <param name="right">A TGCMatrix instance on the right side of the subtraction operation.</param>
        /// <returns>A TGCMatrix instance that represents the result of the subtraction.</returns>
        public static TGCMatrix Subtract(TGCMatrix left, TGCMatrix right)
        {
            return new TGCMatrix(Matrix.Subtract(left.ToMatrix(), right.ToMatrix()));
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            //TODO mejorar.
            return this.DXMatrix.ToString();
        }

        /// <summary>
        /// Transforms the matrix.
        /// </summary>
        /// <param name="scalingCenter">A TGCVector3 that identifies the scaling center point.</param>
        /// <param name="scalingRotation">A TGCQuaternion structure that specifies the scaling rotation. Use TGCQuaternion.Identity to specify no scaling.</param>
        /// <param name="scalingFactor">A TGCVector3 that is the scaling vector.</param>
        /// <param name="rotationCenter">A TGCVector3 that is a point that identifies the center of rotation.</param>
        /// <param name="rotation">A TGCQuaternion structure that specifies the rotation. Use TGCQuaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use Vector3.Empty to specify no translation.</param>
        public void Transform(TGCVector3 scalingCenter, TGCQuaternion scalingRotation, TGCVector3 scalingFactor, TGCVector3 rotationCenter, TGCQuaternion rotation, TGCVector3 translation)
        {
            this.DXMatrix.Transform(scalingCenter, scalingRotation, scalingFactor, rotationCenter, rotation, translation);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Builds a transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">A TGCVector3 that identifies the scaling center point.</param>
        /// <param name="scalingRotation">A TGCQuaternion structure that specifies the scaling rotation. Use TGCQuaternion.Identity to specify no scaling.</param>
        /// <param name="scalingFactor">A TGCVector3 that is the scaling vector.</param>
        /// <param name="rotationCenter">A TGCVector3 that is a point that identifies the center of rotation.</param>
        /// <param name="rotation">A TGCQuaternion structure that specifies the rotation. Use TGCQuaternion.Identity to specify no rotation.</param>
        /// <param name="translation">A TGCVector3 that represents the translation. Use Vector3.Empty to specify no translation.</param>
        /// <returns>Resulting TGCMatrix.</returns>
        public static TGCMatrix Transformation(TGCVector3 scalingCenter, TGCQuaternion scalingRotation, TGCVector3 scalingFactor, TGCVector3 rotationCenter, TGCQuaternion rotation, TGCVector3 translation)
        {
            return new TGCMatrix(Matrix.Transformation(scalingCenter, scalingRotation, scalingFactor, rotationCenter, rotation, translation));
        }

        /// <summary>
        /// Builds a 2-D transformation matrix in the xy plane.
        /// </summary>
        /// <param name="scalingCenter">A TGCVector2 structure that is a point identifying the scaling center.</param>
        /// <param name="scalingRotation">Scaling rotation factor. Use a zero value to specify no rotation.</param>
        /// <param name="scaling">A TGCVector2 structure that is a point identifying the scale. Use TGCVector2.Empty to specify no scaling.</param>
        /// <param name="rotationCenter">A TGCVector2 structure that is a point identifying the rotation center.</param>
        /// <param name="rotation">Angle of rotation, in radians.</param>
        /// <param name="translation">A TGCVector2 structure that identifies the translation. Use TGCVector2.Empty to specify no translation.</param>
        /// <returns>A TGCMatrix that contains the transformation matrix.</returns>
        public static TGCMatrix Transformation2D(TGCVector2 scalingCenter, float scalingRotation, TGCVector2 scaling, TGCVector2 rotationCenter, float rotation, TGCVector2 translation)
        {
            return new TGCMatrix(Matrix.Transformation2D(scalingCenter, scalingRotation, scaling, rotationCenter, rotation, translation));
        }

        /// <summary>
        /// Translates the matrix using specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        public void Translate(float x, float y, float z)
        {
            this.DXMatrix.Translate(x, y, z);
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Translates the matrix using specified offsets.
        /// </summary>
        /// <param name="v">A TGCVector3 that contains the x-coordinate, y-coordinate, and z-coordinate offsets.</param>
        public void Translate(TGCVector3 v)
        {
            this.DXMatrix.Translate(v);
            AssingAllDXMatrix();
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
            return new TGCMatrix(Matrix.Translation(x, y, z));
        }

        /// <summary>
        /// Builds a matrix using specified offsets.
        /// </summary>
        /// <param name="v">A TGCVector3 that contains the x-coordinate, y-coordinate, and z-coordinate offsets.</param>
        /// <returns>A TGCMatrix that contains a translated transformation matrix.</returns>
        public static TGCMatrix Translation(TGCVector3 v)
        {
            return new TGCMatrix(Matrix.Translation(v));
        }

        /// <summary>
        /// Transposes the matrix using a source matrix.
        /// </summary>
        /// <param name="source">Source TGCMatrix.</param>
        public void Transpose(TGCMatrix source)
        {
            this.DXMatrix.Transpose(source.ToMatrix());
            AssingAllDXMatrix();
        }

        /// <summary>
        /// Returns the matrix transpose of a given matrix.
        /// </summary>
        /// <param name="source">Source Matrix.</param>
        /// <returns>A TGCMatrix object that is the matrix transpose of the matrix.</returns>
        public static TGCMatrix TransposeMatrix(TGCMatrix source)
        {
            return new TGCMatrix(Matrix.TransposeMatrix(source.ToMatrix()));
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
            //TODO deprecar.
            return new TGCMatrix(matrix);
        }
    }
}