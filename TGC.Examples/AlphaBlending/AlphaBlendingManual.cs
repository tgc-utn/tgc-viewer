using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Textures;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.AlphaBlending
{
    /// <summary>
    ///     AlphaBlendingManual
    /// </summary>
    public class AlphaBlendingManual : TgcExample
    {
        private TgcPlaneWall mesh1;
        private TgcPlaneWall mesh2;

        public AlphaBlendingManual(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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

            Camara = new TgcFpsCamera();
            Camara.setCamera(new Vector3(50.0f, 50.0f, 150.0f), new Vector3());

            Modifiers.addBoolean("invertRender", "Invert Render", false);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

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

            FinalizarEscena();
        }

        private void setAlphaEnable()
        {
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = true;
        }

        public override void Close()
        {
            base.Close();

            mesh1.dispose();
            mesh2.dispose();
        }
    }
}