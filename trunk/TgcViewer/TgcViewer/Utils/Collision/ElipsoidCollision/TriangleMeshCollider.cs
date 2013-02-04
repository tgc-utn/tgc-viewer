﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.Collision.ElipsoidCollision
{
    /// <summary>
    /// Colisionador a base de triangulos
    /// </summary>
    public class TriangleMeshCollider : Collider
    {

        Triangle[] triangles;
        /// <summary>
        /// Triangulos del Collider
        /// </summary>
        public Triangle[] Triangles
        {
            get { return triangles; }
            set { triangles = value; }
        }

        public TriangleMeshCollider() 
            : base()
        {
        }

        /// <summary>
        /// Crear Collider a partir de TgcMesh.
        /// Los triangulos se calculan CounterClock-Wise.
        /// Crea el BoundingSphere del Collider.
        /// </summary>
        /// <param name="mesh">TgcMesh</param>
        /// <returns>Collider creado</returns>
        public static TriangleMeshCollider fromMesh(TgcMesh mesh)
        {
            TriangleMeshCollider collider = new TriangleMeshCollider();

            //Cargar triangulos
            Vector3[] vertices = mesh.getVertexPositions();
            int triangleCount = vertices.Length / 3;
            collider.triangles = new Triangle[triangleCount];
            for (int i = 0; i < triangleCount; i++)
            {
                //Invertir orden de vertices para que la normal quede CounterClock-Wise
                collider.triangles[i] = new Triangle(
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
        /// Triangulo del Collider
        /// </summary>
        public class Triangle
        {
            Vector3 a;
            /// <summary>
            /// Vertice A
            /// </summary>
            public Vector3 A
            {
                get { return a; }
                set { a = value; }
            }

            Vector3 b;
            /// <summary>
            /// Vertice B
            /// </summary>
            public Vector3 B
            {
                get { return b; }
                set { b = value; }
            }

            Vector3 c;
            /// <summary>
            /// Vertice C
            /// </summary>
            public Vector3 C
            {
                get { return c; }
                set { c = value; }
            }

            Plane plane;
            /// <summary>
            /// Ecuacion del plano del triangulo
            /// </summary>
            public Plane Plane
            {
                get { return plane; }
                set { plane = value; }
            }


            TgcBoundingSphere boundingSphere;
            /// <summary>
            /// BoundingSphere
            /// </summary>
            public TgcBoundingSphere BoundingSphere
            {
                get { return boundingSphere; }
                set { boundingSphere = value; }
            }

            /// <summary>
            /// Crear triangulo.
            /// Calcula su plano y BoundingSphere
            /// </summary>
            public Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.plane = Plane.FromPoints(a, b, c);
                this.boundingSphere = TgcBoundingSphere.computeFromPoints(new Vector3[] { a, b, c }).toClass();
            }

            /// <summary>
            /// Crear triangulo.
            /// Calcula su plano
            /// </summary>
            public Triangle(Vector3 a, Vector3 b, Vector3 c, TgcBoundingSphere sphere)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.plane = Plane.FromPoints(a, b, c);
                this.boundingSphere = sphere;
            }

        }

        /// <summary>
        /// Colisiona un Elipsoide en movimiento contra un conjunto de triangulos.
        /// Si hay colision devuelve el instante t de colision, el punto q de colision y el vector normal n de la superficie contra la que
        /// se colisiona.
        /// Todo se devuelve en Elipsoid space.
        /// Pasa cada triangulo a Elipsoid space para hacer el testeo.
        /// </summary>
        /// <param name="eSphere">BoundingSphere de radio 1 en Elipsoid space</param>
        /// <param name="eMovementVector">movimiento en Elipsoid space</param>
        /// <param name="eRadius">radio del Elipsoide</param>
        /// <param name="movementSphere">BoundingSphere que abarca el sphere en su punto de origen mas el sphere en su punto final deseado</param>
        /// <param name="minT">Menor instante de colision, en Elipsoid space</param>
        /// <param name="minQ">Punto mas cercano de colision, en Elipsoid space</param>
        /// <param name="n">Vector normal de la superficie contra la que se colisiona</param>
        /// <returns>True si hay colision</returns>
        public override bool intersectMovingElipsoid(TgcBoundingSphere eSphere, Vector3 eMovementVector, Vector3 eRadius, TgcBoundingSphere movementSphere, out float minT, out Vector3 minQ, out Vector3 n)
        {
            minQ = Vector3.Empty;
            minT = float.MaxValue;
            n = Vector3.Empty;
            Plane collisionPlane = Plane.Empty;
            
            //Colision contra cada triangulo del collider, quedarse con el menor
            Vector3 q;
            float t;
            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle triangle = triangles[i];

                //Primero hacer un Sphere-Sphere test
                if (TgcCollisionUtils.testSphereSphere(movementSphere, triangle.BoundingSphere))
                {
                    //Pasar triangle a Elipsoid Space
                    Triangle eTriangle = new Triangle(
                        TgcVectorUtils.div(triangle.A, eRadius),
                        TgcVectorUtils.div(triangle.B, eRadius),
                        TgcVectorUtils.div(triangle.C, eRadius),
                        null
                        );

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
        /// Detectar colision entre una esfera que se mueve y un triangulo
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="movementVector">Vector de movimiento de la esferfa</param>
        /// <param name="triangle">Triangulo</param>
        /// <param name="collisionPoint">Menor punto de colision encontrado</param>
        /// <returns>True si hay colision</returns>
        private bool intersectMovingSphereTriangle(TgcBoundingSphere sphere, Vector3 movementVector, Triangle triangle, out float minT, out Vector3 collisionPoint)
        {
            float t;
            Vector3 q;
            collisionPoint = Vector3.Empty;
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
            float distPlane = triangle.Plane.Dot(sphere.Center);
            float sphereRad = distPlane >= 0.0f ? sphere.Radius : -sphere.Radius;
            Vector3 planeNormal = TgcCollisionUtils.getPlaneNormal(triangle.Plane);


            //Chequear colision entre la esfera en movimiento y los tres Edge y obtener el menor punto de colision
            //Es como un Ray del centro de la esfera contra un edge que se convierte en cilindro sin endcap
            Vector3 segmentEnd = sphere.Center + movementVector;
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
            TgcBoundingSphere vertSphere = new TgcBoundingSphere();
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
        private static bool intersectSegmentCylinderNoEndcap(Vector3 segmentInit, Vector3 segmentEnd, Vector3 cylinderInit, Vector3 cylinderEnd, float radius, out float t)
        {
            t = -1;

            Vector3 d = cylinderEnd - cylinderInit, m = segmentInit - cylinderInit, n = segmentEnd - segmentInit;
            float md = Vector3.Dot(m, d);
            float nd = Vector3.Dot(n, d);
            float dd = Vector3.Dot(d, d);
            // Test if segment fully outside either endcap of cylinder
            if (md < 0.0f && md + nd < 0.0f) return false; // Segment outside ’p’ side of cylinder
            if (md > dd && md + nd > dd) return false; // Segment outside ’q’ side of cylinder
            float nn = Vector3.Dot(n, n);
            float mn = Vector3.Dot(m, n);
            float a = dd * nn - nd * nd;
            float k = Vector3.Dot(m, m) - radius * radius;
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




    }
}
