using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Examples.ShadersExamples
{
    public class ParallaxOcclusion : TgcExample
    {
        private Effect effect;
        private Texture g_pBaseTexture;
        private Texture g_pBaseTexture2;
        private Texture g_pBaseTexture3;
        private Texture g_pHeightmap;
        private Texture g_pHeightmap2;
        private Texture g_pHeightmap3;
        private TgcMesh mesh;
        private int nro_textura;
        private bool phong;
        private bool pom;
        private TgcScene scene;

        private float time;

        public ParallaxOcclusion(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
        {
            Category = "Shaders";
            Name = "Workshop-ParallaxOcclusion";
            Description = "Parallax Occlusion. L->Luz Space->Metodo S->Malla";
        }

        public override void Init()
        {
            time = 0f;
            var d3dDevice = D3DDevice.Instance.Device;
            var MyShaderDir = ShadersDir + "WorkshopShaders\\";

            //Crear loader
            var loader = new TgcSceneLoader();

            // parallax oclussion
            scene = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\Piso\\Piso-Custom-TgcScene.xml");

            g_pBaseTexture = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\wood.bmp");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_four_height.tga");

            g_pBaseTexture2 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\stones.bmp");
            g_pHeightmap2 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_stones.tga");

            g_pBaseTexture3 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\rocks.jpg");
            g_pHeightmap3 = TextureLoader.FromFile(d3dDevice, MediaDir + "Texturas\\NM_height_rocks.tga");

            mesh = scene.Meshes[0];
            var adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null,
                out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            Modifiers.addVertex3f("LightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            Modifiers.addFloat("minSample", 1f, 10f, 10f);
            Modifiers.addFloat("maxSample", 11f, 50f, 50f);
            Modifiers.addFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            //Centrar camara rotacional respecto a este mesh
            var rotCamera = new TgcRotationalCamera(mesh.BoundingBox.calculateBoxCenter(), mesh.BoundingBox.calculateBoxRadius() * 2);
            rotCamera.CameraCenter = rotCamera.CameraCenter + new Vector3(0, 20f, 0);
            rotCamera.CameraDistance = 75;
            rotCamera.RotationSpeed = 50f;
            Camara = rotCamera;

            pom = false;
            phong = true;
            nro_textura = 0;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

            var device = D3DDevice.Instance.Device;

            time += ElapsedTime;
            if (TgcD3dInput.Instance.keyPressed(Key.Space))
                pom = !pom;
            if (TgcD3dInput.Instance.keyPressed(Key.L))
                phong = !phong;
            if (TgcD3dInput.Instance.keyPressed(Key.S))
            {
                if (++nro_textura >= 3)
                    nro_textura = 0;
            }

            var lightDir = (Vector3)Modifiers["LightDir"];
            effect.SetValue("g_LightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
            effect.SetValue("min_cant_samples", (float)Modifiers["minSample"]);
            effect.SetValue("max_cant_samples", (float)Modifiers["maxSample"]);
            effect.SetValue("fHeightMapScale", (float)Modifiers["HeightMapScale"]);
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(Camara.Position));

            device.EndScene();
            effect.SetValue("time", time);
            switch (nro_textura)
            {
                case 0:
                default:
                    effect.SetValue("aux_Tex", g_pBaseTexture);
                    effect.SetValue("height_map", g_pHeightmap);
                    break;

                case 1:
                    effect.SetValue("aux_Tex", g_pBaseTexture2);
                    effect.SetValue("height_map", g_pHeightmap2);
                    break;

                case 2:
                    effect.SetValue("aux_Tex", g_pBaseTexture3);
                    effect.SetValue("height_map", g_pHeightmap3);
                    break;
            }
            effect.SetValue("phong_lighting", phong);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            mesh.Effect = effect;
            mesh.Technique = pom ? "ParallaxOcclusion" : "BumpMap";
            mesh.render();

            TgcDrawText.Instance.drawText((pom ? "ParallaxOcclusion" : "BumpMap") +
                                          "  " + (phong ? "Phong Lighting" : "Iluminación estática"), 0, 0, Color.Yellow);

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
            effect.Dispose();
            g_pBaseTexture.Dispose();
            g_pBaseTexture2.Dispose();
            g_pBaseTexture3.Dispose();
            g_pHeightmap.Dispose();
            g_pHeightmap2.Dispose();
            g_pHeightmap3.Dispose();
        }
    }
}