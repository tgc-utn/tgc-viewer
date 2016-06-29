using Microsoft.DirectX;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.SkeletalAnimation
{
    /// <summary>
    ///     Ejemplo EjemploMeshInstance:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animacion - Skeletal Animation
    ///     # Unidad 7 - Tecnicas de Optimizacion - Instancias de Modelos
    ///     Muestra como crear instancias de modelos animados con Skeletal Animation.
    ///     Al crear instancias de un unico modelo original se reutiliza toda su informacion
    ///     grafica (animaciones, vertices, texturas, etc.)
    ///     Autor: Leandro Barbagallo, Matias Leone
    /// </summary>
    public class EjemploMeshInstance : TGCExampleViewer
    {
        private List<TgcSkeletalMesh> instances;
        private TgcSkeletalMesh original;
        private TgcBox suelo;

        public EjemploMeshInstance(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "SkeletalAnimation";
            Name = "MeshInstance";
            Description = "Muestra como crear instancias de modelos animados con Skeletal Animation.";
        }

        public override void Init()
        {
            //Crear suelo
            var pisoTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "Texturas\\Quake\\TexturePack2\\rock_floor1.jpg");
            suelo = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(2000, 0, 2000), pisoTexture);

            //Cargar malla original
            var loader = new TgcSkeletalLoader();
            var pathMesh = MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";
            var mediaPath = MediaDir + "SkeletalAnimations\\Robot\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);

            //Agregar animacion a original
            loader.loadAnimationFromFile(original, mediaPath + "Patear-TgcSkeletalAnim.xml");

            //Agregar attachment a original
            var attachment = new TgcSkeletalBoneAttach();
            var attachmentBox = TgcBox.fromSize(new Vector3(3, 60, 3), Color.Green);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = original.getBoneByName("Bip01 L Hand");
            attachment.Offset = Matrix.Translation(10, -40, 0);
            attachment.updateValues();
            original.Attachments.Add(attachment);

            //Crear 9 instancias mas de este modelo, pero sin volver a cargar el modelo entero cada vez
            float offset = 200;
            var cantInstancias = 4;
            instances = new List<TgcSkeletalMesh>();
            for (var i = 0; i < cantInstancias; i++)
            {
                var instance = original.createMeshInstance(original.Name + i);

                instance.move(i * offset, 0, 0);
                instances.Add(instance);
            }

            //Especificar la animacion actual para todos los modelos
            original.playAnimation("Patear");
            foreach (var instance in instances)
            {
                instance.playAnimation("Patear");
            }

            //Camara en primera persona
            Camara = new TgcFpsCamera(new Vector3(293.201f, 291.0797f, -604.6647f));
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Renderizar suelo
            suelo.render();

            //Renderizar original e instancias
            original.animateAndRender(ElapsedTime);
            foreach (var instance in instances)
            {
                instance.animateAndRender(ElapsedTime);
            }

            PostRender();
        }

        public override void Dispose()
        {
            suelo.dispose();

            //Al hacer dispose del original, se hace dispose automaticamente de todas las instancias
            original.dispose();
        }
    }
}