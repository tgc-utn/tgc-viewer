using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.AlphaBlending
{
    /// <summary>
    /// AlphaBlendingManual
    /// </summary>
    public class AlphaBlendingManual : TgcExample
    {
        TgcPlaneWall mesh1;
        TgcPlaneWall mesh2;

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
            Device d3dDevice = GuiController.Instance.D3dDevice;

            d3dDevice.RenderState.AlphaFunction = Compare.Greater;
            d3dDevice.RenderState.BlendOperation = BlendOperation.Add;
            d3dDevice.RenderState.AlphaBlendEnable = true;
            d3dDevice.RenderState.AlphaTestEnable = true;
            d3dDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            d3dDevice.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            TgcTexture texture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\BoxAlpha\\Textures\\pruebaAlpha.png");

            mesh1 = new TgcPlaneWall(new Vector3(0,0,0), new Vector3(100, 100, 0), TgcPlaneWall.Orientations.XYplane, texture);
            mesh1.AutoAdjustUv = false;
            mesh1.UTile = 1;
            mesh1.VTile = 1;
            mesh1.updateValues();

            mesh2 = new TgcPlaneWall(new Vector3(0,0,100), new Vector3(100, 100, 0), TgcPlaneWall.Orientations.XYplane, texture);
            mesh2.AutoAdjustUv = false;
            mesh2.UTile = 1;
            mesh2.VTile = 1;
            mesh2.updateValues();


            GuiController.Instance.FpsCamera.Enable = true;

            GuiController.Instance.Modifiers.addBoolean("invertRender", "Invert Render", false);
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            bool invert = (bool)GuiController.Instance.Modifiers["invertRender"];

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

            Device d3dDevice = GuiController.Instance.D3dDevice;
        }

    }
}
