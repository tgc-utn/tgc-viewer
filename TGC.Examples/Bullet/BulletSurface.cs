using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Bullet
{
    public class BulletSurface : TGCExampleViewer
    {
        //Fisica 
        private PhysicsGame physicsExample;
        //Vertex buffer que se va a utilizar
        private VertexBuffer vertexBuffer;

        public BulletSurface(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Bullet";
            Name = "Triangle Surface";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\".";
        }

        public override void Init()
        {
            physicsExample = new HelloWorldBullet2();

            //Ideas para generar el terreno para Bullet
            //We are getting a llitle bit crazy xD https://es.wikipedia.org/wiki/Paraboloide
            //Paraboloide Hiperbolico 
            // definicion matematica
            //(x / a) ^ 2 - ( y / b) ^ 2 - z = 0.
            //
            //DirectX
            //(x / a) ^ 2 - ( z / b) ^ 2 - y = 0.

            //Crear vertexBuffer
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, D3DDevice.Instance.Device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            //Cargar informacion de vertices: (X,Y,Z) + Color
            var data = new CustomVertex.PositionColored[3];
            data[0] = new CustomVertex.PositionColored(-1, 0, 0, Color.Red.ToArgb());
            data[1] = new CustomVertex.PositionColored(1, 0, 0, Color.Green.ToArgb());
            data[2] = new CustomVertex.PositionColored(0, 1, 0, Color.Blue.ToArgb());

            //Almacenar informacion en VertexBuffer
            vertexBuffer.SetData(data, 0, LockFlags.None);

            var bulletExampleBase = new BulletExampleWall(MediaDir, ShadersDir, UserVars, new Panel());
            physicsExample.Init(bulletExampleBase);

            //TODO: cuando este terminado el modelo de fisica del ejemplo utilizar lo de abajo
            //physicsExample.Init();
;
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
