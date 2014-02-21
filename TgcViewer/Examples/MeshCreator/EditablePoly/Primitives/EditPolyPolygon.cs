using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.MeshCreator.EditablePolyTools.Primitives
{
    /// <summary>
    /// Poligono de EditablePoly
    /// </summary>
    public class EditPolyPolygon : EditPolyPrimitive
    {
        public List<EditPolyVertex> vertices;
        public List<EditPolyEdge> edges;
        public List<int> vbTriangles;
        public Plane plane;
        public int matId;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vertices.Count; i++)
            {
                sb.Append(vertices[i].vbIndex + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public override EditablePoly.PrimitiveType Type
        {
            get { return EditablePoly.PrimitiveType.Polygon; }
        }

        public override bool projectToScreen(Matrix transform, out Rectangle box2D)
        {
            Vector3[] v = new Vector3[vertices.Count];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
            }
            return MeshCreatorUtils.projectPolygon(v, out box2D);
        }

        public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
        {
            Vector3[] v = new Vector3[vertices.Count];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
            }
            float t;
            return TgcCollisionUtils.intersectRayConvexPolygon(ray, v, out t, out q);
        }

        /// <summary>
        /// Normal del plano del poligono
        /// </summary>
        public Vector3 getNormal()
        {
            return new Vector3(plane.A, plane.B, plane.C);
        }

        public override Vector3 computeCenter()
        {
            Vector3 sum = vertices[0].position;
            for (int i = 1; i < vertices.Count; i++)
            {
                sum += vertices[i].position;
            }
            return Vector3.Scale(sum, 1f / vertices.Count);
        }

        public override void move(Vector3 movement)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].position += movement;
            }
        }

        /// <summary>
        /// Quitar arista de la lista
        /// </summary>
        public void removeEdge(EditPolyEdge e)
        {
            for (int i = 0; i < edges.Count; i++)
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
