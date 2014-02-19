using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.MeshCreator.EditablePolyTools
{
    public class EditablePolyUtils
    {
        /// <summary>
        /// Epsilon para comparar si dos vertices son iguales
        /// </summary>
        public static readonly float EPSILON = 0.0001f;

        /*
        /// <summary>
        /// Filtrar todas las aristas que tiene un poligono y dejarle solo las que son parte del borde del poligono
        /// (Se quitan todas las aristas interiores)
        /// </summary>
        public static void computePolygonExternalEdges(EditablePoly.Polygon p)
        {
            if (p.vertices.Count == 3)
                return;

            Vector3 planeNorm = p.getNormal();
            List<EditablePoly.Edge> externalEdges = new List<EditablePoly.Edge>();
            foreach (EditablePoly.Edge e in p.edges)
            {
                //Half-plane entre la arista y la normal del poligono
                Vector3 vec = e.b.position - e.a.position;
                Vector3 n = Vector3.Cross(planeNorm, vec);
                Plane halfPlane = Plane.FromPointNormal(e.a.position, n);

                //Checkear el signo de todos los demas vertices del poligono
                bool first = true;
                TgcCollisionUtils.PointPlaneResult lastR = TgcCollisionUtils.PointPlaneResult.COINCIDENT;
                bool inside = false;
                foreach (EditablePoly.Vertex v in p.vertices)
                {
                    if(v.vbIndex != e.a.vbIndex && v.vbIndex != e.b.vbIndex )
                    {
                        TgcCollisionUtils.PointPlaneResult r = TgcCollisionUtils.classifyPointPlane(v.position, halfPlane);
                        if(first)
                        {
                            first = false;
                            lastR = r;
                        } 
                        else if(r != lastR)
                        {
                            inside = true;
                            break;
                        }
                    }
                }
                if(!inside)
                {
                    externalEdges.Add(e);
                }
            }
            p.edges = externalEdges;
        }

        /// <summary>
        /// Ordenar los vertices del poligono en base al recorrido de sus aristas externas
        /// </summary>
        public static void sortPolygonVertices(EditablePoly.Polygon p)
        {
            if (p.vertices.Count == 3)
                return;

            List<EditablePoly.Vertex> sortedVertices = new List<EditablePoly.Vertex>();
            EditablePoly.Edge lastEdge = p.edges[0];
            for (int i = 1; i < p.edges.Count; i++)
            {
                sortedVertices.Add(lastEdge.a);
                bool found = false;
                foreach (EditablePoly.Edge e in p.edges)
                {
                    if(lastEdge.b.vbIndex == e.a.vbIndex)
                    {
                        lastEdge = e;
                        found = true;
                        break;
                    }
                }
                if(!found)
                    throw new Exception("No se pudo recorrer aristas de poligono en loop. Poligono: " + p);
                
            }
            sortedVertices.Add(lastEdge.a);
            p.vertices = sortedVertices;
        }
        */


        /// <summary>
        /// Agregar un vertice a un poligono existente, ubicandolo en el medio de los dos vertices de la arista que compartian entre si
        /// </summary>
        public static void addVertexToPolygon(EditablePoly.Polygon p, EditablePoly.Edge sharedEdge, EditablePoly.Vertex newV)
        {
            for (int i = 0; i < p.vertices.Count; i++)
            {
                if (p.vertices[i].vbIndex == sharedEdge.a.vbIndex)
                {
                    p.vertices.Add(null);
                    for (int j = p.vertices.Count - 2; j >= i + 1 ; j--)
                    {
                        p.vertices[j + 1] = p.vertices[j];
                    }
                    p.vertices[i + 1] = newV;
                    break;
                }
            }
        }

        /// <summary>
        /// Indica si dos aristas son iguales
        /// </summary>
        public static bool sameEdge(EditablePoly.Edge e1, EditablePoly.Edge e2)
        {
            return (sameVextex(e1.a, e2.a) && sameVextex(e1.b, e2.b))
                || (sameVextex(e1.a, e2.b) && sameVextex(e1.b, e2.a));
        }

        /// <summary>
        /// Indica si dos vertices son iguales
        /// </summary>
        /// <returns></returns>
        public static bool sameVextex(EditablePoly.Vertex a, EditablePoly.Vertex b)
        {
            return equalsVector3(a.position, b.position);
        }

        /// <summary>
        /// Indica si dos Vector3 son iguales
        /// </summary>
        public static bool equalsVector3(Vector3 a, Vector3 b)
        {
            return equalsFloat(a.X, b.X)
                && equalsFloat(a.Y, b.Y)
                && equalsFloat(a.Z, b.Z);
        }

        /// <summary>
        /// Compara que dos floats sean iguales, o casi
        /// </summary>
        public static bool equalsFloat(float f1, float f2)
        {
            return FastMath.Abs(f1 - f2) <= EPSILON;
        }

        /// <summary>
        /// Compara si dos planos son iguales
        /// </summary>
        public static bool samePlane(Plane p1, Plane p2)
        {
            //TODO: comparar en ambos sentidos por las dudas
            return equalsVector3(new Vector3(p1.A, p1.B, p1.C), new Vector3(p2.A, p2.B, p2.C))
                && equalsFloat(p1.D, p2.D);
        }

        /// <summary>
        /// Busca si ambos poligonos tienen una arista igual.
        /// Si encontro retorna el indice de la arista igual de cada poligono.
        /// </summary>
        public static bool findShareEdgeBetweenPolygons(EditablePoly.Polygon p1, EditablePoly.Polygon p2, out int p1Edge, out int p2Edge)
        {
            for (int i = 0; i < p1.edges.Count; i++)
            {
                for (int j = 0; j < p2.edges.Count; j++)
                {
                    if (EditablePolyUtils.sameEdge(p1.edges[i], p2.edges[j]))
                    {
                        p1Edge = i;
                        p2Edge = j;
                        return true;
                    }
                }
            }
            p1Edge = -1;
            p2Edge = -1;
            return false;
        }

        /// <summary>
        /// Agrega una nueva arista a la lista si es que ya no hay otra igual.
        /// Devuelve el indice de la nuevo arista o de la que ya estaba.
        /// </summary>
        public static int addEdgeToListIfUnique(List<EditablePoly.Edge> edges, EditablePoly.Edge e)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (EditablePolyUtils.sameEdge(edges[i], e))
                {
                    return i;
                }
            }
            e.faces = new List<EditablePoly.Polygon>();
            edges.Add(e);
            return edges.Count - 1;
        }


        /// <summary>
        /// Agrega un nuevo vertice a la lista si es que ya no hay otro igual.
        /// Devuelve el indice del nuevo vertice o del que ya estaba.
        /// </summary>
        public static int addVertexToListIfUnique(List<EditablePoly.Vertex> vertices, EditablePoly.Vertex v)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (EditablePolyUtils.sameVextex(vertices[i], v))
                {
                    return i;
                }
            }
            v.vbIndex = vertices.Count;
            v.edges = new List<EditablePoly.Edge>();
            vertices.Add(v);
            return v.vbIndex;
        }

    }
}
