using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Particle;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Particles
{
    /// <summary>
    ///     Emisor de Particulas
    /// </summary>
    public class EmisorDeParticulas : TGCExampleViewer
    {
        private TGCIntervalModifier textureModifier;
        private TGCIntModifier cantidadModifier;
        private TGCFloatModifier minSizeModifier;
        private TGCFloatModifier maxSizeModifier;
        private TGCFloatModifier timeToLiveModifier;
        private TGCFloatModifier frecuenciaModifier;
        private TGCIntModifier dispersionModifier;
        private TGCVertex3fModifier speedDirModifier;

        private TGCBox box;
        private ParticleEmitter emitter;
        private int selectedParticleCount;
        private string selectedTextureName;
        private string[] textureNames;
        private string texturePath;

        public EmisorDeParticulas(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
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
            textureModifier = AddInterval("texture", textureNames, 0);
            cantidadModifier = AddInt("cantidad", 1, 30, 10);
            minSizeModifier = AddFloat("minSize", 0.25f, 10, 4);
            maxSizeModifier = AddFloat("maxSize", 0.25f, 10, 6);
            timeToLiveModifier = AddFloat("timeToLive", 0.25f, 2, 1);
            frecuenciaModifier = AddFloat("frecuencia", 0.25f, 4, 1);
            dispersionModifier = AddInt("dispersion", 50, 400, 100);
            speedDirModifier = AddVertex3f("speedDir", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), new TGCVector3(30, 30, 30));

            //Crear emisor de particulas
            selectedTextureName = textureNames[0];
            selectedParticleCount = 10;
            emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            emitter.Position = TGCVector3.Empty;

            box = TGCBox.fromSize(new TGCVector3(0, -30, 0), new TGCVector3(10, 10, 10), Color.Blue);

            Camera = new TgcRotationalCamera(TGCVector3.Empty, 300f, Input);
        }

        public override void Update()
        {
            //  Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones ante ellas.
        }

        public override void Render()
        {
            PreRender();
            //IMPORTANTE PARA PERMITIR ESTE EFECTO.
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();

            //Cambiar cantidad de particulas, implica crear un nuevo emisor
            var cantidad = cantidadModifier.Value;
            if (selectedParticleCount != cantidad)
            {
                //Crear nuevo emisor
                selectedParticleCount = cantidad;
                emitter.dispose();
                emitter = new ParticleEmitter(texturePath + selectedTextureName, selectedParticleCount);
            }

            //Cambiar textura
            var textureName = textureModifier.Value.ToString();
            if (selectedTextureName != textureName)
            {
                selectedTextureName = textureName;
                emitter.changeTexture(texturePath + selectedTextureName);
            }

            //Actualizar los demas parametros
            emitter.MinSizeParticle = minSizeModifier.Value;
            emitter.MaxSizeParticle = maxSizeModifier.Value;
            emitter.ParticleTimeToLive = timeToLiveModifier.Value;
            emitter.CreationFrecuency = frecuenciaModifier.Value;
            emitter.Dispersion = dispersionModifier.Value;
            emitter.Speed = speedDirModifier.Value;

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