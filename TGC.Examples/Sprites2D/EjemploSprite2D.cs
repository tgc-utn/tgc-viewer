using Microsoft.DirectX;
using TGC.Core._2D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Viewer;

namespace TGC.Examples.Sprites2D
{
    /// <summary>
    ///     Ejemplo Sprite2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     Muestra como dibujar un Sprite. Un Sprite es una imágen que se dibuja en dos
    ///     dimensiones, arriba de todo la escena 3D.
    ///     Es muy útil para crear menues, íconos, etc.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class EjemploSprite2D : TgcExample
    {
        private TgcBox box;
        private TgcSprite sprite;

        public override string getCategory()
        {
            return "Sprite 2D";
        }

        public override string getName()
        {
            return "Sprite 2D";
        }

        public override string getDescription()
        {
            return "Muestra como dibujar un Sprite en pantalla.";
        }

        public override void init()
        {
            //Crear Sprite
            sprite = new TgcSprite();
            sprite.Texture =
                TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "\\Texturas\\LogoTGC.png");

            //Ubicarlo centrado en la pantalla
            var screenSize = GuiController.Instance.Panel3d.Size;
            var textureSize = sprite.Texture.Size;
            sprite.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width / 2, 0),
                FastMath.Max(screenSize.Height / 2 - textureSize.Height / 2, 0));

            //Modifiers para variar parametros del sprite
            GuiController.Instance.Modifiers.addVertex2f("position", new Vector2(0, 0),
                new Vector2(screenSize.Width, screenSize.Height), sprite.Position);
            GuiController.Instance.Modifiers.addVertex2f("scaling", new Vector2(0, 0), new Vector2(4, 4), sprite.Scaling);
            GuiController.Instance.Modifiers.addFloat("rotation", 0, 360, 0);

            //Creamos un Box3D para que se vea como el Sprite es en 2D y se dibuja siempre arriba de la escena 3D
            box = TgcBox.fromSize(new Vector3(10, 10, 10),
                TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "\\Texturas\\pasto.jpg"));

            //Hacer que la camara se centre en el box3D
            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            //Actualizar valores cargados en modifiers
            sprite.Position = (Vector2)GuiController.Instance.Modifiers["position"];
            sprite.Scaling = (Vector2)GuiController.Instance.Modifiers["scaling"];
            sprite.Rotation = FastMath.ToRad((float)GuiController.Instance.Modifiers["rotation"]);

            //Dibujar box3D. Se deben dibujar primero todos los objetos 3D. Recien al final dibujar los Sprites
            box.render();

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            TgcDrawer2D.Instance.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            sprite.render();

            //Finalizar el dibujado de Sprites
            TgcDrawer2D.Instance.endDrawSprite();
        }

        public override void close()
        {
            sprite.dispose();
            box.dispose();
        }
    }
}