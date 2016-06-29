using Microsoft.DirectX;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     Pan view
    /// </summary>
    public class EjemploPanView : TGCExampleViewer
    {
        private TgcBox box;

        public EjemploPanView(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Others";
            Name = "Pan view";
            Description = "Pan view";
        }

        public override void Init()
        {
            box = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20, 20, 20), Color.Red);

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

            box.render();

            PostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}