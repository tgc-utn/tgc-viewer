using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Sprites2D
{
    /// <summary>
    /// Ejemplo Sprite2D:
    /// Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Transformaciones
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animación 2D
    /// 
    /// Muestra como dibujar un Sprite Animado en 2D. 
    /// Es similar al concepto de un GIF animado. Se tiene una textura de Sprite, compuesta
    /// por un conjunto de tiles, o frames de animación.
    /// El Sprite animado va iterando sobre cada frame de animación y lo muestra en 2D.
    /// Es muy útil para crear menues, íconos, etc.
    /// 
    /// 
    /// Autor: Matías Leone, Leandro Barbagallo
    /// 
    /// </summary>
    public class EjemploAnimatedSprite : TgcExample
    {

        TgcAnimatedSprite animatedSprite;
        TgcBox box;


        public override string getCategory()
        {
            return "Sprite 2D";
        }

        public override string getName()
        {
            return "Sprite Animado";
        }

        public override string getDescription()
        {
            return "Muestra como dibujar un Sprite Animado en 2D";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Crear Sprite animado
            animatedSprite = new TgcAnimatedSprite(
                GuiController.Instance.ExamplesMediaDir + "\\Texturas\\Sprites\\Explosion.png", //Textura de 256x256
                new Size(64, 64), //Tamaño de un frame (64x64px en este caso)
                16, //Cantidad de frames, (son 16 de 64x64px)
                10 //Velocidad de animacion, en cuadros x segundo
                );

            //Ubicarlo centrado en la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = animatedSprite.Sprite.Texture.Size;
            animatedSprite.Position = new Vector2(screenSize.Width / 2 - textureSize.Width / 2, screenSize.Height / 2 - textureSize.Height / 2);
            
            //Modifiers para variar parametros del sprite
            GuiController.Instance.Modifiers.addFloat("frameRate", 1, 30, 10);
            GuiController.Instance.Modifiers.addVertex2f("position", new Vector2(0, 0), new Vector2(screenSize.Width, screenSize.Height), animatedSprite.Position);
            GuiController.Instance.Modifiers.addVertex2f("scaling", new Vector2(0, 0), new Vector2(4, 4), animatedSprite.Scaling);
            GuiController.Instance.Modifiers.addFloat("rotation", 0, 360, 0);


            //Creamos un Box3D para que se vea como el Sprite es en 2D y se dibuja siempre arriba de la escena 3D
            box = TgcBox.fromSize(new Vector3(10, 10, 10), TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "\\Texturas\\pasto.jpg"));

            //Hacer que la camara se centre en el box3D
            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;


            //Actualizar valores cargados en modifiers
            animatedSprite.setFrameRate((float)GuiController.Instance.Modifiers["frameRate"]);
            animatedSprite.Position = (Vector2)GuiController.Instance.Modifiers["position"];
            animatedSprite.Scaling = (Vector2)GuiController.Instance.Modifiers["scaling"];
            animatedSprite.Rotation = FastMath.ToRad((float)GuiController.Instance.Modifiers["rotation"]);


            //Dibujar box3D. Se deben dibujar primero todos los objetos 3D. Recien al final dibujar los Sprites
            box.render();


            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            //Actualizamos el estado de la animacion y renderizamos
            animatedSprite.updateAndRender();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

        }

        public override void close()
        {
            animatedSprite.dispose();
            box.dispose();
        }

    }
}
