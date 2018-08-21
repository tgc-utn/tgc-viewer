using SharpDX;
using SharpDX.Direct3D9;
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
        ///     Constructor privado para poder hacer el singleton
        /// </summary>
        private D3DDevice()
        {
        }

        public static D3DDevice Instance { get; } = new D3DDevice();

        /// <summary>
        ///     Device de DirectX 3D para crear primitivas
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
            Device.SetTransform(TransformState.Projection, TGCMatrix.PerspectiveFovLH(FieldOfView, AspectRatio, ZNearPlaneDistance, ZFarPlaneDistance).ToMatrix());
            //Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(FieldOfView, AspectRatio, ZNearPlaneDistance, ZFarPlaneDistance).ToMatrix();

            //Render state
            Device.SetRenderState(RenderState.SpecularEnable, false);
            Device.SetRenderState(RenderState.FillMode, FillMode.Solid);
            Device.SetRenderState(RenderState.CullMode, Cull.None);
            Device.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
            Device.SetRenderState(RenderState.MultisampleAntialias, true);
            Device.SetRenderState(RenderState.SlopeScaleDepthBias, -0.1f);
            Device.SetRenderState(RenderState.DepthBias, 0f);
            Device.SetRenderState(RenderState.ColorVertex, true);
            Device.SetRenderState(RenderState.Lighting, false);
            Device.SetRenderState(RenderState.ZEnable, true); //TODO: Averiguar si es el mismo valor que ZBufferEnable
            Device.SetRenderState(RenderState.FogEnable, false);

            //Alpha Blending
            Device.SetRenderState(RenderState.AlphaBlendEnable, false);
            Device.SetRenderState(RenderState.AlphaTestEnable, false);
            Device.SetRenderState(RenderState.AlphaRef, 50); //verificar un valor optimo.
            Device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
            Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
            Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            
            //Texture Filtering
            Device.SetSamplerState(0, SamplerState.MinFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MagFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MipFilter, (int)TextureFilter.Linear);
                                      
            Device.SetSamplerState(1, SamplerState.MinFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(1, SamplerState.MagFilter, (int)TextureFilter.Linear);
            Device.SetSamplerState(1, SamplerState.MipFilter, (int)TextureFilter.Linear);

            //Clear lights
            
            //foreach (Light light in Device.Lights)
            //{
            //    light.Enabled = false;
            //}

            //Limpiar todas las texturas
            TexturesManager.Instance.clearAll();

            //Reset Material
            Device.Material = DEFAULT_MATERIAL;

            //Limpiar IndexBuffer
            Device.Indices = null;

            EnableParticles();
        }

        /// <summary>
        ///     habilita los points sprites.
        ///     Estaba este comentario antes, asi que lo dejo con default false.
        ///     INEXPLICABLE PERO ESTO HACE QUE MI NOTEBOOK SE CUELGUE CON LA PANTALLA EN NEGRO!!!!!!!!!!
        /// </summary>
        public void EnableParticles()
        {
            if (ParticlesEnabled)
            {
                //PointSprite
                Device.SetRenderState(RenderState.PointSpriteEnable, true);
                Device.SetRenderState(RenderState.PointScaleEnable, true);
                Device.SetRenderState(RenderState.PointScaleA, 1.0f);
                Device.SetRenderState(RenderState.PointScaleB, 1.0f);
                Device.SetRenderState(RenderState.PointScaleC, 0.0f);
            }
        }

        public void InitializeD3DDevice(Panel panel)
        {
            AspectRatio = (float)panel.Width / panel.Height;
            Width = panel.Width;
            Height = panel.Height;
            
            var caps = Device.Direct3D.GetDeviceCaps(Device.Capabilities.AdapterOrdinal, DeviceType.Hardware);//TODO: Manager.Adapters.Default.Adapter = 0 ??
            Debug.WriteLine("Max primitive count:" + caps.MaxPrimitiveCount);

            CreateFlags flags;
            
            //if (caps.DeviceCaps.HWTransformAndLight)
                flags = CreateFlags.HardwareVertexProcessing;
            //else
            //    flags = CreateFlags.SoftwareVertexProcessing;
            d3dpp = CreatePresentationParameters();

            //Crear Graphics Device
            //Device.IsUsingEventHandlers = false;
            Device = new Device(Device.Direct3D, Device.Capabilities.AdapterOrdinal, DeviceType.Hardware, panel.Handle, flags, d3dpp);
        }

        private PresentParameters CreatePresentationParameters()
        {
            d3dpp = new PresentParameters();
            d3dpp.BackBufferFormat = Format.Unknown;
            d3dpp.SwapEffect = SwapEffect.Discard;
            d3dpp.Windowed = true;
            d3dpp.EnableAutoDepthStencil = true;
            d3dpp.AutoDepthStencilFormat = Format.D24S8;
            d3dpp.PresentationInterval = PresentInterval.Immediate;

            //Antialiasing
            if (Device.Direct3D.CheckDeviceMultisampleType(Device.Capabilities.AdapterOrdinal, DeviceType.Hardware,
                Format.Unknown, true, MultisampleType.NonMaskable))
            {
                d3dpp.MultiSampleType = MultisampleType.NonMaskable;
                d3dpp.MultiSampleQuality = 0;
            }
            else
            {
                d3dpp.MultiSampleType = MultisampleType.None;
            }

            return d3dpp;
        }

        public void UpdateAspectRatioAndProjection(int width, int height)
        {
            AspectRatio = (float)width / height;
            Width = width;
            Height = height;
            //TODO: hay que actualizar tambien la matriz de proyeccion, sino sigue viendo mal. cc: revisar puede estar fallando
            Device.SetTransform(TransformState.Projection, TGCMatrix.PerspectiveFovLH(FieldOfView, AspectRatio, ZNearPlaneDistance,
                ZFarPlaneDistance).ToMatrix());
            //TODO: FALTA TODO ESTO DE ABAJO.... cc: No se si habia que sacarlo o dejarlo
            //DefaultValues();
            //Device.Reset(d3dpp);

            Viewport v = new Viewport();
            //v.MaxDepth = Device.Viewport.MaxDepth;
            //v.MinDepth = Device.Viewport.MinDepth;

            v.X = Device.Viewport.X;
            v.Y = Device.Viewport.Y;
            v.Width = Width;
            v.Height = Height;
            Device.Viewport = v;
        }

        public void FillModeWireFrame()
        {
            Device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
        }

        public void FillModeWireSolid()
        {
            Device.SetRenderState(RenderState.FillMode, FillMode.Solid);
        }

        public void Dispose()
        {
            Device.Dispose();
        }
    }
}