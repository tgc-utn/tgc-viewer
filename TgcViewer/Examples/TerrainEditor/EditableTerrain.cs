using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.Shaders;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;
using System;

namespace Examples.TerrainEditor
{
    public class EditableTerrain
    {
        #region Private fields
       
        private float maxIntensity;
        private float minIntensity;
        private Vector3 traslation;
        private VertexBuffer vbTerrain;
        private CustomVertex.PositionColoredTextured[] vertices;
        private Texture terrainTexture;
        private TgcBoundingBox aabb;
        #endregion


        #region Properties

        private int totalVertices;
        /// <summary>
        /// Cantidad de vertices del terrain
        /// </summary>
        public int TotalVertices { get { return totalVertices; } }

        private float[,] heightmapData;
        /// <summary>
        /// Valor de Y para cada par (X,Z) del Heightmap. 
        /// </summary>
        public float[,] HeightmapData
        {
            get { return heightmapData; }               
            
        }
      
        private bool enabled;
        /// <summary>
        /// Indica si la malla esta habilitada para ser renderizada
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private Vector3 center;
        /// <summary>
        /// Centro del terreno
        /// </summary>
        public Vector3 Center
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

     
        public float ScaleXZ { get; set; }

        public float ScaleY { get; set; }

        #endregion



        public EditableTerrain()
        {
            enabled = true;
            alphaBlendEnable = false;

            //Shader
            this.effect = GuiController.Instance.Shaders.VariosShader;
            this.technique = TgcShaders.T_POSITION_COLORED_TEXTURED;

            aabb = new TgcBoundingBox();
            
        }


        #region Load heightmap

        /// <summary>
        /// Carga los valores del Heightmap en una matriz
        /// </summary>
        protected float[,] loadHeightMap(Device d3dDevice, string path)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
            int width = bitmap.Size.Width;
            int length = bitmap.Size.Height;

