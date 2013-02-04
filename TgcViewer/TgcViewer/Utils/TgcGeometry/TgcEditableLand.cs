using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Herramienta para un area de terreno de 4x4 caras en la que se puede editar la altura de sus vertices
    /// </summary>
    public class TgcEditableLand : IRenderObject, ITransformObject
    {

        #region Seleccion de Vertices

        /// <summary>
        /// Seleccion de vertice central
        /// </summary>
        public static readonly int[] SELECTION_CENTER = new int[]{12};

        /// <summary>
        /// Seleccion de recuadro de vertices interior que rodea al centro
        /// </summary>
        public static readonly int[] SELECTION_INTERIOR_RING = new int[] { 6, 7, 8, 11, 13, 16, 17, 18 };

        /// <summary>
        /// /// <summary>
        /// Seleccion de recuadro de vertices exterior que rodea al centro
        /// </summary>
        /// </summary>
        public static readonly int[] SELECTION_EXTERIOR_RING = new int[] { 0, 1, 2, 3, 4, 5, 9, 10, 14, 15, 19, 20, 21, 22, 23, 24 };

        /// <summary>
        /// Seleccion de vertices del lado exterior superior
        /// </summary>
        public static readonly int[] SELECTION_TOP_SIDE = new int[] { 0, 1, 2, 3, 4 };

        /// <summary>
        /// Seleccion de vertices del lado exterior izquierdo
        /// </summary>
        public static readonly int[] SELECTION_LEFT_SIDE = new int[] { 0, 5, 10, 15, 20 };

        /// <summary>
        /// Seleccion de vertices del lado exterior derecho
        /// </summary>
        public static readonly int[] SELECTION_RIGHT_SIDE = new int[] { 4, 9, 14, 19, 14 };

        /// <summary>
        /// Seleccion de vertices del lado exterior inferior
        /// </summary>
        public static readonly int[] SELECTION_BOTTOM_SIDE = new int[] { 20, 21, 22, 23, 24 };

        #endregion



        const float PATCH_SIZE = 20;

        CustomVertex.PositionTextured[] vertices;
        VertexBuffer vertexBuffer;
        EditableVertex[] editableVertices;


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

        TgcTexture texture;
        /// <summary>
        /// Textura del terreno
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
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
        /// Posicion absoluta del centro del terreno
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

        private Vector3 rotation;
        /// <summary>
        /// Rotación absoluta del terreno
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        private Vector3 scale;
        /// <summary>
        /// Escalado absoluto del terreno
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

        private bool enabled;
        /// <summary>
        /// Indica si el terreno esta habilitado para ser renderizado
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }


        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox del terreno
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
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

        Vector2 uvOffset;
        /// <summary>
        /// Offset UV de textura
        /// </summary>
        public Vector2 UVOffset
        {
            get { return uvOffset; }
            set { uvOffset = value; }
        }

        Vector2 uvTiling;
        /// <summary>
        /// Tiling UV de textura
        /// </summary>
        public Vector2 UVTiling
        {
            get { return uvTiling; }
            set { uvTiling = value; }
        }


        /// <summary>
        /// Crea el terreno
        /// </summary>
        public TgcEditableLand()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //16 caras, 32 triangulos, 96 vertices
            vertices = new CustomVertex.PositionTextured[96];
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), vertices.Length, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            //Crear los 25 vertices editables, formando una grilla de 5x5 vertices
            editableVertices = new EditableVertex[25];
            float uvStep = 1f / 4f;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    EditableVertex v = new EditableVertex();
                    v.Pos = new Vector3(j * PATCH_SIZE, 0, i * PATCH_SIZE);
                    v.UV = new Vector2(j * uvStep, i * uvStep);
                    editableVertices[i * 5 + j] = v;
                }
            }


            this.autoTransformEnable = true;
            this.transform = Matrix.Identity;
            this.translation = new Vector3(0,0,0);
            this.rotation = new Vector3(0,0,0);
            this.scale = new Vector3(1, 1, 1);
            this.enabled = true;
            this.alphaBlendEnable = false;
            this.uvOffset = new Vector2(0, 0);
            this.uvTiling = new Vector2(1, 1);

            //BoundingBox
            boundingBox = new TgcBoundingBox();
            updateBoundingBox();

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        /// Variar la altura de un vertice de los 25 editables
        /// </summary>
        /// <param name="index">Indice del vertice del 0 al 24</param>
        /// <param name="y">altura del vertice a configurar</param>
        public void setVertexY(int index, float y)
        {
            editableVertices[index].setY(y);
        }

        /// <summary>
        /// Variar la altura de los vertices especificados por sus indices
        /// </summary>
        /// <param name="indices">Indices de vertices del 0 al 24</param>
        /// <param name="y">altura del vertice a configurar</param>
        public void setVerticesY(int[] indices, float y)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                setVertexY(indices[i], y);
            }
        }

        /// <summary>
        /// Actualiza la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            //Crear grilla de 4x4 caras rectangulares
            bool triangleSide1 = true;
            int vIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //Los 4 vertices que forman la cara
                    EditableVertex tl = editableVertices[i * 5 + j];
                    EditableVertex tr = editableVertices[i * 5 + j + 1];
                    EditableVertex bl = editableVertices[(i + 1) * 5 + j];
                    EditableVertex br = editableVertices[(i + 1) * 5 + j + 1];

                    if (triangleSide1)
                    {
                        //Sentido 1
                        //Primer triangulo: tl - br - bl                        
                        vertices[vIndex] = new CustomVertex.PositionTextured(tl.Pos, uvOffset.X + uvTiling.X * tl.UV.X, uvOffset.Y + uvTiling.Y * tl.UV.Y);
                        vertices[vIndex + 1] = new CustomVertex.PositionTextured(br.Pos, uvOffset.X + uvTiling.X * br.UV.X, uvOffset.Y + uvTiling.Y * br.UV.Y);
                        vertices[vIndex + 2] = new CustomVertex.PositionTextured(bl.Pos, uvOffset.X + uvTiling.X * bl.UV.X, uvOffset.Y + uvTiling.Y * bl.UV.Y);

                        //Segundo triangulo: tl - tr - br
                        vertices[vIndex + 3] = new CustomVertex.PositionTextured(tl.Pos, uvOffset.X + uvTiling.X * tl.UV.X, uvOffset.Y + uvTiling.Y * tl.UV.Y);
                        vertices[vIndex + 4] = new CustomVertex.PositionTextured(tr.Pos, uvOffset.X + uvTiling.X * tr.UV.X, uvOffset.Y + uvTiling.Y * tr.UV.Y);
                        vertices[vIndex + 5] = new CustomVertex.PositionTextured(br.Pos, uvOffset.X + uvTiling.X * br.UV.X, uvOffset.Y + uvTiling.Y * br.UV.Y);
                    }
                    else
                    {
                        //Sentido 2
                        //bl - tl - tr
                        vertices[vIndex] = new CustomVertex.PositionTextured(bl.Pos, uvOffset.X + uvTiling.X * bl.UV.X, uvOffset.Y + uvTiling.Y * bl.UV.Y);
                        vertices[vIndex + 1] = new CustomVertex.PositionTextured(tl.Pos, uvOffset.X + uvTiling.X * tl.UV.X, uvOffset.Y + uvTiling.Y * tl.UV.Y);
                        vertices[vIndex + 2] = new CustomVertex.PositionTextured(tr.Pos, uvOffset.X + uvTiling.X * tr.UV.X, uvOffset.Y + uvTiling.Y * tr.UV.Y);

                        //bl - tr - br
                        vertices[vIndex + 3] = new CustomVertex.PositionTextured(bl.Pos, uvOffset.X + uvTiling.X * bl.UV.X, uvOffset.Y + uvTiling.Y * bl.UV.Y);
                        vertices[vIndex + 4] = new CustomVertex.PositionTextured(tr.Pos, uvOffset.X + uvTiling.X * tr.UV.X, uvOffset.Y + uvTiling.Y * tr.UV.Y);
                        vertices[vIndex + 5] = new CustomVertex.PositionTextured(br.Pos, uvOffset.X + uvTiling.X * br.UV.X, uvOffset.Y + uvTiling.Y * br.UV.Y);
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
        /// Configurar textura de la pared
        /// </summary>
        public void setTexture(TgcTexture texture)
        {
            if (this.texture != null)
            {
                this.texture.dispose();
            }
            this.texture = texture;
        }


        /// <summary>
        /// Renderizar la caja
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            //transformacion
            if (autoTransformEnable)
            {
                this.transform = Matrix.Scaling(scale) * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.Translation(translation);
            }

            //Activar AlphaBlending
            activateAlphaBlend();

            //renderizar
            if (texture != null)
            {
                texturesManager.shaderSet(effect, "texDiffuseMap", texture);
            }
            else
            {
                texturesManager.clear(0);
            }

            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(this.effect, this.transform);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);
            

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 32);
            effect.EndPass();
            effect.End();

            //Desactivar AlphaBlend
            resetAlphaBlend();
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
        /// Liberar los recursos de la cja
        /// </summary>
        public void dispose()
        {
            if (texture != null)
            {
                texture.dispose();
            }
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
            boundingBox.dispose();
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
        /// Actualiza el BoundingBox del terreno, en base a su posicion actual.
        /// Solo contempla traslacion y escalado
        /// </summary>
        public void updateBoundingBox()
        {
            //Buscar extremos de altura
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (EditableVertex v in editableVertices)
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

            boundingBox.setExtremes(new Vector3(0, minY, 0), new Vector3(PATCH_SIZE * 4, maxY, PATCH_SIZE * 4));
            boundingBox.scaleTranslate(this.translation, this.scale);
        }


        /// <summary>
        /// Convierte el terreno en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener matriz para transformar vertices
            if (autoTransformEnable)
            {
                this.transform = Matrix.Scaling(scale) * Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.Translation(translation);
            }

            //Crear Mesh con DiffuseMap
            Mesh d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed, TgcSceneLoader.TgcSceneLoader.DiffuseMapVertexElements, d3dDevice);

            //Cargar VertexBuffer
            using (VertexBuffer vb = d3dMesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                for (int j = 0; j < vertices.Length; j++)
                {
                    TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex v = new TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex();
                    CustomVertex.PositionTextured vLand = vertices[j];

                    //vertices
                    v.Position = Vector3.TransformCoordinate(vLand.Position, this.transform);

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
            using (IndexBuffer ib = d3dMesh.IndexBuffer)
            {
                short[] indices = new short[vertices.Length];
                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] = (short)j;
                }
                ib.SetData(indices, 0, LockFlags.None);
            }

            //Calcular normales
            d3dMesh.ComputeNormals();

            //Malla de TGC
            TgcMesh tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.DiffuseMaps = new TgcTexture[] { texture };
            tgcMesh.Materials = new Material[] { TgcD3dDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }

        
        /// <summary>
        /// Crear un nuevo Terreno igual a este
        /// </summary>
        /// <returns>Terreno clonado</returns>
        public TgcEditableLand clone()
        {
            TgcEditableLand cloneLand = new TgcEditableLand();

            //Cargar altura de cada vertice
            for (int i = 0; i < cloneLand.editableVertices.Length; i++)
            {
                cloneLand.editableVertices[i].Pos = this.editableVertices[i].Pos;
            }
            cloneLand.setTexture(this.texture.clone());
            cloneLand.autoTransformEnable = this.autoTransformEnable;
            cloneLand.transform = this.transform;
            cloneLand.rotation = this.rotation;
            cloneLand.scale = this.scale;
            cloneLand.alphaBlendEnable = this.alphaBlendEnable;
            cloneLand.uvOffset = this.uvOffset;
            cloneLand.uvTiling = this.uvTiling;

            cloneLand.updateValues();
            return cloneLand;
        }
        

        /// <summary>
        /// Vertice compartido por varias caras
        /// </summary>
        public class EditableVertex
        {
            Vector3 pos;
            /// <summary>
            /// Posicion
            /// </summary>
            public Vector3 Pos
            {
                get { return pos; }
                set { pos = value; }
            }

            Vector2 uv;
            /// <summary>
            /// UV
            /// </summary>
            public Vector2 UV
            {
                get { return uv; }
                set { uv = value; }
            }

            /// <summary>
            /// Cargar altura en Y
            /// </summary>
            public void setY(float y)
            {
                this.pos.Y = y;
            }

            public override string ToString()
            {
                return "Pos: " + TgcParserUtils.printVector3(pos) + ", UV: " + TgcParserUtils.printVector2(uv);
            }

        }

    }
}
