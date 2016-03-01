using System.Drawing;

namespace TGC.Examples.TerrainEditor.Brushes.Terrain
{
    /// <summary>
    ///     Eleva o hunde el terreno
    /// </summary>
    public class Shovel : TerrainBrush
    {
        public Shovel()
        {
            Color1 = Color.Aqua;
            Color2 = Color.Red;
            text.Color = Color1;
            bBrush.Color = Color1;
            bBrush.updateValues();
        }

        protected override float intensityFor(float[,] heightmapData, int i, int j)
        {
            return Intensity;
        }

        protected override void renderText()
        {
            var label = Invert ? "Dig" : "Raise";
            if (text.Text == null || !text.Text.Equals(label)) text.Text = label;
            text.render();
        }
    }
}