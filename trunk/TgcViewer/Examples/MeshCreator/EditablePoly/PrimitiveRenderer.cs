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
                if (v.Selected)
                {
                    selectedVertexBox.Position = pos;
                    selectedVertexBox.render();
                }
                else
                {
                    vertexBox.Position = pos;
                    vertexBox.render();
                }
            }
        }

        /// <summary>
        /// Dibujar poligonos
        /// </summary>
        private void renderPolygons(Matrix transform)
        {
            /*
            foreach (EditablePoly.Polygon p in editablePoly.Polygons)
            {
                Vector3 v0 = Vector3.TransformCoordinate(p.vertices[0].position, transform);
                for (int i = 1; i < p.vertices.Count; i++)
                {
                    Vector3 v1 = Vector3.TransformCoordinate(p.vertices[i].position, transform);
                    batchRenderer.addBoxLine(v0, v1, 0.06f, p.Selected ? Color.Red : Color.Blue);
                    v0 = v1;
                }
            }
            //Vaciar todo lo que haya
            batchRenderer.render();
             */

            foreach (EditablePoly.Polygon p in editablePoly.Polygons)
            {
                if(p.Selected)
                {
                    Vector3 v0 = Vector3.TransformCoordinate(p.vertices[0].position, transform);
                    Vector3 v1 = Vector3.TransformCoordinate(p.vertices[1].position, transform);
                    for (int i = 2; i < p.vertices.Count; i++)
                    {
                        batchRenderer.checkAndFlush(3);
                        Vector3 v2 = Vector3.TransformCoordinate(p.vertices[i].position, transform);
                        batchRenderer.addTriangle(v0, v1, v2, Color.Red);
                        v0 = v1;
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
