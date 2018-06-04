using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Examples.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Bullet
{
    public class BulletExampleWall : TGCExampleViewer
    {
        private PhysicsGame physicsExample;

        public BulletExampleWall(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Bullet";
            Name = "Wall vs Balls";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\".";
        }

        public override void Init()
        {
            physicsExample = new WallBullet();
            //physicsExample = new HelloWorldBullet2();
            //physicsExample = new WallBullet();

            physicsExample.Init(this);
            Camara = new TgcRotationalCamera(new TGCVector3(0, 20, 0), 100, Input);
        }

        public override void Update()
        {
            PreUpdate();           
            physicsExample.Update();
            PostUpdate();
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