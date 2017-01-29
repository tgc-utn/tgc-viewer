using Microsoft.DirectX;
using System;

namespace TGC.Core.Mathematica
{
    /// <summary>
    /// Describes and manipulates a plane.
    /// </summary>
    public struct TGCPlane
    {
        private static TGCPlane EMPTY = new TGCPlane(0f, 0f, 0f, 0f);
        
        /// <summary>
        /// Initializes a new instance of the Plane class.
        /// </summary>
        /// <param name="valuePointA">A Single value used to set the initial value of the A field.</param>
        /// <param name="valuePointB">A Single value used to set the initial value of the B field.</param>
        /// <param name="valuePointC">A Single value used to set the initial value of the C field.</param>
        /// <param name="valuePointD">A Single value used to set the initial value of the D field.</param>
        public TGCPlane(float valuePointA, float valuePointB, float valuePointC, float valuePointD)
        {
            this.DXPlane = new Plane(valuePointA, valuePointB, valuePointC, valuePointD);
            this.A = this.DXPlane.A;
            this.B = this.DXPlane.B;
            this.C = this.DXPlane.C;
            this.D = this.DXPlane.D;
        }

        /// <summary>
        /// Initializes a new instance of the Plane class.
        /// </summary>
        /// <param name="dxPlane">A plane DXPlane.</param>
        public TGCPlane(Plane dxPlane)
        {
            this.DXPlane = dxPlane;
            this.A = this.DXPlane.A;
            this.B = this.DXPlane.B;
            this.C = this.DXPlane.C;
            this.D = this.DXPlane.D;
        }

        private Plane DXPlane { get; set; }

        /// <summary>
        /// Retrieves an empty plane.
        /// </summary>
        public static TGCPlane Empty { get { return EMPTY; } }

        /// <summary>
        /// Retrieves or sets the 'A' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float A { get; set; }

        /// <summary>
        /// Retrieves or sets the 'B' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float B { get; set; }

        /// <summary>
        /// Retrieves or sets the 'C' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float C { get; set; }

        /// <summary>
        /// Retrieves or sets the 'D' coefficient of the clipping plane in the general plane equation.
        /// </summary>
        public float D { get; set; }


        /// <summary>        
        /// Retrieves the DXPlane.
        /// </summary>
        /// <returns></returns>
        public Plane ToPlane()
        {
            return this.DXPlane;
        }

        /// <summary>
        /// Computes the dot product of a plane and a vector.
        /// </summary>
        /// <param name="v">Source TGCVector3.</param>
        /// <returns>A Single value that is the dot product of the plane and the vector.</returns>
        public float Dot(TGCVector3 v)
        {
            return this.DXPlane.Dot(v.ToVector3());            
        }
        
        /// <summary>
        /// Computes the dot product of a plane and a vector.
        /// </summary>
        /// <param name="v">Source Vector4 structure.</param>
        /// <returns>A Single value that is the dot product of the plane and the vector.</returns>
        public float Dot(TGCVector4 v)
        {
            return this.DXPlane.Dot(v.ToVector4());
        }

        /// <summary>
        /// Computes the dot product of a plane and a 3-D vector. The w parameter of the vector is assumed to be 0.
        /// </summary>
        /// <param name="p">Source TGCPlane.</param>
        /// <param name="v">Source TGCVector3.</param>
        /// <returns>A Single value that represents the dot product of the plane and the 3-D vector.</returns>
        public static float DotNormal(TGCPlane p, TGCVector3 v)
        {
            return Plane.DotNormal(p.ToPlane(), v.ToVector3());
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to a specified object.
        /// </summary>
        /// <param name="compare">Object with which to make the comparison.</param>
        /// <returns>Value that is true if the current instance is equal to the specified object, or false if it is not.</returns>
        public override bool Equals(object compare)
        {
            if (compare is TGCPlane)
            {
                TGCPlane other = ((TGCPlane)compare);
                return (A == other.A) && (B == other.B) && (C == other.C) && (D == other.D);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        /// <returns>Hash code for the instance.</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + A.GetHashCode();
                hash = hash * 23 + B.GetHashCode();
                hash = hash * 23 + C.GetHashCode();
                hash = hash * 23 + D.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Constructs a plane from a point and a normal.
        /// </summary>
        /// <param name="point">A TGCVector3 that defines the point used to construct the plane.</param>
        /// <param name="normal">A TGCVector3 that defines the normal used to construct the plane.</param>
        /// <returns>A TGCPlane constructed from the point and the normal.</returns>
        public static TGCPlane FromPointNormal(TGCVector3 point, TGCVector3 normal)
        {
            return new TGCPlane(Plane.FromPointNormal(point.ToVector3(), normal.ToVector3()));
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
            return new TGCPlane(Plane.FromPoints(p1.ToVector3(), p1.ToVector3(), p3.ToVector3()));
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
            return new TGCVector3(Plane.IntersectLine(p.ToPlane(), v1.ToVector3(), v2.ToVector3()));
        }

        /// <summary>
        /// Returns the normal of a plane.
        /// </summary>
        public void Normalize()
        {
            this.DXPlane.Normalize();
            this.A = this.DXPlane.A;
            this.B = this.DXPlane.B;
            this.C = this.DXPlane.C;
            this.D = this.DXPlane.D;
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
        /// Compares the current instance of a class to another instance to determine whether they are the same.
        /// </summary>
        /// <param name="left">The TGCPlane to the left of the equality operator.</param>
        /// <param name="right">The TGCPlane to the right of the equality operator.</param>
        /// <returns>Value that is true if the objects are the same, or false if they are different.</returns>
        public static bool operator ==(TGCPlane left, TGCPlane right)
        {
            //TODO NPE???? deveriamos validar?
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the current instance of a class to another instance to determine whether they are different.
        /// </summary>
        /// <param name="left">The TGCPlane to the left of the inequality operator.</param>
        /// <param name="right">The TGCPlane to the right of the inequality operator.</param>
        /// <returns>Value that is true if the objects are different, or false if they are the same.</returns>
        public static bool operator !=(TGCPlane left, TGCPlane right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Scales the plane with a given scaling factor.
        /// </summary>
        /// <param name="s">Scale factor.</param>
        public void Scale(float s)
        {
            this.DXPlane.Scale(s);
            this.A = this.DXPlane.A;
            this.B = this.DXPlane.B;
            this.C = this.DXPlane.C;
            this.D = this.DXPlane.D;
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
            return "[" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", A) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", B) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", C) + "," +
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.####}", D) + "]";
        }

        /// <summary>
        /// Transforms a plane by a matrix.
        /// </summary>
        /// <param name="m">Source TGCMatrix, which contains the transformation values. This matrix must contain the inverse transpose of the transformation values.</param>
        public void Transform(TGCMatrix m)
        {
            this.DXPlane = Plane.Transform(this.ToPlane(), m.ToMatrix());
            this.A = this.DXPlane.A;
            this.B = this.DXPlane.B;
            this.C = this.DXPlane.C;
            this.D = this.DXPlane.D;
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

        #region Old TGCVectorUtils

        /// <summary>
        ///     Convierte un float[4] a un TGCPlane
        /// </summary>
        public static TGCPlane Float4ArrayToPlane(float[] f)
        {
            return new TGCPlane(f[0], f[1], f[2], f[3]);
        }

        #endregion
    }
}
