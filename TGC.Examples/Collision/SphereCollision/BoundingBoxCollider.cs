using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;

namespace TGC.Examples.Collision.SphereCollision
{
    /// <summary>
    ///     Collider BoundingBox
    /// </summary>
    public class BoundingBoxCollider : Collider
    {
        /// <summary>
        ///     BoundingBox
        /// </summary>
        public TgcBoundingAxisAlignBox Aabb { get; set; }

        /// <summary>
        ///     Crear Collider a partir de BoundingBox.
        ///     Crea el BoundingSphere del Collider.
        /// </summary>
        /// <param name="mesh">BoundingBox</param>
        /// <returns>Collider creado</returns>
        public static BoundingBoxCollider fromBoundingBox(TgcBoundingAxisAlignBox aabb)
        {
            var collider = new BoundingBoxCollider();
            collider.Aabb = aabb;
            collider.BoundingSphere = TgcBoundingSphere.computeFromPoints(aabb.computeCorners()).toClass();
            return collider;
        }

        /// <summary>
        ///     Colisiona un BoundingSphere en movimiento contra el BoundingBox.
        ///     Si hay colision devuelve el instante t de colision mas proximo y el punto q de colision mas cercano
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="movementVector">movimiento del BoundingSphere</param>
        /// <param name="t">Menor instante de colision</param>
        /// <param name="q">Punto mas cercano de colision</param>
        /// <param name="n">Normal de la cara colisionada</param>
        /// <returns>True si hay colision</returns>
        public override bool intersectMovingSphere(TgcBoundingSphere sphere, TGCVector3 movementVector,
            TgcBoundingSphere movementSphere, out float t, out TGCVector3 q, out TGCVector3 n)
        {
            t = -1f;
            q = TGCVector3.Empty;
            n = TGCVector3.Empty;

            // Compute the AABB resulting from expanding b by sphere radius r
            var e = Aabb.toStruct();
            e.min.X -= sphere.Radius;
            e.min.Y -= sphere.Radius;
            e.min.Z -= sphere.Radius;
            e.max.X += sphere.Radius;
            e.max.Y += sphere.Radius;
            e.max.Z += sphere.Radius;

            // Intersect ray against expanded AABB e. Exit with no intersection if ray
            // misses e, else get intersection point p and time t as result
            TGCVector3 p;
            var ray = new TgcRay.RayStruct();
            ray.origin = sphere.Center;
            ray.direction = movementVector;
            if (!intersectRayAABB(ray, e, out t, out p) || t > 1.0f)
                return false;

            // Compute which min and max faces of b the intersection point p lies
            // outside of. Note, u and v cannot have the same bits set and
            // they must have at least one bit set among them
            var i = 0;
            var sign = new int[3];
            if (p.X < Aabb.PMin.X)
            {
                sign[0] = -1;
                i++;
            }
            if (p.X > Aabb.PMax.X)
            {
                sign[0] = 1;
                i++;
            }
            if (p.Y < Aabb.PMin.Y)
            {
                sign[1] = -1;
                i++;
            }
            if (p.Y > Aabb.PMax.Y)
            {
                sign[1] = 1;
                i++;
            }
            if (p.Z < Aabb.PMin.Z)
            {
                sign[2] = -1;
                i++;
            }
            if (p.Z > Aabb.PMax.Z)
            {
                sign[2] = 1;
                i++;
            }

            //Face
            if (i == 1)
            {
                n = new TGCVector3(sign[0], sign[1], sign[2]);
                q = sphere.Center + t * movementVector - sphere.Radius * n;
                return true;
            }

            // Define line segment [c, c+d] specified by the sphere movement
            var seg = new Segment(sphere.Center, sphere.Center + movementVector);

            //Box extent and center
            var extent = Aabb.calculateAxisRadius();
            var center = Aabb.PMin + extent;

            //Edge
            if (i == 2)
            {
                //Generar los dos puntos extremos del Edge
                float[] extentDir = { sign[0], sign[1], sign[2] };
                var zeroIndex = sign[0] == 0 ? 0 : (sign[1] == 0 ? 1 : 2);
                extentDir[zeroIndex] = 1;
                var capsuleA = center + new TGCVector3(extent.X * extentDir[0], extent.Y * extentDir[1], extent.Z * extentDir[2]);
                extentDir[zeroIndex] = -1;
                var capsuleB = center + new TGCVector3(extent.X * extentDir[0], extent.Y * extentDir[1], extent.Z * extentDir[2]);

                //Colision contra el Edge hecho Capsula
                if (intersectSegmentCapsule(seg, new Capsule(capsuleA, capsuleB, sphere.Radius), out t))
                {
                    n = new TGCVector3(sign[0], sign[1], sign[2]);
                    n.Normalize();
                    q = sphere.Center + t * movementVector - sphere.Radius * n;
                    return true;
                }
            }

            //Vertex
            if (i == 3)
            {
                var tmin = float.MaxValue;
                var capsuleA = center + new TGCVector3(extent.X * sign[0], extent.Y * sign[1], extent.Z * sign[2]);
                TGCVector3 capsuleB;

                capsuleB = center + new TGCVector3(extent.X * -sign[0], extent.Y * sign[1], extent.Z * sign[2]);
                if (intersectSegmentCapsule(seg, new Capsule(capsuleA, capsuleB, sphere.Radius), out t))
                    tmin = TgcCollisionUtils.min(t, tmin);

                capsuleB = center + new TGCVector3(extent.X * sign[0], extent.Y * -sign[1], extent.Z * sign[2]);
                if (intersectSegmentCapsule(seg, new Capsule(capsuleA, capsuleB, sphere.Radius), out t))
                    tmin = TgcCollisionUtils.min(t, tmin);

                capsuleB = center + new TGCVector3(extent.X * sign[0], extent.Y * sign[1], extent.Z * -sign[2]);
                if (intersectSegmentCapsule(seg, new Capsule(capsuleA, capsuleB, sphere.Radius), out t))
                    tmin = TgcCollisionUtils.min(t, tmin);

                if (tmin == float.MaxValue) return false; // No intersection

                t = tmin;
                n = new TGCVector3(sign[0], sign[1], sign[2]);
                n.Normalize();
                q = sphere.Center + t * movementVector - sphere.Radius * n;
                return true; // Intersection at time t == tmin
            }

            return false;
        }

