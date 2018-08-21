using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Example;

namespace Examples.Shaders
{
    public class EjemploGPUCompute : TGCExampleViewer
    {
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene;
        private TgcMesh mesh;
        private TgcArrow arrow, arrowN, arrowT;
        private Effect effect;

        private Surface g_pDepthStencil;     // Depth-stencil buffer
        private Texture g_pBaseTexture;
        private Texture g_pHeightmap;
        private Texture g_pVelocidad;
        private Texture g_pVelocidadOut;
        private Texture g_pPos;
        private Texture g_pPosOut;

        private Texture g_pTempVel, g_pTempPos;
        private VertexBuffer g_pVBV3D, g_pVB;
        private Surface pSurf;

        private TGCVector3 LookAt, LookFrom;

        // enviroment map
        private TgcSimpleTerrain terrain;

        private string currentHeightmap;
        private string currentTexture;
        private float currentScaleXZ;
        private float currentScaleY;

        private float time;

        private float esfera_radio;
        private static int MAX_DS = 512;
        private float[,] vel_x;
        private float[,] vel_z;
        private float[,] pos_x;
        private float[,] pos_z;
        private float[,] pos_y;

        public EjemploGPUCompute(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-GPUCompute";
            Description = "GPUCompute";
        }

        public unsafe override void Init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            GuiController.Instance.CustomRenderEnabled = true;

            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            time = 0f;
            vel_x = new float[MAX_DS, MAX_DS];
            vel_z = new float[MAX_DS, MAX_DS];
            pos_x = new float[MAX_DS, MAX_DS];
            pos_y = new float[MAX_DS, MAX_DS];
            pos_z = new float[MAX_DS, MAX_DS];

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            //Path de Heightmap default del terreno y Path de Textura default del terreno
            TGCVector3 PosTerrain = new TGCVector3(0, 0, 0);
            currentHeightmap = MyMediaDir + "Heighmaps\\" + "Heightmap2.jpg";
            currentScaleXZ = 100f;
            currentScaleY = 6f;
            currentTexture = MyMediaDir + "Heighmaps\\" + "Heightmap2.JPG";         //+ "grid.JPG";
            terrain = new TgcSimpleTerrain();
            terrain.loadHeightmap(currentHeightmap, currentScaleXZ, currentScaleY, PosTerrain);
            terrain.loadTexture(currentTexture);
            // tomo el ancho de la textura, ojo tiene que ser cuadrada
            float terrain_width = (float)terrain.HeightmapData.GetLength(0);

            // mesh principal
            scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Sphere\\Sphere-TgcScene.xml");
            Bitmap b = (Bitmap)Bitmap.FromFile(MyMediaDir + "Heighmaps\\grid.jpg");
            g_pBaseTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
            b.Dispose();
            mesh = scene.Meshes[0];
            mesh.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
            mesh.Position = new TGCVector3(0f, 0f, 0f);
            mesh.AutoTransformEnable = false;
            TGCVector3 size = mesh.BoundingBox.calculateSize();
            esfera_radio = Math.Abs(size.Y) / 2;

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "GPUCompute.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique
            effect.Technique = "DefaultTechnique";
            effect.SetValue("map_size", terrain_width);
            effect.SetValue("map_desf", 0.5f / terrain_width);

            arrow = new TgcArrow();
            arrow.Thickness = 1f;
            arrow.HeadSize = new TGCVector2(2f, 2f);
            arrow.BodyColor = Color.Blue;
            arrowN = new TgcArrow();
            arrowN.Thickness = 1f;
            arrowN.HeadSize = new TGCVector2(2f, 2f);
            arrowN.BodyColor = Color.Red;
            arrowT = new TgcArrow();
            arrowT.Thickness = 1f;
            arrowT.HeadSize = new TGCVector2(2f, 2f);
            arrowT.BodyColor = Color.Green;

            GuiController.Instance.RotCamera.CameraCenter = new TGCVector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 3200;
            GuiController.Instance.RotCamera.RotationSpeed = 2f;

            LookAt = new TGCVector3(0, 0, 0);
            LookFrom = new TGCVector3(3200, 3000, 3200);

