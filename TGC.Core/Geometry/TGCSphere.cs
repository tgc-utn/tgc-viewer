using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para crear una esfera 3D de tamaño variable, con color, nivel de detalle y Textura.
    /// </summary>
    public class TGCSphere : IRenderObject, ITransformObject
    {
        /// <summary>
        ///     Crear un nuevo TgcSphere igual a este
        /// </summary>
        /// <returns>Sphere clonado</returns>
        public virtual TGCSphere Clone()
        {
            var cloneSphere = new TGCSphere(radius, Color, translation);

            if (Texture != null)
            {
                cloneSphere.SetTexture(Texture.Clone());
            }

            cloneSphere.AutoTransformEnable = AutoTransformEnable;
            cloneSphere.Transform = Transform;
            cloneSphere.Rotation = Rotation;
            cloneSphere.AlphaBlendEnable = AlphaBlendEnable;
            cloneSphere.uvOffset = uvOffset;

            cloneSphere.UpdateValues();

            return cloneSphere;
        }

        /// <summary>
        ///     Convierte el TgcSphere en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh ToMesh(string meshName)
        {
            //Obtener matriz para transformar vertices
            if (AutoTransformEnable)
            {
                Transform = TGCMatrix.Scaling(radius, radius, radius) *
                            TGCMatrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                            TGCMatrix.Translation(translation);
            }

            return TgcMesh.FromTGCSphere(meshName, Texture, indices, vertices, Transform, AlphaBlendEnable);
        }

        #region FIELDS

        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;
        private bool mustUpdate = true;
        private List<int> indices;
        private List<Vertex.PositionColoredTexturedNormal> vertices;
        private TGCVector3 rotation;
        private TGCVector3 scale;
        private int levelOfDetail;
        private float radius;
        private TGCVector3 translation;
        private TGCVector2 uvOffset;
        private TGCVector2 uvTiling;
        private bool inflate;

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        ///     Cuando esta en true se renderiza tambien el wireframe.
        /// </summary>
        public bool RenderEdges { get; set; }

        /// <summary>
        ///     Obliga a la esfera a recalcular sus vertices.
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        ///     Cantidad de vertices
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        ///     Cantidad de triangulos
        /// </summary>
        public int TriangleCount { get; private set; }

        /// <summary>
        ///     Radio de la esfera
        /// </summary>
        public float Radius
        {
            get => radius;
            set
            {
                if (value > 0)
                {
                    radius = value;
                }

                UpdateBoundingVolume();
            }
        }

        /// <summary>
        ///     Nivel de detalle de la esfera. La cantidad de veces que se subdividen los triangulos de la superficie.
        /// </summary>
        public int LevelOfDetail
        {
            get => levelOfDetail;
            set
            {
                if (value != levelOfDetail)
                {
                    levelOfDetail = value;

                    mustUpdate = true;
                }
            }
        }

        /// <summary>
        ///     Escala de la esfera. Siempre es (1,1,1)
        /// </summary>
        public virtual TGCVector3 Scale
        {
            get => scale;
            set => Console.WriteLine("TODO esta bien que pase por aca? value=" + value);
        }

        /// <summary>
        ///     Color de los vértices de la esfera
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Textura de la esfera
        /// </summary>
        public TGCTexture Texture { get; private set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique { get; set; }

        /// <summary>
        ///     Matriz final que se utiliza para aplicar transformaciones a la malla.
        ///     Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        ///     en base a los valores de: Position, Rotation, Scale.
        ///     Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        public TGCMatrix Transform { get; set; }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en False.
        /// </summary>
        [Obsolete(
            "Utilizar esta propiedad en juegos complejos se pierde el control, es mejor utilizar transformaciones con matrices.")]
        public bool AutoTransformEnable { get; set; }

        /// <summary>
        ///     Posicion absoluta del centro de la esfera
        /// </summary>
        public TGCVector3 Position
        {
            get => translation;
            set
            {
                translation = value;
                UpdateBoundingVolume();
            }
        }

        /// <summary>
        ///     Rotación absoluta de la esfera
        /// </summary>
        public TGCVector3 Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        /// <summary>
        ///     Indica si la esfera esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     BoundingSphere de la esfera
        /// </summary>
        public TgcBoundingSphere BoundingSphere { get; private set; }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Offset UV de textura
        /// </summary>
        public TGCVector2 UVOffset
        {
            get => uvOffset;
            set
            {
                if (uvOffset != value)
                {
                    uvOffset = value;
                    mustUpdate = true;
                }
            }
        }

        /// <summary>
        ///     Tiling UV de textura
        /// </summary>
        public TGCVector2 UVTiling
        {
            get => uvTiling;
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
            get => basePoly;
            set
            {
                if (basePoly != value)
                {
                    basePoly = value;
                    mustUpdate = true;
                }
            }
        }

        /// <summary>
        ///     Cuando esta en false se puede ver el poliedro original con sus subdivisiones.
        /// </summary>
        public bool Inflate
        {
            get => inflate;
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
        public virtual void SetColor(Color color)
        {
            Texture = null;

            Technique = TGCShaders.T_POSITION_COLORED;

            if (Color != color)
            {
                mustUpdate = true;
            }

            Color = color;
        }

        /// <summary>
        ///     Configurar textura de la esfera y setea la technique POSITION_TEXTURED.
        /// </summary>
        public virtual void SetTexture(TGCTexture texture)
        {
            if (Texture != null)
            {
                Texture.Dispose();
            }

            Texture = texture;

            Technique = TGCShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        ///     Configurar valores de posicion y radio en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la esfera</param>
        /// <param name="radius">Radio de la esfera</param>
        protected virtual void SetPositionRadius(TGCVector3 position, float radius)
        {
            translation = position;
            this.radius = radius;
            UpdateBoundingVolume();
        }

        #endregion SETTERS

        #region CONSTRUCTORS

        /// <summary>
        ///     Crea una esfera vacia
        /// </summary>
        public TGCSphere()
            : this(1, Color.White, TGCVector3.Empty)
        {
        }

        /// <summary>
        ///     Crea una esfera con el radio, color y posicion del centro.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="center"></param>
        public TGCSphere(float radius, Color color, TGCVector3 center)
        {
            Configure(radius, color, null, center);
        }

        /// <summary>
        ///     Crea una esfera con radio, textura y posicion del centro.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="texture"></param>
        /// <param name="center"></param>
        public TGCSphere(float radius, TGCTexture texture, TGCVector3 center)
        {
            Configure(radius, Color.White, texture, center);
        }

        protected void Configure(float radius, Color color, TGCTexture texture, TGCVector3 center)
        {
            AutoTransformEnable = false;
            Transform = TGCMatrix.Identity;
            translation = center;
            Rotation = TGCVector3.Empty;
            Enabled = true;
            Scale = TGCVector3.One;
            AlphaBlendEnable = false;
            uvOffset = TGCVector2.Zero;

            //BoundingSphere
            BoundingSphere = new TgcBoundingSphere();

            //Shader
            Effect = TGCShaders.Instance.VariosShader;

            //Tipo de vertice y technique
            if (texture != null)
            {
                SetTexture(texture);
            }
            else
            {
                SetColor(color);
            }

            basePoly = eBasePoly.ICOSAHEDRON;
            levelOfDetail = 2;
            inflate = true;
            ForceUpdate = false;
            uvTiling = TGCVector2.One;
        }

        #endregion CONSTRUCTORS

        #region GEOMETRY

        /// <summary>
        ///     Vuelve a crear la esfera si hubo cambio en el nivel de detalle, color o coordenadas de textura o si ForceUpdate
        ///     esta en true.
        /// </summary>
        public virtual void UpdateValues()
        {
            if (!mustUpdate && !ForceUpdate)
            {
                return;
            }

            if (indexBuffer != null && !indexBuffer.Disposed)
            {
                indexBuffer.Dispose();
            }

            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }

            //Obtengo las posiciones de los vertices e indices de la esfera
            List<TGCVector3> positions;

            CreateSphereSubdividingAPolyhedron(out positions, out indices);

            ///////

            vertices = new List<Vertex.PositionColoredTexturedNormal>();

            var iverticesU1 = new List<int>();

            var polos = new int[2];
            var p = 0;

            var c = Color.ToArgb();

            var twoPi = FastMath.TWO_PI;

            //Creo la lista de vertices
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                var u = 0.5f + FastMath.Atan2(pos.Z, pos.X) / twoPi;
                var v = 0.5f - 2 * FastMath.Asin(pos.Y) / twoPi;
                vertices.Add(new Vertex.PositionColoredTexturedNormal(pos, c, UVTiling.X * u + UVOffset.X,
                    UVTiling.Y * v + UVOffset.Y, pos));

                if (u == 1 || IsPolo(vertices[i]))
                {
                    iverticesU1.Add(i);

                    if (u != 1)
                    {
                        try
                        {
                            polos[p++] = i;
                        }
                        catch (Exception e)
                        {
                            //Arreglar esto... y despues quitar el try catch :(
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            //Corrijo los triangulos que tienen mal las coordenadas debido a vertices compartidos
            FixTexcoords(vertices, indices, iverticesU1, polos);

            VertexCount = vertices.Count;
            TriangleCount = indices.Count / 3;

            vertexBuffer = new VertexBuffer(typeof(Vertex.PositionColoredTexturedNormal), VertexCount,
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
        protected void CreateSphereSubdividingAPolyhedron(out List<TGCVector3> vertices, out List<int> indices)
        {
            indices = new List<int>();
            vertices = new List<TGCVector3>();

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
            {
                GeometryProvider.Subdivide(vertices, indices);
            }

            if (Inflate)
            {
                for (var i = 0; i < vertices.Count; i++)
                {
                    vertices[i] = TGCVector3.Normalize(vertices[i]);
                }
            }
        }

        protected bool IsPolo(Vertex.PositionColoredTexturedNormal v)
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
        protected virtual void FixTexcoords(List<Vertex.PositionColoredTexturedNormal> vertices, List<int> indices,
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
                        if ((!IsPolo(vertices[i1]) && vertices[i1].Tu < U0p5) ||
                            (!IsPolo(vertices[i2]) && vertices[i2].Tu < U0p5) ||
                            (!IsPolo(vertices[i3]) && vertices[i3].Tu < U0p5))
                        {
                            var u1 = new List<int>();
                            var um = new List<int>();

                            //Clasifico cada vertice segun su Tu1
                            for (var j = 0; j < 3; j++)
                            {
                                if (vertices[indices[i + j]].Tu == U1 || IsPolo(vertices[indices[i + j]]))
                                {
                                    u1.Add(i + j);
                                }
                                else if (vertices[indices[i + j]].Tu < U0p5)
                                {
                                    um.Add(i + j);
                                }
                            }

                            if (um.Count == 1 && !(IsPolo(vertices[indices[u1[0]]]) && vertices[indices[um[0]]].X >= 0))
                            {
                                //Casos:
                                //2 vertices con u=1 o uno con u=1 y otro con u>0.5
                                //1 vertice es el polo, y uno de los vertices esta al final de la textura y otro al principio

                                //La coordenada textura del de u >0.5 pasa a ser 1+u
                                indices[um[0]] = DupWithU(vertices, indices, duplicados, um[0],
                                    vertices[indices[um[0]]].Tu + UVTiling.X);
                            }
                            else if (!IsPolo(vertices[indices[u1[0]]]))
                                // Caso:
                                // 1 vertice con u=1 y dos con u<0.5

                                // El u del vertice con u=1 pasa a ser 0
                            {
                                indices[u1[0]] = DupWithU(vertices, indices, duplicados, u1[0], UVOffset.X);
                            }
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
                    while (iipolo < i + 3 && indices[iipolo] != polos[p])
                    {
                        iipolo++;
                    }

                    //Si un vertice es el polo
                    if (iipolo < i + 3)
                    {
                        var u = new Vertex.PositionColoredTexturedNormal[2];

                        var n = 0;
                        //Guardo los vertices que no son polos.
                        for (var j = 0; j < 3; j++)
                        {
                            if (i + j != iipolo)
                            {
                                u[n++] = vertices[indices[i + j]];
                            }
                        }

                        var minU = FastMath.Min(u[0].Tu, u[1].Tu);

                        var pole = vertices[polos[p]];

                        //Chequea que no sea un triangulo rectangulo
                        var zeroXZ = u[0];
                        var noRectangulo = false;

                        if (u[0].X != 0 && u[0].Z != 0)
                        {
                            zeroXZ = u[0];
                        }
                        else if (u[1].X != 0 && u[1].Z != 0)
                        {
                            zeroXZ = u[1];
                        }
                        else
                        {
                            noRectangulo = true;
                        }

                        //Interpolo Tu1
                        if (basePoly.Equals(eBasePoly.ICOSAHEDRON) || noRectangulo)

                        {
                            pole.Tu = minU + FastMath.Abs(u[0].Tu - u[1].Tu) / 2;
                        }
                        else
                        {
                            pole.Tu = zeroXZ.Tu;
                        }

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
        protected static int DupWithU(List<Vertex.PositionColoredTexturedNormal> vertices, List<int> indices,
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
            else
            {
                newIndx = dDup[indices[idx]];
            }

            return newIndx;
        }

        #endregion GEOMETRY

        #region RENDER & DISPOSE

        /// <summary>
        ///     Renderizar la esfera
        /// </summary>
        public virtual void Render()
        {
            if (!Enabled)
            {
                return;
            }

            //transformacion
            if (AutoTransformEnable)
            {
                Transform = TGCMatrix.Scaling(radius, radius, radius) *
                            TGCMatrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                            TGCMatrix.Translation(translation);
            }

            //Activar AlphaBlending
            ActivateAlphaBlend();

            //renderizar
            if (Texture != null)
            {
                TexturesManager.Instance.shaderSet(Effect, "texDiffuseMap", Texture);
            }
            else
            {
                TexturesManager.Instance.clear(0);
            }

            TexturesManager.Instance.clear(1);

            TGCShaders.Instance.SetShaderMatrix(Effect, Transform);
            Effect.Technique = Technique;

            D3DDevice.Instance.Device.VertexDeclaration = Vertex.PositionColoredTexturedNormal_Declaration;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);
            var oldIndex = D3DDevice.Instance.Device.Indices;
            D3DDevice.Instance.Device.Indices = indexBuffer;

            //Render con shader
            RenderWithFill(D3DDevice.Instance.Device.RenderState.FillMode);

            if (RenderEdges)
            {
                if (Texture == null)
                {
                    Effect.Technique = TGCShaders.T_POSITION_TEXTURED;
                }
                else
                {
                    Effect.Technique = TGCShaders.T_POSITION_COLORED;
                }

                RenderWithFill(FillMode.WireFrame);
            }

            //Desactivar AlphaBlend
            ResetAlphaBlend();

            D3DDevice.Instance.Device.Indices = oldIndex;
        }

        protected void RenderWithFill(FillMode fillmode)
        {
            var old = D3DDevice.Instance.Device.RenderState.FillMode;
            D3DDevice.Instance.Device.RenderState.FillMode = fillmode;

            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0,
                TriangleCount);
            Effect.EndPass();
            Effect.End();
            D3DDevice.Instance.Device.RenderState.FillMode = old;
        }

        /// <summary>
        ///     Activar AlphaBlending, si corresponde
        /// </summary>
        protected void ActivateAlphaBlend()
        {
            if (AlphaBlendEnable)
            {
                D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            }
        }

        /// <summary>
        ///     Desactivar AlphaBlending
        /// </summary>
        protected void ResetAlphaBlend()
        {
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Liberar los recursos de la esfera
        /// </summary>
        public virtual void Dispose()
        {
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }

            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }

            if (indexBuffer != null && !indexBuffer.Disposed)
            {
                indexBuffer.Dispose();
            }

            BoundingSphere.Dispose();
        }

        #endregion RENDER & DISPOSE

        #region TRANSFORMATIONS

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        public void Move(TGCVector3 v)
        {
            Move(v.X, v.Y, v.Z);
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        public void Move(float x, float y, float z)
        {
            translation.X += x;
            translation.Y += y;
            translation.Z += z;

            UpdateBoundingVolume();
        }

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        [Obsolete]
        public void MoveOrientedY(float movement)
        {
            var z = (float)Math.Cos(Rotation.Y) * movement;
            var x = (float)Math.Sin(Rotation.Y) * movement;

            Move(x, 0, z);
        }

        /// <summary>
        ///     Obtiene la posicion absoluta de la malla, recibiendo un vector ya creado para
        ///     almacenar el resultado
        /// </summary>
        /// <param name="pos">Vector ya creado en el que se carga el resultado</param>
        public void GetPosition(TGCVector3 pos)
        {
            pos.X = translation.X;
            pos.Y = translation.Y;
            pos.Z = translation.Z;
        }

        /// <summary>
        ///     Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateX(float angle)
        {
            rotation.X += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateY(float angle)
        {
            rotation.Y += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateZ(float angle)
        {
            rotation.Z += angle;
        }

        /// <summary>
        ///     Actualiza el BoundingSphere de la esfera
        /// </summary>
        protected virtual void UpdateBoundingVolume()
        {
            BoundingSphere.setValues(Position, Radius);
        }

        #endregion TRANSFORMATIONS
    }
}
