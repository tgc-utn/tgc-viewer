using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;
using TGC.Viewer.Utils.Shaders;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Viewer.Utils.TgcGeometry
{
    /// <summary>
    ///     Herramienta para un area de terreno de 4x4 caras en la que se puede editar la altura de sus vertices
    /// </summary>
    public class TgcEditableLand : IRenderObject, ITransformObject
    {
        private const float PATCH_SIZE = 20;

        private readonly EditableVertex[] editableVertices;

        private readonly VertexBuffer vertexBuffer;

        private readonly CustomVertex.PositionTextured[] vertices;

        protected Effect effect;

        private Vector3 rotation;

        private Vector3 scale;

        protected string technique;

        private Vector3 translation;

        /// <summary>
        ///     Crea el terreno
        /// </summary>
        public TgcEditableLand()
        {
            //16 caras, 32 triangulos, 96 vertices
            vertices = new CustomVertex.PositionTextured[96];
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), vertices.Length,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Crear los 25 vertices editables, formando una grilla de 5x5 vertices
            editableVertices = new EditableVertex[25];
            var uvStep = 1f / 4f;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var v = new EditableVertex();
                    v.Pos = new Vector3(j * PATCH_SIZE, 0, i * PATCH_SIZE);
                    v.UV = new Vector2(j * uvStep, i * uvStep);
                    editableVertices[i * 5 + j] = v;
                }
            }

            AutoTransformEnable = true;
            Transform = Matrix.Identity;
            translation = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1, 1, 1);
            Enabled = true;
            AlphaBlendEnable = false;
            UVOffset = new Vector2(0, 0);
            UVTiling = new Vector2(1, 1);

            //BoundingBox
            BoundingBox = new TgcBoundingBox();
            updateBoundingBox();

            //Shader
            effect = GuiController.Instance.Shaders.VariosShader;
            technique = TgcShaders.T_POSITION_TEXTURED;
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
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Textura del terreno
        /// </summary>
        public TgcTexture Texture { get; private set; }

        /// <summary>
        ///     Indica si el terreno esta habilitado para ser renderizado
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     BoundingBox del terreno
        /// </summary>
        public TgcBoundingBox BoundingBox { get; }

        /// <summary>
        ///     Offset UV de textura
        /// </summary>
        public Vector2 UVOffset { get; set; }

        /// <summary>
        ///     Tiling UV de textura
        /// </summary>
        public Vector2 UVTiling { get; set; }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar la caja
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            var texturesManager = GuiController.Instance.TexturesManager;

            //transformacion
            if (AutoTransformEnable)
            {
                Transform = Matrix.Scaling(scale) * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            Matrix.Translation(translation);
            }

            //Activar AlphaBlending
            activateAlphaBlend();

            //renderizar
            if (Texture != null)
            {
                texturesManager.shaderSet(effect, "texDiffuseMap", Texture);
            }
            else
            {
                texturesManager.clear(0);
            }

            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(effect, Transform);
            D3DDevice.Instance.Device.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            effect.Technique = technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 32);
            effect.EndPass();
            effect.End();

            //Desactivar AlphaBlend
            resetAlphaBlend();
        }

        /// <summary>
        ///     Liberar los recursos de la cja
        /// </summary>
        public void dispose()
        {
            if (Texture != null)
            {
                Texture.dispose();
            }
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
            BoundingBox.dispose();
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
        ///     Posicion absoluta del centro del terreno
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
        ///     Rotación absoluta del terreno
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        ///     Escalado absoluto del terreno
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
        ///     Variar la altura de un vertice de los 25 editables
        /// </summary>
        /// <param name="index">Indice del vertice del 0 al 24</param>
        /// <param name="y">altura del vertice a configurar</param>
        public void setVertexY(int index, float y)
        {
            editableVertices[index].setY(y);
        }

        /// <summary>
        ///     Variar la altura de los vertices especificados por sus indices
        /// </summary>
        /// <param name="indices">Indices de vertices del 0 al 24</param>
        /// <param name="y">altura del vertice a configurar</param>
        public void setVerticesY(int[] indices, float y)
        {
            for (var i = 0; i < indices.Length; i++)
            {
                setVertexY(indices[i], y);
            }
        }

        /// <summary>
        ///     Actualiza la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            //Crear grilla de 4x4 caras rectangulares
            var triangleSide1 = true;
            var vIndex = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    //Los 4 vertices que forman la cara
                    var tl = editableVertices[i * 5 + j];
                    var tr = editableVertices[i * 5 + j + 1];
                    var bl = editableVertices[(i + 1) * 5 + j];
                    var br = editableVertices[(i + 1) * 5 + j + 1];

                    if (triangleSide1)
                    {
                        //Sentido 1
                        //Primer triangulo: tl - br - bl
                        vertices[vIndex] = new CustomVertex.PositionTextured(tl.Pos, UVOffset.X + UVTiling.X * tl.UV.X,
                            UVOffset.Y + UVTiling.Y * tl.UV.Y);
                        vertices[vIndex + 1] = new CustomVertex.PositionTextured(br.Pos, UVOffset.X + UVTiling.X * br.UV.X,
                            UVOffset.Y + UVTiling.Y * br.UV.Y);
                        vertices[vIndex + 2] = new CustomVertex.PositionTextured(bl.Pos, UVOffset.X + UVTiling.X * bl.UV.X,
                            UVOffset.Y + UVTiling.Y * bl.UV.Y);

                        //Segundo triangulo: tl - tr - br
                        vertices[vIndex + 3] = new CustomVertex.PositionTextured(tl.Pos, UVOffset.X + UVTiling.X * tl.UV.X,
                            UVOffset.Y + UVTiling.Y * tl.UV.Y);
                        vertices[vIndex + 4] = new CustomVertex.PositionTextured(tr.Pos, UVOffset.X + UVTiling.X * tr.UV.X,
                            UVOffset.Y + UVTiling.Y * tr.UV.Y);
                        vertices[vIndex + 5] = new CustomVertex.PositionTextured(br.Pos, UVOffset.X + UVTiling.X * br.UV.X,
                            UVOffset.Y + UVTiling.Y * br.UV.Y);
                    }
                    else
                    {
                        //Sentido 2
                        //bl - tl - tr
                        vertices[vIndex] = new CustomVertex.PositionTextured(bl.Pos, UVOffset.X + UVTiling.X * bl.UV.X,
                            UVOffset.Y + UVTiling.Y * bl.UV.Y);
                        vertices[vIndex + 1] = new CustomVertex.PositionTextured(tl.Pos, UVOffset.X + UVTiling.X * tl.UV.X,
                            UVOffset.Y + UVTiling.Y * tl.UV.Y);
                        vertices[vIndex + 2] = new CustomVertex.PositionTextured(tr.Pos, UVOffset.X + UVTiling.X * tr.UV.X,
                            UVOffset.Y + UVTiling.Y * tr.UV.Y);

                        //bl - tr - br
                        vertices[vIndex + 3] = new CustomVertex.PositionTextured(bl.Pos, UVOffset.X + UVTiling.X * bl.UV.X,
                            UVOffset.Y + UVTiling.Y * bl.UV.Y);
                        vertices[vIndex + 4] = new CustomVertex.PositionTextured(tr.Pos, UVOffset.X + UVTiling.X * tr.UV.X,
                            UVOffset.Y + UVTiling.Y * tr.UV.Y);
                        vertices[vIndex + 5] = new CustomVertex.PositionTextured(br.Pos, UVOffset.X + UVTiling.X * br.UV.X,
                            UVOffset.Y + UVTiling.Y * br.UV.Y);
                    }
                    vIndex += 6;

                    //Invertir proximo sentido (salvo el ultimo de la fila)
                    if (j != 3)
                    {
                        triangleSide1 = !triangleSide1;
                    }
                }
            }

            vertexBuffer.SetData(vertices, 0, LockFlags.None);
            updateBoundingBox();
        }

        /// <summary>
        ///     Configurar textura de la pared
        /// </summary>
        public void setTexture(TgcTexture texture)
        {
            if (Texture != null)
            {
                Texture.dispose();
            }
            Texture = texture;
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
        ///     Actualiza el BoundingBox del terreno, en base a su posicion actual.
        ///     Solo contempla traslacion y escalado
        /// </summary>
        public void updateBoundingBox()
        {
            //Buscar extremos de altura
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            foreach (var v in editableVertices)
            {
                if (v.Pos.Y < minY)
                {
                    minY = v.Pos.Y;
                }
                if (v.Pos.Y > maxY)
                {
                    maxY = v.Pos.Y;
                }
            }

            BoundingBox.setExtremes(new Vector3(0, minY, 0), new Vector3(PATCH_SIZE * 4, maxY, PATCH_SIZE * 4));
            BoundingBox.scaleTranslate(translation, scale);
        }

        /// <summary>
        ///     Convierte el terreno en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            //Obtener matriz para transformar vertices
            if (AutoTransformEnable)
            {
                Transform = Matrix.Scaling(scale) * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            Matrix.Translation(translation);
            }

            //Crear Mesh con DiffuseMap
            var d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed,
                TgcSceneLoader.TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                for (var j = 0; j < vertices.Length; j++)
                {
                    var v = new TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex();
                    var vLand = vertices[j];

                    //vertices
                    v.Position = Vector3.TransformCoordinate(vLand.Position, Transform);

                    //normals
                    v.Normal = Vector3.Empty;

                    //texture coordinates diffuseMap
                    v.Tu = vLand.Tu;
                    v.Tv = vLand.Tv;

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
            tgcMesh.DiffuseMaps = new[] { Texture };
            tgcMesh.Materials = new[] { TgcD3dDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }

        /// <summary>
        ///     Crear un nuevo Terreno igual a este
        /// </summary>
        /// <returns>Terreno clonado</returns>
        public TgcEditableLand clone()
        {
            var cloneLand = new TgcEditableLand();

            //Cargar altura de cada vertice
            for (var i = 0; i < cloneLand.editableVertices.Length; i++)
            {
                cloneLand.editableVertices[i].Pos = editableVertices[i].Pos;
            }
            cloneLand.setTexture(Texture.clone());
            cloneLand.AutoTransformEnable = AutoTransformEnable;
            cloneLand.Transform = Transform;
            cloneLand.rotation = rotation;
            cloneLand.scale = scale;
            cloneLand.AlphaBlendEnable = AlphaBlendEnable;
            cloneLand.UVOffset = UVOffset;
            cloneLand.UVTiling = UVTiling;

            cloneLand.updateValues();
            return cloneLand;
        }

        /// <summary>
        ///     Vertice compartido por varias caras
        /// </summary>
        public class EditableVertex
        {
            private Vector3 pos;

            /// <summary>
            ///     Posicion
            /// </summary>
            public Vector3 Pos
            {
                get { return pos; }
                set { pos = value; }
            }

            /// <summary>
            ///     UV
            /// </summary>
            public Vector2 UV { get; set; }

            /// <summary>
            ///     Cargar altura en Y
            /// </summary>
            public void setY(float y)
            {
                pos.Y = y;
            }

            public override string ToString()
            {
                return "Pos: " + TgcParserUtils.printVector3(pos) + ", UV: " + TgcParserUtils.printVector2(UV);
            }
        }

        #region Seleccion de Vertices

        /// <summary>
        ///     Seleccion de vertice central
        /// </summary>
        public static readonly int[] SELECTION_CENTER = { 12 };

        /// <summary>
        ///     Seleccion de recuadro de vertices interior que rodea al centro
        /// </summary>
        public static readonly int[] SELECTION_INTERIOR_RING = { 6, 7, 8, 11, 13, 16, 17, 18 };

        /// <summary>
        ///     ///
        ///     <summary>
        ///         Seleccion de recuadro de vertices exterior que rodea al centro
        ///     </summary>
        /// </summary>
        public static readonly int[] SELECTION_EXTERIOR_RING = { 0, 1, 2, 3, 4, 5, 9, 10, 14, 15, 19, 20, 21, 22, 23, 24 };

        /// <summary>
        ///     Seleccion de vertices del lado exterior superior
        /// </summary>
        public static readonly int[] SELECTION_TOP_SIDE = { 0, 1, 2, 3, 4 };

        /// <summary>
        ///     Seleccion de vertices del lado exterior izquierdo
        /// </summary>
        public static readonly int[] SELECTION_LEFT_SIDE = { 0, 5, 10, 15, 20 };

        /// <summary>
        ///     Seleccion de vertices del lado exterior derecho
        /// </summary>
        public static readonly int[] SELECTION_RIGHT_SIDE = { 4, 9, 14, 19, 14 };

        /// <summary>
        ///     Seleccion de vertices del lado exterior inferior
        /// </summary>
        public static readonly int[] SELECTION_BOTTOM_SIDE = { 20, 21, 22, 23, 24 };

        #endregion Seleccion de Vertices
    }
}