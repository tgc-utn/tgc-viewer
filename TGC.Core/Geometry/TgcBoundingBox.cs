using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Core.Geometry
{
    /// <summary>
    ///     Representa un BoundingBox
    /// </summary>
    public class TgcBoundingBox : IRenderObject
    {
        private bool dirtyValues;

        protected Effect effect;

        private Vector3 pMax;
        private Vector3 pMaxOriginal;

        private Vector3 pMin;

        private Vector3 pMinOriginal;

        protected string technique;

        private CustomVertex.PositionColored[] vertices;

        /// <summary>
        ///     Construir AABB vacio
        /// </summary>
        public TgcBoundingBox()
        {
            RenderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Construir AABB
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        public TgcBoundingBox(Vector3 pMin, Vector3 pMax)
            : this()
        {
            setExtremes(pMin, pMax);
        }

        /// <summary>
        ///     Construye un AABB a partir de un pMin y pMax que ya tienen aplicada una escala y una traslacion.
        /// </summary>
        /// <param name="pMin">Punto minimo escalado y/o trasladado</param>
        /// <param name="pMax">Punto maximo escalado y/o trasladado</param>
        /// <param name="position">Traslacion</param>
        /// <param name="scale">Escala</param>
        public TgcBoundingBox(Vector3 pMin, Vector3 pMax, Vector3 position, Vector3 scale)
            : this()
        {
            //Seteo los extremos
            this.pMin = pMin;
            this.pMax = pMax;

            //Almaceno los extremos sin transformar para aplicar las futuras transformaciones sobre los puntos originales.
            pMinOriginal.X = (pMin.X - position.X) / scale.X;
            pMinOriginal.Y = (pMin.Y - position.Y) / scale.Y;
            pMinOriginal.Z = (pMin.Z - position.Z) / scale.Z;

            pMaxOriginal.X = (pMax.X - position.X) / scale.X;
            pMaxOriginal.Y = (pMax.Y - position.Y) / scale.Y;
            pMaxOriginal.Z = (pMax.Z - position.Z) / scale.Z;
        }

        /// <summary>
        ///     Punto mínimo del BoundingBox
        /// </summary>
        public Vector3 PMin
        {
            get { return pMin; }
        }

        /// <summary>
        ///     Punto máximo del BoundinBox
        /// </summary>
        public Vector3 PMax
        {
            get { return pMax; }
        }

        /// <summary>
        ///     Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor { get; private set; }

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

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            //get { return calculateBoxCenter(); }
            get { return pMin; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar BoundingBox
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

            //Actualizar vertices de BoundingBox solo si hubo una modificación
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
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
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
        ///     Configurar los valores extremos del BoundingBox
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        public void setExtremes(Vector3 pMin, Vector3 pMax)
        {
            this.pMin = pMin;
            this.pMax = pMax;
            pMinOriginal = pMin;
            pMaxOriginal = pMax;

            dirtyValues = true;
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

        public override string ToString()
        {
            return "Min[" + TgcParserUtils.printFloat(pMin.X) + ", " + TgcParserUtils.printFloat(pMin.Y) + ", " +
                   TgcParserUtils.printFloat(pMin.Z) + "]" +
                   " Max[" + TgcParserUtils.printFloat(pMax.X) + ", " + TgcParserUtils.printFloat(pMax.Y) + ", " +
                   TgcParserUtils.printFloat(pMax.Z) + "]";
        }

        /// <summary>
        ///     Radio al cuadrado del BoundingBox
        /// </summary>
        public float calculateBoxRadiusSquare()
        {
            var diff = Vector3.Subtract(pMax, pMin);
            diff.Scale(0.5f);
            return diff.LengthSq();
        }

        /// <summary>
        ///     Radio del BoundingBox
        /// </summary>
        public float calculateBoxRadius()
        {
            var rsq = calculateBoxRadiusSquare();
            return FastMath.Sqrt(rsq);
        }

        /// <summary>
        ///     Centro del Bounding Box
        /// </summary>
        public Vector3 calculateBoxCenter()
        {
            var axisRadius = calculateAxisRadius();
            return Vector3.Add(pMin, axisRadius);
        }

        /// <summary>
        ///     Tamaño de cada dimensión del BoundingBox
        /// </summary>
        public Vector3 calculateSize()
        {
            return Vector3.Subtract(pMax, pMin);
        }

        /// <summary>
        ///     Devuelve el radio de cada eje (o Extents)
        /// </summary>
        public Vector3 calculateAxisRadius()
        {
            var size = calculateSize();
            size.Multiply(0.5f);
            return size;
        }

        /// <summary>
        ///     <summary>
        ///         Traslada y escala el BoundingBox.
        ///         Si el BoundingBox tenia alguna rotación, se pierde.
        ///     </summary>
        ///     <param name="position">Nueva posición absoluta de referencia</param>
        ///     <param name="scale">Nueva escala absoluta de referencia</param>
        public void scaleTranslate(Vector3 position, Vector3 scale)
        {
            //actualizar puntos extremos
            pMin.X = pMinOriginal.X * scale.X + position.X;
            pMin.Y = pMinOriginal.Y * scale.Y + position.Y;
            pMin.Z = pMinOriginal.Z * scale.Z + position.Z;

            pMax.X = pMaxOriginal.X * scale.X + position.X;
            pMax.Y = pMaxOriginal.Y * scale.Y + position.Y;
            pMax.Z = pMaxOriginal.Z * scale.Z + position.Z;

            dirtyValues = true;
        }

        /// <summary>
        ///     Mueve el BoundingBox
        /// </summary>
        /// <param name="movement">Movimiento relativo que se quiere aplicar</param>
        public void move(Vector3 movement)
        {
            pMin += movement;
            pMax += movement;

            dirtyValues = true;
        }

        /// <summary>
        ///     Transforma el BondingBox en base a una matriz de transformación.
        ///     Esto implica escalar, rotar y trasladar.
        ///     El procedimiento es mas costoso que solo hacer scaleTranslate().
        ///     Se construye un nuevo BoundingBox en base a los puntos extremos del original
        ///     más la transformación pedida.
        ///     Si el BoundingBox se transformó y luego se llama a scaleTranslate(), se respeta
        ///     la traslación y la escala, pero la rotación se va a perder.
        /// </summary>
        /// <param name="transform"></param>
        public void transform(Matrix transform)
        {
            //Transformar vertices extremos originales
            var corners = computeCorners(pMinOriginal, pMaxOriginal);
            var newCorners = new Vector3[corners.Length];
            for (var i = 0; i < corners.Length; i++)
            {
                newCorners[i] = TgcVectorUtils.transform(corners[i], transform);
            }

            //Calcular nuevo BoundingBox en base a extremos transformados
            var newBB = computeFromPoints(newCorners);

            //actualizar solo pMin y pMax, pMinOriginal y pMaxOriginal quedan sin ser transformados
            pMin = newBB.pMin;
            pMax = newBB.pMax;

            dirtyValues = true;
        }

        /// <summary>
        ///     Actualizar los valores de los vertices a renderizar
        /// </summary>
        private void updateValues()
        {
            if (vertices == null)
            {
                vertices = vertices = new CustomVertex.PositionColored[24];
            }

            //Cuadrado de atras
            vertices[0] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, RenderColor);
            vertices[1] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, RenderColor);

            vertices[2] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, RenderColor);
            vertices[3] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, RenderColor);

            vertices[4] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, RenderColor);
            vertices[5] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, RenderColor);

            vertices[6] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, RenderColor);
            vertices[7] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, RenderColor);

            //Cuadrado de adelante
            vertices[8] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, RenderColor);
            vertices[9] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, RenderColor);

            vertices[10] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, RenderColor);
            vertices[11] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, RenderColor);

            vertices[12] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, RenderColor);
            vertices[13] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, RenderColor);

            vertices[14] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, RenderColor);
            vertices[15] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, RenderColor);

            //Union de ambos cuadrados
            vertices[16] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, RenderColor);
            vertices[17] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, RenderColor);

            vertices[18] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, RenderColor);
            vertices[19] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, RenderColor);

            vertices[20] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, RenderColor);
            vertices[21] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, RenderColor);

            vertices[22] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, RenderColor);
            vertices[23] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, RenderColor);
        }

        /// <summary>
        ///     Crea un array con los 8 vertices del BoundingBox, en base a los extremos especificados
        /// </summary>
        private Vector3[] computeCorners(Vector3 min, Vector3 max)
        {
            var corners = new Vector3[8];

            corners[0] = new Vector3(min.X, min.Y, min.Z);
            corners[1] = new Vector3(min.X, min.Y, max.Z);

            corners[2] = new Vector3(min.X, max.Y, min.Z);
            corners[3] = new Vector3(min.X, max.Y, max.Z);

            corners[4] = new Vector3(max.X, min.Y, min.Z);
            corners[5] = new Vector3(max.X, min.Y, max.Z);

            corners[6] = new Vector3(max.X, max.Y, min.Z);
            corners[7] = new Vector3(max.X, max.Y, max.Z);

            return corners;
        }

        /// <summary>
        ///     Crea un array con los 8 vertices del BoundingBox
        /// </summary>
        public Vector3[] computeCorners()
        {
            return computeCorners(pMin, pMax);
        }

        /// <summary>
        ///     Calcula los polígonos que conforman las 6 caras del BoundingBox
        /// </summary>
        /// <returns>Array con las 6 caras del polígono en el siguiente orden: Up, Down, Front, Back, Right, Left</returns>
        public Face[] computeFaces()
        {
            var faces = new Face[6];
            Face face;

            //Up
            face = new Face();
            face.Plane = new Plane(0, 1, 0, -pMax.Y);
            face.Extremes = new[]
            {
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[0] = face;

            //Down
            face = new Face();
            face.Plane = new Plane(0, -1, 0, pMin.Y);
            face.Extremes = new[]
            {
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z)
            };
            faces[1] = face;

            //Front
            face = new Face();
            face.Plane = new Plane(0, 0, 1, -pMax.Z);
            face.Extremes = new[]
            {
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[2] = face;

            //Back
            face = new Face();
            face.Plane = new Plane(0, 0, -1, pMin.Z);
            face.Extremes = new[]
            {
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z)
            };
            faces[3] = face;

            //Right
            face = new Face();
            face.Plane = new Plane(1, 0, 0, -pMax.X);
            face.Extremes = new[]
            {
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[4] = face;

            //Left
            face = new Face();
            face.Plane = new Plane(-1, 0, 0, pMin.X);
            face.Extremes = new[]
            {
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z)
            };
            faces[5] = face;

            return faces;
        }

        /// <summary>
        ///     Crear un BoundingBox igual a este
        /// </summary>
        /// <returns>BoundingBox clonado</returns>
        public TgcBoundingBox clone()
        {
            var cloneBbox = new TgcBoundingBox();
            cloneBbox.pMin = pMin;
            cloneBbox.pMax = pMax;
            cloneBbox.pMinOriginal = pMinOriginal;
            cloneBbox.pMaxOriginal = pMaxOriginal;

            return cloneBbox;
        }

        /// <summary>
        ///     Proyecta el BoundingBox a un rectangulo 2D de screen space
        /// </summary>
        /// <returns>Rectangulo 2D con proyeccion en screen space</returns>
        public Rectangle projectToScreen()
        {
            var viewport = D3DDevice.Instance.Device.Viewport;
            var world = D3DDevice.Instance.Device.Transform.World;
            var view = D3DDevice.Instance.Device.Transform.View;
            var proj = D3DDevice.Instance.Device.Transform.Projection;

            //Proyectar los 8 corners del BoundingBox
            var projVertices = computeCorners();
            for (var i = 0; i < projVertices.Length; i++)
            {
                projVertices[i] = Vector3.Project(projVertices[i], viewport, proj, view, world);
            }

            //Buscar los puntos extremos
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var v in projVertices)
            {
                if (v.X < min.X)
                {
                    min.X = v.X;
                }
                if (v.Y < min.Y)
                {
                    min.Y = v.Y;
                }
                if (v.X > max.X)
                {
                    max.X = v.X;
                }
                if (v.Y > max.Y)
                {
                    max.Y = v.Y;
                }
            }
            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        /// <summary>
        ///     Convertir a struct
        /// </summary>
        public AABBStruct toStruct()
        {
            var aabbStruct = new AABBStruct();
            aabbStruct.min = pMin;
            aabbStruct.max = pMax;
            return aabbStruct;
        }

        /// <summary>
        ///     Cara de un BoundingBox representada por un polígono rectangular de 4 vértices.
        /// </summary>
        public class Face
        {
            public Face()
            {
                Extremes = new Vector3[4];
            }

            /// <summary>
            ///     Los 4 vértices extremos de la cara
            /// </summary>
            public Vector3[] Extremes { get; set; }

            /// <summary>
            ///     Ecuación del plano que engloba la cara, con su normal apuntado hacia afuera normalizada.
            /// </summary>
            public Plane Plane { get; set; }
        }

        /// <summary>
        ///     BoundingBox en un struct liviano
        /// </summary>
        public struct AABBStruct
        {
            public Vector3 min;
            public Vector3 max;

            /// <summary>
            ///     Convertir a clase
            /// </summary>
            public TgcBoundingBox toClass()
            {
                return new TgcBoundingBox(min, max);
            }
        }

        #region Creacion

        /// <summary>
        ///     Crea un BoundingBox que contenga a todos los BoundingBoxes especificados
        /// </summary>
        /// <param name="boundingBoxes">Lista BoundingBoxes a contener</param>
        /// <returns>BoundingBox creado</returns>
        public static TgcBoundingBox computeFromBoundingBoxes(List<TgcBoundingBox> boundingBoxes)
        {
            var points = new Vector3[boundingBoxes.Count * 2];
            for (var i = 0; i < boundingBoxes.Count; i++)
            {
                points[i * 2] = boundingBoxes[i].pMin;
                points[i * 2 + 1] = boundingBoxes[i].pMax;
            }
            return computeFromPoints(points);
        }

        /// <summary>
        ///     Crea un BoundingBox a partir de un conjunto de puntos.
        /// </summary>
        /// <param name="points">Puntos a conentener</param>
        /// <returns>BoundingBox creado</returns>
        public static TgcBoundingBox computeFromPoints(Vector3[] points)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var p in points)
            {
                //min
                if (p.X < min.X)
                {
                    min.X = p.X;
                }
                if (p.Y < min.Y)
                {
                    min.Y = p.Y;
                }
                if (p.Z < min.Z)
                {
                    min.Z = p.Z;
                }

                //max
                if (p.X > max.X)
                {
                    max.X = p.X;
                }
                if (p.Y > max.Y)
                {
                    max.Y = p.Y;
                }
                if (p.Z > max.Z)
                {
                    max.Z = p.Z;
                }
            }

            return new TgcBoundingBox(min, max);
        }

        #endregion Creacion
    }
}