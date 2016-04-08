using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using System.Drawing;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;

namespace Examples.Shaders.WorkshopShaders
{
    public class ParallaxOcclusion : TgcExample
    {
        string MyMediaDir;
        string MyShaderDir;
        TgcScene scene;
        TgcMesh mesh;
        Effect effect;
        Texture g_pBaseTexture;
        Texture g_pHeightmap;
        Texture g_pBaseTexture2;
        Texture g_pHeightmap2;
        Texture g_pBaseTexture3;
        Texture g_pHeightmap3;

        float time;
        bool pom;
        bool phong;
        int nro_textura;

        public override string getCategory()
        {
            return "Shaders";
        }

        public override string getName()
        {
            return "Workshop-ParallaxOcclusion";
        }

        public override string getDescription()
        {
            return "Parallax Occlusion. L->Luz Space->Metodo S->Malla";
        }

        public override void init()
        {
            time = 0f;
            Device d3dDevice = GuiController.Instance.D3dDevice;
            MyMediaDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\";
            MyShaderDir = GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\";

            //Crear loader
            TgcSceneLoader loader = new TgcSceneLoader();

            // parallax oclussion
            scene = loader.loadSceneFromFile(MyMediaDir +"Piso\\Piso-TgcScene.xml");

            g_pBaseTexture  = TextureLoader.FromFile(d3dDevice, MyMediaDir 
                    + "Piso\\Textures\\wood.bmp");
            g_pHeightmap = TextureLoader.FromFile(d3dDevice, MyMediaDir 
                    + "Piso\\Textures\\four_NM_height.tga");

            g_pBaseTexture2 = TextureLoader.FromFile(d3dDevice, MyMediaDir
                    + "Piso\\Textures\\stones.bmp");
            g_pHeightmap2 = TextureLoader.FromFile(d3dDevice, MyMediaDir
                    + "Piso\\Textures\\stones_NM_height.tga");

            g_pBaseTexture3 = TextureLoader.FromFile(d3dDevice, MyMediaDir
                    + "Piso\\Textures\\rocks.jpg");
            g_pHeightmap3 = TextureLoader.FromFile(d3dDevice, MyMediaDir
                    + "Piso\\Textures\\rocks_NM_height.tga");

            mesh = scene.Meshes[0];
            int[] adj = new int[mesh.D3dMesh.NumberFaces * 3];
            mesh.D3dMesh.GenerateAdjacency(0, adj);
            mesh.D3dMesh.ComputeNormals(adj);

            //Cargar Shader
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, MyShaderDir + "Parallax.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }

            GuiController.Instance.Modifiers.addVertex3f("LightDir", new Vector3(-1, -1, -1), new Vector3(1, 1, 1), new Vector3(0, -1, 0));
            GuiController.Instance.Modifiers.addFloat("minSample", 1f, 10f, 10f);
            GuiController.Instance.Modifiers.addFloat("maxSample", 11f, 50f, 50f);
            GuiController.Instance.Modifiers.addFloat("HeightMapScale", 0.001f, 0.5f, 0.1f);

            //Centrar camara rotacional respecto a este mesh
            GuiController.Instance.RotCamera.targetObject(mesh.BoundingBox);
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 100;
            GuiController.Instance.RotCamera.RotationSpeed = 1000f;

 

            pom = false;
            phong = true;
            nro_textura = 0;
        }


        public override void render(float elapsedTime)
        {
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            time += elapsedTime;
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                pom = !pom;
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.L))
                phong = !phong;
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.S))
            {
                if (++nro_textura >= 3)
                    nro_textura = 0;
            }

            
            Vector3 lightDir = (Vector3)GuiController.Instance.Modifiers["LightDir"];
            effect.SetValue("g_LightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
            effect.SetValue("min_cant_samples", (float)GuiController.Instance.Modifiers["minSample"]);
            effect.SetValue("max_cant_samples", (float)GuiController.Instance.Modifiers["maxSample"]);
            effect.SetValue("fHeightMapScale", (float)GuiController.Instance.Modifiers["HeightMapScale"]);
            effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));

            device.EndScene();
            effect.SetValue("time", time);
            switch(nro_textura)
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
            

            GuiController.Instance.Text3d.drawText((pom ? "ParallaxOcclusion" : "BumpMap") +
                "  "+ (phong?"Phong Lighting":"Iluminación estática")
                        , 0, 0, Color.Yellow);


        }

        public override void close()
        {
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
