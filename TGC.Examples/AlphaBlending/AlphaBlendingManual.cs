using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingManual
    /// </summary>
    public class AlphaBlendingManual : TGCExampleViewer
    {
        private TgcPlaneWall mesh1;
        private TgcPlaneWall mesh2;

        public AlphaBlendingManual(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "AlphaBlending";
            Name = "AlphaBlending Manual";
            Description = "AlphaBlending Manual";
        }

        public override void Init()
        {
            D3DDevice.Instance.Device.RenderState.AlphaFunction = Compare.Greater;
            D3DDevice.Instance.Device.RenderState.BlendOperation = BlendOperation.Add;
            D3DDevice.Instance.Device.RenderState.SourceBlend = Blend.SourceAlpha;
            D3DDevice.Instance.Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                MediaDir + "ModelosTgc\\BoxAlpha\\Textures\\pruebaAlpha.png");

            mesh1 = new TgcPlaneWall(new Vector3(0, 0, -50), new Vector3(50, 50, 50), TgcPlaneWall.Orientations.XYplane,
                texture);
            mesh1.AutoAdjustUv = false;
            mesh1.UTile = 1;
            mesh1.VTile = 1;
            mesh1.updateValues();

            mesh2 = new TgcPlaneWall(new Vector3(0, 0, 50), new Vector3(50, 50, 50), TgcPlaneWall.Orientations.XYplane,
                texture);
            mesh2.AutoAdjustUv = false;
            mesh2.UTile = 1;
            mesh2.VTile = 1;
            mesh2.updateValues();

            Camara = new TgcFpsCamera(new Vector3(50.0f, 50.0f, 150.0f), Input);

            Modifiers.addBoolean("invertRender", "Invert Render", false);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            PreRender();

            var invert = (bool)Modifiers["invertRender"];

            //el tgcmesh hace reset de alpha lo que hace que los ejemplos de blending manual no funcionen.
            //setAlphaEnable();
            if (invert)
            {
                setAlphaEnable();
                mesh1.render();
            }
            else
            {
                setAlphaEnable();
                mesh2.render();
            }

            PostRender();
        }

        private void setAlphaEnable()
        {
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
        }

        public override void Dispose()
        {
            mesh1.dispose();
            mesh2.dispose();
        }
    }
}