using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace Examples.MeshCreator.EditablePolyTools
{
    /// <summary>
    /// Herramienta para renderizar las primitivas de Editable Poly
    /// </summary>
    public class PrimitiveRenderer
    {
        readonly Color SELECTED_POLYGON_COLOR = Color.FromArgb(120, 255, 0, 0);

        EditablePoly editablePoly;
        TgcBox vertexBox;
        TgcBox selectedVertexBox;
        TrianglesBatchRenderer batchRenderer;

        public PrimitiveRenderer(EditablePoly editablePoly)
        {
            this.editablePoly = editablePoly;
            this.batchRenderer = new TrianglesBatchRenderer();

            this.vertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Blue);
            this.selectedVertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Red);
        }

        /// <summary>
        /// Dibujar primitivas
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
        /// Dibujar vertices
        /// </summary>
        private void renderVertices(Matrix transform)
        {
            foreach (EditablePoly.Vertex v in editablePoly.Vertices)
            {
                Vector3 pos = Vector3.TransformCoordinate(v.position, transform);
                TgcBox box = v.Selected ? selectedVertexBox : vertexBox;
                box.Position = pos /*+ new Vector3(0.5f, 0.5f, 0.5f)*/;
                box.render();
            }
        }

        /// <summary>
        /// Dibujar poligonos
        /// </summary>
        private void renderPolygons(Matrix transform)
        {
            batchRenderer.reset();
            
            //Polygon edges
            foreach (EditablePoly.Polygon p in editablePoly.Polygons)
            {
                Vector3 v0 = Vector3.TransformCoordinate(p.vertices[0].position, transform);
                for (int i = 1; i < p.vertices.Count; i++)
                {
                    Vector3 v1 = Vector3.TransformCoordinate(p.vertices[i].position, transform);
                    batchRenderer.addBoxLine(v0, v1, 0.06f, Color.Blue);
                    v0 = v1;
                }
            }

            //Selected polygons (as polygon meshes)
            foreach (EditablePoly.Polygon p in editablePoly.Polygons)
            {
                if(p.Selected)
                {
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
                }
            }
            //Vaciar todo lo que haya
            batchRenderer.render();
        }

        /// <summary>
        /// Dibujar aristas
        /// </summary>
        private void renderEdges(Matrix transform)
        {
            batchRenderer.reset();

            foreach (EditablePoly.Edge e in editablePoly.Edges)
            {
                Vector3 a = Vector3.TransformCoordinate(e.a.position, transform);
                Vector3 b = Vector3.TransformCoordinate(e.b.position, transform);
                batchRenderer.addBoxLine(a, b, 0.06f, e.Selected ? Color.Red : Color.Blue);
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
