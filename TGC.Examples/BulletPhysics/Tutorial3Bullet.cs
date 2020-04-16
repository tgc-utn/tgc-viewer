using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Examples.BulletPhysics.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.BulletPhysics
{
    /// <summary>
    ///     Tutorial 3:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Basicos de 3D - Mesh
    /// 	# Unidad 6 - Detección de Colisiones - Bounding Box.
    ///     Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos.
    ///     Autor: Leandro Javier Laino
    /// </summary>
    public class Tutorial3Bullet : TGCExampleViewer
    {
        private const float MOVEMENT_SPEED = 200f;
        private TgcThirdPersonCamera camaraInterna;
        private TgcScene scene;

        //Fisica
        private CubePhysic physicsExample;

        public Tutorial3Bullet(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Tutorial";
            Name = "Tutorial 3 con Bullet";
            Description = "Muestra como cargar una escena 3D y como mover un modelo dentra de ella con el teclado evitando chocar con el resto de los objetos." +
                          "Ejecutar con Update constante activado en el menu.";
        }

        public override void Init()
        {
            //En este ejemplo primero cargamos una escena 3D entera.
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Scenes\\Ciudad\\Ciudad-TgcScene.xml");

            physicsExample = new CubePhysic();
            //physicsExample.setHummer(mainMesh);
            scene.Meshes[0].Position = TGCVector3.Empty;
            physicsExample.setBuildings(scene.Meshes);
            physicsExample.Init(MediaDir);

            //Vamos a utilizar la camara en 3ra persona para que siga al objeto principal a medida que se mueve
            camaraInterna = new TgcThirdPersonCamera(physicsExample.getPositionHummer(), 250, 375);
            Camera = camaraInterna;

            UserVars.addVar("HummerPositionX");
            UserVars.addVar("HummerPositionY");
            UserVars.addVar("HummerPositionZ");
            UserVars.addVar("HummerBodyPositionX");
            UserVars.addVar("HummerBodyPositionY");
            UserVars.addVar("HummerBodyPositionZ");
        }

        public override void Update()
        {
            physicsExample.Update(Input, LastUpdateTime, TimeBetweenFrames);
            UserVars.setValue("HummerPositionX", physicsExample.getHummer().Position.X);
            UserVars.setValue("HummerPositionY", physicsExample.getHummer().Position.Y);
            UserVars.setValue("HummerPositionZ", physicsExample.getHummer().Position.Z);
            UserVars.setValue("HummerBodyPositionX", physicsExample.getBodyPos().X);
            UserVars.setValue("HummerBodyPositionY", physicsExample.getBodyPos().Y);
            UserVars.setValue("HummerBodyPositionZ", physicsExample.getBodyPos().Z);

            camaraInterna.Target = physicsExample.getHummer().Position;
        }

        public override void Render()
        {
            PreRender();

            physicsExample.Render(ElapsedTime);

            PostRender();
        }

        public override void Dispose()
        {
            physicsExample.Dispose();
        }
    }
}