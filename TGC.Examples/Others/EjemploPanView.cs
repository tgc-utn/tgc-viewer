using Microsoft.DirectX;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     Pan view
    /// </summary>
    public class EjemploPanView : TgcExample
    {
        private TgcBox box;

        public EjemploPanView(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Others";
            Name = "Pan view";
            Description = "Pan view";
        }

        public override void Init()
        {
            box = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20, 20, 20), Color.Red);

            Camara = new TgcRotationalCamera(box.BoundingBox.calculateBoxCenter(), box.BoundingBox.calculateBoxRadius() * 2);
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperPreRender();
            

            box.render();

            helperPostRender();
        }

        public override void Dispose()
        {
            box.dispose();
        }
    }
}