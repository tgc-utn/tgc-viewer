using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     Ejemplo EjemploModeloAlpha:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Alpha Blending
    ///     # Unidad 3 - Conceptos Básicos de 3D - Alpha Test
    ///     Carga una escena que posee modelos exportados desde 3Ds MAX con Alpha Blending activado.
    ///     Estos modelos poseen texturas PNG-32 con transparencia.
    ///     Los modelos fueron configurados en 3Ds MAX con un mapa de "Opacity" en el "Material Editor"
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploModeloAlpha : TgcExample
    {
        private TgcScene scene;

        public EjemploModeloAlpha(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "AlphaBlending";
            Name = "Modelo con Alpha";
            Description = "Carga modelos que poseen texturas PNG-32 con transparencia.";
        }

        public override void Init()
        {
            /* Cargar ecena que tiene un modelo configurado con AlphaBlending
             * Los modelos fueron exportados en 3Ds MAX con el mapa "Opacity" cargado en el "Material Editor"
             * Entonces el TgcSceneLoader automáticamente hace mesh.AlphaBlendEnable(true);
             */
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml");

            Camara = new TgcRotationalCamera(scene.BoundingBox.calculateBoxCenter(),
                scene.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            scene.renderAll();

            PostRender();
        }

        public override void Dispose()
        {
            scene.disposeAll();
        }
    }
}