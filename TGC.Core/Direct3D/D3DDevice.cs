using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Utils;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using TGC.Core.Textures;

namespace TGC.Core.Direct3D
{
    public class D3DDevice
    {
        public static readonly Material DEFAULT_MATERIAL = new Material();

        private readonly Color DEFAULT_CLEAR_COLOR = Color.FromArgb(255, 78, 129, 179);

        /// <summary>
        ///     Constructor privado para poder hacer el singleton
        /// </summary>
        private D3DDevice() { }

        /// <summary>
        ///     Device de DirectX 3D para crear primitivas
        /// </summary>
        public Device Device { get; set; }

        //Valores de configuracion de la matriz de Proyeccion
        public float FieldOfViewY { get; } = FastMath.ToRad(45.0f);

        public float AspectRatio { get; set; } = -1f;
        private int width;
        private int height;

        public float ZFarPlaneDistance { get; } = 10000f;
        public float ZNearPlaneDistance { get; } = 1f;

        public static D3DDevice Instance { get; } = new D3DDevice();

        /// <summary>
        ///     Color con el que se limpia la pantalla
        /// </summary>
        public Color ClearColor { get; set; }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        ///     Valores default del Direct3d Device
        /// </summary>
        public void DefaultValues()
        {
            //Frustum values
            Device.Transform.Projection = Matrix.PerspectiveFovLH(FastMath.ToRad(45.0f), AspectRatio,
                ZNearPlaneDistance, ZFarPlaneDistance);

            //Render state
            Device.RenderState.SpecularEnable = false;
            Device.RenderState.FillMode = FillMode.Solid;
            Device.RenderState.CullMode = Cull.None;
            Device.RenderState.ShadeMode = ShadeMode.Gouraud;
            Device.RenderState.MultiSampleAntiAlias = true;
            Device.RenderState.SlopeScaleDepthBias = -0.1f;
            Device.RenderState.DepthBias = 0f;
            Device.RenderState.ColorVertex = true;
            Device.RenderState.Lighting = false;
            Device.RenderState.ZBufferEnable = true;
            Device.RenderState.FogEnable = false;

            //Alpha Blending
            Device.RenderState.AlphaBlendEnable = false;
            Device.RenderState.AlphaTestEnable = false;
            Device.RenderState.ReferenceAlpha = 50;//verificar un valor optimo.
            Device.RenderState.AlphaFunction = Compare.Greater;
            Device.RenderState.BlendOperation = BlendOperation.Add;
            Device.RenderState.SourceBlend = Blend.SourceAlpha;
            Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            //Texture Filtering
            Device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            Device.SetSamplerState(1, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(1, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(1, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            //Clear lights
            foreach (Light light in Device.Lights)
            {
                light.Enabled = false;
            }

            //Limpiar todas las texturas
            TexturesManager.Instance.clearAll();

            //Reset Material
            Device.Material = DEFAULT_MATERIAL;
            ClearColor = DEFAULT_CLEAR_COLOR;

            //Limpiar IndexBuffer
            Device.Indices = null;

            /* INEXPLICABLE PERO ESTO HACE QUE MI NOTEBOOK SE CUELGUE CON LA PANTALLA EN NEGRO!!!!!!!!!!

            //PointSprite
            this.Device.RenderState.PointSpriteEnable = true;
            this.Device.RenderState.PointScaleEnable = true;
            this.Device.RenderState.PointScaleA = 1.0f;
            this.Device.RenderState.PointScaleB = 1.0f;
            this.Device.RenderState.PointScaleC = 0.0f;
             */
        }

        public void InitializeD3DDevice(Panel panel)
        {
            this.AspectRatio = (float)panel.Width / panel.Height;
            this.Width = panel.Width;
            this.Height = panel.Height;

            var caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            Debug.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

            CreateFlags flags;
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            else
                flags = CreateFlags.SoftwareVertexProcessing;

            var d3dpp = new PresentParameters();

            d3dpp.BackBufferFormat = Format.Unknown;
            d3dpp.SwapEffect = SwapEffect.Discard;
            d3dpp.Windowed = true;
            d3dpp.EnableAutoDepthStencil = true;
            d3dpp.AutoDepthStencilFormat = DepthFormat.D24S8;
            d3dpp.PresentationInterval = PresentInterval.Immediate;

            //Antialiasing
            if (Manager.CheckDeviceMultiSampleType(Manager.Adapters.Default.Adapter, DeviceType.Hardware,
                Manager.Adapters.Default.CurrentDisplayMode.Format, true, MultiSampleType.NonMaskable))
            {
                d3dpp.MultiSample = MultiSampleType.NonMaskable;
                d3dpp.MultiSampleQuality = 0;
            }
            else
            {
                d3dpp.MultiSample = MultiSampleType.None;
            }

            //Crear Graphics Device
            Device.IsUsingEventHandlers = false;
            var d3DDevice = new Device(0, DeviceType.Hardware, panel, flags, d3dpp);

            this.Device = d3DDevice;
        }

        public void FillModeWireFrame()
        {
            this.Device.RenderState.FillMode = FillMode.WireFrame;
        }

        public void FillModeWireSolid()
        {
            this.Device.RenderState.FillMode = FillMode.Solid;
        }

        public void Clear()
        {
            this.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, D3DDevice.Instance.ClearColor, 1.0f, 0);
        }
    }
}