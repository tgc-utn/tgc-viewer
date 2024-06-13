using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;

namespace TGC.Core.Geometry
{
    public class TgcCylinder : IRenderObject, ITransformObject
    {
        private const int END_CAPS_RESOLUTION = 40;
        private int color;

        private TGCMatrix manualTransformation;
        private CustomVertex.PositionColoredTextured[] sideTrianglesVertices; //triangle strip
        private TGCTexture texture;

        private bool useTexture;

        public TgcCylinder(TGCVector3 _center, float _topRadius, float _bottomRadius, float _halfLength)
        {
            TopRadius = _topRadius;
            BottomRadius = _bottomRadius;
            BoundingCylinder = new TgcBoundingCylinder(_center, FastMath.Max(TopRadius, BottomRadius), _halfLength);

            color = Color.Red.ToArgb();

            manualTransformation = TGCMatrix.Identity;
            AutoTransformEnable = false;

            initialize();
        }

        public TgcCylinder(TGCVector3 _center, float _radius, float _halfLength)
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
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
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
        public TGCVector3 Center
        {
            get { return BoundingCylinder.Center; }
            set { BoundingCylinder.Center = value; }
        }

        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Actualiza la posicion e inclinacion del cilindro
        /// </summary>
        public void updateValues()
        {
            BoundingCylinder.Radius = FastMath.Max(FastMath.Abs(TopRadius), FastMath.Abs(BottomRadius));
            BoundingCylinder.updateValues();
            updateDraw();
        }

        public void Render()
        {
            if (AlphaBlendEnable)
            {
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
                D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
            }

            if (texture != null)
                TexturesManager.Instance.shaderSet(Effect, "texDiffuseMap", texture);
            else
                TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            TGCShaders.Instance.SetShaderMatrix(Effect, Transform);
            D3DDevice.Instance.Device.VertexDeclaration = TGCShaders.Instance.VdecPositionColoredTextured;
            Effect.Technique = Technique;

            var capsResolution = END_CAPS_RESOLUTION;

            Effect.Begin(0);
            Effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2 * capsResolution,
                sideTrianglesVertices);
            Effect.EndPass();
            Effect.End();

            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
        }

        public void Dispose()
        {
            if (texture != null) texture.Dispose();
            sideTrianglesVertices = null;
            BoundingCylinder.Dispose();
        }

        private void initialize()
        {
            var capsResolution = END_CAPS_RESOLUTION;

            //cara lateral: un vertice por cada vertice de cada tapa, mas dos para cerrarla
            sideTrianglesVertices = new CustomVertex.PositionColoredTextured[2 * capsResolution + 2];

            useColorShader();
            updateValues();
        }

        private void useColorShader()
        {
            Effect = TGCShaders.Instance.VariosShader;
            Technique = TGCShaders.T_POSITION_COLORED;
            useTexture = false;
        }

        private void useTextureShader()
        {
            Technique = TGCShaders.T_POSITION_COLORED_TEXTURED;
            useTexture = true;
        }

        private void updateDraw()
        {
            //vectores utilizados para el dibujado
            var upVector = TGCVector3.Up;
            var n = new TGCVector3(1, 0, 0);

            var capsResolution = END_CAPS_RESOLUTION;

            //matriz de rotacion del vector de dibujado
            var angleStep = FastMath.TWO_PI / capsResolution;
            var rotationMatrix = TGCMatrix.RotationAxis(-upVector, angleStep);
            float angle = 0;

            //transformacion que se le aplicara a cada vertice
            var transformation = Transform;
            var bcInverseRadius = 1 / BoundingCylinder.Radius;

            //arrays donde guardamos los puntos dibujados
            var topCapDraw = new TGCVector3[capsResolution];
            var bottomCapDraw = new TGCVector3[capsResolution];

            //variables temporales utilizadas para el texture mapping
            float u;

            for (var i = 0; i < capsResolution; i++)
            {
                //establecemos los vertices de las tapas
                topCapDraw[i] = upVector + n * TopRadius * bcInverseRadius;
                bottomCapDraw[i] = -upVector + n * BottomRadius * bcInverseRadius;

                u = angle / FastMath.TWO_PI;

                //triangulos de la cara lateral (strip)
                sideTrianglesVertices[2 * i] = new CustomVertex.PositionColoredTextured(topCapDraw[i], color, u, 0);
                sideTrianglesVertices[2 * i + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[i], color, u, 1);

                //rotamos el vector de dibujado
                n.TransformNormal(rotationMatrix);
                angle += angleStep;
            }

            //cerramos la cara lateral
            sideTrianglesVertices[2 * capsResolution] = new CustomVertex.PositionColoredTextured(topCapDraw[0], color, 1,
                0);
            sideTrianglesVertices[2 * capsResolution + 1] = new CustomVertex.PositionColoredTextured(bottomCapDraw[0],
                color, 1, 1);
        }

