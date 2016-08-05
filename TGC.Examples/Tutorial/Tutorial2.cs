using Microsoft.DirectX;
using TGC.Core.Camara;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Tutorial
{
	/// <summary>
	///     Tutorial 2:
	///     Unidades Involucradas:
	///     # Unidad 3 - Conceptos Básicos de 3D - Mesh
	///     Muestra como cargar un modelo 3D.
	/// 	Muestra como cargar una escena 3D completa.
	///     Autor: Matías Leone
	/// </summary>
	public class Tutorial2 : TGCExampleViewer
    {
        //Variable para el modelo 3D
        private TgcMesh mesh;
		//Variable para la escena 3D
		private TgcScene scene;

        public Tutorial2(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 2";
            Description = "Muestra como cargar un modelo 3D y una escena 3D completa.";
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

            //De toda la escena solo nos interesa guardarnos el primer modelo (el único que hay en este caso).
            mesh = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Hummer\\Hummer-TgcScene.xml").Meshes[0];
			mesh.AutoTransformEnable = true;
			mesh.rotateY(FastMath.QUARTER_PI);
			mesh.move(new Vector3(100, 40, -200));
			//mesh.Transform = Matrix.RotationY(FastMath.QUARTER_PI) * Matrix.Translation(100,40,-200);

			//En este ejemplo no cargamos un solo modelo 3D sino una escena completa, compuesta por varios modelos.
			//El framework posee varias escenas ya hechas en la carpeta TgcViewer\Examples\Media\MeshCreator\Scenes.
			scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Iglesia\\Iglesia-TgcScene.xml");

			//Hacemos que la cámara esté centrada sobre el mesh.
			Camara = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2, Input);
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

			//Dibujar la escena entera
			scene.renderAll();

			PostRender();
        }

        public override void Dispose()
        {
            //Liberar memoria del modelo 3D
            mesh.dispose();

			//Liberar memoria de toda la escena
			scene.disposeAll();
        }
    }
}