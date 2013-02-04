using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Particles;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Particles
{
    /// <summary>
    /// Emisor de Particulas
    /// </summary>
    public class EmisorDeParticulas : TgcExample
    {
        ParticleEmitter emitter;
        string texturePath;
        string[] textureNames;
        string selectedTextureName;
        int selectedParticleCount;
        TgcBox box;

        public override string getCategory()
        {
            return "Particles";
        }

        public override string getName()
        {
            return "Emisor de Particulas";
        }

        public override string getDescription()
        {
            return "Emisor de Particulas";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Directorio de texturas
            texturePath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Particles\\";

            //Texturas de particulas a utilizar
            textureNames = new string[] {
                "pisada.png",
                "fuego.png",
                "humo.png",
                "hoja.png",
                "agua.png",
                "nieve.png"
            };


            //Modifiers
            GuiController.Instance.Modifiers.addInterval("texture", textureNames, 0);
            GuiController.Instance.Modifiers.addInt("cantidad", 1, 30, 10);
            GuiController.Instance.Modifiers.addFloat("minSize", 0.25f, 10, 4);
            GuiController.Instance.Modifiers.addFloat("maxSize", 0.25f, 10, 6);
            GuiController.Instance.Modifiers.addFloat("timeToLive", 0.25f, 2, 1);
            GuiController.Instance.Modifiers.addFloat("frecuencia", 0.25f, 4, 1);
            GuiController.Instance.Modifiers.addInt("dispersion", 50, 400, 100);
            GuiController.Instance.Modifiers.addVertex3f("speedDir", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(30, 30, 30));
            


            //Crear emisor de particulas
            selectedTextureName = textureNames[0];
            selectedParticleCount = 10;
            emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            emitter.Position = new Vector3(0, 0, 0);


            box = TgcBox.fromSize(new Vector3(0, -30, 0), new Vector3(10, 10, 10), Color.Blue);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            
            //Cambiar cantidad de particulas, implica crear un nuevo emisor
            int cantidad = (int)GuiController.Instance.Modifiers["cantidad"];
            if (selectedParticleCount != cantidad)
            {
                //Crear nuevo emisor
                selectedParticleCount = cantidad;
                emitter.dispose();
                emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            }

            //Cambiar textura
            string textureName = (string)GuiController.Instance.Modifiers["texture"];
            if(selectedTextureName != textureName)
            {
                selectedTextureName = textureName;
                emitter.changeTexture(texturePath + selectedTextureName);
            }
            
            //Actualizar los dem�s parametros
            emitter.MinSizeParticle = (float)GuiController.Instance.Modifiers["minSize"];
            emitter.MaxSizeParticle = (float)GuiController.Instance.Modifiers["maxSize"];
            emitter.ParticleTimeToLive = (float)GuiController.Instance.Modifiers["timeToLive"];
            emitter.CreationFrecuency = (float)GuiController.Instance.Modifiers["frecuencia"];
            emitter.Dispersion = (int)GuiController.Instance.Modifiers["dispersion"];
            emitter.Speed = (Vector3)GuiController.Instance.Modifiers["speedDir"];


            //Render de emisor
            emitter.render();


            box.render();
        }

        

        public override void close()
        {
            //Liberar recursos
            emitter.dispose();


            box.dispose();
        }

    }
}
