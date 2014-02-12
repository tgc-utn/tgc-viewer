using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using System.Drawing;

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
        TgcBoxLine edgeBoxLine;
        TgcBoxLine selectedEdgeBoxLine;
        TgcBoxLine polygonBoxLine;
        TgcBoxLine selectedPolygonBoxLine;

        public PrimitiveRenderer(EditablePoly editablePoly)
        {
            this.editablePoly = editablePoly;

            this.vertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Blue);
            this.selectedVertexBox = TgcBox.fromSize(new Vector3(1, 1, 1), Color.Red);

            this.edgeBoxLine = new TgcBoxLine();
            this.edgeBoxLine.Color = Color.Blue;
            this.edgeBoxLine.Thickness = 0.06f;
            this.selectedEdgeBoxLine = new TgcBoxLine();
            this.selectedEdgeBoxLine.Color = Color.Red;
            this.selectedEdgeBoxLine.Thickness = 0.06f;

            this.polygonBoxLine = new TgcBoxLine();
            this.polygonBoxLine.Color = Color.Blue;
            this.polygonBoxLine.Thickness = 0.06f;
            this.selectedPolygonBoxLine = new TgcBoxLine();
            this.selectedPolygonBoxLine.Color = Color.Red;
            this.selectedPolygonBoxLine.Thickness = 0.06f;
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
            foreach (EditablePoly.Polygon p in editablePoly.Polygons)
            {
                Vector3 v0 = Vector3.TransformCoordinate(p.vertices[0].position, transform);
                for (int i = 1; i < p.vertices.Count; i++)
                {
                    Vector3 v1 = Vector3.TransformCoordinate(p.vertices[i].position, transform);
                    TgcBoxLine l = p.Selected ? selectedPolygonBoxLine : polygonBoxLine;
                    l.PStart = v0;
                    l.PEnd = v1;
                    l.updateValues();
                    l.render();
                    v0 = v1;
                }
            }
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
                TgcBoxLine l = e.Selected ? selectedEdgeBoxLine : edgeBoxLine;
                l.PStart = a;
                l.PEnd = b;
                l.updateValues();
                l.render();
            }
        }

        public void dispose()
        {
            vertexBox.dispose();
            selectedVertexBox.dispose();
            edgeBoxLine.dispose();
            selectedEdgeBoxLine.dispose();
            polygonBoxLine.dispose();
            selectedPolygonBoxLine.dispose();
        }
        

    }
}
