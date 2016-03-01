using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Viewer.Utils.Shaders;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Viewer.Utils.TgcGeometry
{
    public class TgcSphere : IRenderObject, ITransformObject
    {
        /// <summary>
        ///     Crear un nuevo TgcSphere igual a este
        /// </summary>
        /// <returns>Sphere clonado</returns>
        public virtual TgcSphere clone()
        {
            var cloneSphere = new TgcSphere(radius, color, translation);

            if (Texture != null) cloneSphere.setTexture(Texture.clone());

            cloneSphere.AutoTransformEnable = AutoTransformEnable;
            cloneSphere.Transform = Transform;
            cloneSphere.rotation = rotation;
            cloneSphere.alphaBlendEnable = alphaBlendEnable;
            cloneSphere.uvOffset = uvOffset;

            cloneSphere.updateValues();

            return cloneSphere;
        }

        /// <summary>
        ///     Convierte el TgcSphere en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            //Obtener matriz para transformar vertices
            if (AutoTransformEnable)
            {
                Transform = Matrix.Scaling(radius, radius, radius) * Matrix.Scaling(Scale) *
                            Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            Matrix.Translation(translation);
            }

            //Crear mesh con DiffuseMap
            if (Texture != null)
            {
                //Crear Mesh
                var d3dMesh = new Mesh(indices.Count / 3, indices.Count, MeshFlags.Managed,
                    TgcSceneLoader.TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

                //Cargar VertexBuffer
                using (var vb = d3dMesh.VertexBuffer)
                {
                    var data = vb.Lock(0, 0, LockFlags.None);
                    for (var j = 0; j < indices.Count; j++)
                    {
                        var v = new TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex();
                        var vSphere = vertices[indices[j]];

                        //vertices
                        v.Position = Vector3.TransformCoordinate(vSphere.getPosition(), Transform);

                        //normals
                        v.Normal = vSphere.getNormal();

                        //texture coordinates diffuseMap
                        v.Tu = vSphere.Tu;
                        v.Tv = vSphere.Tv;

                        //color
                        v.Color = vSphere.Color;

                        data.Write(v);
                    }
                    vb.Unlock();
                }

                //Cargar IndexBuffer en forma plana
                using (var ib = d3dMesh.IndexBuffer)
                {
                    var vIndices = new short[indices.Count];
                    for (var j = 0; j < vIndices.Length; j++)
                    {
                        vIndices[j] = (short)j;
                    }
                    ib.SetData(vIndices, 0, LockFlags.None);
                }

                //Calcular normales
                //d3dMesh.ComputeNormals();

                //Malla de TGC
                var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
                tgcMesh.DiffuseMaps = new[] { Texture.clone() };
                tgcMesh.Materials = new[] { TgcD3dDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                return tgcMesh;
            }

            //Crear mesh con solo color
            else
            {
                //Crear Mesh
                var d3dMesh = new Mesh(indices.Count / 3, indices.Count, MeshFlags.Managed,
                    TgcSceneLoader.TgcSceneLoader.VertexColorVertexElements, D3DDevice.Instance.Device);

                //Cargar VertexBuffer
                using (var vb = d3dMesh.VertexBuffer)
                {
                    var data = vb.Lock(0, 0, LockFlags.None);
                    for (var j = 0; j < indices.Count; j++)
                    {
                        var v = new TgcSceneLoader.TgcSceneLoader.VertexColorVertex();
                        var vSphere = vertices[indices[j]];

                        //vertices
                        v.Position = Vector3.TransformCoordinate(vSphere.getPosition(), Transform);

                        //normals
                        v.Normal = vSphere.getNormal();

                        //color
                        v.Color = vSphere.Color;

                        data.Write(v);
                    }
                    vb.Unlock();
                }

                //Cargar IndexBuffer en forma plana
                using (var ib = d3dMesh.IndexBuffer)
                {
                    var vIndices = new short[indices.Count];
                    for (var j = 0; j < vIndices.Length; j++)
                    {
                        vIndices[j] = (short)j;
                    }
                    ib.SetData(vIndices, 0, LockFlags.None);
                }

                //Malla de TGC
                var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.VERTEX_COLOR);
                tgcMesh.Materials = new[] { TgcD3dDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                return tgcMesh;
            }
        }

        #region PROTECTED FIELDS

        protected IndexBuffer indexBuffer;
        protected VertexBuffer vertexBuffer;
        protected bool mustUpdate = true;

        protected List<int> indices;
        protected List<Vertex.PositionColoredTexturedNormal> vertices;

        #endregion PROTECTED FIELDS

        #region PROPERTIES

        /// <summary>
        ///     Cuando esta en true se renderiza tambien el wireframe.
        /// </summary>
        public bool RenderEdges { get; set; }

        /// <summary>
        ///     Obliga a la esfera a recalcular sus vertices.
        /// </summary>
        public bool ForceUpdate { get; set; }

        protected int verticesCount;

        /// <summary>
        ///     Cantidad de vertices
        /// </summary>
        public int VertexCount
        {
            get { return verticesCount; }
        }

        protected int triangleCount;

        /// <summary>
        ///     Cantidad de triangulos
        /// </summary>
        public int TriangleCount
        {
            get { return triangleCount; }
        }

        private float radius;

        /// <summary>
        ///     Radio de la esfera
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set
            {
                if (value > 0) radius = value;
                updateBoundingVolume();
            }
        }

        private int levelOfDetail;

        /// <summary>
        ///     Nivel de detalle de la esfera. La cantidad de veces que se subdividen los triangulos de la superficie.
        /// </summary>
        public int LevelOfDetail
        {
            get { return levelOfDetail; }
            set
            {
                if (value != levelOfDetail)
                {
                    levelOfDetail = value;

                    mustUpdate = true;
                }
            }
        }

        protected Vector3 scale;

        /// <summary>
        ///     Escala de la esfera. Siempre es (1,1,1)
        /// </summary>
        public virtual Vector3 Scale
        {
            get { return scale; }
            set {; }
        }

        private Color color;

        /// <summary>
        ///     Color de los vértices de la esfera
        /// </summary>
        public Color Color
        {
            get { return color; }
        }

        /// <summary>
        ///     Textura de la esfera
        /// </summary>
        public TgcTexture Texture { get; private set; }

        protected Effect effect;

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Matriz final que se utiliza para aplicar transformaciones a la malla.
        ///     Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        ///     en base a los valores de: Position, Rotation, Scale.
        ///     Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en True.
        /// </summary>
        public bool AutoTransformEnable { get; set; }

        protected Vector3 translation;

        /// <summary>
        ///     Posicion absoluta del centro de la esfera
        /// </summary>
        public Vector3 Position
        {
            get { return translation; }
            set
            {
                translation = value;
                updateBoundingVolume();
            }
        }

        protected Vector3 rotation;

        /// <summary>
        ///     Rotación absoluta de la esfera
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        protected bool enabled;

        /// <summary>
        ///     Indica si la esfera esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected TgcBoundingSphere boundingSphere;

        /// <summary>
        ///     BoundingSphere de la esfera
        /// </summary>
        public TgcBoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }

        protected bool alphaBlendEnable;

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        private Vector2 uvOffset;

        /// <summary>
        ///     Offset UV de textura
        /// </summary>
        public Vector2 UVOffset
        {
            get { return uvOffset; }
            set
            {
                if (uvOffset != value)
                {
                    uvOffset = value;
                    mustUpdate = true;
                }
            }
        }

        private Vector2 uvTiling;

        /// <summary>
        ///     Tiling UV de textura
        /// </summary>
        public Vector2 UVTiling
        {
            get { return uvTiling; }
            set
            {
                if (uvTiling != value)
                {
                    uvTiling = value;
                    mustUpdate = true;
                }
            }
        }

        public enum eBasePoly
        {
            CUBE,
            ICOSAHEDRON
        }

        protected eBasePoly basePoly;

        /// <summary>
        ///     Poliedro que se utiliza como base para la creacion de la esfera.
        /// </summary>
        public eBasePoly BasePoly
        {
            get { return basePoly; }
            set
            {
                if (basePoly != value)
                {
                    basePoly = value;
                    mustUpdate = true;
                }
            }
        }

        protected bool inflate;

        /// <summary>
        ///     Cuando esta en false se puede ver el poliedro original con sus subdivisiones.
        /// </summary>
        public bool Inflate
        {
            get { return inflate; }
            set
            {
                if (inflate != value)
                {
                    inflate = value;
                    mustUpdate = true;
                }
            }
        }

        #endregion PROPERTIES

        #region SETTERS

        /// <summary>
        ///     Configurar color de la esfera y setea la technique POSITION_COLORED. Unsetea la textura.
        /// </summary>
        /// <param name="color"></param>
        public virtual void setColor(Color color)
        {
            Texture = null;

            technique = TgcShaders.T_POSITION_COLORED;

            if (this.color != color) mustUpdate = true;

            this.color = color;
        }

        /// <summary>
        ///     Configurar textura de la esfera y setea la technique POSITION_TEXTURED.
        /// </summary>
        public virtual void setTexture(TgcTexture texture)
        {
            if (Texture != null) Texture.dispose();

            Texture = texture;

            technique = TgcShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        ///     Configurar valores de posicion y radio en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la esfera</param>
        /// <param name="size">Radio de la esfera</param>
        protected virtual void setPositionRadius(Vector3 position, float radius)
        {
            translation = position;
            this.radius = radius;
            updateBoundingVolume();
        }

        #endregion SETTERS

        #region CONSTRUCTORS

        /// <summary>
        ///     Crea una esfera vacia
        /// </summary>
        public TgcSphere()
            : this(1, Color.White, new Vector3(0, 0, 0))
        {
        }

        /// <summary>
        ///     Crea una esfera con el radio, color y posicion del centro.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="center"></param>
        public TgcSphere(float radius, Color color, Vector3 center)
        {
            configure(radius, color, null, center);
        }

        /// <summary>
        ///     Crea una esfera con radio, textura y posicion del centro.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="texture"></param>
        /// <param name="center"></param>
        public TgcSphere(float radius, TgcTexture texture, Vector3 center)
        {
            configure(radius, Color.White, texture, center);
        }

        protected void configure(float radius, Color color, TgcTexture texture, Vector3 center)
        {
            AutoTransformEnable = true;
            Transform = Matrix.Identity;
            translation = center;
            rotation = new Vector3(0, 0, 0);
            enabled = true;
            scale = new Vector3(1, 1, 1);
            alphaBlendEnable = false;
            uvOffset = new Vector2(0, 0);

            //BoundingSphere
            boundingSphere = new TgcBoundingSphere();

            //Shader
            effect = GuiController.Instance.Shaders.VariosShader;

            //Tipo de vertice y technique
            if (texture != null) setTexture(texture);
            else setColor(color);

            basePoly = eBasePoly.ICOSAHEDRON;
            levelOfDetail = 2;
            inflate = true;
            ForceUpdate = false;
            uvTiling = new Vector2(1, 1);
        }

        #endregion CONSTRUCTORS

        #region GEOMETRY

        /// <summary>
        ///     Vuelve a crear la esfera si hubo cambio en el nivel de detalle, color o coordenadas de textura o si ForceUpdate
        ///     esta en true.
        /// </summary>
        public virtual void updateValues()
        {
            if (!mustUpdate && !ForceUpdate) return;

            if (indexBuffer != null && !indexBuffer.Disposed) indexBuffer.Dispose();
            if (vertexBuffer != null && !vertexBuffer.Disposed) vertexBuffer.Dispose();

            //Obtengo las posiciones de los vertices e indices de la esfera
            List<Vector3> positions;

            createSphereSubdividingAPolyhedron(out positions, out indices);

            ///////

            vertices = new List<Vertex.PositionColoredTexturedNormal>();

            var iverticesU1 = new List<int>();

            var polos = new int[2];
            var p = 0;

            var c = color.ToArgb();

            var twoPi = FastMath.TWO_PI;

            //Creo la lista de vertices
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                var u = 0.5f + FastMath.Atan2(pos.Z, pos.X) / twoPi;
                var v = 0.5f - 2 * FastMath.Asin(pos.Y) / twoPi;
                vertices.Add(new Vertex.PositionColoredTexturedNormal(pos, c, UVTiling.X * u + UVOffset.X,
                    UVTiling.Y * v + UVOffset.Y, pos));

                if (u == 1 || esPolo(vertices[i]))
                {
                    iverticesU1.Add(i);
                    if (u != 1) polos[p++] = i;
                }
            }

            //Corrijo los triangulos que tienen mal las coordenadas debido a vertices compartidos
            fixTexcoords(vertices, indices, iverticesU1, polos);

            verticesCount = vertices.Count;
            triangleCount = indices.Count / 3;

            vertexBuffer = new VertexBuffer(typeof(Vertex.PositionColoredTexturedNormal), verticesCount,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, Vertex.PositionColoredTexturedNormal.Format, Pool.Default);
            vertexBuffer.SetData(vertices.ToArray(), 0, LockFlags.None);

            indexBuffer = new IndexBuffer(typeof(int), indices.Count, D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly,
                Pool.Default);
            indexBuffer.SetData(indices.ToArray(), 0, LockFlags.None);

            mustUpdate = false;
        }

        /// <summary>
        ///     Subdivide el poliedro base y normaliza los vertices para que formen una esfera de radio 1.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        protected void createSphereSubdividingAPolyhedron(out List<Vector3> vertices, out List<int> indices)
        {
            indices = new List<int>();
            vertices = new List<Vector3>();

            switch (basePoly)
            {
                case eBasePoly.ICOSAHEDRON:
                    GeometryProvider.Icosahedron(vertices, indices);
                    break;

                case eBasePoly.CUBE:
                    GeometryProvider.Cube(vertices, indices);
                    break;
            }

            for (var i = 0; i < levelOfDetail; i++)
                GeometryProvider.Subdivide(vertices, indices);

            if (Inflate)
                for (var i = 0; i < vertices.Count; i++)
                    vertices[i] = Vector3.Normalize(vertices[i]);
        }

        protected bool esPolo(Vertex.PositionColoredTexturedNormal v)
        {
            return v.X == 0 && v.Z == 0;
        }

        /// <summary>
        ///     Duplica los vertices que deben tener coordenadas de textura diferentes para diferentes triangulos
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="iverticesU1"></param>
        /// <param name="polos"></param>
        protected virtual void fixTexcoords(List<Vertex.PositionColoredTexturedNormal> vertices, List<int> indices,
            List<int> iverticesU1, int[] polos)
        {
            var duplicados = new Dictionary<int, int>();
            var U0p5 = 0.5f * UVTiling.X + UVOffset.X;
            var U1 = UVTiling.X + UVOffset.X;

            //Fix compartidos
            foreach (var idx in iverticesU1)
            {
                for (var i = 0; i < indices.Count; i += 3)
                {
                    //Triangulo que tiene ese vertice.
                    if (indices[i] == idx || indices[i + 1] == idx || indices[i + 2] == idx)
                    {
                        var i1 = indices[i];
                        var i2 = indices[i + 1];
                        var i3 = indices[i + 2];

                        //Solo me importan los que tienen un vertice con u=1(fin de la textura) y otro menor a 0.5 (comienzo de la textura)
                        if ((!esPolo(vertices[i1]) && vertices[i1].Tu < U0p5) ||
                            (!esPolo(vertices[i2]) && vertices[i2].Tu < U0p5) ||
                            (!esPolo(vertices[i3]) && vertices[i3].Tu < U0p5))
                        {
                            var u1 = new List<int>();
                            var um = new List<int>();

                            //Clasifico cada vertice segun su Tu1
                            for (var j = 0; j < 3; j++)
                                if (vertices[indices[i + j]].Tu == U1 || esPolo(vertices[indices[i + j]]))
                                    u1.Add(i + j);
                                else if (vertices[indices[i + j]].Tu < U0p5) um.Add(i + j);

                            if (um.Count == 1 && !(esPolo(vertices[indices[u1[0]]]) && vertices[indices[um[0]]].X >= 0))
                            {
                                //Casos:
                                //2 vertices con u=1 o uno con u=1 y otro con u>0.5
                                //1 vertice es el polo, y uno de los vertices esta al final de la textura y otro al principio

                                //La coordenada textura del de u >0.5 pasa a ser 1+u
                                indices[um[0]] = dupWithU(vertices, indices, duplicados, um[0],
                                    vertices[indices[um[0]]].Tu + UVTiling.X);
                            }
                            else if (!esPolo(vertices[indices[u1[0]]]))
                                // Caso:
                                // 1 vertice con u=1 y dos con u<0.5

                                // El u del vertice con u=1 pasa a ser 0
                                indices[u1[0]] = dupWithU(vertices, indices, duplicados, u1[0], UVOffset.X);
                        }
                    }
                }
            }

            //Fix polos

            for (var p = 0; p < 2; p++)
            {
                var first = true;
                for (var i = 0; i < indices.Count; i += 3) //Por cada triangulo
                {
                    var iipolo = i;
                    for (; iipolo < i + 3 && indices[iipolo] != polos[p]; iipolo++) ;
                    //Si un vertice es el polo
                    if (iipolo < i + 3)
                    {
                        var u = new Vertex.PositionColoredTexturedNormal[2];

                        var n = 0;
                        //Guardo los vertices que no son polos.
                        for (var j = 0; j < 3; j++) if (i + j != iipolo) u[n++] = vertices[indices[i + j]];

                        var minU = FastMath.Min(u[0].Tu, u[1].Tu);

                        var pole = vertices[polos[p]];

                        //Chequea que no sea un triangulo rectangulo
                        var zeroXZ = u[0];
                        var noRectangulo = false;

                        if (u[0].X != 0 && u[0].Z != 0) zeroXZ = u[0];
                        else if (u[1].X != 0 && u[1].Z != 0) zeroXZ = u[1];
                        else noRectangulo = true;

                        //Interpolo Tu1
                        if (basePoly.Equals(eBasePoly.ICOSAHEDRON) || noRectangulo)

                            pole.Tu = minU + FastMath.Abs(u[0].Tu - u[1].Tu) / 2;
                        else
                            pole.Tu = zeroXZ.Tu;

                        if (first) //Si es la primera vez que lo hago, modifico el vertice.
                        {
                            vertices[polos[p]] = pole;

                            first = false;
                        }
                        else //Sino lo duplico.
                        {
                            indices[iipolo] = vertices.Count;
                            vertices.Add(pole);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Busca el duplicado del vertice con texcoord u, si no existe lo crea y retorna el indice.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="dDup"></param>
        /// <param name="idx"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        protected static int dupWithU(List<Vertex.PositionColoredTexturedNormal> vertices, List<int> indices,
            Dictionary<int, int> dDup, int idx, float u)
        {
            int newIndx;
            if (!dDup.ContainsKey(indices[idx]))
            {
                var newV = vertices[indices[idx]];
                newV.Tu = u;

                newIndx = vertices.Count;
                vertices.Add(newV);

                dDup.Add(indices[idx], newIndx);
            }
            else newIndx = dDup[indices[idx]];
            return newIndx;
        }

        #endregion GEOMETRY

        #region RENDER & DISPOSE

        /// <summary>
        ///     Renderizar la esfera
        /// </summary>
        public virtual void render()
        {
            if (!enabled)
                return;

            var texturesManager = GuiController.Instance.TexturesManager;

            //transformacion
            if (AutoTransformEnable)
            {
                Transform = Matrix.Scaling(radius, radius, radius) * Matrix.Scaling(Scale) *
                            Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            Matrix.Translation(translation);
            }

            //Activar AlphaBlending
            activateAlphaBlend();

            //renderizar
            if (Texture != null) texturesManager.shaderSet(effect, "texDiffuseMap", Texture);
            else texturesManager.clear(0);

            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(effect, Transform);
            effect.Technique = technique;

            D3DDevice.Instance.Device.VertexDeclaration = Vertex.PositionColoredTexturedNormal_Declaration;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            var oldIndex = D3DDevice.Instance.Device.Indices;
            D3DDevice.Instance.Device.Indices = indexBuffer;

            //Render con shader
            renderWithFill(D3DDevice.Instance.Device.RenderState.FillMode);

            if (RenderEdges)
            {
                if (Texture == null) effect.Technique = TgcShaders.T_POSITION_TEXTURED;
                else effect.Technique = TgcShaders.T_POSITION_COLORED;

                renderWithFill(FillMode.WireFrame);
            }

            //Desactivar AlphaBlend
            resetAlphaBlend();

            D3DDevice.Instance.Device.Indices = oldIndex;
        }

        protected void renderWithFill(FillMode fillmode)
        {
            var old = D3DDevice.Instance.Device.RenderState.FillMode;
            D3DDevice.Instance.Device.RenderState.FillMode = fillmode;

            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0,
                triangleCount);
            effect.EndPass();
            effect.End();
            D3DDevice.Instance.Device.RenderState.FillMode = old;
        }

        /// <summary>
        ///     Activar AlphaBlending, si corresponde
        /// </summary>
        protected void activateAlphaBlend()
        {
            if (alphaBlendEnable)
            {
                D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            }
        }

        /// <summary>
        ///     Desactivar AlphaBlending
        /// </summary>
        protected void resetAlphaBlend()
        {
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Liberar los recursos de la esfera
        /// </summary>
        public virtual void dispose()
        {
            if (Texture != null)
            {
                Texture.dispose();
                Texture = null;
            }

            if (vertexBuffer != null && !vertexBuffer.Disposed) vertexBuffer.Dispose();
            if (indexBuffer != null && !indexBuffer.Disposed) indexBuffer.Dispose();

            boundingSphere.dispose();
        }

        #endregion RENDER & DISPOSE

        #region TRANSFORMATIONS

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void move(Vector3 v)
        {
            move(v.X, v.Y, v.Z);
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void move(float x, float y, float z)
        {
            translation.X += x;
            translation.Y += y;
            translation.Z += z;

            updateBoundingVolume();
        }

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        public void moveOrientedY(float movement)
        {
            var z = (float)Math.Cos(rotation.Y) * movement;
            var x = (float)Math.Sin(rotation.Y) * movement;

            move(x, 0, z);
        }

        /// <summary>
        ///     Obtiene la posicion absoluta de la malla, recibiendo un vector ya creado para
        ///     almacenar el resultado
        /// </summary>
        /// <param name="pos">Vector ya creado en el que se carga el resultado</param>
        public void getPosition(Vector3 pos)
        {
            pos.X = translation.X;
            pos.Y = translation.Y;
            pos.Z = translation.Z;
        }

        /// <summary>
        ///     Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateX(float angle)
        {
            rotation.X += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateY(float angle)
        {
            rotation.Y += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateZ(float angle)
        {
            rotation.Z += angle;
        }

        /// <summary>
        ///     Actualiza el BoundingSphere de la esfera
        /// </summary>
        protected virtual void updateBoundingVolume()
        {
            BoundingSphere.setValues(Position, Radius);
        }

        #endregion TRANSFORMATIONS
    }

    /// <remarks> http://gamedev.stackexchange.com/questions/31308/algorithm-for-creating-spheres David Lively</remarks>
    public static class GeometryProvider
    {
        /// <summary> Divide cada triangulo en cuatro.</summary>
        /// <remarks>
        ///     i0
        ///     /  \
        ///     m02-m01
        ///     /  \ /  \
        ///     i2---m12---i1
        /// </remarks>
        /// <param name="vectors"></param>
        /// <param name="indices"></param>
        public static void Subdivide(List<Vector3> vectors, List<int> indices)
        {
            var midpointIndices = new Dictionary<string, int>();

            var newIndices = new List<int>(indices.Count * 4);

            for (var i = 0; i < indices.Count - 2; i += 3)
            {
                var i0 = indices[i];
                var i1 = indices[i + 1];
                var i2 = indices[i + 2];

                var m01 = GetMidpointIndex(midpointIndices, vectors, i0, i1);
                var m12 = GetMidpointIndex(midpointIndices, vectors, i1, i2);
                var m02 = GetMidpointIndex(midpointIndices, vectors, i2, i0);

                newIndices.AddRange(
                    new[]
                    {
                        i0, m01, m02
                        ,
                        i1, m12, m01
                        ,
                        i2, m02, m12
                        ,
                        m02, m01, m12
                    }
                    );
            }

            indices.Clear();
            indices.AddRange(newIndices);
        }

        /// <summary>
        ///     Busca el indice del vertice que se encuentra entre los vertices i0 e i1, de no existir crea el vertice.
        /// </summary>
        /// <param name="midpointIndices"></param>
        /// <param name="vertices"></param>
        /// <param name="i0"></param>
        /// <param name="i1"></param>
        /// <returns></returns>
        private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<Vector3> vertices, int i0,
            int i1)
        {
            var edgeKey = string.Format("{0}_{1}", Math.Min(i0, i1), Math.Max(i0, i1));

            var midpointIndex = -1;

            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                var v0 = vertices[i0];
                var v1 = vertices[i1];

                var midpoint = (v0 + v1) * 0.5f;

                if (vertices.Contains(midpoint))
                    midpointIndex = vertices.IndexOf(midpoint);
                else
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                }
            }

            return midpointIndex;
        }

        /// <summary>
        ///     Retorna la posicion de los vertices y los indices de un cubo.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void Cube(List<Vector3> vertices, List<int> indices)
        {
            vertices.AddRange(new[]
            {
                new Vector3(-0.65f, -0.65f, -0.65f),
                new Vector3(-0.65f, -0.65f, 0.65f),
                new Vector3(-0.65f, 0.65f, -0.65f),
                new Vector3(-0.65f, 0.65f, 0.65f),
                new Vector3(0.65f, -0.65f, -0.65f),
                new Vector3(0.65f, -0.65f, 0.65f),
                new Vector3(0.65f, 0.65f, -0.65f),
                new Vector3(0.65f, 0.65f, 0.65f)
            });
            indices.AddRange(new[]
            {
                0, 2, 6,
                0, 6, 4,
                7, 4, 6,
                7, 5, 4,
                7, 1, 5,
                7, 3, 1,
                0, 3, 2,
                0, 1, 3,
                0, 5, 1,
                0, 4, 5,
                7, 6, 2,
                7, 2, 3
            });
        }

        /// <summary>
        ///     Retorna la posicion de los vertices y los indices de un icosaedro regular (Un d20)
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void Icosahedron(List<Vector3> vertices, List<int> indices)
        {
            indices.AddRange(
                new[]
                {
                    0, 4, 1,
                    0, 9, 4,
                    9, 5, 4,
                    4, 5, 8,
                    4, 8, 1,
                    8, 10, 1,
                    8, 3, 10,
                    5, 3, 8,
                    5, 2, 3,
                    2, 7, 3,
                    7, 10, 3,
                    7, 6, 10,
                    7, 11, 6,
                    11, 0, 6,
                    0, 1, 6,
                    6, 1, 10,
                    9, 0, 11,
                    9, 11, 2,
                    9, 2, 5,
                    7, 2, 11
                });

            for (var i = 0; i < indices.Count; i++) indices[i] += vertices.Count;

            var X = 0.525731112119133606f;
            var Z = 0.850650808352039932f;

            vertices.AddRange(
                new[]
                {
                    new Vector3(-X, 0f, Z),
                    new Vector3(X, 0f, Z),
                    new Vector3(-X, 0f, -Z),
                    new Vector3(X, 0f, -Z),
                    new Vector3(0f, Z, X),
                    new Vector3(0f, Z, -X),
                    new Vector3(0f, -Z, X),
                    new Vector3(0f, -Z, -X),
                    new Vector3(Z, X, 0f),
                    new Vector3(-Z, X, 0f),
                    new Vector3(Z, -X, 0f),
                    new Vector3(-Z, -X, 0f)
                }
                );
        }
    }

    public class Vertex
    {
        public static readonly VertexElement[] PositionColoredTexturedNormal_VertexElements =
        {
            new VertexElement(0, 0, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            new VertexElement(0, 12, DeclarationType.Color,
                DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            new VertexElement(0, 16, DeclarationType.Float2,
                DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            new VertexElement(0, 24, DeclarationType.Float3,
                DeclarationMethod.Default,
                DeclarationUsage.Normal, 0),
            VertexElement.VertexDeclarationEnd
        };

        public static VertexDeclaration PositionColoredTexturedNormal_Declaration =
            new VertexDeclaration(D3DDevice.Instance.Device, PositionColoredTexturedNormal_VertexElements);

        public struct PositionColoredTexturedNormal
        {
            public float X;
            public float Y;
            public float Z;
            public int Color;
            public float Tu;
            public float Tv;
            public float NX;
            public float NY;
            public float NZ;

            public PositionColoredTexturedNormal(Vector3 pos, int color, float u, float v, Vector3 normal)
                : this(pos.X, pos.Y, pos.Z, color, u, v, normal.X, normal.Y, normal.Z)
            {
            }

            public PositionColoredTexturedNormal(float X, float Y, float Z, int color, float Tu1, float Tv1, float NX,
                float NY, float NZ)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
                Color = color;
                Tu = Tu1;
                Tv = Tv1;
                this.NX = NX;
                this.NY = NY;
                this.NZ = NZ;
            }

            public Vector3 getPosition()
            {
                return new Vector3(X, Y, Z);
            }

            public Vector3 getNormal()
            {
                return new Vector3(NX, NY, NZ);
            }

            public static VertexFormats Format
            {
                get { return VertexFormats.PositionNormal | VertexFormats.Texture1 | VertexFormats.Diffuse; }
            }

            public override string ToString()
            {
                return getPosition().ToString();
            }
        }
    }
}