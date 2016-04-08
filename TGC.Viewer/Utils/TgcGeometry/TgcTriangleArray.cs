using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Geometries;
using TGC.Core.SceneLoader;

namespace TGC.Viewer.Utils.TgcGeometry
{
    /// <summary>
    ///     Herramienta para dibujar una lista de triangulos.
    ///     No está pensado para rasterizar en forma performante, sino mas
    ///     para una herramienta de debug.
    /// </summary>
    public class TgcTriangleArray
    {
        private readonly TgcPickingRay pickingRay;

        public TgcTriangleArray()
        {
            Triangles = new List<TgcTriangle>();
            pickingRay = new TgcPickingRay();
        }

        /// <summary>
        ///     Triangulos
        /// </summary>
        public List<TgcTriangle> Triangles { get; }

        /// <summary>
        ///     Crear a partir de un mesh
        /// </summary>
        public static TgcTriangleArray fromMesh(TgcMesh mesh)
        {
            var triangleArray = new TgcTriangleArray();

            var vertices = mesh.getVertexPositions();
            var triCount = vertices.Length / 3;
            var triangles = new List<TgcTriangle>(triCount);
            for (var i = 0; i < triCount; i++)
            {
                var v1 = vertices[i * 3];
                var v2 = vertices[i * 3 + 1];
                var v3 = vertices[i * 3 + 2];

                var t = new TgcTriangle();
                t.A = v1;
                t.B = v2;
                t.C = v3;
                t.Color = Color.Red;
                t.updateValues();
                triangles.Add(t);
            }

            triangleArray.Triangles.AddRange(triangles);
            return triangleArray;
        }

        public void render()
        {
            foreach (var t in Triangles)
            {
                t.render();
            }
        }

        public void setEnabled(bool enabled)
        {
            foreach (var t in Triangles)
            {
                t.Enabled = enabled;
            }
        }

        public void dispose()
        {
            foreach (var t in Triangles)
            {
                t.dispose();
            }
        }

        /// <summary>
        ///     Picking sobre un triangulo
        /// </summary>
        public bool pickTriangle(out TgcTriangle triangle, out int triangleIndex)
        {
            pickingRay.updateRay();
            var segmentA = pickingRay.Ray.Origin;
            var segmentB = segmentA + Vector3.Scale(pickingRay.Ray.Direction, 10000f);
            var minDist = float.MaxValue;
            triangle = null;
            triangleIndex = -1;

            //Buscar la menor colision rayo-triangulo
            for (var i = 0; i < Triangles.Count; i++)
            {
                var tri = Triangles[i];

                float t;
                Vector3 uvw;
                Vector3 col;
                if (TgcCollisionUtils.intersectLineTriangle(segmentA, segmentB, tri.A, tri.B, tri.C, out uvw, out t,
                    out col))
                {
                    var dist = Vector3.Length(col - segmentA);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        triangle = tri;
                        triangleIndex = i;
                    }
                }
            }

            return triangle != null;
        }
    }
}