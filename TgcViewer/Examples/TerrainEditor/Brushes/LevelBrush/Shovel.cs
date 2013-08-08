using System.Drawing;

namespace Examples.TerrainEditor.Brushes.Level
{
    /// <summary>
    /// Eleva o hunde el terreno
    /// </summary>
    public class Shovel:TerrainBrush
    {


        protected override float intensityFor(float[,] heightmapData, int i, int j)
        {
            return Intensity;
        }

        public override void configureTerrainEffect(EditableTerrain terrain)
        {
            if(Rounded) terrain.Technique = "PositionColoredTexturedWithRoundBrush";
            else terrain.Technique = "PositionColoredTexturedWithSquareBrush";
            terrain.Effect.SetValue("brushPosition", new float[] { Position.X, Position.Z });
            terrain.Effect.SetValue("brushRadius", Radius);
            terrain.Effect.SetValue("brushHardness", Hardness);
            terrain.Effect.SetValue("brushColor1", Color.Aqua.ToArgb());
            terrain.Effect.SetValue("brushColor2", Color.LightSalmon.ToArgb());
          
        }
    }
}
