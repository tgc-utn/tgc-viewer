using TgcViewer;
using TGC.Core.Example;

namespace Examples
{
    /// <summary>
    ///     Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.
    /// </summary>
    public class EjemploEnBlanco : TgcExample
    {
        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Ejemplo en Blanco";
        }

        public override string getDescription()
        {
            return "Ejemplo en Blanco. Ideal para copiar y pegar cuando queres empezar a hacer tu propio ejemplo.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;
        }

        public override void close()
        {
        }
    }
}