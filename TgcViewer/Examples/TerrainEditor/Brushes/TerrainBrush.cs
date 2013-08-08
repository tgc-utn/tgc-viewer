using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer;

namespace Examples.TerrainEditor.Brushes
{
    public abstract class TerrainBrush
    {
        private float radius;
        /// <summary>
        /// Radio del pincel.
        /// </summary>
        public float Radius { get { return radius; } set { radius = FastMath.Max(value, 0); } }
        
        /// <summary>
        /// Posicion del pincel
        /// </summary>
        public Vector3 Position { get; set; }

        private float intensity;
        /// <summary>
        /// Velocidad con la que modifica el terreno. 0 a 255
        /// </summary>
        public float Intensity { get { return intensity; } set { intensity = FastMath.Max(0, FastMath.Min(value, 255)); } }
        
        private float hardness;
        /// <summary>
        /// Valor de 0 a 100 que determina el tamanio del radio interno del pincel. A medida que los vertices se alejan del 
        /// radio interno, el efecto del pincel es menor.
        /// </summary>
        public float Hardness { get { return hardness; } set { hardness = FastMath.Max(0, FastMath.Min(value, 100)); } }

        public bool Rounded { get; set; }

        public bool Invert { get; set; }

        private bool editing;
        public bool Editing { get { return editing; } }
     
        public abstract void configureTerrainEffect(EditableTerrain effect);

       

        public virtual void beginEdition(EditableTerrain terrain) { this.terrain = terrain; editing = true; }

        public virtual void endEdition() { editing = false; }
         
        protected EditableTerrain terrain;



        public virtual bool editTerrain()
        {
            float speed = GuiController.Instance.ElapsedTime*getSpeedAdjustment();
            if (Invert) speed *= -1;

            float radius = Radius / terrain.ScaleXZ;
            float innerRadius = radius * Hardness / 100;
            float radius2 = FastMath.Pow2(radius);
            float innerRadius2 = innerRadius * innerRadius;

            Vector2 coords;
            float[,] heightmapData = terrain.HeightmapData;
            bool changed = false;



            if (!terrain.xzToHeightmapCoords(Position.X, Position.Z, out coords)) return false;

            //Calculo un cuadrado alrededor del vertice seleccionado
            int[] min = new int[] { (int)(coords.X - radius), (int)(coords.Y - radius) };

            int[] max = new int[] { (int)(coords.X + radius), (int)(coords.Y + radius) };

            for (int i = 0; i < 2; i++)
            {
                if (min[i] < 0) min[i] = 0;
                if (max[i] > heightmapData.GetLength(i)) max[i] = heightmapData.GetLength(i);
            }


            for (int i = min[0]; i < max[0]; i++)
            {

                for (int j = min[1]; j < max[1]; j++)
                {
                    float dx = i - coords.X;
                    float dz = j - coords.Y;


                    float d2 = FastMath.Pow2(dx) + FastMath.Pow2(dz);

                    //Si es cuadrado o el vertice esta dentro del circulo
                    if (!Rounded || d2 <= radius2)
                    {

                        float intensity = intensityFor(heightmapData, i, j);
                    
                        if (intensity != 0)
                        {

                            if (innerRadius != radius)
                            {
                                //Si esta entre el circulo/cuadrado interno, disminuyo la intensidad.
                                if (Rounded && d2 > innerRadius2)
                                {
                                    intensity = intensity * (1 - ((d2 - innerRadius2) / (radius2 - innerRadius2)));

                                }
                                else if (!Rounded && (FastMath.Abs(dx) > innerRadius || FastMath.Abs(dz) > innerRadius))
                                {
                                    intensity = intensity * (1 - ((FastMath.Max(FastMath.Abs(dx), FastMath.Abs(dz)) - innerRadius) / (radius - innerRadius)));

                                }
                            }



                            float newHeight = FastMath.Max(0, FastMath.Min(heightmapData[i, j] + intensity * speed, 255));

                            if (heightmapData[i, j] != newHeight)
                            {
                                heightmapData[i, j] = newHeight;
                                changed = true;
                            }

                        }
                    }
                }
            }

            if (changed) terrain.setHeightmapData(heightmapData);
            return changed;
        }

        protected virtual float getSpeedAdjustment()
        {
            return 1;
        }

        protected abstract float intensityFor(float[,] heightmapData, int i, int j);
        
    }
}
