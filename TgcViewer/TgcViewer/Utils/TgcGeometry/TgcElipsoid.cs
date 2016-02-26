using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Scene;
using TGC.Core.Utils;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Representa un volumen de Elipsoide con un centro y un radio distinto para cada uno de los tres ejes
    /// </summary>
    public class TgcElipsoid : IRenderObject
    {
        /// <summary>
        /// Cantidad de tramos que tendrá el mesh del Elipsoid a dibujar
        /// </summary>
        public const int ELIPSOID_MESH_RESOLUTION = 10;

        bool dirtyValues;
        CustomVertex.PositionColored[] vertices;

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



        /// <summary>
        /// Crear Elipsoid vacia
        /// </summary>
        public TgcElipsoid()
        {
            this.renderColor = Color.Yellow.ToArgb();
            this.dirtyValues = true;
            this.alphaBlendEnable = false;
        }

        /// <summary>
        /// Crear Elipsoid con centro y radio
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radios para los 3 ejes</param>
        public TgcElipsoid(Vector3 center, Vector3 radius)
            : this()
        {
            setValues(center, radius);
        }

        /// <summary>
        /// Configurar valores del Elipsoid
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radios para los 3 ejes</param>
        public void setValues(Vector3 center, Vector3 radius)
        {
            this.center = center;
            this.radius = radius;

            this.dirtyValues = true;
        }

        /// <summary>
        /// Configurar un nuevo centro del Elipsoid
        /// </summary>
        /// <param name="center">Nuevo centro</param>
        public void setCenter(Vector3 center)
        {
            setValues(center, this.radius);
        }

        /// <summary>
        /// Desplazar el centro respecto de su posición actual
        /// </summary>
        /// <param name="movement">Movimiento relativo a realizar</param>
        public void moveCenter(Vector3 movement)
        {
            setValues(this.center + movement, this.radius);
        }

        Vector3 center;
        /// <summary>
        /// Centro del Elipsoid
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }

        Vector3 radius;
        /// <summary>
        /// Radios del Elipsoid para cada uno de los 3 ejes
        /// </summary>
        public Vector3 Radius
        {
            get { return radius; }
        }

        int renderColor;
        /// <summary>
        /// Color de renderizado del Elipsoid.
        /// </summary>
        public int RenderColor
        {
            get { return renderColor; }
        }

        public Vector3 Position
        {
            get { return center; }
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
        /// Configurar el color de renderizado del Elipsoid
        /// Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            this.renderColor = color.ToArgb();
            dirtyValues = true;
        }
        

        /// <summary>
        /// Construye el mesh del Elipsoid
        /// </summary>
        private void updateValues()
        {
            if (vertices == null)
            {
                int verticesCount = (ELIPSOID_MESH_RESOLUTION * 2 + 2) * 3;
                this.vertices = new CustomVertex.PositionColored[verticesCount];
            }

            int index = 0;

            float step = FastMath.TWO_PI / (float)ELIPSOID_MESH_RESOLUTION;
            // Plano XY
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a) * radius.X, FastMath.Sin(a) * radius.Y, 0f) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a + step) * radius.X, FastMath.Sin(a + step) * radius.Y, 0f) + center, renderColor);
            }

            // Plano XZ
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a) * radius.X, 0f, FastMath.Sin(a) * radius.Z) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a + step) * radius.X, 0f, FastMath.Sin(a + step) * radius.Z) + center, renderColor);
            }

            // Plano YZ
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(0f, FastMath.Cos(a) * radius.Y, FastMath.Sin(a) * radius.Z) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(0f, FastMath.Cos(a + step) * radius.Y, FastMath.Sin(a + step) * radius.Z) + center, renderColor);
            }
        }


        /// <summary>
        /// Renderizar el Elipsoid
        /// </summary>
        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            //Cargar shader si es la primera vez
            if (this.effect == null)
            {
                this.effect = GuiController.Instance.Shaders.VariosShader;
                this.technique = TgcShaders.T_POSITION_COLORED;
            }

            //Actualizar vertices del Elipsoid solo si hubo una modificación
            if (dirtyValues)
            {
                updateValues();
                dirtyValues = false;
            }

            GuiController.Instance.Shaders.setShaderMatrixIdentity(this.effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = this.technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        /// Libera los recursos del objeto
        /// </summary>
        public void dispose()
        {
            vertices = null;
        }

        public override string ToString()
        {
            return "Center " + TgcParserUtils.printVector3(center) + ", Radius " + TgcParserUtils.printVector3(radius);
        }

        /// <summary>
        /// Devuelve el radio mas grande del Elipsoid
        /// </summary>
        /// <returns>Mayor radio</returns>
        public float getMaxRadius()
        {
            return radius.X > radius.Y ? radius.X : (radius.Y > radius.Z ? radius.Y : radius.Z);
        }




        /// <summary>
        /// Convertir a struct
        /// </summary>
        public ElipsoidStruct toStruct()
        {
            ElipsoidStruct eStruct = new ElipsoidStruct();
            eStruct.center = center;
            eStruct.radius = radius;
            return eStruct;
        }

        /// <summary>
        /// Elipsoid en un struct liviano
        /// </summary>
        public struct ElipsoidStruct
        {
            public Vector3 center;
            public Vector3 radius;

            /// <summary>
            /// Convertir a clase
            /// </summary>
            public TgcElipsoid toClass()
            {
                return new TgcElipsoid(center, radius);
            }
        }


        
    }
}
