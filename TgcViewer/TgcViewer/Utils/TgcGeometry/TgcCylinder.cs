using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.SceneLoader;
using TGC.Core.Utils;

namespace TgcViewer.Utils.TgcGeometry
{
    public class TgcCylinder : IRenderObject, ITransformObject
    {
        private const int END_CAPS_RESOLUTION = 40;
        private int color;

        private Matrix manualTransformation;
        private CustomVertex.PositionColoredTextured[] sideTrianglesVertices; //triangle strip
        private TgcTexture texture;

        private bool useTexture;

        public TgcCylinder(Vector3 _center, float _topRadius, float _bottomRadius, float _halfLength)
        {
            TopRadius = _topRadius;
            BottomRadius = _bottomRadius;
            BoundingCylinder = new TgcBoundingCylinder(_center, 1, _halfLength);

            color = Color.Red.ToArgb();

            manualTransformation = Matrix.Identity;
            AutoTransformEnable = true;

            initialize();
        }

        public TgcCylinder(Vector3 _center, float _radius, float _halfLength)
            : this(_center, _radius, _radius, _halfLength)
        {
            //nothing to do
        }

        public Color Color
        {
            get { return Color.FromArgb(color); }
            set { color = value.ToArgb(); }
        }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique { get; set; }

        /// <summary>
        ///     Habilita el dibujado de la textura
        /// </summary>
        public bool UseTexture
        {
            get { return useTexture; }
            set
            {
                if (value)
                    useTextureShader();
                else
                    useColorShader();
            }
        }

        public TgcBoundingCylinder BoundingCylinder { get; }

        /// <summary>
        ///     Largo o altura del cilindro
        /// </summary>
        public float Length
        {
            get { return BoundingCylinder.Length; }
            set { BoundingCylinder.Length = value; }
        }

        /// <summary>
        ///     Radio de la tapa superior
        /// </summary>
        public float TopRadius { get; set; }

        /// <summary>
        ///     Radio de la tapa inferior
        /// </summary>
        public float BottomRadius { get; set; }

        /// <summary>
        ///     Radio del cilindro
        ///     Si se usa este setter, tanto el radio superior como el inferior quedan igualados
        ///     Si se usa este getter, se devuelve el radio maximo del cilindro
        /// </summary>
        public float Radius
        {
            get { return BoundingCylinder.Radius; }
            set
            {
                TopRadius = value;
                BottomRadius = value;
            }
        }

        /// <summary>
        ///     Centro del cilindro
        /// </summary>
        public Vector3 Center
        {
            get { return BoundingCylinder.Center; }
            set { BoundingCylinder.Center = value; }
        }

        public void render()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;
            var texturesManager = GuiController.Instance.TexturesManager;

            if (AlphaBlendEnable)
            {
                d3dDevice.RenderState.AlphaBlendEnable = true;
                d3dDevice.RenderState.AlphaTestEnable = true;
            }

            if (texture != null)
                texturesManager.shaderSet(Effect, "texDiffuseMap", texture);
            else
                texturesManager.clear(0);
            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(Effect, Transform);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColoredTextured;
            Effect.Technique = Technique;

            var capsResolution = END_CAPS_RESOLUTION;

