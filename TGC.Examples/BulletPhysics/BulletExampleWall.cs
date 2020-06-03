using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Examples.BulletPhysics.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.BulletPhysics
{
    public class BulletExampleWall : TGCExampleViewer
    {
        private PhysicsGame physicsExample;

        public BulletExampleWall(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "BulletPhysics";
            Name = "Wall vs Balls";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\". Ejecutar con Update constante activado en el menu.";
        }

        public override void Init()
        {
            physicsExample = new WallBullet();
            physicsExample.Init(this);
            Camera = new TgcRotationalCamera(new TGCVector3(0, 150, 0), 500, Input);
        }

        public override void Update()
        {
            physicsExample.Update(ElapsedTime, TimeBetweenUpdates);
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            physicsExample.Render();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        public override void Dispose()
        {
            physicsExample.Dispose();
        }
    }
}