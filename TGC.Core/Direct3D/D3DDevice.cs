using Microsoft.DirectX.Direct3D;
using System.Diagnostics;
using System.Windows.Forms;
using TGC.Core.Mathematica;
using TGC.Core.Textures;

namespace TGC.Core.Direct3D
{
    public class D3DDevice
    {
        public static readonly Material DEFAULT_MATERIAL = new Material();
        private PresentParameters d3dpp;

        /// <summary>
        /// Constructor privado para el Singleton
        /// </summary>
        private D3DDevice() { }

        /// <summary>
        /// Permite acceder a la instancia del Singleton.
        /// </summary>
        public static D3DDevice Instance { get; } = new D3DDevice();

        /// <summary>
        /// Device de DirectX 3D para crear primitivas
        /// </summary>
        public Device Device { get; set; }

        //Valores de configuracion de la matriz de Proyeccion
        public float FieldOfView { get; set; } = FastMath.ToRad(45.0f);

        public float AspectRatio { get; set; } = -1f;
        public float ZFarPlaneDistance { get; set; } = 10000f;
        public float ZNearPlaneDistance { get; set; } = 1f;
        public bool ParticlesEnabled { get; set; } = false;
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        ///     Valores default del Direct3d Device
        /// </summary>
        public void DefaultValues()
        {
            //Frustum values
            Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(FieldOfView, AspectRatio, ZNearPlaneDistance,
                ZFarPlaneDistance).ToMatrix();

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
            Device.RenderState.ReferenceAlpha = 50; //verificar un valor optimo.
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

            //Limpiar IndexBuffer
            Device.Indices = null;

            EnableParticles();
        }

        /// <summary>
        /// Habilita los points sprites.
        /// Estaba este comentario antes, asi que lo dejo con default false.
        /// INEXPLICABLE PERO ESTO HACE QUE MI NOTEBOOK SE CUELGUE CON LA PANTALLA EN NEGRO!!!!!!!!!!
        /// </summary>
        public void EnableParticles()
        {
            if (ParticlesEnabled)
            {
                //PointSprite
                Device.RenderState.PointSpriteEnable = true;
                Device.RenderState.PointScaleEnable = true;
                Device.RenderState.PointScaleA = 1.0f;
                Device.RenderState.PointScaleB = 1.0f;
                Device.RenderState.PointScaleC = 0.0f;
            }
        }

        /// <summary>
        /// Inicializa el Device de DirectX con lo parametros del Control.
        /// </summary>
        /// <param name="control"> Control donde va a ejecutar el Device.</param>
        public void InitializeD3DDevice(Control control)
        {
            AspectRatio = (float)control.Width / control.Height;
            Width = control.Width;
            Height = control.Height;

            var caps = Manager.GetDeviceCaps(Manager.Adapters.Default.Adapter, DeviceType.Hardware);
            Debug.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

            CreateFlags flags;
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            else
                flags = CreateFlags.SoftwareVertexProcessing;
            d3dpp = CreatePresentationParameters();

            //Crear Graphics Device
            Device.IsUsingEventHandlers = false;
            Device = new Device(0, DeviceType.Hardware, control, flags, d3dpp);
        }

        private PresentParameters CreatePresentationParameters()
        {
            d3dpp = new PresentParameters();
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

            return d3dpp;
        }

        public void UpdateAspectRatioAndProjection(int width, int height)
        {
            AspectRatio = (float)width / height;
            Width = width;
            Height = height;
            //hay que actualizar tambien la matriz de proyeccion, sino sigue viendo mal.
            Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(FieldOfView, AspectRatio, ZNearPlaneDistance, ZFarPlaneDistance).ToMatrix();
            //FALTA TODO ESTO DE ABAJO....
            //DefaultValues();
            //Device.Reset(d3dpp);

            /*Viewport v = new Viewport();
            v.MaxZ = Device.Viewport.MaxZ;
            v.MinZ = Device.Viewport.MinZ;
            v.X = Device.Viewport.X;
            v.Y = Device.Viewport.Y;
            v.Width = Width;
            v.Height = Height;
            Device.Viewport = v;*/
        }

        public void FillModeWireFrame()
        {
            Device.RenderState.FillMode = FillMode.WireFrame;
        }

        public void FillModeWireSolid()
        {
            Device.RenderState.FillMode = FillMode.Solid;
        }

        public void Dispose()
        {
            Device.Dispose();
        }
    }
}