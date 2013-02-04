using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace Examples.SkeletalAnimation
{
    /// <summary>
    /// Ejemplo EjemploMeshInstance:
    /// Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     # Unidad 7 - Técnicas de Optimización - Instancias de Modelos
    /// 
    /// Muestra como crear instancias de modelos animados con Skeletal Animation.
    /// Al crear instancias de un único modelo original se reutiliza toda su información
    /// gráfica (animaciones, vértices, texturas, etc.)
    /// 
    /// 
    /// Autor: Leandro Barbagallo, Matías Leone
    /// 
    /// </summary>
    public class EjemploMeshInstance : TgcExample
    {
        TgcBox suelo;
        TgcSkeletalMesh original;
        List<TgcSkeletalMesh> instances;

        public override string getCategory()
        {
            return "SkeletalAnimation";
        }

        public override string getName()
        {
            return "MeshInstance";
        }

        public override string getDescription()
        {
            return "Muestra como crear instancias de modelos animados con Skeletal Animation.";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear suelo
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack2\\rock_floor1.jpg");
            suelo = TgcBox.fromSize(new Vector3(500, 0, 500), new Vector3(2000, 0, 2000), pisoTexture);


            //Cargar malla original
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);

            //Agregar animación a original
            loader.loadAnimationFromFile(original, mediaPath + "Patear-TgcSkeletalAnim.xml");

            //Agregar attachment a original
            TgcSkeletalBoneAttach attachment = new TgcSkeletalBoneAttach();
            TgcBox attachmentBox = TgcBox.fromSize(new Vector3(3, 60, 3), Color.Green);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = original.getBoneByName("Bip01 L Hand");
            attachment.Offset = Matrix.Translation(10, -40, 0);
            attachment.updateValues();
            original.Attachments.Add(attachment);


            //Crear 9 instancias mas de este modelo, pero sin volver a cargar el modelo entero cada vez
            float offset = 200;
            int cantInstancias = 4;
            instances = new List<TgcSkeletalMesh>();
            for (int i = 0; i < cantInstancias; i++)
			{
                TgcSkeletalMesh instance = original.createMeshInstance(original.Name + i);

                instance.move(i * offset, 0, 0);
                instances.Add(instance);
			}


            //Especificar la animación actual para todos los modelos
            original.playAnimation("Patear");
            foreach (TgcSkeletalMesh instance in instances)
            {
                instance.playAnimation("Patear");
            }


            //Camara en primera persona
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.MovementSpeed = 400;
            GuiController.Instance.FpsCamera.JumpSpeed = 400;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(293.201f, 291.0797f, -604.6647f), new Vector3(299.1028f, -63.9185f, 330.1836f));
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar suelo
            suelo.render();

            //Renderizar original e instancias
            original.animateAndRender();
            foreach (TgcSkeletalMesh instance in instances)
            {
                instance.animateAndRender();
            }

        }

        public override void close()
        {
            suelo.dispose();

            //Al hacer dispose del original, se hace dispose automáticamente de todas las instancias
            original.dispose();
        }

    }
}
