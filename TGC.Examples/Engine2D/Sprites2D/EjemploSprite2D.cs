using Microsoft.DirectX;
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
    ///     Muestra como dibujar un Sprite. Un Sprite es una im�gen que se dibuja en dos
    ///     dimensiones, arriba de todo la escena 3D.
    ///     Es muy �til para crear menues, �conos, etc.
    ///     Autor: Mat�as Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSprite2D : TGCExampleViewer
    {
        private TgcBox box;
        private CustomSprite sprite;
        private Drawer2D drawer2D;

        public EjemploSprite2D(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "2D";
            Name = "Sprite 2D";
            Description = "Muestra como dibujar un Sprite en pantalla.";
        }

        public override void Init()
        {
            //Crear Sprite
            drawer2D = new Drawer2D();
            sprite = new CustomSprite();
            sprite.Bitmap = new CustomBitmap(MediaDir + "\\Texturas\\LogoTGC.png", D3DDevice.Instance.Device);

            //Ubicarlo centrado en la pantalla
            var textureSize = sprite.Bitmap.Size;
            sprite.Position = new Vector2(FastMath.Max(D3DDevice.Instance.Width / 2 - textureSize.Width / 2, 0),
                FastMath.Max(D3DDevice.Instance.Height / 2 - textureSize.Height / 2, 0));

            //Modifiers para variar parametros del sprite
            Modifiers.addVertex2f("position", new Vector2(0, 0),
                new Vector2(D3DDevice.Instance.Width, D3DDevice.Instance.Height), sprite.Position);
            Modifiers.addVertex2f("scaling", new Vector2(0, 0), new Vector2(4, 4), sprite.Scaling);
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
        }

        public override void Render()
        {
            PreRender();

            //Actualizar valores cargados en modifiers
            sprite.Position = (Vector2)Modifiers["position"];
            sprite.Scaling = (Vector2)Modifiers["scaling"];
            sprite.Rotation = FastMath.ToRad((float)Modifiers["rotation"]);

            //Dibujar box3D. Se deben dibujar primero todos los objetos 3D. Recien al final dibujar los Sprites
            box.render();

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aqu�)
            drawer2D.DrawSprite(sprite);

            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

            PostRender();
        }

        public override void Dispose()
        {
            sprite.Dispose();
            box.dispose();
        }
    }
}