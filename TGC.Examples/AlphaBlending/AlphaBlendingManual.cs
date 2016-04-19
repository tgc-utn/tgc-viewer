using System;
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
            D3DDevice.Instance.Device.RenderState.SourceBlend = Blend.SourceAlpha;
            D3DDevice.Instance.Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BoxAlpha\\Textures\\pruebaAlpha.png");

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

            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(50.0f, 50.0f, 150.0f), new Vector3());

            GuiController.Instance.Modifiers.addBoolean("invertRender", "Invert Order Render", false);
        }

        public override void render(float elapsedTime)
        {
            var invert = (bool)GuiController.Instance.Modifiers["invertRender"];

            //el tgcmesh hace reset de alpha lo que hace que los ejemplos de blending manual no funcionen.
            //setAlphaEnable();
            if (invert)
            {
                setAlphaEnable();
                mesh2.render();
                setAlphaEnable();
                mesh1.render();
            }
            else
            {
                setAlphaEnable();
                mesh1.render();
                setAlphaEnable();
                mesh2.render();
            }
        }

        private void setAlphaEnable()
        {
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
        }

        public override void close()
        {
            mesh1.dispose();
            mesh2.dispose();
        }
    }
}