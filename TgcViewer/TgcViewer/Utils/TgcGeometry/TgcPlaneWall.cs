using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Scene;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Pared 3D plana que solo crece en dos dimensiones.
    /// </summary>
    public class TgcPlaneWall : IRenderObject
    {
        private CustomVertex.PositionTextured[] vertices;

        Vector3 origin;
        /// <summary>
        /// Origen de coordenadas de la pared.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        Vector3 size;
        /// <summary>
        /// Dimensiones de la pared.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        Orientations orientation;
        /// <summary>
        /// Orientación de la pared.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public Orientations Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        TgcTexture texture;
        /// <summary>
        /// Textura de la pared
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

        float uTile;
        /// <summary>
        /// Cantidad de tile de la textura en coordenada U.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float UTile
        {
            get { return uTile; }
            set { uTile = value; }
        }

        float vTile;
        /// <summary>
        /// Cantidad de tile de la textura en coordenada V.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public float VTile
        {
            get { return vTile; }
            set { vTile = value; }
        }

        bool autoAdjustUv;
        /// <summary>
        /// Auto ajustar coordenadas UV en base a la relación de tamaño de la pared y la textura
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        public bool AutoAdjustUv
        {
            get { return autoAdjustUv; }
            set { autoAdjustUv = value; }
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

        private bool enabled;
        /// <summary>
        /// Indica si la pared esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private TgcBoundingBox boundingBox;
        /// <summary>
        /// BoundingBox de la pared
        /// </summary>
        public TgcBoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        public Vector3 Position
        {
            get { return origin; }
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

        /// <summary>
        /// Orientaciones posibles de la pared
        /// </summary>
        public enum Orientations
        {
            /// <summary>
            /// Pared vertical a lo largo de X
            /// </summary>
            XYplane = 0,
            /// <summary>
            /// Pared horizontal
            /// </summary>
            XZplane = 1,
            /// <summary>
            /// Pared vertical a lo largo de Z
            /// </summary>
            YZplane = 2,
        }



        /// <summary>
        /// Crea una pared vacia.
        /// </summary>
        public TgcPlaneWall()
        {
            this.vertices = new CustomVertex.PositionTextured[6];
            this.autoAdjustUv = false;
            this.enabled = true;
            this.boundingBox = new TgcBoundingBox();
            this.uTile = 1;
            this.vTile = 1;
            this.alphaBlendEnable = false;
            this.uvOffset = new Vector2(0, 0);

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_TEXTURED;

        }

        /// <summary>
        /// Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, especificando
        /// el tiling de la textura
        /// </summary>
        /// <param name="origin">Punto de origen de la pared</param>
        /// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="texture">Textura de la pared</param>
        /// <param name="uTile">Cantidad de tile de la textura en coordenada U</param>
        /// <param name="vTile">Cantidad de tile de la textura en coordenada V</param>
        public TgcPlaneWall(Vector3 origin, Vector3 size, Orientations orientation, TgcTexture texture, float uTile, float vTile) 
            : this()
        {
            setTexture(texture);

            autoAdjustUv = false;
            this.origin = origin;
            this.size = size;
            this.orientation = orientation;
            this.uTile = uTile;
            this.vTile = vTile;

            updateValues();
        }

        /// <summary>
        /// Crea una pared con un punto de origen, el tamaño de la pared y la orientación de la misma, con ajuste automatico
        /// de coordenadas de textura
        /// </summary>
        /// <param name="origin">Punto de origen de la pared</param>
        /// <param name="size">Dimensiones de la pared. Uno de los valores será ignorado, según la orientación elegida</param>
        /// <param name="orientation">Orientacion de la pared</param>
        /// <param name="texture">Textura de la pared</param>
        public TgcPlaneWall(Vector3 origin, Vector3 size, Orientations orientation, TgcTexture texture)
            : this()
        {
            setTexture(texture);

            autoAdjustUv = true;
            this.origin = origin;
            this.size = size;
            this.orientation = orientation;
            this.uTile = 1;
            this.vTile = 1;

            updateValues();
        }

        /// <summary>
        /// Configurar punto minimo y maximo de la pared.
        /// Se ignora un valor de cada punto según la orientación elegida.
        /// Llamar a updateValues() para aplicar cambios.
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public void setExtremes(Vector3 min, Vector3 max)
        {
            this.origin = min;
            this.size = Vector3.Subtract(max, min);
        }

        /// <summary>
        /// Actualizar parámetros de la pared en base a los valores configurados
        /// </summary>
        public void updateValues()
        {
            float autoWidth;
            float autoHeight;

            //Calcular los 4 corners de la pared, segun el tipo de orientacion
            Vector3 bLeft, tLeft, bRight, tRight;
            if (orientation == Orientations.XYplane)
            {
                bLeft = origin;
                tLeft = new Vector3(origin.X + size.X, origin.Y, origin.Z);
                bRight = new Vector3(origin.X, origin.Y + size.Y, origin.Z);
                tRight = new Vector3(origin.X + size.X, origin.Y + size.Y, origin.Z);

                autoWidth = (size.X / texture.Width);
                autoHeight = (size.Y / texture.Height); 
            }
            else if(orientation == Orientations.YZplane)
            {
                bLeft = origin;
                tLeft = new Vector3(origin.X, origin.Y, origin.Z + size.Z);
                bRight = new Vector3(origin.X, origin.Y + size.Y, origin.Z);
                tRight = new Vector3(origin.X, origin.Y + size.Y, origin.Z + size.Z);

                autoWidth = (size.Y / texture.Width);
                autoHeight = (size.Z / texture.Height); 
            }
            else
            {
                bLeft = origin;
                tLeft = new Vector3(origin.X + size.X, origin.Y, origin.Z);
                bRight = new Vector3(origin.X, origin.Y, origin.Z + size.Z);
                tRight = new Vector3(origin.X + size.X, origin.Y, origin.Z + size.Z);

                autoWidth = (size.X / texture.Width);
                autoHeight = (size.Z / texture.Height); 
            }

            //Auto ajustar UV
            if (autoAdjustUv)
            {
                this.uTile = autoHeight;
                this.vTile = autoWidth;
            }
            float offsetU = this.uvOffset.X;
            float offsetV = this.uvOffset.Y;

            //Primer triangulo
            vertices[0] = new CustomVertex.PositionTextured(bLeft, offsetU + uTile, offsetV + vTile);
            vertices[1] = new CustomVertex.PositionTextured(tLeft, offsetU, offsetV + vTile);
            vertices[2] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);

            //Segundo triangulo
            vertices[3] = new CustomVertex.PositionTextured(bLeft, offsetU + uTile, offsetV + vTile);
            vertices[4] = new CustomVertex.PositionTextured(tRight, offsetU, offsetV);
            vertices[5] = new CustomVertex.PositionTextured(bRight, offsetU + uTile, offsetV);

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
            boundingBox.setExtremes(bLeft, tRight);
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
        /// Renderizar la pared
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            activateAlphaBlend();

            texturesManager.shaderSet(effect, "texDiffuseMap", texture);
            texturesManager.clear(1);
            GuiController.Instance.Shaders.setShaderMatrixIdentity(this.effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionTextured;
            effect.Technique = this.technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleList, 2, vertices);
            effect.EndPass();
            effect.End();

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
        /// Liberar recursos de la pared
        /// </summary>
        public void dispose()
        {
            texture.dispose();
        }

        /// <summary>
        /// Convierte la pared en un TgcMesh
        /// </summary>
        /// <param name="meshName">Nombre de la malla que se va a crear</param>
        public TgcMesh toMesh(string meshName)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Mesh
            Mesh d3dMesh = new Mesh(vertices.Length / 3, vertices.Length, MeshFlags.Managed, TgcSceneLoader.TgcSceneLoader.DiffuseMapVertexElements, d3dDevice);

            //Cargar VertexBuffer
            using (VertexBuffer vb = d3dMesh.VertexBuffer)
            {
                GraphicsStream data = vb.Lock(0, 0, LockFlags.None);
                Vector3 ceroNormal = new Vector3(0, 0, 0);
                int whiteColor = Color.White.ToArgb();
                for (int j = 0; j < vertices.Length; j++)
                {
                    TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex v = new TgcSceneLoader.TgcSceneLoader.DiffuseMapVertex();
                    CustomVertex.PositionTextured vWall = vertices[j];

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

        /// <summary>
        /// Crear un nuevo Wall igual a este
        /// </summary>
        /// <returns>Wall clonado</returns>
        public TgcPlaneWall clone()
        {
            TgcPlaneWall cloneWall = new TgcPlaneWall();
            cloneWall.origin = this.origin;
            cloneWall.size = this.size;
            cloneWall.orientation = this.orientation;
            cloneWall.autoAdjustUv = this.autoAdjustUv;
            cloneWall.uTile = this.uTile;
            cloneWall.vTile = this.vTile;
            cloneWall.alphaBlendEnable = this.alphaBlendEnable;
            cloneWall.uvOffset = this.uvOffset;
            cloneWall.setTexture(this.texture.clone());

            updateValues();
            return cloneWall;
        }

    }
}
