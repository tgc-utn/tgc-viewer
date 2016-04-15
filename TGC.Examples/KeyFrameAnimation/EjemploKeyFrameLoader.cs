using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Example;
using TGC.Core.KeyFrameLoader;
using TGC.Util;

namespace TGC.Examples.KeyFrameAnimation
{
    /// <summary>
    ///     Ejemplo EjemploKeyFrameLoader:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animación - KeyFrame Animation
    ///     Carga un personaje animado con el método de KeyFrameAnimation, utilizando
    ///     la herramienta TgcKeyFrameLoader.
    ///     Es una alternativa de animación a la herramienta TgcSkeletalLoader
    ///     Se crea un Modifier para que el usuario puede alternar la animación que se muestra.
    ///     La herramienta TgcKeyFrameLoader crea una malla del tipo TgcKeyFrameMesh.
    ///     Una malla TgcKeyFrameMesh puede tener una o varias animaciones.
    ///     Cada animacion es un archivo XML diferente.
    ///     La estructura general de la malla tambien es un XML diferente.
    ///     Todos los XML son del formato TGC.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class EjemploKeyFrameLoader : TgcExample
    {
        private bool animateWithLoop;
        private Color currentColor;
        private TgcKeyFrameMesh mesh;
        private string selectedAnim;

        public override string getCategory()
        {
            return "KeyFrameAnimation";
        }

        public override string getName()
        {
            return "MeshLoader";
        }

        public override string getDescription()
        {
            return "Muestra como cargar un personaje con animaciones, en formato TGC";
        }

        public override void init()
        {
            //Paths para archivo XML de la malla
            var pathMesh = GuiController.Instance.ExamplesMediaDir +
                           "KeyframeAnimations\\Robot\\Robot-TgcKeyFrameMesh.xml";

            //Path para carpeta de texturas de la malla
            var mediaPath = GuiController.Instance.ExamplesMediaDir + "KeyframeAnimations\\Robot\\";

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
                animationsPath[i] = mediaPath + animationList[i] + "-TgcKeyFrameAnim.xml";
            }

            //Cargar mesh y animaciones
            var loader = new TgcKeyFrameLoader();
            mesh = loader.loadMeshAndAnimationsFromFile(pathMesh, mediaPath, animationsPath);

            //Agregar combo para elegir animacion
            GuiController.Instance.Modifiers.addInterval("animation", animationList, 0);
            selectedAnim = animationList[0];

            //Modifier para especificar si la animación se anima con loop
            animateWithLoop = true;
            GuiController.Instance.Modifiers.addBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para color
            currentColor = Color.White;
            GuiController.Instance.Modifiers.addColor("Color", currentColor);

            //Modifier para BoundingBox
            GuiController.Instance.Modifiers.addBoolean("BoundingBox", "BoundingBox:", false);

            //Elegir animacion Caminando
            mesh.playAnimation(selectedAnim, true);

            //Configurar camara
            GuiController.Instance.RotCamera.setCamera(new Vector3(0, 70, 0), 200);
        }

        public override void render(float elapsedTime)
        {
            //Ver si cambio la animacion
            var anim = (string)GuiController.Instance.Modifiers.getValue("animation");
            if (!anim.Equals(selectedAnim))
            {
                //Ver si animamos con o sin loop
                animateWithLoop = (bool)GuiController.Instance.Modifiers.getValue("loop");

                //Cargar nueva animacion elegida
                selectedAnim = anim;
                mesh.playAnimation(selectedAnim, animateWithLoop);
            }

            //Ver si cambio el color
            var selectedColor = (Color)GuiController.Instance.Modifiers.getValue("Color");
            if (currentColor == null || currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Animar y Renderizar.
            //Este metodo actualiza la animacion actual segun el tiempo transcurrido y renderiza la malla resultante
            mesh.animateAndRender(elapsedTime);

            //BoundingBox
            var showBB = (bool)GuiController.Instance.Modifiers["BoundingBox"];
            if (showBB)
            {
                mesh.BoundingBox.render();
            }
        }

        public override void close()
        {
            mesh.dispose();
        }
    }
}