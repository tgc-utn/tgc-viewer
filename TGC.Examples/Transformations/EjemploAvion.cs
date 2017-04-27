using Microsoft.DirectX.DirectInput;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.Transformations
{
    public class EjemploAvion : TGCExampleViewer
    {
        private const float VELOCIDAD_ANGULAR = 1.1f;
        private readonly TGCVector3 lengthAvion = new TGCVector3(10f, 2f, 2f);
        private readonly TGCVector3 lengthHelice = new TGCVector3(1f, 5f, 1f);

        private TGCBox box;
        private float ang = 0;
        private float pos = 0;
        private TGCMatrix escalaAvion;
        private TGCMatrix escalaHelice;
        private TGCMatrix transformacionAvion;
        private TGCMatrix transformacionHelice;
        private float angHelice = 0;

        public EjemploAvion(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Transformations";
            Name = "Avion parcial";
            Description =
                "Ejemplo Avion.";
        }

        public override void Init()
        {
            var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Metal\\floor1.jpg");
            var center = new TGCVector3(0, 0, 0);
            var size = new TGCVector3(1f, 1f, 1f);
            box = TGCBox.fromSize(center, size, texture);
            //Por defecto se deshabilito esto, cada uno debe implementar su modelo de transformaciones.
            //box.AutoTransformEnable = false;
            box.Transform = TGCMatrix.Identity;
            Camara = new TgcRotationalCamera(new TGCVector3(0f, 1.5f, 0f), 20f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            // Los movimientos de teclado no validan que la mesh se atraviecen, solo modifican el angulo o traslacion.
            if (Input.keyDown(Key.W))
            {
                ang += VELOCIDAD_ANGULAR * ElapsedTime;
            }
            else if (Input.keyDown(Key.S))
            {
                ang -= VELOCIDAD_ANGULAR * ElapsedTime;
            }
            if (Input.keyDown(Key.A))
            {
                pos += 5f * ElapsedTime;
            }
            else if (Input.keyDown(Key.D))
            {
                pos -= 5f * ElapsedTime;
            }
            angHelice += ElapsedTime * VELOCIDAD_ANGULAR;

            escalaAvion = TGCMatrix.Scaling(lengthAvion);

            escalaHelice = TGCMatrix.Scaling(lengthHelice);

            var T1 = TGCMatrix.Translation(pos, pos, 0);
            var R1 = TGCMatrix.RotationZ(ang);
            transformacionAvion = R1 * T1;

            var T2 = TGCMatrix.Translation(lengthAvion.X / 2 + lengthHelice.X / 2, 0, 0);

            var R3 = TGCMatrix.RotationX(angHelice);

            transformacionHelice = R3 * T2 * transformacionAvion;
        }

        public override void Render()
        {
            PreRender();

            // Primero asignamos la transformacion de la base, esta solo se escala.
            box.Transform = escalaAvion * transformacionAvion;
            box.Render();

            // Asignamos la transformacion del brazo, se escala ya que estamos utilizando siempre la misma caja
            // y se aplica la transformacion calculada en update.
            box.Transform = escalaHelice * transformacionHelice;
            box.Render();

            PostRender();
        }

        public override void Dispose()
        {
            box.Dispose();
        }
    }
}