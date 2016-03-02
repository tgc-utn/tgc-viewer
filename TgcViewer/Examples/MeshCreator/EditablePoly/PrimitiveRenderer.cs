using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Geometries;

namespace TGC.Examples.MeshCreator.EditablePoly
{
    /// <summary>
    ///     Herramienta para renderizar las primitivas de Editable Poly
    /// </summary>
    public class PrimitiveRenderer
    {
        private readonly TrianglesBatchRenderer batchRenderer;

        private readonly EditablePoly editablePoly;
        private readonly Color SELECTED_POLYGON_COLOR = Color.FromArgb(120, 255, 0, 0);
        private readonly TgcBox selectedVertexBox;
        private readonly TgcBox vertexBox;

        public PrimitiveRenderer(EditablePoly editablePoly)
        {
            this.editablePoly = editablePoly;
            batchRenderer = new TrianglesBatchRenderer();

            vertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Blue);
            selectedVertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Red);
        }

        /// <summary>
        ///     Dibujar primitivas
        /// </summary>
        /// <param name="transform">Transform matrix del mesh</param>
        public void render(Matrix transform)
        {
            switch (editablePoly.CurrentPrimitive)
            {
                case EditablePoly.PrimitiveType.Vertex:
                    renderVertices(transform);
                    break;

                case EditablePoly.PrimitiveType.Edge:
                    renderEdges(transform);
                    break;

                case EditablePoly.PrimitiveType.Polygon:
                    renderPolygons(transform);
                    break;
            }
        }

        /// <summary>
        ///     Dibujar vertices
        /// </summary>
        private void renderVertices(Matrix transform)
        {
            foreach (var v in editablePoly.Vertices)
            {
                var pos = Vector3.TransformCoordinate(v.position, transform);
                var box = v.Selected ? selectedVertexBox : vertexBox;
                box.Position = pos /*+ new Vector3(0.5f, 0.5f, 0.5f)*/;
                box.render();
            }
        }

        /// <summary>
        ///     Dibujar poligonos
        /// </summary>
        private void renderPolygons(Matrix transform)
        {
            batchRenderer.reset();

            //Edges
            foreach (var e in editablePoly.Edges)
            {
                var a = Vector3.TransformCoordinate(e.a.position, transform);
                var b = Vector3.TransformCoordinate(e.b.position, transform);
                batchRenderer.addBoxLine(a, b, 0.06f, e.Selected ? Color.Red : Color.Blue);
            }

            //Selected polygons (as polygon meshes)
            foreach (var p in editablePoly.Polygons)
            {
                if (p.Selected)
                {
                    /*
                    Vector3 n = new Vector3(p.plane.A, p.plane.B, p.plane.C) * 0.1f;
                    Vector3 v0 = Vector3.TransformCoordinate(p.vertices[0].position, transform);
                    Vector3 v1 = Vector3.TransformCoordinate(p.vertices[1].position, transform);
                    for (int i = 2; i < p.vertices.Count; i++)
                    {
                        batchRenderer.checkAndFlush(6);
                        Vector3 v2 = Vector3.TransformCoordinate(p.vertices[i].position, transform);
                        batchRenderer.addTriangle(v0 + n, v1 + n, v2 + n, SELECTED_POLYGON_COLOR);
                        batchRenderer.addTriangle(v0 - n, v1 - n, v2 - n, SELECTED_POLYGON_COLOR);
                        v1 = v2;
                    }
                     */
                    var n = new Vector3(p.plane.A, p.plane.B, p.plane.C) * 0.1f;
                    for (var i = 0; i < p.vbTriangles.Count; i++)
                    {
                        var triIdx = p.vbTriangles[i];
                        var v0 =
                            Vector3.TransformCoordinate(
                                editablePoly.Vertices[editablePoly.IndexBuffer[triIdx]].position, transform);
                        var v1 =
                            Vector3.TransformCoordinate(
                                editablePoly.Vertices[editablePoly.IndexBuffer[triIdx + 1]].position, transform);
                        var v2 =
                            Vector3.TransformCoordinate(
                                editablePoly.Vertices[editablePoly.IndexBuffer[triIdx + 2]].position, transform);

                        batchRenderer.checkAndFlush(6);
                        batchRenderer.addTriangle(v0 + n, v1 + n, v2 + n, SELECTED_POLYGON_COLOR);
                        batchRenderer.addTriangle(v0 - n, v1 - n, v2 - n, SELECTED_POLYGON_COLOR);
                    }
                }
            }
            //Vaciar todo lo que haya
            batchRenderer.render();
        }

        /// <summary>
        ///     Dibujar aristas
        /// </summary>
        private void renderEdges(Matrix transform)
        {
            batchRenderer.reset();

            foreach (var e in editablePoly.Edges)
            {
                var a = Vector3.TransformCoordinate(e.a.position, transform);
                var b = Vector3.TransformCoordinate(e.b.position, transform);
                batchRenderer.addBoxLine(a, b, 0.12f, e.Selected ? Color.Red : Color.Blue);
            }
            //Vaciar todo lo que haya
            batchRenderer.render();
        }

        public void dispose()
        {
            vertexBox.dispose();
            selectedVertexBox.dispose();
            batchRenderer.dispose();
        }
    }
}