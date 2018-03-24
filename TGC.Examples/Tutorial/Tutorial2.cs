using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 2:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos B�sicos de 3D - Mesh
    ///     Muestra como cargar un modelo 3D.
    /// 	Muestra como cargar una escena 3D completa.
    ///     Autor: Mat�as Leone
    /// </summary>
    public class Tutorial2 : TGCExampleViewer
    {
        //Variable para el modelo 3D
        private TgcMesh mesh;

        //Variable para la escena 3D
        private TgcScene scene;

        public Tutorial2(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Tutorial";
            Name = "Tutorial 2";
            Description = "Muestra como cargar un modelo 3D y una escena 3D completa.";
        }

        public override void Init()
        {
            //El framework posee la clase TgcSceneLoader que permite cargar modelos 3D.
            //Estos modelos 3D est�n almacenados en un archivo XML llamado TgcScene.xml.
            //Este archivo es un formato a medida hecho para el framework. Y puede ser creado desde herramientas de
            //dise�o como 3Ds MAX (exportando a traves de un plugin) o con el editor MeshCreator que viene con el framework.
            //El framework viene con varios modelos 3D incluidos en la carpeta: TgcViewer\Examples\Media\MeshCreator\Meshes.
            //El formato especifica una escena, representada por la clase TgcScene. Una escena puede estar compuesta por varios
            //modelos 3D. Cada modelo se representa con la clase TgcMesh.
            //En este ejemplo vamos a cargar una escena con un �nico modelo.
            var loader = new TgcSceneLoader();

            //De toda la escena solo nos interesa guardarnos el primer modelo (el �nico que hay en este caso).
            mesh = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];
            mesh.AutoTransform = true;
            mesh.RotateY(FastMath.QUARTER_PI);
            mesh.Move(new TGCVector3(100, 40, -200));
            //mesh.Transform = TGCMatrix.RotationY(FastMath.QUARTER_PI) * TGCMatrix.Translation(100,40,-200);

            //En este ejemplo no cargamos un solo modelo 3D sino una escena completa, compuesta por varios modelos.
            //El framework posee varias escenas ya hechas en la carpeta TgcViewer\Examples\Media\MeshCreator\Scenes.
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Iglesia\\Iglesia-TgcScene.xml");

            //Hacemos que la c�mara est� centrada sobre el mesh.
            Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Dibujar el modelo 3D
            mesh.Render();

            //Dibujar la escena entera
            scene.RenderAll();

            PostRender();
        }

        public override void Dispose()
        {
            //Liberar memoria del modelo 3D
            mesh.Dispose();

            //Liberar memoria de toda la escena
            scene.DisposeAll();
        }
    }
}