using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
using TgcViewer;
using System.Drawing;

namespace Examples.MeshCreator.EditablePolyTools
{
    /// <summary>
    /// Herramienta para poder editar todos los componentes de un mesh: vertices, aristas y poligonos.
    /// </summary>
    public class EditablePoly
    {
        /// <summary>
        /// Estado dentro del modo EditablePoly
        /// </summary>
        public enum State
        {
            SelectObject,
            SelectingObject,
            TranslateGizmo,
        }

        /// <summary>
        /// Tipo de primitiva de EditablePoly
        /// </summary>
        public enum PrimitiveType
        {
            Vertex,
            Edge,
            Polygon,
            None
        }

        TgcMesh mesh;
        short[] indexBuffer;
        bool dirtyValues;
        State currentState;
        bool selectiveObjectsAdditive;
        SelectionRectangleMesh rectMesh;
        Vector2 initMousePos;
        TgcPickingRay pickingRay;
        PrimitiveRenderer primitiveRenderer;

        List<Primitive> selectionList;
        /// <summary>
        /// Primitivas seleccionadas
        /// </summary>
        public List<Primitive> SelectionList
        {
            get { return selectionList; }
        }

        List<Vertex> vertices;
        /// <summary>
        /// Vertices
        /// </summary>
        public List<Vertex> Vertices
        {
            get { return vertices; }
        }

        List<Edge> edges;
        /// <summary>
        /// Aristas
        /// </summary>
        public List<Edge> Edges
        {
            get { return edges; }
        }

        List<Polygon> polygons;
        /// <summary>
        /// Poligonos
        /// </summary>
        public List<Polygon> Polygons
        {
            get { return polygons; }
        }

        PrimitiveType currentPrimitive;
        /// <summary>
        /// Primitiva actual que se esta editando
        /// </summary>
        public PrimitiveType CurrentPrimitive
        {
            get { return currentPrimitive; }
        }

        /// <summary>
        /// Construir un EditablePoly a partir de un mesh
        /// </summary>
        public EditablePoly(TgcMesh origMesh)
        {
            this.currentPrimitive = PrimitiveType.None;
            this.rectMesh = new SelectionRectangleMesh();
            this.selectionList = new List<Primitive>();
            this.pickingRay = new TgcPickingRay();
            this.primitiveRenderer = new PrimitiveRenderer(this);
            loadMesh(origMesh);
        }

        /// <summary>
        /// Cargar tipo de primitiva actual a editar
        /// </summary>
        public void setPrimitiveType(PrimitiveType p)
        {
            this.currentPrimitive = p;
            clearSelection();
        }

        /// <summary>
        /// Actualizacion de estado en render loop
        /// </summary>
        public void update()
        {
            //Maquina de estados
            switch (currentState)
            {
                case State.SelectObject:
                    doSelectObject();
                    break;
                case State.SelectingObject:
                    doSelectingObject();
                    break;
                case State.TranslateGizmo:
                    doTranslateGizmo();
                    break;
            }
        }



        #region Primitive selection
       

