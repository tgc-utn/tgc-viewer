using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;

namespace TgcViewer.Utils.TgcSceneLoader
{
    /// <summary>
    /// Representa una malla estática.
    /// Puede moverse y rotarse pero no tiene animación.
    /// La malla puede tener colores por vértice, texturas y lightmaps.
    /// Puede crearse como una malla nueva o como una instancia de otra existente y reutilizar así su geometría.
    /// </summary>
    public class TgcMesh : IRenderObject, ITransformObject
    {
        protected Mesh d3dMesh;
        /// <summary>
        /// Mesh interna de DirectX
        /// </summary>
        public Mesh D3dMesh
        {
            get { return d3dMesh; }
        }

        protected string name;
        /// <summary>
        /// Nombre de la malla
        /// </summary>
        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        protected string layer;
        /// <summary>
        /// Layer al que pertenece la malla.
        /// </summary>
        public string Layer
        {
            set { layer = value; }
            get { return layer; }
        }

        private Dictionary<string, string> userProperties;
        /// <summary>
        /// User properties de la malla
        /// </summary>
        public Dictionary<string, string> UserProperties
        {
            set { userProperties = value; }
            get { return userProperties; }
        }

        protected Material[] materials;
        /// <summary>
        /// Array de Materials
        /// </summary>
        public Material[] Materials
        {
            get { return materials; }
            set { materials = value; }
        }

        protected Effect effect;
        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        protected string technique;
        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        protected TgcTexture[] diffuseMaps;
        /// <summary>
        /// Array de texturas para DiffuseMap
        /// </summary>
        public TgcTexture[] DiffuseMaps
        {
            get { return diffuseMaps; }
            set { diffuseMaps = value; }
        }

        protected TgcTexture lightMap = null;
        /// <summary>
        /// Textura de LightMap
        /// </summary>
        public TgcTexture LightMap
        {
            get { return lightMap; }
            set { lightMap = value; }
        }

        protected bool enabled;
        /// <summary>
        /// Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }


        protected Matrix transform;
        /// <summary>
        /// Matriz final que se utiliza para aplicar transformaciones a la malla.
        /// Si la propiedad AutoTransformEnable esta en True, la matriz se reconstruye en cada cuadro
        /// en base a los valores de: Position, Rotation, Scale.
        /// Si AutoTransformEnable está en False, se respeta el valor que el usuario haya cargado en la matriz.
        /// </summary>
        public Matrix Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        protected bool autoTransformEnable;
        /// <summary>
        /// En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        /// cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        /// En False se respeta lo que el usuario haya cargado a mano en la matriz.
        /// Por default está en True.
        /// </summary>
        public bool AutoTransformEnable
        {
            get { return autoTransformEnable; }
            set { autoTransformEnable = value; }
        }


        protected Vector3 translation;
        /// <summary>
        /// Posicion absoluta de la Malla
        /// </summary>
        public Vector3 Position
        {
            get { return translation; }
            set
            {
                translation = value;
                updateBoundingBox();
            }
        }

