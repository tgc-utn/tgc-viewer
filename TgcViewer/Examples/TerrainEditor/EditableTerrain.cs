using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.Shaders;
using System.Drawing;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;

namespace Examples.TerrainEditor
{
    public class EditableTerrain
    {
        #region Private fields
       
        private float maxIntensity;
        private float minIntensity;
        private float halfwidth;
        private float halflength;
        private Vector3 traslation;
        private VertexBuffer vbTerrain;
        private CustomVertex.PositionColoredTextured[] vertices;
        private Texture terrainTexture;       
        
       
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

            halfwidth = length / 2;
            halflength = width / 2;

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
            for (int i = 0; i < length; i++) for (int j = 0; j < width; j++) heightmapData[i,j] = level;

            //Crear vertexBuffer
            totalVertices = 2 * 3 * (width-1) * (length-1);
            vbTerrain = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), totalVertices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColoredTextured.Format, Pool.Default);

            
            traslation.X = center.X - (length / 2);
            traslation.Y = center.Y - level; //Lo bajo para que el nivel más bajo quede a esa altura.
            this.center.Y = traslation.Y;
            traslation.Z = center.Z - (width / 2);

            halfwidth = length / 2;
            halflength = width / 2;

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

        public void setHeightmapData(float[,] heightmapData)
        {
            if (heightmapData.GetLength(0) == this.heightmapData.GetLength(0) && this.heightmapData.GetLength(1) == heightmapData.GetLength(1))
            {
                this.heightmapData = heightmapData;
            }


        }
        public void updateVertices()
        {

            for(int i=0; i< vertices.Length; i++)
            {
                CustomVertex.PositionColoredTextured v = vertices[i];
                vertices[i].Y = heightmapData[(int)vertices[i].X, (int)vertices[i].Z];
            }

            vbTerrain.SetData(vertices, 0, LockFlags.None);
        }
    
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
        /// Transforma coordenadas del mundo en coordenadas de HeightmapData
        /// </summary>
        public bool xzToHeightmapCoords(float x, float z, out Vector2 coords)
        {
            int i, j;

            i = (int)(x / ScaleXZ - traslation.X);
            j = (int)(z /ScaleXZ - traslation.Z);


            coords = new Vector2(i, j);

            if (coords.X >= HeightmapData.GetLength(0) || coords.Y >= HeightmapData.GetLength(1) || coords.Y < 0 || coords.X < 0) return false;

            return true;
        }

        /// <summary>
        /// Obtiene la altura de un punto, si el punto pertenece al heightmap.
        /// </summary>
        public bool getY(float x, float z, out float y)
        {
            y = 0;
            Vector2 coords;
            if (!this.xzToHeightmapCoords(x, z, out coords)) return false;


            y = (HeightmapData[(int)coords.X, (int)coords.Y] + traslation.Y)* ScaleY;

            return true;
        }

     
        /// <summary>
        /// Retorna true si hubo interseccion con el terreno y setea el collisionPoint.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="collisionPoint"></param>
        /// <returns></returns>
        public bool intersectRay(TgcRay ray, out Vector3 collisionPoint)
        {
            collisionPoint = Vector3.Empty;
            
            Vector3 a = ray.Origin;
            Vector3 r = ray.Direction;

            float maxHeight = (maxIntensity + traslation.Y) * ScaleY;
            float minHeight = (minIntensity + traslation.Y) * ScaleY;
          
            if (a.Y < minHeight) return false;
            float minT = 0;
            bool inside = true;
            float[] aabbMin = new float[] { (Center.X - halfwidth) * ScaleXZ, minHeight, (Center.Z - halflength) * ScaleXZ };
            float[] aabbMax = new float[] { (Center.X + halfwidth) * ScaleXZ, maxHeight, (Center.Z + halflength) * ScaleXZ };
            float[] rayOrigin = new float[]{a.X, a.Y, a.Z};
            float[] rayDir = new float[] { r.X, r.Y, r.Z };

            float[] max_t = new float[3] { -1.0f, -1.0f, -1.0f };
            float[] coord = new float[3];
            
            //Primero calculo la t del ray que hace que intersecte con el AABB del terreno.
            for (uint i = 0; i < 3; ++i)
            {
                if (rayOrigin[i] < aabbMin[i])
                {
                    inside = false;
                    coord[i] = aabbMin[i];

                    if (rayDir[i] != 0.0f)
                    {
                        max_t[i] = (aabbMin[i] - rayOrigin[i]) / rayDir[i];
                    }
                }
                else if (rayOrigin[i] > aabbMax[i])
                {
                    inside = false;
                    coord[i] = aabbMax[i];

                    if (rayDir[i] != 0.0f)
                    {
                        max_t[i] = (aabbMax[i] - rayOrigin[i]) / rayDir[i];
                    }
                }
            }
            bool intersects = false;
            
            //Si no estoy adentro, uso la t mas alta, sino uso t=0
            if (!inside)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (max_t[i] > 0)
                    {
                        intersects = true;
                        if (max_t[i] > minT) minT = max_t[i] - 1;
                        
                    }
                }
              
            }
            if (!inside && !intersects) return false;


            //Me desplazo por el rayo hasta que su altura sea menor a la del terreno en ese punto
            //o me salga del AABB.
            float step = 1;
            for (float t = minT; ; t+=step)
            {
                collisionPoint = a + t * r;
               
                float y;
                bool valid = getY(collisionPoint.X, collisionPoint.Z, out y);
                
                if (valid)
                {
                    
                    
                    if (collisionPoint.Y <= y + 0.5f)
                    {
                        if (collisionPoint.Y < y - 0.5f && t!=minT)
                        {
                            collisionPoint = a + (t - 0.5f) * r;
                            getY(collisionPoint.X, collisionPoint.Z, out y);
                            
                        }
                        return true;

                    }
                }
                else if (collisionPoint.Y < minHeight || t==0)
                {

                    collisionPoint = a + (t-0.5f)* r;
                    

                    return getY(collisionPoint.X, collisionPoint.Z, out collisionPoint.Y);

                }
                else return false;
               
               

            }


            
                     
            
        }
        #endregion







        
    }
 }

