using TGC.Core.Example;

namespace TGC.Examples.Engine2D
{
    /// <summary>
    ///     Ejemplo Ejemplo2D:
    ///     Unidades Involucradas:
    ///     # Unidad 2 - Conceptos Avanzados de 2D - Animación, Sprites, Primitivas
    ///     MiniJuego de nave con asteroides.
    ///     Muestra como utilizar las herramientas de dibujado 2D de DirectX.
    ///     Muestra como utilizar eventos del mouse y detección de colisiones 2D.
    ///     Autor: Leandro Barbagallo, Matías Leone
    /// </summary>
    public class Ejemplo2D : TgcExample
    {
        private GameManager gameManager;

        public override string getCategory()
        {
            return "2D";
        }

        public override string getName()
        {
            return "Ejemplo 2D";
        }

        public override string getDescription()
        {
            return "Ejemplo 2D utilizando Sprites. Hacer clic con el mouse para mover la nave.";
        }

        public override void init()
        {
            gameManager = GameManager.Instance;

            gameManager.Init(ExampleDir);
        }

        public override void close()
        {
        }

        public override void render(float elapsedTime)
        {
            gameManager.Update(elapsedTime);

            gameManager.Render(elapsedTime);
        }
    }
}