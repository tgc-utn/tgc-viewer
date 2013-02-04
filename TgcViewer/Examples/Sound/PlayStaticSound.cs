
using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils._2D;
using System.IO;

namespace Examples.Sound
{
    /// <summary>
    /// Ejemplo PlayMp3:
    /// Unidades PlayStaticSound:
    ///     # Unidad 3 - Conceptos Básicos de 3D - GameEngine
    /// 
    /// Muestra como reproducir un archivo de sonido estático en formato WAV.
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class PlayStaticSound : TgcExample
    {
        string currentFile;
        TgcText2d currentSoundText;
        TgcText2d instruccionesText;
        TgcStaticSound sound;

        public override string getCategory()
        {
            return "Sound";
        }

        public override string getName()
        {
            return "Play StaticSound";
        }

        public override string getDescription()
        {
            return "Muestra como reproducir un archivo de sonido estático en formato WAV.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Texto para el sonido actual
            currentSoundText = new TgcText2d();
            currentSoundText.Text = "No sound";
            currentSoundText.Position = new Point(50, 20);
            currentSoundText.Color = Color.Gold;
            currentSoundText.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 16, FontStyle.Italic));

            //Texto de instrucciones
            instruccionesText = new TgcText2d();
            instruccionesText.Text = "Y = Play, O = Stop.";
            instruccionesText.Position = new Point(50, 60);
            instruccionesText.Color = Color.Green;
            instruccionesText.changeFont(new System.Drawing.Font(FontFamily.GenericMonospace, 16, FontStyle.Bold));

            //Modifier para archivo MP3
            currentFile = null;
            GuiController.Instance.Modifiers.addFile("WAV-File", GuiController.Instance.ExamplesMediaDir + "Sound\\campanadas horas.wav", "WAVs|*.wav");
        
            //Modifier para loop
            GuiController.Instance.Modifiers.addBoolean("PlayLoop", "Play Loop", false);
        }

        /// <summary>
        /// Cargar un nuevo WAV si hubo una variacion
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
                sound.loadSound(currentFile);

                currentSoundText.Text = "Playing: " + new FileInfo(currentFile).Name;
            }
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio el WAV
            string filePath = (string)GuiController.Instance.Modifiers["WAV-File"];
            loadSound(filePath);


            //Contro el input de teclado
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Y))
            {
                bool playLoop = (bool)GuiController.Instance.Modifiers["PlayLoop"];
                sound.play(playLoop);
            }
            else if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.O))
            {
                sound.stop();
            }


            //Render texto
            currentSoundText.render();
            instruccionesText.render();
        }

        

        public override void close()
        {
            sound.dispose();
        }

    }
}
