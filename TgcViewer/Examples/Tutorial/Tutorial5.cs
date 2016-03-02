using TGC.Core.Example;
using TGC.Core.SceneLoader;
using TGC.Viewer;

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

        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 5";
        }

        public override string getDescription()
        {
            return "Muestra como cargar un modelo 3D.";
        }

        public override void init()
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
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml");

            //De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            mesh = scene.Meshes[0];

            //Hacemos que la cámara esté centrada sobre este modelo.
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            //Dibujar el modelo 3D
            mesh.render();
        }

        public override void close()
        {
            //Liberar memoria del modelo 3D
            mesh.dispose();
        }
    }
}