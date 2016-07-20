using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Tutorial
{
    /// <summary>
    ///     Tutorial 4:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Transformaciones
    ///     Muestra como crear una caja 3D que se mueve cuando las flechas del teclado.
    ///     Autor: Matías Leone
    /// </summary>
    public class Tutorial4 : TGCExampleViewer
    {
        private const float MOVEMENT_SPEED = 10f;
        private TgcBox box;

        public Tutorial4(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Tutorial";
            Name = "Tutorial 4";
            Description = "Muestra como crear una caja 3D que se mueve cuando las flechas del teclado.";
        }

        public override void Init()
        {
            //Creamos una caja 3D con textura
            var center = new Vector3(0, -3, 0);
            var size = new Vector3(5, 5, 5);
            var texture = TgcTexture.createTexture(MediaDir + "MeshCreator\\Textures\\Ladrillo\\streetbricks.jpg");
            box = TgcBox.fromSize(center, size, texture);

            //Hacemos que la cámara esté centrada el box.
            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(),
                box.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtenemos acceso al objeto que maneja input de mouse y teclado del framework
            var input = Input;

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
            movement *= MOVEMENT_SPEED * ElapsedTime;

            //Aplicar movimiento
            box.move(movement);

            box.render();

            PostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}