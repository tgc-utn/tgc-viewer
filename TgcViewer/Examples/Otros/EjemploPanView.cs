using System.Drawing;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TGC.Core.Example;

namespace Examples.Otros
{
    /// <summary>
    ///     Pan view
    /// </summary>
    public class EjemploPanView : TgcExample
    {
        private TgcBox box;

        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Pan view";
        }

        public override string getDescription()
        {
            return "Pan view.";
        }

        public override void init()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            box = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20, 20, 20), Color.Red);

            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            box.render();
        }

        public override void close()
        {
            box.dispose();
        }
    }
}