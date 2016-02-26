using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Herramienta para dibujar una lista de triangulos.
    /// No está pensado para rasterizar en forma performante, sino mas
    /// para una herramienta de debug.
    /// </summary>
    public class TgcTriangleArray
    {
        TgcPickingRay pickingRay;

        /// <summary>
        /// Crear a partir de un mesh
        /// </summary>
        public static TgcTriangleArray fromMesh(TgcMesh mesh)
        {
            TgcTriangleArray triangleArray = new TgcTriangleArray();

            Vector3[] vertices = mesh.getVertexPositions();
            int triCount = vertices.Length / 3;
            List<TgcTriangle> triangles = new List<TgcTriangle>(triCount);
            for (int i = 0; i < triCount; i++)
            {
                Vector3 v1 = vertices[i * 3];
                Vector3 v2 = vertices[i * 3 + 1];
                Vector3 v3 = vertices[i * 3 + 2];

                TgcTriangle t = new TgcTriangle();
                t.A = v1;
                t.B = v2;
                t.C = v3;
                t.Color = Color.Red;
                t.updateValues();
                triangles.Add(t);
            }

            triangleArray.triangles.AddRange(triangles);
            return triangleArray;
        }


        List<TgcTriangle> triangles;
        /// <summary>
        /// Triangulos
        /// </summary>
        public List<TgcTriangle> Triangles
        {
            get { return triangles; }
        }

        public TgcTriangleArray()
        {
            triangles = new List<TgcTriangle>();
            pickingRay = new TgcPickingRay();
        }

        public void render()
        {
            foreach (TgcTriangle t in triangles)
            {
                t.render();
            }
        }

        public void setEnabled(bool enabled)
        {
            foreach (TgcTriangle t in triangles)
            {
                t.Enabled = enabled;
            }
        }

        public void dispose()
        {
            foreach (TgcTriangle t in triangles)
            {
                t.dispose();
            }
        }

        /// <summary>
        /// Picking sobre un triangulo
        /// </summary>
        public bool pickTriangle(out TgcTriangle triangle, out int triangleIndex)
        {
            pickingRay.updateRay();
            Vector3 segmentA = pickingRay.Ray.Origin;
            Vector3 segmentB = segmentA + Vector3.Scale(pickingRay.Ray.Direction, 10000f);
            float minDist = float.MaxValue;
            triangle = null;
            triangleIndex = -1;

            //Buscar la menor colision rayo-triangulo
            for (int i = 0; i < triangles.Count; i++)
            {
                TgcTriangle tri = triangles[i];

                float t;
                Vector3 uvw;
                Vector3 col;
                if (TgcCollisionUtils.intersectLineTriangle(segmentA, segmentB, tri.A, tri.B, tri.C, out uvw, out t, out col))
                {
                    float dist = Vector3.Length(col - segmentA);
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
