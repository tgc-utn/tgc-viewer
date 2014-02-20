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
    /// Arista de EditablePoly
    /// </summary>
    public class EditPolyEdge : EditPolyPrimitive
    {
        private static readonly TgcObb COLLISION_OBB = new TgcObb();

        public EditPolyVertex a;
        public EditPolyVertex b;
        public List<EditPolyPolygon> faces;

        public override string ToString()
        {
            return a.vbIndex + " => " + b.vbIndex;
        }

        public override EditablePoly.PrimitiveType Type
        {
            get { return EditablePoly.PrimitiveType.Edge; }
        }

        public override bool projectToScreen(Matrix transform, out Rectangle box2D)
        {
            return MeshCreatorUtils.projectSegmentToScreenRect(
                Vector3.TransformCoordinate(a.position, transform),
                Vector3.TransformCoordinate(b.position, transform), out box2D);
        }

        public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
        {
            //Actualizar OBB con posiciones de la arista para utilizar en colision
            EditablePolyUtils.updateObbFromSegment(COLLISION_OBB,
                Vector3.TransformCoordinate(a.position, transform),
                Vector3.TransformCoordinate(b.position, transform),
                0.2f);

            //ray-obb
            return TgcCollisionUtils.intersectRayObb(ray, COLLISION_OBB, out q);
        }

        public override Vector3 computeCenter()
        {
            return (a.position + b.position) * 0.5f;
        }

        public override void move(Vector3 movement)
        {
            a.position += movement;
            b.position += movement;
        }
    }
}
