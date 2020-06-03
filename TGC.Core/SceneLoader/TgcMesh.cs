using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.SceneLoader
{
    /// <summary>
    ///     Representa una malla estática.
    ///     Puede moverse y rotarse pero no tiene animación.
    ///     La malla puede tener colores por vértice, texturas y lightmaps.
    ///     Puede crearse como una malla nueva o como una instancia de otra existente y reutilizar así su geometría.
    /// </summary>
    public class TgcMesh : IRenderObject, ITransformObject
    {
        /// <summary>
        ///     Tipos de de renderizado de mallas
        /// </summary>
        public enum MeshRenderType
        {
            /// <summary>
            ///     Solo colores por vertice
            /// </summary>
            VERTEX_COLOR,

            /// <summary>
            ///     Solo un canal de textura en DiffuseMap
            /// </summary>
            DIFFUSE_MAP,

            /// <summary>
            ///     Un canal de textura en DiffuseMap y otro para Lightmap,
            ///     utilizando Multitexture
            /// </summary>
            DIFFUSE_MAP_AND_LIGHTMAP
        }

        protected TgcBoundingAxisAlignBox boundingBox;
        protected Mesh d3dMesh;

        protected TgcTexture[] diffuseMaps;

        protected bool enabled;

        protected string layer;

        protected TgcTexture lightMap;

        protected Material[] materials;

        protected List<TgcMesh> meshInstances;

        protected string name;

        protected TgcMesh parentInstance;

        protected MeshRenderType renderType;

        protected TGCVector3 rotation;

        protected TGCVector3 scale;

        protected TGCMatrix transform;

        protected TGCVector3 translation;

        protected VertexDeclaration vertexDeclaration;

        /// <summary>
        ///     Constructor vacio, para facilitar la herencia de esta clase.
        /// </summary>
        protected TgcMesh()
        {
        }

        /// <summary>
        ///     Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        public TgcMesh(Mesh mesh, string name, MeshRenderType renderType)
        {
            initData(mesh, name, renderType);
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de otra malla original.
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="parentInstance">Malla original desde la cual basarse</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        [Obsolete]
        public TgcMesh(string name, TgcMesh parentInstance, TGCVector3 translation, TGCVector3 rotation, TGCVector3 scale)
        {
            //Cargar datos en base al original
            initData(parentInstance.d3dMesh, name, parentInstance.renderType);
            //Si no se hace clone luego no pueden cambiar las texturas.
            diffuseMaps = new TgcTexture[parentInstance.diffuseMaps.Length];
            for (var i = 0; i < diffuseMaps.Length; i++)
            {
                diffuseMaps[i] = parentInstance.diffuseMaps[i].Clone();
            }
            materials = parentInstance.materials; //es un clone necesario?
            lightMap = parentInstance.lightMap; //es un clone necesario?
            Effect = parentInstance.Effect; //.Clone() pide device ????...... FIXIT.

            //Almacenar transformación inicial
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            //almacenar instancia en el padre
            this.parentInstance = parentInstance;
            parentInstance.meshInstances.Add(this);
        }

        /// <summary>
        ///     Mesh interna de DirectX
        /// </summary>
        public Mesh D3dMesh
        {
            get { return d3dMesh; }
        }

        /// <summary>
        ///     Nombre de la malla
        /// </summary>
        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        /// <summary>
        ///     Layer al que pertenece la malla.
        /// </summary>
        public string Layer
        {
            set { layer = value; }
            get { return layer; }
        }

        /// <summary>
        ///     User properties de la malla
        /// </summary>
        public Dictionary<string, string> UserProperties { set; get; }

        /// <summary>
        ///     Array de Materials
        /// </summary>
        public Material[] Materials
        {
            get { return materials; }
            set { materials = value; }
        }

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
        ///     Array de texturas para DiffuseMap
        /// </summary>
        public TgcTexture[] DiffuseMaps
        {
            get { return diffuseMaps; }
            set { diffuseMaps = value; }
        }

        /// <summary>
        ///     Textura de LightMap
        /// </summary>
        public TgcTexture LightMap
        {
            get { return lightMap; }
            set { lightMap = value; }
        }

        /// <summary>
        ///     Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        ///     BoundingBox del Mesh
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }

        /// <summary>
        ///     Tipo de formato de Render de esta malla
        /// </summary>
        public MeshRenderType RenderType
        {
            get { return renderType; }
            set { renderType = value; }
        }

        /// <summary>
        ///     VertexDeclaration del Flexible Vertex Format (FVF) usado por la malla
        /// </summary>
        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        /// <summary>
        ///     Indica si se actualiza automaticamente el BoundingBox con cada movimiento de la malla
        /// </summary>
        public bool AutoUpdateBoundingBox { get; set; }

        /// <summary>
        ///     Original desde el cual esta malla fue clonada.
        /// </summary>
        public TgcMesh ParentInstance
        {
            get { return parentInstance; }
        }

        /// <summary>
        ///     Lista de mallas que fueron clonadas a partir de este original
        /// </summary>
        public List<TgcMesh> MeshInstances
        {
            get { return meshInstances; }
        }

        /// <summary>
        ///     Cantidad de triángulos de la malla
        /// </summary>
        public int NumberTriangles
        {
            get { return d3dMesh.NumberFaces; }
        }

        /// <summary>
        ///     Cantidad de vértices de la malla
        /// </summary>
        public int NumberVertices
        {
            get { return d3dMesh.NumberVertices; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderiza la malla, si esta habilitada
        /// </summary>
        public void Render()
        {
            if (!enabled)
                return;

            //Aplicar transformacion de malla
            if (AutoTransformEnable)
            {
                UpdateMeshTransform();
            }

            //Cargar VertexDeclaration
            D3DDevice.Instance.Device.VertexDeclaration = vertexDeclaration;

            //Activar AlphaBlending si corresponde
            activateAlphaBlend();

            //Cargar matrices para el shader
            setShaderMatrix();

            //Renderizar segun el tipo de Render de la malla
            Effect.Technique = Technique;
            var numPasses = Effect.Begin(0);
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:

                    //Hacer reset de texturas
                    TexturesManager.Instance.clear(0);
                    TexturesManager.Instance.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (var n = 0; n < numPasses; n++)
                    {
                        //Iniciar pasada de shader
                        Effect.BeginPass(n);
                        d3dMesh.DrawSubset(0);
                        Effect.EndPass();
                    }

                    break;

                case MeshRenderType.DIFFUSE_MAP:

                    //Hacer reset de Lightmap
                    TexturesManager.Instance.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (var n = 0; n < numPasses; n++)
                    {
                        //Dibujar cada subset con su DiffuseMap correspondiente
                        for (var i = 0; i < diffuseMaps.Length; i++)
                        {
                            //Setear textura en shader
                            TexturesManager.Instance.shaderSet(Effect, "texDiffuseMap", diffuseMaps[i]);

                            //Iniciar pasada de shader
                            Effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            Effect.EndPass();
                        }
                    }

                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (var n = 0; n < numPasses; n++)
                    {
                        //Cargar lightmap
                        TexturesManager.Instance.shaderSet(Effect, "texLightMap", lightMap);

                        //Dibujar cada subset con su Material y su DiffuseMap correspondiente
                        for (var i = 0; i < diffuseMaps.Length; i++)
                        {
                            //Setear textura en shader
                            TexturesManager.Instance.shaderSet(Effect, "texDiffuseMap", diffuseMaps[i]);

                            //Iniciar pasada de shader
                            Effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            Effect.EndPass();
                        }
                    }

                    break;
            }

            //Finalizar shader
            Effect.End();

            //Desactivar alphaBlend
            resetAlphaBlend();
        }

        /// <summary>
        ///     Libera los recursos de la malla.
        ///     Si la malla es una instancia se deshabilita pero no se liberan recursos.
        ///     Si la malla es el original y tiene varias instancias adjuntadas, se hace dispose() también de las instancias.
        /// </summary>
        public void Dispose()
        {
            enabled = false;
            if (boundingBox != null)
            {
                boundingBox.Dispose();
            }

            //Si es una instancia no liberar nada, lo hace el original.
            if (parentInstance != null)
            {
                parentInstance = null;
                return;
            }

            //hacer dispose de instancias
            if (meshInstances != null)
            {
                foreach (var meshInstance in meshInstances)
                {
                    meshInstance.Dispose();
                }
                meshInstances = null;
            }

            //Dispose de mesh
            d3dMesh.Dispose();
            d3dMesh = null;

            //Dispose de texturas
            if (diffuseMaps != null)
            {
                for (var i = 0; i < diffuseMaps.Length; i++)
                {
                    if (diffuseMaps[i] != null)
                    {
                        diffuseMaps[i].dispose();
                    }
                }
                diffuseMaps = null;
            }
            if (lightMap != null)
            {
                lightMap.dispose();
                lightMap = null;
            }

            //VertexDeclaration
            vertexDeclaration.Dispose();
            vertexDeclaration = null;
        }

        /// <summary>
        ///     Matriz final que se utiliza para aplicar transformaciones a la malla.
        ///     Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        ///     en base a los valores de: Position, Rotation, Scale.
        ///     Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        public TGCMatrix Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en True.
        /// </summary>
        [Obsolete("Utilizar esta propiedad en juegos complejos se pierde el control, es mejor utilizar transformaciones con matrices.")]
        public bool AutoTransformEnable { get; set; }

        /// <summary>
        ///     Posicion absoluta de la Malla
        /// </summary>
        public TGCVector3 Position
        {
            get { return translation; }
            set
            {
                translation = value;
                updateBoundingBox();
            }
        }

        /// <summary>
        ///     Rotación absoluta de la malla
        /// </summary>
        public TGCVector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        ///     Escalado absoluto de la malla;
        /// </summary>
        public TGCVector3 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                updateBoundingBox();
            }
        }

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

            updateBoundingBox();
        }

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        [Obsolete]
        public void MoveOrientedY(float movement)
        {
            var z = FastMath.Cos(rotation.Y) * movement;
            var x = FastMath.Sin(rotation.Y) * movement;

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
        ///     Cargar datos iniciales
        /// </summary>
        protected void initData(Mesh mesh, string name, MeshRenderType renderType)
        {
            d3dMesh = mesh;
            this.name = name;
            this.renderType = renderType;
            enabled = false;
            parentInstance = null;
            meshInstances = new List<TgcMesh>();
            AlphaBlendEnable = false;

            AutoTransformEnable = false;
            AutoUpdateBoundingBox = true;
            translation = TGCVector3.Empty;
            rotation = TGCVector3.Empty;
            scale = TGCVector3.One;
            transform = TGCMatrix.Identity;

            //Shader
            vertexDeclaration = new VertexDeclaration(mesh.Device, mesh.Declaration);
            Effect = TGCShaders.Instance.TgcMeshShader;
            Technique = TGCShaders.Instance.GetTGCMeshTechnique(this.renderType);
        }

        /// <summary>
        ///     Cargar todas la matrices que necesita el shader
        /// </summary>
        protected void setShaderMatrix()
        {
            TGCShaders.Instance.SetShaderMatrix(Effect, transform);
        }

        /// <summary>
        ///     Actualiza la matriz de transformacion con los datos internos del mesh (scale. rotation, traslation) para casos complejos es mejor no utilizar este metodo.
        /// </summary>
        public void UpdateMeshTransform()
        {
            transform = TGCMatrix.Scaling(scale)
                            * TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                            * TGCMatrix.Translation(translation);
        }

        /// <summary>
        ///     Activar AlphaBlending, si corresponde
        /// </summary>
        protected void activateAlphaBlend()
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
        protected void resetAlphaBlend()
        {
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Devuelve un array con todas las posiciones de los vértices de la malla
        /// </summary>
        /// <returns>Array creado</returns>
        public TGCVector3[] getVertexPositions()
        {
            TGCVector3[] points = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var verts1 = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new TGCVector3[verts1.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts1[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new TGCVector3[verts2.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts2[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    var verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new TGCVector3[verts3.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts3[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return points;
        }

        /// <summary>
        ///     Devuelve un array con todas las coordenadas de textura de la malla
        ///     Solo puede hacerse para meshes del tipo DIFFUSE_MAP y DIFFUSE_MAP_AND_LIGHTMAP.
        /// </summary>
        /// <returns>Array creado</returns>
        public TGCVector2[] getTextureCoordinates()
        {
            TGCVector2[] uvCoords = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    throw new Exception("No se puede obtener coordenadas de UV en un mesh del tipo VERTEX_COLOR");

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    uvCoords = new TGCVector2[verts2.Length];
                    for (var i = 0; i < uvCoords.Length; i++)
                    {
                        uvCoords[i] = new TGCVector2(verts2[i].Tu, verts2[i].Tv);
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    var verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    uvCoords = new TGCVector2[verts3.Length];
                    for (var i = 0; i < uvCoords.Length; i++)
                    {
                        uvCoords[i] = new TGCVector2(verts3[i].Tu0, verts3[i].Tv0);
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return uvCoords;
        }

        /// <summary>
        ///     Calcula el BoundingBox de la malla, en base a todos sus vertices.
        ///     Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public TgcBoundingAxisAlignBox createBoundingBox()
        {
            if (boundingBox != null)
            {
                boundingBox.Dispose();
                boundingBox = null;
            }
            //Obtener vertices en base al tipo de malla
            var points = getVertexPositions();
            boundingBox = TgcBoundingAxisAlignBox.computeFromPoints(points);
            return boundingBox;
        }

        /// <summary>
        ///     Actualiza el BoundingBox de la malla, en base a su posicion actual.
        ///     Solo contempla traslacion y escalado
        /// </summary>
        public void updateBoundingBox()
        {
            if (AutoUpdateBoundingBox)
            {
                boundingBox.scaleTranslate(translation, scale);
            }
        }

        /// <summary>
        ///     Cambia el color de todos los vértices de la malla.
        ///     Esta operacion tiene que hacer un lock del VertexBuffer y es poco performante.
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public void setColor(Color color)
        {
            var c = color.ToArgb();
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var verts1 = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (var i = 0; i < verts1.Length; i++)
                    {
                        verts1[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts1, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (var i = 0; i < verts2.Length; i++)
                    {
                        verts2[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts2, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    var verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (var i = 0; i < verts3.Length; i++)
                    {
                        verts3[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts3, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
        }

        /// <summary>
        ///     Permite cambiar las texturas de DiffuseMap de esta malla
        /// </summary>
        /// <param name="newDiffuseMaps">Array de nuevas texturas. Tiene que tener la misma cantidad que el original</param>
        public void changeDiffuseMaps(TgcTexture[] newDiffuseMaps)
        {
            //Solo aplicar si la malla tiene texturas
            if (renderType == MeshRenderType.DIFFUSE_MAP || renderType == MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP)
            {
                if (diffuseMaps.Length != newDiffuseMaps.Length)
                {
                    throw new Exception("The new DiffuseMap array does not have the same length than the original.");
                }

                //Liberar texturas anteriores
                foreach (var t in diffuseMaps)
                {
                    t.dispose();
                }

                //Asignar nuevas texturas
                diffuseMaps = newDiffuseMaps;
            }
        }

        /// <summary>
        ///     Agregar una nueva textura a la lista de texturas que tiene el mesh.
        ///     Esta nueva textura no va a ser utilizada por ningún triángulo si no se
        ///     adapta correctamente el attributeBuffer.
        ///     No se controla si esa textura ya esta repetida en el mesh.
        /// </summary>
        /// <param name="newDiffuseMap">Nueva textura</param>
        public void addDiffuseMap(TgcTexture newDiffuseMap)
        {
            //Solo aplicar si la malla tiene texturas
            if (renderType == MeshRenderType.DIFFUSE_MAP || renderType == MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP)
            {
                var newDiffuseMapsArray = new TgcTexture[diffuseMaps.Length + 1];
                Array.Copy(diffuseMaps, newDiffuseMapsArray, diffuseMaps.Length);
                newDiffuseMapsArray[newDiffuseMapsArray.Length - 1] = newDiffuseMap;
                diffuseMaps = newDiffuseMapsArray;
            }
        }

        /// <summary>
        ///     Eliminar un slot de textura del mesh.
        ///     Se modifica el attributeBuffer para que todos los triangulos que
        ///     apuntaban a esta textura ahora apunten a replacementSlot
        /// </summary>
        /// <param name="diffuseMapSlot">Slot de textura a eliminar</param>
        /// <param name="replacementSlot">Nuevo slot al que apuntan los triangulos que usaban el anterior</param>
        public void deleteDiffuseMap(int diffuseMapSlot, int replacementSlot)
        {
            //Solo aplicar si la malla tiene texturas
            if (renderType == MeshRenderType.DIFFUSE_MAP || renderType == MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP)
            {
                if (diffuseMapSlot < 0 || diffuseMapSlot >= diffuseMaps.Length)
                {
                    throw new Exception("Incorrect diffuseMap slot: " + diffuseMapSlot + ". Total: " +
                                        diffuseMaps.Length);
                }
                if (replacementSlot < 0 || replacementSlot >= diffuseMaps.Length - 1)
                {
                    replacementSlot = 0;
                }

                //Crear nuevo array sin la textura que se quiere eliminar
                var newDiffuseMapsArray = new TgcTexture[diffuseMaps.Length - 1];
                var w = 0;
                for (var i = 0; i < diffuseMaps.Length; i++)
                {
                    if (i != diffuseMapSlot)
                    {
                        newDiffuseMapsArray[w++] = diffuseMaps[i];
                    }
                }
                diffuseMaps = newDiffuseMapsArray;

                //Modificar attributeBuffer. Hay que reemplazar el id que se elimino y hacer shift de todo lo que estaba a la derecha
                var attributeBuffer = d3dMesh.LockAttributeBufferArray(LockFlags.None);
                for (var i = 0; i < attributeBuffer.Length; i++)
                {
                    if (attributeBuffer[i] == diffuseMapSlot)
                    {
                        attributeBuffer[i] = replacementSlot;
                    }
                    else if (attributeBuffer[i] > diffuseMapSlot)
                    {
                        attributeBuffer[i]--;
                    }
                }
                d3dMesh.UnlockAttributeBuffer(attributeBuffer);
            }
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de esta malla original
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Solo se puede crear instancias a partir de originales.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcMesh createMeshInstance(string name, TGCVector3 translation, TGCVector3 rotation, TGCVector3 scale)
        {
            if (parentInstance != null)
            {
                throw new Exception(
                    "No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            var instance = new TgcMesh(name, this, translation, rotation, scale);

            //BoundingBox
            instance.boundingBox = new TgcBoundingAxisAlignBox(boundingBox.PMin, boundingBox.PMax);
            instance.updateBoundingBox();

            instance.enabled = true;
            return instance;
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de esta malla original
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Solo se puede crear instancias a partir de originales.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        public TgcMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, TGCVector3.Empty, TGCVector3.Empty, TGCVector3.One);
        }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString()
        {
            return "Mesh: " + name;
        }

        /// <summary>
        ///     Crear un nuevo mesh igual
        /// </summary>
        /// <param name="cloneName">Nombre del mesh clonado</param>
        /// <returns>Mesh clonado</returns>
        public TgcMesh clone(string cloneName)
        {
            //Clonar D3dMesh
            var d3dCloneMesh = d3dMesh.Clone(MeshFlags.Managed, d3dMesh.Declaration, D3DDevice.Instance.Device);

            //Crear mesh de TGC y cargar atributos generales
            var cloneMesh = new TgcMesh(d3dCloneMesh, cloneName, renderType);
            cloneMesh.Materials = Materials;
            cloneMesh.layer = layer;
            cloneMesh.boundingBox = boundingBox.clone();
            cloneMesh.AlphaBlendEnable = AlphaBlendEnable;
            cloneMesh.enabled = true;
            cloneMesh.AutoUpdateBoundingBox = AutoUpdateBoundingBox;

            //Transformaciones
            cloneMesh.translation = translation;
            cloneMesh.rotation = rotation;
            cloneMesh.scale = scale;
            cloneMesh.transform = transform;
            cloneMesh.AutoTransformEnable = AutoTransformEnable;

            //Clonar userProperties
            if (UserProperties != null)
            {
                cloneMesh.UserProperties = new Dictionary<string, string>();
                foreach (var entry in UserProperties)
                {
                    cloneMesh.UserProperties.Add(entry.Key, entry.Value);
                }
            }

            //Clonar DiffuseMaps
            if (diffuseMaps != null)
            {
                cloneMesh.diffuseMaps = new TgcTexture[diffuseMaps.Length];
                for (var i = 0; i < diffuseMaps.Length; i++)
                {
                    cloneMesh.diffuseMaps[i] = diffuseMaps[i].Clone();
                }
            }

            //Clonar LightMap
            if (lightMap != null)
            {
                cloneMesh.lightMap = lightMap.Clone();
            }

            return cloneMesh;
        }

        /// <summary>
        ///     Cambiar el mesh interno de DirectX por uno nuevo.
        ///     Se asume que el nuevo mesh es del mismo RenderType que el anterior.
        /// </summary>
        /// <param name="newD3dMesh">Nuevo mesh</param>
        public void changeD3dMesh(Mesh newD3dMesh)
        {
            vertexDeclaration = new VertexDeclaration(newD3dMesh.Device, newD3dMesh.Declaration);
            d3dMesh.Dispose();
            d3dMesh = newD3dMesh;
        }

        /// <summary>
        ///     Acceder al VertexBuffer del mesh.
        ///     Una vez que se termina de trabajar con el buffer se debe invocar siempre a unlock.
        /// </summary>
        /// <param name="lockFlags">Flags de lectura del buffer</param>
        /// <returns>array de elementos</returns>
        public Array lockVertexBuffer(LockFlags lockFlags)
        {
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    return (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), lockFlags, d3dMesh.NumberVertices);

                case MeshRenderType.DIFFUSE_MAP:
                    return (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), lockFlags, d3dMesh.NumberVertices);

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    return (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), lockFlags, d3dMesh.NumberVertices);
            }
            return null;
        }

        public static TgcMesh FromTGCBox(string meshName, TgcTexture texture, CustomVertex.PositionColoredTextured[] vertices, TGCMatrix transform, bool alphaBlendEnable)
        {
            TgcMesh tgcMesh;

            if (texture != null)
            {
                //Crear mesh con DiffuseMap
                tgcMesh = TgcMesh.ToDiffuseMapMesh(meshName, vertices, transform, texture, alphaBlendEnable);
            }
            else
            {
                //Crear mesh con solo color
                tgcMesh = TgcMesh.ToColorMesh(meshName, vertices, transform);
            }

            return tgcMesh;
        }

        /// <summary>
        /// Crea un TGCMesh con solo color.
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear.</param>
        /// <param name="vertices">Vertices a utilizar para crear la malla.</param>
        /// <param name="transform">Matriz a utilizar para aplicar transformaciones a la malla.</param>
        /// <returns>Un nuevo TGCMesh basado en los parametros dados.</returns>
        public static TgcMesh ToColorMesh(string meshName, CustomVertex.PositionColoredTextured[] vertices, TGCMatrix transform)
        {
            //Crear Mesh
            var d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed,
                TgcSceneLoader.VertexColorVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < vertices.Length; j++)
                {
                    var v = new TgcSceneLoader.VertexColorVertex();
                    var vBox = vertices[j];

                    //vertices
                    v.Position = TGCVector3.TransformCoordinate(new TGCVector3(vBox.Position), transform);

                    //normals
                    v.Normal = TGCVector3.Empty;

                    //color
                    v.Color = vBox.Color;

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (var ib = d3dMesh.IndexBuffer)
            {
                var indices = new short[vertices.Length];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Malla de TGC
            var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.VERTEX_COLOR);
            tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }

        /// <summary>
        /// Crea un TGCMesh con DiffuseMap.
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear.</param>
        /// <param name="vertices">Vertices a utilizar para crear la malla.</param>
        /// <param name="transform">Matriz que se utiliza para aplicar transformaciones a la malla.</param>
        /// <param name="texture">Textura a utilizar.</param>
        /// <param name="alphaBlendEnable">Habilita el AlphaBlending para los modelos.</param>
        /// <returns>Un nuevo TGCMesh basado en los parametros dados.</returns>
        public static TgcMesh ToDiffuseMapMesh(string meshName, CustomVertex.PositionColoredTextured[] vertices, TGCMatrix transform, TgcTexture texture, bool alphaBlendEnable)
        {
            //Crear Mesh
            var d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed,
                TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < vertices.Length; j++)
                {
                    var v = new TgcSceneLoader.DiffuseMapVertex();
                    var vBox = vertices[j];

                    //vertices
                    v.Position = TGCVector3.TransformCoordinate(new TGCVector3(vBox.Position), transform);

                    //normals
                    v.Normal = TGCVector3.Empty;

                    //texture coordinates diffuseMap
                    v.Tu = vBox.Tu;
                    v.Tv = vBox.Tv;

                    //color
                    v.Color = vBox.Color;

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (var ib = d3dMesh.IndexBuffer)
            {
                var indices = new short[vertices.Length];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Calcular normales
            d3dMesh.ComputeNormals();

            //Malla de TGC
            var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.DiffuseMaps = new[] { texture.Clone() };
            tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            tgcMesh.AlphaBlendEnable = alphaBlendEnable;
            return tgcMesh;
        }

        public static TgcMesh FromTGCSphere(string meshName, TgcTexture texture, List<int> indices, List<TGCSphere.Vertex.PositionColoredTexturedNormal> vertices, TGCMatrix transform, bool alphaBlendEnable)
        {
            //Crear mesh con DiffuseMap
            if (texture != null)
            {
                //Crear Mesh
                var d3dMesh = new Mesh(indices.Count / 3, indices.Count, MeshFlags.Managed,
                    TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

                //Cargar VertexBuffer
                using (var vb = d3dMesh.VertexBuffer)
                {
                    var data = vb.Lock(0, 0, LockFlags.None);
                    for (var j = 0; j < indices.Count; j++)
                    {
                        var v = new TgcSceneLoader.DiffuseMapVertex();
                        var vSphere = vertices[indices[j]];

                        //vertices
                        v.Position = TGCVector3.TransformCoordinate(vSphere.getPosition(), transform);

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
                tgcMesh.DiffuseMaps = new[] { texture.Clone() };
                tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                return tgcMesh;
            }

            //Crear mesh con solo color
            else
            {
                //Crear Mesh
                var d3dMesh = new Mesh(indices.Count / 3, indices.Count, MeshFlags.Managed,
                    TgcSceneLoader.VertexColorVertexElements, D3DDevice.Instance.Device);

                //Cargar VertexBuffer
                using (var vb = d3dMesh.VertexBuffer)
                {
                    var data = vb.Lock(0, 0, LockFlags.None);
                    for (var j = 0; j < indices.Count; j++)
                    {
                        var v = new TgcSceneLoader.VertexColorVertex();
                        var vSphere = vertices[indices[j]];

                        //vertices
                        v.Position = TGCVector3.TransformCoordinate(vSphere.getPosition(), transform);

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
                tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                tgcMesh.AlphaBlendEnable = alphaBlendEnable;
                return tgcMesh;
            }
        }

        public static TgcMesh FromTGCPlane(string meshName, CustomVertex.PositionTextured[] vertices, TgcTexture texture, TGCVector3? Normal)
        {
            //Crear Mesh
            var d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed, TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                var ceroNormal = TGCVector3.Empty;
                var whiteColor = Color.White.ToArgb();
                for (var j = 0; j < vertices.Length; j++)
                {
                    var v = new TgcSceneLoader.DiffuseMapVertex();
                    var vPlane = vertices[j];

                    //vertices
                    v.Position = new TGCVector3(vPlane.Position);

                    //normals
                    v.Normal = (Normal ?? TGCVector3.Empty);

                    //texture coordinates diffuseMap
                    v.Tu = vPlane.Tu;
                    v.Tv = vPlane.Tv;

                    //color
                    v.Color = whiteColor;

                    data.Write(v);
                }
                vb.Unlock();
            }

            //Cargar IndexBuffer en forma plana
            using (var ib = d3dMesh.IndexBuffer)
            {
                var indices = new short[vertices.Length];
                for (var j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Calcular normales
            if (Normal == null)
            {
                d3dMesh.ComputeNormals();
            }

            //Malla de TGC
            var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.DiffuseMaps = new[] { texture.Clone() };
            tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }
    }
}