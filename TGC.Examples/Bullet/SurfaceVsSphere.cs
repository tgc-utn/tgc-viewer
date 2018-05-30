using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Examples.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;

namespace TGC.Examples.Bullet
{
    public class SurfaceVsSphere : TGCExampleViewer
    {
        public SurfaceVsSphere(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel) : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Bullet";
            Name = "Triangles vs Sphere";
            Description = "Ejemplo de como poder utilizar el motor de fisica Bullet con \"BulletSharp + TGC.Core\". " +
                "Donde se emplea una esfera para el personaje, un terreno generado por un heighmap con muchos triangulos para que la misma colisione como terreno.";
        }

        //Terreno
        TgcSimpleTerrain terrain;
        //Fisica 
        private TriangleSpherePhysics physicsExample;

        public override void Init()
        {
            terrain = new TgcSimpleTerrain();
            var position = TGCVector3.Empty;
            terrain.loadHeightmap(MediaDir + "Heighmaps\\" + "TerrainTexture2.jpg", 20, 0.5f, position);
            terrain.loadTexture(MediaDir + "Heighmaps\\" + "grass.jpg");

            physicsExample = new TriangleSpherePhysics();
            physicsExample.setTriangleDataVB(terrain.getData());
            physicsExample.Init(MediaDir);

            Camara = new TgcRotationalCamera(new TGCVector3(0, 20, 0), 1000, Input);
        }

        public override void Update()
        {
            PreUpdate();
            physicsExample.Update(Input);
            //UserVars.setValue("Tgccito_Position", physicsExample.getCharacterPosition());
            PostUpdate();
        }

        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            physicsExample.Render();

            terrain.Render();

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        public override void Dispose()
        {
            terrain.Dispose();
            physicsExample.Dispose();
        }
    }
}
