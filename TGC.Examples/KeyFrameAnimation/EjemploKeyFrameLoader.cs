using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.KeyFrameLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

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

        public EjemploKeyFrameLoader(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "KeyFrameAnimation";
            Name = "MeshLoader";
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
            Modifiers.addInterval("animation", animationList, 0);
            selectedAnim = animationList[0];

            //Modifier para especificar si la animación se anima con loop
            animateWithLoop = true;
            Modifiers.addBoolean("loop", "Loop anim:", animateWithLoop);

            //Modifier para color
            currentColor = Color.White;
            Modifiers.addColor("Color", currentColor);

            //Modifier para BoundingBox
            Modifiers.addBoolean("BoundingBox", "BoundingBox:", false);

            //Elegir animacion Caminando
            mesh.playAnimation(selectedAnim, true);

            //Configurar camara
            Camara = new TgcRotationalCamera(new Vector3(0, 70, 0), 200);
        }

        public override void Update()
        {
            base.PreUpdate();
        }

        public override void Render()
        {
            base.PreRender();
            

            //Ver si cambio la animacion
            var anim = (string)Modifiers.getValue("animation");
            if (!anim.Equals(selectedAnim))
            {
                //Ver si animamos con o sin loop
                animateWithLoop = (bool)Modifiers.getValue("loop");

                //Cargar nueva animacion elegida
                selectedAnim = anim;
                mesh.playAnimation(selectedAnim, animateWithLoop);
            }

            //Ver si cambio el color
            var selectedColor = (Color)Modifiers.getValue("Color");
            if (currentColor == null || currentColor != selectedColor)
            {
                currentColor = selectedColor;
                mesh.setColor(currentColor);
            }

            //Animar y Renderizar.
            //Este metodo actualiza la animacion actual segun el tiempo transcurrido y renderiza la malla resultante
            mesh.animateAndRender(ElapsedTime);

            //BoundingBox
            var showBB = (bool)Modifiers["BoundingBox"];
            if (showBB)
            {
                mesh.BoundingBox.render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            

            mesh.dispose();
        }
    }
}