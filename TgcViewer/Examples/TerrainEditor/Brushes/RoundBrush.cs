using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using Examples.TerrainEditor;
using Microsoft.DirectX.Direct3D;
using TgcViewer;

namespace Examples.TerrainEditor.Brushes
{
    /// <summary>
    /// Setea la misma intensidad a todos los vertices afectados.
    /// </summary>
    public class RoundBrush:TerrainBrush
    {
  

      
       
        protected override float editVertice(float speed, float radius2, float innerRadius2, float height, float d2)
        {
            float intensity = Intensity;
            float newHeight;


            if (d2 > innerRadius2)
            {
                float dI = (d2 - innerRadius2) / (radius2 - innerRadius2);
                intensity *= (1 - dI);

            }

            newHeight = FastMath.Max(0, FastMath.Min(height + intensity * speed, 255));

            return newHeight;
        }

        public override void configureTerrainEffect(EditableTerrain terrain)
        {
            terrain.Technique = "PositionColoredTexturedWithBrush";
            terrain.Effect.SetValue("brushPosition", new float[] { Position.X, Position.Y });
            terrain.Effect.SetValue("brushRadius", Radius);
            terrain.Effect.SetValue("brushHardness", Hardness);
        }
    }
}
