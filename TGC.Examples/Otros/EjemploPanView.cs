using Microsoft.DirectX;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Otros
{
    /// <summary>
    ///     Pan view
    /// </summary>
    public class EjemploPanView : TgcExample
    {
        private TgcBox box;

        public EjemploPanView(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers, TgcAxisLines axisLines, TgcCamera camara) : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            this.Category = "Otros";
            this.Name = "Pan view";
            this.Description = "Pan view";
        }

        public override void Init()
        {
            box = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20, 20, 20), Color.Red);

            ((TgcRotationalCamera)this.Camara).targetObject(box.BoundingBox);
        }

        public override void Update(float elapsedTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);

            box.render();
        }

        public override void Close()
        {
            box.dispose();
        }
    }
}