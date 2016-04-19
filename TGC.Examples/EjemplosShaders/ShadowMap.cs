using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometries;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Util;

namespace TGC.Examples.EjemplosShaders
{
    /// <summary>
    ///     Ejemplo ShadowMap:
    ///     Unidades Involucradas:
    ///     # Unidad 8 - Adaptadores de Video - Shaders
    ///     Ejemplo avanzado. Ver primero ejemplo "Shaders/WorkshopShaders/BasicShader".
    ///     Muestra como generar efecto de sombras en tiempo real utilizando la técnicade ShadowMap.
    ///     Autor: Mariano Banquiero
    /// </summary>
    public class ShadowMap : TgcExample
    {
        private readonly float far_plane = 1500f;
        private readonly float near_plane = 2f;

        // Shadow map
        private readonly int SHADOWMAP_SIZE = 1024;

        private TgcArrow arrow;
        private TgcMesh avion;

        private Vector3 dir_avion;
        private Effect effect;
        private Vector3 g_LightDir; // direccion de la luz actual
        private Vector3 g_LightPos; // posicion de la luz actual (la que estoy analizando)
        private Matrix g_LightView; // matriz de view del light
        private Matrix g_mShadowProj; // Projection matrix for shadow map
        private Surface g_pDSShadow; // Depth-stencil buffer for rendering to shadow map

        private Texture g_pShadowMap; // Texture to which the shadow map is rendered
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene, scene2;
        private float time;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-ShadowMap";
        }

        public override string getDescription()
        {
            return "Image Space Shadows con Shadow Map";
        }

