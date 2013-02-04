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
    /// Ejemplo EjemploSkeletalLoader:
    /// Unidades Involucradas:
    ///     # Unidad 5 - Animaci�n - Skeletal Animation
    /// 
    /// Carga un personaje animado con el m�todo de Animacion Esqueletica, utilizando
    /// la herramienta TgcSkeletalLoader.
    /// Es una alternativa a la herramienta de animaci�n TgcKeyFrameLoader.
    /// Se crea un Modifier para que el usuario puede alternar la animaci�n que se muestra.
    /// La herramienta TgcSkeletalLoader crea una malla del tipo TgcSkeletalMesh.
    /// Una malla TgcSkeletalMesh puede tener una o varias animaciones.
    /// Cada animacion es un archivo XML diferente.
    /// La estructura general de la malla tambien es un XML diferente.
    /// Todos los XML son del formato TGC.
    /// Muestra como renderizar el esqueleto del modelo.
    /// Tambi�n muestra como agregar un objeto "Attachment" que siga un hueso del modelo.
    /// 
    /// 
    /// Autor: Leandro Barbagallo, Mat�as Leone
    /// 
    /// </summary>
    public class EjemploSkeletalLoader : TgcExample
    {

        TgcSkeletalMesh mesh;
        string selectedAnim;
        Color currentColor;
        TgcSkeletalBoneAttach attachment;
        bool showAttachment;

        public override string getCategory()
        {
            return "SkeletalAnimation";
        }

        public override string getName()
        {
            return "MeshLoader";
        }

        public override string getDescription()
        {
            return "Muestra como cargar un personaje con animaci�n esquel�tica, en formato TGC";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Paths para archivo XML de la malla
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";

            //Path para carpeta de texturas de la malla
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\";

            //Lista de animaciones disponibles
            string[] animationList = new string[]{
                "Parado",
                "Caminando",
                "Correr",
                "PasoDerecho",
                "PasoIzquierdo",
                "Empujar",
                "Patear",
                "Pegar",
                "Arrojar",
            };

            //Crear rutas con cada animacion
            string[] animationsPath = new string[animationList.Length];
            for (int i = 0; i < animationList.Length; i++)
            {
                animationsPath[i] = mediaPath + animationList[i] + "-TgcSkeletalAnim.xml";
            }

            //Cargar mesh y animaciones
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            mesh = loader.loadMeshAndAnimationsFromFile(pathMesh, mediaPath, animationsPath);

            //Crear esqueleto a modo Debug
            mesh.buildSkletonMesh();

            //Agregar combo para elegir animacion
            GuiController.Instance.Modifiers.addInterval("animation", animationList, 0);
            selectedAnim = animationList[0];

            //Modifier para especificar si la animaci�n se anima con loop
            bool animateWithLoop = true;
            GuiController.Instance.Modifiers.addBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para renderizar el esqueleto
            bool renderSkeleton = false;
            GuiController.Instance.Modifiers.addBoolean("renderSkeleton", "Show skeleton:", renderSkeleton);

            //Modifier para FrameRate
            GuiController.Instance.Modifiers.addFloat("frameRate", 0, 100, 30);

            //Modifier para color
            currentColor = Color.White;
            GuiController.Instance.Modifiers.addColor("Color", currentColor);

            //Modifier para BoundingBox
            GuiController.Instance.Modifiers.addBoolean("BoundingBox", "BoundingBox:", false);

            //Elegir animacion Caminando
            mesh.playAnimation(selectedAnim, true);

            //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
            attachment = new TgcSkeletalBoneAttach();
            TgcBox attachmentBox = TgcBox.fromSize(new Vector3(5, 100, 5), Color.Blue);
            attachment.Mesh = attachmentBox.toMesh("attachment");
            attachment.Bone = mesh.getBoneByName("Bip01 L Hand");
            attachment.Offset = Matrix.Translation(10, -40, 0);
            attachment.updateValues();

            //Modifier para habilitar attachment
            showAttachment = false;
            GuiController.Instance.Modifiers.addBoolean("Attachment", "Attachment:", showAttachment);

            //Configurar camara
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 70, 0), 200);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio la animacion
            string anim = (string)GuiController.Instance.Modifiers.getValue("animation");
            if (!anim.Equals(selectedAnim))
            {
                //Ver si animamos con o sin loop
                bool animateWithLoop = (bool)GuiController.Instance.Modifiers.getValue("loop");
                float frameRate = (float)GuiController.Instance.Modifiers.getValue("frameRate");

                //Cargar nueva animacion elegida
                selectedAnim = anim;
                mesh.playAnimation(selectedAnim, animateWithLoop, frameRate);
            }

            //Ver si rendeizamos el esqueleto
            bool renderSkeleton = (bool)GuiController.Instance.Modifiers.getValue("renderSkeleton");

            //Ver si cambio el color
            Color selectedColor = (Color)GuiController.Instance.Modifiers.getValue("Color");
            if (currentColor == null || currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Agregar o quitar Attachment
            bool showAttachmentFlag = (bool)GuiController.Instance.Modifiers["Attachment"];
            if (showAttachment != showAttachmentFlag)
            {
                showAttachment = showAttachmentFlag;
                if (showAttachment)
                {
                    //Al agregar el attachment, el modelo se encarga de renderizarlo en forma autom�tica
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
            mesh.updateAnimation();

            //Solo malla o esqueleto, depende lo seleccionado
            mesh.RenderSkeleton = renderSkeleton;
            mesh.render();

            //Se puede renderizar todo mucho mas simple (sin esqueleto) de la siguiente forma:
            //mesh.animateAndRender();


            //BoundingBox
            bool showBB = (bool)GuiController.Instance.Modifiers["BoundingBox"];
            if (showBB)
            {
                mesh.BoundingBox.render();
            }

        }

        public override void close()
        {
            //La malla tambi�n hace dispose del attachment
            mesh.dispose();
        }

    }
}
