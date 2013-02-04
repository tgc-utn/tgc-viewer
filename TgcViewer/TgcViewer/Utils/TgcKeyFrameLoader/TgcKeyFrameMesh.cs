using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;

namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    /// <summary>
    /// Malla que representa un modelo 3D con varias animaciones, animadas por KeyFrame Animation
    /// </summary>
    public class TgcKeyFrameMesh : IRenderObject, ITransformObject
    {
        /// <summary>
        /// Mesh de DirectX
        /// </summary>
        private Mesh d3dMesh;
        /// <summary>
        /// Mesh interna de DirectX
        /// </summary>
        public Mesh D3dMesh
        {
            get { return d3dMesh; }
        }

        private Dictionary<string, TgcKeyFrameAnimation> animations;
        /// <summary>
        /// Mapa de animaciones de la malla
        /// </summary>
        public Dictionary<string, TgcKeyFrameAnimation> Animations
        {
            get { return animations; }
        }

        private string name;
        /// <summary>
        /// Nombre de la malla
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private Material[] materials;
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

        private TgcTexture[] diffuseMaps;
        /// <summary>
        /// Array de texturas para DiffuseMap
        /// </summary>
        public TgcTexture[] DiffuseMaps
        {
            get { return diffuseMaps; }
            set { diffuseMaps = value; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        Matrix transform;
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

        bool autoTransformEnable;
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


        private Vector3 translation;
        /// <summary>
        /// Posicion absoluta de la Malla
        /// </summary>
        public Vector3 Position
        {
            get { return translation; }
            set { 
                translation = value;
                updateBoundingBox();
            }
        }

        private Vector3 rotation;
        /// <summary>
        /// Rotación absoluta de la malla
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        private Vector3 scale;
        /// <summary>
        /// Escalado absoluto de la malla;
        /// </summary>
        public Vector3 Scale
        {
            get { return scale; }
            set { 
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
        };

        /// <summary>
        /// BoundingBox de la malla sin ninguna animación.
        /// </summary>
        private TgcBoundingBox staticMeshBoundingBox;

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox del Mesh.
        /// Puede variar según la animación que tiene configurada en el momento.
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
            set
            {
                staticMeshBoundingBox = value;
                boundingBox = value;
            }
        }

        private MeshRenderType renderType;
        /// <summary>
        /// Tipo de formato de Render de esta malla
        /// </summary>
        public MeshRenderType RenderType
        {
            get { return renderType; }
            set { renderType = value; }
        }

        VertexDeclaration vertexDeclaration;
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

        TgcKeyFrameAnimation currentAnimation;
        /// <summary>
        /// Animación actual de la malla
        /// </summary>
        public TgcKeyFrameAnimation CurrentAnimation
        {
            get { return currentAnimation; }
        }

        private float frameRate;
        /// <summary>
        /// Velocidad de la animacion medida en cuadros por segundo.
        /// </summary>
        public float FrameRate
        {
            get { return frameRate; }
        }

        private int currentFrame;
        /// <summary>
        /// Cuadro actual de animacion
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        bool isAnimating;
        /// <summary>
        /// Indica si actualmente hay una animación en curso.
        /// </summary>
        public bool IsAnimating
        {
            get { return isAnimating; }
        }

        bool playLoop;
        /// <summary>
        /// Indica si la animación actual se ejecuta con un Loop
        /// </summary>
        public bool PlayLoop
        {
            get { return playLoop; }
        }

        private TgcKeyFrameMesh parentInstance;
        /// <summary>
        /// Original desde el cual esta malla fue clonada.
        /// </summary>
        public TgcKeyFrameMesh ParentInstance
        {
            get { return parentInstance; }
        }

        private List<TgcKeyFrameMesh> meshInstances;
        /// <summary>
        /// Lista de mallas que fueron clonadas a partir de este original
        /// </summary>
        public List<TgcKeyFrameMesh> MeshInstances
        {
            get { return meshInstances; }
        }

        private bool alphaBlendEnable;
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


        #region Eventos

        /// <summary>
        /// Indica que la animación actual ha finalizado.
        /// Se llama cuando se acabaron los frames de la animación.
        /// Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        /// <param name="mesh">Malla animada</param>
        public delegate void AnimationEndsHandler(TgcKeyFrameMesh mesh);
        /// <summary>
        /// Evento que se llama cada vez que la animación actual finaliza.
        /// Se llama cuando se acabaron los frames de la animación.
        /// Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        public event AnimationEndsHandler AnimationEnds;

        #endregion

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

        /// <summary>
        /// Informacion del MeshData original que hay que guardar para poder alterar el VertexBuffer con la animacion
        /// </summary>
        private OriginalData originalData;

        //Variables de animacion
        float currentTime;
        float animationTimeLenght;

        /// <summary>
        /// Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        /// <param name="coordinatesIndices">Datos parseados de la malla</param>
        public TgcKeyFrameMesh(Mesh mesh, string name, MeshRenderType renderType, OriginalData originalData)
        {
            this.initData(mesh, name, renderType, originalData);
        }

        /// <summary>
        /// Crea una nueva malla que es una instancia de otra malla original.
        /// Reutiliza toda la geometría de la malla original sin duplicarla.
        /// Debe crearse luego de haber cargado todas las animaciones en la malla original
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="parentInstance">Malla original desde la cual basarse</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcKeyFrameMesh(string name, TgcKeyFrameMesh parentInstance, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            //Cargar iniciales datos en base al original
            this.initData(parentInstance.d3dMesh, name, parentInstance.renderType, parentInstance.originalData);
            this.diffuseMaps = parentInstance.diffuseMaps;
            this.materials = parentInstance.materials;

            //Almacenar transformación inicial
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            //Agregar animaciones del original
            foreach (KeyValuePair<string, TgcKeyFrameAnimation> entry in parentInstance.animations)
            {
                this.animations.Add(entry.Key, entry.Value);
            }

            //almacenar instancia en el padre
            this.parentInstance = parentInstance;
            parentInstance.meshInstances.Add(this);
        }

        /// <summary>
        /// Cargar datos iniciales
        /// </summary>
        private void initData(Mesh mesh, string name, MeshRenderType renderType, OriginalData originalData)
        {
            this.d3dMesh = mesh;
            this.name = name;
            this.renderType = renderType;
            this.originalData = originalData;
            this.enabled = false;
            this.autoUpdateBoundingBox = true;
            this.meshInstances = new List<TgcKeyFrameMesh>();
            this.alphaBlendEnable = false;

            vertexDeclaration = new VertexDeclaration(mesh.Device, mesh.Declaration);

            //variables de movimiento
            this.autoTransformEnable = true;
            this.translation = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector3(0f, 0f, 0f);
            this.scale = new Vector3(1f, 1f, 1f);
            this.transform = Matrix.Identity;

            //variables de animacion
            this.isAnimating = false;
            this.currentAnimation = null;
            this.playLoop = false;
            this.currentTime = 0f;
            this.currentFrame = 0;
            this.animationTimeLenght = 0f;
            this.animations = new Dictionary<string, TgcKeyFrameAnimation>();

            //Shader
            this.effect = GuiController.Instance.Shaders.TgcKeyFrameMeshShader;
            this.technique = GuiController.Instance.Shaders.getTgcKeyFrameMeshTechnique(this.renderType);
        }


        /// <summary>
        /// Establece cual es la animacion activa de la malla.
        /// Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        /// Para forzar que se reinicie es necesario hacer stopAnimation()
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        /// <param name="playLoop">Indica si la animacion vuelve a comenzar al terminar</param>
        /// <param name="userFrameRate">FrameRate personalizado. Con -1 se utiliza el default de la animación</param>
        public void playAnimation(string animationName, bool playLoop, float userFrameRate)
        {
            //ya se esta animando algo
            if (isAnimating)
            {
                //Si la animacion pedida es la misma que la actual no la quitamos
                if(currentAnimation.Data.name.Equals(animationName))
                {
                    //solo actualizamos el playLoop
                    this.playLoop = playLoop;
                }
                //es una nueva animacion
                else
                {
                    //parar animacion actual
                    stopAnimation();
                    //cargar nueva animacion
                    initAnimationSettings(animationName, playLoop, userFrameRate);
                }
            }

            //no se esta animando nada
            else
            {
                //cargar nueva animacion
                initAnimationSettings(animationName, playLoop, userFrameRate);
            }
        }

        /// <summary>
        /// Establece cual es la animacion activa de la malla.
        /// Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        /// Para forzar que se reinicie es necesario hacer stopAnimation().
        /// Utiliza el FrameRate default de cada animación
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        /// <param name="playLoop">Indica si la animacion vuelve a comenzar al terminar</param>
        public void playAnimation(string animationName, bool playLoop)
        {
            playAnimation(animationName, playLoop, -1f);
        }

        /// <summary>
        /// Establece cual es la animacion activa de la malla.
        /// Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        /// Para forzar que se reinicie es necesario hacer stopAnimation().
        /// Se reproduce con loop.
        /// Utiliza el FrameRate default de cada animación
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        public void playAnimation(string animationName)
        {
            playAnimation(animationName, true);
        }





        /// <summary>
        /// Prepara una nueva animacion para ser ejecutada
        /// </summary>
        private void initAnimationSettings(string animationName, bool playLoop, float userFrameRate)
        {
            isAnimating = true;
            currentAnimation = animations[animationName];
            this.playLoop = playLoop;
            currentTime = 0;
            currentFrame = 0;

            //Cambiar BoundingBox
            boundingBox = currentAnimation.BoundingBox;
            updateBoundingBox();

            //Si el usuario no especifico un FrameRate, tomar el default de la animacion
            if (userFrameRate == -1f)
            {
                frameRate = (float)currentAnimation.Data.frameRate;
            }
            else
            {
                frameRate = userFrameRate;
            }

            //La duracion de la animacion.
            animationTimeLenght = (float)currentAnimation.Data.framesCount / frameRate;
        }


        /// <summary>
        /// Desactiva la animacion actual
        /// </summary>
        public void stopAnimation()
        {
            isAnimating = false;
            boundingBox = staticMeshBoundingBox;

            //Invocar evento de finalización
            if (AnimationEnds != null)
            {
                AnimationEnds.Invoke(this);
            }
        }


        /// <summary>
        /// Actualiza el cuadro actual de la animacion.
        /// Debe ser llamado en cada cuadro antes de render()
        /// </summary>
        public void updateAnimation()
        {
            Device device = GuiController.Instance.D3dDevice;
            float elapsedTime = GuiController.Instance.ElapsedTime;

            //Ver que haya transcurrido cierta cantidad de tiempo
            if (elapsedTime < 0.0f)
            {
                return;
            }

            //Sumo el tiempo transcurrido
            currentTime += elapsedTime;

            //Se termino la animacion
            if (currentTime > animationTimeLenght)
            {
                //Ver si hacer loop
                if (playLoop)
                {
                    currentTime = 0;
                }
                else
                {
                    stopAnimation();
                }
            }

            //La animacion continua
            else
            {
                //TODO: controlar caso especial cuando hay solo un KeyFrame


                //Tomar el frame actual.
                int frameNumber = getCurrentFrame();
                this.currentFrame = frameNumber;

                //KeyFrames a interpolar
                TgcKeyFrameFrameData keyFrame1 = currentAnimation.Data.keyFrames[frameNumber];
                TgcKeyFrameFrameData keyFrame2 = currentAnimation.Data.keyFrames[frameNumber + 1];

                float[] verticesFrame1 = keyFrame1.verticesCoordinates;
                float[] verticesFrame2 = keyFrame2.verticesCoordinates;

                //La diferencia de tiempo entre el frame actual y el siguiente.
                float timeDifferenceBetweenFrames = keyFrame2.relativeTime - keyFrame1.relativeTime;

                //En que posicion relativa de los dos frames actuales estamos.
                float interpolationValue = ((currentTime / animationTimeLenght) - keyFrame1.relativeTime) / timeDifferenceBetweenFrames;

                //Cargar array de vertices interpolados
                float[] verticesFrameFinal = new float[verticesFrame1.Length];
                for (int i = 0; i < verticesFrameFinal.Length; i++)
                {
                    verticesFrameFinal[i] = (verticesFrame2[i] - verticesFrame1[i]) * interpolationValue + verticesFrame1[i];
                }

                //expandir array para el vertex buffer
                fillVertexBufferData(verticesFrameFinal);
            }
        }

        /// <summary>
        /// Llena la informacion del VertexBuffer con los vertices especificados
        /// </summary>
        /// <param name="verticesCoordinates"></param>
        private void fillVertexBufferData(float[] verticesCoordinates)
        {
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    TgcKeyFrameLoader.VertexColorVertex[] data1 = new TgcKeyFrameLoader.VertexColorVertex[originalData.coordinatesIndices.Length];
                    for (int i = 0; i < originalData.coordinatesIndices.Length; i++)
                    {
                        TgcKeyFrameLoader.VertexColorVertex v = new TgcKeyFrameLoader.VertexColorVertex();

                        //vertices
                        int coordIdx = originalData.coordinatesIndices[i] * 3;
                        v.Position = new Vector3(
                            verticesCoordinates[coordIdx],
                            verticesCoordinates[coordIdx + 1],
                            verticesCoordinates[coordIdx + 2]
                            );

                        //color
                        int colorIdx = originalData.colorIndices[i];
                        v.Color = originalData.verticesColors[colorIdx];

                        data1[i] = v;
                    }
                    d3dMesh.SetVertexBufferData(data1, LockFlags.None);
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    TgcKeyFrameLoader.DiffuseMapVertex[] data2 = new TgcKeyFrameLoader.DiffuseMapVertex[originalData.coordinatesIndices.Length];
                    for (int i = 0; i < originalData.coordinatesIndices.Length; i++)
                    {
                        TgcKeyFrameLoader.DiffuseMapVertex v = new TgcKeyFrameLoader.DiffuseMapVertex();

                        //vertices
                        int coordIdx = originalData.coordinatesIndices[i] * 3;
                        v.Position = new Vector3(
                            verticesCoordinates[coordIdx],
                            verticesCoordinates[coordIdx + 1],
                            verticesCoordinates[coordIdx + 2]
                            );

                        //texture coordinates diffuseMap
                        int texCoordIdx = originalData.texCoordinatesIndices[i] * 2;
                        v.Tu = originalData.textureCoordinates[texCoordIdx];
                        v.Tv = originalData.textureCoordinates[texCoordIdx + 1];

                        //color
                        int colorIdx = originalData.colorIndices[i];
                        v.Color = originalData.verticesColors[colorIdx];

                        data2[i] = v;
                    }
                    d3dMesh.SetVertexBufferData(data2, LockFlags.None);
                    break;
            }
        }

        /// <summary>
        /// Devuelve el cuadro actual de animacion
        /// </summary>
        /// <returns></returns>
        private int getCurrentFrame()
        {
            float position = currentTime / animationTimeLenght;
            TgcKeyFrameAnimationData data = currentAnimation.Data;

            for (int i = 0; i < data.keyFrames.Length; i++)
            {
                if (position < data.keyFrames[i].relativeTime)
                {
                    return i - 1;
                }
            }

            return data.keyFrames.Length - 2;
        }


        /// <summary>
        /// Renderiza la malla, si esta habilitada.
        /// Para que haya animacion se tiene que haber seteado una y haber
        /// llamado previamente al metodo updateAnimation()
        /// Sino se renderiza la pose fija de la malla
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
                this.transform = Matrix.Identity
                    * Matrix.Scaling(scale)
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
        /// Actualiza el cuadro actual de animacion y renderiza la malla.
        /// Es equivalente a llamar a updateAnimation() y luego a render()
        /// </summary>
        public void animateAndRender()
        {
            if (!enabled)
                return;

            updateAnimation();
            render();
        }


        /// <summary>
        /// Libera los recursos de la malla
        /// </summary>
        public void dispose()
        {
            this.enabled = false;
            if (boundingBox != null)
            {
                boundingBox.dispose();
            }

            //dejar de utilizar originalData
            originalData = null;

            //Si es una instancia no liberar nada, lo hace el original.
            if (parentInstance != null)
            {
                parentInstance = null;
                return;
            }

            //hacer dispose de instancias
            foreach (TgcKeyFrameMesh meshInstance in meshInstances)
            {
                meshInstance.dispose();
            }
            meshInstances = null;

            //Dispose de mesh
            this.d3dMesh.Dispose();
            this.d3dMesh = null;

            //Dispose de texturas
            if (diffuseMaps != null)
            {
                for (int i = 0; i < diffuseMaps.Length; i++)
                {
                    diffuseMaps[i].dispose();
                }
                diffuseMaps = null;
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
            float z = (float)Math.Cos((float)rotation.Y) * movement;
            float x = (float)Math.Sin((float)rotation.Y) * movement;

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
        /// Devuelve un array con todas las posiciones de los vértices de la malla, en el estado actual
        /// </summary>
        /// <returns>Array creado</returns>
        public Vector3[] getVertexPositions()
        {
            Vector3[] points = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    TgcKeyFrameLoader.VertexColorVertex[] verts1 = (TgcKeyFrameLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcKeyFrameLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts1.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = verts1[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    TgcKeyFrameLoader.DiffuseMapVertex[] verts2 = (TgcKeyFrameLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcKeyFrameLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts2.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = verts2[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return points;
        }

        /// <summary>
        /// Calcula el BoundingBox de la malla, en base a todos sus vertices.
        /// Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public TgcBoundingBox createBoundingBox()
        {
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
        /// En modelos complejos puede resultar una operación poco performante.
        /// La actualización será visible la próxima vez que se haga updateAnimation()
        /// Si hay instnacias de este modelo, sea el original o una copia, todos los demás se verán
        /// afectados
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public void setColor(Color color)
        {
            int c = color.ToArgb();
            for (int i = 0; i < originalData.verticesColors.Length; i++)
            {
                originalData.verticesColors[i] = c;
            }
        }
            

        /// <summary>
        /// Permite cambiar las texturas de DiffuseMap de esta malla
        /// </summary>
        /// <param name="newDiffuseMaps">Array de nuevas texturas. Tiene que tener la misma cantidad que el original</param>
        public void changeDiffuseMaps(TgcTexture[] newDiffuseMaps)
        {
            //Solo aplicar si la malla tiene texturas
            if (renderType == MeshRenderType.DIFFUSE_MAP)
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
        /// Se debe crear después de haber agregado todas las animaciones al original.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcKeyFrameMesh createMeshInstance(string name, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (this.parentInstance != null)
            {
                throw new Exception("No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            TgcKeyFrameMesh instance = new TgcKeyFrameMesh(name, this, translation, rotation, scale);

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
        /// Se debe crear después de haber agregado todas las animaciones al original.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        public TgcKeyFrameMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, Vector3.Empty, Vector3.Empty, new Vector3(1, 1, 1));
        }


        /// <summary>
        /// Informacion del MeshData original que hay que guardar para poder alterar el VertexBuffer con la animacion
        /// </summary>
        public class OriginalData
        {
            public int[] coordinatesIndices;
            public int[] texCoordinatesIndices;
            public int[] colorIndices;
            public float[] textureCoordinates;
            public int[] verticesColors;
        }

        public override string ToString()
        {
            return "Mesh: " + name;
        }
    }
}
