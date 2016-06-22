using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 5:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
    ///     Muestra como cargar un modelo 3D.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial5 : TgcExample
    {
        //Variable para el modelo 3D
        private TgcMesh mesh;

        public Tutorial5(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Tutorial";
            Name = "Tutorial 5";
            Description = "Muestra como cargar un modelo 3D.";
        }

        public override void Init()
        {
            //El framework posee la clase TgcSceneLoader que permite cargar modelos 3D.
            //Estos modelos 3D están almacenados en un archivo XML llamado TgcScene.xml.
            //Este archivo es un formato a medida hecho para el framework. Y puede ser creado desde herramientas de
            //diseño como 3Ds MAX (exportando a traves de un plugin) o con el editor MeshCreator que viene con el framework.
            //El framework viene con varios modelos 3D incluidos en la carpeta: TgcViewer\Examples\Media\MeshCreator\Meshes.
            //El formato especifica una escena, representada por la clase TgcScene. Una escena puede estar compuesta por varios
            //modelos 3D. Cada modelo se representa con la clase TgcMesh.
            //En este ejemplo vamos a cargar una escena con un único modelo.
            var loader = new TgcSceneLoader();
            var scene =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            mesh = scene.Meshes[0];

            //Hacemos que la cámara esté centrada sobre el mesh.
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Dibujar el modelo 3D
            mesh.render();

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar memoria del modelo 3D
            mesh.dispose();
        }
    }
}