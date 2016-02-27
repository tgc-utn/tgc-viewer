using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TGC.Core.Utils;

namespace Examples.MeshCreator.EditablePolyTools.Primitives
{
    /// <summary>
    ///     Vertice de EditablePoly
    /// </summary>
    public class EditPolyVertex : EditPolyPrimitive
    {
        /// <summary>
        ///     Sphere for ray-collisions
        /// </summary>
        private static readonly TgcBoundingSphere COLLISION_SPHERE = new TgcBoundingSphere(new Vector3(0, 0, 0), 2);

        /*public Vector3 normal;
        public Vector2 texCoords;
        public Vector2 texCoords2;
        public int color;*/
        public List<EditPolyEdge> edges;
        public Vector3 movement;

        public Vector3 position;
        public int vbIndex;

        public EditPolyVertex()
        {
            movement = new Vector3(0, 0, 0);
        }

        public override EditablePoly.PrimitiveType Type
        {
            get { return EditablePoly.PrimitiveType.Vertex; }
        }

        public override string ToString()
        {
            return "Index: " + vbIndex + ", Pos: " + TgcParserUtils.printVector3(position);
        }

        public override bool projectToScreen(Matrix transform, out Rectangle box2D)
        {
            return MeshCreatorUtils.projectPoint(Vector3.TransformCoordinate(position, transform), out box2D);
        }

        public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
        {
            COLLISION_SPHERE.setCenter(Vector3.TransformCoordinate(position, transform));
            float t;
            return TgcCollisionUtils.intersectRaySphere(ray, COLLISION_SPHERE, out t, out q);
        }

        public override Vector3 computeCenter()
        {
            return position;
        }

        public override void move(Vector3 movement)
        {
            //position += movement;
            this.movement = movement;
        }

        /// <summary>
        ///     Eliminar arista de la lista
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