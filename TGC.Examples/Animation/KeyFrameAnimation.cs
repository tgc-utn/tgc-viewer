using System.Drawing;
using System.Windows.Forms;
using TGC.Core.KeyFrameLoader;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.Animation
{
    /// <summary>
    ///     Ejemplo KeyFrameAnimation:
    ///     Unidades Involucradas:
    ///     # Unidad 5 - Animacion - KeyFrame Animation
    ///     Carga un personaje animado con el metodo de KeyFrameAnimation, utilizando
    ///     la herramienta TgcKeyFrameLoader.
    ///     Es una alternativa de animacion a la herramienta TgcSkeletalLoader
    ///     Se crea un Modifier para que el usuario puede alternar la animacion que se muestra.
    ///     La herramienta TgcKeyFrameLoader crea una malla del tipo TgcKeyFrameMesh.
    ///     Una malla TgcKeyFrameMesh puede tener una o varias animaciones.
    ///     Cada animacion es un archivo XML diferente.
    ///     La estructura general de la malla tambien es un XML diferente.
    ///     Todos los XML son del formato TGC.
    ///     Autor: Leandro Barbagallo, Matias Leone
    /// </summary>
    public class KeyFrameAnimation : TGCExampleViewer
    {
        private TGCIntervalModifier animationModifier;
        private TGCBooleanModifier boundingBoxModifier;
        private TGCColorModifier colorModifier;
        private TGCBooleanModifier loopModifier;

        private bool animateWithLoop;
        private Color currentColor;
        private TgcKeyFrameMesh mesh;
        private string selectedAnim;

        public KeyFrameAnimation(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Animation";
            Name = "KeyFrame Animation";
            Description = "Muestra como cargar un personaje con animaciones, en formato TGC.";
        }

        public override void Init()
        {
            //Paths para archivo XML de la malla
            var pathMesh = MediaDir + "KeyframeAnimations\\Robot\\Robot-TgcKeyFrameMesh.xml";

            //Path para carpeta de texturas de la malla
            var mediaPath = MediaDir + "KeyframeAnimations\\Robot\\";

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
            selectedAnim = animationList[0];
            animationModifier = AddInterval("animation", animationList, 0);

            //Modifier para especificar si la animacion se anima con loop
            animateWithLoop = true;
            loopModifier = AddBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para color
            currentColor = Color.White;
            colorModifier = AddColor("Color", currentColor);

            //Modifier para BoundingBox
            boundingBoxModifier = AddBoolean("BoundingBox", "BoundingBox:", false);

            //Elegir animacion Caminando
            mesh.playAnimation(selectedAnim, true);

            //Configurar camara
            Camara = new TgcRotationalCamera(new TGCVector3(0, 70, 0), 200, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Ver si cambio la animacion
            var anim = animationModifier.Value.ToString();
            if (!anim.Equals(selectedAnim))
            {
                //Ver si animamos con o sin loop
                animateWithLoop = loopModifier.Value;

                //Cargar nueva animacion elegida
                selectedAnim = anim;
                mesh.playAnimation(selectedAnim, animateWithLoop);
            }

            //Ver si cambio el color
            var selectedColor = colorModifier.Value;
            if (currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Animar y Renderizar.
            //Este metodo actualiza la animacion actual segun el tiempo transcurrido y renderiza la malla resultante
            mesh.animateAndRender(ElapsedTime);

            //BoundingBox
            var showBB = boundingBoxModifier.Value;
            if (showBB)
            {
                mesh.BoundingBox.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
        }
    }
}