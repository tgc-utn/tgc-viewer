using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Examples.Collision.ElipsoidCollision
{
    /// <summary>
    ///     Colisionador a base de triangulos
    /// </summary>
    public class TriangleMeshCollider : Collider
    {
        /// <summary>
        ///     Triangulos del Collider
        /// </summary>
        public Triangle[] Triangles { get; set; }

        /// <summary>
        ///     Crear Collider a partir de TgcMesh.
        ///     Los triangulos se calculan CounterClock-Wise.
        ///     Crea el BoundingSphere del Collider.
        /// </summary>
        /// <param name="mesh">TgcMesh</param>
        /// <returns>Collider creado</returns>
        public static TriangleMeshCollider fromMesh(TgcMesh mesh)
        {
            var collider = new TriangleMeshCollider();

            //Cargar triangulos
            var vertices = mesh.getVertexPositions();
            var triangleCount = vertices.Length / 3;
            collider.Triangles = new Triangle[triangleCount];
            for (var i = 0; i < triangleCount; i++)
            {
                //Invertir orden de vertices para que la normal quede CounterClock-Wise
                collider.Triangles[i] = new Triangle(
                    vertices[i * 3 + 2],
                    vertices[i * 3 + 1],
                    vertices[i * 3]
                    );
            }

            //Crear BoundingSphere
            collider.BoundingSphere = TgcBoundingSphere.computeFromMesh(mesh);

            return collider;
        }

        /// <summary>
        ///     Colisiona un Elipsoide en movimiento contra un conjunto de triangulos.
        ///     Si hay colision devuelve el instante t de colision, el punto q de colision y el vector normal n de la superficie
        ///     contra la que
        ///     se colisiona.
        ///     Todo se devuelve en Elipsoid space.
        ///     Pasa cada triangulo a Elipsoid space para hacer el testeo.
        /// </summary>
        /// <param name="eSphere">BoundingSphere de radio 1 en Elipsoid space</param>
        /// <param name="eMovementVector">movimiento en Elipsoid space</param>
        /// <param name="eRadius">radio del Elipsoide</param>
        /// <param name="movementSphere">
        ///     BoundingSphere que abarca el sphere en su punto de origen mas el sphere en su punto final
        ///     deseado
        /// </param>
        /// <param name="minT">Menor instante de colision, en Elipsoid space</param>
        /// <param name="minQ">Punto mas cercano de colision, en Elipsoid space</param>
        /// <param name="n">Vector normal de la superficie contra la que se colisiona</param>
        /// <returns>True si hay colision</returns>
        public override bool intersectMovingElipsoid(TgcBoundingSphere eSphere, TGCVector3 eMovementVector, TGCVector3 eRadius,
            TgcBoundingSphere movementSphere, out float minT, out TGCVector3 minQ, out TGCVector3 n)
        {
            minQ = TGCVector3.Empty;
            minT = float.MaxValue;
            n = TGCVector3.Empty;
            var collisionPlane = TGCPlane.Zero;

            //Colision contra cada triangulo del collider, quedarse con el menor
            TGCVector3 q;
            float t;
            for (var i = 0; i < Triangles.Length; i++)
            {
                var triangle = Triangles[i];

                //Primero hacer un Sphere-Sphere test
                if (TgcCollisionUtils.testSphereSphere(movementSphere, triangle.BoundingSphere))
                {
                    //Pasar triangle a Elipsoid Space
                    var eTriangle = new Triangle(TGCVector3.Div(triangle.A, eRadius), TGCVector3.Div(triangle.B, eRadius), TGCVector3.Div(triangle.C, eRadius), null);

                    //Interseccion Moving Sphere-Triangle
                    if (intersectMovingSphereTriangle(eSphere, eMovementVector, eTriangle, out t, out q))
                    {
                        if (t < minT)
                        {
                            minT = t;
                            minQ = q;
                            collisionPlane = triangle.Plane;
                        }
                    }
                }
            }

            if (minT != float.MaxValue)
            {
                n = TgcCollisionUtils.getPlaneNormal(collisionPlane);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Detectar colision entre una esfera que se mueve y un triangulo
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="movementVector">Vector de movimiento de la esferfa</param>
        /// <param name="triangle">Triangulo</param>
        /// <param name="collisionPoint">Menor punto de colision encontrado</param>
        /// <returns>True si hay colision</returns>
        private bool intersectMovingSphereTriangle(TgcBoundingSphere sphere, TGCVector3 movementVector, Triangle triangle,
            out float minT, out TGCVector3 collisionPoint)
        {
            float t;
            TGCVector3 q;
            collisionPoint = TGCVector3.Empty;
            minT = float.MaxValue;

            //Ver si la esfera en movimiento colisiona con el plano del triangulo
            if (!TgcCollisionUtils.intersectMovingSpherePlane(sphere, movementVector, triangle.Plane, out t, out q))
            {
                return false;
            }

            //Ver si la esfera ya esta dentro del Plano, hacer un chequeo Sphere-Triangle
            if (t == 0.0f)
            {
                if (TgcCollisionUtils.testSphereTriangle(sphere, triangle.A, triangle.B, triangle.C, out q))
                {
                    minT = 0.0f;
                    collisionPoint = q;
                    return true;
                }
            }
            else
            {
                //Si el punto de colision contra el plano pertenece al triangulo, entonces ya encontramos el punto de colision
                if (TgcCollisionUtils.testPointInTriangle(q, triangle.A, triangle.B, triangle.C))
                {
                    minT = t;
                    collisionPoint = q;
                    return true;
                }
            }

            //Ver de que lado del plano del triangulo esta la esfera
            var distPlane = triangle.Plane.Dot(sphere.Center);
            var sphereRad = distPlane >= 0.0f ? sphere.Radius : -sphere.Radius;
            var planeNormal = TgcCollisionUtils.getPlaneNormal(triangle.Plane);

            //Chequear colision entre la esfera en movimiento y los tres Edge y obtener el menor punto de colision
            //Es como un Ray del centro de la esfera contra un edge que se convierte en cilindro sin endcap
            var segmentEnd = sphere.Center + movementVector;
            if (intersectSegmentCylinderNoEndcap(sphere.Center, segmentEnd, triangle.A, triangle.B, sphere.Radius, out t))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            if (intersectSegmentCylinderNoEndcap(sphere.Center, segmentEnd, triangle.B, triangle.C, sphere.Radius, out t))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            if (intersectSegmentCylinderNoEndcap(sphere.Center, segmentEnd, triangle.C, triangle.A, sphere.Radius, out t))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            //Si hubo colision, retornar la menor encontrada
            if (minT != float.MaxValue)
            {
                collisionPoint = sphere.Center + minT * movementVector - sphereRad * planeNormal;
                return true;
            }

            //Sino, chequear colision entra la esfera y los tres vertices del triangulo y obtener la menor
            //Es como un Ray del centro de la esfera contra un vertice que se convierte en esfera
            var vertSphere = new TgcBoundingSphere();
            minT = float.MaxValue;
            vertSphere.setValues(triangle.A, sphere.Radius);
            if (TgcCollisionUtils.intersectSegmentSphere(sphere.Center, segmentEnd, vertSphere, out t, out q))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            vertSphere.setValues(triangle.B, sphere.Radius);
            if (TgcCollisionUtils.intersectSegmentSphere(sphere.Center, segmentEnd, vertSphere, out t, out q))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            vertSphere.setValues(triangle.C, sphere.Radius);
            if (TgcCollisionUtils.intersectSegmentSphere(sphere.Center, segmentEnd, vertSphere, out t, out q))
            {
                minT = TgcCollisionUtils.min(t, minT);
            }
            //Si hubo colision, retornar la menor encontrada
            if (minT != float.MaxValue)
            {
                collisionPoint = sphere.Center + minT * movementVector - sphereRad * planeNormal;
                return true;
            }

            //No hay colision
            return false;
        }

        /// <summary>
        ///     Indica si un cilindro colisiona con un segmento.
        ///     El cilindro se especifica con dos puntos centrales "cylinderInit" y "cylinderEnd" que forman una recta y con un
        ///     radio "radius".
        ///     Si hay colision se devuelve el instante de colision "t".
        ///     No chequear EndCaps
        /// </summary>
        /// <param name="segmentInit">Punto de inicio del segmento</param>
        /// <param name="segmentEnd">Punto de fin del segmento</param>
        /// <param name="cylinderInit">Punto inicial del cilindro</param>
        /// <param name="cylinderEnd">Punto final del cilindro</param>
        /// <param name="radius">Radio del cilindro</param>
        /// <param name="t">Instante de colision</param>
        /// <returns>True si hay colision</returns>
        private static bool intersectSegmentCylinderNoEndcap(TGCVector3 segmentInit, TGCVector3 segmentEnd,
            TGCVector3 cylinderInit, TGCVector3 cylinderEnd, float radius, out float t)
        {
            t = -1;

            TGCVector3 d = cylinderEnd - cylinderInit, m = segmentInit - cylinderInit, n = segmentEnd - segmentInit;
            var md = TGCVector3.Dot(m, d);
            var nd = TGCVector3.Dot(n, d);
            var dd = TGCVector3.Dot(d, d);
            // Test if segment fully outside either endcap of cylinder
            if (md < 0.0f && md + nd < 0.0f) return false; // Segment outside ’p’ side of cylinder
            if (md > dd && md + nd > dd) return false; // Segment outside ’q’ side of cylinder
            var nn = TGCVector3.Dot(n, n);
            var mn = TGCVector3.Dot(m, n);
            var a = dd * nn - nd * nd;
            var k = TGCVector3.Dot(m, m) - radius * radius;
            var c = dd * k - md * md;
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
            var b = dd * mn - nd * md;
            var discr = b * b - a * c;
            if (discr < 0.0f) return false; // No real roots; no intersection
            t = (-b - FastMath.Sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f) return false; // Intersection lies outside segment

            /* No chequear EndCaps

            if (md + t * nd < 0.0f) {
                // Intersection outside cylinder on 'p' side
                if (nd <= 0.0f) return false; // Segment pointing away from endcap
                t = -md / nd;
                // Keep intersection if Dot(S(t) - p, S(t) - p) <= r^2
                return k + t * (2.0f * mn + t * nn) <= 0.0f;
            } else if (md + t * nd > dd) {
                // Intersection outside cylinder on 'q' side
                if (nd >= 0.0f) return false; // Segment pointing away from endcap
                t = (dd - md) / nd;
                // Keep intersection if Dot(S(t) - q, S(t) - q) <= r^2
                return k + dd - 2.0f * md + t * (2.0f * (mn - nd) + t * nn) <= 0.0f;
            }
            */

            // Segment intersects cylinder between the endcaps; t is correct
            return true;
        }

        /// <summary>
        ///     Triangulo del Collider
        /// </summary>
        public class Triangle
        {
            /// <summary>
            ///     Crear triangulo.
            ///     Calcula su plano y BoundingSphere
            /// </summary>
            public Triangle(TGCVector3 a, TGCVector3 b, TGCVector3 c)
            {
                A = a;
                B = b;
                C = c;
                Plane = TGCPlane.FromPoints(a, b, c);
                BoundingSphere = TgcBoundingSphere.computeFromPoints(new[] { a, b, c }).toClass();
            }

            /// <summary>
            ///     Crear triangulo.
            ///     Calcula su plano
            /// </summary>
            public Triangle(TGCVector3 a, TGCVector3 b, TGCVector3 c, TgcBoundingSphere sphere)
            {
                A = a;
                B = b;
                C = c;
                Plane = TGCPlane.FromPoints(a, b, c);
                BoundingSphere = sphere;
            }

            /// <summary>
            ///     Vertice A
            /// </summary>
            public TGCVector3 A { get; set; }

            /// <summary>
            ///     Vertice B
            /// </summary>
            public TGCVector3 B { get; set; }

            /// <summary>
            ///     Vertice C
            /// </summary>
            public TGCVector3 C { get; set; }

            /// <summary>
            ///     Ecuacion del plano del triangulo
            /// </summary>
            public TGCPlane Plane { get; set; }

            /// <summary>
            ///     BoundingSphere
            /// </summary>
            public TgcBoundingSphere BoundingSphere { get; set; }
        }
    }
}