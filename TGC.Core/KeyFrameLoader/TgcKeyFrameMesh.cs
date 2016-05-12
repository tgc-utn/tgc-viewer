using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.KeyFrameLoader
{
    /// <summary>
    ///     Malla que representa un modelo 3D con varias animaciones, animadas por KeyFrame Animation
    /// </summary>
    public class TgcKeyFrameMesh : IRenderObject, ITransformObject
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
            DIFFUSE_MAP
        }

        private float animationTimeLenght;

        private TgcBoundingBox boundingBox;

        //Variables de animacion
        private float currentTime;

        /// <summary>
        ///     Mesh de DirectX
        /// </summary>
        private Mesh d3dMesh;

        protected Effect effect;

        /// <summary>
        ///     Informacion del MeshData original que hay que guardar para poder alterar el VertexBuffer con la animacion
        /// </summary>
        private OriginalData originalData;

        private Vector3 rotation;

        private Vector3 scale;

        /// <summary>
        ///     BoundingBox de la malla sin ninguna animación.
        /// </summary>
        private TgcBoundingBox staticMeshBoundingBox;

        protected string technique;

        private Vector3 translation;

        /// <summary>
        ///     Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        /// <param name="coordinatesIndices">Datos parseados de la malla</param>
        public TgcKeyFrameMesh(Mesh mesh, string name, MeshRenderType renderType, OriginalData originalData)
        {
            initData(mesh, name, renderType, originalData);
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de otra malla original.
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Debe crearse luego de haber cargado todas las animaciones en la malla original
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="parentInstance">Malla original desde la cual basarse</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcKeyFrameMesh(string name, TgcKeyFrameMesh parentInstance, Vector3 translation, Vector3 rotation,
            Vector3 scale)
        {
            //Cargar iniciales datos en base al original
            initData(parentInstance.d3dMesh, name, parentInstance.RenderType, parentInstance.originalData);
            DiffuseMaps = parentInstance.DiffuseMaps;
            Materials = parentInstance.Materials;

            //Almacenar transformación inicial
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            //Agregar animaciones del original
            foreach (var entry in parentInstance.Animations)
            {
                Animations.Add(entry.Key, entry.Value);
            }

            //almacenar instancia en el padre
            ParentInstance = parentInstance;
            parentInstance.MeshInstances.Add(this);
        }

        /// <summary>
        ///     Mesh interna de DirectX
        /// </summary>
        public Mesh D3dMesh
        {
            get { return d3dMesh; }
        }

        /// <summary>
        ///     Mapa de animaciones de la malla
        /// </summary>
        public Dictionary<string, TgcKeyFrameAnimation> Animations { get; private set; }

        /// <summary>
        ///     Nombre de la malla
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Array de Materials
        /// </summary>
        public Material[] Materials { get; set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Array de texturas para DiffuseMap
        /// </summary>
        public TgcTexture[] DiffuseMaps { get; set; }

        /// <summary>
        ///     Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     BoundingBox del Mesh.
        ///     Puede variar según la animación que tiene configurada en el momento.
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

        /// <summary>
        ///     Tipo de formato de Render de esta malla
        /// </summary>
        public MeshRenderType RenderType { get; set; }

        /// <summary>
        ///     VertexDeclaration del Flexible Vertex Format (FVF) usado por la malla
        /// </summary>
        public VertexDeclaration VertexDeclaration { get; private set; }

        /// <summary>
        ///     Indica si se actualiza automaticamente el BoundingBox con cada movimiento de la malla
        /// </summary>
        public bool AutoUpdateBoundingBox { get; set; }

        /// <summary>
        ///     Animación actual de la malla
        /// </summary>
        public TgcKeyFrameAnimation CurrentAnimation { get; private set; }

        /// <summary>
        ///     Velocidad de la animacion medida en cuadros por segundo.
        /// </summary>
        public float FrameRate { get; private set; }

        /// <summary>
        ///     Cuadro actual de animacion
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        ///     Indica si actualmente hay una animación en curso.
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        ///     Indica si la animación actual se ejecuta con un Loop
        /// </summary>
        public bool PlayLoop { get; private set; }

        /// <summary>
        ///     Original desde el cual esta malla fue clonada.
        /// </summary>
        public TgcKeyFrameMesh ParentInstance { get; private set; }

        /// <summary>
        ///     Lista de mallas que fueron clonadas a partir de este original
        /// </summary>
        public List<TgcKeyFrameMesh> MeshInstances { get; private set; }

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
        ///     Renderiza la malla, si esta habilitada.
        ///     Para que haya animacion se tiene que haber seteado una y haber
        ///     llamado previamente al metodo updateAnimation()
        ///     Sino se renderiza la pose fija de la malla
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            //Aplicar transformaciones
            updateMeshTransform();

            //Cargar VertexDeclaration
            D3DDevice.Instance.Device.VertexDeclaration = VertexDeclaration;

            //Activar AlphaBlending si corresponde
            activateAlphaBlend();

            //Cargar matrices para el shader
            setShaderMatrix();

            //Renderizar segun el tipo de Render de la malla
            effect.Technique = technique;
            var numPasses = effect.Begin(0);
            switch (RenderType)
            {
                case MeshRenderType.VERTEX_COLOR:

                    //Hacer reset de texturas
                    TexturesManager.Instance.clear(0);
                    TexturesManager.Instance.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (var n = 0; n < numPasses; n++)
                    {
                        //Iniciar pasada de shader
                        effect.BeginPass(n);
                        d3dMesh.DrawSubset(0);
                        effect.EndPass();
                    }
                    break;

                case MeshRenderType.DIFFUSE_MAP:

                    //Hacer reset de Lightmap
                    TexturesManager.Instance.clear(1);

                    //Iniciar Shader e iterar sobre sus Render Passes
                    for (var n = 0; n < numPasses; n++)
                    {
                        //Dibujar cada subset con su DiffuseMap correspondiente
                        for (var i = 0; i < Materials.Length; i++)
                        {
                            //Setear textura en shader
                            TexturesManager.Instance.shaderSet(effect, "texDiffuseMap", DiffuseMaps[i]);

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
        ///     Libera los recursos de la malla
        /// </summary>
        public void dispose()
        {
            Enabled = false;
            if (boundingBox != null)
            {
                boundingBox.dispose();
            }

            //dejar de utilizar originalData
            originalData = null;

            //Si es una instancia no liberar nada, lo hace el original.
            if (ParentInstance != null)
            {
                ParentInstance = null;
                return;
            }

            //hacer dispose de instancias
            foreach (var meshInstance in MeshInstances)
            {
                meshInstance.dispose();
            }
            MeshInstances = null;

            //Dispose de mesh
            d3dMesh.Dispose();
            d3dMesh = null;

            //Dispose de texturas
            if (DiffuseMaps != null)
            {
                for (var i = 0; i < DiffuseMaps.Length; i++)
                {
                    DiffuseMaps[i].dispose();
                }
                DiffuseMaps = null;
            }

            //VertexDeclaration
            VertexDeclaration.Dispose();
            VertexDeclaration = null;
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

        /// <summary>
        ///     Posicion absoluta de la Malla
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

        /// <summary>
        ///     Rotación absoluta de la malla
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        ///     Escalado absoluto de la malla;
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

            updateBoundingBox();
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
        ///     Cargar datos iniciales
        /// </summary>
        private void initData(Mesh mesh, string name, MeshRenderType renderType, OriginalData originalData)
        {
            d3dMesh = mesh;
            Name = name;
            RenderType = renderType;
            this.originalData = originalData;
            Enabled = false;
            AutoUpdateBoundingBox = true;
            MeshInstances = new List<TgcKeyFrameMesh>();
            AlphaBlendEnable = false;

            VertexDeclaration = new VertexDeclaration(mesh.Device, mesh.Declaration);

            //variables de movimiento
            AutoTransformEnable = true;
            translation = new Vector3(0f, 0f, 0f);
            rotation = new Vector3(0f, 0f, 0f);
            scale = new Vector3(1f, 1f, 1f);
            Transform = Matrix.Identity;

            //variables de animacion
            IsAnimating = false;
            CurrentAnimation = null;
            PlayLoop = false;
            currentTime = 0f;
            CurrentFrame = 0;
            animationTimeLenght = 0f;
            Animations = new Dictionary<string, TgcKeyFrameAnimation>();

            //Shader
            effect = TgcShaders.Instance.TgcKeyFrameMeshShader;
            technique = TgcShaders.Instance.getTgcKeyFrameMeshTechnique(RenderType);
        }

        /// <summary>
        ///     Establece cual es la animacion activa de la malla.
        ///     Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        ///     Para forzar que se reinicie es necesario hacer stopAnimation()
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        /// <param name="playLoop">Indica si la animacion vuelve a comenzar al terminar</param>
        /// <param name="userFrameRate">FrameRate personalizado. Con -1 se utiliza el default de la animación</param>
        public void playAnimation(string animationName, bool playLoop, float userFrameRate)
        {
            //ya se esta animando algo
            if (IsAnimating)
            {
                //Si la animacion pedida es la misma que la actual no la quitamos
                if (CurrentAnimation.Data.name.Equals(animationName))
                {
                    //solo actualizamos el playLoop
                    PlayLoop = playLoop;
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
        ///     Establece cual es la animacion activa de la malla.
        ///     Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        ///     Para forzar que se reinicie es necesario hacer stopAnimation().
        ///     Utiliza el FrameRate default de cada animación
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        /// <param name="playLoop">Indica si la animacion vuelve a comenzar al terminar</param>
        public void playAnimation(string animationName, bool playLoop)
        {
            playAnimation(animationName, playLoop, -1f);
        }

        /// <summary>
        ///     Establece cual es la animacion activa de la malla.
        ///     Si la animacion activa es la misma que ya esta siendo animada actualmente, no se para ni se reinicia.
        ///     Para forzar que se reinicie es necesario hacer stopAnimation().
        ///     Se reproduce con loop.
        ///     Utiliza el FrameRate default de cada animación
        /// </summary>
        /// <param name="animationName">Nombre de la animacion a activar</param>
        public void playAnimation(string animationName)
        {
            playAnimation(animationName, true);
        }

        /// <summary>
        ///     Prepara una nueva animacion para ser ejecutada
        /// </summary>
        private void initAnimationSettings(string animationName, bool playLoop, float userFrameRate)
        {
            IsAnimating = true;
            CurrentAnimation = Animations[animationName];
            PlayLoop = playLoop;
            currentTime = 0;
            CurrentFrame = 0;

            //Cambiar BoundingBox
            boundingBox = CurrentAnimation.BoundingBox;
            updateBoundingBox();

            //Si el usuario no especifico un FrameRate, tomar el default de la animacion
            if (userFrameRate == -1f)
            {
                FrameRate = CurrentAnimation.Data.frameRate;
            }
            else
            {
                FrameRate = userFrameRate;
            }

            //La duracion de la animacion.
            animationTimeLenght = CurrentAnimation.Data.framesCount / FrameRate;
        }

        /// <summary>
        ///     Desactiva la animacion actual
        /// </summary>
        public void stopAnimation()
        {
            IsAnimating = false;
            boundingBox = staticMeshBoundingBox;

            //Invocar evento de finalización
            if (AnimationEnds != null)
            {
                AnimationEnds.Invoke(this);
            }
        }

        /// <summary>
        ///     Actualiza el cuadro actual de la animacion.
        ///     Debe ser llamado en cada cuadro antes de Render()
        /// </summary>
        public void updateAnimation(float elapsedTime)
        {
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
                if (PlayLoop)
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
                var frameNumber = getCurrentFrame();
                CurrentFrame = frameNumber;

                //KeyFrames a interpolar
                var keyFrame1 = CurrentAnimation.Data.keyFrames[frameNumber];
                var keyFrame2 = CurrentAnimation.Data.keyFrames[frameNumber + 1];

                var verticesFrame1 = keyFrame1.verticesCoordinates;
                var verticesFrame2 = keyFrame2.verticesCoordinates;

                //La diferencia de tiempo entre el frame actual y el siguiente.
                var timeDifferenceBetweenFrames = keyFrame2.relativeTime - keyFrame1.relativeTime;

                //En que posicion relativa de los dos frames actuales estamos.
                var interpolationValue = (currentTime / animationTimeLenght - keyFrame1.relativeTime) /
                                         timeDifferenceBetweenFrames;

                //Cargar array de vertices interpolados
                var verticesFrameFinal = new float[verticesFrame1.Length];
                for (var i = 0; i < verticesFrameFinal.Length; i++)
                {
                    verticesFrameFinal[i] = (verticesFrame2[i] - verticesFrame1[i]) * interpolationValue +
                                            verticesFrame1[i];
                }

                //expandir array para el vertex buffer
                fillVertexBufferData(verticesFrameFinal);
            }
        }

        /// <summary>
        ///     Llena la informacion del VertexBuffer con los vertices especificados
        /// </summary>
        /// <param name="verticesCoordinates"></param>
        private void fillVertexBufferData(float[] verticesCoordinates)
        {
            switch (RenderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var data1 = new TgcKeyFrameLoader.VertexColorVertex[originalData.coordinatesIndices.Length];
                    for (var i = 0; i < originalData.coordinatesIndices.Length; i++)
                    {
                        var v = new TgcKeyFrameLoader.VertexColorVertex();

                        //vertices
                        var coordIdx = originalData.coordinatesIndices[i] * 3;
                        v.Position = new Vector3(
                            verticesCoordinates[coordIdx],
                            verticesCoordinates[coordIdx + 1],
                            verticesCoordinates[coordIdx + 2]
                            );

                        //color
                        var colorIdx = originalData.colorIndices[i];
                        v.Color = originalData.verticesColors[colorIdx];

                        data1[i] = v;
                    }
                    d3dMesh.SetVertexBufferData(data1, LockFlags.None);
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var data2 = new TgcKeyFrameLoader.DiffuseMapVertex[originalData.coordinatesIndices.Length];
                    for (var i = 0; i < originalData.coordinatesIndices.Length; i++)
                    {
                        var v = new TgcKeyFrameLoader.DiffuseMapVertex();

                        //vertices
                        var coordIdx = originalData.coordinatesIndices[i] * 3;
                        v.Position = new Vector3(
                            verticesCoordinates[coordIdx],
                            verticesCoordinates[coordIdx + 1],
                            verticesCoordinates[coordIdx + 2]
                            );

                        //texture coordinates diffuseMap
                        var texCoordIdx = originalData.texCoordinatesIndices[i] * 2;
                        v.Tu = originalData.textureCoordinates[texCoordIdx];
                        v.Tv = originalData.textureCoordinates[texCoordIdx + 1];

                        //color
                        var colorIdx = originalData.colorIndices[i];
                        v.Color = originalData.verticesColors[colorIdx];

                        data2[i] = v;
                    }
                    d3dMesh.SetVertexBufferData(data2, LockFlags.None);
                    break;
            }
        }

        /// <summary>
        ///     Devuelve el cuadro actual de animacion
        /// </summary>
        /// <returns></returns>
        private int getCurrentFrame()
        {
            var position = currentTime / animationTimeLenght;
            var data = CurrentAnimation.Data;

            for (var i = 0; i < data.keyFrames.Length; i++)
            {
                if (position < data.keyFrames[i].relativeTime)
                {
                    return i - 1;
                }
            }

            return data.keyFrames.Length - 2;
        }

        /// <summary>
        ///     Cargar todas la matrices que necesita el shader
        /// </summary>
        protected void setShaderMatrix()
        {
            TgcShaders.Instance.setShaderMatrix(effect, Transform);
        }

        /// <summary>
        ///     Aplicar transformaciones del mesh
        /// </summary>
        protected void updateMeshTransform()
        {
            //Aplicar transformacion de malla
            if (AutoTransformEnable)
            {
                Transform = Matrix.Identity
                            * Matrix.Scaling(scale)
                            * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                            * Matrix.Translation(translation);
            }
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
        ///     Actualiza el cuadro actual de animacion y renderiza la malla.
        ///     Es equivalente a llamar a updateAnimation() y luego a Render()
        /// </summary>
        public void animateAndRender(float elapsedTime)
        {
            if (!Enabled)
                return;

            updateAnimation(elapsedTime);
            render();
        }

        /// <summary>
        ///     Devuelve un array con todas las posiciones de los vértices de la malla, en el estado actual
        /// </summary>
        /// <returns>Array creado</returns>
        public Vector3[] getVertexPositions()
        {
            Vector3[] points = null;
            switch (RenderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var verts1 = (TgcKeyFrameLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcKeyFrameLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts1.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts1[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcKeyFrameLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcKeyFrameLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new Vector3[verts2.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts2[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;
            }
            return points;
        }

        /// <summary>
        ///     Calcula el BoundingBox de la malla, en base a todos sus vertices.
        ///     Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public TgcBoundingBox createBoundingBox()
        {
            //Obtener vertices en base al tipo de malla
            var points = getVertexPositions();
            boundingBox = TgcBoundingBox.computeFromPoints(points);
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
        ///     En modelos complejos puede resultar una operación poco performante.
        ///     La actualización será visible la próxima vez que se haga updateAnimation()
        ///     Si hay instnacias de este modelo, sea el original o una copia, todos los demás se verán
        ///     afectados
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public void setColor(Color color)
        {
            var c = color.ToArgb();
            for (var i = 0; i < originalData.verticesColors.Length; i++)
            {
                originalData.verticesColors[i] = c;
            }
        }

        /// <summary>
        ///     Permite cambiar las texturas de DiffuseMap de esta malla
        /// </summary>
        /// <param name="newDiffuseMaps">Array de nuevas texturas. Tiene que tener la misma cantidad que el original</param>
        public void changeDiffuseMaps(TgcTexture[] newDiffuseMaps)
        {
            //Solo aplicar si la malla tiene texturas
            if (RenderType == MeshRenderType.DIFFUSE_MAP)
            {
                if (DiffuseMaps.Length != newDiffuseMaps.Length)
                {
                    throw new Exception("The new DiffuseMap array does not have the same length than the original.");
                }

                //Liberar texturas anteriores
                foreach (var t in DiffuseMaps)
                {
                    t.dispose();
                }

                //Asignar nuevas texturas
                DiffuseMaps = newDiffuseMaps;
            }
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de esta malla original
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Solo se puede crear instancias a partir de originales.
        ///     Se debe crear después de haber agregado todas las animaciones al original.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcKeyFrameMesh createMeshInstance(string name, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (ParentInstance != null)
            {
                throw new Exception(
                    "No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            var instance = new TgcKeyFrameMesh(name, this, translation, rotation, scale);

            //BoundingBox
            instance.boundingBox = new TgcBoundingBox(boundingBox.PMin, boundingBox.PMax);
            instance.updateBoundingBox();

            instance.Enabled = true;
            return instance;
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de esta malla original
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Solo se puede crear instancias a partir de originales.
        ///     Se debe crear después de haber agregado todas las animaciones al original.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        public TgcKeyFrameMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, Vector3.Empty, Vector3.Empty, new Vector3(1, 1, 1));
        }

        public override string ToString()
        {
            return "Mesh: " + Name;
        }

        #region Eventos

        /// <summary>
        ///     Indica que la animación actual ha finalizado.
        ///     Se llama cuando se acabaron los frames de la animación.
        ///     Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        /// <param name="mesh">Malla animada</param>
        public delegate void AnimationEndsHandler(TgcKeyFrameMesh mesh);

        /// <summary>
        ///     Evento que se llama cada vez que la animación actual finaliza.
        ///     Se llama cuando se acabaron los frames de la animación.
        ///     Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        public event AnimationEndsHandler AnimationEnds;

        #endregion Eventos
    }
}