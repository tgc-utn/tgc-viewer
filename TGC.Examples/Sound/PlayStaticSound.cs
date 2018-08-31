using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Sound;
using TGC.Core.Text;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Sound
{
    /// <summary>
    ///     Ejemplo PlayMp3:
    ///     Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Basicos de 3D - GameEngine
    ///     Muestra como reproducir un archivo de sonido estatico en formato WAV.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class PlayStaticSound : TGCExampleViewer
    {
        private TGCFileModifier wavFileModifier;
        private TGCBooleanModifier playLoopModifier;

        private string currentFile;
        private TgcText2D currentSoundText;
        private TgcText2D instruccionesText;
        private TgcSound sound;

        public PlayStaticSound(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Sound";
            Name = "Play StaticSound";
            Description = "Muestra como reproducir un archivo de sonido estatico en formato WAV.";
        }

        public override void Init()
        {
            //Texto para el sonido actual
            currentSoundText = new TgcText2D();
            currentSoundText.Text = "No sound";
            currentSoundText.Position = new Point(50, 20);
            currentSoundText.Color = Color.Gold;
            currentSoundText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Italic));

            //Texto de instrucciones
            instruccionesText = new TgcText2D();
            instruccionesText.Text = "Y = Play, U = Pause, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            wavFileModifier = AddFile("WAV-File", MediaDir + "Sound\\campanadas horas.wav", "WAVs|*.wav");

            //Modifier para loop
            playLoopModifier = AddBoolean("PlayLoop", "Play Loop", false);

            var filePath = wavFileModifier.Value;
            loadSound(filePath);
           
        }

        /// <summary>
        ///     Cargar un nuevo WAV si hubo una variacion
        /// </summary>
        private void loadSound(string filePath)
        {
            if (currentFile == null || currentFile != filePath)
            {
                currentFile = filePath;

                //Borrar sonido anterior
                if (sound != null)
                {
                    sound.Dispose();
                }

                //Cargar sonido
                sound = new TgcSound(currentFile);

                currentSoundText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
            
        }

        public override void Update()
        {
            PreUpdate();

            //Ver si cambio el WAV
            var filePath = wavFileModifier.Value;
            loadSound(filePath);

            sound.LoopCount = ((playLoopModifier.Value) ? TgcSound.LOOP_INFINITE : 0);

            //Contro el input de teclado
            if (Input.keyPressed(Key.Y))
            {
                sound.Play();
                
            }
            else if(Input.keyPressed(Key.U))
            {
                sound.Pause();
            }
            else if (Input.keyPressed(Key.O))
            {
                sound.Stop();
            }
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Render texto
            currentSoundText.render();
            instruccionesText.render();

            PostRender();
        }

        public override void Dispose()
        {
            sound.Dispose();
        }
    }
}