        public override void init()
        {
            MyMediaDir = GuiController.Instance.ExamplesMediaDir + "WorkshopShaders\\";
            MyShaderDir = GuiController.Instance.ShadersDir + "WorkshopShaders\\";

            //Crear loader
            var loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            //Cargar la escena
            scene = loader.loadSceneFromFile(MyMediaDir
                                             + "shadowTest\\ShadowTest-TgcScene.xml");

            scene2 = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir
                                              + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene2.Meshes[0];

            avion.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            avion.Position = new Vector3(100f, 100f, 0f);
            avion.AutoTransformEnable = false;
            dir_avion = new Vector3(0, 0, 1);

            //GuiController.Instance.RotCamera.CameraDistance = 600;
            //GuiController.Instance.RotCamera.RotationSpeed = 1.5f;

            //Cargar Shader personalizado
            effect =
                TgcShaders.loadEffect(MyShaderDir + "ShadowMap.fx");

            // le asigno el efecto a las mallas
            foreach (var T in scene.Meshes)
            {
                T.Scale = new Vector3(1f, 1f, 1f);
                T.Effect = effect;
            }
            avion.Effect = effect;

            //--------------------------------------------------------------------------------------
            // Creo el shadowmap.
            // Format.R32F
            // Format.X8R8G8B8
            g_pShadowMap = new Texture(D3DDevice.Instance.Device, SHADOWMAP_SIZE, SHADOWMAP_SIZE,
                1, Usage.RenderTarget, Format.R32F,
                Pool.Default);

            // tengo que crear un stencilbuffer para el shadowmap manualmente
            // para asegurarme que tenga la el mismo tamaño que el shadowmap, y que no tenga
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
            var panel3d = GuiController.Instance.Panel3d;
            var aspectRatio = panel3d.Width / (float)panel3d.Height;
            g_mShadowProj = Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(80),
                aspectRatio, 50, 5000);
            D3DDevice.Instance.Device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                    aspectRatio, near_plane, far_plane);

            arrow = new TgcArrow();
            arrow.Thickness = 1f;
            arrow.HeadSize = new Vector2(2f, 2f);
            arrow.BodyColor = Color.Blue;

            
            float K = 300;
            GuiController.Instance.Modifiers.addVertex3f("LightLookFrom", new Vector3(-K, -K, -K), new Vector3(K, K, K),
                new Vector3(80, 120, 0));
            GuiController.Instance.Modifiers.addVertex3f("LightLookAt", new Vector3(-K, -K, -K), new Vector3(K, K, K),
                new Vector3(0, 0, 0));

            GuiController.Instance.RotCamera.targetObject(scene.Meshes[0].BoundingBox);
            GuiController.Instance.RotCamera.CameraCenter = GuiController.Instance.RotCamera.CameraCenter + new Vector3(0, 50f, 0);
            //GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 150f, 0);
            GuiController.Instance.RotCamera.CameraDistance = 300;
            GuiController.Instance.RotCamera.RotationSpeed = 50f;
        }

        public override void render(float elapsedTime)
        {
            var panel3d = GuiController.Instance.Panel3d;
            var aspectRatio = panel3d.Width / (float)panel3d.Height;
            time += elapsedTime;
            // animo la pos del avion
            var alfa = -time * Geometry.DegreeToRadian(15.0f);
            avion.Position = new Vector3(80f * (float)Math.Cos(alfa), 40 - 20 * (float)Math.Sin(alfa),
                80f * (float)Math.Sin(alfa));
            dir_avion = new Vector3(-(float)Math.Sin(alfa), 0, (float)Math.Cos(alfa));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);

            g_LightPos = (Vector3)GuiController.Instance.Modifiers["LightLookFrom"];
            g_LightDir = (Vector3)GuiController.Instance.Modifiers["LightLookAt"] - g_LightPos;
            g_LightDir.Normalize();

            arrow.PStart = g_LightPos;
            arrow.PEnd = g_LightPos + g_LightDir * 20;

            // Shadow maps:
            D3DDevice.Instance.Device.EndScene(); // termino el thread anterior

            CamaraManager.Instance.CurrentCamera.updateCamera(elapsedTime);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Genero el shadow map
            RenderShadowMap();

            D3DDevice.Instance.Device.BeginScene();
            // dibujo la escena pp dicha
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            RenderScene(false);

            //Cargar valores de la flecha
            arrow.render();
        }

        public void RenderShadowMap()
        {
            // Calculo la matriz de view de la luz
            effect.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            effect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));

            arrow.PStart = g_LightPos;
            arrow.PEnd = g_LightPos + g_LightDir * 20f;
            arrow.updateValues();

            // inicializacion standard:
            effect.SetValue("g_mProjLight", g_mShadowProj);
            effect.SetValue("g_mViewLightProj", g_LightView * g_mShadowProj);

            // Primero genero el shadow map, para ello dibujo desde el pto de vista de luz
            // a una textura, con el VS y PS que generan un mapa de profundidades.
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pShadowSurf = g_pShadowMap.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pShadowSurf);
            var pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            D3DDevice.Instance.Device.DepthStencilSurface = g_pDSShadow;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();

            // Hago el render de la escena pp dicha
            effect.SetValue("g_txShadow", g_pShadowMap);
            RenderScene(true);

            // Termino
            D3DDevice.Instance.Device.EndScene();

            //TextureLoader.Save("shadowmap.bmp", ImageFileFormat.Bmp, g_pShadowMap);

            // restuaro el render target y el stencil
            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
        }

        public void RenderScene(bool shadow)
        {
            foreach (var T in scene.Meshes)
            {
                if (shadow)
                {
                    T.Technique = "RenderShadow";
                }
                else
                {
                    T.Technique = "RenderScene";
                }

                T.render();
            }

            // avion
            if (shadow)
            {
                avion.Technique = "RenderShadow";
            }
            else
            {
                avion.Technique = "RenderScene";
            }
            avion.render();
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

        public override void close()
        {
            effect.Dispose();
            scene.disposeAll();
            scene2.disposeAll();
            g_pShadowMap.Dispose();
            g_pDSShadow.Dispose();
        }
    }
}