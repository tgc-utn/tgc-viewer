using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    /// <summary>
    ///     Ejemplo EnvMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    ///     Demo general que integra diversos efectos de shaders.
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class DemoShaders : TGCExampleViewer
    {
        private readonly float far_plane = 10000f;
        private readonly float near_plane = 1f;

        // Shadow map
        private readonly int SHADOWMAP_SIZE = 512;

        private float alfa_sol; // pos. del sol
        private float an_tanque; // angulo actual del tanque
        private TgcArrow arrow;
        private List<TgcMesh> bosque;

        private bool camara_rot;
        private TgcRotationalCamera CamaraRot;
        private int cant_palmeras; // sin contar la isla
        private TgcRotationalCamera DefaultCamera;
        private TGCVector3 dir_canoa;
        private Effect effect;
        private TGCVector3 g_LightDir; // direccion de la luz actual
        private TGCVector3 g_LightPos; // posicion de la luz actual (la que estoy analizando)
        private TGCMatrix g_LightView; // matriz de view del light
        private TGCMatrix g_mShadowProj; // Projection matrix for shadow map
        private CubeTexture g_pCubeMapAgua;
        private Surface g_pDSShadow; // Depth-stencil buffer for rendering to shadow map

        private Texture g_pShadowMap; // Texture to which the shadow map is rendered
        private float largo_tanque, alto_tanque;
        private TGCVector3 LookFrom, LookAt;
        private TgcMesh mesh, piso;
        private float nivel_mar;
        private TgcMesh palmera, canoa;
        private TgcScene scene, scene2, scene3, scene4;

        private TgcSkyBox skyBox;

        // enviroment map
        private MySimpleTerrain terrain;

        private float time;
        // no quiero que la isla entre en el env.map

        // modo demo
        private float timer_preview;

        private int tipo_vista, ant_vista;
        private float vel_tanque;
        private Viewport View1, View2, ViewF;

        public DemoShaders(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel y Vertex Shaders";
            Name = "Demo Shaders";
            Description =
                "Demostracion de distintos Effectos Vs Fixed Pipeline. C->Camara, F->Fixed Pipeline, D->Dos Vistas al mismo tiempo [SPACE]->Parar/Arrancar Tanque";
        }

        public override void Init()
        {
            //Crear loader
            var loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            // Creo el Heightmap para el terreno:
            terrain = new MySimpleTerrain();
            terrain.loadHeightmap(MediaDir + "Heighmaps\\Heightmap3.jpg", 100f, 1f,
                TGCVector3.Empty);
            terrain.loadTexture(MediaDir + "Heighmaps\\TerrainTexture3.jpg");

            // ------------------------------------------------------------
            // Crear SkyBox:
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(8000, 8000, 8000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox1\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            skyBox.SkyEpsilon = 50f;
            skyBox.Init();

            // ------------------------------------------------------------
            //Cargar los mesh:
            scene =
                loader.loadSceneFromFile(MediaDir +
                                         "MeshCreator\\Meshes\\Vehiculos\\TanqueFuturistaRuedas\\TanqueFuturistaRuedas-TgcScene.xml");
            mesh = scene.Meshes[0];

            scene2 =
                loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmera = scene2.Meshes[0];

            scene3 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\Canoa\\Canoa-TgcScene.xml");
            canoa = scene3.Meshes[0];

            scene4 = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Piso\\Agua-TgcScene.xml");
            piso = scene4.Meshes[0];

            mesh.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);
            mesh.Position = TGCVector3.Empty;
            var size = mesh.BoundingBox.calculateSize();
            largo_tanque = Math.Abs(size.Z);
            alto_tanque = Math.Abs(size.Y) * mesh.Scale.Y;
            vel_tanque = 10;
            an_tanque = 0;
            canoa.Scale = TGCVector3.One;
            canoa.Position = new TGCVector3(3000f, 550f, 0f);
            dir_canoa = new TGCVector3(0, 0, 1);
            nivel_mar = 135f;
            piso.Scale = new TGCVector3(25f, 1f, 25f);
            piso.Position = new TGCVector3(0f, nivel_mar, 0f);

            size = palmera.BoundingBox.calculateSize();
            var alto_palmera = Math.Abs(size.Y);
            cant_palmeras = 0;
            int i;
            bosque = new List<TgcMesh>();
            float[] r = { 1850f, 2100f, 2300f, 1800f };
            for (i = 0; i < 4; i++)
                for (var j = 0; j < 15; j++)
                {
                    var instance = palmera.createMeshInstance(palmera.Name + i);
                    instance.Scale = new TGCVector3(0.5f, 1.5f, 0.5f);
                    var x = r[i] * (float)Math.Cos(Geometry.DegreeToRadian(100 + 10.0f * j));
                    var z = r[i] * (float)Math.Sin(Geometry.DegreeToRadian(100 + 10.0f * j));
                    instance.Position = new TGCVector3(x, terrain.CalcularAltura(x, z)
                        /*+ alto_palmera / 2 * instance.Scale.Y*/, z);
                    bosque.Add(instance);
                    ++cant_palmeras;
                }

            // segunda parte: la isla del medio
            // estas no entran en el env. map (porque se supone que el env. map esta lejos
            // del pto de vista del observador y estas palmeras estan en el medio del lago)
            float[] r2 = { 200f, 350f, 400f, 477f };
            for (i = 0; i < 4; i++)
                for (var j = 0; j < 5; j++)
                {
                    var instance = palmera.createMeshInstance(palmera.Name + i);
                    instance.Scale = new TGCVector3(0.5f, 1f + j / 5f * 0.33f, 0.5f);
                    var x = r2[i] * (float)Math.Cos(Geometry.DegreeToRadian(25.0f * j));
                    var z = r2[i] * (float)Math.Sin(Geometry.DegreeToRadian(25.0f * j));
                    instance.Position = new TGCVector3(x, terrain.CalcularAltura(x, z)
                        /*+ alto_palmera / 2 * instance.Scale.Y*/, z);
                    bosque.Add(instance);
                }

            // Arreglo las normales del tanque
            /*int[] adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);
             */

            g_pCubeMapAgua = null;

            //Cargar Shader personalizado
            effect =
                TGCShaders.Instance.LoadEffect(ShadersDir + "WorkshopShaders\\Demo.fx");

            // le asigno el efecto a las mallas
            mesh.Effect = effect;
            mesh.Technique = "RenderScene";
            piso.Effect = effect;
            piso.Technique = "RenderScene";
            palmera.Effect = effect;
            palmera.Technique = "RenderScene";
            canoa.Effect = effect;
            canoa.Technique = "RenderScene";

            //--------------------------------------------------------------------------------------
            // Creo el shadowmap.
            // Format.R32F
            // Format.X8R8G8B8
            g_pShadowMap = new Texture(D3DDevice.Instance.Device, SHADOWMAP_SIZE, SHADOWMAP_SIZE,
                1, Usage.RenderTarget, Format.R32F,
                Pool.Default);

            // tengo que crear un stencilbuffer para el shadowmap manualmente
            // para asegurarme que tenga la el mismo tamano que el shadowmap, y que no tenga
            // multisample, etc etc.
            g_pDSShadow = D3DDevice.Instance.Device.CreateDepthStencilSurface(SHADOWMAP_SIZE,
                SHADOWMAP_SIZE,
                DepthFormat.D24S8,
                MultiSampleType.None,
                0,
                true);
            // por ultimo necesito una matriz de proyeccion para el shadowmap, ya
            // que voy a dibujar desde el pto de vista de la luz.
            // El angulo tiene que ser mayor a 45 para que la sombra no falle en los extremos del cono de luz
            // de hecho, un valor mayor a 90 todavia es mejor, porque hasta con 90 grados es muy dificil
            // lograr que los objetos del borde generen sombras
            var aspectRatio = D3DDevice.Instance.AspectRatio;
            g_mShadowProj = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(130.0f), aspectRatio, near_plane, far_plane);
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), aspectRatio, near_plane, far_plane).ToMatrix();

            alfa_sol = 1.7f;
            //alfa_sol = 0;

            //--------------------------------------------------------------------------------------
            //Centrar camara rotacional respecto a este mesh
            camara_rot = false;
            CamaraRot = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(),
                mesh.BoundingBox.calculateBoxRadius() * 2, Input);
            CamaraRot.CameraDistance = 300;
            CamaraRot.RotationSpeed = 1.5f;
            DefaultCamera = new TgcRotationalCamera(new TGCVector3(0, 200, 0), 5000, 0.1f, 1f, Input);
            Camara = DefaultCamera;

            LookFrom = new TGCVector3(0, 400, 2000);
            LookAt = new TGCVector3(0, 200, 0);

            // inicio unos segundos de preview
            timer_preview = 50;

            arrow = new TgcArrow();
            arrow.Thickness = 1f;
            arrow.HeadSize = new TGCVector2(2f, 2f);
            arrow.BodyColor = Color.Blue;

            ant_vista = tipo_vista = 0;
            View1 = new Viewport();
            View1.X = 0;
            View1.Y = 0;
            View1.Width = D3DDevice.Instance.Width;
            View1.Height = D3DDevice.Instance.Height / 2;
            View1.MinZ = 0;
            View1.MaxZ = 1;
            View2 = new Viewport();
            View2.X = 0;
            View2.Y = View1.Height;
            View2.Width = D3DDevice.Instance.Width;
            View2.Height = D3DDevice.Instance.Height / 2;
            View2.MinZ = 0;
            View2.MaxZ = 1;

            ViewF = D3DDevice.Instance.Device.Viewport;
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            ClearTextures();
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();

            var aspectRatio = D3DDevice.Instance.AspectRatio;

            time += ElapsedTime;

            if (Input.keyPressed(Key.C))
            {
                timer_preview = 0;
                camara_rot = !camara_rot;
            }

            if (Input.keyPressed(Key.F))
            {
                if (tipo_vista == 1)
                    tipo_vista = 0;
                else
                    tipo_vista = 1;
                ant_vista = tipo_vista;
            }

            if (Input.keyPressed(Key.D))
            {
                if (tipo_vista == 2)
                    tipo_vista = ant_vista;
                else
                    tipo_vista = 2;
            }

            if (Input.keyPressed(Key.Space))
            {
                if (vel_tanque <= 1)
                    vel_tanque = 10;
                else
                    vel_tanque = 1;
            }

            if (timer_preview > 0)
            {
                timer_preview -= ElapsedTime;
                if (timer_preview < 0)
                    timer_preview = 0;
            }

            // animar tanque
            an_tanque -= ElapsedTime * Geometry.DegreeToRadian(vel_tanque);
            var alfa = an_tanque;
            var x0 = 2000f * (float)Math.Cos(alfa);
            var z0 = 2000f * (float)Math.Sin(alfa);
            float offset_rueda = 10;
            var H = terrain.CalcularAltura(x0, z0) + alto_tanque / 2 - offset_rueda;
            if (H < nivel_mar)
                H = nivel_mar;
            mesh.Position = new TGCVector3(x0, H, z0);
            // direccion tangente sobre el piso:
            var dir_tanque = new TGCVector2(-(float)Math.Sin(alfa), (float)Math.Cos(alfa));
            dir_tanque.Normalize();
            // Posicion de la parte de adelante del tanque
            var pos2d = new TGCVector2(x0, z0);
            pos2d = pos2d + dir_tanque * (largo_tanque / 2);
            var H_frente = terrain.CalcularAltura(pos2d.X, pos2d.Y) + alto_tanque / 2 - offset_rueda;
            if (H_frente < nivel_mar - 15)
                H_frente = nivel_mar - 15;
            var pos_frente = new TGCVector3(pos2d.X, H_frente, pos2d.Y);
            var Vel = pos_frente - mesh.Position;
            Vel.Normalize();
            mesh.Transform = CalcularMatriz(mesh.Position, mesh.Scale, Vel);

            // animo la canoa en circulos:
            alfa = -time * Geometry.DegreeToRadian(10.0f);
            x0 = 400f * (float)Math.Cos(alfa);
            z0 = 400f * (float)Math.Sin(alfa);
            canoa.Position = new TGCVector3(x0, 150, z0);
            dir_canoa = new TGCVector3(-(float)Math.Sin(alfa), 0, (float)Math.Cos(alfa));
            canoa.Transform = CalcularMatriz(canoa.Position, canoa.Scale, dir_canoa);

            alfa_sol += ElapsedTime * Geometry.DegreeToRadian(1.0f);
            if (alfa_sol > 2.5)
                alfa_sol = 1.5f;
            // animo la posicion del sol
            //g_LightPos = new TGCVector3(1500f * (float)Math.Cos(alfa_sol), 1500f * (float)Math.Sin(alfa_sol), 0f);
            g_LightPos = new TGCVector3(2000f * (float)Math.Cos(alfa_sol), 2000f * (float)Math.Sin(alfa_sol),
                0f);
            g_LightDir = -g_LightPos;
            g_LightDir.Normalize();

            if (timer_preview > 0)
            {
                var an = -time * Geometry.DegreeToRadian(10.0f);
                LookFrom.X = 1500f * (float)Math.Sin(an);
                LookFrom.Z = 1500f * (float)Math.Cos(an);
            }
            else
            {
                if (camara_rot)
                {
                    CamaraRot.CameraCenter = mesh.BoundingBox.calculateBoxCenter();
                    CamaraRot.UpdateCamera(ElapsedTime); //FIXME, puede que no haga falta esto.
                    Camara = CamaraRot;
                }
                else
                {
                    Camara = DefaultCamera;
                }
            }

            // --------------------------------------------------------------------
            D3DDevice.Instance.Device.EndScene();
            if (g_pCubeMapAgua == null)
            {
                // solo la primera vez crea el env map del agua
                CrearEnvMapAgua();
                // ya que esta creado, se lo asigno al effecto:
                effect.SetValue("g_txCubeMapAgua", g_pCubeMapAgua);
            }

            // Creo el env map del tanque:
            var g_pCubeMap = new CubeTexture(D3DDevice.Instance.Device, 256, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, near_plane, far_plane).ToMatrix();

            // Genero las caras del enviroment map
            for (var nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                var pFace = g_pCubeMap.GetCubeMapSurface(nFace, 0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pFace);
                TGCVector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new TGCVector3(1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Black;
                        break;

                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new TGCVector3(-1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Red;
                        break;

                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = TGCVector3.Up;
                        VUP = new TGCVector3(0, 0, -1);
                        color = Color.Gray;
                        break;

                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = TGCVector3.Down;
                        VUP = new TGCVector3(0, 0, 1);
                        color = Color.Yellow;
                        break;

                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new TGCVector3(0, 0, 1);
                        VUP = TGCVector3.Up;
                        color = Color.Green;
                        break;

                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new TGCVector3(0, 0, -1);
                        VUP = TGCVector3.Up;
                        color = Color.Blue;
                        break;
                }

                //Obtener ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
                var Pos = mesh.Position;
                D3DDevice.Instance.Device.Transform.View = TGCMatrix.LookAtLH(Pos, Pos + Dir, VUP).ToMatrix();

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                D3DDevice.Instance.Device.BeginScene();

                //Renderizar
                renderScene(ElapsedTime, true);

                D3DDevice.Instance.Device.EndScene();
                //string fname = string.Format("face{0:D}.bmp", nFace);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pFace);
            }
            // restuaro el render target
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            //TextureLoader.Save("test.bmp", ImageFileFormat.Bmp, g_pCubeMap);

            //Genero el shadow map
            RenderShadowMap();

            // Restauro el estado de las transformaciones
            if (timer_preview > 0)
                D3DDevice.Instance.Device.Transform.View = TGCMatrix.LookAtLH(LookFrom, LookAt, TGCVector3.Up).ToMatrix();
            else
                D3DDevice.Instance.Device.Transform.View = Camara.GetViewMatrix().ToMatrix();
            // FIXME! esto no se bien para que lo hace aca.

            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f), aspectRatio, near_plane, far_plane).ToMatrix();

            // Cargo las var. del shader:
            effect.SetValue("g_txCubeMap", g_pCubeMap);
            effect.SetValue("fvLightPosition", new TGCVector4(0, 400, 0, 0));
            effect.SetValue("fvEyePosition", TGCVector3.TGCVector3ToFloat3Array(timer_preview > 0 ? LookFrom : Camara.Position));
            effect.SetValue("time", time);

            // -----------------------------------------------------
            // dibujo la escena pp dicha:
            D3DDevice.Instance.Device.BeginScene();

            if (tipo_vista != 1)
            {
                // con shaders :
                if (tipo_vista == 2)
                    // dibujo en una vista:
                    D3DDevice.Instance.Device.Viewport = View1;
                else
                    // dibujo en la pantalla completa
                    D3DDevice.Instance.Device.Viewport = ViewF;

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                // 1ero sin el agua
                renderScene(ElapsedTime, false);

                // Ahora dibujo el agua
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
                effect.SetValue("aux_Tex", terrain.terrainTexture);
                // posicion de la canoa (divido por la escala)
                effect.SetValue("canoa_x", x0 / 10.0f);
                effect.SetValue("canoa_y", z0 / 10.0f);
                piso.Technique = "RenderAgua";
                piso.Render();
            }

            if (tipo_vista != 0)
            {
                // sin shaders
                if (tipo_vista == 2)
                    // dibujo en una vista:
                    D3DDevice.Instance.Device.Viewport = View2;
                else
                    // dibujo en la pantalla completa
                    D3DDevice.Instance.Device.Viewport = ViewF;

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                //Renderizar terreno
                terrain.render();
                //Renderizar SkyBox
                skyBox.Render();
                // dibujo el bosque
                foreach (var instance in bosque)
                {
                    instance.UpdateMeshTransform();
                    instance.Render();
                }
                // canoa
                canoa.Render();
                // tanque
                mesh.Render();
                // agua
                var ant_src = D3DDevice.Instance.Device.RenderState.SourceBlend;
                var ant_dest = D3DDevice.Instance.Device.RenderState.DestinationBlend;
                var ant_alpha = D3DDevice.Instance.Device.RenderState.AlphaBlendEnable;
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
                D3DDevice.Instance.Device.RenderState.SourceBlend = Blend.SourceColor;
                D3DDevice.Instance.Device.RenderState.DestinationBlend = Blend.InvSourceColor;
                piso.Render();
                D3DDevice.Instance.Device.RenderState.SourceBlend = ant_src;
                D3DDevice.Instance.Device.RenderState.DestinationBlend = ant_dest;
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = ant_alpha;
            }

            g_pCubeMap.Dispose();

            RenderFPS();
            RenderAxis();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public void renderScene(float elapsedTime, bool cubemap)
        {
            //Renderizar terreno
            if (!cubemap)
            {
                effect.Technique = "RenderSceneShadows";
                terrain.executeRender(effect);
            }
            else
                terrain.render();

            //Renderizar SkyBox
            skyBox.Render();

            // dibujo el bosque
            var total = cubemap ? cant_palmeras : bosque.Count;
            for (var i = 0; i < total; ++i)
            {
                bosque[i].UpdateMeshTransform();
                bosque[i].Render();
            }

            // canoa
            canoa.Render();

            if (!cubemap)
            {
                // dibujo el mesh
                mesh.Technique = "RenderScene";
                mesh.Render();
            }
        }

        public void CrearEnvMapAgua()
        {
            // creo el enviroment map para el agua
            g_pCubeMapAgua = new CubeTexture(D3DDevice.Instance.Device, 256, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            D3DDevice.Instance.Device.Transform.Projection = TGCMatrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f), 1f, near_plane, far_plane).ToMatrix();
            // Genero las caras del enviroment map
            for (var nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                var pFace = g_pCubeMapAgua.GetCubeMapSurface(nFace, 0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pFace);
                TGCVector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new TGCVector3(1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Black;
                        break;

                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new TGCVector3(-1, 0, 0);
                        VUP = TGCVector3.Up;
                        color = Color.Red;
                        break;

                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = TGCVector3.Up;
                        VUP = new TGCVector3(0, 0, -1);
                        color = Color.Gray;
                        break;

                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = TGCVector3.Down;
                        VUP = new TGCVector3(0, 0, 1);
                        color = Color.Yellow;
                        break;

                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new TGCVector3(0, 0, 1);
                        VUP = TGCVector3.Up;
                        color = Color.Green;
                        break;

                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new TGCVector3(0, 0, -1);
                        VUP = TGCVector3.Up;
                        color = Color.Blue;
                        break;
                }

                var Pos = piso.Position;
                if (nFace == CubeMapFace.NegativeY)
                    Pos.Y += 2000;

                D3DDevice.Instance.Device.Transform.View = TGCMatrix.LookAtLH(Pos, Pos + Dir, VUP).ToMatrix();
                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                D3DDevice.Instance.Device.BeginScene();
                //Renderizar: solo algunas cosas:
                if (nFace == CubeMapFace.NegativeY)
                {
                    //Renderizar terreno
                    terrain.render();
                }
                else
                {
                    //Renderizar SkyBox
                    skyBox.Render();
                    // dibujo el bosque
                    foreach (var instance in bosque)
                    {
                        instance.UpdateMeshTransform();
                        instance.Render();
                    }
                }
                var fname = string.Format("face{0:D}.bmp", nFace);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pFace);

                D3DDevice.Instance.Device.EndScene();
            }
            // restuaro el render target
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
        }

        public void RenderShadowMap()
        {
            //Doy posicion a la luz
            // Calculo la matriz de view de la luz
            effect.SetValue("g_vLightPos", new TGCVector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            effect.SetValue("g_vLightDir", new TGCVector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = TGCMatrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new TGCVector3(0, 0, 1));

            // inicializacion standard:
            effect.SetValue("g_mProjLight", g_mShadowProj.ToMatrix());
            effect.SetValue("g_mViewLightProj", (g_LightView * g_mShadowProj).ToMatrix());

            // Primero genero el shadow map, para ello dibujo desde el pto de vista de luz
            // a una textura, con el VS y PS que generan un mapa de profundidades.
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pShadowSurf = g_pShadowMap.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pShadowSurf);
            var pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            D3DDevice.Instance.Device.DepthStencilSurface = g_pDSShadow;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();

            // Hago el render de la escena pp dicha
            // solo los objetos que proyectan sombras:
            //Renderizar terreno
            terrain.executeRender(effect);
            // dibujo el bosque
            foreach (var instance in bosque)
            {
                instance.UpdateMeshTransform();
                instance.Render();
            }

            // el tanque
            // Seteo la tecnica: estoy generando la sombra o estoy dibujando la escena
            mesh.Technique = "RenderShadow";
            mesh.Render();
            // Termino
            D3DDevice.Instance.Device.EndScene();
            //TextureLoader.Save("shadowmap.bmp", ImageFileFormat.Bmp, g_pShadowMap);

            // restuaro el render target y el stencil
            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);

            effect.SetValue("g_txShadow", g_pShadowMap);
        }

        // helper
        public TGCMatrix CalcularMatriz(TGCVector3 Pos, TGCVector3 Scale, TGCVector3 Dir)
        {
            var VUP = TGCVector3.Up;

            var matWorld = TGCMatrix.Scaling(Scale);
            // determino la orientacion
            var U = TGCVector3.Cross(VUP, Dir);
            U.Normalize();
            var V = TGCVector3.Cross(Dir, U);
            TGCMatrix Orientacion = new TGCMatrix();
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
            matWorld = matWorld * TGCMatrix.Translation(Pos);
            return matWorld;
        }

        public TGCMatrix CalcularMatrizUp(TGCVector3 Pos, TGCVector3 Scale, TGCVector3 Dir, TGCVector3 VUP)
        {
            var matWorld = TGCMatrix.Scaling(Scale);
            // determino la orientacion
            var U = TGCVector3.Cross(VUP, Dir);
            U.Normalize();
            var V = TGCVector3.Cross(Dir, U);
            TGCMatrix Orientacion = new TGCMatrix();
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
            matWorld = matWorld * TGCMatrix.Translation(Pos);
            return matWorld;
        }

        public override void Dispose()
        {
            effect.Dispose();
            scene.DisposeAll();
            scene2.DisposeAll();
            scene3.DisposeAll();
            scene4.DisposeAll();
            terrain.dispose();
            g_pCubeMapAgua.Dispose();
            g_pShadowMap.Dispose();
            g_pDSShadow.Dispose();
        }
    }
}
