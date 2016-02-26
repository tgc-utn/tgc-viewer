using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Linq;
using TGC.Core.Scene;
using TGC.Core.Utils;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;

namespace TgcViewer.Utils.TgcGeometry
{
    public class TgcCylinder : IRenderObject, ITransformObject
    {
        private float topRadius;
        private float bottomRadius;
        private int color;
        private TgcBoundingCylinder boundingCylinder;

        private const int END_CAPS_RESOLUTION = 40;
        private CustomVertex.PositionColoredTextured[] sideTrianglesVertices; //triangle strip

        private bool useTexture;
        private TgcTexture texture;
        private Effect effect;
        private string technique;

        private Matrix manualTransformation;

        public TgcCylinder(Vector3 _center, float _topRadius, float _bottomRadius, float _halfLength)
        {
            this.topRadius = _topRadius;
            this.bottomRadius = _bottomRadius;
            this.boundingCylinder = new TgcBoundingCylinder(_center, 1, _halfLength);

            this.color = Color.Red.ToArgb();

            this.manualTransformation = Matrix.Identity;
            this.AutoTransformEnable = true;

            this.initialize();
        }

        public TgcCylinder(Vector3 _center, float _radius, float _halfLength)
            : this(_center, _radius, _radius, _halfLength)
        {
            //nothing to do
        }

        private void initialize()
        {
            int capsResolution = END_CAPS_RESOLUTION;

            //cara lateral: un vertice por cada vertice de cada tapa, mas dos para cerrarla
            this.sideTrianglesVertices = new CustomVertex.PositionColoredTextured[2 * capsResolution + 2];

            this.useColorShader();
            this.updateValues();
        }

        private void useColorShader()
        {
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED;
            this.useTexture = false;
        }

        private void useTextureShader()
        {
            this.technique = TgcShaders.T_POSITION_COLORED_TEXTURED;
            this.useTexture = true;
        }

        private void updateDraw()
        {
            //vectores utilizados para el dibujado
            Vector3 upVector = new Vector3(0, 1, 0);
            Vector3 n = new Vector3(1, 0, 0);

            int capsResolution = END_CAPS_RESOLUTION;

            //matriz de rotacion del vector de dibujado
            float angleStep = FastMath.TWO_PI / (float)capsResolution;
            Matrix rotationMatrix = Matrix.RotationAxis(-upVector, angleStep);
            float angle = 0;

            //transformacion que se le aplicara a cada vertice
            Matrix transformation = this.Transform;
            float bcInverseRadius = 1 / this.boundingCylinder.Radius;

            //arrays donde guardamos los puntos dibujados
            Vector3[] topCapDraw = new Vector3[capsResolution];
            Vector3[] bottomCapDraw = new Vector3[capsResolution];

            //variables temporales utilizadas para el texture mapping
            float u;

            for (int i = 0; i < capsResolution; i++)
            {
                //establecemos los vertices de las tapas
                topCapDraw[i] = upVector + (n * this.topRadius * bcInverseRadius);
                bottomCapDraw[i] = -upVector + (n * this.bottomRadius * bcInverseRadius);

                u = angle / FastMath.TWO_PI;

                //triangulos de la cara lateral (strip)
                this.sideTrianglesVertices[2 * i] = new CustomVertex.PositionColoredTextured(topCapDraw[i], color, u, 0);
                this.sideTrianglesVertices[2 * i + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[i], color, u, 1);

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);
                angle += angleStep;
            }

            //cerramos la cara lateral
            this.sideTrianglesVertices[2 * capsResolution] = new CustomVertex.PositionColoredTextured(topCapDraw[0], color, 1, 0);
            this.sideTrianglesVertices[2 * capsResolution + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[0], color, 1, 1);
        }

        public void render()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

            if (this.AlphaBlendEnable)
            {
                d3dDevice.RenderState.AlphaBlendEnable = true;
                d3dDevice.RenderState.AlphaTestEnable = true;
            }

            if (texture != null)
                texturesManager.shaderSet(effect, "texDiffuseMap", texture);
            else
                texturesManager.clear(0);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(this.effect, this.Transform);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColoredTextured;
            effect.Technique = this.technique;

