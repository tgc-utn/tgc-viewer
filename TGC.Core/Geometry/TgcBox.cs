using Microsoft.DirectX.Direct3D;
using System;
using System.Diagnostics;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Herramienta para crear una Caja 3D de tamaño variable, con color y Textura
    /// </summary>
    public class TGCBox : IRenderObject, ITransformObject
    {
        private readonly VertexBuffer vertexBuffer;

        private readonly CustomVertex.PositionColoredTextured[] vertices;

        private TGCVector3 rotation;

        private TGCVector3 size;

        private TGCVector3 translation;

        /// <summary>
        ///     Crea una caja vacia
        /// </summary>
        public TGCBox()
        {
            vertices = new CustomVertex.PositionColoredTextured[36];
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), vertices.Length,
                D3DDevice.Instance.Device,
                Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColoredTextured.Format, Pool.Default);

            AutoTransformEnable = false;
            Transform = TGCMatrix.Identity;
            translation = TGCVector3.Empty;
            rotation = TGCVector3.Empty;
            Enabled = true;
            Color = Color.White;
            AlphaBlendEnable = false;
            UVOffset = TGCVector2.Zero;
            UVTiling = TGCVector2.One;

            //BoundingBox
            BoundingBox = new TgcBoundingAxisAlignBox();

            //Shader
            Effect = TgcShaders.Instance.VariosShader;
            Technique = TgcShaders.T_POSITION_COLORED;
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en False.
        /// </summary>
        public bool AutoTransformEnable { get; set; }

        /// <summary>
        ///     BoundingBox de la caja
        /// </summary>
        public TgcBoundingAxisAlignBox BoundingBox { get; }

        /// <summary>
        ///     Color de los vértices de la caja
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        ///     Indica si la caja esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Posicion absoluta del centro de la caja
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
        ///     Rotación absoluta de la caja
        /// </summary>
        public TGCVector3 Rotation { get; set; }

        /// <summary>
        ///     Escala de la caja. Siempre es (1, 1, 1).
        ///     Utilizar Size
        /// </summary>
        public TGCVector3 Scale
        {
            get { return TGCVector3.One; }
            set { Debug.WriteLine("TODO esta bien que pase por aca?"); }
        }

        /// <summary>
        ///     Dimensiones de la caja
        /// </summary>
        public TGCVector3 Size
        {
            get { return size; }
            set
            {
                size = value;
                updateBoundingBox();
            }
        }

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
        ///     Textura de la caja
        /// </summary>
        public TgcTexture Texture { get; private set; }

        /// <summary>
        ///     Offset UV de textura
        /// </summary>
        public TGCVector2 UVOffset { get; set; }

        /// <summary>
        ///     Tiling UV de textura
        /// </summary>
        public TGCVector2 UVTiling { get; set; }

        /// <summary>
        ///     Renderizar la caja
        /// </summary>
        public void Render()
        {
            if (!Enabled)
                return;

            //transformacion
            if (AutoTransformEnable)
            {
                Transform = TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            TGCMatrix.Translation(translation);
            }

            //Activar AlphaBlending
            activateAlphaBlend();

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

            TgcShaders.Instance.setShaderMatrix(Effect, Transform);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColoredTextured;
            Effect.Technique = Technique;
            D3DDevice.Instance.Device.SetStreamSource(0, vertexBuffer, 0);

            //Render con shader
            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            Effect.EndPass();
            Effect.End();

            //Desactivar AlphaBlend
            resetAlphaBlend();
        }

        /// <summary>
        ///     Liberar los recursos de la cja
        /// </summary>
        public void Dispose()
        {
            if (Texture != null)
            {
                Texture.dispose();
            }
            if (vertexBuffer != null && !vertexBuffer.Disposed)
            {
                vertexBuffer.Dispose();
            }
            BoundingBox.Dispose();
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        public void Move(TGCVector3 v)
        {
            Move(v.X, v.Y, v.Z);
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
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
        public void RotateX(float angle)
        {
            rotation.X += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void RotateY(float angle)
        {
            rotation.Y += angle;
        }

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        public void RotateZ(float angle)
        {
            rotation.Z += angle;
        }

        /// <summary>
        ///     Actualiza la caja en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            var c = Color.ToArgb();
            var x = size.X / 2;
            var y = size.Y / 2;
            var z = size.Z / 2;
            var u = UVTiling.X;
            var v = UVTiling.Y;
            var offsetU = UVOffset.X;
            var offsetV = UVOffset.Y;

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
        ///     Configurar textura de la pared
        /// </summary>
        public void setTexture(TgcTexture texture)
        {
            if (Texture != null)
            {
                Texture.dispose();
            }
            Texture = texture;
            Technique = TgcShaders.T_POSITION_COLORED_TEXTURED;
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
        ///     Configurar valores de posicion y tamaño en forma conjunta
        /// </summary>
        /// <param name="position">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        public void setPositionSize(TGCVector3 position, TGCVector3 size)
        {
            translation = position;
            this.size = size;
            updateBoundingBox();
        }

        /// <summary>
        ///     Configurar punto mínimo y máximo del box
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public void setExtremes(TGCVector3 min, TGCVector3 max)
        {
            var size = TGCVector3.Subtract(max, min);
            var midSize = TGCVector3.Scale(size, 0.5f);
            var center = min + midSize;
            setPositionSize(center, size);
        }

        /// <summary>
        ///     Actualiza el BoundingBox de la caja.
        ///     No contempla rotacion
        /// </summary>
        private void updateBoundingBox()
        {
            var midSize = TGCVector3.Scale(size, 0.5f);
            BoundingBox.setExtremes(TGCVector3.Subtract(translation, midSize), TGCVector3.Add(translation, midSize));
        }

        /// <summary>
        ///     Convierte el box en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            //Obtener matriz para transformar vertices
            if (AutoTransformEnable)
            {
                Transform = TGCMatrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z) *
                            TGCMatrix.Translation(translation);
            }

            //Crear mesh con DiffuseMap
            if (Texture != null)
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
                        v.Position = TGCVector3.TransformCoordinate(TGCVector3.FromVector3(vBox.Position), Transform);

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
                tgcMesh.DiffuseMaps = new[] { Texture.Clone() };
                tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
                tgcMesh.createBoundingBox();
                tgcMesh.Enabled = true;
                tgcMesh.AlphaBlendEnable = AlphaBlendEnable;
                return tgcMesh;
            }

            //Crear mesh con solo color
            else
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
                        v.Position = TGCVector3.TransformCoordinate(TGCVector3.FromVector3(vBox.Position), Transform);

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
        }

        /// <summary>
        ///     Crear un nuevo TgcBox igual a este
        /// </summary>
        /// <returns>Box clonado</returns>
        public TGCBox clone()
        {
            var cloneBox = new TGCBox();
            cloneBox.setPositionSize(translation, size);
            cloneBox.Color = Color;
            if (Texture != null)
            {
                cloneBox.setTexture(Texture.Clone());
            }
            cloneBox.AutoTransformEnable = AutoTransformEnable;
            cloneBox.Transform = Transform;
            cloneBox.rotation = rotation;
            cloneBox.AlphaBlendEnable = AlphaBlendEnable;
            cloneBox.UVOffset = UVOffset;
            cloneBox.UVTiling = UVTiling;

            cloneBox.updateValues();
            return cloneBox;
        }

        #region Creacion

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 center, TGCVector3 size)
        {
            var box = new TGCBox();
            box.setPositionSize(center, size);
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado, con el color especificado
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 center, TGCVector3 size, Color color)
        {
            var box = new TGCBox();
            box.setPositionSize(center, size);
            box.Color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja con el centro y tamaño especificado, con la textura especificada
        /// </summary>
        /// <param name="center">Centro de la caja</param>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 center, TGCVector3 size, TgcTexture texture)
        {
            var box = fromSize(center, size);
            box.setTexture(texture);
            return box;
        }

        /// <summary>
        ///     Crea una caja con centro (0,0,0) y el tamaño especificado
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 size)
        {
            return fromSize(TGCVector3.Empty, size);
        }

        /// <summary>
        ///     Crea una caja con centro (0,0,0) y el tamaño especificado, con el color especificado
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 size, Color color)
        {
            return fromSize(TGCVector3.Empty, size, color);
        }

        /// <summary>
        ///     Crea una caja con centro (0,0,0) y el tamaño especificado, con la textura especificada
        /// </summary>
        /// <param name="size">Tamaño de la caja</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromSize(TGCVector3 size, TgcTexture texture)
        {
            return fromSize(TGCVector3.Empty, size, texture);
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromExtremes(TGCVector3 pMin, TGCVector3 pMax)
        {
            var size = TGCVector3.Subtract(pMax, pMin);
            var midSize = TGCVector3.Scale(size, 0.5f);
            var center = pMin + midSize;
            return fromSize(center, size);
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="color">Color de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromExtremes(TGCVector3 pMin, TGCVector3 pMax, Color color)
        {
            var box = fromExtremes(pMin, pMax);
            box.Color = color;
            box.updateValues();
            return box;
        }

        /// <summary>
        ///     Crea una caja en base al punto minimo y maximo, con el color especificado
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        /// <param name="texture">Textura de la caja</param>
        /// <returns>Caja creada</returns>
        public static TGCBox fromExtremes(TGCVector3 pMin, TGCVector3 pMax, TgcTexture texture)
        {
            var box = fromExtremes(pMin, pMax);
            box.setTexture(texture);
            return box;
        }

        #endregion
    }
}