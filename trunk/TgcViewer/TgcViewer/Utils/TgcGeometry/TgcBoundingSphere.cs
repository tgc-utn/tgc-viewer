using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Shaders;

namespace TgcViewer.Utils.TgcGeometry
{
    /// <summary>
    /// Representa un volumen de esfera con un centro y un radio
    /// </summary>
    public class TgcBoundingSphere : IRenderObject
    {
        /// <summary>
        /// Cantidad de tramos que tendrá el mesh del BoundingSphere a dibujar
        /// </summary>
        public const int SPHERE_MESH_RESOLUTION = 10;

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
        /// Crear BoundingSphere vacia
        /// </summary>
        public TgcBoundingSphere()
        {
            this.renderColor = Color.Yellow.ToArgb();
            this.dirtyValues = true;
            this.alphaBlendEnable = false;
        }

        /// <summary>
        /// Crear BoundingSphere con centro y radio
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radio</param>
        public TgcBoundingSphere(Vector3 center, float radius) : this()
        {
            setValues(center, radius);
        }

        /// <summary>
        /// Configurar valores del BoundingSphere
        /// </summary>
        /// <param name="center">Centro</param>
        /// <param name="radius">Radio</param>
        public void setValues(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;

            this.dirtyValues = true;
        }

        /// <summary>
        /// Configurar un nuevo centro del BoundingSphere
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
        /// Centro de la esfera
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }

        float radius;
        /// <summary>
        /// Radio de la esfera
        /// </summary>
        public float Radius
        {
            get { return radius; }
        }

