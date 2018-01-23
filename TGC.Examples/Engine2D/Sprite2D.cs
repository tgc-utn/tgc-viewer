using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Engine2D.Spaceship.Core;
using TGC.Examples.Example;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Sprite2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     Muestra como dibujar un Sprite. Un Sprite es una imágen que se dibuja en dos dimensiones, arriba de todo la escena
    ///     3D.
    ///     Muestra tambien como dibujar un Sprite Animado en 2D. Es similar al concepto de un GIF animado. Se tiene una
    ///     textura de Sprite, compuesta por un conjunto de tiles, o frames de animación.
    ///     El Sprite animado va iterando sobre cada frame de animación y lo muestra en 2D.
    ///     Ambos son muy útiles para crear menues, íconos, etc.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class Sprite2D : TGCExampleViewer
    {
        private AnimatedSprite animatedSprite;
        private TGCBox box;
        private Drawer2D drawer2D;
        private CustomSprite sprite;

        public Sprite2D(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "2D";
            Name = "Sprite 2D";
            Description = "Muestra como dibujar un Sprite y un Sprite animado en pantalla.";
        }

        public override void Init()
        {
            drawer2D = new Drawer2D();

            //Crear Sprite
            sprite = new CustomSprite();
            sprite.Bitmap = new CustomBitmap(MediaDir + "\\Texturas\\LogoTGC.png", D3DDevice.Instance.Device);

            //Ubicarlo centrado en la pantalla
            var textureSize = sprite.Bitmap.Size;
            sprite.Position = new TGCVector2(FastMath.Max(D3DDevice.Instance.Width / 2 - textureSize.Width / 2, 0),
                FastMath.Max(D3DDevice.Instance.Height / 2 - textureSize.Height / 2, 0));

            //Crear Sprite animado
            animatedSprite = new AnimatedSprite(MediaDir + "\\Texturas\\Sprites\\Explosion.png", //Textura de 256x256
                new Size(64, 64), //Tamaño de un frame (64x64px en este caso)
                16, //Cantidad de frames, (son 16 de 64x64px)
                10); //Velocidad de animacion, en cuadros x segundo,

            //Ubicarlo centrado en la pantalla
            var textureSizeAnimado = animatedSprite.Bitmap.Size;
            animatedSprite.Position = new TGCVector2(D3DDevice.Instance.Width / 2 - textureSizeAnimado.Width / 2,
                D3DDevice.Instance.Height / 2 - textureSizeAnimado.Height / 2);

            //Modifiers para variar parametros del sprite
            Modifiers.addVertex2f("position", TGCVector2.Zero,
                new TGCVector2(D3DDevice.Instance.Width, D3DDevice.Instance.Height), sprite.Position);
            Modifiers.addVertex2f("scaling", TGCVector2.Zero, new TGCVector2(4, 4), sprite.Scaling);
            Modifiers.addFloat("rotation", 0, 360, 0);

            //Modifiers para variar parametros del sprite
            Modifiers.addFloat("frameRateAnimated", 1, 30, 10);
            Modifiers.addVertex2f("positionAnimated", TGCVector2.Zero,
                new TGCVector2(D3DDevice.Instance.Width, D3DDevice.Instance.Height), animatedSprite.Position);
            Modifiers.addVertex2f("scalingAnimated", TGCVector2.Zero, new TGCVector2(4, 4), animatedSprite.Scaling);
            Modifiers.addFloat("rotationAnimated", 0, 360, 0);

            //Creamos un Box3D para que se vea como el Sprite es en 2D y se dibuja siempre arriba de la escena 3D
            box = TGCBox.fromSize(new TGCVector3(10, 10, 10), TgcTexture.createTexture(MediaDir + "\\Texturas\\pasto.jpg"));
            box.Transform = TGCMatrix.RotationX(FastMath.QUARTER_PI);

            //Hacer que la camara se centre en el box3D
            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(),
                box.BoundingBox.calculateBoxRadius() * 2, Input);
        }

        public override void Update()
        {
            PreUpdate();

            //Actualizar valores cargados en modifiers
            sprite.Position = (TGCVector2)Modifiers["position"];
            sprite.Scaling = (TGCVector2)Modifiers["scaling"];
            sprite.Rotation = FastMath.ToRad((float)Modifiers["rotation"]);

            //Actualizar valores cargados en modifiers
            animatedSprite.setFrameRate((float)Modifiers["frameRateAnimated"]);
            animatedSprite.Position = (TGCVector2)Modifiers["positionAnimated"];
            animatedSprite.Scaling = (TGCVector2)Modifiers["scalingAnimated"];
            animatedSprite.Rotation = FastMath.ToRad((float)Modifiers["rotationAnimated"]);

            //Actualizamos el estado de la animacion y renderizamos
            animatedSprite.update(ElapsedTime);

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Dibujar box3D. Se deben dibujar primero todos los objetos 3D. Recien al final dibujar los Sprites
            box.Render();

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(sprite);
            drawer2D.DrawSprite(animatedSprite);

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

            PostRender();
        }

        public override void Dispose()
        {
            sprite.Dispose();
            animatedSprite.Dispose();
            box.Dispose();
        }
    }
}