        /// <summary>
        ///     Setea la textura
        /// </summary>
        public void setTexture(TGCTexture _texture)
        {
            if (texture != null)
                texture.Dispose();
            texture = _texture;
        }

        #region Transformation

        /// <summary>
        ///     En True hace que la matriz de transformacion (Transform) de la malla se actualiza en
        ///     cada cuadro en forma automática, según los valores de: Position, Rotation, Scale.
        ///     En False se respeta lo que el usuario haya cargado a mano en la matriz.
        ///     Por default está en True.
        /// </summary>
        [Obsolete("Utilizar esta propiedad en juegos complejos se pierde el control, es mejor utilizar transformaciones con matrices.")]
        public bool AutoTransformEnable { get; set; }

        public TGCMatrix Transform
        {
            get
            {
                if (AutoTransformEnable) return BoundingCylinder.Transform;
                return manualTransformation;
            }
            set { manualTransformation = value; }
        }

        public TGCVector3 Position
        {
            get { return BoundingCylinder.Center; }
            set { BoundingCylinder.Center = value; }
        }

        public TGCVector3 Rotation
        {
            get { return BoundingCylinder.Rotation; }
            set { BoundingCylinder.Rotation = value; }
        }

        public TGCVector3 Scale
        {
            get { return TGCVector3.One; }
            set { Console.WriteLine("TODO esta bien que pase por aca?"); }
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        public void Move(TGCVector3 v)
        {
            BoundingCylinder.move(v);
        }

        /// <summary>
        ///     Desplaza la malla la distancia especificada, respecto de su posicion actual
        /// </summary>
        [Obsolete]
        public void Move(float x, float y, float z)
        {
            BoundingCylinder.move(x, y, z);
        }

        /// <summary>
        ///     Mueve la malla en base a la orientacion actual de rotacion.
        ///     Es necesario rotar la malla primero
        /// </summary>
        /// <param name="movement">Desplazamiento. Puede ser positivo (hacia adelante) o negativo (hacia atras)</param>
        [Obsolete]
        public void MoveOrientedY(float movement)
        {
            var z = FastMath.Cos(Rotation.Y) * movement;
            var x = FastMath.Sin(Rotation.Y) * movement;
            Move(x, 0, z);
        }

        public void GetPosition(TGCVector3 pos)
        {
            pos.X = Position.X;
            pos.Y = Position.Y;
            pos.Z = Position.Z;
        }

        /// <summary>
        ///     Rota la malla respecto del eje X
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateX(float angle)
        {
            BoundingCylinder.rotateX(angle);
        }

        /// <summary>
        ///     Rota la malla respecto del eje Y
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateY(float angle)
        {
            BoundingCylinder.rotateY(angle);
        }

        /// <summary>
        ///     Rota la malla respecto del eje Z
        /// </summary>
        /// <param name="angle">Ángulo de rotación en radianes</param>
        [Obsolete]
        public void RotateZ(float angle)
        {
            BoundingCylinder.rotateZ(angle);
        }

        #endregion Transformation
    }
}