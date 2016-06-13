using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.SkeletalAnimation
{
    /// <summary>
    ///     Ejemplo EjemploSkeletalLoader:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     Carga un personaje animado con el método de Animacion Esqueletica, utilizando
    ///     la herramienta TgcSkeletalLoader.
    ///     Es una alternativa a la herramienta de animación TgcKeyFrameLoader.
    ///     Se crea un Modifier para que el usuario puede alternar la animación que se muestra.
    ///     La herramienta TgcSkeletalLoader crea una malla del tipo TgcSkeletalMesh.
    ///     Una malla TgcSkeletalMesh puede tener una o varias animaciones.
    ///     Cada animacion es un archivo XML diferente.
    ///     La estructura general de la malla tambien es un XML diferente.
    ///     Todos los XML son del formato TGC.
    ///     Muestra como renderizar el esqueleto del modelo.
    ///     También muestra como agregar un objeto "Attachment" que siga un hueso del modelo.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class EjemploSkeletalLoader : TgcExample
    {
        private TgcSkeletalBoneAttach attachment;
        private Color currentColor;
        private TgcSkeletalMesh mesh;
        private string selectedAnim;
        private bool showAttachment;

        public EjemploSkeletalLoader(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "SkeletalAnimation";
            Name = "MeshLoader";
            Description = "Muestra como cargar un personaje con animación esquelética, en formato TGC.";
        }

        public override void Init()
        {
            //Paths para archivo XML de la malla
            var pathMesh = MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";

            //Path para carpeta de texturas de la malla
            var mediaPath = MediaDir + "SkeletalAnimations\\Robot\\";

            //Lista de animaciones disponibles
            string[] animationList =
            {
                "Parado",
                "Caminando",
                "Correr",
                "PasoDerecho",
                "PasoIzquierdo",
                "Empujar",
                "Patear",
                "Pegar",
                "Arrojar"
            };

            //Crear rutas con cada animacion
            var animationsPath = new string[animationList.Length];
            for (var i = 0; i < animationList.Length; i++)
            {
                animationsPath[i] = mediaPath + animationList[i] + "-TgcSkeletalAnim.xml";
            }

            //Cargar mesh y animaciones
            var loader = new TgcSkeletalLoader();
            mesh = loader.loadMeshAndAnimationsFromFile(pathMesh, mediaPath, animationsPath);

            //Crear esqueleto a modo Debug
            mesh.buildSkletonMesh();

            //Agregar combo para elegir animacion
            Modifiers.addInterval("animation", animationList, 0);
            selectedAnim = animationList[0];

            //Modifier para especificar si la animación se anima con loop
            var animateWithLoop = true;
            Modifiers.addBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para renderizar el esqueleto
            var renderSkeleton = false;
            Modifiers.addBoolean("renderSkeleton", "Show skeleton:", renderSkeleton);

            //Modifier para FrameRate
            Modifiers.addFloat("frameRate", 0, 100, 30);

            //Modifier para color
            currentColor = Color.White;
            Modifiers.addColor("Color", currentColor);

            //Modifier para BoundingBox
            Modifiers.addBoolean("BoundingBox", "BoundingBox:", false);

            //Elegir animacion Caminando
            mesh.playAnimation(selectedAnim, true);

            //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
            attachment = new TgcSkeletalBoneAttach();
            var attachmentBox = TgcBox.fromSize(new Vector3(5, 100, 5), Color.Blue);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = mesh.getBoneByName("Bip01 L Hand");
            attachment.Offset = Matrix.Translation(10, -40, 0);
            attachment.updateValues();

            //Modifier para habilitar attachment
            showAttachment = false;
            Modifiers.addBoolean("Attachment", "Attachment:", showAttachment);

            //Configurar camara
            Camara = new TgcRotationalCamera(new Vector3(0, 70, 0), 200);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //Ver si cambio la animacion
            var anim = (string)Modifiers.getValue("animation");
            if (!anim.Equals(selectedAnim))
            {
                //Ver si animamos con o sin loop
                var animateWithLoop = (bool)Modifiers.getValue("loop");
                var frameRate = (float)Modifiers.getValue("frameRate");

                //Cargar nueva animacion elegida
                selectedAnim = anim;
                mesh.playAnimation(selectedAnim, animateWithLoop, frameRate);
            }

            //Ver si rendeizamos el esqueleto
            var renderSkeleton = (bool)Modifiers.getValue("renderSkeleton");

            //Ver si cambio el color
            var selectedColor = (Color)Modifiers.getValue("Color");
            if (currentColor == null || currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Agregar o quitar Attachment
            var showAttachmentFlag = (bool)Modifiers["Attachment"];
            if (showAttachment != showAttachmentFlag)
            {
                showAttachment = showAttachmentFlag;
                if (showAttachment)
                {
                    //Al agregar el attachment, el modelo se encarga de renderizarlo en forma automática
                    attachment.Mesh.Enabled = true;
                    mesh.Attachments.Add(attachment);
                }
                else
                {
                    attachment.Mesh.Enabled = false;
                    mesh.Attachments.Remove(attachment);
                }
            }

            //Actualizar animacion
            mesh.updateAnimation(ElapsedTime);

            //Solo malla o esqueleto, depende lo seleccionado
            mesh.RenderSkeleton = renderSkeleton;
            mesh.render();

            //Se puede renderizar todo mucho mas simple (sin esqueleto) de la siguiente forma:
            //mesh.animateAndRender();

            //BoundingBox
            var showBB = (bool)Modifiers["BoundingBox"];
            if (showBB)
            {
                mesh.BoundingBox.render();
            }

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            //La malla también hace dispose del attachment
            mesh.dispose();
        }
    }
}