using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.SkeletalAnimation
{
    /// <summary>
    ///     Malla que representa un modelo 3D con varias animaciones, animadas por Skeletal Animation
    /// </summary>
    public class TgcSkeletalMesh : IRenderObject, ITransformObject
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

        /// <summary>
        ///     Maxima cantidad de huesos soportados por TgcSkeletalMesh
        ///     Coincide con el tamaño del array de matrices que se envia
        ///     al VertexShader para hacer skinning.
        /// </summary>
        public const int MAX_BONE_COUNT = 26;

        protected Dictionary<string, TgcSkeletalAnimation> animations;

        //Variables de animacion
        protected float animationTimeLenght;

        protected List<TgcSkeletalBoneAttach> attachments;

        protected bool autoUpdateBoundingBox;

        protected TgcSkeletalBone[] bones;

        //Matrices final de transformacion de cada ueso
        private TGCMatrix[] boneSpaceFinalTransforms;

        protected TgcBoundingAxisAlignBox boundingBox;

        protected TgcSkeletalAnimation currentAnimation;

        protected int currentFrame;

        protected float currentTime;

        /// <summary>
        ///     Mesh de DirectX
        /// </summary>
        protected Mesh d3dMesh;

        protected TgcTexture[] diffuseMaps;

        protected Effect effect;

        protected bool enabled;

        protected float frameRate;

        protected bool isAnimating;

        protected Material[] materials;

        protected List<TgcSkeletalMesh> meshInstances;

        protected string name;

        protected TgcSkeletalMesh parentInstance;

        protected bool playLoop;

        protected bool renderSkeleton;

        protected MeshRenderType renderType;

        protected TGCVector3 rotation;

        protected TGCVector3 scale;

        protected TgcLine[] skeletonRenderBones;

        //Elementos para renderizar el esqueleto
        protected TGCBox[] skeletonRenderJoints;

        /// <summary>
        ///     BoundingBox de la malla sin ninguna animación.
        /// </summary>
        protected TgcBoundingAxisAlignBox staticMeshBoundingBox;

        protected string technique;

        protected TGCMatrix transform;

        protected TGCVector3 translation;

        protected VertexDeclaration vertexDeclaration;

        /// <summary>
        ///     Constructor vacio, para facilitar la herencia de esta clase.
        /// </summary>
        protected TgcSkeletalMesh()
        {
        }

        /// <summary>
        ///     Crea una nueva malla.
        /// </summary>
        /// <param name="mesh">Mesh de DirectX</param>
        /// <param name="renderType">Formato de renderizado de la malla</param>
        /// <param name="bones">Datos de los huesos</param>
        public TgcSkeletalMesh(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
        {
            initData(mesh, name, renderType, bones);
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
        public TgcSkeletalMesh(string name, TgcSkeletalMesh parentInstance, TGCVector3 translation, TGCVector3 rotation,
            TGCVector3 scale)
        {
            //Cargar iniciales datos en base al original
            initData(parentInstance.d3dMesh, name, parentInstance.renderType, parentInstance.bones);
            diffuseMaps = parentInstance.diffuseMaps;
            materials = parentInstance.materials;

            //Almacenar transformación inicial
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            //Agregar animaciones del original
            foreach (var entry in parentInstance.animations)
            {
                animations.Add(entry.Key, entry.Value);
            }

            //Agregar attachments del original, creando una instancia por cada attach
            foreach (var parentAttach in parentInstance.attachments)
            {
                var attach = new TgcSkeletalBoneAttach();
                attach.Bone = parentAttach.Bone;
                attach.Offset = parentAttach.Offset;
                //Crear instancia del mesh del attach del padre
                attach.Mesh = parentAttach.Mesh.createMeshInstance(name + "-" + parentAttach.Mesh.Name);
                attach.updateValues();
                attachments.Add(attach);
            }

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
            get { return name; }
        }

        /// <summary>
        ///     Array de Materials
        /// </summary>
        public Material[] Materials
        {
            get { return materials; }
            set { materials = value; }
        }

        /// <summary>
        ///     Array de texturas para DiffuseMap
        /// </summary>
        public TgcTexture[] DiffuseMaps
        {
            get { return diffuseMaps; }
            set { diffuseMaps = value; }
        }

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
        ///     Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        ///     BoundingBox del Mesh.
        ///     Puede variar según la animación que tiene configurada en el momento.
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox
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
        public bool AutoUpdateBoundingBox
        {
            get { return autoUpdateBoundingBox; }
            set { autoUpdateBoundingBox = value; }
        }

        /// <summary>
        ///     Huesos del esqueleto de la malla. Ordenados en forma jerárquica
        /// </summary>
        public TgcSkeletalBone[] Bones
        {
            get { return bones; }
        }

        /// <summary>
        ///     Mapa de animaciones de la malla
        /// </summary>
        public Dictionary<string, TgcSkeletalAnimation> Animations
        {
            get { return animations; }
        }

        /// <summary>
        ///     Animación actual de la malla
        /// </summary>
        public TgcSkeletalAnimation CurrentAnimation
        {
            get { return currentAnimation; }
        }

        /// <summary>
        ///     Velocidad de la animacion medida en cuadros por segundo.
        /// </summary>
        public float FrameRate
        {
            get { return frameRate; }
        }

        /// <summary>
        ///     Cuadro actual de animacion
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        /// <summary>
        ///     Indica si actualmente hay una animación en curso.
        /// </summary>
        public bool IsAnimating
        {
            get { return isAnimating; }
        }

        /// <summary>
        ///     Indica si la animación actual se ejecuta con un Loop
        /// </summary>
        public bool PlayLoop
        {
            get { return playLoop; }
        }

        /// <summary>
        ///     Modelos adjuntados para seguir la trayectoria de algún hueso
        /// </summary>
        public List<TgcSkeletalBoneAttach> Attachments
        {
            get { return attachments; }
        }

        /// <summary>
        ///     Original desde el cual esta malla fue clonada.
        /// </summary>
        public TgcSkeletalMesh ParentInstance
        {
            get { return parentInstance; }
        }

        /// <summary>
        ///     Lista de mallas que fueron clonadas a partir de este original
        /// </summary>
        public List<TgcSkeletalMesh> MeshInstances
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
        ///     En true renderiza solo el esqueleto del modelo, en lugar de la malla.
        /// </summary>
        public bool RenderSkeleton
        {
            get { return renderSkeleton; }
            set { renderSkeleton = value; }
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
        public void Render()
        {
            if (!enabled)
                return;

            //Actualizar transformacion de malla con el default.
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

            //Enviar al shader el array de matrices de huesos para poder hacer skinning en el Vertex Shader
            effect.SetValue("bonesMatWorldArray", TGCMatrix.ToMatrixArray(boneSpaceFinalTransforms));

            //Renderizar malla
            if (!renderSkeleton)
            {
                //Renderizar segun el tipo de Render de la malla
                effect.Technique = technique;
                var numPasses = effect.Begin(0);
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
                            for (var i = 0; i < materials.Length; i++)
                            {
                                //Setear textura en shader
                                TexturesManager.Instance.shaderSet(effect, "texDiffuseMap", diffuseMaps[i]);

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
            }
            //Renderizar esqueleto
            else
            {
                renderSkeletonMesh();
            }

            //Desactivar alphaBlend
            resetAlphaBlend();

            //Renderizar attachments
            foreach (var attach in attachments)
            {
                attach.updateMeshTransform(transform);
                attach.Mesh.Render();
            }
        }

        /// <summary>
        ///     Libera los recursos de la malla
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
            foreach (var meshInstance in meshInstances)
            {
                meshInstance.Dispose();
            }
            meshInstances = null;

            //Dispose de mesh
            d3dMesh.Dispose();
            d3dMesh = null;

            //Dispose de texturas
            if (diffuseMaps != null)
            {
                for (var i = 0; i < diffuseMaps.Length; i++)
                {
                    diffuseMaps[i].dispose();
                }
                diffuseMaps = null;
            }

            //Dispose de Box de joints
            if (skeletonRenderJoints != null)
            {
                foreach (var jointBox in skeletonRenderJoints)
                {
                    jointBox.Dispose();
                }
                skeletonRenderJoints = null;
            }

            //Dispose de lineas de Bones
            if (skeletonRenderBones != null)
            {
                foreach (var boneLine in skeletonRenderBones)
                {
                    if (boneLine != null)
                    {
                        boneLine.Dispose();
                    }
                }
                skeletonRenderBones = null;
            }

            //Liberar attachments
            foreach (var attach in attachments)
            {
                attach.Mesh.Dispose();
            }
            attachments = null;

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
            var z = (float)Math.Cos(rotation.Y) * movement;
            var x = (float)Math.Sin(rotation.Y) * movement;

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
        protected void initData(Mesh mesh, string name, MeshRenderType renderType, TgcSkeletalBone[] bones)
        {
            d3dMesh = mesh;
            this.name = name;
            this.renderType = renderType;
            enabled = false;
            autoUpdateBoundingBox = true;
            this.bones = bones;
            attachments = new List<TgcSkeletalBoneAttach>();
            meshInstances = new List<TgcSkeletalMesh>();
            renderSkeleton = false;
            AlphaBlendEnable = false;

            //variables de movimiento
            AutoTransformEnable = false;
            translation = new TGCVector3(0f, 0f, 0f);
            rotation = new TGCVector3(0f, 0f, 0f);
            scale = new TGCVector3(1f, 1f, 1f);
            transform = TGCMatrix.Identity;

            //variables de animacion
            isAnimating = false;
            currentAnimation = null;
            playLoop = false;
            frameRate = 0f;
            currentTime = 0f;
            animationTimeLenght = 0f;
            currentFrame = 0;
            animations = new Dictionary<string, TgcSkeletalAnimation>();

            //Matrices de huesos
            boneSpaceFinalTransforms = new TGCMatrix[MAX_BONE_COUNT];

            //Shader
            vertexDeclaration = new VertexDeclaration(mesh.Device, mesh.Declaration);
            effect = TGCShaders.Instance.TgcSkeletalMeshShader;
            technique = TGCShaders.Instance.GetTGCSkeletalMeshTechnique(this.renderType);

            //acomodar huesos
            setupSkeleton();
        }

        /// <summary>
        ///     Configuracion inicial del esquleto
        /// </summary>
        protected void setupSkeleton()
        {
            //Actualizar jerarquia
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                //Es hijo o padre
                if (bone.ParentBone == null)
                {
                    bone.MatFinal = bone.MatLocal;
                }
                else
                {
                    //Multiplicar por la matriz del padre
                    bone.MatFinal = bone.MatLocal * bone.ParentBone.MatFinal;
                }

                //Almacenar la inversa de la posicion original del hueso, para la referencia inicial de los vertices
                bone.MatInversePose = TGCMatrix.Invert(bone.MatFinal);
            }
        }

        /// <summary>
        ///     Crea mallas a modo Debug para visualizar la configuración del esqueleto
        /// </summary>
        public void buildSkletonMesh()
        {
            //Crear array para dibujar los huesos y joints
            var jointsColor = Color.Violet;
            var bonesColor = Color.Yellow;
            var jointsSize = new TGCVector3(2, 2, 2);
            var ceroVec = TGCVector3.Empty;
            skeletonRenderJoints = new TGCBox[bones.Length];
            skeletonRenderBones = new TgcLine[bones.Length];
            var boneColor = Color.Yellow.ToArgb();

            //Actualizar jerarquia
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                //Es hijo o padre
                if (bone.ParentBone == null)
                {
                    skeletonRenderBones[i] = null;
                }
                else
                {
                    //Crear linea de hueso para renderziar esqueleto
                    var boneLine = new TgcLine();
                    boneLine.PStart = TGCVector3.transform(ceroVec, bone.MatFinal);
                    boneLine.PEnd = TGCVector3.transform(ceroVec, bone.ParentBone.MatFinal);
                    boneLine.Color = bonesColor;
                    skeletonRenderBones[i] = boneLine;
                }
                //Crear malla de Joint para renderizar el esqueleto
                var jointBox = TGCBox.fromSize(jointsSize, jointsColor);
                jointBox.AutoTransformEnable = false;
                skeletonRenderJoints[i] = jointBox;
            }
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
            if (isAnimating)
            {
                //Si la animacion pedida es la misma que la actual no la quitamos
                if (currentAnimation.Name.Equals(animationName))
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
        protected void initAnimationSettings(string animationName, bool playLoop, float userFrameRate)
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
                frameRate = currentAnimation.FrameRate;
            }
            else
            {
                frameRate = userFrameRate;
            }

            //La duracion de la animacion.
            animationTimeLenght = ((float)currentAnimation.FramesCount - 1) / frameRate;

            //Configurar postura inicial de los huesos
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                if (!currentAnimation.hasFrames(i))
                {
                    throw new Exception("El hueso " + bone.Name + " no posee KeyFrames");
                }

                //Determinar matriz local inicial
                var firstFrame = currentAnimation.BoneFrames[i][0];
                bone.MatLocal = TGCMatrix.RotationTGCQuaternion(firstFrame.Rotation) * TGCMatrix.Translation(firstFrame.Position);

                //Multiplicar por matriz del padre, si tiene
                if (bone.ParentBone != null)
                {
                    bone.MatFinal = bone.MatLocal * bone.ParentBone.MatFinal;
                }
                else
                {
                    bone.MatFinal = bone.MatLocal;
                }
            }

            //Ajustar vertices a posicion inicial del esqueleto
            updateMeshVertices();
        }

        /// <summary>
        ///     Desactiva la animacion actual
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
                if (playLoop)
                {
                    //Dejar el remanente de tiempo transcurrido para el proximo loop
                    currentTime = currentTime % animationTimeLenght;
                    //setSkleletonLastPose();
                    //updateMeshVertices();
                }
                else
                {
                    //TODO: Puede ser que haya que quitar este stopAnimation() y solo llamar al Listener (sin cargar isAnimating = false)

                    stopAnimation();
                }
            }

            //La animacion continua
            else
            {
                //Actualizar esqueleto y malla
                updateSkeleton();
                updateMeshVertices();
            }
        }

        /// <summary>
        ///     Actualiza la posicion de cada hueso del esqueleto segun sus KeyFrames de la animacion
        /// </summary>
        protected void updateSkeleton()
        {
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                //Tomar el frame actual para este hueso
                var boneFrames = currentAnimation.BoneFrames[i];

                //Solo hay un frame, no hacer nada, ya se hizo en el Init de la animacion
                if (boneFrames.Count == 1)
                {
                    continue;
                }

                //Obtener cuadro actual segun el tiempo transcurrido
                var currentFrameF = currentTime * frameRate;
                //Ve a que KeyFrame le corresponde
                var keyFrameIdx = getCurrentFrameBone(boneFrames, currentFrameF);
                currentFrame = keyFrameIdx;

                //Armar un intervalo entre el proximo KeyFrame y el anterior
                var frame1 = boneFrames[keyFrameIdx - 1];
                var frame2 = boneFrames[keyFrameIdx];

                //Calcular la cantidad que hay interpolar en base al la diferencia entre cuadros
                float framesDiff = frame2.Frame - frame1.Frame;
                var interpolationValue = (currentFrameF - frame1.Frame) / framesDiff;

                //Interpolar traslacion
                var frameTranslation = (frame2.Position - frame1.Position) * interpolationValue + frame1.Position;

                //Interpolar rotacion con SLERP
                var quatFrameRotation = TGCQuaternion.Slerp(frame1.Rotation, frame2.Rotation, interpolationValue);

                //Unir ambas transformaciones de este frame
                var frameMatrix = TGCMatrix.RotationTGCQuaternion(quatFrameRotation) * TGCMatrix.Translation(frameTranslation);

                //Multiplicar por la matriz del padre, si tiene
                if (bone.ParentBone != null)
                {
                    bone.MatFinal = frameMatrix * bone.ParentBone.MatFinal;
                }
                else
                {
                    bone.MatFinal = frameMatrix;
                }
            }
        }

        /// <summary>
        ///     Obtener el KeyFrame correspondiente a cada hueso segun el tiempo transcurrido
        /// </summary>
        protected int getCurrentFrameBone(List<TgcSkeletalAnimationFrame> boneFrames, float currentFrame)
        {
            for (var i = 0; i < boneFrames.Count; i++)
            {
                if (currentFrame < boneFrames[i].Frame)
                {
                    return i;
                }
            }

            return boneFrames.Count - 1;
        }

        /// <summary>
        ///     Actualizar los vertices de la malla segun las posiciones del los huesos del esqueleto
        /// </summary>
        protected void updateMeshVertices()
        {
            //Precalcular la multiplicación para llevar a un vertice a Bone-Space y luego transformarlo segun el hueso
            //Estas matrices se envian luego al Vertex Shader para hacer skinning en GPU
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                boneSpaceFinalTransforms[i] = bone.MatInversePose * bone.MatFinal;
            }
        }

        /// <summary>
        ///     Cargar todas la matrices que necesita el shader
        /// </summary>
        protected void setShaderMatrix()
        {
            TGCShaders.Instance.SetShaderMatrix(effect, transform);
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
        ///     Dibujar el esqueleto de la malla
        /// </summary>
        protected void renderSkeletonMesh()
        {
            var ceroVec = TGCVector3.Empty;

            //Dibujar huesos y joints
            for (var i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                //Renderizar Joint
                var jointBox = skeletonRenderJoints[i];
                jointBox.Transform = bone.MatFinal * transform;
                jointBox.Render();

                //Modificar línea del bone
                if (bone.ParentBone != null)
                {
                    var boneLine = skeletonRenderBones[i];

                    boneLine.PStart = TGCVector3.transform(ceroVec, bone.MatFinal * transform);
                    boneLine.PEnd = TGCVector3.transform(ceroVec, bone.ParentBone.MatFinal * transform);
                    boneLine.updateValues();
                }
            }

            //Dibujar bones
            foreach (var boneLine in skeletonRenderBones)
            {
                if (boneLine != null)
                {
                    boneLine.Render();
                }
            }
        }

        /// <summary>
        ///     Actualiza el cuadro actual de animacion y renderiza la malla.
        ///     Es equivalente a llamar a updateAnimation() y luego a Render()
        /// </summary>
        public void animateAndRender(float elapsedTime)
        {
            if (!enabled)
                return;

            updateAnimation(elapsedTime);
            Render();
        }

        /// <summary>
        ///     Devuelve un array con todas las posiciones de los vértices de la malla, en el estado actual
        /// </summary>
        /// <returns>Array creado</returns>
        public TGCVector3[] getVertexPositions()
        {
            TGCVector3[] points = null;
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var verts1 = (TgcSkeletalLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSkeletalLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new TGCVector3[verts1.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        points[i] = verts1[i].Position;
                    }
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcSkeletalLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSkeletalLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    points = new TGCVector3[verts2.Length];
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
        ///     Recalcula todas las normales del mesh
        /// </summary>
        public void computeNormals()
        {
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    throw new NotImplementedException("Falta hacer");

                case MeshRenderType.DIFFUSE_MAP:

                    //Calcular normales usando DirectX
                    var adj = new int[d3dMesh.NumberFaces * 3];
                    d3dMesh.GenerateAdjacency(0, adj);
                    d3dMesh.ComputeNormals(adj);

                    //Obtener vertexBuffer original
                    var origVertexBuffer = (TgcSkeletalLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSkeletalLoader.DiffuseMapVertex), LockFlags.Discard, d3dMesh.NumberVertices);

                    //Calcular normales recorriendo los triangulos
                    var triCount = origVertexBuffer.Length / 3;
                    var normals = new TGCVector3[origVertexBuffer.Length];
                    for (var i = 0; i < normals.Length; i++)
                    {
                        normals[i] = TGCVector3.Empty;
                    }
                    for (var i = 0; i < triCount; i++)
                    {
                        //Los 3 vertices del triangulo
                        /*
                        TgcSkeletalLoader.DiffuseMapVertex v1 = origVertexBuffer[i * 3];
                        TgcSkeletalLoader.DiffuseMapVertex v2 = origVertexBuffer[i * 3 + 1];
                        TgcSkeletalLoader.DiffuseMapVertex v3 = origVertexBuffer[i * 3 + 2];

                        //Face-normal (left-handend)
                        TGCVector3 a = v2.Position - v1.Position;
                        TGCVector3 b = v3.Position - v1.Position;
                        TGCVector3 n = TGCVector3.Cross(b,a);

                        //Normalizar
                        n.Normalize();

                        //Cargar normal
                        origVertexBuffer[i * 3].Normal = n;
                        origVertexBuffer[i * 3 + 1].Normal = n;
                        origVertexBuffer[i * 3 + 2].Normal = n;
                         */

                        //Invetir normal
                        origVertexBuffer[i * 3].Normal = -origVertexBuffer[i * 3].Normal;
                        origVertexBuffer[i * 3 + 1].Normal = -origVertexBuffer[i * 3 + 1].Normal;
                        origVertexBuffer[i * 3 + 2].Normal = -origVertexBuffer[i * 3 + 2].Normal;
                    }

                    //Aplicar cambios
                    d3dMesh.SetVertexBufferData(origVertexBuffer, LockFlags.Discard);
                    d3dMesh.UnlockVertexBuffer();

                    break;
            }
        }

        /// <summary>
        ///     Calcula el BoundingBox de la malla, en base a todos sus vertices.
        ///     Llamar a este metodo cuando ha cambiado la estructura interna de la malla.
        /// </summary>
        public TgcBoundingAxisAlignBox createBoundingBox()
        {
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
            if (autoUpdateBoundingBox)
            {
                boundingBox.scaleTranslate(translation, scale);
            }
        }

        /// <summary>
        ///     Cambia el color de todos los vértices de la malla, actualizando el VertexBuffer
        ///     En modelos complejos puede resultar una operación poco performante.
        ///     La actualización será visible la próxima vez que se haga updateAnimation().
        ///     Si hay instnacias de este modelo, sea el original o una copia, todos los demás se verán
        ///     afectados
        /// </summary>
        /// <param name="color">Color nuevo</param>
        public void setColor(Color color)
        {
            var c = color.ToArgb();
            switch (renderType)
            {
                case MeshRenderType.VERTEX_COLOR:
                    var verts1 = (TgcSkeletalLoader.VertexColorVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSkeletalLoader.VertexColorVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (var i = 0; i < verts1.Length; i++)
                    {
                        verts1[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts1, LockFlags.None);
                    d3dMesh.UnlockVertexBuffer();
                    break;

                case MeshRenderType.DIFFUSE_MAP:
                    var verts2 = (TgcSkeletalLoader.DiffuseMapVertex[])d3dMesh.LockVertexBuffer(
                        typeof(TgcSkeletalLoader.DiffuseMapVertex), LockFlags.ReadOnly, d3dMesh.NumberVertices);
                    for (var i = 0; i < verts2.Length; i++)
                    {
                        verts2[i].Color = c;
                    }
                    d3dMesh.SetVertexBufferData(verts2, LockFlags.None);
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
            if (renderType == MeshRenderType.DIFFUSE_MAP)
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
        ///     Busca el hueso con el nombre especificado.
        /// </summary>
        /// <param name="boneName">Nombre del hueso buscado</param>
        /// <returns>Hueso encontrado o null si no lo encontró</returns>
        public TgcSkeletalBone getBoneByName(string boneName)
        {
            foreach (var bone in bones)
            {
                if (bone.Name.Equals(boneName))
                {
                    return bone;
                }
            }
            return null;
        }

        /// <summary>
        ///     Crea una nueva malla que es una instancia de esta malla original
        ///     Reutiliza toda la geometría de la malla original sin duplicarla.
        ///     Solo se puede crear instancias a partir de originales.
        ///     Se debe crear después de haber agregado todas las animaciones al original.
        ///     Los attachments de la malla original se duplican.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        /// <param name="translation">Traslación respecto de la malla original</param>
        /// <param name="rotation">Rotación respecto de la malla original</param>
        /// <param name="scale">Escala respecto de la malla original</param>
        public TgcSkeletalMesh createMeshInstance(string name, TGCVector3 translation, TGCVector3 rotation, TGCVector3 scale)
        {
            if (parentInstance != null)
            {
                throw new Exception(
                    "No se puede crear una instancia de otra malla instancia. Hay que partir del original.");
            }

            //Crear instancia
            var instance = new TgcSkeletalMesh(name, this, translation, rotation, scale);

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
        ///     Se debe crear después de haber agregado todas las animaciones al original.
        ///     Los attachments de la malla original se duplican.
        /// </summary>
        /// <param name="name">Nombre de la malla</param>
        public TgcSkeletalMesh createMeshInstance(string name)
        {
            return createMeshInstance(name, TGCVector3.Empty, TGCVector3.Empty, TGCVector3.One);
        }

        public override string ToString()
        {
            return "Mesh: " + name;
        }

        #region Eventos

        /// <summary>
        ///     Indica que la animación actual ha finalizado.
        ///     Se llama cuando se acabaron los frames de la animación.
        ///     Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        /// <param name="mesh">Malla animada</param>
        public delegate void AnimationEndsHandler(TgcSkeletalMesh mesh);

        /// <summary>
        ///     Evento que se llama cada vez que la animación actual finaliza.
        ///     Se llama cuando se acabaron los frames de la animación.
        ///     Si se anima en Loop, se llama cada vez que termina.
        /// </summary>
        public event AnimationEndsHandler AnimationEnds;

        #endregion Eventos
    }
}