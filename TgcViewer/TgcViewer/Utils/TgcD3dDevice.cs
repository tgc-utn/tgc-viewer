using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;
using System.Drawing;

namespace TgcViewer.Utils
{
    public class TgcD3dDevice
    {
        private Device d3dDevice = null;
        /// <summary>
        /// Direct3D Device
        /// </summary>
        public Device D3dDevice
        {
            get { return d3dDevice; }
        }

        Control panel3d;
        readonly Color DEFAULT_CLEAR_COLOR = Color.FromArgb(255, 78, 129, 179);
        public static readonly Material DEFAULT_MATERIAL = new Material();

        /// <summary>
        /// Cantidad de texturas simultaneas soportadas por DirectX
        /// </summary>
        public static readonly int DIRECTX_MULTITEXTURE_COUNT = 8;

        //Valores de configuracion de la matriz de Proyeccion
        public static float fieldOfViewY = Geometry.DegreeToRadian(45.0f);
        public static float aspectRatio = -1f;
        public static float zNearPlaneDistance = 1f;
        public static float zFarPlaneDistance = 10000f;

        private Color clearColor;
        /// <summary>
        /// Color con el que se limpia la pantalla
        /// </summary>
        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        public TgcD3dDevice(Control panel3d)
        {
            this.panel3d = panel3d;
            aspectRatio = (float)this.panel3d.Width / this.panel3d.Height;

            Caps caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            CreateFlags flags;

            Console.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            else
                flags = CreateFlags.SoftwareVertexProcessing;

            PresentParameters d3dpp = new PresentParameters();

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
            d3dDevice = new Device(0, DeviceType.Hardware, panel3d, flags, d3dpp);
            d3dDevice.DeviceReset += new System.EventHandler(this.OnResetDevice);
        }

        /// <summary>
        /// This event-handler is a good place to create and initialize any 
        /// Direct3D related objects, which may become invalid during a 
        /// device reset.
        /// </summary>
        public void OnResetDevice(object sender, EventArgs e)
        {
            GuiController.Instance.onResetDevice();
        }

        /// <summary>
        /// Hace las operaciones de Reset del device
        /// </summary>
        internal void doResetDevice()
        {
            setDefaultValues();

            //Reset Timer
            HighResolutionTimer.Instance.Reset();
        }

        /// <summary>
        /// Valores default del Direct3d Device
        /// </summary>
        internal void setDefaultValues()
        {
            //Frustum values
            d3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                aspectRatio, zNearPlaneDistance, zFarPlaneDistance);

            //Render state
            d3dDevice.RenderState.SpecularEnable = false;
            d3dDevice.RenderState.FillMode = FillMode.Solid;
            d3dDevice.RenderState.CullMode = Cull.None;
            d3dDevice.RenderState.ShadeMode = ShadeMode.Gouraud;
            d3dDevice.RenderState.MultiSampleAntiAlias = true;
            d3dDevice.RenderState.SlopeScaleDepthBias = -0.1f;
            d3dDevice.RenderState.DepthBias = 0f;
            d3dDevice.RenderState.ColorVertex = true;
            d3dDevice.RenderState.Lighting = false;
            d3dDevice.RenderState.ZBufferEnable = true;
            d3dDevice.RenderState.FogEnable = false;

            //Alpha Blending
            d3dDevice.RenderState.AlphaBlendEnable = false;
            d3dDevice.RenderState.AlphaTestEnable = false;
            d3dDevice.RenderState.ReferenceAlpha = 100;
            d3dDevice.RenderState.AlphaFunction = Compare.Greater;
            d3dDevice.RenderState.BlendOperation = BlendOperation.Add;
            d3dDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            d3dDevice.RenderState.DestinationBlend = Blend.InvSourceAlpha;

            //Texture Filtering
            d3dDevice.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            d3dDevice.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            d3dDevice.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            d3dDevice.SetSamplerState(1, SamplerStageStates.MinFilter, (int)TextureFilter.Linear);
            d3dDevice.SetSamplerState(1, SamplerStageStates.MagFilter, (int)TextureFilter.Linear);
            d3dDevice.SetSamplerState(1, SamplerStageStates.MipFilter, (int)TextureFilter.Linear);

            //Clear lights
            foreach (Light light in d3dDevice.Lights)
            {
                light.Enabled = false;
            }

            //Limpiar todas las texturas
            GuiController.Instance.TexturesManager.clearAll();

            //Reset Material
            d3dDevice.Material = DEFAULT_MATERIAL;
            clearColor = DEFAULT_CLEAR_COLOR;

            //Limpiar IndexBuffer
            d3dDevice.Indices = null;



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
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, clearColor, 1.0f, 0);
            HighResolutionTimer.Instance.Set();
        }

        internal void resetWorldTransofrm()
        {
            d3dDevice.Transform.World = Matrix.Identity;
        }

        /// <summary>
        /// Liberar Device al finalizar la aplicacion
        /// </summary>
        internal void shutDown()
        {
            d3dDevice.Dispose();
        }
    }
}
