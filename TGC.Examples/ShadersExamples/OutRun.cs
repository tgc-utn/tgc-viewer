using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    public class OutRun : TgcExample
    {
        public float acel_mouse_wheel = 20f;
        private TgcMesh car;
        public Vector3 car_Scale = new Vector3(0.5f, 0.5f, 0.5f);
        private F1Circuit circuito;

        public Vector3 desf = new Vector3(0, 20, 0); // 40
        public Vector3 dir;
        public float dist_cam = 13; //130;
        private Effect effect;
        public float ftime; // frame time
        private Surface g_pDepthStencil; // Depth-stencil buffer
        private Texture g_pRenderTarget, g_pRenderTarget2, g_pRenderTarget3, g_pRenderTarget4, g_pRenderTarget5;

        private VertexBuffer g_pVBV3D;
        public bool mouseCaptured;
        public Point mouseCenter;
        private string MyShaderDir;

        public bool paused;
        public Vector3 pos;
        public float rotationSpeed = 0.1f;
        private TgcSkyBox skyBox;

        private TgcSimpleTerrain terrain;
        public float vel = 100f;

        public OutRun(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Workshop-OutRun";
            Description = "OutRun Circuit Demo";
        }

        public override void Init()
        {
            var d3dDevice = D3DDevice.Instance.Device;

            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            circuito = new F1Circuit(MediaDir);

            //Cargar terreno: cargar heightmap y textura de color
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(MediaDir + "Heighmaps\\" + "TerrainTexture2.jpg",
                20, 0.1f, new Vector3(0, -125, 0));
            terrain.loadTexture(MediaDir + "Heighmaps\\" + "TerrainTexture2.jpg");

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.updateValues();

            var loader = new TgcSceneLoader();
            var scene = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Auto\\Auto-TgcScene.xml");
            car = scene.Meshes[0];
            car.AutoTransformEnable = false;

            //Cargar Shader personalizado
            string compilationErrors;
            effect = Effect.FromFile(D3DDevice.Instance.Device, MyShaderDir + "OutRun.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";

            //Configurar FPS Camara
            Camara = new TgcFpsCamera(new Vector3(315.451f, 40, -464.28490f), 250f, 50f);

            reset_pos();

            // para capturar el mouse
            var focusWindows = D3DDevice.Instance.Device.CreationParameters.FocusWindow;
            mouseCenter = focusWindows.PointToScreen(new Point(focusWindows.Width / 2, focusWindows.Height / 2));
            mouseCaptured = true;
            Cursor.Hide();

            // stencil
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                d3dDevice.PresentationParameters.BackBufferHeight,
                DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pRenderTarget2 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pRenderTarget3 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);
            g_pRenderTarget5 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                Format.X8R8G8B8, Pool.Default);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices =
            {
                new CustomVertex.PositionTextured(-1, 1, 1, 0, 0),
                new CustomVertex.PositionTextured(1, 1, 1, 1, 0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0, 1),
                new CustomVertex.PositionTextured(1, -1, 1, 1, 1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);
        }

        public void reset_pos()
        {
            // pongo el auto en el primer tramo de la ruta
            pos = circuito.pt_ruta[40];
            dir = circuito.pt_ruta[41] - pos;
            pos.Y += 15;
            dir.Y = 0;
            dir.Normalize();
        }

        public override void Update()
        {
            var d3dInput = TgcD3dInput.Instance;

            if (d3dInput.keyPressed(Key.F1))
                paused = !paused;

            if (paused)
                return;

            if (d3dInput.keyPressed(Key.M))
            {
                mouseCaptured = !mouseCaptured;
                if (mouseCaptured)
                    Cursor.Hide();
                else
                    Cursor.Show();
            }

            vel += d3dInput.WheelPos * acel_mouse_wheel;

            if (mouseCaptured && d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //float pitch = d3dInput.YposRelative * rotationSpeed;
                var heading = d3dInput.XposRelative * rotationSpeed;
                dir = rotar_xz(dir, -heading);
            }

            if (mouseCaptured)
                Cursor.Position = mouseCenter;

            // aplico la velocidad
            pos = pos + dir * vel * ElapsedTime;

            // verifico si sigue en la ruta
            var H = circuito.updatePos(pos.X, pos.Z);
            if (!circuito.en_ruta)
            {
                // lo mando de nuevo a la ruta
                pos = circuito.que_pos_buena(pos.X, pos.Z);
            }

            // mantengo el auto en la ruta
            pos.Y = H;

            // actualizo la posiicon del auto
            car.Position = pos;
            car.Transform = CalcularMatriz(pos, car_Scale, -dir);

            // actualizo la camara
            Camara.setCamera(pos - dir * dist_cam + desf, pos + desf);
            //this.Camara.setCamera(new Vector3(500, 4000, 500), new Vector3(0, 0, 0));
            Camara.updateCamera(ElapsedTime);
        }

        public override void Render()
        {
            base.Render();

            Update();

            var device = D3DDevice.Instance.Device;
            effect.Technique = "DefaultTechnique";

            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = device.GetRenderTarget(0);
            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            IniciarEscena();

            // -------------------------------------
            //Renderizar terreno
            terrain.Effect = effect;
            terrain.Technique = "DefaultTechnique";
            terrain.render();
            // skybox
            skyBox.render();
            //Renderizar pista
            circuito.render(effect);
            //Renderizar auto
            car.Effect = effect;
            car.Technique = "DefaultTechnique";
            car.render();
            // -------------------------------------

            FinalizarEscena();

            pSurf.Dispose();

            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.SetRenderTarget(0, pOldRT);
            IniciarEscena();

            effect.Technique = "FrameMotionBlur";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_RenderTarget2", g_pRenderTarget2);
            effect.SetValue("g_RenderTarget3", g_pRenderTarget3);
            effect.SetValue("g_RenderTarget4", g_pRenderTarget4);
            effect.SetValue("g_RenderTarget5", g_pRenderTarget5);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();

            //TgcDrawText.Instance.drawText("Pos: " + this.Camara.Position, 0, 0, Color.Yellow);
            //TgcDrawText.Instance.drawText("Look At: " + CamaraManager.Instance.CurrentCamera.getLookAt(), 500, 0, Color.Yellow);

            if (circuito.en_ruta)
                TgcDrawText.Instance.drawText("Tramo:" + circuito.pos_en_ruta, 0, 0, Color.Yellow);

            //TgcDrawText.Instance.drawText("dist_cam:" + dist_cam + "defY" + desf.Y, 0, 0, Color.Yellow);
            //TgcDrawText.Instance.drawText("vel:" + vel, 0, 0, Color.Yellow);

            FinalizarEscena();

            ftime += ElapsedTime;
            if (ftime > 0.03f)
            {
                ftime = 0;
                var aux = g_pRenderTarget5;
                g_pRenderTarget5 = g_pRenderTarget4;
                g_pRenderTarget4 = g_pRenderTarget3;
                g_pRenderTarget3 = g_pRenderTarget2;
                g_pRenderTarget2 = g_pRenderTarget;
                g_pRenderTarget = aux;
            }
        }

        public override void Close()
        {
            base.Close();

            circuito.dispose();
            car.dispose();
            effect.Dispose();
            terrain.dispose();
            skyBox.dispose();

            g_pRenderTarget.Dispose();
            g_pRenderTarget2.Dispose();
            g_pRenderTarget3.Dispose();
            g_pRenderTarget4.Dispose();
            g_pRenderTarget5.Dispose();
            g_pDepthStencil.Dispose();
            g_pVBV3D.Dispose();
        }

        // helper
        public Matrix CalcularMatriz(Vector3 Pos, Vector3 Scale, Vector3 Dir)
        {
            var VUP = new Vector3(0, 1, 0);

            var matWorld = Matrix.Scaling(Scale);
            // determino la orientacion
            var U = Vector3.Cross(VUP, Dir);
            U.Normalize();
            var V = Vector3.Cross(Dir, U);
            Matrix Orientacion;
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.Translation(Pos);
            return matWorld;
        }

        public Vector3 rotar_xz(Vector3 v, float an)
        {
            return new Vector3((float)(v.X * System.Math.Cos(an) - v.Z * System.Math.Sin(an)), v.Y,
                (float)(v.X * System.Math.Sin(an) + v.Z * System.Math.Cos(an)));
        }
    }
}