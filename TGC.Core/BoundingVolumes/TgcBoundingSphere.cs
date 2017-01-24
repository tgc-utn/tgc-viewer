using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Core.BoundingVolumes
{
    /// <summary>
    ///     Representa un volumen de esfera con un centro y un radio
    /// </summary>
    public class TgcBoundingSphere : IRenderObject
    {
        /// <summary>
        ///     Cantidad de tramos que tendrá el mesh del BoundingSphere a dibujar
        /// </summary>
        public const int SPHERE_MESH_RESOLUTION = 10;

        private bool dirtyValues;

        protected Effect effect;

        protected string technique;
        private CustomVertex.PositionColored[] vertices;

        /// <summary>
        ///     Crear BoundingSphere vacia
        /// </summary>
        public TgcBoundingSphere()
        {
            RenderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Crear BoundingSphere con centro y radio
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radio</param>
        public TgcBoundingSphere(TGCVector3 center, float radius) : this()
        {
            setValues(center, radius);
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
        ///     Cada vez que se llama a Render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Centro de la esfera
        /// </summary>
        public TGCVector3 Center { get; private set; }

        /// <summary>
        ///     Radio de la esfera
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        ///     Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor { get; private set; }

        public TGCVector3 Position
        {
            get { return Center; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar el BoundingSphere
        /// </summary>
        public void render()
        {
            TexturesManager.Instance.clear(0);
            TexturesManager.Instance.clear(1);

            //Cargar shader si es la primera vez
            if (effect == null)
            {
                effect = TgcShaders.Instance.VariosShader;
                technique = TgcShaders.T_POSITION_COLORED;
            }

            //Actualizar vertices de BoundingSphere solo si hubo una modificación
            if (dirtyValues)
            {
                updateValues();
                dirtyValues = false;
            }

            TgcShaders.Instance.setShaderMatrixIdentity(effect);
            D3DDevice.Instance.Device.VertexDeclaration = TgcShaders.Instance.VdecPositionColored;
            effect.Technique = technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Libera los recursos del objeto
        /// </summary>
        public void dispose()
        {
            vertices = null;
        }

        /// <summary>
        ///     Configurar valores del BoundingSphere
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radio</param>
        public void setValues(TGCVector3 center, float radius)
        {
            Center = center;
            Radius = radius;

            dirtyValues = true;
        }

        /// <summary>
        ///     Configurar un nuevo centro del BoundingSphere
        /// </summary>
        /// <param name="center">Nuevo centro</param>
        public void setCenter(TGCVector3 center)
        {
            setValues(center, Radius);
        }

        /// <summary>
        ///     Desplazar el centro respecto de su posición actual
        /// </summary>
        /// <param name="movement">Movimiento relativo a realizar</param>
        public void moveCenter(TGCVector3 movement)
        {
            setValues(Center + movement, Radius);
        }

        /// <summary>
        ///     Configurar el color de renderizado del BoundingBox
        ///     Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            RenderColor = color.ToArgb();
            dirtyValues = true;
        }

        /// <summary>
        ///     Construye el mesh del BoundingSphere
        /// </summary>
        private void updateValues()
        {
            if (vertices == null)
            {
                var verticesCount = (SPHERE_MESH_RESOLUTION * 2 + 2) * 3;
                vertices = new CustomVertex.PositionColored[verticesCount];
            }

            var index = 0;

            var step = FastMath.TWO_PI / SPHERE_MESH_RESOLUTION;
            // Plano XY
            for (var a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(FastMath.Cos(a) * Radius, FastMath.Sin(a) * Radius, 0f)).ToVector3() + Center.ToVector3(), RenderColor);
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(FastMath.Cos(a + step) * Radius, FastMath.Sin(a + step) * Radius, 0f)).ToVector3() + Center.ToVector3(),
                        RenderColor);
            }

            // Plano XZ
            for (var a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(FastMath.Cos(a) * Radius, 0f, FastMath.Sin(a) * Radius)).ToVector3() + Center.ToVector3(), RenderColor);
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(FastMath.Cos(a + step) * Radius, 0f, FastMath.Sin(a + step) * Radius)).ToVector3() + Center.ToVector3(), RenderColor);
            }

            // Plano YZ
            for (var a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(0f, FastMath.Cos(a) * Radius, FastMath.Sin(a) * Radius)).ToVector3() + Center.ToVector3(), RenderColor);
                vertices[index++] =
                    new CustomVertex.PositionColored(
                        (new TGCVector3(0f, FastMath.Cos(a + step) * Radius, FastMath.Sin(a + step) * Radius)).ToVector3() + Center.ToVector3(),
                        RenderColor);
            }
        }

        public override string ToString()
        {
            return "Center[" + TgcParserUtils.printFloat(Center.X) + ", " + TgcParserUtils.printFloat(Center.Y) + ", " +
                   TgcParserUtils.printFloat(Center.Z) + "]" + " Radius[" + TgcParserUtils.printFloat(Radius) + "]";
        }

        /// <summary>
        ///     Convertir a struct
        /// </summary>
        public SphereStruct toStruct()
        {
            var sphereStruct = new SphereStruct();
            sphereStruct.center = Center;
            sphereStruct.radius = Radius;
            return sphereStruct;
        }

        /// <summary>
        ///     BoundingSphere en un struct liviano
        /// </summary>
        public struct SphereStruct
        {
            public TGCVector3 center;
            public float radius;

            /// <summary>
            ///     Convertir a clase
            /// </summary>
            public TgcBoundingSphere toClass()
            {
                return new TgcBoundingSphere(center, radius);
            }
        }

        #region Creacion

        /// <summary>
        ///     Crea un BoundingSphere a partir de los vertices de un Mesh, utilizando el algoritmo de Ritter
        /// </summary>
        /// <param name="mesh">Mesh a partir del cual crear el BoundingSphere</param>
        /// <returns>BoundingSphere creado</returns>
        public static TgcBoundingSphere computeFromMesh(TgcMesh mesh)
        {
            var vertices = mesh.getVertexPositions();
            var s = computeFromPoints(vertices);
            return s.toClass();
        }

        /// <summary>
        ///     Crear un BoundingSphere a partir de un conjunto de puntos, utilizando el algoritmo de Ritter:
        ///     [Ritter, Jack. "An Efficient Bounding Sphere," in Andrew Glassner (ed.), Graphics Gems, Academic Press, pp.
        ///     301–303, 1990.]
        /// </summary>
        /// <param name="pt">Puntos a partir del cual calcular el BoundingSphere</param>
        /// <returns>BoundingSphere calculado</returns>
        public static SphereStruct computeFromPoints(TGCVector3[] pt)
        {
            //Get sphere encompassing two approximately most distant points
            var s = sphereFromDistantPoints(pt);

            // Grow sphere to include all points
            for (var i = 0; i < pt.Length; i++)
            {
                sphereOfSphereAndPt(ref s, pt[i]);
            }

            return s;
        }

        /// <summary>
        ///     Given Sphere s and Point p, update s (if needed) to just encompass p
        /// </summary>
        private static void sphereOfSphereAndPt(ref SphereStruct s, TGCVector3 p)
        {
            // Compute squared distance between point and sphere center
            var d = p - s.center;
            var dist2 = TGCVector3.Dot(d, d);
            // Only update s if point p is outside it
            if (dist2 > s.radius * s.radius)
            {
                var dist = FastMath.Sqrt(dist2);
                var newRadius = (s.radius + dist) * 0.5f;
                var k = (newRadius - s.radius) / dist;
                s.radius = newRadius;
                s.center += d * k;
            }
        }

        /// <summary>
        ///     Crear esfera a partir de los puntos mas distintas
        /// </summary>
        private static SphereStruct sphereFromDistantPoints(TGCVector3[] pt)
        {
            // Find the most separated point pair defining the encompassing AABB
            int min, max;
            mostSeparatedPointsOnAABB(pt, out min, out max);

            // Set up sphere to just encompass these two points
            var s = new SphereStruct();
            s.center = (pt[min] + pt[max]) * 0.5f;
            s.radius = TGCVector3.Dot(pt[max] - s.center, pt[max] - s.center);
            s.radius = FastMath.Sqrt(s.radius);
            return s;
        }

        /// <summary>
        ///     Compute indices to the two most separated points of the (up to) six points
        ///     defining the AABB encompassing the point set. Return these as min and max.
        /// </summary>
        private static void mostSeparatedPointsOnAABB(TGCVector3[] pt, out int min, out int max)
        {
            // First find most extreme points along principal axes
            int minx = 0, maxx = 0, miny = 0, maxy = 0, minz = 0, maxz = 0;
            for (var i = 0; i < pt.Length; i++)
            {
                if (pt[i].X < pt[minx].X) minx = i;
                if (pt[i].X > pt[maxx].X) maxx = i;
                if (pt[i].Y < pt[miny].Y) miny = i;
                if (pt[i].Y > pt[maxy].Y) maxy = i;
                if (pt[i].Z < pt[minz].Z) minz = i;
                if (pt[i].Z > pt[maxz].Z) maxz = i;
            }
            // Compute the squared distances for the three pairs of points
            var dist2x = TGCVector3.Dot(pt[maxx] - pt[minx], pt[maxx] - pt[minx]);
            var dist2y = TGCVector3.Dot(pt[maxy] - pt[miny], pt[maxy] - pt[miny]);
            var dist2z = TGCVector3.Dot(pt[maxz] - pt[minz], pt[maxz] - pt[minz]);
            // Pick the pair (min,max) of points most distant
            min = minx;
            max = maxx;
            if (dist2y > dist2x && dist2y > dist2z)
            {
                max = maxy;
                min = miny;
            }
            if (dist2z > dist2x && dist2z > dist2y)
            {
                max = maxz;
                min = minz;
            }
        }

        #endregion Creacion
    }
}