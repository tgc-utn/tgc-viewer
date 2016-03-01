using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Utils;

namespace TGC.Viewer.Utils
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

        public TgcD3dDevice(Control panel3d)
        {
            aspectRatio = (float)panel3d.Width / panel3d.Height;

            var caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            Console.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

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
            var d3DDevice = new Device(0, DeviceType.Hardware, panel3d, flags, d3dpp);
            d3DDevice.DeviceReset += OnResetDevice;

            D3DDevice.Instance.Device = d3DDevice;
        }

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
            D3DDevice.Instance.Device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                    aspectRatio, zNearPlaneDistance, zFarPlaneDistance);

            //Render state
            D3DDevice.Instance.Device.RenderState.SpecularEnable = false;
            D3DDevice.Instance.Device.RenderState.FillMode = FillMode.Solid;
            D3DDevice.Instance.Device.RenderState.CullMode = Cull.None;
            D3DDevice.Instance.Device.RenderState.ShadeMode = ShadeMode.Gouraud;
            D3DDevice.Instance.Device.RenderState.MultiSampleAntiAlias = true;
            D3DDevice.Instance.Device.RenderState.SlopeScaleDepthBias = -0.1f;
            D3DDevice.Instance.Device.RenderState.DepthBias = 0f;
            D3DDevice.Instance.Device.RenderState.ColorVertex = true;
            D3DDevice.Instance.Device.RenderState.Lighting = false;
            D3DDevice.Instance.Device.RenderState.ZBufferEnable = true;
            D3DDevice.Instance.Device.RenderState.FogEnable = false;

            //Alpha Blending
            D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
            D3DDevice.Instance.Device.RenderState.AlphaTestEnable = false;
            D3DDevice.Instance.Device.RenderState.ReferenceAlpha = 100;
            D3DDevice.Instance.Device.RenderState.AlphaFunction = Compare.Greater;
            D3DDevice.Instance.Device.RenderState.BlendOperation = BlendOperation.Add;
            D3DDevice.Instance.Device.RenderState.SourceBlend = Blend.SourceAlpha;
            D3DDevice.Instance.Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            //Texture Filtering
            D3DDevice.Instance.Device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            D3DDevice.Instance.Device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            D3DDevice.Instance.Device.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            D3DDevice.Instance.Device.SetSamplerState(1, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            D3DDevice.Instance.Device.SetSamplerState(1, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            D3DDevice.Instance.Device.SetSamplerState(1, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            //Clear lights
            foreach (Light light in D3DDevice.Instance.Device.Lights)
            {
                light.Enabled = false;
            }

            //Limpiar todas las texturas
            GuiController.Instance.TexturesManager.clearAll();

            //Reset Material
            D3DDevice.Instance.Device.Material = DEFAULT_MATERIAL;
            ClearColor = DEFAULT_CLEAR_COLOR;

            //Limpiar IndexBuffer
            D3DDevice.Instance.Device.Indices = null;

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
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ClearColor, 1.0f, 0);
            HighResolutionTimer.Instance.Set();
        }

        internal void resetWorldTransofrm()
        {
            D3DDevice.Instance.Device.Transform.World = Matrix.Identity;
        }

        /// <summary>
        ///     Liberar Device al finalizar la aplicacion
        /// </summary>
        internal void shutDown()
        {
            D3DDevice.Instance.Device.Dispose();
        }
    }
}