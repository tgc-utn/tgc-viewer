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
    /// Representa un BoundingBox
    /// </summary>
    public class TgcBoundingBox : IRenderObject
    {

        #region Creacion


        /// <summary>
        /// Crea un BoundingBox que contenga a todos los BoundingBoxes especificados
        /// </summary>
        /// <param name="boundingBoxes">Lista BoundingBoxes a contener</param>
        /// <returns>BoundingBox creado</returns>
        public static TgcBoundingBox computeFromBoundingBoxes(List<TgcBoundingBox> boundingBoxes)
        {
            Vector3[] points = new Vector3[boundingBoxes.Count * 2];
            for (int i = 0; i < boundingBoxes.Count; i++)
			{
                points[i * 2] = boundingBoxes[i].pMin;
                points[i * 2 + 1] = boundingBoxes[i].pMax;
			}
            return computeFromPoints(points);
        }

        /// <summary>
        /// Crea un BoundingBox a partir de un conjunto de puntos.
        /// </summary>
        /// <param name="points">Puntos a conentener</param>
        /// <returns>BoundingBox creado</returns>
        public static TgcBoundingBox computeFromPoints(Vector3[] points)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (Vector3 p in points)
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

        #endregion


        Vector3 pMinOriginal;
        Vector3 pMaxOriginal;

        Vector3 pMin;
        /// <summary>
        /// Punto mínimo del BoundingBox
        /// </summary>
        public Vector3 PMin
        {
            get { return pMin; }
        }

        Vector3 pMax;
        /// <summary>
        /// Punto máximo del BoundinBox
        /// </summary>
        public Vector3 PMax
        {
            get { return pMax; }
        }


        int renderColor;
        /// <summary>
        /// Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor
        {
            get { return renderColor; }
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



        CustomVertex.PositionColored[] vertices;
        bool dirtyValues;

        /// <summary>
        /// Construir AABB vacio
        /// </summary>
        public TgcBoundingBox()
        {
            renderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            alphaBlendEnable = false;
        }

        /// <summary>
        /// Construir AABB
        /// </summary>
        /// <param name="pMin">Punto mínimo</param>
        /// <param name="pMax">Punto máximo</param>
        public TgcBoundingBox(Vector3 pMin, Vector3 pMax) 
            : this()
        {
            setExtremes(pMin, pMax);
        }

        /// <summary>
        /// Configurar los valores extremos del BoundingBox
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
        /// Configurar el color de renderizado del BoundingBox
        /// Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            this.renderColor = color.ToArgb();
            dirtyValues = true;
        }

        public override string ToString()
        {
            return "Min[" + TgcParserUtils.printFloat(pMin.X) + ", " + TgcParserUtils.printFloat(pMin.Y) + ", " + TgcParserUtils.printFloat(pMin.Z) + "]" +
                " Max[" + TgcParserUtils.printFloat(pMax.X) + ", " + TgcParserUtils.printFloat(pMax.Y) + ", " + TgcParserUtils.printFloat(pMax.Z) + "]";
        }


        /// <summary>
        /// Radio al cuadrado del BoundingBox
        /// </summary>
        public float calculateBoxRadiusSquare()
        {
            Vector3 diff = Vector3.Subtract(pMax, pMin);
            diff.Scale(0.5f);
            return diff.LengthSq();
        }

        /// <summary>
        /// Radio del BoundingBox
        /// </summary>
        public float calculateBoxRadius()
        {
            float rsq = calculateBoxRadiusSquare();
            return FastMath.Sqrt(rsq);
        }

        /// <summary>
        /// Centro del Bounding Box
        /// </summary>
        public Vector3 calculateBoxCenter()
        {
            Vector3 axisRadius = calculateAxisRadius();
            return Vector3.Add(pMin, axisRadius);
        }

        /// <summary>
        /// Tamaño de cada dimensión del BoundingBox
        /// </summary>
        public Vector3 calculateSize()
        {
            return Vector3.Subtract(pMax, pMin);
        }

        /// <summary>
        /// Devuelve el radio de cada eje (o Extents)
        /// </summary>
        public Vector3 calculateAxisRadius()
        {
            Vector3 size = calculateSize();
            size.Multiply(0.5f);
            return size;
        }

        public Vector3 Position
        {
            //Lo correcto sería calcular el centro, pero con un extremo es suficiente.
            //get { return calculateBoxCenter(); }
            get { return pMin; }
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
        /// <summary>
        /// Traslada y escala el BoundingBox.
        /// Si el BoundingBox tenia alguna rotación, se pierde.
        /// </summary>
        /// <param name="position">Nueva posición absoluta de referencia</param>
        /// <param name="scale">Nueva escala absoluta de referencia</param>
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
        /// Mueve el BoundingBox
        /// </summary>
        /// <param name="movement">Movimiento relativo que se quiere aplicar</param>
        public void move(Vector3 movement)
        {
            pMin += movement;
            pMax += movement;

            dirtyValues = true;
        }

        /// <summary>
        /// Transforma el BondingBox en base a una matriz de transformación.
        /// Esto implica escalar, rotar y trasladar.
        /// El procedimiento es mas costoso que solo hacer scaleTranslate().
        /// Se construye un nuevo BoundingBox en base a los puntos extremos del original
        /// más la transformación pedida.
        /// Si el BoundingBox se transformó y luego se llama a scaleTranslate(), se respeta
        /// la traslación y la escala, pero la rotación se va a perder.
        /// </summary>
        /// <param name="transform"></param>
        public void transform(Matrix transform)
        {
            //Transformar vertices extremos originales
            Vector3[] corners = computeCorners(pMinOriginal, pMaxOriginal);
            Vector3[] newCorners = new Vector3[corners.Length];
            for (int i = 0; i < corners.Length; i++)
            {
                
                newCorners[i] = TgcVectorUtils.transform(corners[i], transform);
            }

            //Calcular nuevo BoundingBox en base a extremos transformados
            TgcBoundingBox newBB = TgcBoundingBox.computeFromPoints(newCorners);

            //actualizar solo pMin y pMax, pMinOriginal y pMaxOriginal quedan sin ser transformados
            pMin = newBB.pMin;
            pMax = newBB.pMax;

            dirtyValues = true;
        }

        /// <summary>
        /// Actualizar los valores de los vertices a renderizar
        /// </summary>
        private void updateValues()
        {
            if (vertices == null)
            {
                vertices = vertices = new CustomVertex.PositionColored[24];
            }

            //Cuadrado de atras
            vertices[0] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);
            vertices[1] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, renderColor);

            vertices[2] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);
            vertices[3] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);

            vertices[4] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);
            vertices[5] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, renderColor);

            vertices[6] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, renderColor);
            vertices[7] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, renderColor);

            //Cuadrado de adelante
            vertices[8] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, renderColor);
            vertices[9] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);

            vertices[10] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, renderColor);
            vertices[11] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, renderColor);

            vertices[12] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, renderColor);
            vertices[13] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);

            vertices[14] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);
            vertices[15] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);

            //Union de ambos cuadrados
            vertices[16] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMin.Z, renderColor);
            vertices[17] = new CustomVertex.PositionColored(pMin.X, pMin.Y, pMax.Z, renderColor);

            vertices[18] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMin.Z, renderColor);
            vertices[19] = new CustomVertex.PositionColored(pMax.X, pMin.Y, pMax.Z, renderColor);

            vertices[20] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMin.Z, renderColor);
            vertices[21] = new CustomVertex.PositionColored(pMin.X, pMax.Y, pMax.Z, renderColor);

            vertices[22] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMin.Z, renderColor);
            vertices[23] = new CustomVertex.PositionColored(pMax.X, pMax.Y, pMax.Z, renderColor);
        }


        /// <summary>
        /// Renderizar BoundingBox
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

            //Actualizar vertices de BoundingBox solo si hubo una modificación
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
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
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

        /// <summary>
        /// Crea un array con los 8 vertices del BoundingBox, en base a los extremos especificados
        /// </summary>
        private Vector3[] computeCorners(Vector3 min, Vector3 max)
        {
            Vector3[] corners = new Vector3[8];

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
        /// Crea un array con los 8 vertices del BoundingBox
        /// </summary>
        public Vector3[] computeCorners()
        {
            return computeCorners(pMin, pMax);
        }

        /// <summary>
        /// Calcula los polígonos que conforman las 6 caras del BoundingBox 
        /// </summary>
        /// <returns>Array con las 6 caras del polígono en el siguiente orden: Up, Down, Front, Back, Right, Left</returns>
        public Face[] computeFaces()
        {
            Face[] faces = new Face[6];
            Face face;

            //Up
            face = new Face();
            face.Plane = new Plane(0, 1, 0, -pMax.Y);
            face.Extremes = new Vector3[] { 
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[0] = face;

            //Down
            face = new Face();
            face.Plane = new Plane(0, -1, 0, pMin.Y);
            face.Extremes = new Vector3[] { 
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z)
            };
            faces[1] = face;

            //Front
            face = new Face();
            face.Plane = new Plane(0, 0, 1, -pMax.Z);
            face.Extremes = new Vector3[] { 
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[2] = face;

            //Back
            face = new Face();
            face.Plane = new Plane(0, 0, -1, pMin.Z);
            face.Extremes = new Vector3[] { 
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z)
            };
            faces[3] = face;

            //Right
            face = new Face();
            face.Plane = new Plane(1, 0, 0, -pMax.X);
            face.Extremes = new Vector3[] { 
                new Vector3(pMax.X, pMin.Y, pMin.Z),
                new Vector3(pMax.X, pMin.Y, pMax.Z),
                new Vector3(pMax.X, pMax.Y, pMin.Z),
                new Vector3(pMax.X, pMax.Y, pMax.Z)
            };
            faces[4] = face;

            //Left
            face = new Face();
            face.Plane = new Plane(-1, 0, 0, pMin.X);
            face.Extremes = new Vector3[] { 
                new Vector3(pMin.X, pMin.Y, pMin.Z),
                new Vector3(pMin.X, pMin.Y, pMax.Z),
                new Vector3(pMin.X, pMax.Y, pMin.Z),
                new Vector3(pMin.X, pMax.Y, pMax.Z)
            };
            faces[5] = face;

            return faces;
        }

        /// <summary>
        /// Cara de un BoundingBox representada por un polígono rectangular de 4 vértices.
        /// </summary>
        public class Face
        {
            public Face()
            {
                extremes = new Vector3[4];
            }

            Vector3[] extremes;
            /// <summary>
            /// Los 4 vértices extremos de la cara
            /// </summary>
            public Vector3[] Extremes
            {
                get { return extremes; }
                set { extremes = value; }
            }

            Plane plane;
            /// <summary>
            /// Ecuación del plano que engloba la cara, con su normal apuntado hacia afuera normalizada.
            /// </summary>
            public Plane Plane
            {
                get { return plane; }
                set { plane = value; }
            }
        }

        /// <summary>
        /// Crear un BoundingBox igual a este
        /// </summary>
        /// <returns>BoundingBox clonado</returns>
        public TgcBoundingBox clone()
        {
            TgcBoundingBox cloneBbox = new TgcBoundingBox();
            cloneBbox.pMin = this.pMin;
            cloneBbox.pMax = this.pMax;
            cloneBbox.pMinOriginal = this.pMinOriginal;
            cloneBbox.pMaxOriginal = this.pMaxOriginal;

            return cloneBbox;
        }

        /// <summary>
        /// Proyecta el BoundingBox a un rectangulo 2D de screen space
        /// </summary>
        /// <returns>Rectangulo 2D con proyeccion en screen space</returns>
        public Rectangle projectToScreen()
        {
            Device device = GuiController.Instance.D3dDevice;
            Viewport viewport = device.Viewport;
            Matrix world = device.Transform.World;
            Matrix view = device.Transform.View;
            Matrix proj = device.Transform.Projection;

            //Proyectar los 8 corners del BoundingBox
            Vector3[] projVertices = computeCorners();
            for (int i = 0; i < projVertices.Length; i++)
            {
                projVertices[i] = Vector3.Project(projVertices[i], viewport, proj, view, world);
            }

            //Buscar los puntos extremos
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector3 v in projVertices)
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
        /// Convertir a struct
        /// </summary>
        public AABBStruct toStruct()
        {
            AABBStruct aabbStruct = new AABBStruct();
            aabbStruct.min = this.pMin;
            aabbStruct.max = this.pMax;
            return aabbStruct;
        }

        /// <summary>
        /// BoundingBox en un struct liviano
        /// </summary>
        public struct AABBStruct
        {
            public Vector3 min;
            public Vector3 max;

            /// <summary>
            /// Convertir a clase
            /// </summary>
            public TgcBoundingBox toClass()
            {
                return new TgcBoundingBox(min, max);
            }
        }


    }

    
}
