using System.Drawing;

namespace Examples.TerrainEditor.Brushes.Level
{
    /// <summary>
    /// Eleva o hunde el terreno
    /// </summary>
    public class Shovel:TerrainBrush
    {
        public Shovel()
        {
            Color1 = Color.Aqua;
            Color2 = Color.LightPink;
            bBrush.Color = Color1;
            bBrush.updateValues();
        }
       
        protected override float intensityFor(float[,] heightmapData, int i, int j)
        {
            return Intensity;
        }
    }
}
