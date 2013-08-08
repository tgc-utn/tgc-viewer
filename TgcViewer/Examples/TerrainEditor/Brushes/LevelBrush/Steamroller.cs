using Microsoft.DirectX;
using System.Drawing;

namespace Examples.TerrainEditor.Brushes.LevelBrush
{
    /// <summary>
    /// Aplana el terreno tomando como altura la del primer vertice seleccionado.
    /// </summary>
    public class Steamroller : TerrainBrush
    {
        private float level;

      

        protected override float getSpeedAdjustment()
        {
            return Intensity / 42.5f;
        }

        /// <summary>
        /// Retorno lo que habria que sumarle al vertice para que tenga la altura deseada.
        /// </summary>
        /// <param name="heightmapData"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        protected override float intensityFor(float[,] heightmapData, int i, int j)
        {

            float intensity = level - heightmapData[i, j];

            return intensity;
        }

        public override void configureTerrainEffect(EditableTerrain terrain)
        {
            if (Rounded) terrain.Technique = "PositionColoredTexturedWithRoundBrush";
            else terrain.Technique = "PositionColoredTexturedWithSquareBrush";
            terrain.Effect.SetValue("brushPosition", new float[] { Position.X, Position.Z });
            terrain.Effect.SetValue("brushRadius", Radius);
            terrain.Effect.SetValue("brushHardness", Hardness);
            terrain.Effect.SetValue("brushColor1", Color.LightSalmon.ToArgb());
            terrain.Effect.SetValue("brushColor2", Color.LightGoldenrodYellow.ToArgb());
       
        }
      
        /// <summary>
        /// Me guardo la altura del primer vertice.
        /// </summary>
        /// <param name="terrain"></param>
        public override void beginEdition(EditableTerrain terrain)
        {
            base.beginEdition(terrain);
            Vector2 coords;
            if (terrain.xzToHeightmapCoords(Position.X, Position.Z, out coords))
                level = terrain.HeightmapData[(int)coords.X, (int)coords.Y];          

        }

       
    }
}
