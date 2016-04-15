using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core._2D;
using TGC.Core.Geometries;
using TGC.Core.Utils;
using TGC.Util;
using TGC.Util.Input;
using TGC.Util.Sound;

namespace TGC.Examples.TerrainEditor.Brushes.Terrain
{
    public abstract class TerrainBrush : ITerrainEditorBrush
    {
        private static TgcStaticSound sound;
        protected TgcBox bBrush;

        protected SmartTerrain terrain;
        protected TgcText2d text;

        public TerrainBrush()
        {
            SoundEnabled = true;
            text = new TgcText2d();
            text.Align = TgcText2d.TextAlign.RIGHT;
            text.changeFont(new Font("Arial", 12, FontStyle.Bold));

            bBrush = TgcBox.fromSize(new Vector3(10, 100, 10));
            sound = new TgcStaticSound();
            sound.loadSound(GuiController.Instance.ExamplesMediaDir + "Sound\\tierra.wav");
        }

        /// <summary>
        ///     Setea y configura la technique que muestra el pincel sobre el terreno.
        /// </summary>
        /// <param name="terrain"></param>
        public virtual void configureTerrainEffect(SmartTerrain terrain)
        {
            if (Rounded) terrain.Technique = "PositionColoredTexturedWithRoundBrush";
            else terrain.Technique = "PositionColoredTexturedWithSquareBrush";
            terrain.Effect.SetValue("brushPosition", new[] { Position.X, Position.Z });
            terrain.Effect.SetValue("brushRadius", Radius);
            terrain.Effect.SetValue("brushHardness", Hardness);
            terrain.Effect.SetValue("brushColor1", Color1.ToArgb());
            terrain.Effect.SetValue("brushColor2", Color2.ToArgb());
        }

        public virtual void beginEdition(SmartTerrain terrain)
        {
            this.terrain = terrain;
            Editing = true;
        }

        public virtual bool editTerrain()
        {
            var speed = GuiController.Instance.ElapsedTime * getSpeedAdjustment();
            if (Invert) speed *= -1;

            var radius = Radius / terrain.ScaleXZ;
            var innerRadius = radius * (Hardness / 100);
            var radius2 = FastMath.Pow2(radius);
            var innerRadius2 = FastMath.Pow2(innerRadius);

            Vector2 coords;
            var heightmapData = terrain.HeightmapData;
            var changed = false;

            if (!terrain.xzToHeightmapCoords(Position.X, Position.Z, out coords)) return false;

            //Calculo un cuadrado alrededor del vertice seleccionado
            int[] min = { (int)FastMath.Ceiling(coords.X - radius), (int)FastMath.Ceiling(coords.Y - radius) };

            float[] max = { coords.X + radius, coords.Y + radius };

            for (var i = 0; i < 2; i++)
            {
                if (min[i] < 0) min[i] = 0;
                if (max[i] > heightmapData.GetLength(i)) max[i] = heightmapData.GetLength(i);
            }

            for (var i = min[0]; i < max[0]; i++)
            {
                for (var j = min[1]; j < max[1]; j++)
                {
                    var dx = i - coords.X;
                    var dz = j - coords.Y;

                    var d2 = FastMath.Pow2(dx) + FastMath.Pow2(dz);

                    //Si es cuadrado o el vertice esta dentro del circulo
                    if (!Rounded || d2 <= radius2)
                    {
                        var intensity = intensityFor(heightmapData, i, j);

                        if (intensity != 0)
                        {
                            if (Hardness != 100)
                            {
                                //Si esta entre el circulo/cuadrado interno, disminuyo la intensidad.
                                if (Rounded)
                                {
                                    var outterIntensity = intensity * (1 - d2 / radius2);

                                    if (d2 > innerRadius2)
                                    {
                                        intensity = outterIntensity;
                                    }
                                    else
                                    {
                                        var alpha = 1 - d2 / innerRadius2;
                                        intensity = outterIntensity + alpha * (intensity - outterIntensity);
                                    }
                                }
                                else
                                {
                                    var maxD = FastMath.Max(FastMath.Abs(dx), FastMath.Abs(dz));
                                    if (maxD > innerRadius)
                                    {
                                        intensity = intensity * (1 - (maxD - innerRadius) / (radius - innerRadius));
                                    }
                                }
                            }

                            var newHeight = FastMath.Max(0, FastMath.Min(heightmapData[i, j] + intensity * speed, 255));

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
            Editing = false;
            stopSound();
        }

        protected virtual float getSpeedAdjustment()
        {
            return 1;
        }

        protected abstract float intensityFor(float[,] heightmapData, int i, int j);

        #region Properties

        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public bool SoundEnabled { get; set; }

        private void reproduceSound()
        {
            if (SoundEnabled)
            {
                sound.play();
            }
        }

        private float radius;

        /// <summary>
        ///     Radio del pincel.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = FastMath.Max(value, 0); }
        }

        /// <summary>
        ///     Posicion del pincel
        /// </summary>
        private Vector3 position;

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                bBrush.Position = value + new Vector3(0, 50, 0);
            }
        }

        private float intensity;

        /// <summary>
        ///     Velocidad con la que modifica el terreno. 0 a 255
        /// </summary>
        public float Intensity
        {
            get { return intensity; }
            set { intensity = FastMath.Max(0, FastMath.Min(value, 255)); }
        }

        private float hardness;

        /// <summary>
        ///     Valor de 0 a 100 que determina el tamanio del radio interno del pincel. A medida que los vertices se alejan del
        ///     radio interno, el efecto del pincel es menor.
        /// </summary>
        public float Hardness
        {
            get { return hardness; }
            set { hardness = FastMath.Max(0, FastMath.Min(value, 100)); }
        }

        public bool Rounded { get; set; }

        public bool Invert { get; set; }

        public bool Editing { get; private set; }

        public bool Enabled { get; set; }

        #endregion Properties

        #region TerrainEditorBrush

        public bool mouseMove(TgcTerrainEditor editor)
        {
            Vector3 pos;
            Enabled = editor.mousePositionInTerrain(out pos);
            Position = pos;
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
            var changes = false;
            if (Enabled)
            {
                if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
                {
                    reproduceSound();
                    var invert = GuiController.Instance.D3dInput.keyDown(Key.LeftAlt);

                    var oldInvert = Invert;
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
            renderText();
            editor.doRender();
        }

        protected abstract void renderText();

        public void dispose()
        {
            bBrush.dispose();
            sound.dispose();
            text.dispose();
        }

        #endregion TerrainEditorBrush
    }
}