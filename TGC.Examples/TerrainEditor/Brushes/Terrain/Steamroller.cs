using Microsoft.DirectX;
using System.Drawing;

namespace TGC.Examples.TerrainEditor.Brushes.Terrain
{
    /// <summary>
    ///     Aplana el terreno tomando como altura la del primer vertice seleccionado.
    /// </summary>
    public class Steamroller : TerrainBrush
    {
        private float level;

        public Steamroller()
        {
            Color1 = Color.LightGoldenrodYellow;
            Color2 = Color.Purple;
            text.Color = Color1;
            bBrush.Color = Color1;
            bBrush.updateValues();
        }

        protected override float getSpeedAdjustment()
        {
            return Intensity / 42.5f;
        }

        /// <summary>
        ///     Retorno lo que habria que sumarle al vertice para que tenga la altura deseada.
        /// </summary>
        /// <param name="heightmapData"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        protected override float intensityFor(float[,] heightmapData, int i, int j)
        {
            var intensity = level - heightmapData[i, j];

            return intensity;
        }

        /// <summary>
        ///     Me guardo la altura del primer vertice.
        /// </summary>
        /// <param name="terrain"></param>
        public override void beginEdition(SmartTerrain terrain)
        {
            base.beginEdition(terrain);
            Vector2 coords;
            if (terrain.xzToHeightmapCoords(Position.X, Position.Z, out coords))
                terrain.interpoledIntensity(coords.X, coords.Y, out level);
        }

        protected override void renderText()
        {
            var label = Invert ? "Sharpen" : "Flatten";
            if (text.Text == null || !text.Text.Equals(label)) text.Text = label;
            text.render();
        }
    }
}