        protected Vector3 rotation;
        /// <summary>
        /// Rotación absoluta de la malla
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        protected Vector3 scale;
        /// <summary>
        /// Escalado absoluto de la malla;
        /// </summary>
        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                updateBoundingBox();
            }
        }


        /// <summary>
        /// Tipos de de renderizado de mallas
        /// </summary>
        public enum MeshRenderType
        {
            /// <summary>
            /// Solo colores por vertice
            /// </summary>
            VERTEX_COLOR,
            /// <summary>
            /// Solo un canal de textura en DiffuseMap
            /// </summary>
            DIFFUSE_MAP,
            /// <summary>
            /// Un canal de textura en DiffuseMap y otro para Lightmap,
            /// utilizando Multitexture
            /// </summary>
            DIFFUSE_MAP_AND_LIGHTMAP,
        };

        protected TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox del Mesh
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }


        protected MeshRenderType renderType;
        /// <summary>
        /// Tipo de formato de Render de esta malla
        /// </summary>
        public MeshRenderType RenderType
        {
            get { return renderType; }
            set { renderType = value; }
        }

        protected VertexDeclaration vertexDeclaration;
        /// <summary>
        /// VertexDeclaration del Flexible Vertex Format (FVF) usado por la malla
        /// </summary>
        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        private bool autoUpdateBoundingBox;
        /// <summary>
        /// Indica si se actualiza automaticamente el BoundingBox con cada movimiento de la malla
        /// </summary>
        public bool AutoUpdateBoundingBox
        {
            get { return autoUpdateBoundingBox; }
            set { autoUpdateBoundingBox = value; }
        }

        protected TgcMesh parentInstance;
        /// <summary>
        /// Original desde el cual esta malla fue clonada.
        /// </summary>
        public TgcMesh ParentInstance
        {
            get { return parentInstance; }
        }

        protected List<TgcMesh> meshInstances;
        /// <summary>
        /// Lista de mallas que fueron clonadas a partir de este original
        /// </summary>
        public List<TgcMesh> MeshInstances
        {
            get { return meshInstances; }
        }

        /// <summary>
        /// Cantidad de triángulos de la malla
        /// </summary>
        public int NumberTriangles
        {
            get { return d3dMesh.NumberFaces; }
        }

        /// <summary>
        /// Cantidad de vértices de la malla
        /// </summary>
        public int NumberVertices
        {
            get { return d3dMesh.NumberVertices; }
        }

        protected bool alphaBlendEnable;
        /// <summary>
        /// Habilita el renderizado con AlphaBlending para los modelos
        /// con textura o colores por vértice de canal Alpha.
        /// Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        /// <summary>
        /// Constructor vacio, para facilitar la herencia de esta clase.
        /// </summary>
        protected TgcMesh()
        {
        }

        /// <summary>
        /// Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        public TgcMesh(Mesh mesh, string name, MeshRenderType renderType)
        {
            this.initData(mesh, name, renderType);
        }

        /// <summary>
        /// Crea una nueva malla que es una instancia de otra malla original.
        /// Reutiliza toda la geometría de la malla original sin duplicarla.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="parentInstance">Malla original desde la cual basarse</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcMesh(string name, TgcMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            //Cargar datos en base al original
            this.initData(parentInstance.d3dMesh, name, parentInstance.renderType);
            this.diffuseMaps = parentInstance.diffuseMaps;
            this.materials = parentInstance.materials;
            this.lightMap = parentInstance.lightMap;
            this.effect = parentInstance.effect;

            //Almacenar transformación inicial
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            //almacenar instancia en el padre
            this.parentInstance = parentInstance;
            parentInstance.meshInstances.Add(this);
        }

        /// <summary>
        /// Cargar datos iniciales
        /// </summary>
        protected void initData(Mesh mesh, string name, MeshRenderType renderType)
        {
            this.d3dMesh = mesh;
            this.name = name;
            this.renderType = renderType;
            this.enabled = false;
            this.parentInstance = null;
            this.meshInstances = new List<TgcMesh>();
            this.alphaBlendEnable = false;

            this.autoTransformEnable = true;
            this.autoUpdateBoundingBox = true;
            this.translation = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector3(0f, 0f, 0f);
            this.scale = new Vector3(1f, 1f, 1f);
            this.transform = Matrix.Identity;

            //Shader
            this.vertexDeclaration = new VertexDeclaration(mesh.Device, mesh.Declaration);
            this.effect = GuiController.Instance.Shaders.TgcMeshShader;
            this.technique = GuiController.Instance.Shaders.getTgcMeshTechnique(this.renderType);
        }


        /// <summary>
        /// Renderiza la malla, si esta habilitada
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device device = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //Aplicar transformaciones
            updateMeshTransform();

            //Cargar VertexDeclaration
            device.VertexDeclaration = vertexDeclaration;

            //Activar AlphaBlending si corresponde
            activateAlphaBlend();

            //Cargar matrices para el shader
            setShaderMatrix();

            //Renderizar segun el tipo de render de la malla
            effect.Technique = this.technique;
            int numPasses = effect.Begin(0);
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:

                    //Hacer reset de texturas
                    texturesManager.clear(0);
                    texturesManager.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Iniciar pasada de shader
                        effect.BeginPass(n);
                        d3dMesh.DrawSubset(0);
                        effect.EndPass();
                    }

                    break;

                case MeshRenderType.DIFFUSE_MAP:

                    //Hacer reset de Lightmap
                    texturesManager.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Dibujar cada subset con su DiffuseMap correspondiente
                        for (int i = 0; i < materials.Length; i++)
                        {
                            //Setear textura en shader
                            texturesManager.shaderSet(effect, "texDiffuseMap", diffuseMaps[i]);

                            //Iniciar pasada de shader
                            effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            effect.EndPass();
                        }
                    }

                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (int n = 0; n < numPasses; n++)
                    {
                        //Cargar lightmap
                        texturesManager.shaderSet(effect, "texLightMap", lightMap);

                        //Dibujar cada subset con su Material y su DiffuseMap correspondiente
                        for (int i = 0; i < materials.Length; i++)
                        {
                            //Setear textura en shader
                            texturesManager.shaderSet(effect, "texDiffuseMap", diffuseMaps[i]);

                            //Iniciar pasada de shader
                            effect.BeginPass(n);
                            d3dMesh.DrawSubset(i);
                            effect.EndPass();
                        }
                    }

                    break;
            }

            //Finalizar shader
            effect.End();

            //Desactivar alphaBlend
            resetAlphaBlend();
        }


        /// <summary>
        /// Cargar todas la matrices que necesita el shader
        /// </summary>
        protected void setShaderMatrix()
        {
            GuiController.Instance.Shaders.setShaderMatrix(this.effect, this.transform);
        }

        /// <summary>
        /// Aplicar transformaciones del mesh
        /// </summary>
        protected void updateMeshTransform()
        {
            //Aplicar transformacion de malla
            if (autoTransformEnable)
            {
                this.transform = Matrix.Scaling(scale)
                    * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                    * Matrix.Translation(translation);
            }
        }

        /// <summary>
        /// Activar AlphaBlending, si corresponde
        /// </summary>
        protected void activateAlphaBlend()
        {
            Device device = GuiController.Instance.D3dDevice;
            if (alphaBlendEnable)
            {
                device.RenderState.AlphaTestEnable = true;
                device.RenderState.AlphaBlendEnable = true;
            }
        }
        
        /// <summary>
        /// Desactivar AlphaBlending
        /// </summary>
        protected void resetAlphaBlend()
        {
            Device device = GuiController.Instance.D3dDevice;
            device.RenderState.AlphaTestEnable = false;
            device.RenderState.AlphaBlendEnable = false;
        }

        /// <summary>
        /// Libera los recursos de la malla.
        /// Si la malla es una instancia se deshabilita pero no se liberan recursos.
        /// Si la malla es el original y tiene varias instancias adjuntadas, se hace dispose() también de las instancias.
        /// </summary>
        public void dispose()
        {
            this.enabled = false;
            if (boundingBox != null)
            {
                boundingBox.dispose();
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
                foreach (TgcMesh meshInstance in meshInstances)
                {
                    meshInstance.dispose();
                }
                meshInstances = null;
            }
            

            //Dispose de mesh
            this.d3dMesh.Dispose();
            this.d3dMesh = null;

            //Dispose de texturas
            if (diffuseMaps != null)
            {
                for (int i = 0; i < diffuseMaps.Length; i++)
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
        /// Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void move(Vector3 v)
        {
            this.move(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void move(float x, float y, float z)
        {
            this.translation.X += x;
            this.translation.Y += y;
            this.translation.Z += z;

            updateBoundingBox();
        }

        /// <summary>
        /// Mueve la malla en base a la orientacion actual de rotacion.
        /// Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        public void moveOrientedY(float movement)
        {
            float z = FastMath.Cos(rotation.Y) * movement;
            float x = FastMath.Sin(rotation.Y) * movement;

            move(x, 0, z);
        }

        /// <summary>
        /// Obtiene la posicion absoluta de la malla, recibiendo un vector ya creado para
        /// almacenar el resultado
        /// </summary>
        /// <param name="pos">Vector ya creado en el que se carga el resultado</param>
        public void getPosition(Vector3 pos)
        {
            pos.X = translation.X;
            pos.Y = translation.Y;
            pos.Z = translation.Z;
        }

        /// <summary>
        /// Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateX(float angle)
        {
            this.rotation.X += angle;
        }

        /// <summary>
        /// Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateY(float angle)
        {
            this.rotation.Y += angle;
        }

        /// <summary>
        /// Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void rotateZ(float angle)
        {
            this.rotation.Z += angle;
        }

        /// <summary>
        /// Devuelve un array con todas las posiciones de los vértices de la malla
        /// </summary>
        /// <returns>Array creado</returns>
        public Vector3[] getVertexPositions()
        {
            Vector3[] points = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts1.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = verts1[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts2.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = verts2[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts3.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = verts3[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return points;
        }

        /// <summary>
        /// Devuelve un array con todas las coordenadas de textura de la malla
        /// Solo puede hacerse para meshes del tipo DIFFUSE_MAP y DIFFUSE_MAP_AND_LIGHTMAP.
        /// </summary>
        /// <returns>Array creado</returns>
        public Vector2[] getTextureCoordinates()
        {
            Vector2[] uvCoords = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    throw new Exception("No se puede obtener coordenadas de UV en un mesh del tipo VERTEX_COLOR");

                case MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    uvCoords = new Vector2[verts2.Length];
                    for (int i = 0; i < uvCoords.Length; i++)
                    {
                        uvCoords[i] = new Vector2(verts2[i].Tu, verts2[i].Tv);
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    uvCoords = new Vector2[verts3.Length];
                    for (int i = 0; i < uvCoords.Length; i++)
                    {
                        uvCoords[i] = new Vector2(verts3[i].Tu0, verts3[i].Tv0);
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return uvCoords;
        }

        /// <summary>
        /// Calcula el BoundingBox de la malla, en base a todos sus vertices.
        /// Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public TgcBoundingBox createBoundingBox()
        {
            if (this.boundingBox != null)
            {
                this.boundingBox.dispose();
                this.boundingBox = null;
            }
            //Obtener vertices en base al tipo de malla
            Vector3[] points = getVertexPositions();
            this.boundingBox = TgcBoundingBox.computeFromPoints(points);
            return this.boundingBox;
        }

        /// <summary>
        /// Actualiza el BoundingBox de la malla, en base a su posicion actual.
        /// Solo contempla traslacion y escalado
        /// </summary>
        public void updateBoundingBox()
        {
            if (autoUpdateBoundingBox)
            {
                this.boundingBox.scaleTranslate(this.translation, this.scale);
            }
        }

        /// <summary>
        /// Cambia el color de todos los vértices de la malla.
        /// Esta operacion tiene que hacer un lock del VertexBuffer y es poco performante.
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public void setColor(Color color)
        {
            int c = color.ToArgb();
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    TgcSceneLoader.VertexColorVertex[] verts1 = (TgcSceneLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (int i = 0; i < verts1.Length; i++)
                    {
                        verts1[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts1, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    TgcSceneLoader.DiffuseMapVertex[] verts2 = (TgcSceneLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (int i = 0; i < verts2.Length; i++)
                    {
                        verts2[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts2, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
                    TgcSceneLoader.DiffuseMapAndLightmapVertex[] verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (int i = 0; i < verts3.Length; i++)
                    {
                        verts3[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts3, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
        }


        /// <summary>
        /// Permite cambiar las texturas de DiffuseMap de esta malla
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
                foreach (TgcTexture t in diffuseMaps)
                {
                    t.dispose();
                }

                //Asignar nuevas texturas
                this.diffuseMaps = newDiffuseMaps;
            }
        }

        /// <summary>
        /// Crea una nueva malla que es una instancia de esta malla original
        /// Reutiliza toda la geometría de la malla original sin duplicarla.
        /// Solo se puede crear instancias a partir de originales.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcMesh createMeshInstance(string name, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (this.parentInstance != null)
            {
                throw new Exception("No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            TgcMesh instance = new TgcMesh(name, this, translation, rotation, scale);

            //BoundingBox
            instance.boundingBox = new TgcBoundingBox(this.boundingBox.PMin, this.boundingBox.PMax);
            instance.updateBoundingBox();

            instance.enabled = true;
            return instance;
        }

        /// <summary>
        /// Crea una nueva malla que es una instancia de esta malla original
        /// Reutiliza toda la geometría de la malla original sin duplicarla.
        /// Solo se puede crear instancias a partir de originales.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        public TgcMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, Vector3.Empty, Vector3.Empty, new Vector3(1, 1, 1));
        }

        public override string ToString()
        {
            return "Mesh: " + name;
        }

        /// <summary>
        /// Crear un nuevo mesh igual
        /// </summary>
        /// <param name="cloneName">Nombre del mesh clonado</param>
        /// <returns>Mesh clonado</returns>
        public TgcMesh clone(string cloneName)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Clonar D3dMesh
            Mesh d3dCloneMesh = this.d3dMesh.Clone(MeshFlags.Managed, this.d3dMesh.Declaration, device);

            //Crear mesh de TGC y cargar atributos generales
            TgcMesh cloneMesh = new TgcMesh(d3dCloneMesh, cloneName, this.renderType);
            cloneMesh.Materials = this.Materials;
            cloneMesh.layer = this.layer;
            cloneMesh.boundingBox = this.boundingBox.clone();
            cloneMesh.alphaBlendEnable = this.alphaBlendEnable;
            cloneMesh.enabled = true;
            cloneMesh.autoUpdateBoundingBox = this.autoUpdateBoundingBox;

            //Transformaciones
            cloneMesh.translation = this.translation;
            cloneMesh.rotation = this.rotation;
            cloneMesh.scale = this.scale;
            cloneMesh.transform = this.transform;
            cloneMesh.autoTransformEnable = this.autoTransformEnable;

            //Clonar userProperties
            if (this.userProperties != null)
            {
                cloneMesh.userProperties = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> entry in this.userProperties)
                {
                    cloneMesh.userProperties.Add(entry.Key, entry.Value);
                }
            }

            //Clonar DiffuseMaps
            if (this.diffuseMaps != null)
            {
                cloneMesh.diffuseMaps = new TgcTexture[this.diffuseMaps.Length];
                for (int i = 0; i < this.diffuseMaps.Length; i++)
			    {
                    cloneMesh.diffuseMaps[i] = this.diffuseMaps[i].clone();
			    }
            }

            //Clonar LightMap
            if (this.lightMap != null)
            {
                cloneMesh.lightMap = this.lightMap.clone();
            }


            return cloneMesh;
        }

        /// <summary>
        /// Cambiar el mesh interno de DirectX por uno nuevo.
        /// Se asume que el nuevo mesh es del mismo RenderType que el anterior.
        /// </summary>
        /// <param name="newD3dMesh">Nuevo mesh</param>
        public void changeD3dMesh(Mesh newD3dMesh)
        {
            this.d3dMesh.Dispose();
            this.d3dMesh = newD3dMesh;
        }

        /// <summary>
        /// Acceder al VertexBuffer del mesh.
        /// Una vez que se termina de trabajar con el buffer se debe invocar siempre a unlock.
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

    }
}