            float[,] heightmap = new float[length, width];

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Color pixel = bitmap.GetPixel(j, i);
                    float intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    heightmap[i, j] = intensity;
                }

            }
            bitmap.Dispose();
            return heightmap;
        }

        /// <summary>
        /// Crea un heightmap plano con las dimensiones y altura(level) indicadas
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="level">Altura de 0 a 255</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void loadPlainHeightmap(int width, int length, int level, float scaleXZ, float scaleY, Vector3 center)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.center = center;
            this.ScaleXZ = scaleXZ;
            this.ScaleY = scaleY;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //crear heightmap con ese level
            heightmapData = new float[length, width];
            if (level < 0) level = 0; else if (level > 255) level = 255;
            for (int i = 0; i < length; i++) for (int j = 0; j < width; j++) heightmapData[i, j] = level;

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (length - 1) * (width - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColoredTextured.Format, Pool.Default);

            traslation.X = center.X - (length / 2);
            traslation.Y = center.Y - level;
            this.center.Y = traslation.Y;
            traslation.Z = center.Z - (width / 2);

            //Cargar vertices
            loadVertices();
        }

        /// <summary>
        /// Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmapPath">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void loadHeightmap(string heightmapPath, float scaleXZ, float scaleY, Vector3 center)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.center = center;
            this.ScaleXZ = scaleXZ;
            this.ScaleY = scaleY;

            //Dispose de VertexBuffer anterior, si habia
            if (vbTerrain != null && !vbTerrain.Disposed)
            {
                vbTerrain.Dispose();
            }

            //cargar heightmap
            heightmapData = loadHeightMap(d3dDevice, heightmapPath);
          
            //Crear vertexBuffer
            totalVertices = 2 * 3 * (heightmapData.GetLength(0) - 1) * (heightmapData.GetLength(1) - 1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColoredTextured.Format, Pool.Default);
            
            float width = (float)heightmapData.GetLength(0);
            float length = (float)heightmapData.GetLength(1);
            
            traslation.X = center.X - (width / 2);
            traslation.Y = center.Y;
            traslation.Z = center.Z - (length / 2);
            
            //Cargar vertices
            loadVertices();
            
        }

        #endregion

        #region Load & update vertices

        /// <summary>
        /// Cambia el heightmapData por uno de igual ancho y largo.
        /// </summary>
        /// <param name="heightmapData"></param>
        public void setHeightmapData(float[,] heightmapData)
        {
            if (heightmapData.GetLength(0) == this.heightmapData.GetLength(0) && this.heightmapData.GetLength(1) == heightmapData.GetLength(1))
            {
                this.heightmapData = heightmapData;
            }


        }

        /// <summary>
        /// Actualiza los vertices segun los valores de HeightmapData
        /// </summary>
        public void updateVertices()
        {
            minIntensity = -1;
            maxIntensity = 0;
            for(int i=0; i< vertices.Length; i++)
            {
                CustomVertex.PositionColoredTextured v = vertices[i];
                float intensity = heightmapData[(int)vertices[i].X, (int)vertices[i].Z];
                vertices[i].Y = intensity;
                if (intensity > maxIntensity) maxIntensity = intensity;
                if (minIntensity == -1 || intensity < minIntensity) minIntensity = intensity;
                   
                
            }

            vbTerrain.SetData(vertices, 0, LockFlags.None);
            aabb.setExtremes(new Vector3(0, minIntensity, 0), new Vector3(HeightmapData.GetLength(0), maxIntensity, HeightmapData.GetLength(1)));
          
      
        }

        /// <summary>
        /// Crea los vertices
        /// </summary>
        private void loadVertices()
        {
            int dataIdx = 0;

            float width = (float)heightmapData.GetLength(0);
            float length = (float)heightmapData.GetLength(1);
            int color = Color.White.ToArgb();
            vertices = new CustomVertex.PositionColoredTextured[totalVertices];
            
            maxIntensity = 0;
            minIntensity = -1;

            for (int i = 0; i < width - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {

                    if (heightmapData[i, j] > maxIntensity) maxIntensity = heightmapData[i, j];
                    if (minIntensity == -1 || heightmapData[i, j] < minIntensity) minIntensity = heightmapData[i, j];
                    
                    
                    //Vertices
                    Vector3 v1 = new Vector3(i, heightmapData[i, j],  j);
                    Vector3 v2 = new Vector3(i, heightmapData[i, j + 1] , j + 1);
                    Vector3 v3 = new Vector3(i + 1, heightmapData[i + 1, j], j );
                    Vector3 v4 = new Vector3(i + 1, heightmapData[i + 1, j + 1], j + 1);

                    //Coordendas de textura
                    Vector2 t1 = new Vector2(i / width, j / length);
                    Vector2 t2 = new Vector2(i / width, (j + 1) / length);
                    Vector2 t3 = new Vector2((i + 1) / width, j / length);
                    Vector2 t4 = new Vector2((i + 1) / width, (j + 1) / length);

                    //Cargar triangulo 1
                    vertices[dataIdx] = new CustomVertex.PositionColoredTextured(v1, color, t1.X, t1.Y);
                    vertices[dataIdx + 1] = new CustomVertex.PositionColoredTextured(v2, color, t2.X, t2.Y);
                    vertices[dataIdx + 2] = new CustomVertex.PositionColoredTextured(v4, color, t4.X, t4.Y);

                    //Cargar triangulo 2
                    vertices[dataIdx + 3] = new CustomVertex.PositionColoredTextured(v1,color, t1.X, t1.Y);
                    vertices[dataIdx + 4] = new CustomVertex.PositionColoredTextured(v4, color, t4.X, t4.Y);
                    vertices[dataIdx + 5] = new CustomVertex.PositionColoredTextured(v3, color, t3.X, t3.Y);

                    dataIdx += 6;
                }
                vbTerrain.SetData(vertices, 0, LockFlags.None);
           
                aabb.setExtremes(new Vector3(0, minIntensity, 0), new Vector3(HeightmapData.GetLength(0), maxIntensity, HeightmapData.GetLength(1)));
          
            }
        }
        #endregion


        /// <summary>
        /// Carga la textura del terreno
        /// </summary>
        public void loadTexture(string path)
        {
            //Dispose textura anterior, si habia
            if (terrainTexture != null && !terrainTexture.Disposed)
            {
                terrainTexture.Dispose();
            }

            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Rotar e invertir textura
            Bitmap b = (Bitmap)Bitmap.FromFile(path);
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            terrainTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
        }




        #region Render & Dispose
        /// <summary>
        /// Renderiza el terreno
        /// </summary>
        public void render()
        {
            if (!enabled)
                return;

            Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;
            Matrix transform =Matrix.Translation(traslation)*Matrix.Scaling(ScaleXZ, ScaleY, ScaleXZ);
          
            //Textura
            effect.SetValue("texDiffuseMap", terrainTexture);
            effect.SetValue("matTransform", transform);
            

            texturesManager.clear(1);

            GuiController.Instance.Shaders.setShaderMatrix(this.effect, Matrix.Identity);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColoredTextured;
            effect.Technique = this.technique;
            d3dDevice.SetStreamSource(0, vbTerrain, 0);

            //Render con shader
            int p = effect.Begin(0);
            for (int i = 0; i < p; i++)
            {
                effect.BeginPass(i);
                d3dDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, totalVertices / 3);
                effect.EndPass();
            }
            effect.End();

        }

     

        /// <summary>
        /// Libera los recursos del Terreno
        /// </summary>
        public void dispose()
        {
            if (vbTerrain != null)
            {
                vbTerrain.Dispose();
            }

            if (terrainTexture != null)
            {
                terrainTexture.Dispose();
            }
        }

        #endregion

        #region Utils
     

   
        /// <summary>
        /// Retorna true si hubo interseccion con el terreno y setea el collisionPoint.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="collisionPoint"></param>
        /// <returns></returns>
        public bool intersectRay(TgcRay ray, out Vector3 collisionPoint)
        {
            collisionPoint = Vector3.Empty;
            Matrix scaleInv = Matrix.Scaling(new Vector3(1/ScaleXZ, 1/ScaleY, 1/ScaleXZ));
            Vector3 a = Vector3.TransformCoordinate(ray.Origin, scaleInv) - traslation; 
            Vector3 r = Vector3.TransformCoordinate(ray.Direction, scaleInv);
                       

            if (a.Y < minIntensity) 
                return false;
            
            Vector3 q;
            //Me fijo si intersecta con el BB del terreno.
            if (!TgcCollisionUtils.intersectRayAABB(new TgcRay(a, r).toStruct(), aabb.toStruct(), out q)) 
                return false;
            
            float minT=0;
            //Obtengo el T de la interseccion.
            if (q != a)
            {
                if (r.X != 0) minT = (q.X - a.X) / r.X;
                else if (r.Y != 0) minT = (q.Y - a.Y) / r.Y;
                else if (r.Z != 0) minT = (q.Z - a.Z) / r.Z;
            }
            

            //Me desplazo por el rayo hasta que su altura sea menor a la del terreno en ese punto
            //o me salga del AABB.
            float t=0;
            float step = 1;
          
            for (t = minT; ; t += step)
            {
                collisionPoint = a + t * r;
                float y;
                
                if(!interpoledIntensity(collisionPoint.X, collisionPoint.Z, out y))  
                    return false;

                                 
                if (collisionPoint.Y <= y + float.Epsilon)
                {
                    collisionPoint.Y = y;
                    collisionPoint = Vector3.TransformCoordinate(collisionPoint + traslation, Matrix.Scaling(ScaleXZ, ScaleY, ScaleXZ));
                    return true;
                }

            }    



        }




        /// <summary>
        /// Retorna true si hubo interseccion con el plano del terreno y setea el collisionPoint con la altura en ese punto. 
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="collisionPoint"></param>
        /// <returns></returns>
        public bool intersectRayPlane(TgcRay ray, out Vector3 collisionPoint)
        {

            collisionPoint = Vector3.Empty;
           float minHeight = (minIntensity + traslation.Y) * ScaleY;


            float t;
            //Me fijo si intersecta con el BB del terreno.
            if (!TgcCollisionUtils.intersectRayPlane(ray, new Plane(0, 1, 0, -minHeight), out t, out collisionPoint)) return false;

            return interpoledHeight(collisionPoint.X, collisionPoint.Z, out collisionPoint.Y);

        }
       
        /// <summary>
        /// Transforma coordenadas del mundo en coordenadas del heightmap.
        /// </summary>
        public bool xzToHeightmapCoords(float x, float z, out Vector2 coords)
        {
            float i, j;

            i = x / ScaleXZ - traslation.X;
            j = z / ScaleXZ - traslation.Z;


            coords = new Vector2(i, j);

            if (coords.X >= HeightmapData.GetLength(0) || coords.Y >= HeightmapData.GetLength(1) || coords.Y < 0 || coords.X < 0) return false;

            return true;
        }

        /// <summary>
        /// Retorna la altura del terreno en ese punto utilizando interpolacion bilineal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool interpoledHeight(float x, float z, out float y)
        {
            Vector2 coords;
            float i;
            y = 0;
           
            if(!xzToHeightmapCoords(x, z, out coords)) return false;
            interpoledIntensity(coords.X, coords.Y, out i);

            y = (i + traslation.Y) * ScaleY;
            return true;
        }
       
        /// <summary>
        /// Retorna la intensidad del heightmap en ese punto utilizando interpolacion bilineal.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool interpoledIntensity(float u, float v, out float i)
        {
             i =0;

            float maxX =HeightmapData.GetLength(0);
            float maxZ = HeightmapData.GetLength(1);
            if (u >= maxX || v >= maxZ || v < 0 ||u < 0)return false;
                
            int x1, x2, z1, z2;
            float s, t;

            x1 = (int)FastMath.Floor(u);
            x2 = x1 + 1;
            s = u - x1;
            
            z1 = (int)FastMath.Floor(v);
            z2 = z1 + 1;
            t = v - z1;
             
            if(z2>=maxZ) z2--;
            if(x2>=maxX) x2--;
             
            float i1 = HeightmapData[x1,z1] + s*(HeightmapData[x2,z1]-HeightmapData[x1,z1]);
            float i2 = HeightmapData[x1,z2] + s*(HeightmapData[x2,z2]-HeightmapData[x1,z2]);

            i = i1 + t*(i2-i1);
            return true;


        }


        #endregion
    }
 }

