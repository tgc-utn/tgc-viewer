using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Textures;
using TGC.Core.Utils;

namespace TGC.Core.Direct3D
{
    public class D3DDevice
    {
        public static readonly Material DEFAULT_MATERIAL = new Material();

        private readonly Color DEFAULT_CLEAR_COLOR = Color.FromArgb(255, 78, 129, 179);

        /// <summary>
        ///     Constructor privado para poder hacer el singleton
        /// </summary>
        private D3DDevice()
        {
        }

        /// <summary>
        ///     Device de DirectX 3D para crear primitivas
        /// </summary>
        public Device Device { get; set; }

        //Valores de configuracion de la matriz de Proyeccion
        public float FieldOfViewY { get; } = FastMath.ToRad(45.0f);

        public float AspectRatio { get; set; } = -1f;

        public float ZFarPlaneDistance { get; } = 10000f;
        public float ZNearPlaneDistance { get; } = 1f;

        public static D3DDevice Instance { get; } = new D3DDevice();

        /// <summary>
        ///     Color con el que se limpia la pantalla
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        ///     Valores default del Direct3d Device
        /// </summary>
        public void setDefaultValues()
        {
            //Frustum values
            Device.Transform.Projection = Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), AspectRatio,
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
    }
}