        /// <summary>
        ///     Detectar colision entre un Segmento de recta y una Capsula.
        /// </summary>
        /// <param name="seg">Segmento</param>
        /// <param name="capsule">Capsula</param>
        /// <param name="t">Menor instante de colision</param>
        /// <returns>True si hay colision</returns>
        private bool intersectSegmentCapsule(Segment seg, Capsule capsule, out float t)
        {
            var ray = new TgcRay.RayStruct();
            ray.origin = seg.a;
            ray.direction = seg.dir;

            if (intersectRayCapsule(ray, capsule, out t))
            {
                if (t >= 0.0f && t <= seg.length)
                {
                    t /= seg.length;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Detectar colision entre un Ray y una Capsula
        ///     Basado en: http://www.geometrictools.com/LibMathematics/Intersection/Wm5IntrLine3Capsule3.cpp
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="capsule">Capsula</param>
        /// <param name="t">Menor instante de colision</param>
        /// <returns>True si hay colision</returns>
        private bool intersectRayCapsule(TgcRay.RayStruct ray, Capsule capsule, out float t)
        {
            t = -1;
            var origin = ray.origin;
            var dir = ray.direction;

            // Create a coordinate system for the capsule.  In this system, the
            // capsule segment center C is the origin and the capsule axis direction
            // W is the z-axis.  U and V are the other coordinate axis directions.
            // If P = x*U+y*V+z*W, the cylinder containing the capsule plane is
            // x^2 + y^2 = r^2, where r is the capsule radius.  The finite cylinder
            // that makes up the capsule minus its hemispherical end caps has z-values
            // |z| <= e, where e is the extent of the capsule segment.  The top
            // hemisphere cap is x^2+y^2+(z-e)^2 = r^2 for z >= e, and the bottom
            // hemisphere cap is x^2+y^2+(z+e)^2 = r^2 for z <= -e.
            var U = capsule.segment.dir;
            var V = U;
            var W = U;
            generateComplementBasis(ref U, ref V, W);
            var rSqr = capsule.radius * capsule.radius;
            var extent = capsule.segment.extent;

            // Convert incoming line origin to capsule coordinates.
            var diff = origin - capsule.segment.center;
            var P = new TGCVector3(TGCVector3.Dot(U, diff), TGCVector3.Dot(V, diff), TGCVector3.Dot(W, diff));

            // Get the z-value, in capsule coordinates, of the incoming line's unit-length direction.
            var dz = TGCVector3.Dot(W, dir);
            if (FastMath.Abs(dz) >= 1f - float.Epsilon)
            {
                // The line is parallel to the capsule axis.  Determine whether the line intersects the capsule hemispheres.
                var radialSqrDist = rSqr - P.X * P.X - P.Y * P.Y;
                if (radialSqrDist < 0f)
                {
                    // Line outside the cylinder of the capsule, no intersection.
                    return false;
                }

                // line intersects the hemispherical caps
                var zOffset = FastMath.Sqrt(radialSqrDist) + extent;
                if (dz > 0f)
                {
                    t = -P.Z - zOffset;
                }
                else
                {
                    t = P.Z - zOffset;
                }
                return true;
            }

            // Convert incoming line unit-length direction to capsule coordinates.
            var D = new TGCVector3(TGCVector3.Dot(U, dir), TGCVector3.Dot(V, dir), dz);

            // Test intersection of line P+t*D with infinite cylinder x^2+y^2 = r^2.
            // This reduces to computing the roots of a quadratic equation.  If
            // P = (px,py,pz) and D = (dx,dy,dz), then the quadratic equation is
            //   (dx^2+dy^2)*t^2 + 2*(px*dx+py*dy)*t + (px^2+py^2-r^2) = 0
            var a0 = P.X * P.X + P.Y * P.Y - rSqr;
            var a1 = P.X * D.X + P.Y * D.Y;
            var a2 = D.X * D.X + D.Y * D.Y;
            var discr = a1 * a1 - a0 * a2;
            if (discr < 0f)
            {
                // Line does not intersect infinite cylinder.
                return false;
            }

            float root, inv, tValue, zValue;
            var quantity = 0;
            if (discr > float.Epsilon)
            {
                // Line intersects infinite cylinder in two places.
                root = FastMath.Sqrt(discr);
                inv = 1f / a2;
                tValue = (-a1 - root) * inv;
                zValue = P.Z + tValue * D.Z;
                if (FastMath.Abs(zValue) <= extent)
                {
                    quantity++;
                    t = tValue;
                }

                tValue = (-a1 + root) * inv;
                zValue = P.Z + tValue * D.Z;
                if (FastMath.Abs(zValue) <= extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                }

                if (quantity == 2)
                {
                    // Line intersects capsule plane in two places.
                    return true;
                }
            }
            else
            {
                // Line is tangent to infinite cylinder.
                tValue = -a1 / a2;
                zValue = P.Z + tValue * D.Z;
                if (FastMath.Abs(zValue) <= extent)
                {
                    t = tValue;
                    return true;
                }
            }

            // Test intersection with bottom hemisphere.  The quadratic equation is
            // t^2 + 2*(px*dx+py*dy+(pz+e)*dz)*t + (px^2+py^2+(pz+e)^2-r^2) = 0
            // Use the fact that currently a1 = px*dx+py*dy and a0 = px^2+py^2-r^2.
            // The leading coefficient is a2 = 1, so no need to include in the
            // construction.
            var PZpE = P.Z + extent;
            a1 += PZpE * D.Z;
            a0 += PZpE * PZpE;
            discr = a1 * a1 - a0;
            if (discr > float.Epsilon)
            {
                root = FastMath.Sqrt(discr);
                tValue = -a1 - root;
                zValue = P.Z + tValue * D.Z;
                if (zValue <= -extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }

                tValue = -a1 + root;
                zValue = P.Z + tValue * D.Z;
                if (zValue <= -extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }
            }
            else if (FastMath.Abs(discr) <= float.Epsilon)
            {
                tValue = -a1;
                zValue = P.Z + tValue * D.Z;
                if (zValue <= -extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }
            }

            // Test intersection with top hemisphere.  The quadratic equation is
            // t^2 + 2*(px*dx+py*dy+(pz-e)*dz)*t + (px^2+py^2+(pz-e)^2-r^2) = 0
            // Use the fact that currently a1 = px*dx+py*dy+(pz+e)*dz and
            // a0 = px^2+py^2+(pz+e)^2-r^2.  The leading coefficient is a2 = 1, so
            // no need to include in the construction.
            a1 -= 2f * extent * D.Z;
            a0 -= 4 * extent * P.Z;
            discr = a1 * a1 - a0;
            if (discr > float.Epsilon)
            {
                root = FastMath.Sqrt(discr);
                tValue = -a1 - root;
                zValue = P.Z + tValue * D.Z;
                if (zValue >= extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }

                tValue = -a1 + root;
                zValue = P.Z + tValue * D.Z;
                if (zValue >= extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }
            }
            else if (FastMath.Abs(discr) <= float.Epsilon)
            {
                tValue = -a1;
                zValue = P.Z + tValue * D.Z;
                if (zValue >= extent)
                {
                    quantity++;
                    t = TgcCollisionUtils.min(t, tValue);
                    if (quantity == 2)
                    {
                        return true;
                    }
                }
            }

            return quantity > 0;
        }

        /// <summary>
        ///     Input W must be a unit-length vector.  The output vectors {U,V} are
        ///     unit length and mutually perpendicular, and {U,V,W} is an orthonormal basis.
        /// </summary>
        private void generateComplementBasis(ref TGCVector3 u, ref TGCVector3 v, TGCVector3 w)
        {
            float invLength;

            if (FastMath.Abs(w.X) >= FastMath.Abs(w.Y))
            {
                // W.x or W.z is the largest magnitude component, swap them
                invLength = FastMath.InvSqrt(w.X * w.X + w.Z * w.Z);
                u.X = -w.Z * invLength;
                u.Y = 0f;
                u.Z = +w.X * invLength;
                v.X = w.Y * u.Z;
                v.Y = w.Z * u.X - w.X * u.Z;
                v.Z = -w.Y * u.X;
            }
            else
            {
                // W.y or W.z is the largest magnitude component, swap them
                invLength = FastMath.InvSqrt(w.Y * w.Y + w.Z * w.Z);
                u.X = 0f;
                u.Y = +w.Z * invLength;
                u.Z = -w.Y * invLength;
                v.X = w.Y * u.Z - w.Z * u.Y;
                v.Y = -w.X * u.Z;
                v.Z = w.X * u.Y;
            }
        }

        /// <summary>
        ///     Interseccion entre un Ray y un AABB
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="aabb">AABB</param>
        /// <param name="tmin">Instante minimo de colision</param>
        /// <param name="q">Punto minimo de colision</param>
        /// <returns>True si hay colision</returns>
        private bool intersectRayAABB(TgcRay.RayStruct ray, TgcBoundingAxisAlignBox.AABBStruct aabb, out float tmin,
            out TGCVector3 q)
        {
            var aabbMin = TgcCollisionUtils.toArray(aabb.min);
            var aabbMax = TgcCollisionUtils.toArray(aabb.max);
            var p = TgcCollisionUtils.toArray(ray.origin);
            var d = TgcCollisionUtils.toArray(ray.direction);

            tmin = 0.0f; // set to -FLT_MAX to get first hit on line
            var tmax = float.MaxValue; // set to max distance ray can travel (for segment)
            q = TGCVector3.Empty;

            // For all three slabs
            for (var i = 0; i < 3; i++)
            {
                if (FastMath.Abs(d[i]) < float.Epsilon)
                {
                    // Ray is parallel to slab. No hit if origin not within slab
                    if (p[i] < aabbMin[i] || p[i] > aabbMax[i]) return false;
                }
                else
                {
                    // Compute intersection t value of ray with near and far plane of slab
                    var ood = 1.0f / d[i];
                    var t1 = (aabbMin[i] - p[i]) * ood;
                    var t2 = (aabbMax[i] - p[i]) * ood;
                    // Make t1 be intersection with near plane, t2 with far plane
                    if (t1 > t2) TgcCollisionUtils.swap(ref t1, ref t2);
                    // Compute the intersection of slab intersection intervals
                    tmin = TgcCollisionUtils.max(tmin, t1);
                    tmax = TgcCollisionUtils.min(tmax, t2);
                    // Exit with no collision as soon as slab intersection becomes empty
                    if (tmin > tmax) return false;
                }
            }
            // Ray intersects all 3 slabs. Return point (q) and intersection t value (tmin)
            q = ray.origin + ray.direction * tmin;
            return true;
        }

        /// <summary>
        ///     Estructura para segmento de recta
        /// </summary>
        private struct Segment
        {
            public Segment(TGCVector3 a, TGCVector3 b)
            {
                this.a = a;
                this.b = b;

                dir = b - a;
                center = (a + b) * 0.5f;
                length = dir.Length();
                extent = length * 0.5f;
                dir.Normalize();
            }

            public readonly TGCVector3 a;
            public TGCVector3 b;

            public readonly TGCVector3 center;
            public readonly TGCVector3 dir;
            public readonly float extent;
            public readonly float length;
        }

        /// <summary>
        ///     Estructura para Capsula
        /// </summary>
        private struct Capsule
        {
            public Capsule(TGCVector3 a, TGCVector3 b, float radius)
            {
                segment = new Segment(a, b);
                this.radius = radius;
            }

            public Segment segment;
            public readonly float radius;
        }

        /*
        private bool intersectSegmentCapsule(Segment seg, Capsule capsule, out float t)
        {
            TgcRay.RayStruct ray = new TgcRay.RayStruct();
            ray.origin = seg.a;
            ray.direction = seg.b - seg.a;

            float minT = float.MaxValue;
            if (intersectSegmentCylinderNoEndcap(seg.a, seg.b, capsule.segment.a, capsule.segment.b, capsule.radius, out t))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }

            TgcBoundingSphere.SphereStruct s = new TgcBoundingSphere.SphereStruct();
            s.center = capsule.segment.a;
            s.radius = capsule.radius;
            if (intersectRaySphere(ray, s, out t) && t <= 1.0f)
            {
                minT = TgcCollisionUtils.min(t, minT);
            }

            s.center = capsule.segment.b;
            if (intersectRaySphere(ray, s, out t) && t <= 1.0f)
            {
                minT = TgcCollisionUtils.min(t, minT);
            }

            if (minT != float.MaxValue)
            {
                t = minT;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indica si un Ray colisiona con un BoundingSphere.
        /// </summary>
        public static bool intersectRaySphere(TgcRay.RayStruct ray, TgcBoundingSphere.SphereStruct sphere, out float t)
        {
            t = -1;

            TGCVector3 m = ray.origin - sphere.center;
            float b = TGCVector3.Dot(m, ray.direction);
            float c = TGCVector3.Dot(m, m) - sphere.radius * sphere.radius;
            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0)
            if (c > 0.0f && b > 0.0f) return false;
            float discr = b * b - c;
            // A negative discriminant corresponds to ray missing sphere
            if (discr < 0.0f) return false;
            // Ray now found to intersect sphere, compute smallest t value of intersection
            t = -b - FastMath.Sqrt(discr);
            // If t is negative, ray started inside sphere so clamp t to zero
            if (t < 0.0f) t = 0.0f;
            return true;
        }

        /// <summary>
        /// Indica si un cilindro colisiona con un segmento.
        /// El cilindro se especifica con dos puntos centrales "cylinderInit" y "cylinderEnd" que forman una recta y con un radio "radius".
        /// Si hay colision se devuelve el instante de colision "t".
        /// No chequear EndCaps
        /// </summary>
        /// <param name="segmentInit">Punto de inicio del segmento</param>
        /// <param name="segmentEnd">Punto de fin del segmento</param>
        /// <param name="cylinderInit">Punto inicial del cilindro</param>
        /// <param name="cylinderEnd">Punto final del cilindro</param>
        /// <param name="radius">Radio del cilindro</param>
        /// <param name="t">Instante de colision</param>
        /// <returns>True si hay colision</returns>
        private static bool intersectSegmentCylinderNoEndcap(TGCVector3 segmentInit, TGCVector3 segmentEnd, TGCVector3 cylinderInit, TGCVector3 cylinderEnd, float radius, out float t)
        {
            t = -1;

            TGCVector3 d = cylinderEnd - cylinderInit, m = segmentInit - cylinderInit, n = segmentEnd - segmentInit;
            float md = TGCVector3.Dot(m, d);
            float nd = TGCVector3.Dot(n, d);
            float dd = TGCVector3.Dot(d, d);
            // Test if segment fully outside either endcap of cylinder
            if (md < 0.0f && md + nd < 0.0f) return false; // Segment outside ’p’ side of cylinder
            if (md > dd && md + nd > dd) return false; // Segment outside ’q’ side of cylinder
            float nn = TGCVector3.Dot(n, n);
            float mn = TGCVector3.Dot(m, n);
            float a = dd * nn - nd * nd;
            float k = TGCVector3.Dot(m, m) - radius * radius;
            float c = dd * k - md * md;
            if (FastMath.Abs(a) < float.Epsilon)
            {
                // Segment runs parallel to cylinder axis
                if (c > 0.0f) return false; // 'a' and thus the segment lie outside cylinder
                // Now known that segment intersects cylinder; figure out how it intersects
                if (md < 0.0f) t = -mn / nn; // Intersect segment against 'p' endcap
                else if (md > dd) t = (nd - mn) / nn; // Intersect segment against ’q’ endcap
                else t = 0.0f; // ’a’ lies inside cylinder
                return true;
            }
            float b = dd * mn - nd * md;
            float discr = b * b - a * c;
            if (discr < 0.0f) return false; // No real roots; no intersection
            t = (-b - FastMath.Sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f) return false; // Intersection lies outside segment

            //No chequear EndCaps

            // Segment intersects cylinder between the endcaps; t is correct
            return true;
        }
        */
    }
}