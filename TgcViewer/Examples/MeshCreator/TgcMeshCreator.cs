using System.Drawing;
using TGC.Core.Example;
using TGC.Viewer;

namespace TGC.Examples.MeshCreator
{
    /// <summary>
    ///     Ejemplo TgcMeshCreator:
    ///     Unidades Involucradas:
    ///     # Unidad 3 - Conceptos Básicos de 3D - Mesh, Transformaciones, GameEngine
    ///     # Unidad 6 - Detección de Colisiones - BoundingBox, Picking
    ///     Herramienta para crear modelos 3D en base a primitivas simples.
    ///     Permite luego exportarlos a un un XML de formato TgcScene.
    ///     El ejemplo crea su propio Modifier con todos los controles visuales de .NET que necesita.
    ///     Autor: Matías Leone, Leandro Barbagallo
    /// </summary>
    public class TgcMeshCreator : TgcExample
    {
        private MeshCreatorModifier modifier;

        public override string getCategory()
        {
            return "Utils";
        }

        public override string getName()
        {
            return "MeshCreator";
        }

        public override string getDescription()
        {
            return "Creador de objetos 3D a paritir de primitivas simples." +
                   "Luego se puede exportar a un archivo XML de formato TgcScee para su posterior uso.";
        }

        public override void init()
        {
            //Configurar camara FPS
            GuiController.Instance.RotCamera.Enable = false;

            //Color de fondo
            GuiController.Instance.BackgroundColor = Color.FromArgb(38, 38, 38);

            //Crear modifier especial para este editor
            modifier = new MeshCreatorModifier("TgcMeshCreator", this);
            GuiController.Instance.Modifiers.add(modifier);
        }

        public override void render(float elapsedTime)
        {
            //Delegar render al control
            modifier.Control.render();
        }

        public override void close()
        {
            //Delegar al control
            modifier.dispose();
        }
    }
}