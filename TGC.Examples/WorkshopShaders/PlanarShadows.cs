using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace Examples.WorkshopShaders
{
    public class PlanarShadows : TGCExampleViewer
    {
        private string MyMediaDir;
        private string MyShaderDir;
        private TgcScene scene, scene2;
        private TGCBox box;
        private Effect effect;
        private TgcMesh avion;

        // Shadow map
        private TGCVector3 g_LightPos;	// posicion de la luz actual (la que estoy analizando)

        private float near_plane = 2f;
        private float far_plane = 1500f;

        private TGCVector3 dir_avion;
        private float time;

        public PlanarShadows(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Shaders";
            Name = "Workshop-PlanarShadows";
            Description = "Planar Shadows";
        }

        public override void Init()
        {
            MyMediaDir = MediaDir + "WorkshopShaders\\";
            MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            // ------------------------------------------------------------
            //Cargar la escena
            scene = loader.loadSceneFromFile(MyMediaDir + "shadowTest\\ShadowTest-TgcScene.xml");

            scene2 = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vehiculos\\AvionCaza\\AvionCaza-TgcScene.xml");
            avion = scene2.Meshes[0];

            avion.Scale = new TGCVector3(0.1f, 0.1f, 0.1f);
            avion.Position = new TGCVector3(100f, 100f, 0f);
            avion.AutoTransform = false;
            dir_avion = new TGCVector3(0, 0, 1);

            Camara = new TgcRotationalCamera(new TGCVector3(0, 20, 125), 50, 0.15f, 50f, Input);

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "WorkshopShaders\\PlanarShadows.fx");

            // le asigno el efecto a las mallas
            foreach (TgcMesh T in scene.Meshes)
            {
                T.Scale = new TGCVector3(1f, 1f, 1f);
                T.Effect = effect;
            }
            avion.Effect = effect;

            box = new TGCBox();
            box.Color = Color.Yellow;

            //GuiController.Instance.RotCamera.targetObject(scene.Meshes[0].BoundingBox);
            float K = 300;
            Modifiers.addVertex3f("LightLookFrom", new TGCVector3(-K, -K, -K), new TGCVector3(K, K, K), new TGCVector3(80, 120, 0));
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            Device d3dDevice = D3DDevice.Instance.Device;

            time += ElapsedTime;
            // animo la pos del avion
            float alfa = -time * Geometry.DegreeToRadian(115.0f);
            avion.Position = new TGCVector3(80f * (float)Math.Cos(alfa), 20 - 20 * (float)Math.Sin(alfa), 80f * (float)Math.Sin(alfa));
            dir_avion = new TGCVector3(-(float)Math.Sin(alfa), 0, (float)Math.Cos(alfa));
            avion.Transform = CalcularMatriz(avion.Position, avion.Scale, dir_avion);
            g_LightPos = (TGCVector3)Modifiers["LightLookFrom"];

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // dibujo la escena pp dicha
            d3dDevice.BeginScene();
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            // piso
            scene.Meshes[0].Technique = "RenderScene";
            scene.Meshes[0].Render();

            // dibujo las sombra del avion sobre el piso
            effect.SetValue("matViewProj", d3dDevice.Transform.View * d3dDevice.Transform.Projection);
            effect.SetValue("g_vLightPos", new TGCVector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            d3dDevice.RenderState.ZBufferEnable = false;
            avion.Technique = "RenderShadows";
            avion.Render();
            d3dDevice.RenderState.ZBufferEnable = true;

            // avion
            avion.Technique = "RenderScene";
            avion.Render();

            // dibujo la luz
            box.setPositionSize(g_LightPos, new TGCVector3(5, 5, 5));
            box.updateValues();
            box.Render();

            PostRender();
        }

        // helper
        public TGCMatrix CalcularMatriz(TGCVector3 Pos, TGCVector3 Scale, TGCVector3 Dir)
        {
            TGCVector3 VUP = new TGCVector3(0, 1, 0);

            TGCMatrix matWorld = TGCMatrix.Scaling(Scale);
            // determino la orientacion
            TGCVector3 U = TGCVector3.Cross(VUP, Dir);
            U.Normalize();
            TGCVector3 V = TGCVector3.Cross(Dir, U);
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
        }
    }
}