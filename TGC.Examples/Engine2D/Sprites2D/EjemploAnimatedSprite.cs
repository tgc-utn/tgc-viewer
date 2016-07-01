using Microsoft.DirectX;
using System.Drawing;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Engine2D;
using TGC.Examples.Engine2D.Core;
using TGC.Examples.Example;

namespace TGC.Examples.Sprites2D
{
    /// <summary>
    ///     Ejemplo Sprite2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animaci�n 2D
    ///     Muestra como dibujar un Sprite Animado en 2D.
    ///     Es similar al concepto de un GIF animado. Se tiene una textura de Sprite, compuesta
    ///     por un conjunto de tiles, o frames de animaci�n.
    ///     El Sprite animado va iterando sobre cada frame de animaci�n y lo muestra en 2D.
    ///     Es muy �til para crear menues, �conos, etc.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploAnimatedSprite : TGCExampleViewer
    {
        private AnimatedSprite animatedSprite;
        private TgcBox box;

        public Drawer2D Drawer2D { get; private set; }

        public EjemploAnimatedSprite(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "2D";
            Name = "Sprite Animado";
            Description = "Muestra como dibujar un Sprite Animado en 2D.";
        }

        public override void Init()
        {
            Drawer2D = new Drawer2D();

            //Crear Sprite animado
            animatedSprite = new AnimatedSprite(MediaDir + "\\Texturas\\Sprites\\Explosion.png", //Textura de 256x256
                new Size(64, 64), //Tama�o de un frame (64x64px en este caso)
                16, //Cantidad de frames, (son 16 de 64x64px)
                10); //Velocidad de animacion, en cuadros x segundo,


            //Ubicarlo centrado en la pantalla
            var textureSize = animatedSprite.Bitmap.Size;
            animatedSprite.Position = new Vector2(D3DDevice.Instance.Width / 2 - textureSize.Width / 2,
                D3DDevice.Instance.Height / 2 - textureSize.Height / 2);

            //Modifiers para variar parametros del sprite
            Modifiers.addFloat("frameRate", 1, 30, 10);
            Modifiers.addVertex2f("position", new Vector2(0, 0),
                new Vector2(D3DDevice.Instance.Width, D3DDevice.Instance.Height), animatedSprite.Position);
            Modifiers.addVertex2f("scaling", new Vector2(0, 0), new Vector2(4, 4), animatedSprite.Scaling);
            Modifiers.addFloat("rotation", 0, 360, 0);

            //Creamos un Box3D para que se vea como el Sprite es en 2D y se dibuja siempre arriba de la escena 3D
            box = TgcBox.fromSize(new Vector3(10, 10, 10), TgcTexture.createTexture(MediaDir + "\\Texturas\\pasto.jpg"));

            //Hacer que la camara se centre en el box3D
            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(),
                box.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            PreUpdate();
            //Actualizamos el estado de la animacion y renderizamos
            animatedSprite.update(ElapsedTime);
        }

        public override void Render()
        {
            PreRender();

            //Actualizar valores cargados en modifiers
            animatedSprite.setFrameRate((float)Modifiers["frameRate"]);
            animatedSprite.Position = (Vector2)Modifiers["position"];
            animatedSprite.Scaling = (Vector2)Modifiers["scaling"];
            animatedSprite.Rotation = FastMath.ToRad((float)Modifiers["rotation"]);

            //Dibujar box3D. Se deben dibujar primero todos los objetos 3D. Recien al final dibujar los Sprites
            box.render();

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            Drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aqu�)
            Drawer2D.DrawSprite(animatedSprite);

            //Finalizar el dibujado de Sprites
            Drawer2D.EndDrawSprite();

            PostRender();
        }

        public override void Dispose()
        {
            animatedSprite.Dispose();
            box.dispose();
        }
    }
}