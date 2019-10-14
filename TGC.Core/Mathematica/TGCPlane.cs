using Microsoft.DirectX;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a plane.
    /// </summary>
    public struct TGCPlane
    {
        /// <summary>
        /// Retrieves or sets the DirectX of a Plane.
        /// </summary>
        private Plane dxPlane;

        /// <summary>
        /// Initializes a new instance of the Plane class.
        /// </summary>
        /// <param name="valuePointA">A Single value used to set the initial value of the A field.</param>
        /// <param name="valuePointB">A Single value used to set the initial value of the B field.</param>
        /// <param name="valuePointC">A Single value used to set the initial value of the C field.</param>
        /// <param name="valuePointD">A Single value used to set the initial value of the D field.</param>
        public TGCPlane(float valuePointA, float valuePointB, float valuePointC, float valuePointD)
        {
            this.dxPlane = new Plane(valuePointA, valuePointB, valuePointC, valuePointD);
        }

        /// <summary>
        /// Initializes a new instance of the Plane class.
        /// </summary>
        /// <param name="dxPlane">A plane DXPlane.</param>
        public TGCPlane(Plane dxPlane)
        {
            this.dxPlane = dxPlane;
        }

        /// <summary>
        /// Retrieves an empty plane.
        /// </summary>
        public static TGCPlane Zero
        {
            get { return new TGCPlane(Plane.Empty); }
        }

        /// <summary>
        /// Retrieves or sets the 'A' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float A
        {
            get { return this.dxPlane.A; }
            set { this.dxPlane.A = value; }
        }

        /// <summary>
        /// Retrieves or sets the 'B' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float B
        {
            get { return this.dxPlane.B; }
            set { this.dxPlane.B = value; }
        }

        /// <summary>
        /// Retrieves or sets the 'C' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float C
        {
            get { return this.dxPlane.C; }
            set { this.dxPlane.C = value; }
        }

        /// <summary>
        /// Retrieves or sets the 'D' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float D
        {
            get { return this.dxPlane.D; }
            set { this.dxPlane.D = value; }
        }

        /// <summary>
        /// Computes the dot product of a plane and a vector.
        /// </summary>
        /// <param name="v">Source TGCVector3.</param>
        /// <returns>A Single value that is the dot product of the plane and the vector.</returns>
        public float Dot(TGCVector3 v)
        {
            return this.dxPlane.Dot(v);
        }

        /// <summary>
        /// Computes the dot product of a plane and a vector.
        /// </summary>
        /// <param name="v">Source TGCVector4 structure.</param>
        /// <returns>A Single value that is the dot product of the plane and the vector.</returns>
        public float Dot(TGCVector4 v)
        {
            return this.dxPlane.Dot(v);
        }

        /// <summary>
        /// Computes the dot product of a plane and a 3-D vector. The w parameter of the vector is assumed to be 0.
        /// </summary>
        /// <param name="p">Source TGCPlane.</param>
        /// <param name="v">Source TGCVector3.</param>
        /// <returns>A Single value that represents the dot product of the plane and the 3-D vector.</returns>
        public static float DotNormal(TGCPlane p, TGCVector3 v)
        {
            return Plane.DotNormal(p.ToPlane(), v);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TGCPlane))
                return false;
            else
                return this == ((TGCPlane)obj);
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            return this.dxPlane.GetHashCode();
        }

        /// <summary>
        /// Constructs a plane from a point and a normal.
        /// </summary>
        /// <param name="point">A TGCVector3 that defines the point used to construct the plane.</param>
        /// <param name="normal">A TGCVector3 that defines the normal used to construct the plane.</param>
        /// <returns>A TGCPlane constructed from the point and the normal.</returns>
        public static TGCPlane FromPointNormal(TGCVector3 point, TGCVector3 normal)
        {
            return new TGCPlane(Plane.FromPointNormal(point, normal));
        }

        /// <summary>
        /// Constructs a plane from three points.
        /// </summary>
        /// <param name="p1">A TGCVector3 that defines one of the points used to construct the plane.</param>
        /// <param name="p2">A TGCVector3 that defines one of the points used to construct the plane.</param>
        /// <param name="p3">A TGCVector3 that defines one of the points used to construct the plane.</param>
        /// <returns>A TGCPlane constructed from the given points.</returns>
        public static TGCPlane FromPoints(TGCVector3 p1, TGCVector3 p2, TGCVector3 p3)
        {
            return new TGCPlane(Plane.FromPoints(p1, p1, p3));
        }

        /// <summary>
        /// Finds the intersection between a plane and a line.
        /// </summary>
        /// <param name="p">Source TGCPlane.</param>
        /// <param name="v1">Source TGCVector3 that defines a line starting point.</param>
        /// <param name="v2">Source TGCVector3 that defines a line ending point.</param>
        /// <returns>A TGCVector3 that is the intersection between the specified plane and line.</returns>
        public static TGCVector3 IntersectLine(TGCPlane p, TGCVector3 v1, TGCVector3 v2)
        {
            return new TGCVector3(Plane.IntersectLine(p.ToPlane(), v1, v2));
        }

        /// <summary>
        /// Returns the normal of a plane.
        /// </summary>
        public void Normalize()
        {
            this.dxPlane.Normalize();
        }

        /// <summary>
        /// Returns the normal of a plane.
        /// </summary>
        /// <param name="p">Source TGCPlane.</param>
        /// <returns>A TGCPlane that represents the normal of the plane.</returns>
        public static TGCPlane Normalize(TGCPlane p)
        {
            return new TGCPlane(Plane.Normalize(p.ToPlane()));
        }

        /// <summary>
        /// Cast TGCPlane to DX Plane
        /// </summary>
        /// <param name="plane">TGCPlane to become into Plane</param>
        public static implicit operator Plane(TGCPlane plane)
        {
            return plane.ToPlane();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCPlane to the left of the equality operator.</param>
        /// <param name="right">The TGCPlane to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCPlane left, TGCPlane right)
        {
            return left.ToPlane() == right.ToPlane();
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCPlane to the left of the inequality operator.</param>
        /// <param name="right">The TGCPlane to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCPlane left, TGCPlane right)
        {
            return left.ToPlane() != right.ToPlane();
        }

        /// <summary>
        /// Scales the plane with a given scaling factor.
        /// </summary>
        /// <param name="s">Scale factor.</param>
        public void Scale(float s)
        {
            this.dxPlane.Scale(s);
        }

        /// <summary>
        /// Scales the plane with a given scaling factor.
        /// </summary>
        /// <param name="p">Pointer to the source TGCPlane.</param>
        /// <param name="s">Scale factor.</param>
        /// <returns>The Plane structure that represents the scaled plane.</returns>
        public static TGCPlane Scale(TGCPlane p, float s)
        {
            return new TGCPlane(Plane.Scale(p.ToPlane(), s));
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return this.dxPlane.ToString();
        }

        /// <summary>
        /// Transforms a plane by a matrix.
        /// </summary>
        /// <param name="m">Source TGCMatrix, which contains the transformation values. This matrix must contain the inverse transpose of the transformation values.</param>
        public void Transform(TGCMatrix m)
        {
            this.dxPlane.Transform(m.ToMatrix());
        }

        /// <summary>
        /// Transforms a plane by a matrix.
        /// </summary>
        /// <param name="p">Input TGCPlane, which contains the plane to be transformed. The vector (a,b,c) that describes the plane must be normalized before this method is called.</param>
        /// <param name="m">Source TGCMatrix, which contains the transformation values. This matrix must contain the inverse transpose of the transformation values.</param>
        /// <returns>A TGCPlane that represents the transformed plane.</returns>
        public static TGCPlane Transform(TGCPlane p, TGCMatrix m)
        {
            return new TGCPlane(Plane.Transform(p.ToPlane(), m.ToMatrix()));
        }

        /// <summary>
        /// Retrieves the DXPlane.
        /// </summary>
        /// <returns></returns>
        private Plane ToPlane()
        {
            return this.dxPlane;
        }

        #region Old TGCVectorUtils

        /// <summary>
        ///     Convierte un float[4] a un TGCPlane
        /// </summary>
        public static TGCPlane Float4ArrayToPlane(float[] f)
        {
            return new TGCPlane(f[0], f[1], f[2], f[3]);
        }

        #endregion Old TGCVectorUtils
    }
}