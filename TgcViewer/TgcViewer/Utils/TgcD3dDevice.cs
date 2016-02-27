using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Utils;

namespace TgcViewer.Utils
{
    public class TgcD3dDevice
    {
        public static readonly Material DEFAULT_MATERIAL = new Material();

        /// <summary>
        ///     Cantidad de texturas simultaneas soportadas por DirectX
        /// </summary>
        public static readonly int DIRECTX_MULTITEXTURE_COUNT = 8;

        //Valores de configuracion de la matriz de Proyeccion
        public static float fieldOfViewY = FastMath.ToRad(45.0f);

        public static float aspectRatio = -1f;
        public static float zNearPlaneDistance = 1f;
        public static float zFarPlaneDistance = 10000f;
        private readonly Color DEFAULT_CLEAR_COLOR = Color.FromArgb(255, 78, 129, 179);

        private readonly Control panel3d;

        public TgcD3dDevice(Control panel3d)
        {
            this.panel3d = panel3d;
            aspectRatio = (float) this.panel3d.Width/this.panel3d.Height;

            var caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            CreateFlags flags;

            Console.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

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
            D3dDevice = new Device(0, DeviceType.Hardware, panel3d, flags, d3dpp);
            D3dDevice.DeviceReset += OnResetDevice;
        }

        /// <summary>
        ///     Direct3D Device
        /// </summary>
        public Device D3dDevice { get; }

        /// <summary>
        ///     Color con el que se limpia la pantalla
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        ///     This event-handler is a good place to create and initialize any
        ///     Direct3D related objects, which may become invalid during a
        ///     device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            GuiController.Instance.onResetDevice();
        }

        /// <summary>
        ///     Hace las operaciones de Reset del device
        /// </summary>
        internal void doResetDevice()
        {
            setDefaultValues();

            //Reset Timer
            HighResolutionTimer.Instance.Reset();
        }

        /// <summary>
        ///     Valores default del Direct3d Device
        /// </summary>
        internal void setDefaultValues()
        {
            //Frustum values
            D3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                    aspectRatio, zNearPlaneDistance, zFarPlaneDistance);

            //Render state
            D3dDevice.RenderState.SpecularEnable = false;
            D3dDevice.RenderState.FillMode = FillMode.Solid;
            D3dDevice.RenderState.CullMode = Cull.None;
            D3dDevice.RenderState.ShadeMode = ShadeMode.Gouraud;
            D3dDevice.RenderState.MultiSampleAntiAlias = true;
            D3dDevice.RenderState.SlopeScaleDepthBias = -0.1f;
            D3dDevice.RenderState.DepthBias = 0f;
            D3dDevice.RenderState.ColorVertex = true;
            D3dDevice.RenderState.Lighting = false;
            D3dDevice.RenderState.ZBufferEnable = true;
            D3dDevice.RenderState.FogEnable = false;

            //Alpha Blending
            D3dDevice.RenderState.AlphaBlendEnable = false;
            D3dDevice.RenderState.AlphaTestEnable = false;
            D3dDevice.RenderState.ReferenceAlpha = 100;
            D3dDevice.RenderState.AlphaFunction = Compare.Greater;
            D3dDevice.RenderState.BlendOperation = BlendOperation.Add;
            D3dDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            D3dDevice.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            //Texture Filtering
            D3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int) TextureFilter.Linear);
            D3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int) TextureFilter.Linear);
            D3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int) TextureFilter.Linear);

            D3dDevice.SetSamplerState(1, SamplerStageStates.MinFilter, (int) TextureFilter.Linear);
            D3dDevice.SetSamplerState(1, SamplerStageStates.MagFilter, (int) TextureFilter.Linear);
            D3dDevice.SetSamplerState(1, SamplerStageStates.MipFilter, (int) TextureFilter.Linear);

            //Clear lights
            foreach (Light light in D3dDevice.Lights)
            {
                light.Enabled = false;
            }

            //Limpiar todas las texturas
            GuiController.Instance.TexturesManager.clearAll();

            //Reset Material
            D3dDevice.Material = DEFAULT_MATERIAL;
            ClearColor = DEFAULT_CLEAR_COLOR;

            //Limpiar IndexBuffer
            D3dDevice.Indices = null;

            /* INEXPLICABLE PERO ESTO HACE QUE MI NOTEBOOK SE CUELGUE CON LA PANTALLA EN NEGRO!!!!!!!!!!

            //PointSprite
            this.d3dDevice.RenderState.PointSpriteEnable = true;
            this.d3dDevice.RenderState.PointScaleEnable = true;
            this.d3dDevice.RenderState.PointScaleA = 1.0f;
            this.d3dDevice.RenderState.PointScaleB = 1.0f;
            this.d3dDevice.RenderState.PointScaleC = 0.0f;
             */
        }

        internal void doClear()
        {
            D3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ClearColor, 1.0f, 0);
            HighResolutionTimer.Instance.Set();
        }

        internal void resetWorldTransofrm()
        {
            D3dDevice.Transform.World = Matrix.Identity;
        }

        /// <summary>
        ///     Liberar Device al finalizar la aplicacion
        /// </summary>
        internal void shutDown()
        {
            D3dDevice.Dispose();
        }
    }
}