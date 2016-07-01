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
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - GameEngine
    ///     Muestra como reproducir un archivo de sonido en formato MP3.
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class PlayMp3 : TGCExampleViewer
    {
        private string currentFile;
        private TgcText2D currentMusicText;
        private TgcText2D instruccionesText;
        private TgcMp3Player mp3Player;

        public PlayMp3(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
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
            instruccionesText.Text = "Y = Play, U = Pause, I = Resume, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            Modifiers.addFile("MP3-File", MediaDir + "Music\\I am The Money.mp3", "MP3s|*.mp3");

            mp3Player = new TgcMp3Player();
        }

        public override void Update()
        {
            PreUpdate();
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
                mp3Player.closeFile();
                mp3Player.FileName = currentFile;

                currentMusicText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
        }

        public override void Render()
        {
            PreRender();

            //Ver si cambio el MP3
            var filePath = (string)Modifiers["MP3-File"];
            loadMp3(filePath);

            //Contro del reproductor por teclado
            var currentState = mp3Player.getStatus();
            if (TgcD3dInput.Instance.keyPressed(Key.Y))
            {
                if (currentState == TgcMp3Player.States.Open)
                {
                    //Reproducir MP3
                    mp3Player.play(true);
                }
                if (currentState == TgcMp3Player.States.Stopped)
                {
                    //Parar y reproducir MP3
                    mp3Player.closeFile();
                    mp3Player.play(true);
                }
            }
            else if (TgcD3dInput.Instance.keyPressed(Key.U))
            {
                if (currentState == TgcMp3Player.States.Playing)
                {
                    //Pausar el MP3
                    mp3Player.pause();
                }
            }
            else if (TgcD3dInput.Instance.keyPressed(Key.I))
            {
                if (currentState == TgcMp3Player.States.Paused)
                {
                    //Resumir la ejecuci�n del MP3
                    mp3Player.resume();
                }
            }
            else if (TgcD3dInput.Instance.keyPressed(Key.O))
            {
                if (currentState == TgcMp3Player.States.Playing)
                {
                    //Parar el MP3
                    mp3Player.stop();
                }
            }

            //Render texto
            currentMusicText.render();
            instruccionesText.render();

            PostRender();
        }

        public override void Dispose()
        {
            mp3Player.closeFile();
        }
    }
}