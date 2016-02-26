using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TGC.Core.Utils;

namespace Examples.Tutorial
{
    /// <summary>
    /// Tutorial 3:
    /// Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    /// 
    /// Muestra como crear una caja 3D con textura que se traslada y rota en cada cuadro.
    /// 
    /// Autor: Matías Leone
    /// 
    /// </summary>
    public class Tutorial3 : TgcExample
    {

        //Constantes para velocidades de movimiento
        const float ROTATION_SPEED = 1f;
        const float MOVEMENT_SPEED = 5f;

        //Variable direccion de movimiento
        float currentMoveDir = 1f;

        TgcBox box;


        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 3";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D con textura que se traslada y rota en cada cuadro.";
        }


        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Creamos una caja 3D con textura
            Vector3 center = new Vector3(0, -3, 0);
            Vector3 size = new Vector3(5, 5, 5);
            TgcTexture texture = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Textures\\Metal\\cajaMetal.jpg");
            box = TgcBox.fromSize(center, size, texture);


            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //En cada cuadro de render rotamos la caja con cierta velocidad (en radianes)
            //Siempre tenemos que multiplicar las velocidades por el elapsedTime.
            //De esta forma la velocidad de rotacion es independiente de la potencia del CPU.
            //Sino en computadoras con CPU más rápido la caja giraría mas rápido que en computadoras mas lentas.
            box.rotateY(ROTATION_SPEED * elapsedTime);

            //Tambien aplicamos una traslación en Y. Hacemos que la caja se mueva en forma intermitente en el 
            //intervalo [0, 3] de Y. Cuando llega a uno de los límites del intervalo invertimos la dirección
            //del movimiento.
            //Tambien tenemos que multiplicar la velocidad por el elapsedTime
            box.move(0, MOVEMENT_SPEED * currentMoveDir * elapsedTime, 0);
            if (FastMath.Abs(box.Position.Y) > 3f)
            {
                currentMoveDir *= -1;
            }


            box.render();
        }

        public override void close()
        {
            box.dispose();
        }

    }
}
