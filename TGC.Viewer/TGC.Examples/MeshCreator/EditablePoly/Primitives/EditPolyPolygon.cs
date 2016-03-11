using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TGC.Core.Geometries;

namespace TGC.Examples.MeshCreator.EditablePoly.Primitives
{
    /// <summary>
    ///     Poligono de EditablePoly
    /// </summary>
    public class EditPolyPolygon : EditPolyPrimitive
    {
        public List<EditPolyEdge> edges;
        public int matId;
        public Plane plane;
        public List<int> vbTriangles;
        public List<EditPolyVertex> vertices;

        public override EditablePoly.PrimitiveType Type
        {
            get { return EditablePoly.PrimitiveType.Polygon; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < vertices.Count; i++)
            {
                sb.Append(vertices[i].vbIndex + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public override bool projectToScreen(Matrix transform, out Rectangle box2D)
        {
            var v = new Vector3[vertices.Count];
            for (var i = 0; i < v.Length; i++)
            {
                v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
            }
            return MeshCreatorUtils.projectPolygon(v, out box2D);
        }

        public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
        {
            var v = new Vector3[vertices.Count];
            for (var i = 0; i < v.Length; i++)
            {
                v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
            }
            float t;
            return TgcCollisionUtils.intersectRayConvexPolygon(ray, v, out t, out q);
        }

        /// <summary>
        ///     Normal del plano del poligono
        /// </summary>
        public Vector3 getNormal()
        {
            return new Vector3(plane.A, plane.B, plane.C);
        }

        public override Vector3 computeCenter()
        {
            var sum = vertices[0].position;
            for (var i = 1; i < vertices.Count; i++)
            {
                sum += vertices[i].position;
            }
            return Vector3.Scale(sum, 1f / vertices.Count);
        }

        public override void move(Vector3 movement)
        {
            /*
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].position += movement;
            }
            */
            for (var i = 0; i < vertices.Count; i++)
            {
                vertices[i].movement = movement;
            }
        }

        /// <summary>
        ///     Quitar arista de la lista
        /// </summary>
        public void removeEdge(EditPolyEdge e)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                if (e == edges[i])
                {
                    edges.RemoveAt(i);
                    break;
                }
            }
        }
    }
}