using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 6:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como cargar una escena 3D completa.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial6 : TgcExample
    {
        //Variable para la escena 3D
        private TgcScene scene;

        public Tutorial6(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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
            Camara = new TgcRotationalCamera(scene.BoundingBox.calculateBoxCenter(), scene.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            //Dibujar la escena entera
            scene.renderAll();

            //Hacer renderAll() es el equivalente a recorrer todos sus modelos internos y dibujar cada uno:
            /*
            foreach (TgcMesh mesh in scene.Meshes)
            {
                mesh.render();
            }
            */

            helperPostRender();
        }

        public override void Close()
        {
            base.Close();

            //Liberar memoria de toda la escena
            scene.disposeAll();
        }
    }
}