using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Particle;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Particles
{
    /// <summary>
    ///     Emisor de Particulas
    /// </summary>
    public class EmisorDeParticulas : TGCExampleViewer
    {
        private TGCBox box;
        private ParticleEmitter emitter;
        private int selectedParticleCount;
        private string selectedTextureName;
        private string[] textureNames;
        private string texturePath;

        public EmisorDeParticulas(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Particles";
            Name = "Emisor de Particulas";
            Description = "Emisor de Particulas";
        }

        public override void Init()
        {
            //Directorio de texturas
            texturePath = MediaDir + "Texturas\\Particles\\";

            //Texturas de particulas a utilizar
            textureNames = new[]
            {
                "pisada.png",
                "fuego.png",
                "humo.png",
                "hoja.png",
                "agua.png",
                "nieve.png"
            };

            //Modifiers
            Modifiers.addInterval("texture", textureNames, 0);
            Modifiers.addInt("cantidad", 1, 30, 10);
            Modifiers.addFloat("minSize", 0.25f, 10, 4);
            Modifiers.addFloat("maxSize", 0.25f, 10, 6);
            Modifiers.addFloat("timeToLive", 0.25f, 2, 1);
            Modifiers.addFloat("frecuencia", 0.25f, 4, 1);
            Modifiers.addInt("dispersion", 50, 400, 100);
            Modifiers.addVertex3f("speedDir", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50),
                new TGCVector3(30, 30, 30));

            //Crear emisor de particulas
            selectedTextureName = textureNames[0];
            selectedParticleCount = 10;
            emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            emitter.Position = TGCVector3.Empty;

            box = TGCBox.fromSize(new TGCVector3(0, -30, 0), new TGCVector3(10, 10, 10), Color.Blue);

            Camara = new TgcRotationalCamera(TGCVector3.Empty, 300f, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();
            //IMPORTANTE PARA PERMITIR ESTE EFECTO.
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();

            //Cambiar cantidad de particulas, implica crear un nuevo emisor
            var cantidad = (int)Modifiers["cantidad"];
            if (selectedParticleCount != cantidad)
            {
                //Crear nuevo emisor
                selectedParticleCount = cantidad;
                emitter.dispose();
                emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            }

            //Cambiar textura
            var textureName = (string)Modifiers["texture"];
            if (selectedTextureName != textureName)
            {
                selectedTextureName = textureName;
                emitter.changeTexture(texturePath + selectedTextureName);
            }

            //Actualizar los demas parametros
            emitter.MinSizeParticle = (float)Modifiers["minSize"];
            emitter.MaxSizeParticle = (float)Modifiers["maxSize"];
            emitter.ParticleTimeToLive = (float)Modifiers["timeToLive"];
            emitter.CreationFrecuency = (float)Modifiers["frecuencia"];
            emitter.Dispersion = (int)Modifiers["dispersion"];
            emitter.Speed = (TGCVector3)Modifiers["speedDir"];

            //Render de emisor
            emitter.render(ElapsedTime);

            box.Render();

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar recursos
            emitter.dispose();

            box.Dispose();
        }
    }
}