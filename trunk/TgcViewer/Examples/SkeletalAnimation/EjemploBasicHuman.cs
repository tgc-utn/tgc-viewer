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
using System.IO;

namespace Examples.SkeletalAnimation
{
    /// <summary>
    /// Ejemplo BasicHuman:
    /// Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    /// 
    /// Utiliza el esqueleto genérico BasicHuman provisto por la cátedra para
    /// animar varios modelos distintos mediante animación esquelética.
    /// El esqueleto posee una lista de animaciones default que pueden ser reutilizadas
    /// para varios modelos distintos que se hayan acoplado a estos huesos.
    /// 
    /// 
    /// Autor: Leandro Barbagallo, Matías Leone
    /// 
    /// </summary>
    public class BasicHuman : TgcExample
    {

        string selectedMesh;
        string selectedAnim;
        TgcSkeletalMesh mesh;
        Color currentColor;
        TgcSkeletalBoneAttach attachment;
        bool showAttachment;
        string mediaPath;
        string[] animationsPath;


        public override string getCategory()
        {
            return "SkeletalAnimation";
        }

        public override string getName()
        {
            return "BasicHuman";
        }

        public override string getDescription()
        {
            return "Utiliza el esqueleto genérico BasicHuman provisto por la cátedra para animar varios modelos distintos mediante animación esquelética";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Path para carpeta de texturas de la malla
            mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";

            //Cargar dinamicamente todos los Mesh animados que haya en el directorio
            DirectoryInfo dir = new DirectoryInfo(mediaPath);
            FileInfo[] meshFiles = dir.GetFiles("*-TgcSkeletalMesh.xml", SearchOption.TopDirectoryOnly);
            string[] meshList = new string[meshFiles.Length];
            for (int i = 0; i < meshFiles.Length; i++)
            {
                string name = meshFiles[i].Name.Replace("-TgcSkeletalMesh.xml", "");
                meshList[i] = name;
            }

            //Cargar dinamicamente todas las animaciones que haya en el directorio "Animations"
            DirectoryInfo dirAnim = new DirectoryInfo(mediaPath + "Animations\\");
            FileInfo[] animFiles = dirAnim.GetFiles("*-TgcSkeletalAnim.xml", SearchOption.TopDirectoryOnly);
            string[] animationList = new string[animFiles.Length];
            animationsPath = new string[animFiles.Length];
            for (int i = 0; i < animFiles.Length; i++)
            {
                string name = animFiles[i].Name.Replace("-TgcSkeletalAnim.xml", "");
                animationList[i] = name;
                animationsPath[i] = animFiles[i].FullName;
            }

            //Cargar mesh inicial
            selectedAnim = animationList[0];
            changeMesh(meshList[0]);


            //Modifier para elegir modelo
            GuiController.Instance.Modifiers.addInterval("mesh", meshList, 0);

            //Agregar combo para elegir animacion
            GuiController.Instance.Modifiers.addInterval("animation", animationList, 0);

            //Modifier para especificar si la animación se anima con loop
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

            //Modifier para habilitar attachment
            showAttachment = false;
            GuiController.Instance.Modifiers.addBoolean("Attachment", "Attachment:", showAttachment);

        }

        /// <summary>
        /// Cargar una nueva malla
        /// </summary>
        private void changeMesh(string meshName)
        {
            if (selectedMesh == null || selectedMesh != meshName)
            {
                if (mesh != null)
                {
                    mesh.dispose();
                    mesh = null;
                }

                selectedMesh = meshName;

                //Cargar mesh y animaciones
                TgcSkeletalLoader loader = new TgcSkeletalLoader();
                mesh = loader.loadMeshAndAnimationsFromFile(mediaPath + selectedMesh + "-TgcSkeletalMesh.xml", mediaPath, animationsPath);

                //Crear esqueleto a modo Debug
                mesh.buildSkletonMesh();

                //Elegir animacion inicial
                mesh.playAnimation(selectedAnim, true);

                //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
                attachment = new TgcSkeletalBoneAttach();
                TgcBox attachmentBox = TgcBox.fromSize(new Vector3(2, 40, 2), Color.Red);
                attachment.Mesh = attachmentBox.toMesh("attachment");
                attachment.Bone = mesh.getBoneByName("Bip01 L Hand");
                attachment.Offset = Matrix.Translation(3, -15, 0);
                attachment.updateValues();


                //Configurar camara
                GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
            }
        }

        private void changeAnimation(string animation)
        {
            if (selectedAnim != animation)
            {
                selectedAnim = animation;
                mesh.playAnimation(selectedAnim, true);
            }
        }



        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Ver si cambio la malla
            string meshPath = (string)GuiController.Instance.Modifiers.getValue("mesh");
            changeMesh(meshPath);

            //Ver si cambio la animacion
            string anim = (string)GuiController.Instance.Modifiers.getValue("animation");
            changeAnimation(anim);

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
            //La malla también hace dispose del attachment
            mesh.dispose();
            mesh = null;
            selectedMesh = null;
        }

    }
}
