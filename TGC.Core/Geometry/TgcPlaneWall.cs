using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Pared 3D plana que solo crece en dos dimensiones.
    /// </summary>
    public class TgcPlaneWall : IRenderObject
    {
        /// <summary>
        ///     Orientaciones posibles de la pared
        /// </summary>
        public enum Orientations
        {
            /// <summary>
            ///     Pared vertical a lo largo de X
            /// </summary>
            XYplane = 0,

            /// <summary>
            ///     Pared horizontal
            /// </summary>
            XZplane = 1,

            /// <summary>
            ///     Pared vertical a lo largo de Z
            /// </summary>
            YZplane = 2
        }

        private readonly CustomVertex.PositionTextured[] vertices;

        protected Effect effect;

        protected string technique;

        /// <summary>
        ///     Crea una pared vacia.
        /// </summary>
        public TgcPlaneWall()
        {
            vertices = new CustomVertex.PositionTextured[6];
            AutoAdjustUv = false;
            Enabled = true;
            BoundingBox = new TgcBoundingBox();
            UTile = 1;
            VTile = 1;
            AlphaBlendEnable = false;
            UVOffset = new Vector2(0, 0);

            //Shader
            effect = TgcShaders.Instance.VariosShader;
            technique = TgcShaders.T_POSITION_TEXTURED;
        }

        /// <summary>
        ///     Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, especificando
        ///     el tiling de la textura
        /// </summary>
        /// <param name="origin">Punto de origen de la pared</param>
        /// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="texture">Textura de la pared</param>
        /// <param name="uTile">Cantidad de tile de la textura en coordenada U</param>
        /// <param name="vTile">Cantidad de tile de la textura en coordenada V</param>
        public TgcPlaneWall(Vector3 origin, Vector3 size, Orientations orientation, TgcTexture texture, float uTile,
            float vTile)
            : this()
        {
            setTexture(texture);

            AutoAdjustUv = false;
            Origin = origin;
            Size = size;
            Orientation = orientation;
            UTile = uTile;
            VTile = vTile;

            updateValues();
        }

        /// <summary>
        ///     Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, con ajuste automatico
        ///     de coordenadas de textura
        /// </summary>
        /// <param name="origin">Punto de origen de la pared</param>
        /// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="texture">Textura de la pared</param>
        public TgcPlaneWall(Vector3 origin, Vector3 size, Orientations orientation, TgcTexture texture)
            : this()
        {
            setTexture(texture);

            AutoAdjustUv = true;
            Origin = origin;
            Size = size;
            Orientation = orientation;
            UTile = 1;
            VTile = 1;

            updateValues();
        }

        /// <summary>
        ///     Origen de coordenadas de la pared.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        ///     Dimensiones de la pared.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Size { get; set; }

        /// <summary>
        ///     Orientación de la pared.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Orientations Orientation { get; set; }

        /// <summary>
        ///     Textura de la pared
        /// </summary>
        public TgcTexture Texture { get; private set; }

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
        ///     Cantidad de tile de la textura en coordenada U.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float UTile { get; set; }

        /// <summary>
        ///     Cantidad de tile de la textura en coordenada V.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float VTile { get; set; }

        /// <summary>
        ///     Auto ajustar coordenadas UV en base a la relación de tamaño de la pared y la textura
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public bool AutoAdjustUv { get; set; }

        /// <summary>
        ///     Offset UV de textura
        /// </summary>
        public Vector2 UVOffset { get; set; }

        /// <summary>
        ///     Indica si la pared esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     BoundingBox de la pared
        /// </summary>
        public TgcBoundingBox BoundingBox { get; }

        public Vector3 Position
        {
            get { return Origin; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar la pared
        /// </summary>
        public void render()
        {
            if (!Enabled)
                return;

            activateAlphaBlend();

            TexturesManager.Instance.shaderSet(effect, "texDiffuseMap", Texture);
            TexturesManager.Instance.clear(1);
            TgcShaders.Instance.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionTextured;
            effect.Technique = technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, vertices);
            effect.EndPass();
            effect.End();

            resetAlphaBlend();
        }

        /// <summary>
        ///     Liberar recursos de la pared
        /// </summary>
        public void dispose()
        {
            Texture.dispose();
        }

        /// <summary>
        ///     Configurar punto minimo y maximo de la pared.
        ///     Se ignora un valor de cada punto según la orientación elegida.
        ///     Llamar a updateValues() para aplicar cambios.
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public void setExtremes(Vector3 min, Vector3 max)
        {
            Origin = min;
            Size = Vector3.Subtract(max, min);
        }

        /// <summary>
        ///     Actualizar parámetros de la pared en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            float autoWidth;
            float autoHeight;

            //Calcular los 4 corners de la pared, segun el tipo de orientacion
            Vector3 bLeft, tLeft, bRight, tRight;
            if (Orientation == Orientations.XYplane)
            {
                bLeft = Origin;
                tLeft = new Vector3(Origin.X + Size.X, Origin.Y, Origin.Z);
                bRight = new Vector3(Origin.X, Origin.Y + Size.Y, Origin.Z);
                tRight = new Vector3(Origin.X + Size.X, Origin.Y + Size.Y, Origin.Z);

                autoWidth = Size.X / Texture.Width;
                autoHeight = Size.Y / Texture.Height;
            }
            else if (Orientation == Orientations.YZplane)
            {
                bLeft = Origin;
                tLeft = new Vector3(Origin.X, Origin.Y, Origin.Z + Size.Z);
                bRight = new Vector3(Origin.X, Origin.Y + Size.Y, Origin.Z);
                tRight = new Vector3(Origin.X, Origin.Y + Size.Y, Origin.Z + Size.Z);

                autoWidth = Size.Y / Texture.Width;
                autoHeight = Size.Z / Texture.Height;
            }
            else
            {
                bLeft = Origin;
                tLeft = new Vector3(Origin.X + Size.X, Origin.Y, Origin.Z);
                bRight = new Vector3(Origin.X, Origin.Y, Origin.Z + Size.Z);
                tRight = new Vector3(Origin.X + Size.X, Origin.Y, Origin.Z + Size.Z);

                autoWidth = Size.X / Texture.Width;
                autoHeight = Size.Z / Texture.Height;
            }

            //Auto ajustar UV
            if (AutoAdjustUv)
            {
                UTile = autoHeight;
                VTile = autoWidth;
            }
            var offsetU = UVOffset.X;
            var offsetV = UVOffset.Y;

            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(bLeft, offsetU + UTile, offsetV + VTile);
            vertices[1] = new CustomVertex.PositionTextured(tLeft, offsetU, offsetV + VTile);
            vertices[2] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bLeft, offsetU + UTile, offsetV + VTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);
            vertices[5] = new CustomVertex.PositionTextured(bRight, offsetU + UTile, offsetV);

            /*Versión con triángulos para el otro sentido
            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(tLeft, 0 * this.uTile, 1 * this.vTile);
            vertices[1] = new CustomVertex.PositionTextured(bLeft, 1 * this.uTile, 1 * this.vTile);
            vertices[2] = new CustomVertex.PositionTextured(bRight, 1 * this.uTile, 0 * this.vTile);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bRight, 1 * this.uTile, 0 * this.vTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, 0 * this.uTile, 0 * this.vTile);
            vertices[5] = new CustomVertex.PositionTextured(tLeft, 0 * this.uTile, 1 * this.vTile);
            */

            //BoundingBox
            BoundingBox.setExtremes(bLeft, tRight);
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
        ///     Convierte la pared en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            //Crear Mesh
            var d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed,
                TgcSceneLoader.DiffuseMapVertexElements, D3DDevice.Instance.Device);

            //Cargar VertexBuffer
            using (var vb = d3dMesh.VertexBuffer)
            {
                var data = vb.Lock(0, 0, LockFlags.None);
                var ceroNormal = new Vector3(0, 0, 0);
                var whiteColor = Color.White.ToArgb();
                for (var j = 0; j < vertices.Length; j++)
                {
                    var v = new TgcSceneLoader.DiffuseMapVertex();
                    var vWall = vertices[j];

                    //vertices
                    v.Position = vWall.Position;

                    //normals
                    v.Normal = ceroNormal;

                    //texture coordinates diffuseMap
                    v.Tu = vWall.Tu;
                    v.Tv = vWall.Tv;

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
            d3dMesh.ComputeNormals();

            //Malla de TGC
            var tgcMesh = new TgcMesh(d3dMesh, meshName, TgcMesh.MeshRenderType.DIFFUSE_MAP);
            tgcMesh.DiffuseMaps = new[] { Texture.clone() };
            tgcMesh.Materials = new[] { D3DDevice.DEFAULT_MATERIAL };
            tgcMesh.createBoundingBox();
            tgcMesh.Enabled = true;
            return tgcMesh;
        }

        /// <summary>
        ///     Crear un nuevo Wall igual a este
        /// </summary>
        /// <returns>Wall clonado</returns>
        public TgcPlaneWall clone()
        {
            var cloneWall = new TgcPlaneWall();
            cloneWall.Origin = Origin;
            cloneWall.Size = Size;
            cloneWall.Orientation = Orientation;
            cloneWall.AutoAdjustUv = AutoAdjustUv;
            cloneWall.UTile = UTile;
            cloneWall.VTile = VTile;
            cloneWall.AlphaBlendEnable = AlphaBlendEnable;
            cloneWall.UVOffset = UVOffset;
            cloneWall.setTexture(Texture.clone());

            updateValues();
            return cloneWall;
        }
    }
}