        int renderColor;
        /// <summary>
        /// Color de renderizado del BoundingBox.
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
        /// Configurar el color de renderizado del BoundingBox
        /// Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            this.renderColor = color.ToArgb();
            dirtyValues = true;
        }
        

        /// <summary>
        /// Construye el mesh del BoundingSphere
        /// </summary>
        private void updateValues()
        {
            if (vertices == null)
            {
                int verticesCount = (SPHERE_MESH_RESOLUTION * 2 + 2) * 3;
                this.vertices = new CustomVertex.PositionColored[verticesCount];
            }

            int index = 0;

            float step = FastMath.TWO_PI / (float)SPHERE_MESH_RESOLUTION;
            // Plano XY
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a) * radius, FastMath.Sin(a) * radius, 0f) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a + step) * radius, FastMath.Sin(a + step) * radius, 0f) + center, renderColor);
            }

            // Plano XZ
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a) * radius, 0f, FastMath.Sin(a) * radius) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(FastMath.Cos(a + step) * radius, 0f, FastMath.Sin(a + step) * radius) + center, renderColor);
            }

            // Plano YZ
            for (float a = 0f; a <= FastMath.TWO_PI; a += step)
            {
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(0f, FastMath.Cos(a) * radius, FastMath.Sin(a) * radius) + center, renderColor);
                vertices[index++] = new CustomVertex.PositionColored(new Vector3(0f, FastMath.Cos(a + step) * radius, FastMath.Sin(a + step) * radius) + center, renderColor);
            }
        }


        /// <summary>
        /// Renderizar el BoundingSphere
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

            //Actualizar vertices de BoundingSphere solo si hubo una modificación
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
            return "Center[" + TgcParserUtils.printFloat(center.X) + ", " + TgcParserUtils.printFloat(center.Y) + ", " + TgcParserUtils.printFloat(center.Z) + "]" + " Radius[" + TgcParserUtils.printFloat(radius) + "]";
        }

        /// <summary>
        /// Convertir a struct
        /// </summary>
        public SphereStruct toStruct()
        {
            SphereStruct sphereStruct = new SphereStruct();
            sphereStruct.center = center;
            sphereStruct.radius = radius;
            return sphereStruct;
        }

        /// <summary>
        /// BoundingSphere en un struct liviano
        /// </summary>
        public struct SphereStruct
        {
            public Vector3 center;
            public float radius;

            /// <summary>
            /// Convertir a clase
            /// </summary>
            public TgcBoundingSphere toClass()
            {
                return new TgcBoundingSphere(center, radius);
            }
        }


        #region Creacion

        /// <summary>
        /// Crea un BoundingSphere a partir de los vertices de un Mesh, utilizando el algoritmo de Ritter
        /// </summary>
        /// <param name="mesh">Mesh a partir del cual crear el BoundingSphere</param>
        /// <returns>BoundingSphere creado</returns>
        public static TgcBoundingSphere computeFromMesh(TgcMesh mesh)
        {
            Vector3[] vertices = mesh.getVertexPositions();
            SphereStruct s = TgcBoundingSphere.computeFromPoints(vertices);
            return s.toClass();
        }

        /// <summary>
        /// Crear un BoundingSphere a partir de un conjunto de puntos, utilizando el algoritmo de Ritter:
        /// [Ritter, Jack. "An Efficient Bounding Sphere," in Andrew Glassner (ed.), Graphics Gems, Academic Press, pp. 301–303, 1990.]
        /// </summary>
        /// <param name="pt">Puntos a partir del cual calcular el BoundingSphere</param>
        /// <returns>BoundingSphere calculado</returns>
        public static SphereStruct computeFromPoints(Vector3[] pt)
        {
            //Get sphere encompassing two approximately most distant points
            SphereStruct s = sphereFromDistantPoints(pt);

            // Grow sphere to include all points
            for (int i = 0; i < pt.Length; i++)
            {
                TgcBoundingSphere.sphereOfSphereAndPt(ref s, pt[i]);
            }

            return s;
        }

        /// <summary>
        /// Given Sphere s and Point p, update s (if needed) to just encompass p
        /// </summary>
        private static void sphereOfSphereAndPt(ref SphereStruct s, Vector3 p)
        {
            // Compute squared distance between point and sphere center
            Vector3 d = p - s.center;
            float dist2 = Vector3.Dot(d, d);
            // Only update s if point p is outside it
            if (dist2 > s.radius * s.radius) {
                float dist = FastMath.Sqrt(dist2);
                float newRadius = (s.radius + dist) * 0.5f;
                float k = (newRadius - s.radius) / dist;
                s.radius = newRadius;
                s.center += d * k;
            }
        }

        /// <summary>
        /// Crear esfera a partir de los puntos mas distintas
        /// </summary>
        private static SphereStruct sphereFromDistantPoints(Vector3[] pt)
        {
            // Find the most separated point pair defining the encompassing AABB
            int min, max;
            TgcBoundingSphere.mostSeparatedPointsOnAABB(pt, out min, out max);

            // Set up sphere to just encompass these two points
            SphereStruct s = new SphereStruct();
            s.center = (pt[min] + pt[max]) * 0.5f;
            s.radius = Vector3.Dot(pt[max] - s.center, pt[max] - s.center);
            s.radius = FastMath.Sqrt(s.radius);
            return s;
        }

        /// <summary>
        /// Compute indices to the two most separated points of the (up to) six points
        /// defining the AABB encompassing the point set. Return these as min and max.
        /// </summary>
        private static void mostSeparatedPointsOnAABB(Vector3[] pt, out int min, out int max)
        {
            // First find most extreme points along principal axes
            int minx = 0, maxx = 0, miny = 0, maxy = 0, minz = 0, maxz = 0;
            for (int i = 0; i < pt.Length; i++)
            {
                if (pt[i].X < pt[minx].X) minx = i;
                if (pt[i].X > pt[maxx].X) maxx = i;
                if (pt[i].Y < pt[miny].Y) miny = i;
                if (pt[i].Y > pt[maxy].Y) maxy = i;
                if (pt[i].Z < pt[minz].Z) minz = i;
                if (pt[i].Z > pt[maxz].Z) maxz = i;
            }
            // Compute the squared distances for the three pairs of points
            float dist2x = Vector3.Dot(pt[maxx] - pt[minx], pt[maxx] - pt[minx]);
            float dist2y = Vector3.Dot(pt[maxy] - pt[miny], pt[maxy] - pt[miny]);
            float dist2z = Vector3.Dot(pt[maxz] - pt[minz], pt[maxz] - pt[minz]);
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

        #endregion

    }
}
