using Microsoft.DirectX;
using System.Drawing;
using System.IO;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.SkeletalAnimation;
using TGC.Viewer;

namespace TGC.Examples.SkeletalAnimation
{
    /// <summary>
    ///     Ejemplo BasicHuman:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animación - Skeletal Animation
    ///     Utiliza el esqueleto genérico BasicHuman provisto por la cátedra para
    ///     animar varios modelos distintos mediante animación esquelética.
    ///     El esqueleto posee una lista de animaciones default que pueden ser reutilizadas
    ///     para varios modelos distintos que se hayan acoplado a estos huesos.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class BasicHuman : TgcExample
    {
        private string[] animationsPath;
        private TgcSkeletalBoneAttach attachment;
        private Color currentColor;
        private string mediaPath;
        private TgcSkeletalMesh mesh;
        private string selectedAnim;
        private string selectedMesh;
        private bool showAttachment;

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
            return
                "Utiliza el esqueleto genérico BasicHuman provisto por la cátedra para animar varios modelos distintos mediante animación esquelética";
        }

        public override void init()
        {
            //Path para carpeta de texturas de la malla
            mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";

            //Cargar dinamicamente todos los Mesh animados que haya en el directorio
            var dir = new DirectoryInfo(mediaPath);
            var meshFiles = dir.GetFiles("*-TgcSkeletalMesh.xml", SearchOption.TopDirectoryOnly);
            var meshList = new string[meshFiles.Length];
            for (var i = 0; i < meshFiles.Length; i++)
            {
                var name = meshFiles[i].Name.Replace("-TgcSkeletalMesh.xml", "");
                meshList[i] = name;
            }

            //Cargar dinamicamente todas las animaciones que haya en el directorio "Animations"
            var dirAnim = new DirectoryInfo(mediaPath + "Animations\\");
            var animFiles = dirAnim.GetFiles("*-TgcSkeletalAnim.xml", SearchOption.TopDirectoryOnly);
            var animationList = new string[animFiles.Length];
            animationsPath = new string[animFiles.Length];
            for (var i = 0; i < animFiles.Length; i++)
            {
                var name = animFiles[i].Name.Replace("-TgcSkeletalAnim.xml", "");
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
            var animateWithLoop = true;
            GuiController.Instance.Modifiers.addBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para renderizar el esqueleto
            var renderSkeleton = false;
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
        ///     Cargar una nueva malla
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
                var loader = new TgcSkeletalLoader();
                mesh = loader.loadMeshAndAnimationsFromFile(mediaPath + selectedMesh + "-TgcSkeletalMesh.xml", mediaPath,
                    animationsPath);

                //Crear esqueleto a modo Debug
                mesh.buildSkletonMesh();

                //Elegir animacion inicial
                mesh.playAnimation(selectedAnim, true);

                //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
                attachment = new TgcSkeletalBoneAttach();
                var attachmentBox = TgcBox.fromSize(new Vector3(2, 40, 2), Color.Red);
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
            //Ver si cambio la malla
            var meshPath = (string)GuiController.Instance.Modifiers.getValue("mesh");
            changeMesh(meshPath);

            //Ver si cambio la animacion
            var anim = (string)GuiController.Instance.Modifiers.getValue("animation");
            changeAnimation(anim);

            //Ver si rendeizamos el esqueleto
            var renderSkeleton = (bool)GuiController.Instance.Modifiers.getValue("renderSkeleton");

            //Ver si cambio el color
            var selectedColor = (Color)GuiController.Instance.Modifiers.getValue("Color");
            if (currentColor == null || currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Agregar o quitar Attachment
            var showAttachmentFlag = (bool)GuiController.Instance.Modifiers["Attachment"];
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
            mesh.updateAnimation(elapsedTime);

            //Solo malla o esqueleto, depende lo seleccionado
            mesh.RenderSkeleton = renderSkeleton;
            mesh.render();

            //Se puede renderizar todo mucho mas simple (sin esqueleto) de la siguiente forma:
            //mesh.animateAndRender();

            //BoundingBox
            var showBB = (bool)GuiController.Instance.Modifiers["BoundingBox"];
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