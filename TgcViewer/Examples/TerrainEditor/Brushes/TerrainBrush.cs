using Microsoft.DirectX;
using Examples.TerrainEditor;
using TgcViewer.Utils.TgcGeometry;
using Microsoft.DirectX.Direct3D;
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
        /// Posicion XZ en el terreno.
        /// </summary>
        public Vector2 Position { get; set; }

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
        
        
       
        public abstract void configureTerrainEffect(EditableTerrain effect);

        public bool editTerrain(EditableTerrain terrain, bool up)
        {
            float radius = Radius / terrain.ScaleXZ;
            float speed = GuiController.Instance.ElapsedTime;
            if (!up) speed *= -1;
            float radius2 = FastMath.Pow2(radius);
            float innerRadius2 = radius2 * Hardness / 100;

            Vector2 coords;
            float[,] heightmapData = terrain.HeightmapData;
            bool changed = false;


            if (!terrain.xzToHeightmapCoords(Position.X, Position.Y, out coords)) return false;

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
                    float d2 = FastMath.Pow2(i - coords.X) + FastMath.Pow2(j - coords.Y);
                    if (d2 <= radius2)
                    {

                        float newHeight = editVertice(speed, radius2, innerRadius2, heightmapData[i, j], d2);
                        if (heightmapData[i, j] != newHeight)
                        {

                            heightmapData[i, j] = newHeight;
                            changed = true;
                        }
                    }
                }
            }
           
            if (changed) terrain.setHeightmapData(heightmapData);
              
            
            return changed;
        }

        protected abstract float editVertice(float speed, float radius2, float innerRadius2, float p, float d2);
       

        
    }
}
