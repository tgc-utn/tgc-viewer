using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    public class EjemploVideoPostProcess : TgcExample
    {
        private int cant_frames;

        private int cur_frame;
        private Effect effect;
        private Texture g_pRenderTarget;

        // Render Target pasadas
        private Texture g_pRenderTarget_A;

        private Texture g_pRenderTarget_ant;

        private Texture g_pRenderTarget_B;
        private Texture g_pRenderTarget_C;
        private VertexBuffer g_pVB;
        private Texture[] g_pVideoBuffer;
        private Texture g_pVideoFrame;
        private string MyMediaDir;
        private string MyShaderDir;
        private Surface pOldRT;

        //private Surface pOldDS;
        private Surface pSurf;

        private float time;

        public EjemploVideoPostProcess(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Workshop-VideoPostProcess";
            Description = "Procesamiento de imagenes";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            cur_frame = 0;
            time = 0;
            g_pVideoBuffer = new Texture[500];

            Camara = new TgcRotationalCamera(new Vector3(0, 0, 0), 100);

            // Quad
            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };

            //vertex buffer de los triangulos
            g_pVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVB.SetData(vertices, 0, LockFlags.None);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);
            g_pRenderTarget_ant = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);
            g_pRenderTarget_A = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);
            g_pRenderTarget_B = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);
            g_pRenderTarget_C = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8,
                Pool.Default);

            //Cargar Shader de post.procesado de imagenes
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "ImageProccesing.fx", null, null, ShaderFlags.None, null,
                out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            effect.SetValue("screen_inc_x", 1.0f / d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_inc_y", 1.0f / d3dDevice.PresentationParameters.BackBufferHeight);

            cant_frames = 400;

            // parche: convierto todos los bmps a jpgs:
            /*
            for (int i = 0; i < cant_frames; ++i)
            {
                string fname = "c:\\msdev\\varios\\videoana\\" + string.Format("snap{0:D}.bmp", i);
                Bitmap b = (Bitmap)Bitmap.FromFile(fname);
                string fname_jpg = "c:\\msdev\\varios\\videoana\\" + string.Format("snap{0:D}.jpg", i);
                b.Save(fname_jpg, System.Drawing.Imaging.ImageFormat.Jpeg);
                b.Dispose();
            }*/

            /*
            for (int i = 0; i < cant_frames; ++i)
            {
                string fname = "c:\\msdev\\varios\\videoana\\" + string.Format("snap{0:D}.jpg", i*6);
                Bitmap b = (Bitmap)Bitmap.FromFile(fname);
                g_pVideoBuffer[i] = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
                b.Dispose();
            }*/
        }

        public override void Update()
        {
            base.helperPreUpdate();
        }

        public override void Render()
        {
            base.helperRenderClearTextures();

            var d3dDevice = D3DDevice.Instance.Device;

            time += ElapsedTime;
            var ant_frame = cur_frame;
            //cur_frame = (int)(time * 24);      // 24 fps?
            cur_frame += 2;
            if (cur_frame >= cant_frames)
                cur_frame = 0; // reinicio

            if (cur_frame != ant_frame)
            {
                if (g_pVideoFrame != null)
                    g_pVideoFrame.Dispose();

                // cargo y proceso el frame:
                // Cargo el frame en una texture
                var fname = MyMediaDir + "video\\" + string.Format("snap{0:D}.jpg", cur_frame);
                var b = (Bitmap)Image.FromFile(fname);
                g_pVideoFrame = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
                b.Dispose();
            }

            // seteos varios
            d3dDevice.Transform.View = Matrix.Identity;
            d3dDevice.Transform.World = Matrix.Identity;
            d3dDevice.Transform.Projection = Matrix.Identity;
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, g_pVB, 0);

            // dibujo la escena una textura
            effect.Technique = "ImageFilter";
            pOldRT = d3dDevice.GetRenderTarget(0);
            pSurf = g_pRenderTarget_A.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            effect.SetValue("base_Tex", g_pVideoFrame);

            //1-era pasada: calculo la intensidad
            // imagen-->rendertarget_A
            d3dDevice.BeginScene();
            effect.Begin(0);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();

            //2-da pasada: aplico el erotion filter
            // rendertarget_A--->rendertarget_B
            pSurf = g_pRenderTarget_B.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("base_Tex", g_pRenderTarget_A);
            effect.BeginPass(1);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();

            //3-da pasada: aplico el filtro sobel
            // rendertarget_B->rendertarget_C
            pSurf = g_pRenderTarget_C.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("base_Tex", g_pRenderTarget_B);
            effect.BeginPass(2);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();

            //4-da pasada: gaussian blur
            // rendertarget_C->rendertarget
            pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, pSurf);
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.SetValue("base_Tex", g_pRenderTarget_C);
            effect.BeginPass(3);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            d3dDevice.EndScene();

            // restuaro el render target
            d3dDevice.SetRenderTarget(0, pOldRT);
            d3dDevice.BeginScene();
            effect.Technique = "motionDetect";
            effect.SetValue("base_Tex", g_pVideoFrame);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_RenderTarget_ant", g_pRenderTarget_ant);
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            d3dDevice.EndScene();
            d3dDevice.Present();

            // cambio los render Targets
            var p_aux = g_pRenderTarget_ant;
            g_pRenderTarget_ant = g_pRenderTarget;
            g_pRenderTarget = p_aux;
        }

        /// <summary>
        ///     Método que se llama cuando termina la ejecución del ejemplo.
        ///     Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void Dispose()
        {
            

            //for (int i = 0; i < cant_frames; ++i)
            //  g_pVideoBuffer[i].Dispose();

            if (g_pVideoFrame != null)
                g_pVideoFrame.Dispose();
            effect.Dispose();
        }

        /// </summary>
        /// and plays the first frame inside the panel
        /// opens a video from an avi file

        /// <summary>
    }
}