        /// <summary>
        /// Estado: seleccionar objetos (estado default)
        /// </summary>
        private void doSelectObject()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Si mantiene control y clic con el mouse, iniciar cuadro de seleccion para agregar/quitar a la seleccion actual
            if ((input.keyDown(Microsoft.DirectX.DirectInput.Key.LeftControl) || input.keyDown(Microsoft.DirectX.DirectInput.Key.RightControl))
                && input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                currentState = State.SelectingObject;
                this.initMousePos = new Vector2(input.Xpos, input.Ypos);
                this.selectiveObjectsAdditive = true;
            }
            //Si mantiene el clic con el mouse, iniciar cuadro de seleccion
            else if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                currentState = State.SelectingObject;
                this.initMousePos = new Vector2(input.Xpos, input.Ypos);
                this.selectiveObjectsAdditive = false;
            }
        }

        /// <summary>
        /// Estado: Cuando se esta arrastrando el mouse para armar el cuadro de seleccion
        /// </summary>
        private void doSelectingObject()
        {
            TgcD3dInput input = GuiController.Instance.D3dInput;

            //Mantiene el mouse apretado
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Definir recuadro
                Vector2 mousePos = new Vector2(input.Xpos, input.Ypos);
                Vector2 min = Vector2.Minimize(initMousePos, mousePos);
                Vector2 max = Vector2.Maximize(initMousePos, mousePos);

                rectMesh.updateMesh(min, max);

            }
            //Solo el mouse
            else if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Definir recuadro
                Vector2 mousePos = new Vector2(input.Xpos, input.Ypos);
                Vector2 min = Vector2.Minimize(initMousePos, mousePos);
                Vector2 max = Vector2.Maximize(initMousePos, mousePos);
                Rectangle r = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

                //Usar recuadro de seleccion solo si tiene un tamaño minimo
                if (r.Width > 1 && r.Height > 1)
                {
                    //Limpiar seleccionar anterior si no estamos agregando en forma aditiva
                    if (!selectiveObjectsAdditive)
                    {
                        clearSelection();
                    }

                    //Buscar que primitivas caen dentro de la seleccion y elegirlos
                    int i = 0;
                    Primitive p = iteratePrimitive(currentPrimitive, i);
                    while (p != null)
                    {
                        //Ver si hay colision contra la proyeccion de la primitiva y el rectangulo 2D
                        Rectangle primRect;
                        if (p.projectToScreen(mesh.Transform, out primRect))
                        {
                            if (r.IntersectsWith(primRect))
                            {
                                //Agregar el objeto en forma aditiva
                                if (selectiveObjectsAdditive)
                                {
                                    selectOrRemovePrimitiveIfPresent(p);
                                }
                                //Agregar el objeto en forma simple
                                else
                                {
                                    selectPrimitive(p);
                                }
                            }
                        }
                        p = iteratePrimitive(currentPrimitive, ++i);
                    }
                }
                //Si el recuadro no tiene tamaño suficiente, hacer seleccion directa
                else
                {
                    doDirectSelection(selectiveObjectsAdditive);
                }

                currentState = State.SelectObject;

                //Si quedo algo seleccionado activar gizmo
                if (selectionList.Count > 0)
                {
                    activateTranslateGizmo();
                }

                //Actualizar panel de Modify con lo que se haya seleccionado, o lo que no
                //control.updateModifyPanel();
            }



            //Dibujar recuadro
            rectMesh.render();
        }

        

        /// <summary>
        /// Seleccionar una sola primitiva
        /// </summary>
        private void selectPrimitive(Primitive p)
        {
            p.Selected = true;
            selectionList.Add(p);
        }

        /// <summary>
        /// Selecciona una sola primitiva pero antes se fija si ya no estaba en la lista de seleccion.
        /// Si ya estaba, entonces la quita de la lista de seleccion
        /// </summary>
        private void selectOrRemovePrimitiveIfPresent(Primitive p)
        {
            //Ya existe, quitar
            if (selectionList.Contains(p))
            {
                p.Selected = false;
                selectionList.Remove(p);
            }
            //No existe, agregar
            else
            {
                p.Selected = true;
                selectionList.Add(p);
            }
        }

        /// <summary>
        /// Deseleccionar todo
        /// </summary>
        private void clearSelection()
        {
            foreach (Primitive p in selectionList)
            {
                p.Selected = false;
            }
            selectionList.Clear();
        }

        /// <summary>
        /// Hacer picking para seleccionar la primitiva mas cercano del ecenario.
        /// </summary>
        /// <param name="additive">En True agrega/quita la primitiva a la seleccion actual</param>
        private void doDirectSelection(bool additive)
        {
            this.pickingRay.updateRay();

            //Buscar menor colision con primitivas
            float minDistSq = float.MaxValue;
            Primitive closestPrimitive = null;
            Vector3 q;
            int i = 0;
            Primitive p = iteratePrimitive(currentPrimitive, i);
            while (p != null)
            {
                if (p.intersectRay(pickingRay.Ray, mesh.Transform, out q))
                {
                    float lengthSq = Vector3.Subtract(pickingRay.Ray.Origin, q).LengthSq();
                    if (lengthSq < minDistSq)
                    {
                        minDistSq = lengthSq;
                        closestPrimitive = p;
                    }
                }
                p = iteratePrimitive(currentPrimitive, ++i);
            }

            //Agregar
            if (closestPrimitive != null)
            {
                //Sumar a la lista de seleccion
                if (additive)
                {
                    selectOrRemovePrimitiveIfPresent(closestPrimitive);
                }
                //Seleccionar uno solo
                else
                {
                    clearSelection();
                    selectPrimitive(closestPrimitive);
                }
                activateTranslateGizmo();
            }
            //Nada seleccionado
            else
            {
                //Limpiar seleccion
                clearSelection();
            }

            //Pasar a modo seleccion
            currentState = State.SelectObject;
            //control.updateModifyPanel();
        }


        #endregion


        #region Translate Gizmo

        private void activateTranslateGizmo()
        {
            //TODO gizmo translate
        }

        private void doTranslateGizmo()
        {
            //TODO gizmo translate
        }

        #endregion



        public void render()
        {
            //Actualizar mesh si hubo algun cambio
            if (dirtyValues)
            {
                updateMesh();
                dirtyValues = false;
            }

            //Render de mesh
            mesh.render();

            //Render de primitivas
            primitiveRenderer.render(mesh.Transform);
        }

        public void dispose()
        {
            primitiveRenderer.dispose();
        }

        





        #region Mesh loading

        /// <summary>
        /// Tomar un mesh cargar todas las estructuras internas necesarias para poder editarlo
        /// </summary>
        private void loadMesh(TgcMesh origMesh)
        {
            //Obtener vertices del mesh
            this.mesh = origMesh;
            List<Vertex> origVertices = getMeshOriginalVertexData(origMesh);
            int origTriCount = origVertices.Count / 3;

            //Iterar sobre los triangulos y generar data auxiliar unificada
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            polygons = new List<Polygon>();
            indexBuffer = new short[origTriCount * 3];
            int[] attributeBuffer = origMesh.D3dMesh.LockAttributeBufferArray(LockFlags.ReadOnly);
            origMesh.D3dMesh.UnlockAttributeBuffer(attributeBuffer);
            for (int i = 0; i < origTriCount; i++)
            {
                Vertex v1 = origVertices[i * 3];
                Vertex v2 = origVertices[i * 3 + 1];
                Vertex v3 = origVertices[i * 3 + 2];

                //Agregar vertices a la lista, si es que son nuevos
                int v1Idx = EditablePolyUtils.addVertexToListIfUnique(vertices, v1);
                int v2Idx = EditablePolyUtils.addVertexToListIfUnique(vertices, v2);
                int v3Idx = EditablePolyUtils.addVertexToListIfUnique(vertices, v3);
                v1 = vertices[v1Idx];
                v2 = vertices[v2Idx];
                v3 = vertices[v3Idx];

                //Crear edges
                Edge e1 = new Edge();
                e1.a = v1;
                e1.b = v2;
                Edge e2 = new Edge();
                e2.a = v2;
                e2.b = v3;
                Edge e3 = new Edge();
                e3.a = v3;
                e3.b = v1;

                /*
                //Agregar edges a la lista, si es que son nuevos
                int e1Idx = EditablePolyUtils.addEdgeToListIfUnique(edges, e1);
                int e2Idx = EditablePolyUtils.addEdgeToListIfUnique(edges, e2);
                int e3Idx = EditablePolyUtils.addEdgeToListIfUnique(edges, e3);
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
                */

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
                p.matId = attributeBuffer[i];

                //Agregar triangulo al index buffer
                indexBuffer[i * 3] = (short)v1Idx;
                indexBuffer[i * 3 + 1] = (short)v2Idx;
                indexBuffer[i * 3 + 2] = (short)v3Idx;

                //Buscar si hay un poligono ya existente al cual sumarnos (coplanar y que compartan una arista)
                Polygon coplanarP = null;
                for (int j = 0; j < polygons.Count; j++)
                {
                    //Coplanares y con igual material ID
                    Polygon p0 = polygons[j];
                    if (p0.matId == p.matId && EditablePolyUtils.samePlane(p0.plane, p.plane))
                    {
                        //Buscar si tienen una arista igual
                        int p0SharedEdgeIdx;
                        int pSharedEdgeIdx;
                        if (EditablePolyUtils.findShareEdgeBetweenPolygons(p0, p, out p0SharedEdgeIdx, out pSharedEdgeIdx))
                        {
                            //Obtener el tercer vertice del triangulo que no es parte de la arista compartida
                            Edge sharedEdge = p0.edges[p0SharedEdgeIdx];
                            Vertex thirdVert;
                            if (p.vertices[0] != sharedEdge.a && p.vertices[0] != sharedEdge.b)
                                thirdVert = p.vertices[0];
                            else if (p.vertices[1] != sharedEdge.a && p.vertices[1] != sharedEdge.b)
                                thirdVert = p.vertices[1];
                            else
                                thirdVert = p.vertices[2];

                            //Agregar el tercer vertice al poligno existente
                            //p0.vertices.Add(thirdVert);
                            EditablePolyUtils.addVertexToPolygon(p0, sharedEdge, thirdVert);

                            //Quitar arista compartida
                            p0.edges.Remove(sharedEdge);

                            //Agregar al poligono dos nuevas aristas que conectar los extremos de la arista compartida hacia el tercer vertice
                            Edge newPolEdge1 = new Edge();
                            newPolEdge1.a = sharedEdge.a;
                            newPolEdge1.b = thirdVert;
                            //newPolEdge1.faces = new List<Polygon>();
                            //newPolEdge1.faces.Add(p0);
                            //sharedEdge.a.edges.Add(newPolEdge1);
                            //sharedEdge.b.edges.Add(newPolEdge1);
                            p0.edges.Add(newPolEdge1);

                            Edge newPolEdge2 = new Edge();
                            newPolEdge2.a = thirdVert;
                            newPolEdge2.b = sharedEdge.b;
                            //newPolEdge2.faces = new List<Polygon>();
                            //newPolEdge2.faces.Add(p0);
                            //sharedEdge.a.edges.Add(newPolEdge2);
                            //sharedEdge.b.edges.Add(newPolEdge2);
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

            /*
            //Eliminar aristas interiores de los poligonos
            foreach (Polygon p in polygons)
            {
                EditablePolyUtils.computePolygonExternalEdges(p);
                EditablePolyUtils.sortPolygonVertices(p);
            }
            */


            //Unificar aristas de los poligonos
            foreach (Polygon p in polygons)
            {
                for (int i = 0; i < p.edges.Count; i++)
                {
                    int eIdx = EditablePolyUtils.addEdgeToListIfUnique(edges, p.edges[i]);
                    Edge e = edges[eIdx];
                    
                    //Nueva arista incorporada a la lista
                    if(eIdx == edges.Count - 1)
                    {
                        e.faces = new List<Polygon>();

                        //Agregar referencia a vertices que usan la arista
                        e.a.edges.Add(e);
                        e.b.edges.Add(e);
                    }
                    //Se usa arista existente de la lista
                    else
                    {
                        //Reemplazar en poligono por la nueva
                        p.edges[i] = e;
                    }

                    //Indicar a la arista que pertenece al poligono actual
                    e.faces.Add(p);
                }
            }



            dirtyValues = true;
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
                        /*v.normal = verts1[i].Normal;
                        v.color = verts1[i].Color;*/
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
                        /*v.normal = verts2[i].Normal;
                        v.texCoords = new Vector2(verts2[i].Tu, verts2[i].Tv);
                        v.color = verts2[i].Color;*/
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
                        /*v.normal = verts3[i].Normal;
                        v.texCoords = new Vector2(verts3[i].Tu0, verts3[i].Tv0);
                        v.color = verts3[i].Color;
                        v.texCoords2 = new Vector2(verts3[i].Tu1, verts3[i].Tv1);*/
                        origVertices.Add(v);
                    }
                    origMesh.D3dMesh.UnlockVertexBuffer();
                    break;
            }

            return origVertices;
        }

        /// <summary>
        /// Actualizar vertexBuffer de mesh original en base a la estructura interna del editablePoly
        /// </summary>
        private void updateMesh()
        {
            //Actualizar vertexBuffer
            using (VertexBuffer vb = mesh.D3dMesh.VertexBuffer)
            {
                switch (mesh.RenderType)
                {
                    case TgcMesh.MeshRenderType.VERTEX_COLOR:
                        TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.None, mesh.D3dMesh.NumberVertices);
                        for (int i = 0; i < verts1.Length; i++)
                        {
                            verts1[i].Position = vertices[indexBuffer[i]].position;
                        }
                        mesh.D3dMesh.SetVertexBufferData(verts1, LockFlags.None);
                        mesh.D3dMesh.UnlockVertexBuffer();
                        break;
                    case TgcMesh.MeshRenderType.DIFFUSE_MAP:
                        TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                        for (int i = 0; i < verts2.Length; i++)
                        {
                            verts2[i].Position = vertices[indexBuffer[i]].position;
                        }
                        mesh.D3dMesh.SetVertexBufferData(verts2, LockFlags.None);
                        mesh.D3dMesh.UnlockVertexBuffer();
                        break;
                    case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                        TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])mesh.D3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
                        for (int i = 0; i < verts3.Length; i++)
                        {
                            verts3[i].Position = vertices[indexBuffer[i]].position;
                        }
                        mesh.D3dMesh.SetVertexBufferData(verts3, LockFlags.None);
                        mesh.D3dMesh.UnlockVertexBuffer();
                        break;
                }

            }

            //Actualizar indexBuffer (en forma secuencial)
            using (IndexBuffer ib = mesh.D3dMesh.IndexBuffer)
            {
                short[] seqIndexBuffer = new short[indexBuffer.Length];
                for (int i = 0; i < seqIndexBuffer.Length; i++)
                {
                    seqIndexBuffer[i] = (short)i;
                }
                ib.SetData(seqIndexBuffer, 0, LockFlags.None);
            }

            //Actualizar attributeBuffer
            int[] attributeBuffer = mesh.D3dMesh.LockAttributeBufferArray(LockFlags.None);
            foreach (Polygon p in polygons)
            {
                //Setear en cada triangulo el material ID del poligono
                foreach (int idx in p.vbTriangles)
                {
                    int triIdx = idx / 3;
                    attributeBuffer[triIdx] = p.matId;
                }
            }
            mesh.D3dMesh.UnlockAttributeBuffer(attributeBuffer);
        }

        /// <summary>
        /// Actualizar estructuras internas en base a mesh original
        /// </summary>
        public void updateValuesFromMesh(TgcMesh mesh)
        {
            this.mesh = mesh;
            List<Vertex> origVertices = getMeshOriginalVertexData(mesh);
            for (int i = 0; i < origVertices.Count; i++)
            {
                Vertex origV = origVertices[i];
                Vertex v = vertices[indexBuffer[i]];
                v.position = origV.position;
                /*v.normal = origV.normal;
                v.color = origV.color;
                v.texCoords = origV.texCoords;
                v.texCoords2 = origV.texCoords2;*/
            }
            dirtyValues = true;
        }


        #endregion



        #region Estructuras auxiliares

        /// <summary>
        /// Primitiva generica
        /// </summary>
        public abstract class Primitive
        {
            protected bool selected;
            /// <summary>
            /// Indica si la primitiva esta seleccionada
            /// </summary>
            public bool Selected
            {
                get { return selected; }
                set { selected = value; }
            }

            public Primitive()
            {
                selected = false;
            }

            /// <summary>
            /// Tipo de primitiva
            /// </summary>
            public abstract PrimitiveType Type {get;}

            /// <summary>
            /// Proyectar primitva a rectangulo 2D en la pantalla
            /// </summary>
            /// <param name="transform">Mesh transform</param>
            /// <param name="box2D">Rectangulo 2D proyectado</param>
            /// <returns>False si es un caso degenerado de proyeccion y no debe considerarse</returns>
            public abstract bool projectToScreen(Matrix transform, out Rectangle box2D);

            /// <summary>
            /// Intersect ray againts primitive
            /// </summary>
            public abstract bool intersectRay(TgcRay tgcRay, Matrix transform, out Vector3 q);
        }

        /// <summary>
        /// Estructura auxiliar de vertice
        /// </summary>
        public class Vertex : Primitive
        {
            /// <summary>
            /// Sphere for ray-collisions
            /// </summary>
            private static readonly TgcBoundingSphere COLLISION_SPHERE = new TgcBoundingSphere(new Vector3(0, 0, 0), 2);

            public Vector3 position;
            /*public Vector3 normal;
            public Vector2 texCoords;
            public Vector2 texCoords2;
            public int color;*/
            public List<Edge> edges;
            public int vbIndex;

            public override string ToString()
            {
                return "Index: " + vbIndex + ", Pos: " + TgcParserUtils.printVector3(position);
            }

            public override PrimitiveType Type
            {
                get { return PrimitiveType.Vertex; }
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

        }

        /// <summary>
        /// Estructura auxiliar de arista
        /// </summary>
        public class Edge : Primitive
        {
            public Vertex a;
            public Vertex b;
            public List<Polygon> faces;

            public override string ToString()
            {
                return a.vbIndex + " => " + b.vbIndex;
            }

            public override PrimitiveType Type
            {
                get { return PrimitiveType.Edge; }
            }

            public override bool projectToScreen(Matrix transform, out Rectangle box2D)
            {
                return MeshCreatorUtils.projectSegmentToScreenRect(
                    Vector3.TransformCoordinate(a.position, transform),
                    Vector3.TransformCoordinate(b.position, transform), out box2D);
            }

            public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
            {
                //TODO: hacer ray-obb (hacer un obb previamente para el edge)
                q = Vector3.Empty;
                return false;
            }

        }

        /// <summary>
        /// Estructura auxiliar de poligono
        /// </summary>
        public class Polygon : Primitive
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

            public override PrimitiveType Type
            {
                get { return PrimitiveType.Polygon; }
            }

            public override bool projectToScreen(Matrix transform, out Rectangle box2D)
            {
                Vector3[] v = new Vector3[vertices.Count];
                for (int i = 0; i < v.Length; i++)
			    {
                    v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
			    }
                return MeshCreatorUtils.projectPolygon(v, out box2D);
            }

            public override bool intersectRay(TgcRay ray, Matrix transform, out Vector3 q)
            {
                Vector3[] v = new Vector3[vertices.Count];
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = Vector3.TransformCoordinate(vertices[i].position, transform);
                }
                float t;
                return TgcCollisionUtils.intersectRayConvexPolygon(ray, v, plane, out t, out q);
            }

            /// <summary>
            /// Normal del plano del poligono
            /// </summary>
            public Vector3 getNormal()
            {
                return new Vector3(plane.A, plane.B, plane.C);
            }

        }

        /// <summary>
        /// Iterar sobre lista de primitivas
        /// </summary>
        /// <param name="primitiveType">Primitiva</param>
        /// <param name="i">indice</param>
        /// <returns>Elemento o null si no hay mas</returns>
        private Primitive iteratePrimitive(PrimitiveType primitiveType, int i)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Vertex:
                    if (i == vertices.Count) return null;
                    return vertices[i];
                case PrimitiveType.Edge:
                    if (i == edges.Count) return null;
                    return edges[i];
                case PrimitiveType.Polygon:
                    if (i == polygons.Count) return null;
                    return polygons[i];
            }
            return null;
        }

        #endregion

        




        
    }
}
