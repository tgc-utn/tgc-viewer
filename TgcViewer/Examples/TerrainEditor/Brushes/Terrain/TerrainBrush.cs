using System.Drawing;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;

namespace Examples.TerrainEditor.Brushes.Terrain
{
    public abstract class TerrainBrush:ITerrainEditorBrush
    {
        protected TgcBox bBrush;
        private static TgcStaticSound sound;
        private static int instances = 0;
        #region Properties
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public bool SoundEnabled { get; set; }
        private void reproduceSound(){
            if (SoundEnabled)
            {
                sound.play();

            }
        }
        private float radius;
        /// <summary>
        /// Radio del pincel.
        /// </summary>
        public float Radius { get { return radius; } set { radius = FastMath.Max(value, 0); } }
        
        /// <summary>
        /// Posicion del pincel
        /// </summary>
        private Vector3 position;
        public Vector3 Position { get { return position; } set { this.position = value; bBrush.Position = value + new Vector3(0, 50, 0); } }

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

        public bool Enabled { get; set; }

        #endregion

        public TerrainBrush()
        {
            SoundEnabled = true;
            bBrush = TgcBox.fromSize(new Vector3(10, 100, 10));
            if (sound == null)
            {
                sound = new TgcStaticSound();
                sound.loadSound(GuiController.Instance.ExamplesMediaDir + "Sound\\tierra.wav"); 
            }
            instances++;
        }

        #region TerrainEditorBrush


        public bool mouseMove(TgcTerrainEditor editor)
        {
            Vector3 pos;
            Enabled = editor.mousePositionInTerrain(out pos);
            this.Position = pos;
            return false;
        }



        public bool mouseLeave(TgcTerrainEditor editor)
        {
            Enabled = false;
            if (Editing) endEdition();
            return false;
        }

        public bool update(TgcTerrainEditor editor)
        {
            bool changes = false;
            if (Enabled)
            {
                if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    reproduceSound();
                    bool invert = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftAlt);

                    bool oldInvert = Invert;
                    Invert ^= invert;
                    if (!Editing)
                    {
                        beginEdition(editor.Terrain);
                    }
                    if (editTerrain())
                    {
                        changes = true;
                        terrain.updateVertices();

                    }
                    Invert = oldInvert;
                }
                else if (Editing) endEdition();
           

            }
            if (changes) editor.updateVegetationY();
            return changes;
        }

        private void stopSound()
        {
            sound.stop();
        }

        public void render(TgcTerrainEditor editor)
        {
            if (Enabled)
            {
                configureTerrainEffect(editor.Terrain);
                bBrush.render();
            }
            editor.doRender();
        }


        public void dispose()
        {
            bBrush.dispose();
            instances--;
            if (instances == 0 && sound!=null)
            {
                sound.dispose();
                sound = null;
            }
        }

        #endregion

        /// <summary>
        /// Setea y configura la technique que muestra el pincel sobre el terreno.
        /// </summary>
        /// <param name="terrain"></param>
        public virtual void configureTerrainEffect(SmartTerrain terrain)
        {
            if (Rounded) terrain.Technique = "PositionColoredTexturedWithRoundBrush";
            else terrain.Technique = "PositionColoredTexturedWithSquareBrush";
            terrain.Effect.SetValue("brushPosition", new float[] { Position.X, Position.Z });
            terrain.Effect.SetValue("brushRadius", Radius);
            terrain.Effect.SetValue("brushHardness", Hardness);
            terrain.Effect.SetValue("brushColor1", Color1.ToArgb());
            terrain.Effect.SetValue("brushColor2", Color2.ToArgb());

        }



        protected SmartTerrain terrain;
        public virtual void beginEdition(SmartTerrain terrain) { this.terrain = terrain; editing = true; }

        public virtual bool editTerrain()
        {
            float speed = GuiController.Instance.ElapsedTime * getSpeedAdjustment();
            if (Invert) speed *= -1;

            float radius = Radius / terrain.ScaleXZ;
            float innerRadius = radius * (Hardness / 100);
            float radius2 = FastMath.Pow2(radius);
            float innerRadius2 = FastMath.Pow2(innerRadius);

            Vector2 coords;
            float[,] heightmapData = terrain.HeightmapData;
            bool changed = false;



            if (!terrain.xzToHeightmapCoords(Position.X, Position.Z, out coords)) return false;

            //Calculo un cuadrado alrededor del vertice seleccionado
            int[] min = new int[] {(int)FastMath.Ceiling(coords.X - radius),(int)FastMath.Ceiling(coords.Y - radius) };

            float[] max = new float[] { coords.X + radius, coords.Y + radius };

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
                                else if(!Rounded)
                                {
                                    float maxD = FastMath.Max(FastMath.Abs(dx), FastMath.Abs(dz));
                                    if (maxD>innerRadius)
                                    {
                                        intensity = intensity * (1 - (maxD - innerRadius) / (radius - innerRadius));

                                    }
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

            if (changed)
            {
                terrain.setHeightmapData(heightmapData);

                float y;
                terrain.interpoledHeight(bBrush.Position.X, bBrush.Position.Z, out y);
                bBrush.Position = new Vector3(bBrush.Position.X, y + 50, bBrush.Position.Z);

            }
            return changed;
        }

        public virtual void endEdition()
        {
            editing = false;
            stopSound();
        }
       

        protected virtual float getSpeedAdjustment()
        {
            return 1;
        }

        protected abstract float intensityFor(float[,] heightmapData, int i, int j);

        
    }
}
