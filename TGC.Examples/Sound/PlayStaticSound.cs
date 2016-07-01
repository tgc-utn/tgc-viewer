using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using TGC.Core._2D;
using TGC.Core.Input;
using TGC.Core.Sound;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

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
        private string currentFile;
        private TgcText2D currentSoundText;
        private TgcText2D instruccionesText;
        private TgcStaticSound sound;

        public PlayStaticSound(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
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
            instruccionesText.Text = "Y = Play, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            Modifiers.addFile("WAV-File", MediaDir + "Sound\\campanadas horas.wav", "WAVs|*.wav");

            //Modifier para loop
            Modifiers.addBoolean("PlayLoop", "Play Loop", false);

            var filePath = (string)Modifiers["WAV-File"];
            loadSound(filePath);
        }

        public override void Update()
        {
            PreUpdate();
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
                    sound.dispose();
                    sound = null;
                }

                //Cargar sonido
                sound = new TgcStaticSound();
                sound.loadSound(currentFile, DirectSound.DsDevice);

                currentSoundText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
        }

        public override void Render()
        {
            PreRender();

            //Ver si cambio el WAV
            var filePath = (string)Modifiers["WAV-File"];
            loadSound(filePath);

            //Contro el input de teclado
            if (TgcD3dInput.Instance.keyPressed(Key.Y))
            {
                var playLoop = (bool)Modifiers["PlayLoop"];
                sound.play(playLoop);
            }
            else if (TgcD3dInput.Instance.keyPressed(Key.O))
            {
                sound.stop();
            }

            //Render texto
            currentSoundText.render();
            instruccionesText.render();

            PostRender();
        }

        public override void Dispose()
        {
            sound.dispose();
        }
    }
}