            Effect.Begin(0);
            Effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2*capsResolution, sideTrianglesVertices);
            Effect.EndPass();
            Effect.End();

            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.AlphaBlendEnable = false;
        }

        public void dispose()
        {
            if (texture != null) texture.dispose();
            sideTrianglesVertices = null;
            BoundingCylinder.dispose();
        }

        public bool AlphaBlendEnable { get; set; }

        private void initialize()
        {
            var capsResolution = END_CAPS_RESOLUTION;

            //cara lateral: un vertice por cada vertice de cada tapa, mas dos para cerrarla
            sideTrianglesVertices = new CustomVertex.PositionColoredTextured[2*capsResolution + 2];

            useColorShader();
            updateValues();
        }

        private void useColorShader()
        {
            Effect = GuiController.Instance.Shaders.VariosShader;
            Technique = TgcShaders.T_POSITION_COLORED;
            useTexture = false;
        }

        private void useTextureShader()
        {
            Technique = TgcShaders.T_POSITION_COLORED_TEXTURED;
            useTexture = true;
        }

        private void updateDraw()
        {
            //vectores utilizados para el dibujado
            var upVector = new Vector3(0, 1, 0);
            var n = new Vector3(1, 0, 0);

            var capsResolution = END_CAPS_RESOLUTION;

            //matriz de rotacion del vector de dibujado
            var angleStep = FastMath.TWO_PI/capsResolution;
            var rotationMatrix = Matrix.RotationAxis(-upVector, angleStep);
            float angle = 0;

            //transformacion que se le aplicara a cada vertice
            var transformation = Transform;
            var bcInverseRadius = 1/BoundingCylinder.Radius;

            //arrays donde guardamos los puntos dibujados
            var topCapDraw = new Vector3[capsResolution];
            var bottomCapDraw = new Vector3[capsResolution];

            //variables temporales utilizadas para el texture mapping
            float u;

            for (var i = 0; i < capsResolution; i++)
            {
                //establecemos los vertices de las tapas
                topCapDraw[i] = upVector + n*TopRadius*bcInverseRadius;
                bottomCapDraw[i] = -upVector + n*BottomRadius*bcInverseRadius;

                u = angle/FastMath.TWO_PI;

                //triangulos de la cara lateral (strip)
                sideTrianglesVertices[2*i] = new CustomVertex.PositionColoredTextured(topCapDraw[i], color, u, 0);
                sideTrianglesVertices[2*i + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[i], color, u, 1);

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);
                angle += angleStep;
            }

            //cerramos la cara lateral
            sideTrianglesVertices[2*capsResolution] = new CustomVertex.PositionColoredTextured(topCapDraw[0], color, 1,
                0);
            sideTrianglesVertices[2*capsResolution + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[0],
                color, 1, 1);
        }

        /// <summary>
        ///     Setea la textura
        /// </summary>
        public void setTexture(TgcTexture _texture)
        {
            if (texture != null)
                texture.dispose();
            texture = _texture;
        }

        /// <summary>
        ///     Actualiza la posicion e inclinacion del cilindro
        /// </summary>
        public void updateValues()
        {
            BoundingCylinder.Radius = FastMath.Max(
                FastMath.Abs(TopRadius), FastMath.Abs(BottomRadius));
            BoundingCylinder.updateValues();
            updateDraw();
        }

        #region Transformation

        public bool AutoTransformEnable { get; set; }

        public Matrix Transform
        {
            get
            {
                if (AutoTransformEnable) return BoundingCylinder.Transform;
                return manualTransformation;
            }
            set { manualTransformation = value; }
        }

        public Vector3 Position
        {
            get { return BoundingCylinder.Center; }
            set { BoundingCylinder.Center = value; }
        }

        public Vector3 Rotation
        {
            get { return BoundingCylinder.Rotation; }
            set { BoundingCylinder.Rotation = value; }
        }

        public Vector3 Scale
        {
            get { return new Vector3(1, 1, 1); }
            set { ; }
        }

        public void move(Vector3 v)
        {
            BoundingCylinder.move(v);
        }

        public void move(float x, float y, float z)
        {
            BoundingCylinder.move(x, y, z);
        }

        public void moveOrientedY(float movement)
        {
            var z = FastMath.Cos(Rotation.Y)*movement;
            var x = FastMath.Sin(Rotation.Y)*movement;
            move(x, 0, z);
        }

        public void getPosition(Vector3 pos)
        {
            pos.X = Position.X;
            pos.Y = Position.Y;
            pos.Z = Position.Z;
        }

        public void rotateX(float angle)
        {
            BoundingCylinder.rotateX(angle);
        }

        public void rotateY(float angle)
        {
            BoundingCylinder.rotateY(angle);
        }

        public void rotateZ(float angle)
        {
            BoundingCylinder.rotateZ(angle);
        }

        #endregion Transformation
    }
}