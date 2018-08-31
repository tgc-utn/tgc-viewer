using Microsoft.DirectX.DirectInput;
using System;
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
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - GameEngine
    ///     Muestra como reproducir un archivo de sonido en formato MP3.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class PlayMp3 : TGCExampleViewer
    {
        //private TgcMp3Player mp3Player;

        private string currentFile;
        private TGCFileModifier mp3FileModifier;
        private TgcText2D currentMusicText;
        private TgcText2D instruccionesText;

        private TgcSound music;

        public PlayMp3(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Sound";
            Name = "Play Mp3";
            Description = "Muestra como reproducir un archivo de sonido en formato MP3.";
        }

        public override void Init()
        {
            //Texto para la musica actual
            currentMusicText = new TgcText2D();
            currentMusicText.Text = "No music";
            currentMusicText.Position = new Point(50, 20);
            currentMusicText.Color = Color.Gold;
            currentMusicText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Italic));

            //Texto para las instrucciones de uso
            instruccionesText = new TgcText2D();
            instruccionesText.Text = "Y = Play/Resume, U = Pause, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            mp3FileModifier = AddFile("MP3-File", MediaDir + "Music\\I am The Money.mp3", "");

        }

        public override void Update()
        {
            PreUpdate();
            loadMp3(mp3FileModifier.Value);

            if (Input.keyPressed(Key.Y))
            {
                music.Play();
            }
            if(Input.keyPressed(Key.U))
            {
                music.Pause();
            }
            if (Input.keyPressed(Key.O))
            {
                music.Stop();
            }
            PostUpdate();
        }

        /// <summary>
        ///     Cargar un nuevo MP3 si hubo una variacion
        /// </summary>
        private void loadMp3(string filePath)
        {
            if (currentFile == null || currentFile != filePath)
            {
                currentFile = filePath;

                //Cargar archivo
                if (music != null)
                {
                    music.Dispose();
                }
                try
                {
                    music = new TgcSound(currentFile);
                }
                catch(Exception e)
                {
                    currentMusicText.Text = "Exception: " + e.Message;
                    return;
                }
                music = new TgcSound(currentFile);
                music.Play();

                currentMusicText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
        }

        public override void Render()
        {
            PreRender();
            currentMusicText.render();
            instruccionesText.render();
            PostRender();
        }

        public override void Dispose()
        {
            music.Dispose();
        }
    }
}