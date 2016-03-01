using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Example;
using TGC.Viewer;
using TGC.Viewer.Utils.TgcGeometry;
using TGC.Viewer.Utils.TgcSceneLoader;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 4:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    ///     Muestra como crear una caja 3D que se mueve cuando las flechas del teclado.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial4 : TgcExample
    {
        private const float MOVEMENT_SPEED = 10f;

        private TgcBox box;

        public override string getCategory()
        {
            return "Tutorial";
        }

        public override string getName()
        {
            return "Tutorial 4";
        }

        public override string getDescription()
        {
            return "Muestra como crear una caja 3D que se mueve cuando las flechas del teclado.";
        }

        public override void init()
        {
            //Creamos una caja 3D con textura
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 5, 5);
            var texture =
                TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir +
                                         "MeshCreator\\Textures\\Ladrillo\\streetbricks.jpg");
            box = TgcBox.fromSize(center, size, texture);

            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = GuiController.Instance.D3dInput;

            //Declaramos un vector de movimiento inicializado todo en cero.
            //El movimiento sobre el suelo es sobre el plano XZ.
            //El movimiento en altura es sobre el eje Y.
            //Sobre XZ nos movemos con las flechas del teclado o con las letas WASD.
            //Sobre el eje Y nos movemos con Space y Ctrl.
            var movement = new Vector3(0, 0, 0);

            //Movernos de izquierda a derecha, sobre el eje X.
            if (input.keyDown(Key.Left) || input.keyDown(Key.A))
            {
                movement.X = 1;
            }
            else if (input.keyDown(Key.Right) || input.keyDown(Key.D))
            {
                movement.X = -1;
            }

            //Movernos arriba y abajo, sobre el eje Y.
            if (input.keyDown(Key.Space))
            {
                movement.Y = 1;
            }
            else if (input.keyDown(Key.LeftControl) || input.keyDown(Key.RightControl))
            {
                movement.Y = -1;
            }

            //Movernos adelante y atras, sobre el eje Z.
            if (input.keyDown(Key.Up) || input.keyDown(Key.W))
            {
                movement.Z = -1;
            }
            else if (input.keyDown(Key.Down) || input.keyDown(Key.S))
            {
                movement.Z = 1;
            }

            //Multiplicar movimiento por velocidad y elapsedTime
            movement *= MOVEMENT_SPEED * elapsedTime;

            //Aplicar movimiento
            box.move(movement);

            box.render();
        }

        public override void close()
        {
            box.dispose();
        }
    }
}