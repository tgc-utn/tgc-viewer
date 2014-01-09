using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.MeshCreator.EditablePoly
{
    /// <summary>
    /// Herramienta para poder editar todos los componentes de un mesh: vertices, aristas y poligonos.
    /// </summary>
    public class EditablePoly
    {
        /// <summary>
        /// Epsilon para comparar si dos vertices son iguales
        /// </summary>
        private const float EPSILON = 0.0001f;


        TgcMesh mesh;
        List<Vertex> vertices;
        List<Edge> edges;
        List<Polygon> polygons;

        /// <summary>
        /// Construir un EditablePoly a partir de un mesh
        /// </summary>
        public EditablePoly(TgcMesh origMesh)
        {
            loadMesh(origMesh);
        }

        /// <summary>
        /// Tomar un mesh cargar todas las estructuras internas necesarias para poder editarlo
        /// </summary>
        private void loadMesh(TgcMesh origMesh)
        {
            //Obtener vertices del mesh
            List<Vertex> origVertices = getMeshOriginalVertexData(origMesh);
            int origTriCount = origVertices.Count / 3;

            //Iterar sobre los triangulos y generar data auxiliar unificada
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            polygons = new List<Polygon>();
            for (int i = 0; i < origTriCount; i++)
            {
                Vertex v1 = origVertices[i * 3];
                Vertex v2 = origVertices[i * 3 + 1];
                Vertex v3 = origVertices[i * 3 + 2];

                //Agregar vertices a la lista, si es que son nuevos
                int v1Idx = addVertexToListIfUnique(v1);
                int v2Idx = addVertexToListIfUnique(v2);
                int v3Idx = addVertexToListIfUnique(v3);
                v1 = vertices[v1Idx];
                v2 = vertices[v2Idx];
                v3 = vertices[v3Idx];

                //TODO: agregar vertices al vertexBuffer

                //Crear edges (vertices ordenados segun indice ascendente)
                Edge e1 = new Edge();
                e1.a = vertices[FastMath.Min(v1Idx, v2Idx)];
                e1.b = vertices[FastMath.Max(v1Idx, v2Idx)];
                Edge e2 = new Edge();
                e2.a = vertices[FastMath.Min(v2Idx, v3Idx)];
                e2.b = vertices[FastMath.Max(v2Idx, v3Idx)];
                Edge e3 = new Edge();
                e3.a = vertices[FastMath.Min(v3Idx, v1Idx)];
                e3.b = vertices[FastMath.Max(v3Idx, v1Idx)];

                //Agregar edges a la lista, si es que son nuevos
                int e1Idx = addEdgeToListIfUnique(e1);
                int e2Idx = addEdgeToListIfUnique(e2);
                int e3Idx = addEdgeToListIfUnique(e3);
                e1 = edges[e1Idx];
                e2 = edges[e2Idx];
                e3 = edges[e3Idx];

                //Guardar referencias a aristas en cada vertice
                v1.edges.Add(e1);
                v1.edges.Add(e3);
                v2.edges.Add(e1);
                v2.edges.Add(e2);
                v3.edges.Add(e2);
                v3.edges.Add(e3);

                //Crear poligono para este triangulo
                Polygon p = new Polygon();
                p.vertices = new List<Vertex>();
                p.vertices.Add(v1);
                p.vertices.Add(v2);
                p.vertices.Add(v3);
                p.edges = new List<Edge>();
                p.edges.Add(e1);
                p.edges.Add(e2);
                p.edges.Add(e3);
                p.vbTriangles = new List<int>();
                p.vbTriangles.Add(i * 3);
                p.plane = Plane.FromPoints(v1.position, v2.position, v3.position);
                p.plane.Normalize();
                p.matId = 0; //TODO: obtener material ID del attribute buffer

                //TODO: agregar triangulo al index buffer


                //Buscar si hay un poligono ya existente al cual sumarnos (coplanar y que compartan una arista)
                Polygon coplanarP = null;
                for (int j = 0; j < polygons.Count; j++)
                {
                    //Coplanares y con igual material ID
                    Polygon p0 = polygons[j];
                    if (p0.matId == p.matId && samePlane(p0.plane, p.plane))
                    {
                        //Buscar si tienen una arista igual
                        int p0SharedEdgeIdx;
                        int pSharedEdgeIdx;
                        if (findShareEdgeBetweenPolygons(p0, p, out p0SharedEdgeIdx, out pSharedEdgeIdx))
                        {
                            //Obtener el tercer vertice del triangulo que no es parte de la arista compartida
                            Edge sharedEdge = p.edges[pSharedEdgeIdx];
                            Vertex thirdVert;
                            if (p.vertices[0] != sharedEdge.a && p.vertices[0] != sharedEdge.b)
                                thirdVert = p.vertices[0];
                            else if(p.vertices[1] != sharedEdge.a && p.vertices[1] != sharedEdge.b)
                                thirdVert = p.vertices[1];
                            else
                                thirdVert = p.vertices[2];

                            //Quitar arista compartida de poligono existente
                            p0.edges.RemoveAt(p0SharedEdgeIdx);

                            //Agregar el tercer vertice a poligno existente
                            p0.vertices.Add(thirdVert);

                            //Eliminar arista compartida de la lista global
                            for (int k = 0; k < edges.Count; k++)
                            {
                                if(sameEdge(edges[k], sharedEdge))
                                {
                                    edges.RemoveAt(k);
                                    break;
                                }
                            }

                            //Agregar al poligono dos nuevas aristas que conectar los extremos de la arista compartida hacia el tercer vertice
                            Edge newPolEdge1 = new Edge();
                            newPolEdge1.a = vertices[FastMath.Min(sharedEdge.a.vbIndex, thirdVert.vbIndex)];
                            newPolEdge1.b = vertices[FastMath.Max(sharedEdge.a.vbIndex, thirdVert.vbIndex)];
                            newPolEdge1.faces = new List<Polygon>();
                            newPolEdge1.faces.Add(p0);
                            sharedEdge.a.edges.Add(newPolEdge1);
                            sharedEdge.b.edges.Add(newPolEdge1);
                            p0.edges.Add(newPolEdge1);

                            Edge newPolEdge2 = new Edge();
                            newPolEdge2.a = vertices[FastMath.Min(sharedEdge.b.vbIndex, thirdVert.vbIndex)];
                            newPolEdge2.b = vertices[FastMath.Max(sharedEdge.b.vbIndex, thirdVert.vbIndex)];
                            newPolEdge2.faces = new List<Polygon>();
                            newPolEdge2.faces.Add(p0);
                            sharedEdge.a.edges.Add(newPolEdge2);
                            sharedEdge.b.edges.Add(newPolEdge2);
                            p0.edges.Add(newPolEdge2);

                            //Agregar indice de triangulo del vertexBuffer que se sumo al poligono
                            p0.vbTriangles.Add(p.vbTriangles[0]);

                            coplanarP = p0;
                        }
                    }
                }
                //Es un nuevo poligono, agregarlo
                if (coplanarP == null)
                {
                    polygons.Add(p);
                }
            }

            //Crear nuevo mesh con data unificada

            



        }

        /// <summary>
        /// Busca si ambos poligonos tienen una arista igual.
        /// Si encontro retorna el indice de la arista igual de cada poligono.
        /// </summary>
        private bool findShareEdgeBetweenPolygons(Polygon p1, Polygon p2, out int p1Edge, out int p2Edge)
        {
            for (int i = 0; i < p1.edges.Count; i++)
            {
                for (int j = 0; j < p2.edges.Count; j++)
                {
                    if (sameEdge(p1.edges[i], p2.edges[j]))
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
        private int addEdgeToListIfUnique(Edge e)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (sameEdge(edges[i], e))
                {
                    return i;
                }
            }
            e.faces = new List<Polygon>();
            edges.Add(e);
            return edges.Count - 1;
        }


        /// <summary>
        /// Agrega un nuevo vertice a la lista si es que ya no hay otro igual.
        /// Devuelve el indice del nuevo vertice o del que ya estaba.
        /// </summary>
        private int addVertexToListIfUnique(Vertex v)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (sameVextex(vertices[i], v))
                {
                    return i;
                }
            }
            v.vbIndex = vertices.Count;
            v.edges = new List<Edge>();
            vertices.Add(v);
            return v.vbIndex;
        }

        /// <summary>
        /// Indica si dos aristas son iguales
        /// </summary>
        private bool sameEdge(Edge e1, Edge e2)
        {
            return sameVextex(e1.a, e2.a) && sameVextex(e1.b, e2.b);
        }

        /// <summary>
        /// Indica si dos vertices son iguales
        /// </summary>
        /// <returns></returns>
        private bool sameVextex(Vertex a, Vertex b)
        {
            return equalsVector3(a.position, b.position);
        }

        /// <summary>
        /// Indica si dos Vector3 son iguales
        /// </summary>
        private bool equalsVector3(Vector3 a, Vector3 b)
        {
            return equalsFloat(a.X, b.X) 
                && equalsFloat(a.Y, b.Y) 
                && equalsFloat(a.Z, b.Z);
        }

        /// <summary>
        /// Compara que dos floats sean iguales, o casi
        /// </summary>
        private bool equalsFloat(float f1, float f2)
        {
            return FastMath.Abs(f1 - f2) <= EPSILON;
        }

        /// <summary>
        /// Compara si dos planos son iguales
        /// </summary>
        private bool samePlane(Plane p1, Plane p2)
        {
            //TODO: comparar en ambos sentidos por las dudas
            return equalsVector3(new Vector3(p1.A, p1.B, p1.C), new Vector3(p2.A, p2.B, p2.C))
                && equalsFloat(p1.D, p2.D);
        }

        /// <summary>
        /// Obtener la lista de vertices originales del mesh
        /// </summary>
        private List<Vertex> getMeshOriginalVertexData(TgcMesh origMesh)
        {
            List<Vertex> origVertices = new List<Vertex>();
            switch (origMesh.RenderType)
            {
                case TgcMesh.MeshRenderType.VERTEX_COLOR:
                    TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])origMesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, origMesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts1.Length; i++)
                    {
                        Vertex v = new Vertex();
                        v.position = verts1[i].Position;
                        v.normal = verts1[i].Normal;
                        origVertices.Add(v);
                    }
                    origMesh.D3dMesh.UnlockVertexBuffer();
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])origMesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, origMesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts2.Length; i++)
                    {
                        Vertex v = new Vertex();
                        v.position = verts2[i].Position;
                        v.normal = verts2[i].Normal;
                        v.texCoords = new Vector2(verts2[i].Tu, verts2[i].Tv);
                        origVertices.Add(v);
                    }
                    origMesh.D3dMesh.UnlockVertexBuffer();
                    break;

                case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])origMesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, origMesh.D3dMesh.NumberVertices);
                    for (int i = 0; i < verts3.Length; i++)
                    {
                        Vertex v = new Vertex();
                        v.position = verts3[i].Position;
                        v.normal = verts3[i].Normal;
                        v.texCoords = new Vector2(verts3[i].Tu0, verts3[i].Tv0);
                        origVertices.Add(v);
                    }
                    origMesh.D3dMesh.UnlockVertexBuffer();
                    break;
            }

            return origVertices;
        }

        public void render()
        {
        }

        public void dispose()
        {
        }

        /// <summary>
        /// Estructura auxiliar de vertice
        /// </summary>
        private class Vertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 texCoords;
            public List<Edge> edges;
            public int vbIndex;

            public override string ToString()
            {
                return "Index: " + vbIndex + ", Pos: " + TgcParserUtils.printVector3(position);
            }
        }

        /// <summary>
        /// Estructura auxiliar de arista
        /// </summary>
        private class Edge
        {
            public Vertex a;
            public Vertex b;
            public List<Polygon> faces;

            public override string ToString()
            {
                return a.vbIndex + " => " + b.vbIndex;
            }
        }

        /// <summary>
        /// Estructura auxiliar de poligono
        /// </summary>
        private class Polygon
        {
            public List<Vertex> vertices;
            public List<Edge> edges;
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
        }


    }
}
