﻿using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Examples.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Bullet
{
    public class BulletExample3 : TGCExampleViewer
    {
        private PhysicsGame physicsExample;

        public BulletExample3(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "BulletPhysics";
            Name = "Box and Sphere";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\".";
        }

        public override void Init()
        {
            physicsExample = new HelloWorldBullet2();
            physicsExample.Init(this);

            UserVars.addVar("MeshCount");

            Camara = new TgcRotationalCamera(new TGCVector3(0, 20, 0), 100, Input);
        }

        public override void Update()
        {
            PreUpdate();
            physicsExample.Update();

            UserVars.setValue("MeshCount", physicsExample.GetElements());

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