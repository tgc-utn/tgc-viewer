using Microsoft.DirectX;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 3:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    ///     Muestra como crear una caja 3D con textura que se traslada y rota en cada cuadro.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial3 : TgcExample
    {
        //Constantes para velocidades de movimiento
        private const float ROTATION_SPEED = 1f;

        private const float MOVEMENT_SPEED = 5f;
        private TgcBox box;

        //Variable direccion de movimiento
        private float currentMoveDir = 1f;

        public Tutorial3(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Tutorial";
            Name = "Tutorial 3";
            Description = "Muestra como crear una caja 3D con textura que se traslada y rota en cada cuadro.";
        }

        public override void Init()
        {
            //Creamos una caja 3D con textura
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 5, 5);
            var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Metal\\cajaMetal.jpg");
            box = TgcBox.fromSize(center, size, texture);

            //Hacemos que la cámara esté centrada sobre el box.
            ((TgcRotationalCamera)Camara).targetObject(box.BoundingBox);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            //En cada cuadro de render rotamos la caja con cierta velocidad (en radianes)
            //Siempre tenemos que multiplicar las velocidades por el elapsedTime.
            //De esta forma la velocidad de rotacion es independiente de la potencia del CPU.
            //Sino en computadoras con CPU más rápido la caja giraría mas rápido que en computadoras mas lentas.
            box.rotateY(ROTATION_SPEED * ElapsedTime);

            //Tambien aplicamos una traslación en Y. Hacemos que la caja se mueva en forma intermitente en el
            //intervalo [0, 3] de Y. Cuando llega a uno de los límites del intervalo invertimos la dirección
            //del movimiento.
            //Tambien tenemos que multiplicar la velocidad por el elapsedTime
            box.move(0, MOVEMENT_SPEED * currentMoveDir * ElapsedTime, 0);
            if (FastMath.Abs(box.Position.Y) > 3f)
            {
                currentMoveDir *= -1;
            }

            box.render();

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            box.dispose();
        }
    }
}