            int capsResolution = END_CAPS_RESOLUTION;

            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2 * capsResolution, this.sideTrianglesVertices);
            effect.EndPass();
            effect.End();

            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.AlphaBlendEnable = false;
        }

        public void dispose()
        {
            if (this.texture != null) this.texture.dispose();
            this.sideTrianglesVertices = null;
            this.boundingCylinder.dispose();
        }

        public Color Color {
            get { return Color.FromArgb(this.color); }
            set { this.color = value.ToArgb(); }
        }

        public bool AlphaBlendEnable { get; set; }

        #region Transformation

        public bool AutoTransformEnable { get; set; }

        public Matrix Transform
        {
            get
            {
                if (this.AutoTransformEnable) return this.boundingCylinder.Transform;
                else return this.manualTransformation;
            }
            set { this.manualTransformation = value; }
        }

        public Vector3 Position
        {
            get { return this.boundingCylinder.Center; }
            set { this.boundingCylinder.Center = value; }
        }

        public Vector3 Rotation
        {
            get { return this.boundingCylinder.Rotation; }
            set { this.boundingCylinder.Rotation = value; }
        }

        public Vector3 Scale
        {
            get { return new Vector3(1, 1, 1); }
            set { ; }
        }

        public void move(Vector3 v)
        {
            this.boundingCylinder.move(v);
        }

        public void move(float x, float y, float z)
        {
            this.boundingCylinder.move(x, y, z);
        }

        public void moveOrientedY(float movement)
        {
            float z = FastMath.Cos(this.Rotation.Y) * movement;
            float x = FastMath.Sin(this.Rotation.Y) * movement;
            this.move(x, 0, z);
        }

        public void getPosition(Vector3 pos)
        {
            pos.X = this.Position.X;
            pos.Y = this.Position.Y;
            pos.Z = this.Position.Z;
        }

        public void rotateX(float angle)
        {
            this.boundingCylinder.rotateX(angle);
        }

        public void rotateY(float angle)
        {
            this.boundingCylinder.rotateY(angle);
        }

        public void rotateZ(float angle)
        {
            this.boundingCylinder.rotateZ(angle);
        }

        #endregion

        /// <summary>
        /// Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return this.effect; }
            set { this.effect = value; }
        }

        /// <summary>
        /// Technique que se va a utilizar en el effect.
        /// Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return this.technique; }
            set { this.technique = value; }
        }

        /// <summary>
        /// Setea la textura
        /// </summary>
        public void setTexture(TgcTexture _texture)
        {
            if (this.texture != null)
                this.texture.dispose();
            this.texture = _texture;
        }

        /// <summary>
        /// Habilita el dibujado de la textura
        /// </summary>
        public bool UseTexture {
            get { return this.useTexture; }
            set
            {
                if (value)
                    this.useTextureShader();
                else
                    this.useColorShader();
            }
        }

        public TgcBoundingCylinder BoundingCylinder { get { return this.boundingCylinder; } }

        /// <summary>
        /// Actualiza la posicion e inclinacion del cilindro
        /// </summary>
        public void updateValues()
        {
            this.boundingCylinder.Radius = FastMath.Max(
                FastMath.Abs(this.topRadius), FastMath.Abs(this.bottomRadius));
            this.boundingCylinder.updateValues();
            this.updateDraw();
        }

        /// <summary>
        /// Largo o altura del cilindro
        /// </summary>
        public float Length
        {
            get { return this.boundingCylinder.Length; }
            set { this.boundingCylinder.Length = value; }
        }

        /// <summary>
        /// Radio de la tapa superior
        /// </summary>
        public float TopRadius
        {
            get { return this.topRadius; }
            set { this.topRadius = value; }
        }

        /// <summary>
        /// Radio de la tapa inferior
        /// </summary>
        public float BottomRadius
        {
            get { return this.bottomRadius; }
            set { this.bottomRadius = value; }
        }

        /// <summary>
        /// Radio del cilindro
        /// Si se usa este setter, tanto el radio superior como el inferior quedan igualados
        /// Si se usa este getter, se devuelve el radio maximo del cilindro
        /// </summary>
        public float Radius
        {
            get { return this.boundingCylinder.Radius; }
            set { this.TopRadius = value; this.BottomRadius = value; }
        }

        /// <summary>
        /// Centro del cilindro
        /// </summary>
        public Vector3 Center
        {
            get { return this.boundingCylinder.Center; }
            set { this.boundingCylinder.Center = value; }
        }
    }
}
