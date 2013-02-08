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
    /// Herramienta para crear una Caja 3D de tamaño variable, con color y Textura
    /// </summary>
    public class TgcBox : IRenderObject, ITransformObject
    {

        #region Creacion

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 center, Vector3 size)
        {
            TgcBox box = new TgcBox();
            box.setPositionSize(center, size);
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado, con el color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 center, Vector3 size, Color color)
        {
            TgcBox box = new TgcBox();
            box.setPositionSize(center, size);
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja con el centro y tamaño especificado, con la textura especificada
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 center, Vector3 size, TgcTexture texture)
        {
            TgcBox box = TgcBox.fromSize(center, size);
            box.setTexture(texture);
            return box;
        }

        /// <summary>
        /// Crea una caja con centro (0,0,0) y el tamaño especificado
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 size)
        {
            return TgcBox.fromSize(new Vector3(0, 0, 0), size);
        }

        /// <summary>
        /// Crea una caja con centro (0,0,0) y el tamaño especificado, con el color especificado
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 size, Color color)
        {
            return TgcBox.fromSize(new Vector3(0, 0, 0), size, color);
        }

        /// <summary>
        /// Crea una caja con centro (0,0,0) y el tamaño especificado, con la textura especificada
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromSize(Vector3 size, TgcTexture texture)
        {
            return TgcBox.fromSize(new Vector3(0, 0, 0), size, texture);
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromExtremes(Vector3 pMin, Vector3 pMax)
        {
            Vector3 size = Vector3.Subtract(pMax, pMin);
            Vector3 midSize = Vector3.Scale(size, 0.5f);
            Vector3 center = pMin + midSize;
            return TgcBox.fromSize(center, size);
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromExtremes(Vector3 pMin, Vector3 pMax, Color color)
        {
            TgcBox box = TgcBox.fromExtremes(pMin, pMax);
            box.color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        /// Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TgcBox fromExtremes(Vector3 pMin, Vector3 pMax, TgcTexture texture)
        {
            TgcBox box = TgcBox.fromExtremes(pMin, pMax);
            box.setTexture(texture);
            return box;
        }

        #endregion


        CustomVertex.PositionColoredTextured[] vertices;
        VertexBuffer vertexBuffer;

        Vector3 size;
        /// <summary>
        /// Dimensiones de la caja
        /// </summary>
        public Vector3 Size
        {
            get { return size; }
            set { 
                size = value;
                updateBoundingBox();
            }
        }

        /// <summary>
        /// Escala de la caja. Siempre es (1, 1, 1).
        /// Utilizar Size
        /// </summary>
        public Vector3 Scale
        {
            get { return new Vector3(1, 1, 1); }
            set { ; }
        }

        Color color;
        /// <summary>
        /// Color de los vértices de la caja
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        TgcTexture texture;
        /// <summary>
        /// Textura de la caja
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
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
        /// Posicion absoluta del centro de la caja
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
        /// Rotación absoluta de la caja
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        private bool enabled;
        /// <summary>
        /// Indica si la caja esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }


        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de la caja
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
        /// Crea una caja vacia
        /// </summary>
        public TgcBox()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            vertices = new CustomVertex.PositionColoredTextured[36];
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), vertices.Length, d3dDevice,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColoredTextured.Format, Pool.Default);

            this.autoTransformEnable = true;
            this.transform = Matrix.Identity;
            this.translation = new Vector3(0,0,0);
            this.rotation = new Vector3(0,0,0);
            this.enabled = true;
            this.color = Color.White;
            this.alphaBlendEnable = false;
            this.uvOffset = new Vector2(0, 0);
            this.uvTiling = new Vector2(1, 1);

            //BoundingBox
            boundingBox = new TgcBoundingBox();

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        /// Actualiza la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            int c = color.ToArgb();
            float x = size.X / 2;
            float y = size.Y / 2;
            float z = size.Z / 2;
            float u = uvTiling.X;
            float v = uvTiling.Y;
            float offsetU = uvOffset.X;
            float offsetV = uvOffset.Y;

            // Front face
            vertices[0] = new CustomVertex.PositionColoredTextured(-x, y, z, c, offsetU, offsetV);
            vertices[1] = new CustomVertex.PositionColoredTextured(-x, -y, z, c, offsetU, offsetV + v);
            vertices[2] = new CustomVertex.PositionColoredTextured(x, y, z, c, offsetU + u, offsetV);
            vertices[3] = new CustomVertex.PositionColoredTextured(-x, -y, z, c, offsetU, offsetV + v);
            vertices[4] = new CustomVertex.PositionColoredTextured(x, -y, z, c, offsetU + u, offsetV + v);
            vertices[5] = new CustomVertex.PositionColoredTextured(x, y, z, c, offsetU + u, offsetV);

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[6] = new CustomVertex.PositionColoredTextured(-x, y, -z, c, offsetU, offsetV);
            vertices[7] = new CustomVertex.PositionColoredTextured(x, y, -z, c, offsetU + u, offsetV);
            vertices[8] = new CustomVertex.PositionColoredTextured(-x, -y, -z, c, offsetU, offsetV + v);
            vertices[9] = new CustomVertex.PositionColoredTextured(-x, -y, -z, c, offsetU, offsetV + v);
            vertices[10] = new CustomVertex.PositionColoredTextured(x, y, -z, c, offsetU + u, offsetV);
            vertices[11] = new CustomVertex.PositionColoredTextured(x, -y, -z, c, offsetU + u, offsetV + v);

            // Top face
            vertices[12] = new CustomVertex.PositionColoredTextured(-x, y, z, c, offsetU, offsetV);
            vertices[13] = new CustomVertex.PositionColoredTextured(x, y, -z, c, offsetU + u, offsetV + v);
            vertices[14] = new CustomVertex.PositionColoredTextured(-x, y, -z, c, offsetU, offsetV + v);
            vertices[15] = new CustomVertex.PositionColoredTextured(-x, y, z, c, offsetU, offsetV);
            vertices[16] = new CustomVertex.PositionColoredTextured(x, y, z, c, offsetU + u, offsetV);
            vertices[17] = new CustomVertex.PositionColoredTextured(x, y, -z, c, offsetU + u, offsetV + v);

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[18] = new CustomVertex.PositionColoredTextured(-x, -y, z, c, offsetU, offsetV);
            vertices[19] = new CustomVertex.PositionColoredTextured(-x, -y, -z, c, offsetU, offsetV + v);
            vertices[20] = new CustomVertex.PositionColoredTextured(x, -y, -z, c, offsetU + u, offsetV + v);
            vertices[21] = new CustomVertex.PositionColoredTextured(-x, -y, z, c, offsetU, offsetV);
            vertices[22] = new CustomVertex.PositionColoredTextured(x, -y, -z, c, offsetU + u, offsetV + v);
            vertices[23] = new CustomVertex.PositionColoredTextured(x, -y, z, c, offsetU + u, offsetV);

            // Left face
            vertices[24] = new CustomVertex.PositionColoredTextured(-x, y, z, c, offsetU, offsetV);
            vertices[25] = new CustomVertex.PositionColoredTextured(-x, -y, -z, c, offsetU + u, offsetV + v);
            vertices[26] = new CustomVertex.PositionColoredTextured(-x, -y, z, c, offsetU, offsetV + v);
            vertices[27] = new CustomVertex.PositionColoredTextured(-x, y, -z, c, offsetU + u, offsetV);
            vertices[28] = new CustomVertex.PositionColoredTextured(-x, -y, -z, c, offsetU + u, offsetV + v);
            vertices[29] = new CustomVertex.PositionColoredTextured(-x, y, z, c, offsetU, offsetV); 

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[30] = new CustomVertex.PositionColoredTextured(x, y, z, c, offsetU, offsetV);
            vertices[31] = new CustomVertex.PositionColoredTextured(x, -y, z, c, offsetU, offsetV + v);
            vertices[32] = new CustomVertex.PositionColoredTextured(x, -y, -z, c, offsetU + u, offsetV + v);
            vertices[33] = new CustomVertex.PositionColoredTextured(x, y, -z, c, offsetU + u, offsetV);
            vertices[34] = new CustomVertex.PositionColoredTextured(x, y, z, c, offsetU, offsetV);
            vertices[35] = new CustomVertex.PositionColoredTextured(x, -y, -z, c, offsetU + u, offsetV + v);

            vertexBuffer.SetData(vertices, 0, LockFlags.None);
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
            this.technique = TgcShaders.T_POSITION_COLORED_TEXTURED;
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
                this.transform = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.Translation(translation);
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
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColoredTextured;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
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
        /// Configurar valores de posicion y tamaño en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        public void setPositionSize(Vector3 position, Vector3 size)
        {
            this.translation = position;
            this.size = size;
            updateBoundingBox();
        }

        /// <summary>
        /// Configurar punto mínimo y máximo del box
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public void setExtremes(Vector3 min, Vector3 max)
        {
            Vector3 size = Vector3.Subtract(max, min);
            Vector3 midSize = Vector3.Scale(size, 0.5f);
            Vector3 center = min + midSize;
            setPositionSize(center, size);
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
        /// Actualiza el BoundingBox de la caja.
        /// No contempla rotacion
        /// </summary>
        private void updateBoundingBox()
        {
            Vector3 midSize = Vector3.Scale(size, 0.5f);
            boundingBox.setExtremes(Vector3.Subtract(translation, midSize), Vector3.Add(translation, midSize));
        }

        /// <summary>
        /// Convierte el box en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Obtener matriz para transformar vertices
            if (autoTransformEnable)
            {
                this.transform = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.Translation(translation);
            }

            //Crear mesh con DiffuseMap
            if (texture != null)
            {
                //Crear Mesh
                Mesh d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed, TgcSceneLoader.TgcSceneLoader.DiffuseMapVertexElements, d3dDevice);

                //Cargar VertexBuffer
                using (VertexBuffer vb = d3dMesh.VertexBuffer)
                {
                    GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex v = new TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex();
                        CustomVertex.PositionColoredTextured vBox = vertices[j];

                        //vertices
                        v.Position = Vector3.TransformCoordinate(vBox.Position, this.transform);

                        //normals
                        v.Normal = Vector3.Empty;

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
                tgcMesh.DiffuseMaps = new TgcTexture[] { texture.clone() };
                tgcMesh.Materials = new Material[] { TgcD3dDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                return tgcMesh;
            }

            //Crear mesh con solo color
            else
            {
                //Crear Mesh
                Mesh d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed, TgcSceneLoader.TgcSceneLoader.VertexColorVertexElements, d3dDevice);

                //Cargar VertexBuffer
                using (VertexBuffer vb = d3dMesh.VertexBuffer)
                {
                    GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        TgcSceneLoader.TgcSceneLoader.VertexColorVertex v = new TgcSceneLoader.TgcSceneLoader.VertexColorVertex();
                        CustomVertex.PositionColoredTextured vBox = vertices[j];

                        //vertices
                        v.Position = Vector3.TransformCoordinate(vBox.Position, this.transform);

                        //normals
                        v.Normal = Vector3.Empty;

                        //color
                        v.Color = vBox.Color;

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


                //Malla de TGC
                TgcMesh tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.VERTEX_COLOR);
                tgcMesh.Materials = new Material[] { TgcD3dDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                return tgcMesh;
            }
        }

        /// <summary>
        /// Crear un nuevo TgcBox igual a este
        /// </summary>
        /// <returns>Box clonado</returns>
        public TgcBox clone()
        {
            TgcBox cloneBox = new TgcBox();
            cloneBox.setPositionSize(this.translation, this.size);
            cloneBox.color = this.color;
            if (this.texture != null)
            {
                cloneBox.setTexture(this.texture.clone());
            }
            cloneBox.autoTransformEnable = this.autoTransformEnable;
            cloneBox.transform = this.transform;
            cloneBox.rotation = this.rotation;
            cloneBox.alphaBlendEnable = this.alphaBlendEnable;
            cloneBox.uvOffset = this.uvOffset;
            cloneBox.uvTiling = this.uvTiling;

            cloneBox.updateValues();
            return cloneBox;
        }


        


    }
}
