using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 6:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como cargar una escena 3D completa.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial6 : TGCExampleViewer
    {
        //Variable para la escena 3D
        private TgcScene scene;

        public Tutorial6(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 6";
            Description = "Muestra como cargar una escena 3D completa.";
        }

        public override void Init()
        {
            //En este ejemplo no cargamos un solo modelo 3D sino una escena completa, compuesta por varios modelos.
            //El framework posee varias escenas ya hechas en la carpeta TgcViewer\Examples\Media\MeshCreator\Scenes.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Iglesia\\Iglesia-TgcScene.xml");

            //Hacemos que la cámara esté centrada sobre la escena
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

            //Dibujar la escena entera
            scene.renderAll();

            //Hacer renderAll() es el equivalente a recorrer todos sus modelos internos y dibujar cada uno:
            /*
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.render();
            }
            */

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar memoria de toda la escena
            scene.disposeAll();
        }
    }
}