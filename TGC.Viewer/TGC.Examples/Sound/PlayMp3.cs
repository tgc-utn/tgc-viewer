using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using TGC.Core._2D;
using TGC.Core.Example;
using TGC.Viewer;
using TGC.Viewer.Utils.Sound;

namespace TGC.Examples.Sound
{
    /// <summary>
    ///     Ejemplo PlayMp3:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - GameEngine
    ///     Muestra como reproducir un archivo de sonido en formato MP3.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class PlayMp3 : TgcExample
    {
        private string currentFile;
        private TgcText2d currentMusicText;
        private TgcText2d instruccionesText;

        public override string getCategory()
        {
            return "Sound";
        }

        public override string getName()
        {
            return "Play Mp3";
        }

        public override string getDescription()
        {
            return "Muestra como reproducir un archivo de sonido en formato MP3.";
        }

        public override void init()
        {
            //Texto para la musica actual
            currentMusicText = new TgcText2d();
            currentMusicText.Text = "No music";
            currentMusicText.Position = new Point(50, 20);
            currentMusicText.Color = Color.Gold;
            currentMusicText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Italic));

            //Texto para las instrucciones de uso
            instruccionesText = new TgcText2d();
            instruccionesText.Text = "Y = Play, U = Pause, I = Resume, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            GuiController.Instance.Modifiers.addFile("MP3-File",
                GuiController.Instance.ExamplesMediaDir + "Music\\I am The Money.mp3", "MP3s|*.mp3");
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
                GuiController.Instance.Mp3Player.closeFile();
                GuiController.Instance.Mp3Player.FileName = currentFile;

                currentMusicText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
        }

        public override void render(float elapsedTime)
        {
            //Ver si cambio el MP3
            var filePath = (string)GuiController.Instance.Modifiers["MP3-File"];
            loadMp3(filePath);

            //Contro del reproductor por teclado
            var player = GuiController.Instance.Mp3Player;
            var currentState = player.getStatus();
            if (GuiController.Instance.D3dInput.keyPressed(Key.Y))
            {
                if (currentState == TgcMp3Player.States.Open)
                {
                    //Reproducir MP3
                    player.play(true);
                }
                if (currentState == TgcMp3Player.States.Stopped)
                {
                    //Parar y reproducir MP3
                    player.closeFile();
                    player.play(true);
                }
            }
            else if (GuiController.Instance.D3dInput.keyPressed(Key.U))
            {
                if (currentState == TgcMp3Player.States.Playing)
                {
                    //Pausar el MP3
                    player.pause();
                }
            }
            else if (GuiController.Instance.D3dInput.keyPressed(Key.I))
            {
                if (currentState == TgcMp3Player.States.Paused)
                {
                    //Resumir la ejecución del MP3
                    player.resume();
                }
            }
            else if (GuiController.Instance.D3dInput.keyPressed(Key.O))
            {
                if (currentState == TgcMp3Player.States.Playing)
                {
                    //Parar el MP3
                    player.stop();
                }
            }

            //Render texto
            currentMusicText.render();
            instruccionesText.render();
        }

        public override void close()
        {
            //el Mp3Player se limpia solo al salir del ejemplo
        }
    }
}