            float aspectRatio = (float)GuiController.Instance.Panel3d.Width / GuiController.Instance.Panel3d.Height;
            GuiController.Instance.CurrentCamera.updateCamera();
            d3dDevice.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), aspectRatio, 5f, 40000f);

            // Creo el mapa de velocidad
            g_pVelocidad = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);
            g_pVelocidadOut = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);
            // Mapa de Posicion
            g_pPos = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);
            g_pPosOut = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);

            // stencil compatible sin multisampling
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(MAX_DS, MAX_DS, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            // temporaria para recuperar los valores
            g_pTempVel = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, 0, Format.A32B32G32R32F, Pool.SystemMemory);
            g_pTempPos = new Texture(d3dDevice, MAX_DS, MAX_DS, 1, 0, Format.A32B32G32R32F, Pool.SystemMemory);

            effect.SetValue("g_pVelocidad", g_pVelocidad);
            effect.SetValue("g_pPos", g_pPos);
            // Textura del heigmap
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, currentHeightmap);
            effect.SetValue("height_map", g_pHeightmap);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            effect.SetValue("currentScaleXZ", currentScaleXZ);
            effect.SetValue("currentScaleY", currentScaleY);

            //Se crean 2 triangulos con las dimensiones de la pantalla con sus posiciones ya transformadas
            // x = -1 es el extremo izquiedo de la pantalla, x=1 es el extremo derecho
            // Lo mismo para la Y con arriba y abajo
            // la Z en 1 simpre
            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
            {
                new CustomVertex.PositionTextured( -1, 1, 1, 0,0),
                new CustomVertex.PositionTextured(1,  1, 1, 1,0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
                new CustomVertex.PositionTextured(1,-1, 1, 1,1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            g_pVB = new VertexBuffer(typeof(CustomVertex.PositionColored), MAX_DS * MAX_DS, d3dDevice, Usage.Dynamic | Usage.None, CustomVertex.PositionColored.Format, Pool.Default);

            // inicializo el mapa de velocidades
            Device device = GuiController.Instance.D3dDevice;
            TGCMatrix ant_Proj = device.Transform.Projection;
            TGCMatrix ant_World = device.Transform.World;
            TGCMatrix ant_View = device.Transform.View;
            device.Transform.Projection = TGCMatrix.Identity;
            device.Transform.World = TGCMatrix.Identity;
            device.Transform.View = TGCMatrix.Identity;

            // rt1 = velocidad
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pVelocidad.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;

            // rt2 = posicion
            Surface pSurf2 = g_pPos.GetSurfaceLevel(0);
            device.SetRenderTarget(1, pSurf2);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            effect.Technique = "ComputeVel";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            device.EndScene();

            effect.SetValue("Kp", esfera_radio * (float)Math.PI / 2);

            // restauro los RT
            device.SetRenderTarget(0, pOldRT);
            device.SetRenderTarget(1, null);
            device.DepthStencilSurface = pOldDS;
            // restauro las Transf.
            device.Transform.Projection = ant_Proj;
            device.Transform.World = ant_World;
            device.Transform.View = ant_View;

            Modifiers.addBoolean("dibujar_terreno", "Dibujar Terreno", true);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public unsafe override void Render()
        {
            Device device = GuiController.Instance.D3dDevice;

            time += ElapsedTime;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            //Obtener variacion XY del mouse
            if (d3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                float mouseX = d3dInput.XposRelative;
                float mouseY = d3dInput.YposRelative;
                float an = mouseX * 0.1f;

                float x = (float)(LookFrom.X * Math.Cos(an) + LookFrom.Z * Math.Sin(an));
                float z = (float)(LookFrom.Z * Math.Cos(an) - LookFrom.X * Math.Sin(an));
                LookFrom.X = x;
                LookFrom.Z = z;
                LookFrom.Y += mouseY * 150f;
            }

            //Determinar distancia de la camara o zoom segun el Mouse Wheel
            if (d3dInput.WheelPos != 0)
            {
                TGCVector3 vdir = LookFrom - LookAt;
                vdir.Normalize();
                LookFrom = LookFrom - vdir * (d3dInput.WheelPos * 500);
            }

            device.Transform.View = TGCMatrix.LookAtLH(LookFrom, LookAt, new TGCVector3(0, 1, 0));

            TGCMatrix ant_Proj = device.Transform.Projection;
            TGCMatrix ant_World = device.Transform.World;
            TGCMatrix ant_View = device.Transform.View;
            device.Transform.Projection = TGCMatrix.Identity;
            device.Transform.World = TGCMatrix.Identity;
            device.Transform.View = TGCMatrix.Identity;
            device.SetRenderState(RenderStates.AlphaBlendEnable, false);

            // rt1= velocidad
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pVelocidadOut.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;
            // rt2 = posicion
            Surface pSurf2 = g_pPosOut.GetSurfaceLevel(0);
            device.SetRenderTarget(1, pSurf2);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            effect.SetValue("elapsedTime", ElapsedTime);
            effect.Technique = "ComputeVel";
            effect.SetValue("g_pVelocidad", g_pVelocidad);
            effect.SetValue("g_pPos", g_pPos);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.Begin(FX.None);
            effect.BeginPass(1);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            device.EndScene();

            // leo los datos de la textura de velocidades
            // ----------------------------------------------------------------------
            Surface pDestSurf = g_pTempVel.GetSurfaceLevel(0);
            device.GetRenderTargetData(pSurf, pDestSurf);
            Surface pDestSurf2 = g_pTempPos.GetSurfaceLevel(0);
            device.GetRenderTargetData(pSurf2, pDestSurf2);

            float* pDataVel = (float*)pDestSurf.LockRectangle(LockFlags.None).InternalData.ToPointer();
            float* pDataPos = (float*)pDestSurf2.LockRectangle(LockFlags.None).InternalData.ToPointer();
            for (int i = 0; i < MAX_DS; i++)
            {
                for (int j = 0; j < MAX_DS; j++)
                {
                    vel_x[i, j] = *pDataVel++;
                    vel_z[i, j] = *pDataVel++;
                    pDataVel++;     // no usado
                    pDataVel++;     // no usado

                    pos_x[i, j] = *pDataPos++;
                    pos_z[i, j] = *pDataPos++;
                    pos_y[i, j] = *pDataPos++;
                    pDataPos++;     // no usado
                }
            }
            pDestSurf.UnlockRectangle();
            pDestSurf2.UnlockRectangle();

            pSurf.Dispose();
            pSurf2.Dispose();

            device.SetRenderTarget(0, pOldRT);
            device.SetRenderTarget(1, null);
            device.DepthStencilSurface = pOldDS;

            // swap de texturas
            Texture aux = g_pVelocidad;
            g_pVelocidad = g_pVelocidadOut;
            g_pVelocidadOut = aux;
            aux = g_pPos;
            g_pPos = g_pPosOut;
            g_pPosOut = aux;

            // dibujo pp dicho ----------------------------------------------
            device.Transform.Projection = ant_Proj;
            device.Transform.World = ant_World;
            device.Transform.View = ant_View;

            device.BeginScene();
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Renderizar terreno
            if ((bool)Modifiers["dibujar_terreno"])
                terrain.Render();

            // dibujo las particulas como point sprites
            mesh.Effect = effect;
            mesh.Technique = "DefaultTechnique";
            effect.SetValue("texDiffuseMap", g_pBaseTexture);

            CustomVertex.PositionColored[,] vertices = new CustomVertex.PositionColored[MAX_DS, MAX_DS];
            for (int i = 0; i < MAX_DS; i++)
            {
                for (int j = 0; j < MAX_DS; j++)
                {
                    float x0 = pos_x[i, j];
                    float z0 = pos_z[i, j];
                    float H = pos_y[i, j];
                    vertices[i, j] = new CustomVertex.PositionColored(x0, H + esfera_radio, z0, Color.Blue.ToArgb());
                }
            }
            g_pVB.SetData(vertices, 0, LockFlags.None);

            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.SetStreamSource(0, g_pVB, 0);
            device.SetTexture(0, null);
            device.SetRenderState(RenderStates.PointSize, 32);
            device.SetRenderState(RenderStates.PointScaleEnable, true);
            device.SetRenderState(RenderStates.PointSpriteEnable, true);
            device.DrawPrimitives(PrimitiveType.PointList, 0, MAX_DS * MAX_DS);
            device.EndScene();
        }

        public override void Dispose()
        {
            mesh.Dispose();
            effect.Dispose();
            terrain.Dispose();
            g_pDepthStencil.Dispose();
            g_pBaseTexture.Dispose();
            g_pVelocidad.Dispose();
            g_pVelocidadOut.Dispose();
            g_pPos.Dispose();
            g_pPosOut.Dispose();
            g_pTempPos.Dispose();
            g_pTempVel.Dispose();
            g_pHeightmap.Dispose();
            scene.disposeAll();
            g_pVB.Dispose();
            g_pVBV3D.Dispose();
        }
    }
}