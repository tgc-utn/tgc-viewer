using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SkeletalAnimation;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Animation
{
    /// <summary>
    ///     Ejemplo SkeletalAnimation:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animacion - Skeletal Animation
    ///     Carga un personaje animado con el megtodo de Animacion Esqueletica, utilizando
    ///     la herramienta TgcSkeletalLoader.
    ///     Es una alternativa a la herramienta de animacion TgcKeyFrameLoader.
    ///     Se crea un Modifier para que el usuario puede alternar la animacion que se muestra.
    ///     La herramienta TgcSkeletalLoader crea una malla del tipo TgcSkeletalMesh.
    ///     Una malla TgcSkeletalMesh puede tener una o varias animaciones.
    ///     Cada animacion es un archivo XML diferente.
    ///     La estructura general de la malla tambien es un XML diferente.
    ///     Todos los XML son del formato TGC.
    ///     Muestra como renderizar el esqueleto del modelo.
    ///     Tambien muestra como agregar un objeto "Attachment" que siga un hueso del modelo.
    ///     Autor: Leandro Barbagallo, Matias Leone
    /// </summary>
    public class SkeletalAnimation : TGCExampleViewer
    {
        private TgcSkeletalBoneAttach attachment;
        private Color currentColor;
        private TgcSkeletalMesh mesh;
        private string selectedAnim;
        private bool showAttachment;

        public SkeletalAnimation(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Animation";
            Name = "Skeletal Animation";
            Description = "Muestra como cargar un personaje con animacion esqueletica, en formato TGC.";
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

            //Modifier para especificar si la animacion se anima con loop
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
            var attachmentBox = TgcBox.fromSize(new TGCVector3(5, 100, 5), Color.Blue);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = mesh.getBoneByName("Bip01 L Hand");
            attachment.Offset = TGCMatrix.Translation(10, -40, 0);
            attachment.updateValues();

            //Modifier para habilitar attachment
            showAttachment = false;
            Modifiers.addBoolean("Attachment", "Attachment:", showAttachment);

            //Configurar camara
            Camara = new TgcRotationalCamera(new TGCVector3(0, 70, 0), 200, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

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
                    //Al agregar el attachment, el modelo se encarga de renderizarlo en forma automatica
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
            mesh.Render();

            //Se puede renderizar todo mucho mas simple (sin esqueleto) de la siguiente forma:
            //mesh.animateAndRender();

            //BoundingBox
            var showBB = (bool)Modifiers["BoundingBox"];
            if (showBB)
            {
                mesh.BoundingBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            //La malla tambien hace dispose del attachment
            mesh.Dispose();
        }
    }
}