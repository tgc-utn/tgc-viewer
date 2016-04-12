using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Util;

namespace TGC.Examples.Otros
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
            box = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20, 20, 20), Color.Red);

            GuiController.Instance.RotCamera.targetObject(box.BoundingBox);
        }

        public override void render(float elapsedTime)
        {
            box.render();
        }

        public override void close()
        {
            box.dispose();
        }
    }
}