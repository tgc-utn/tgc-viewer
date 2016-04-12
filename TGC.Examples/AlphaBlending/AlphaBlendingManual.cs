using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Textures;
using TGC.Util;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingManual
    /// </summary>
    public class AlphaBlendingManual : TgcExample
    {
        private TgcPlaneWall mesh1;
        private TgcPlaneWall mesh2;

        public override string getCategory()
        {
            return "AlphaBlending";
        }

        public override string getName()
        {
            return "AlphaBlending Manual";
        }

        public override string getDescription()
        {
            return "AlphaBlending Manual";
        }

        public override void init()
        {
            D3DDevice.Instance.Device.RenderState.AlphaFunction = Compare.Greater;
            D3DDevice.Instance.Device.RenderState.BlendOperation = BlendOperation.Add;
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
            D3DDevice.Instance.Device.RenderState.SourceBlend = Blend.SourceAlpha;
            D3DDevice.Instance.Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BoxAlpha\\Textures\\pruebaAlpha.png");

            mesh1 = new TgcPlaneWall(new Vector3(0, 0, 0), new Vector3(100, 100, 0), TgcPlaneWall.Orientations.XYplane,
                texture);
            mesh1.AutoAdjustUv = false;
            mesh1.UTile = 1;
            mesh1.VTile = 1;
            mesh1.updateValues();

            mesh2 = new TgcPlaneWall(new Vector3(0, 0, 100), new Vector3(100, 100, 0), TgcPlaneWall.Orientations.XYplane,
                texture);
            mesh2.AutoAdjustUv = false;
            mesh2.UTile = 1;
            mesh2.VTile = 1;
            mesh2.updateValues();

            GuiController.Instance.FpsCamera.Enable = true;

            GuiController.Instance.Modifiers.addBoolean("invertRender", "Invert Render", false);
        }

        public override void render(float elapsedTime)
        {
            var invert = (bool)GuiController.Instance.Modifiers["invertRender"];

            if (invert)
            {
                mesh2.render();
                mesh1.render();
            }
            else
            {
                mesh1.render();
                mesh2.render();
            }
        }

        public override void close()
        {
            mesh1.dispose();
            mesh2.dispose();
        }
    }
}