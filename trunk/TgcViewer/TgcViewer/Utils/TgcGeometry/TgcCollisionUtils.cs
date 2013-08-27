using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Utilidades para hacer detección de colisiones
    /// </summary>
    public class TgcCollisionUtils
    {

        #region BoundingBox

        /// <summary>
        /// Clasifica un BoundingBox respecto de otro. Las opciones de clasificacion son:
        /// <para># Adentro: box1 se encuentra completamente dentro de la box2</para>
        /// <para># Afuera: box2 se encuentra completamente afuera de box1</para>
        /// <para># Atravesando: box2 posee una parte dentro de box1 y otra parte fuera de la box1</para>
        /// <para># Encerrando: box1 esta completamente adentro a la box1, es decir, la box1 se encuentra dentro
        ///     de la box2. Es un caso especial de que box2 esté afuera de box1</para>
        /// </summary>
        public static BoxBoxResult classifyBoxBox(TgcBoundingBox box1, TgcBoundingBox box2)
        {
            if (((box1.PMin.X <= box2.PMin.X && box1.PMax.X >= box2.PMax.X) ||
                (box1.PMin.X >= box2.PMin.X && box1.PMin.X <= box2.PMax.X) ||
                (box1.PMax.X >= box2.PMin.X && box1.PMax.X <= box2.PMax.X)) &&
               ((box1.PMin.Y <= box2.PMin.Y && box1.PMax.Y >= box2.PMax.Y) ||
                (box1.PMin.Y >= box2.PMin.Y && box1.PMin.Y <= box2.PMax.Y) ||
                (box1.PMax.Y >= box2.PMin.Y && box1.PMax.Y <= box2.PMax.Y)) &&
               ((box1.PMin.Z <= box2.PMin.Z && box1.PMax.Z >= box2.PMax.Z) ||
                (box1.PMin.Z >= box2.PMin.Z && box1.PMin.Z <= box2.PMax.Z) ||
                (box1.PMax.Z >= box2.PMin.Z && box1.PMax.Z <= box2.PMax.Z)))
            {
                if ((box1.PMin.X <= box2.PMin.X) &&
                   (box1.PMin.Y <= box2.PMin.Y) &&
                   (box1.PMin.Z <= box2.PMin.Z) &&
                   (box1.PMax.X >= box2.PMax.X) &&
                   (box1.PMax.Y >= box2.PMax.Y) &&
                   (box1.PMax.Z >= box2.PMax.Z))
                {
                    return BoxBoxResult.Adentro;
                }
                else if ((box1.PMin.X > box2.PMin.X) &&
                         (box1.PMin.Y > box2.PMin.Y) &&
                         (box1.PMin.Z > box2.PMin.Z) &&
                         (box1.PMax.X < box2.PMax.X) &&
                         (box1.PMax.Y < box2.PMax.Y) &&
                         (box1.PMax.Z < box2.PMax.Z))
                {
                    return BoxBoxResult.Encerrando;
                }
                else
                {
                    return BoxBoxResult.Atravesando;
                }
            }
            else
            {
                return BoxBoxResult.Afuera;
            }
        }

        /// <summary>
        /// Resultado de una clasificación BoundingBox-BoundingBox
        /// </summary>
        public enum BoxBoxResult
        {
            /// <summary>
            /// El BoundingBox 1 se encuentra completamente adentro del BoundingBox 2
            /// </summary>
            Adentro,
            /// <summary>
            /// El BoundingBox 1 se encuentra completamente afuera del BoundingBox 2
            /// </summary>
            Afuera,
            /// <summary>
            /// El BoundingBox 1 posee parte adentro y parte afuera del BoundingBox 2
            /// </summary>
            Atravesando,
            /// <summary>
            /// El BoundingBox 1 contiene completamente adentro al BoundingBox 2.
            /// Caso particular de Afuera.
            /// </summary>
            Encerrando
        }
    
        /// <summary>
        /// Indica si un BoundingBox colisiona con otro.
        /// Solo indica si hay colisión o no. No va mas en detalle.
        /// </summary>
        /// <param name="a">BoundingBox 1</param>
        /// <param name="b">BoundingBox 2</param>
        /// <returns>True si hay colisión</returns>
        public static bool testAABBAABB(TgcBoundingBox a, TgcBoundingBox b)
        {
            // Exit with no intersection if separated along an axis
            if (a.PMax.X < b.PMin.X || a.PMin.X > b.PMax.X) return false;
            if (a.PMax.Y < b.PMin.Y || a.PMin.Y > b.PMax.Y) return false;
            if (a.PMax.Z < b.PMin.Z || a.PMin.Z > b.PMax.Z) return false;
            // Overlapping on all axes means AABBs are intersecting
            return true;
        }
        
        /// <summary>
        /// Indica si un Ray colisiona con un AABB.
        /// Si hay intersección devuelve True, q contiene
        /// el punto de intesección.
        /// Basado en el código de: http://www.codercorner.com/RayAABB.cpp
        /// La dirección del Ray puede estar sin normalizar.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="a">AABB</param>
        /// <param name="q">Punto de intersección</param>
        /// <returns>True si hay colisión</returns>
        public static bool intersectRayAABB(TgcRay ray, TgcBoundingBox aabb, out Vector3 q)
        {
            float t;
            return TgcCollisionUtils.intersectRayAABB(ray.toStruct(), aabb.toStruct(), out q);
        }

        /// <summary>
        /// Indica si un Ray colisiona con un AABB.
        /// Si hay intersección devuelve True, q contiene
        /// el punto de intesección.
        /// Basado en el código de: http://www.codercorner.com/RayAABB.cpp
        /// La dirección del Ray puede estar sin normalizar.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="a">AABB</param>
        /// <param name="q">Punto de intersección</param>
        /// <returns>True si hay colisión</returns>
        public static bool intersectRayAABB(TgcRay.RayStruct ray, TgcBoundingBox.AABBStruct aabb, out Vector3 q)
        {
            q = Vector3.Empty;
            bool inside = true;
            float[] aabbMin = toArray(aabb.min);
            float[]aabbMax = toArray(aabb.max);
            float[]rayOrigin = toArray(ray.origin);
            float[]rayDir = toArray(ray.direction);

            float[] max_t = new float[3]{-1.0f, -1.0f, -1.0f};
            float[] coord = new float[3];

            for (uint i = 0; i < 3; ++i)
            {
                if (rayOrigin[i] < aabbMin[i])
                {
                    inside = false;
                    coord[i] = aabbMin[i];

                    if (rayDir[i] != 0.0f)
                    {
                        max_t[i] = (aabbMin[i] - rayOrigin[i]) / rayDir[i];
                    }
                }
                else if (rayOrigin[i] > aabbMax[i])
                {
                    inside = false;
                    coord[i] = aabbMax[i];

                    if (rayDir[i] != 0.0f)
                    {
                        max_t[i] = (aabbMax[i] - rayOrigin[i]) / rayDir[i];
                    }
                }
            }

            // If the Ray's start position is inside the Box, we can return true straight away.
            if (inside)
            {
                q = toVector3(rayOrigin);
                return true;
            }

            uint plane = 0;
            if (max_t[1] > max_t[plane])
            {
                plane = 1;
            }
            if (max_t[2] > max_t[plane])
            {
                plane = 2;
            }
            
            if (max_t[plane] < 0.0f)
            {
                return false;
            }

            for (uint i = 0; i < 3; ++i)
            {
                if (plane != i)
                {
                    coord[i] = rayOrigin[i] + max_t[plane] * rayDir[i];

                    if (coord[i] < aabbMin[i] - float.Epsilon || coord[i] > aabbMax[i] + float.Epsilon)
                    {
                        return false;
                    }
                }
            }

            q = toVector3(coord);
            return true;
        }

        /// <summary>
        /// Indica si el segmento de recta compuesto por p0-p1 colisiona con el BoundingBox.
        /// </summary>
        /// <param name="p0">Punto inicial del segmento</param>
        /// <param name="p1">Punto final del segmento</param>
        /// <param name="aabb">BoundingBox</param>
        /// <param name="q">Punto de intersección</param>
        /// <returns>True si hay colisión</returns>
        public static bool intersectSegmentAABB(Vector3 p0, Vector3 p1, TgcBoundingBox aabb, out Vector3 q)
        {
            Vector3 segmentDir = p1 - p0;
            TgcRay ray = new TgcRay(p0, segmentDir);
            if (TgcCollisionUtils.intersectRayAABB(ray, aabb, out q))
            {
                float segmentLengthSq = segmentDir.LengthSq();
                Vector3 collisionDiff = q - p0;
                float collisionLengthSq = collisionDiff.LengthSq();
                if (collisionLengthSq <= segmentLengthSq)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Dado el punto p, devuelve el punto del contorno del BoundingBox mas próximo a p.
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="aabb">BoundingBox a testear</param>
        /// <returns>Punto mas cercano a p del BoundingBox</returns>
        public static Vector3 closestPointAABB(Vector3 p, TgcBoundingBox aabb)
        {
            float[] aabbMin = toArray(aabb.PMin);
            float[] aabbMax = toArray(aabb.PMax);
            float[] pArray = toArray(p);
            float[] q = new float[3];

            // For each coordinate axis, if the point coordinate value is
            // outside box, clamp it to the box, else keep it as is
            for (int i = 0; i < 3; i++)
            {
                float v = pArray[i];
                if (v < aabbMin[i]) v = aabbMin[i]; // v = max(v, b.min[i])
                if (v > aabbMax[i]) v = aabbMax[i]; // v = min(v, b.max[i])
                q[i] = v;
            }
            return TgcCollisionUtils.toVector3(q);
        }

        /// <summary>
        /// Calcula la mínima distancia al cuadrado entre el punto p y el BoundingBox.
        /// Si no se necesita saber el punto exacto de colisión es más ágil que utilizar closestPointAABB(). 
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>Mínima distacia al cuadrado</returns>
        public static float sqDistPointAABB(Vector3 p, TgcBoundingBox aabb)
        {
            return TgcCollisionUtils.sqDistPointAABB(p, aabb.toStruct());
        }

        /// <summary>
        /// Calcula la mínima distancia al cuadrado entre el punto p y el BoundingBox.
        /// Si no se necesita saber el punto exacto de colisión es más ágil que utilizar closestPointAABB(). 
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>Mínima distacia al cuadrado</returns>
        public static float sqDistPointAABB(Vector3 p, TgcBoundingBox.AABBStruct aabb)
        {
            float[] aabbMin = toArray(aabb.min);
            float[] aabbMax = toArray(aabb.max);
            float[] pArray = toArray(p);
            float sqDist = 0.0f;

            for (int i = 0; i < 3; i++)
            {
                // For each axis count any excess distance outside box extents
                float v = pArray[i];
                if (v < aabbMin[i]) sqDist += (aabbMin[i] - v) * (aabbMin[i] - v);
                if (v > aabbMax[i]) sqDist += (v - aabbMax[i]) * (v - aabbMax[i]);
            }
            return sqDist;
        }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con un BoundingBox.
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión</returns>
        public static bool testSphereAABB(TgcBoundingSphere sphere, TgcBoundingBox aabb)
        {
            return TgcCollisionUtils.testSphereAABB(sphere.toStruct(), aabb.toStruct());
        }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con un BoundingBox.
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión</returns>
        public static bool testSphereAABB(TgcBoundingSphere.SphereStruct sphere, TgcBoundingBox.AABBStruct aabb)
        {
            //Compute squared distance between sphere center and AABB
            float sqDist = TgcCollisionUtils.sqDistPointAABB(sphere.center, aabb);
            //Sphere and AABB intersect if the (squared) distance
            //between them is less than the (squared) sphere radius
            return sqDist <= sphere.radius * sphere.radius;
        }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con un BoundingBox.
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión</returns>
        public static bool testSphereOBB(TgcBoundingSphere sphere, TgcObb obb)
        {
            return TgcCollisionUtils.testSphereOBB(sphere.toStruct(), obb.toStruct());
        }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con un BoundingBox.
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión</returns>
        public static bool testSphereOBB(TgcBoundingSphere.SphereStruct sphere, TgcObb.OBBStruct obb)
        {
            //Transformar esfera a OBB-Space
            TgcBoundingSphere.SphereStruct sphere2 = new TgcBoundingSphere.SphereStruct();
            sphere2.center = obb.toObbSpace(sphere.center);
            sphere2.radius = sphere.radius;

            //Crear AABB que representa al OBB
            Vector3 min = -obb.extents;
            Vector3 max = obb.extents;
            TgcBoundingBox.AABBStruct aabb = new TgcBoundingBox.AABBStruct();
            aabb.min = min;
            aabb.max = max;

            return TgcCollisionUtils.testSphereAABB(sphere2, aabb);
        }


        /// <summary>
        /// Clasifica un BoundingBox respecto de un Plano.
        /// </summary>
        /// <param name="plane">Plano</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>
        /// Resultado de la clasificación.
        /// </returns>
        /// 
        public static PlaneBoxResult classifyPlaneAABB(Plane plane, TgcBoundingBox aabb)
        {
            Vector3 vmin = Vector3.Empty;
            Vector3 vmax = Vector3.Empty;

            //Obtener puntos minimos y maximos en base a la dirección de la normal del plano
            if (plane.A >= 0f)
            {
                vmin.X = aabb.PMin.X;
                vmax.X = aabb.PMax.X;
            }
            else
            {
                vmin.X = aabb.PMax.X;
                vmax.X = aabb.PMin.X;
            }

            if (plane.B >= 0f)
            {
                vmin.Y = aabb.PMin.Y;
                vmax.Y = aabb.PMax.Y;
            }
            else
            {
                vmin.Y = aabb.PMax.Y;
                vmax.Y = aabb.PMin.Y;
            }

            if (plane.C >= 0f)
            {
                vmin.Z = aabb.PMin.Z;
                vmax.Z = aabb.PMax.Z;
            }
            else
            {
                vmin.Z = aabb.PMax.Z;
                vmax.Z = aabb.PMin.Z;
            }

            //Analizar punto minimo y maximo contra el plano
            PlaneBoxResult result;
            if (plane.Dot(vmin) > 0f)
            {
                result = PlaneBoxResult.IN_FRONT_OF;
            } 
            else if(plane.Dot(vmax) > 0f)
            {
                result = PlaneBoxResult.INTERSECT;
            }
            else 
            {
                result = PlaneBoxResult.BEHIND;
            }

            return result;
        }

        /// <summary>
        /// Resultado de una clasificación Plano-BoundingBox
        /// </summary>
        public enum PlaneBoxResult
        {
            /// <summary>
            /// El BoundingBox está completamente en el lado negativo el plano
            /// </summary>
            BEHIND,

            /// <summary>
            /// El BoundingBox está completamente en el lado positivo del plano
            /// </summary>
            IN_FRONT_OF,

            /// <summary>
            /// El Plano atraviesa al BoundingBox
            /// </summary>
            INTERSECT,
        }

        /// <summary>
        /// Indica si un Plano colisiona con un BoundingBox
        /// </summary>
        /// <param name="plane">Plano</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión.</returns>
        public static bool testPlaneAABB(Plane plane, TgcBoundingBox aabb)
        {
            Vector3 c = (aabb.PMax + aabb.PMin) * 0.5f; // Compute AABB center
            Vector3 e = aabb.PMax - c; // Compute positive extents

            // Compute the projection interval radius of b onto L(t) = b.c + t * p.n
            float r = e.X * FastMath.Abs(plane.A) + e.Y * FastMath.Abs(plane.B) + e.Z * FastMath.Abs(plane.C);
            // Compute distance of box center from plane
            float s = plane.Dot(c);
            // Intersection occurs when distance s falls within [-r,+r] interval
            return FastMath.Abs(s) <= r;
        }



        /// <summary>
        /// /// Indica si un Triangulo colisiona con un BoundingBox
        /// Basado en:
        /// http://fileadmin.cs.lth.se/cs/Personal/Tomas_Akenine-Moller/code/tribox3.txt
        /// </summary>
        /// <param name="vert0">Vertice 0 del triángulo</param>
        /// <param name="vert1">Vertice 1 del triángulo</param>
        /// <param name="vert2">Vertice 2 del triángulo</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>True si hay colisión.</returns>
        public static bool testTriangleAABB(Vector3 vert0, Vector3 vert1, Vector3 vert2, TgcBoundingBox aabb)
        {
            /*   use separating axis theorem to test overlap between triangle and box need to test for overlap in these directions:
            *    1) the {x,y,z}-directions (actually, since we use the AABB of the triangle we do not even need to test these)
            *    2) normal of the triangle
            *    3) crossproduct(edge from tri, {x,y,z}-directin) this gives 3x3=9 more tests 
            */

            //Compute box center and boxhalfsize (if not already given in that format)
            Vector3 boxcenter = aabb.calculateBoxCenter();
            Vector3 boxhalfsize = aabb.calculateAxisRadius();

            //Aux vars
            Vector3[] triverts = new Vector3[3] { vert0, vert1, vert2 };
            float min,max,p0,p1,p2,rad,fex,fey,fez;

            //move everything so that the boxcenter is in (0,0,0)
            Vector3 v0 = Vector3.Subtract(triverts[0], boxcenter);
            Vector3 v1 = Vector3.Subtract(triverts[1], boxcenter);
            Vector3 v2 = Vector3.Subtract(triverts[2], boxcenter);

            //compute triangle edges
            Vector3 e0 = Vector3.Subtract(v1, v0); //tri edge 0
            Vector3 e1 = Vector3.Subtract(v2, v1); //tri edge 1
            Vector3 e2 = Vector3.Subtract(v0, v2); //tri edge 2


            //Bullet 3:
            //test the 9 tests first (this was faster)

            //edge 0
            fex = FastMath.Abs(e0.X);
            fey = FastMath.Abs(e0.Y);
            fez = FastMath.Abs(e0.Z);

            //AXISTEST_X01
            p0 = e0.Z * v0.Y - e0.Y * v0.Z;
            p2 = e0.Z * v2.Y - e0.Y * v2.Z;
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize.Y + fey * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Y02
            p0 = -e0.Z * v0.X + e0.X * v0.Z;
            p2 = -e0.Z * v2.X + e0.X * v2.Z;
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize.X + fex * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Z12
            p1 = e0.Y * v1.X - e0.X * v1.Y;
            p2 = e0.Y * v2.X - e0.X * v2.Y;
            if (p2 < p1) { min = p2; max = p1; } else { min = p1; max = p2; }
            rad = fey * boxhalfsize.X + fex * boxhalfsize.Y;
            if (min > rad || max < -rad) return false;


            //edge 1
            fex = FastMath.Abs(e1.X);
            fey = FastMath.Abs(e1.Y);
            fez = FastMath.Abs(e1.Z);

            //AXISTEST_X01
            p0 = e1.Z * v0.Y - e1.Y * v0.Z;
            p2 = e1.Z * v2.Y - e1.Y * v2.Z;
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize.Y + fey * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Y02
            p0 = -e1.Z * v0.X + e1.X * v0.Z;
            p2 = -e1.Z * v2.X + e1.X * v2.Z;
            if (p0 < p2) { min = p0; max = p2; } else { min = p2; max = p0; }
            rad = fez * boxhalfsize.X + fex * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Z0
            p0 = e1.Y * v0.X - e1.X * v0.Y;
            p1 = e1.Y * v1.X - e1.X * v1.Y;
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fey * boxhalfsize.X + fex * boxhalfsize.Y;
            if (min > rad || max < -rad) return false;


            //edge 2
            fex = FastMath.Abs(e2.X);
            fey = FastMath.Abs(e2.Y);
            fez = FastMath.Abs(e2.Z);

            //AXISTEST_X2
            p0 = e2.Z * v0.Y - e2.Y * v0.Z;
            p1 = e2.Z * v1.Y - e2.Y * v1.Z;
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fez * boxhalfsize.Y + fey * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Y1
            p0 = -e2.Z * v0.X + e2.X * v0.Z;
            p1 = -e2.Z * v1.X + e2.X * v1.Z;
            if (p0 < p1) { min = p0; max = p1; } else { min = p1; max = p0; }
            rad = fez * boxhalfsize.X + fex * boxhalfsize.Z;
            if (min > rad || max < -rad) return false;

            //AXISTEST_Z12
            p1 = e2.Y * v1.X - e2.X * v1.Y;
            p2 = e2.Y * v2.X - e2.X * v2.Y;
            if (p2 < p1) { min = p2; max = p1; } else { min = p1; max = p2; }
            rad = fey * boxhalfsize.X + fex * boxhalfsize.Y;
            if (min > rad || max < -rad) return false;
           


           //Bullet 1:
           /*  first test overlap in the {x,y,z}-directions
            *  find min, max of the triangle each direction, and test for overlap in
            *  that direction -- this is equivalent to testing a minimal AABB around
            *  the triangle against the AABB 
            */
            //test in X-direction
            findMinMax(v0.X, v1.X, v2.X, ref min, ref max);
            if (min > boxhalfsize.X || max < -boxhalfsize.X) return false;

            //test in Y-direction
            findMinMax(v0.Y, v1.Y, v2.Y, ref min, ref max);
            if (min > boxhalfsize.Y || max < -boxhalfsize.Y) return false;

            //test in Z-direction
            findMinMax(v0.Z, v1.Z, v2.Z, ref min, ref max);
            if (min > boxhalfsize.Z || max < -boxhalfsize.Z) return false;



            //Bullet 2:
            /*  test if the box intersects the plane of the triangle
            *  compute plane equation of triangle: normal*x+d=0 
            */
            Vector3 normal = Vector3.Cross(e0, e1);
            if (!testTriangleAABB_planeBoxOverlap(toArray(normal), toArray(v0), toArray(boxhalfsize))) return false;


            //box and triangle overlaps
            return true;
        }

        /// <summary>
        /// Utilizado por testTriangleAABB.
        /// Indica si un Box colisiona con un plano.
        /// Adaptado especificamente a la forma que lo utiliza testTriangleAABB.
        /// </summary>
        /// <param name="normal">normal del plano</param>
        /// <param name="vert">un punto del plano</param>
        /// <param name="maxbox">????</param>
        /// <returns>true si hay colision</returns>
        private static bool testTriangleAABB_planeBoxOverlap(float[] normal, float[] vert, float[] maxbox)
        {
            int q;
            float[] vmin = new float[3];
            float[]vmax = new float[3];
            float v;

            for (q = 0; q <= 2; q++)
            {
                v = vert[q];
                if(normal[q]>0.0f)
                {
                    vmin[q] = -maxbox[q] - v;
                    vmax[q] = maxbox[q] - v;
                }
                else
                {
                    vmin[q] = maxbox[q] - v;
                    vmax[q] = -maxbox[q] - v;
                }
            }

            if (dot(normal, vmin) > 0.0f) return false;
            if (dot(normal, vmax) >= 0.0f) return true;

            return false;
        }
            


        #endregion


        #region BoundingSphere

        /// <summary>
        /// Indica si un BoundingSphere colisiona con otro.
        /// </summary>
        /// <returns>True si hay colisión</returns>
        public static bool testSphereSphere(TgcBoundingSphere a, TgcBoundingSphere b)
        {
            // Calculate squared distance between centers
            Vector3 d = a.Center - b.Center;
            float dist2 = Vector3.Dot(d, d);
            // Spheres intersect if squared distance is less than squared sum of radii
            float radiusSum = a.Radius + b.Radius;
            return dist2 <= radiusSum * radiusSum;
        }

        /// <summary>
        /// Idica si un BoundingSphere colisiona con un plano
        /// </summary>
        /// <returns>True si hay colisión</returns>
        public static bool testSpherePlane(TgcBoundingSphere s, Plane plane)
        {
            Vector3 p = toVector3(plane);

            // For a normalized plane (|p.n| = 1), evaluating the plane equation
            // for a point gives the signed distance of the point to the plane
            float dist = Vector3.Dot(s.Center, p) - plane.D;
            // If sphere center within +/-radius from plane, plane intersects sphere
            return FastMath.Abs(dist) <= s.Radius;
        }

        // 
        /// <summary>
        /// Indica si un BoundingSphere se encuentra completamente en el lado negativo del plano
        /// </summary>
        /// <returns>True si se encuentra completamente en el lado negativo del plano</returns>
        public static bool insideSpherePlane(TgcBoundingSphere s, Plane plane)
        {
            Vector3 p = toVector3(plane);

            float dist = Vector3.Dot(s.Center, p) - plane.D;
            return dist < -s.Radius;
        }


        /// <summary>
        /// Indica si un Ray colisiona con un BoundingSphere.
        /// Si el resultado es True se carga el punto de colision (q) y la distancia de colision en el Ray (t).
        /// La dirección del Ray debe estar normalizada.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="t">Distancia de colision del Ray</param>
        /// <param name="q">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectRaySphere(TgcRay ray, TgcBoundingSphere sphere, out float t, out Vector3 q)
        {
            t = -1;
            q = Vector3.Empty;

            Vector3 m = ray.Origin - sphere.Center;
            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - sphere.Radius * sphere.Radius;
            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0)
            if (c > 0.0f && b > 0.0f) return false;
            float discr = b*b - c;
            // A negative discriminant corresponds to ray missing sphere
            if (discr < 0.0f) return false;
            // Ray now found to intersect sphere, compute smallest t value of intersection
            t = -b - FastMath.Sqrt(discr);
            // If t is negative, ray started inside sphere so clamp t to zero
            if (t < 0.0f) t = 0.0f;
            q = ray.Origin + t * ray.Direction;
            return true;
        }

        /// <summary>
        /// Indica si un segmento de recta colisiona con un BoundingSphere.
        /// Si el resultado es True se carga el punto de colision (q) y la distancia de colision en el t.
        /// La dirección del Ray debe estar normalizada.
        /// </summary>
        /// <param name="p0">Punto inicial del segmento</param>
        /// <param name="p1">Punto final del segmento</param>
        /// <param name="s">BoundingSphere</param>
        /// <param name="t">Distancia de colision del segmento</param>
        /// <param name="q">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectSegmentSphere(Vector3 p0, Vector3 p1, TgcBoundingSphere sphere, out float t, out Vector3 q)
        {
            Vector3 segmentDir = p1 - p0;
            TgcRay ray = new TgcRay(p0, segmentDir);
            if (TgcCollisionUtils.intersectRaySphere(ray, sphere, out t, out q))
            {
                float segmentLengthSq = segmentDir.LengthSq();
                Vector3 collisionDiff = q - p0;
                float collisionLengthSq = collisionDiff.LengthSq();
                if (collisionLengthSq <= segmentLengthSq)
                {
                    return true;
                }
            }

            return false;
        }


        // 
        /// <summary>
        /// Indica si un BoundingSphere colisiona con un Ray (sin indicar su punto de colision)
        /// La dirección del Ray debe estar normalizada.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="sphere">BoundingSphere</param>
        /// <returns>True si hay colision</returns>
        public static bool testRaySphere(TgcRay ray, TgcBoundingSphere sphere)
        {
            Vector3 m = ray.Origin - sphere.Center;
            float c = Vector3.Dot(m, m) - sphere.Radius * sphere.Radius;
            // If there is definitely at least one real root, there must be an intersection
            if (c <= 0.0f) return true;
            float b = Vector3.Dot(m, ray.Direction);
            // Early exit if ray origin outside sphere and ray pointing away from sphere
            if (b > 0.0f) return false;
            float disc = b * b - c;
            // A negative discriminant corresponds to ray missing sphere
            if (disc < 0.0f) return false;
            // Now ray must hit sphere
            return true;
        }

        /// <summary>
        /// Indica si el punto p se encuentra dentro de la esfera
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="p">Punto a testear</param>
        /// <returns>True si p está dentro de la esfera</returns>
        public static bool testPointSphere(TgcBoundingSphere sphere, Vector3 p)
        {
            Vector3 cp = p - sphere.Center;
            float d = cp.LengthSq();

            return d <= (sphere.Radius * sphere.Radius);
        }

        /// <summary>
        /// Detecta colision entre una esfera que se esta moviendo contra un plano.
        /// Si hay colision devuelve el instante t de la colision y el punto q de colision.
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="velocity">Vector de movimiento de la esfera</param>
        /// <param name="plane">Plano</param>
        /// <param name="t">Instante de colision en el intervalo [0, 1]</param>
        /// <param name="q">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectMovingSpherePlane(TgcBoundingSphere sphere, Vector3 velocity, Plane plane, out float t, out Vector3 q)
        {
            // Compute distance of sphere center to plane
            float dist = plane.Dot(sphere.Center);
            if (FastMath.Abs(dist) <= sphere.Radius) {
                // The sphere is already overlapping the plane. Set time of
                // intersection to zero and q to sphere center
                t = 0.0f;
                q = sphere.Center;
                return true;
            } else {
                Vector3 p_n = getPlaneNormal(plane);
                float denom = Vector3.Dot(p_n, velocity);
                if (denom * dist >= 0.0f) {
                    // No intersection as sphere moving parallel to or away from plane
                    t = -1;
                    q = Vector3.Empty;
                    return false;
                } else {
                    // Sphere is moving towards the plane
                    // Use +r in computations if sphere in front of plane, else -r
                    float r = dist > 0.0f ? sphere.Radius : -sphere.Radius;
                    t = (r - dist) / denom;
                    q = sphere.Center + t * velocity - r * p_n;
                    if (t > 1) return false;
                    return true;
                }
            }
        }

        /// <summary>
        /// Indica si una esfera que se esta moviendo colisiona contra un plano.
        /// Solo indica si hay colision. No calcula el punto de colision.
        /// Es mas eficiente que el metodo: "intersectMovingSpherePlane()"
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="velocity">Vector de movimiento de la esfera</param>
        /// <param name="plane">Plano</param>
        /// <returns>True si hay colision</returns>
        public static bool testMovingSpherePlane(TgcBoundingSphere sphere, Vector3 velocity, Plane plane)
        {
            Vector3 a = sphere.Center;
            Vector3 b = sphere.Center + velocity;

            // Get the distance for both a and b from plane p
            float adist = plane.Dot(a);
            float bdist = plane.Dot(b);

            // Intersects if on different sides of plane (distances have different signs)
            if (adist * bdist < 0.0f) return true;

            // Intersects if start or end position within radius from plane
            if (FastMath.Abs(adist) <= sphere.Radius || FastMath.Abs(bdist) <= sphere.Radius) return true;

            // No intersection
            return false;
        }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con un triangulo (a, b, c).
        /// Si hay colision devuelve el punto p mas cercano de la colision
        /// </summary>
        /// <param name="sphere">BoundingSphere</param>
        /// <param name="a">Vertice A del triangulo</param>
        /// <param name="b">Vertice B del triangulo</param>
        /// <param name="c">Vertice C del triangulo</param>
        /// <param name="p">Punto mas cercano de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool testSphereTriangle(TgcBoundingSphere sphere, Vector3 a, Vector3 b, Vector3 c, out Vector3 p)
        {
            // Find point P on triangle ABC closest to sphere center
            p = TgcCollisionUtils.closestPointTriangle(sphere.Center, a, b, c);
            // Sphere and triangle intersect if the (squared) distance from sphere
            // center to point p is less than the (squared) sphere radius
            Vector3 v = p - sphere.Center;
            return Vector3.Dot(v, v) <= sphere.Radius * sphere.Radius;
        }


        #endregion


        #region Planos, Segmentos y otros

        /// <summary>
        /// Determina el punto del plano p mas cercano al punto q.
        /// La normal del plano puede estar sin normalizar.
        /// </summary>
        /// <param name="q">Punto a testear</param>
        /// <param name="p">Plano</param>
        /// <returns>Punto del plano que mas cerca esta de q</returns>
        public static Vector3 closestPointPlane(Vector3 q, Plane p)
        {
            Vector3 p_n = toVector3(p);

            float t = (Vector3.Dot(p_n, q) + p.D) / Vector3.Dot(p_n, p_n);
            return q - t * p_n;
        }

        /// <summary>
        /// Determina el punto del plano p mas cercano al punto q.
        /// Más ágil que closestPointPlane() pero la normal del plano debe estar normalizada.
        /// </summary>
        /// <param name="q">Punto a testear</param>
        /// <param name="p">Plano</param>
        /// <returns>Punto del plano que mas cerca esta de q</returns>
        public static Vector3 closestPointPlaneNorm(Vector3 q, Plane p)
        {
            Vector3 p_n = toVector3(p);

            float t = (Vector3.Dot(p_n, q) + p.D);
            return q - t * p_n;
        }

        /// <summary>
        /// Indica la distancia de un punto al plano
        /// </summary>
        /// <param name="q">Punto a testear</param>
        /// <param name="p">Plano</param>
        /// <returns>Distancia del punto al plano</returns>
        public static float distPointPlane(Vector3 q, Plane p)
        {
            /*
            Vector3 p_n = toVector3(p);
            return (Vector3.Dot(p_n, q) + p.D) / Vector3.Dot(p_n, p_n);
            */
            return p.Dot(q);
        }

        /// <summary>
        /// Clasifica un Punto respecto de un Plano
        /// </summary>
        /// <param name="q">Punto a clasificar</param>
        /// <param name="p">Plano</param>
        /// <returns>Resultado de la colisión</returns>
        public static PointPlaneResult classifyPointPlane(Vector3 q, Plane p)
        {
            float distance = distPointPlane(q, p);

            if (distance < -float.Epsilon)
            {
                return PointPlaneResult.BEHIND;
            }
            else if (distance > float.Epsilon)
            {
                return PointPlaneResult.IN_FRONT_OF;
            }
            else
            {
                return PointPlaneResult.COINCIDENT;
            } 
        }

        /// <summary>
        /// Resultado de una clasificación Punto-Plano
        /// </summary>
        public enum PointPlaneResult
        {
            /// <summary>
            /// El punto está sobre el lado negativo el plano
            /// </summary>
            BEHIND,

            /// <summary>
            /// El punto está sobre el lado positivo del plano
            /// </summary>
            IN_FRONT_OF,

            /// <summary>
            /// El punto pertenece al plano
            /// </summary>
            COINCIDENT,
        }

        /// <summary>
        /// Dado el segmento ab y el punto p, determina el punto mas cercano sobre el segmento ab.
        /// </summary>
        /// <param name="c">Punto a testear</param>
        /// <param name="a">Inicio del segmento ab</param>
        /// <param name="b">Fin del segmento ab</param>
        /// <param name="t">Valor que cumple la ecuacion d(t) = a + t*(b - a)</param>
        /// <returns>Punto sobre ab que esta mas cerca de p</returns>
        public static Vector3 closestPointSegment(Vector3 p, Vector3 a, Vector3 b, out float t)
        {
            Vector3 ab = b - a;
            // Project c onto ab, computing parameterized position d(t) = a + t*(b – a)
            t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
            // If outside segment, clamp t (and therefore d) to the closest endpoint
            if (t < 0.0f) t = 0.0f;
            if (t > 1.0f) t = 1.0f;
            // Compute projected position from the clamped t
            return a + t * ab;
        }

        /// <summary>
        /// Devuelve la distancia al cuadrado entre el punto c y el segmento ab
        /// </summary>
        /// <param name="a">Inicio del segmento ab</param>
        /// <param name="b">Fin del segmento ab</param>
        /// <param name="c">Punto a testear</param>
        /// <returns>Distancia al cuadrado entre c y ab</returns>
        public static float sqDistPointSegment(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a, ac = c - a, bc = c - b;
            float e = Vector3.Dot(ac, ab);
            // Handle cases where c projects outside ab
            if (e <= 0.0f) return Vector3.Dot(ac, ac);
            float f = Vector3.Dot(ab, ab);
            if (e >= f) return Vector3.Dot(bc, bc);
            // Handle cases where c projects onto ab
            return Vector3.Dot(ac, ac) - e * e / f;
        }

        /// <summary>
        /// Indica si un Ray colisiona con un Plano.
        /// Tanto la normal del plano como la dirección del Ray se asumen normalizados.
        /// </summary>
        /// <param name="ray">Ray a testear</param>
        /// <param name="plane">Plano a testear</param>
        /// <param name="t">Instante de colisión</param>
        /// <param name="q">Punto de colisión con el plano</param>
        /// <returns>True si hubo colisión</returns>
        public static bool intersectRayPlane(TgcRay ray, Plane plane, out float t, out Vector3 q)
        {
            Vector3 planeNormal = TgcCollisionUtils.getPlaneNormal(plane);
            float numer = plane.Dot(ray.Origin);
            float denom = Vector3.Dot(planeNormal, ray.Direction);
            t = -numer / denom;

            if(t > 0.0f)
            {
                q = ray.Origin + ray.Direction * t;
                return true;
            }

            q = Vector3.Empty;
            return false;
        }
        
        /// <summary>
        /// Indica si el segmento de recta compuesto por a-b colisiona con el Plano.
        /// La normal del plano se considera normalizada.
        /// </summary>
        /// <param name="a">Punto inicial del segmento</param>
        /// <param name="b">Punto final del segmento</param>
        /// <param name="plane">Plano a testear</param>
        /// <param name="t">Instante de colisión</param>
        /// <param name="q">Punto de colisión</param>
        /// <returns>True si hay colisión</returns>
        public static bool intersectSegmentPlane(Vector3 a, Vector3 b, Plane plane, out float t, out Vector3 q)
        {
            Vector3 planeNormal = getPlaneNormal(plane);

            //t = -(n.A + d / n.(B - A))
            Vector3 ab = b - a;
            t = -(plane.Dot(a)) / (Vector3.Dot(planeNormal, ab));

            // If t in [0..1] compute and return intersection point
            if (t >= 0.0f && t <= 1.0f)
            {
                q = a  + t * ab;
                return true;
            }

            q = Vector3.Empty;
            return false;
        }

        /// <summary>
        /// Determina el punto mas cercano entre el triángulo (abc) y el punto p.
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="a">Vértice A del triángulo</param>
        /// <param name="b">Vértice B del triángulo</param>
        /// <param name="c">Vértice C del triángulo</param>
        /// <returns>Punto mas cercano al triángulo</returns>
        public static Vector3 closestPointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            // Check if P in vertex region outside A
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;
            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f) return a; // barycentric coordinates (1,0,0)
            // Check if P in vertex region outside B
            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3) return b; // barycentric coordinates (0,1,0)
            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                return a + v * ab; // barycentric coordinates (1-v,v,0)
            }
            // Check if P in vertex region outside C
            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6) return c; // barycentric coordinates (0,0,1)

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                return a + w * ac; // barycentric coordinates (1-w,0,w)
            }
            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + w * (c - b); // barycentric coordinates (0,1-w,w)
            }
            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float vFinal = vb * denom;
            float wFinal = vc * denom;
            return a + ab * vFinal + ac * wFinal; // = u*a + v*b + w*c, u = va * denom = 1.0f - v - w
        }

        /*
        /// <summary>
        /// Determina el punto mas cercano entre el rectángulo 3D (abcd) y el punto p.
        /// Los cuatro puntos abcd del rectángulo deben estar contenidos sobre el mismo plano.
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="a">Vértice A del rectángulo</param>
        /// <param name="b">Vértice B del rectángulo</param>
        /// <param name="c">Vértice C del rectángulo</param>
        /// <param name="c">Vértice D del rectángulo</param>
        /// <returns>Punto mas cercano al rectángulo</returns>
        public static Vector3 closestPointRectangle3d(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            //Buscar el punto mas cercano a cada uno de los 4 segmentos de recta que forman el rectángulo
            float t;
            Vector3[] points = new Vector3[4];
            points[0] = closestPointSegment(p, a, b, out t);
            points[1] = closestPointSegment(p, b, c, out t);
            points[2] = closestPointSegment(p, c, d, out t);
            points[3] = closestPointSegment(p, d, a, out t);

            //Buscar el menor punto de los 4
            return TgcCollisionUtils.closestPoint(p, points, out t);
        }
        */

        /// <summary>
        /// Determina el punto mas cercano entre un rectánglo 3D (especificado por a, b y c) y el punto p.
        /// Los puntos a, b y c deben formar un rectángulo 3D tal que los vectores AB y AC expandan el rectángulo.
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="a">Vértice A del rectángulo</param>
        /// <param name="b">Vértice B del rectángulo</param>
        /// <param name="c">Vértice C del rectángulo</param>
        /// <returns></returns>
        public static Vector3 closestPointRectangle3d(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a; // vector across rect
            Vector3 ac = c - a; // vector down rect
            Vector3 d = p - a;
            // Start result at top-left corner of rect; make steps from there
            Vector3 q = a;
            // Clamp p’ (projection of p to plane of r) to rectangle in the across direction
            float dist = Vector3.Dot(d, ab);
            float maxdist = Vector3.Dot(ab, ab);
            if (dist >= maxdist)
                q += ab;
            else if (dist > 0.0f)
                q += (dist / maxdist) * ab;
            // Clamp p’ (projection of p to plane of r) to rectangle in the down direction
            dist = Vector3.Dot(d, ac);
            maxdist = Vector3.Dot(ac, ac);
            if (dist >= maxdist)
                q += ac;
            else if (dist > 0.0f)
                q += (dist / maxdist) * ac;

            return q;
        }

        /// <summary>
        /// Indica el punto mas cercano a p
        /// </summary>
        /// <param name="p">Punto a testear</param>
        /// <param name="points">Array de puntos del cual se quiere buscar el mas cercano</param>
        /// <param name="minDistSq">Distancia al cuadrado del punto mas cercano</param>
        /// <returns>Punto más cercano a p del array</returns>
        public static Vector3 closestPoint(Vector3 p, Vector3[] points, out float minDistSq)
        {
            Vector3 min = points[0];
            Vector3 diffVec = points[0] - p;
            minDistSq = diffVec.LengthSq();
            float distSq;
            for (int i = 1; i < points.Length; i++)
            {
                diffVec = points[i] - p;
                distSq = diffVec.LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    min = points[i];
                }
            }

            return min;
        }

        

        
        #endregion


        #region Frustum

        
        /// <summary>
        /// Clasifica un BoundingBox respecto del Frustum
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>Resultado de la clasificación</returns>
        public static FrustumResult classifyFrustumAABB(TgcFrustum frustum, TgcBoundingBox aabb)
        {
	        int totalIn = 0;
            Plane[] frustumPlanes = frustum.FrustumPlanes;

	        // get the corners of the box into the vCorner array
	        Vector3[] aabbCorners = aabb.computeCorners();

	        // test all 8 corners against the 6 sides 
	        // if all points are behind 1 specific plane, we are out
	        // if we are in with all points, then we are fully in
	        for(int p = 0; p < 6; ++p) 
            {
		        int inCount = 8;
		        int ptIn = 1;

		        for(int i = 0; i < 8; ++i) 
                {
			        // test this point against the planes
                    if (classifyPointPlane(aabbCorners[i], frustumPlanes[p]) == PointPlaneResult.BEHIND)
                    {
			            ptIn = 0;
				        --inCount;
			        }
		        }

		        // were all the points outside of plane p?
                if (inCount == 0)
                {
                    return FrustumResult.OUTSIDE;
                }
			        
		        // check if they were all on the right side of the plane
		        totalIn += ptIn;
	        }

	        // so if iTotalIn is 6, then all are inside the view
            if (totalIn == 6)
            {
                return FrustumResult.INSIDE;
            }

	        // we must be partly in then otherwise
            return FrustumResult.INTERSECT;
        }


        /*
        classifyFrustumAABB SUPUESTAMENTE ES MAS RAPIDO PERO NO FUNCIONA BIEN
        
        /// <summary>
        /// Clasifica un BoundingBox respecto del Frustum
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>Resultado de la colisión</returns>
        public static FrustumResult classifyFrustumAABB(TgcFrustum frustum, TgcBoundingBox aabb)
        {
            bool intersect = false;
            FrustumResult result = FrustumResult.OUTSIDE;
            Vector3 minExtreme;
            Vector3 maxExtreme;
            Plane[] m_frustumPlanes = frustum.FrustumPlanes;


            for (int i = 0; i < 6 ; i++ )
            {
                if (m_frustumPlanes[i].A <= 0)
                {
                   minExtreme.X = aabb.PMin.X;
                   maxExtreme.X = aabb.PMax.X;
                }
                else
                {
                    minExtreme.X = aabb.PMax.X;
                   maxExtreme.X = aabb.PMin.X;
                }

                if (m_frustumPlanes[i].B <= 0)
                {
                    minExtreme.Y = aabb.PMin.Y;
                    maxExtreme.Y = aabb.PMax.Y;
                }
                else
                {
                    minExtreme.Y = aabb.PMax.Y;
                   maxExtreme.Y = aabb.PMin.Y;
                }

                if (m_frustumPlanes[i].C <= 0)
                {
                    minExtreme.Z = aabb.PMin.Z;
                    maxExtreme.Z = aabb.PMax.Z;
                }
                else
                {
                    minExtreme.Z = aabb.PMax.Z;
                    maxExtreme.Z = aabb.PMin.Z; 
                }

                if (classifyPointPlane(minExtreme, m_frustumPlanes[i]) == PointPlaneResult.IN_FRONT_OF)
                {
                    result = FrustumResult.OUTSIDE;
                    return result;
                }

                if (classifyPointPlane(maxExtreme, m_frustumPlanes[i]) != PointPlaneResult.BEHIND)
                {
                    intersect = true;
                }    
            }

            if (intersect)
            {
                result = FrustumResult.INTERSECT;
            }
            else
            {
                result = FrustumResult.INSIDE;
            }
                
            return result;
        }
        */


        /// <summary>
        /// Resultado de una colisión entre un objeto y el Frustum
        /// </summary>
        public enum FrustumResult
        {
            /// <summary>
            /// El objeto se encuentra completamente fuera del Frustum
            /// </summary>
            OUTSIDE,

            /// <summary>
            /// El objeto se encuentra completamente dentro del Frustum
            /// </summary>
            INSIDE,

            /// <summary>
            /// El objeto posee parte fuera y parte dentro del Frustum
            /// </summary>
            INTERSECT,
        }
        
        /// <summary>
        /// Indica si un Punto colisiona con el Frustum
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="p">Punto</param>
        /// <returns>True si el Punto está adentro del Frustum</returns>
        public static bool testPointFrustum(TgcFrustum frustum, Vector3 p) {

            bool result = true;
            Plane[] frustumPlanes = frustum.FrustumPlanes;

		    for(int i=0; i < 6; i++) 
            {
                if (distPointPlane(p, frustumPlanes[i]) < 0)
                {
                    return false;
                }
		    }
            return result;

	    }

        /// <summary>
        /// Indica si un BoundingSphere colisiona con el Frustum
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="sphere">BoundingSphere</param>
        /// <returns>Resultado de la colisión</returns>
        public static FrustumResult classifyFrustumSphere(TgcFrustum frustum, TgcBoundingSphere sphere)
        {
            float distance;
            FrustumResult result = FrustumResult.INSIDE;
            Plane[] frustumPlanes = frustum.FrustumPlanes;

            for (int i = 0; i < 6; i++)
            {
                distance = distPointPlane(sphere.Center, frustumPlanes[i]);

                if (distance < - sphere.Radius)
                {
                    return FrustumResult.OUTSIDE;
                }
                else if (distance < sphere.Radius)
                {
                    result = FrustumResult.INTERSECT;
                }
                    
            }
            return result;
        }



        #endregion


        #region ConvexPolyhedron

        /// <summary>
        /// Clasifica un BoundingBox respecto de un Cuerpo Convexo.
        /// Los planos del Cuerpo Convexo deben apuntar hacia adentro.
        /// </summary>
        /// <param name="polyhedron">Cuerpo convexo</param>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>Resultado de la clasificación</returns>
        public static ConvexPolyhedronResult classifyConvexPolyhedronAABB(TgcConvexPolyhedron polyhedron, TgcBoundingBox aabb)
        {
            int totalIn = 0;
            Plane[] polyhedronPlanes = polyhedron.Planes;

            // get the corners of the box into the vCorner array
            Vector3[] aabbCorners = aabb.computeCorners();

            // test all 8 corners against the polyhedron sides 
            // if all points are behind 1 specific plane, we are out
            // if we are in with all points, then we are fully in
            for (int p = 0; p < polyhedronPlanes.Length; ++p)
            {
                int inCount = 8;
                int ptIn = 1;

                for (int i = 0; i < 8; ++i)
                {
                    // test this point against the planes
                    if (classifyPointPlane(aabbCorners[i], polyhedronPlanes[p]) == PointPlaneResult.BEHIND)
                    {
                        ptIn = 0;
                        --inCount;
                    }
                }

                // were all the points outside of plane p?
                if (inCount == 0)
                {
                    return ConvexPolyhedronResult.OUTSIDE;
                }

                // check if they were all on the right side of the plane
                totalIn += ptIn;
            }

            // so if iTotalIn is 6, then all are inside the view
            if (totalIn == 6)
            {
                return ConvexPolyhedronResult.INSIDE;
            }

            // we must be partly in then otherwise
            return ConvexPolyhedronResult.INTERSECT;
        }

        /// <summary>
        /// Resultado de una colisión entre un objeto y un Cuerpo Convexo
        /// </summary>
        public enum ConvexPolyhedronResult
        {
            /// <summary>
            /// El objeto se encuentra completamente fuera del Cuerpo Convexo
            /// </summary>
            OUTSIDE,

            /// <summary>
            /// El objeto se encuentra completamente dentro del Cuerpo Convexo
            /// </summary>
            INSIDE,

            /// <summary>
            /// El objeto posee parte fuera y parte dentro del Cuerpo Convexo
            /// </summary>
            INTERSECT,
        }

        /// <summary>
        /// Clasifica un punto respecto de un Cuerpo Convexo de N caras.
        /// Puede devolver OUTSIDE o INSIDE (si es coincidente se considera como INSIDE).
        /// Los planos del Cuerpo Convexo deben apuntar hacia adentro.
        /// </summary>
        /// <param name="q">Punto a clasificar</param>
        /// <param name="polyhedron">Cuerpo Convexo</param>
        /// <returns>Resultado de la clasificación</returns>
        public static ConvexPolyhedronResult classifyPointConvexPolyhedron(Vector3 q, TgcConvexPolyhedron polyhedron)
        {
            bool fistTime = true;
            PointPlaneResult lastC = PointPlaneResult.BEHIND;
            PointPlaneResult c;

            for (int i = 0; i < polyhedron.Planes.Length; i++)
            {
                c = TgcCollisionUtils.classifyPointPlane(q, polyhedron.Planes[i]);

                if (c == PointPlaneResult.COINCIDENT)
                    continue;

                //guardar clasif para primera vez
                if (fistTime)
                {
                    fistTime = false;
                    lastC = c;
                }


                //comparar con ultima clasif
                if (c != lastC)
                {
                    //basta con que haya una distinta para que este Afuera
                    return ConvexPolyhedronResult.OUTSIDE;
                }
            }

            //Si todos dieron el mismo resultado, entonces esta adentro
            return ConvexPolyhedronResult.INSIDE;
        }

        /// <summary>
        /// Indica si un punto se encuentra dentro de un Cuerpo Convexo.
        /// Los planos del Cuerpo Convexo deben apuntar hacia adentro.
        /// Es más ágil que llamar a classifyPointConvexPolyhedron()
        /// </summary>
        /// <param name="q">Punto a clasificar</param>
        /// <param name="polyhedron">Cuerpo Convexo</param>
        /// <returns>True si se encuentra adentro.</returns>
        public static bool testPointConvexPolyhedron(Vector3 q, TgcConvexPolyhedron polyhedron)
        {
            for (int i = 0; i < polyhedron.Planes.Length; i++)
            {
                //Si el punto está detrás de algún plano, entonces está afuera
                if (TgcCollisionUtils.classifyPointPlane(q, polyhedron.Planes[i]) == PointPlaneResult.BEHIND)
                {
                    return false;
                }
            }
            //Si está delante de todos los planos, entonces está adentro.
            return true;
        }

        #endregion


        #region Convex Polygon

        /// <summary>
        /// Recorta un polígono convexo en 3D por un plano.
        /// Devuelve el nuevo polígono recortado.
        /// Algoritmo de Sutherland-Hodgman
        /// </summary>
        /// <param name="poly">Vértices del polígono a recortar</param>
        /// <param name="p">Plano con el cual se recorta</param>
        /// <param name="clippedPoly">Vértices del polígono recortado></param>
        /// <returns>True si el polígono recortado es válido. False si está degenerado</returns>
        public static bool clipConvexPolygon(Vector3[] polyVertices, Plane p, out Vector3[] clippedPolyVertices)
        {
            int thisInd = polyVertices.Length - 1;
            PointPlaneResult thisRes = classifyPointPlane(polyVertices[thisInd], p);
            List<Vector3> outVert = new List<Vector3>(polyVertices.Length);
            float t;

            for (int nextInd = 0; nextInd < polyVertices.Length; nextInd++)
			{
                PointPlaneResult nextRes = classifyPointPlane(polyVertices[nextInd], p);
                if(thisRes == PointPlaneResult.IN_FRONT_OF || thisRes == PointPlaneResult.COINCIDENT)
                {
                    // Add the point
                    outVert.Add(polyVertices[thisInd]);
                }

                if((thisRes == PointPlaneResult.BEHIND && nextRes == PointPlaneResult.IN_FRONT_OF) ||
                    thisRes == PointPlaneResult.IN_FRONT_OF && nextRes == PointPlaneResult.BEHIND)
                {
                    // Add the split point
                    Vector3 q;
                    intersectSegmentPlane(polyVertices[thisInd], polyVertices[nextInd], p, out t, out q);
                    outVert.Add(q);
                }

                thisInd = nextInd;
                thisRes = nextRes;
			}

            //Polígono válido
            if (outVert.Count >= 3)
            {
                clippedPolyVertices = outVert.ToArray();
                return true;
            }

            //Polígono degenerado
            clippedPolyVertices = null;
            return false;
        }

        #endregion


        #region Triangle


        /// <summary>
        /// Indica si un punto p en el espacio se encuentra dentro de un triangulo (a, b, c)
        /// </summary>
        /// <param name="p">Punto a probar</param>
        /// <param name="a">Vertice A del triangulo</param>
        /// <param name="b">Vertice B del triangulo</param>
        /// <param name="c">Vertice C del triangulo</param>
        /// <returns>True si el punto pertenece al triangulo</returns>
        public static bool testPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            // Translate point and triangle so that point lies at origin
            a -= p; b -= p; c -= p;
            // Compute normal vectors for triangles pab and pbc
            Vector3 u = Vector3.Cross(b, c);
            Vector3 v = Vector3.Cross(c, a);
            // Make sure they are both pointing in the same direction
            if (Vector3.Dot(u, v) < 0.0f) return false;
            // Compute normal vector for triangle pca
            Vector3 w = Vector3.Cross(a, b);
            // Make sure it points in the same direction as the first two
            if (Vector3.Dot(u, w) < 0.0f) return false;
            // Otherwise P must be in (or on) the triangle
            return true;
        }

        /// <summary>
        /// Detecta colision entre un segmento pq y un triangulo abc.
        /// Devuelve true si hay colision y carga las coordenadas barycentricas (u,v,w) de la colision, el
        /// instante t de colision y el punto c de colision.
        /// Basado en: Real Time Collision Detection pag 191
        /// </summary>
        /// <param name="p">Inicio del segmento</param>
        /// <param name="q">Fin del segmento</param>
        /// <param name="a">Vertice 1 del triangulo</param>
        /// <param name="b">Vertice 2 del triangulo</param>
        /// <param name="c">Vertice 3 del triangulo</param>
        /// <param name="uvw">Coordenadas barycentricas de colision</param>
        /// <param name="t">Instante de colision</param>
        /// <param name="col">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectSegmentTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c, out Vector3 uvw, out float t, out Vector3 col)
        {
            float u;
            float v;
            float w;
            uvw = Vector3.Empty;
            col = Vector3.Empty;
            t = -1;

            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 qp = p - q;

            // Compute triangle normal. Can be precalculated or cached if
            // intersecting multiple segments against the same triangle
            Vector3 n = Vector3.Cross(ab, ac);

            // Compute denominator d. If d <= 0, segment is parallel to or points
            // away from triangle, so exit early
            float d = Vector3.Dot(qp, n);
            if (d <= 0.0f) return false;

            // Compute intersection t value of pq with plane of triangle. A ray
            // intersects iff 0 <= t. Segment intersects iff 0 <= t <= 1. Delay
            // dividing by d until intersection has been found to pierce triangle
            Vector3 ap = p - a;
            t = Vector3.Dot(ap, n);
            if (t < 0.0f) return false;
            if (t > d) return false; // For segment; exclude this code line for a ray test

            // Compute barycentric coordinate components and test if within bounds
            Vector3 e = Vector3.Cross(qp, ap);
            v = Vector3.Dot(ac, e);
            if (v < 0.0f || v > d) return false;
            w = -Vector3.Dot(ab, e);
            if (w < 0.0f || v + w > d) return false;

            // Segment/ray intersects triangle. Perform delayed division and
            // compute the last barycentric coordinate component
            float ood = 1.0f / d;
            t *= ood;
            v *= ood;
            w *= ood;
            u = 1.0f - v - w;

            uvw.X = u;
            uvw.Y = v;
            uvw.Z = w;
            col = p + t * (p - q);
            return true;
        }

        /// <summary>
        /// Interseccion entre una Linea pq y un Triangulo abc
        /// Devuelve true si hay colision y carga las coordenadas barycentricas (u,v,w) de la colision, el
        /// instante t de colision y el punto c de colision.
        /// Basado en: Real Time Collision Detection pag 186
        /// </summary>
        /// <param name="p">Inicio del segmento</param>
        /// <param name="q">Fin del segmento</param>
        /// <param name="a">Vertice 1 del triangulo</param>
        /// <param name="b">Vertice 2 del triangulo</param>
        /// <param name="c">Vertice 3 del triangulo</param>
        /// <param name="uvw">Coordenadas barycentricas de colision</param>
        /// <param name="t">Instante de colision</param>
        /// <param name="col">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectLineTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c, out Vector3 uvw, out float t, out Vector3 col)
        {
            float u;
            float v;
            float w;
            uvw = Vector3.Empty;
            col = Vector3.Empty;
            t = -1;

            Vector3 pq = q - p;
            Vector3 pa = a - p;
            Vector3 pb = b - p;
            Vector3 pc = c - p;

            /*
            // Test if pq is inside the edges bc, ca and ab. Done by testing
            // that the signed tetrahedral volumes, computed using scalar triple
            // products, are all positive
            u = scalarTriple(pq, pc, pb);
            if (u < 0.0f) return false;
            v = ScalarTriple(pq, pa, pc);
            if (v < 0.0f) return false;
            w = scalarTriple(pq, pb, pa);
            if (w < 0.0f) return false;
            */

            //For a double-sided test the same code would instead read:
            Vector3 m = Vector3.Cross(pq, pc);
            u = Vector3.Dot(pb, m); // scalarTriple(pq, pc, pb);
            v = -Vector3.Dot(pa, m); // scalarTriple(pq, pa, pc);
            if (!TgcCollisionUtils.sameSign(u, v)) return false;
            w = TgcCollisionUtils.scalarTriple(pq, pb, pa);
            if (!TgcCollisionUtils.sameSign(u, w)) return false;

            // Compute the barycentric coordinates (u, v, w) determining the
            // intersection point r, r = u*a + v*b + w*c
            float denom = 1.0f / (u + v + w);
            u *= denom;
            v *= denom;
            w *= denom; // w = 1.0f - u - v;

            uvw.X = u;
            uvw.Y = v;
            uvw.Z = w;
            col = p + t * pq;
            return true;
        }


        #endregion


        #region Cylinder

        /// <summary>
        /// Indica si un rayo colisiona con un cilindro.
        /// </summary>
        /// <param name="ray">Rayo</param>
        /// <param name="cylinder">Cilindro orientado</param>
        /// <returns>True si el rayo colisiona con el cilindro</returns>
        public static bool testRayCylinder(TgcRay ray, TgcBoundingCylinder cylinder)
        {
            Matrix transformation = cylinder.AntiTransformationMatrix;
            Vector3 origin = Vector3.TransformCoordinate(ray.Origin, transformation);
            Vector3 direction = Vector3.TransformNormal(ray.Direction, transformation);

            return TgcCollisionUtils.testRayCylinder(origin, direction);
        }

        /// <summary>
        /// Indica si un Punto colisiona con un Cilindro.
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylinder">Cilindro</param>
        /// <returns>True si el Punto esta adentro del Cilindro</returns>
        public static bool testPointCylinder(Vector3 p, TgcBoundingCylinder cylinder)
        {
            Vector3 uvwPoint = Vector3.TransformCoordinate(p, cylinder.AntiRotationMatrix);
            return TgcCollisionUtils.testPointCylinder(uvwPoint, cylinder.Center, cylinder.HalfLength, cylinder.Radius);
        }

        /// <summary>
        /// Determina el punto del cilindro mas cercano al punto p.
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylinder">Cilindro orientable</param>
        /// <returns>Punto del cilindro que esta mas cerca de p</returns>
        public static Vector3 closestPointCylinder(Vector3 p, TgcBoundingCylinder cylinder)
        {
            //transformamos el punto a coordenadas uvw del cilindro
            Matrix transformation = cylinder.AntiRotationMatrix;
            Vector3 uvwPoint = Vector3.TransformCoordinate(p, transformation);
            //buscamos el punto mas cercano en uvw
            Vector3 uvwResult = TgcCollisionUtils.closestPointCylinder(uvwPoint, cylinder.Center, cylinder.HalfLength, cylinder.Radius);
            //transformamos ese resultado a xyz
            transformation.Invert();
            return Vector3.TransformCoordinate(uvwResult, transformation);
        }

        /// <summary>
        /// Indica si un Cilindro colisiona con una Esfera.
        /// Solo indica si hay colision o no. No va mas en detalle.
        /// </summary>
        /// <param name="sphere">Esfera</param>
        /// <param name="cylinder">Cilindro orientable</param>
        /// <returns>True si hay colision</returns>
        public static bool testSphereCylinder(TgcBoundingSphere sphere, TgcBoundingCylinder cylinder)
        {
            //transformamos la posicion de la esfera a coordenadas uvw
            Vector3 uvwSphereCenter = Vector3.TransformCoordinate(sphere.Center, cylinder.AntiRotationMatrix);
            //nos fijamos si hay colision en el espacio uvw
            return TgcCollisionUtils.testSphereCylinder(
                uvwSphereCenter, sphere.Radius,
                cylinder.Center, cylinder.HalfLength, cylinder.Radius);
        }


        /// <summary>
        /// Indica si un cilindro colisiona con un segmento.
        /// El cilindro se especifica con dos puntos centrales "cylinderInit" y "cylinderEnd" que forman una recta y con un radio "radius".
        /// Si hay colision se devuelve el instante de colision "t" y el punto de colision "q"
        /// </summary>
        /// <param name="segmentInit">Punto de inicio del segmento</param>
        /// <param name="segmentEnd">Punto de fin del segmento</param>
        /// <param name="cylinderInit">Punto inicial del cilindro</param>
        /// <param name="cylinderEnd">Punto final del cilindro</param>
        /// <param name="radius">Radio del cilindro</param>
        /// <param name="t">Instante de colision</param>
        /// <param name="q">Punto de colision</param>
        /// <returns>True si hay colision</returns>
        public static bool intersectSegmentCylinder(Vector3 segmentInit, Vector3 segmentEnd, TgcBoundingCylinder cylinder, out float t, out Vector3 q)
        //(Vector3 segmentInit, Vector3 segmentEnd, Vector3 cylinderInit, Vector3 cylinderEnd, float radius, out float t, out Vector3 q)
        {
            Vector3 hh = cylinder.HalfHeight;
            Vector3 cylinderInit = cylinder.Center - hh;
            Vector3 cylinderEnd = cylinder.Center + hh;
            float radius = cylinder.Radius;

            t = -1;
            q = Vector3.Empty;

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
                q = segmentInit + t * n;
                return true;
            }
            float b = dd * mn - nd * md;
            float discr = b * b - a * c;
            if (discr < 0.0f) return false; // No real roots; no intersection
            t = (-b - FastMath.Sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f) return false; // Intersection lies outside segment

            if (md + t * nd < 0.0f)
            {
                // Intersection outside cylinder on 'p' side
                if (nd <= 0.0f) return false; // Segment pointing away from endcap
                t = -md / nd;
                // Keep intersection if Dot(S(t) - p, S(t) - p) <= r^2
                return k + t * (2.0f * mn + t * nn) <= 0.0f;
            }
            else if (md + t * nd > dd)
            {
                // Intersection outside cylinder on 'q' side
                if (nd >= 0.0f) return false; // Segment pointing away from endcap
                t = (dd - md) / nd;
                // Keep intersection if Dot(S(t) - q, S(t) - q) <= r^2
                return k + dd - 2.0f * md + t * (2.0f * (mn - nd) + t * nn) <= 0.0f;
            }

            // Segment intersects cylinder between the endcaps; t is correct
            q = segmentInit + t * n;
            return true;
        }

        #endregion


        #region FixedYCylinder

        /// <summary>
        /// Indica si un rayo colisiona con un cilindro.
        /// </summary>
        /// <param name="ray">Rayo</param>
        /// <param name="cylinder">Cilindro alineado</param>
        /// <returns>True si el rayo colisiona con el cilindro</returns>
        public static bool testRayCylinder(TgcRay ray, TgcFixedYBoundingCylinder cylinder)
        {
            Matrix transformation = cylinder.AntiTransformationMatrix;
            Vector3 origin = Vector3.TransformCoordinate(ray.Origin, transformation);
            Vector3 direction = Vector3.TransformNormal(ray.Direction, transformation);

            return TgcCollisionUtils.testRayCylinder(origin, direction);
        }

        /// <summary>
        /// Indica si un Punto colisiona con un Cilindro.
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylinder">Cilindro alineado</param>
        /// <returns>True si el Punto esta adentro del Cilindro</returns>
        private static bool testPointCylinder(Vector3 p, TgcFixedYBoundingCylinder cylinder)
        {
            return TgcCollisionUtils.testPointCylinder(p, cylinder.Center, cylinder.HalfLength, cylinder.Radius);
        }

        /// <summary>
        /// Determina el punto mas cercano de un cilindro al punto P especificado
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylinder">Cilindro alineado</param>
        /// <returns>Punto perteneciente al cilindro mas cercano a P</returns>
        public static Vector3 closestPointCylinder(Vector3 p, TgcFixedYBoundingCylinder cylinder)
        {
            return TgcCollisionUtils.closestPointCylinder(p, cylinder.Center, cylinder.HalfLength, cylinder.Radius);
        }

        /// <summary>
        /// Indica si un Cilindro colisiona con una Esfera.
        /// Solo indica si hay colision o no. No va mas en detalle.
        /// </summary>
        /// <param name="sphere">Esfera</param>
        /// <param name="cylinder">Cilindro alineado</param>
        /// <returns>True si hay colision</returns>
        public static bool testSphereCylinder(TgcBoundingSphere sphere, TgcFixedYBoundingCylinder cylinder)
        {
            return TgcCollisionUtils.testSphereCylinder(
                sphere.Center, sphere.Radius,
                cylinder.Center, cylinder.HalfLength, cylinder.Radius);
        }

        /// <summary>
        /// Indica si un Cilindro colisiona con un AABB.
        /// Solo indica si hay colision o no. No va mas en detalle.
        /// </summary>
        /// <param name="box">AABB</param>
        /// <param name="cylinder">Cilindro alineado</param>
        /// <returns>True si hay colision</returns>
        public static bool testAABBCylinder(TgcBoundingBox box, TgcFixedYBoundingCylinder cylinder)
        {
            //datos del aabb
            Vector3 boxCenter = box.calculateBoxCenter();
            Vector3 boxHalfSize = box.calculateSize() * 0.5f;

            //datos del cilindro
            Vector3 cylCenter = cylinder.Center;
            float cylHalfLength = cylinder.HalfLength;
            float cylRadius = cylinder.Radius;

            //vector de distancias
            Vector3 distances = new Vector3(
                FastMath.Abs(boxCenter.X - cylCenter.X),
                FastMath.Abs(boxCenter.Y - cylCenter.Y),
                FastMath.Abs(boxCenter.Z - cylCenter.Z));

            //si el aabb esta muy arriba o muy abajo no hay colision
            if (distances.Y > boxHalfSize.Y + cylHalfLength) return false;

            //si el aabb esta muy lejos en x o en z no hay colision
            if (distances.X > boxHalfSize.X + cylRadius) return false;
            if (distances.Z > boxHalfSize.Z + cylRadius) return false;

            //si el centro del cilindro esta dentro del aabb hay colision
            if (distances.X <= boxHalfSize.X) return true;
            if (distances.Z <= boxHalfSize.Z) return true;

            //si el cilindro toca alguno de los extremos hay colision
            float cornerDistanceSq =
                FastMath.Pow2(distances.X - boxHalfSize.X) +
                FastMath.Pow2(distances.Z - boxHalfSize.Z);
            return (cornerDistanceSq <= FastMath.Pow2(cylRadius));
        }

        /// <summary>
        /// Indica si un Cilindro colisiona con otro Cilindro.
        /// Solo indica si hay colision o no. No va mas en detalle.
        /// </summary>
        /// <param name="collider">Cilindro alineado que genera la colision</param>
        /// <param name="collisionable">Cilindro alineado estatico</param>
        /// <returns>True si hay colision</returns>
        public static bool testCylinderCylinder(TgcFixedYBoundingCylinder collider, TgcFixedYBoundingCylinder collisionable)
        {
            Vector3 centerToCenter = collider.Center - collisionable.Center;
            if (FastMath.Pow2(centerToCenter.X) + FastMath.Pow2(centerToCenter.Z) > FastMath.Pow2(collisionable.Radius + collider.Radius)) return false;
            if (FastMath.Abs(centerToCenter.Y) > collider.HalfLength + collisionable.HalfLength) return false;
            return true;
        }

        /// <summary>
        /// Indica si un rayo colisiona con el cilindro de radio 1, altura 2, y centro en el origen de coordenadas.
        /// </summary>
        /// <param name="origin">Origen del rayo</param>
        /// <param name="direction">Vector director del rayo</param>
        /// <returns>True si el rayo colisiona con el cilindro</returns>
        private static bool testRayCylinder(Vector3 origin, Vector3 direction)
        {
            float x0 = origin.X, xt = direction.X;
            float y0 = origin.Y, yt = direction.Y;
            float z0 = origin.Z, zt = direction.Z;

            //nota: esta solucion esta planteada con las siguientes ecuaciones
            //x^2 + z^2 = 1, -1 <= y <= 1 para el cilindro
            //(x,y,z) = (x0,y0,z0)+t(xt,yt,zt) para el rayo

            float t1, t2;

            if (yt == 0)
            {
                if (y0 > 1) return false;
                if (y0 < -1) return false;
                t1 = float.MinValue;
                t2 = float.MaxValue;
            }
            else
            {
                t1 = FastMath.Min((-1 - y0) / yt, (1 - y0) / yt);
                t2 = FastMath.Max((-1 - y0) / yt, (1 - y0) / yt);
            }

            float a = xt * xt + zt * zt,
                b = 2 * x0 * xt + 2 * z0 * zt,
                c = x0 * x0 + z0 * z0 - 1;

            float raiz = b * b - 4 * a * c;

            if (raiz < 0) return false;
            else if (raiz == 0)
            {
                float t = -b / (2 * a);
                return t >= t1 && t <= t2;
            }
            else
            {
                float up = -b;
                float down = 2 * a;
                float sqrt = FastMath.Sqrt(raiz);

                float t3, t4;
                t3 = FastMath.Min((up + sqrt) / down, (up - sqrt) / down);
                t4 = FastMath.Max((up + sqrt) / down, (up - sqrt) / down);

                if (t3 <= t1 && t4 >= t2) return true;
                if (t3 >= t1 && t3 <= t2) return true;
                if (t4 >= t1 && t4 <= t2) return true;
                return false;
            }
        }

        /// <summary>
        /// Indica si un Punto colisiona con un Cilindro.
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylCenter">Centro o posicion del cilindro</param>
        /// <param name="cylHalfLength">Media altura del cilindro</param>
        /// <param name="cylRadius">Radio del cilindro</param>
        /// <returns>True si el Punto esta adentro del Cilindro</returns>
        private static bool testPointCylinder(Vector3 p, Vector3 cylCenter, float cylHalfLength, float cylRadius)
        {
            if (FastMath.Abs(cylCenter.Y - p.Y) > cylHalfLength) return false;
            Vector3 centerToPoint = p - cylCenter;
            centerToPoint.Y = 0;
            if (centerToPoint.LengthSq() > FastMath.Pow2(cylRadius)) return false;
            return true;
        }

        /// <summary>
        /// Determina el punto mas cercano de un cilindro al punto P especificado
        /// </summary>
        /// <param name="p">Punto</param>
        /// <param name="cylCenter">Centro o posicion del cilindro</param>
        /// <param name="cylHalfLength">Media altura del cilindro</param>
        /// <param name="cylRadius">Radio del cilindro</param>
        /// <returns>Punto perteneciente al cilindro mas cercano a P</returns>
        private static Vector3 closestPointCylinder(Vector3 p, Vector3 cylCenter, float cylHalfLength, float cylRadius)
        {
            Vector3 direction = p - cylCenter;
            direction.Y = 0;
            if (direction.LengthSq() > FastMath.Pow2(cylRadius))
            {
                direction.Normalize();
                direction *= cylRadius;
            }

            float distanceY = p.Y - cylCenter.Y;
            if (FastMath.Abs(distanceY) > cylHalfLength)
                return cylCenter + new Vector3(0, cylHalfLength, 0) * Math.Sign(distanceY) + direction;
            else
                return cylCenter + new Vector3(0, distanceY, 0) + direction;
        }

        /// <summary>
        /// Indica si un Cilindro colisiona con una Esfera.
        /// Solo indica si hay colision o no. No va mas en detalle.
        /// </summary>
        /// <param name="sphere">Esfera</param>
        /// <param name="cylCenter">Centro o posicion del cilindro</param>
        /// <param name="cylHalfLength">Media altura del cilindro</param>
        /// <param name="cylRadius">Radio del cilindro</param>
        /// <returns>True si hay colision</returns>
        private static bool testSphereCylinder(Vector3 sphereCenter, float sphereRadius, Vector3 cylCenter, float cylHalfLength, float cylRadius)
        {
            //si la esfera esta muy arriba o muy abajo no hay colision
            if (FastMath.Abs(sphereCenter.Y - cylCenter.Y) > cylHalfLength + sphereRadius) return false;

            Vector2 sphereXZ = new Vector2(sphereCenter.X, sphereCenter.Z);
            Vector2 cylXZ = new Vector2(cylCenter.X, cylCenter.Z);

            //si estan muy lejos en el plano XZ no hay colision
            if ((cylXZ - sphereXZ).LengthSq() > FastMath.Pow2(cylRadius + sphereRadius)) return false;

            //si el centro de la esfera esta dentro del cilindro en Y hay colision
            if (FastMath.Abs(cylCenter.Y - sphereCenter.Y) < cylHalfLength) return true;

            //vemos si el punto mas cercano al centro de la esfera pertenece a esta
            Vector3 point = sphereCenter - cylCenter;
            point.Y = 0;
            point.Normalize();
            point *= cylRadius;
            point.Y = cylHalfLength * Math.Sign(sphereCenter.Y - cylCenter.Y);
            point += cylCenter;
            return (point - sphereCenter).LengthSq() <= FastMath.Pow2(sphereRadius);
        }

        #endregion


        #region OBB

        /// <summary>
        /// Testear si hay olision entre dos OBB
        /// </summary>
        /// <param name="a">Primer OBB</param>
        /// <param name="b">Segundo OBB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbObb(TgcObb a, TgcObb b)
        {
            return TgcCollisionUtils.testObbObb(a.toStruct(), b.toStruct());
        }

        /// <summary>
        /// Testear si hay olision entre dos OBB
        /// </summary>
        /// <param name="a">Primer OBB</param>
        /// <param name="b">Segundo OBB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbObb(TgcObb.OBBStruct a, TgcObb.OBBStruct b)
        {
            float ra, rb;
            float[,] R = new float[3, 3];
            float[,] AbsR = new float[3, 3];
            float[] ae = toArray(a.extents);
            float[] be = toArray(b.extents);


            // Compute rotation matrix expressing b in a’s coordinate frame
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    R[i, j] = Vector3.Dot(a.orientation[i], b.orientation[j]);

            // Compute translation vector t
            Vector3 tVec = b.center - a.center;
            // Bring translation into a’s coordinate frame
            float[] t = new float[3];
            t[0] = Vector3.Dot(tVec, a.orientation[0]);
            t[1] = Vector3.Dot(tVec, a.orientation[1]);
            t[2] = Vector3.Dot(tVec, a.orientation[2]);

            // Compute common subexpressions. Add in an epsilon term to
            // counteract arithmetic errors when two edges are parallel and
            // their cross product is (near) null (see text for details)
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    AbsR[i, j] = FastMath.Abs(R[i, j]) + float.Epsilon;

            // Test axes L = A0, L = A1, L = A2
            for (int i = 0; i < 3; i++)
            {
                ra = ae[i];
                rb = be[0] * AbsR[i, 0] + be[1] * AbsR[i, 1] + be[2] * AbsR[i, 2];
                if (FastMath.Abs(t[i]) > ra + rb) return false;
            }

            // Test axes L = B0, L = B1, L = B2
            for (int i = 0; i < 3; i++)
            {
                ra = ae[0] * AbsR[0, i] + ae[1] * AbsR[1, i] + ae[2] * AbsR[2, i];
                rb = be[i];
                if (FastMath.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]) > ra + rb) return false;
            }

            // Test axis L = A0 x B0
            ra = ae[1] * AbsR[2, 0] + ae[2] * AbsR[1, 0];
            rb = be[1] * AbsR[0, 2] + be[2] * AbsR[0, 1];
            if (FastMath.Abs(t[2] * R[1, 0] - t[1] * R[2, 0]) > ra + rb) return false;

            // Test axis L = A0 x B1
            ra = ae[1] * AbsR[2, 1] + ae[2] * AbsR[1, 1];
            rb = be[0] * AbsR[0, 2] + be[2] * AbsR[0, 0];
            if (FastMath.Abs(t[2] * R[1, 1] - t[1] * R[2, 1]) > ra + rb) return false;

            // Test axis L = A0 x B2
            ra = ae[1] * AbsR[2, 2] + ae[2] * AbsR[1, 2];
            rb = be[0] * AbsR[0, 1] + be[1] * AbsR[0, 0];
            if (FastMath.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > ra + rb) return false;

            // Test axis L = A1 x B0
            ra = ae[0] * AbsR[2, 0] + ae[2] * AbsR[0, 0];
            rb = be[1] * AbsR[1, 2] + be[2] * AbsR[1, 1];
            if (FastMath.Abs(t[0] * R[2, 0] - t[2] * R[0, 0]) > ra + rb) return false;

            // Test axis L = A1 x B1
            ra = ae[0] * AbsR[2, 1] + ae[2] * AbsR[0, 1];
            rb = be[0] * AbsR[1, 2] + be[2] * AbsR[1, 0];
            if (FastMath.Abs(t[0] * R[2, 1] - t[2] * R[0, 1]) > ra + rb) return false;

            // Test axis L = A1 x B2
            ra = ae[0] * AbsR[2, 2] + ae[2] * AbsR[0, 2];
            rb = be[0] * AbsR[1, 1] + be[1] * AbsR[1, 0];
            if (FastMath.Abs(t[0] * R[2, 2] - t[2] * R[0, 2]) > ra + rb) return false;

            // Test axis L = A2 x B0
            ra = ae[0] * AbsR[1, 0] + ae[1] * AbsR[0, 0];
            rb = be[1] * AbsR[2, 2] + be[2] * AbsR[2, 1];
            if (FastMath.Abs(t[1] * R[0, 0] - t[0] * R[1, 0]) > ra + rb) return false;

            // Test axis L = A2 x B1
            ra = ae[0] * AbsR[1, 1] + ae[1] * AbsR[0, 1];
            rb = be[0] * AbsR[2, 2] + be[2] * AbsR[2, 0];
            if (FastMath.Abs(t[1] * R[0, 1] - t[0] * R[1, 1]) > ra + rb) return false;

            // Test axis L = A2 x B2
            ra = ae[0] * AbsR[1, 2] + ae[1] * AbsR[0, 2];
            rb = be[0] * AbsR[2, 1] + be[1] * AbsR[2, 0];
            if (FastMath.Abs(t[1] * R[0, 2] - t[0] * R[1, 2]) > ra + rb) return false;


            // Since no separating axis is found, the OBBs must be intersecting
            return true;
        }

        /// <summary>
        /// Interseccion Ray-OBB.
        /// Devuelve true y el punto q de colision si hay interseccion.
        /// </summary>
        public static bool intersectRayObb(TgcRay ray, TgcObb obb, out Vector3 q)
        {
            //Transformar Ray a OBB-space
            Vector3 a = ray.Origin;
            Vector3 b = ray.Origin + ray.Direction;
            a = obb.toObbSpace(a);
            b = obb.toObbSpace(b);
            TgcRay.RayStruct ray2 = new TgcRay.RayStruct();
            ray2.origin = a;
            ray2.direction = Vector3.Normalize(b - a);

            //Crear AABB que representa al OBB
            Vector3 min = -obb.Extents;
            Vector3 max = obb.Extents;
            TgcBoundingBox.AABBStruct aabb = new TgcBoundingBox.AABBStruct();
            aabb.min = min;
            aabb.max = max;

            //Hacer interseccion Ray-AABB
            if (TgcCollisionUtils.intersectRayAABB(ray2, aabb, out q))
            {
                //Pasar q a World-Space
                q = obb.toWorldSpace(q);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Testear si hay olision entre un OBB y un AABB
        /// </summary>
        /// <param name="a">OBB</param>
        /// <param name="b">AABB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbAABB(TgcObb obb, TgcBoundingBox aabb)
        {
            return TgcCollisionUtils.testObbAABB(obb.toStruct(), aabb.toStruct());
        }

        /// <summary>
        /// Testear si hay olision entre un OBB y un AABB
        /// </summary>
        /// <param name="a">OBB</param>
        /// <param name="b">AABB</param>
        /// <returns>True si hay colision</returns>
        public static bool testObbAABB(TgcObb.OBBStruct obb, TgcBoundingBox.AABBStruct aabb)
        {
            //Crear un OBB que represente al AABB
            TgcObb.OBBStruct obb2 = TgcObb.computeFromAABB(aabb);

            //Hacer colision obb-obb
            return TgcCollisionUtils.testObbObb(obb, obb2);
        }

        #endregion


        #region Herramientas generales

        /// <summary>
        /// Crea un vector en base a los valores A, B y C de un plano
        /// </summary>
        public static Vector3 toVector3(Plane p)
        {
            return new Vector3(p.A, p.B, p.C);
        }

        /// <summary>
        /// Crea un array de floats con X,Y,Z
        /// </summary>
        public static float[] toArray(Vector3 v)
        {
            return new float[] { v.X, v.Y, v.Z };
        }

        /// <summary>
        /// Crea un vector en base a un array de floats con X,Y,Z
        /// </summary>
        public static Vector3 toVector3(float[] a)
        {
            return new Vector3(a[0], a[1], a[2]);
        }

        /// <summary>
        /// Invierte el valor de dos floats
        /// </summary>
        public static void swap(ref float t1, ref float t2)
        {
            float aux = t1;
            t1 = t2;
            t2 = aux;
        }

        /// <summary>
        /// Devuelve un Vector3 con la normal del plano (sin normalizar)
        /// </summary>
        public static Vector3 getPlaneNormal(Plane p)
        {
            return new Vector3(p.A, p.B, p.C);
        }

        /// <summary>
        /// Devuelve el mayor valor
        /// </summary>
        public static float max(float n1, float n2)
        {
            return n1 > n2 ? n1 : n2;
        }

        /// <summary>
        /// Devuelve el mayor valor
        /// </summary>
        public static float max(float n1, float n2, float n3)
        {
            return max(max(n1, n2), n3);
        }

        /// <summary>
        /// Devuelve el menor valor
        /// </summary>
        public static float min(float n1, float n2)
        {
            return n1 < n2 ? n1 : n2;
        }

        /// <summary>
        /// Devuelve el menor valor
        /// </summary>
        public static float min(float n1, float n2, float n3)
        {
            return min(min(n1, n2), n3);
        }
        
        /// <summary>
        /// Devuelve el menor y mayor valor de los tres
        /// </summary>
        public static void findMinMax(float x0, float x1, float x2, ref float min, ref float max)
        {
            min = max = x0;
            if (x1 < min) min = x1;
            if (x1 > max) max = x1;
            if (x2 < min) min = x2;
            if (x2 > max) max = x2;
        }

        /// <summary>
        /// Dot product entre dos float[3]
        /// </summary>
        public static float dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        /// <summary>
        /// Compara el signo de dos float.
        /// Devuelve TRUE si tienen el mismo signo.
        /// </summary>
        public static bool sameSign(float a, float b)
        {
            return a * b >= 0;
        }

        /// <summary>
        /// Expresion: (u x v) . w
        /// Devuelve un escalar
        /// </summary>
        public static float scalarTriple(Vector3 u, Vector3 v, Vector3 w)
        {
            return Vector3.Dot(Vector3.Cross(u, v), w);
        }


        #endregion







        
    }


    



    

}
