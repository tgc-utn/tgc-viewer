using System.Windows.Forms;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.MeshExamples
{
    /// <summary>
    ///     Ejemplo ModeloConAlpha:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Alpha Blending
    ///     # Unidad 3 - Conceptos Basicos de 3D - Alpha Test
    ///     Carga una escena que posee modelos exportados desde 3Ds MAX con Alpha Blending activado.
    ///     Estos modelos poseen texturas PNG-32 con transparencia.
    ///     Los modelos fueron configurados en 3Ds MAX con un mapa de "Opacity" en el "Material Editor"
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class ModeloConAlpha : TGCExampleViewer
    {
        private TGCBooleanModifier alphaModifier;

        private TgcScene scene;

        public ModeloConAlpha(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Mesh Examples";
            Name = "Modelo con Alpha";
            Description = "Carga modelos que poseen texturas PNG-32 con transparencia.";
        }

        public override void Init()
        {
            /* Cargar ecena que tiene un modelo configurado con AlphaBlending
             * Los modelos fueron exportados en 3Ds MAX con el mapa "Opacity" cargado en el "Material Editor"
             * Entonces el TgcSceneLoader automaticamente hace mesh.AlphaBlendEnable(true);
             */
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml");

            //Modifier para activar o desactivar el alpha
            alphaModifier = AddBoolean("alpha", "Alpha blend", true);

            Camera = new TgcRotationalCamera(scene.BoundingBox.calculateBoxCenter(), scene.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            var alpha = alphaModifier.Value;

            foreach (var mesh in scene.Meshes)
            {
                mesh.AlphaBlendEnable = alpha;
            }
        }

        public override void Render()
        {
            PreRender();

            scene.RenderAll();

            PostRender();
        }

        public override void Dispose()
        {
            scene.DisposeAll();
        }
    }
}