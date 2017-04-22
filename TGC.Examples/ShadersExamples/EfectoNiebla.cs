using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Fog;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Examples.Example;

namespace TGC.Examples.PostProcess
{
    /// <summary>
    ///     Ejemplo EfectoNiebla
    ///     Unidades Involucradas:
    ///     # Unidad 4 - Conceptos Basicos de 3D - Fog
    ///     Muestra como utilizar los efectos de niebla provistos por el Pipeline, a traves de la herramienta TgcFog
    ///     Autor: Matias Leone, Leandro Barbagallo
    /// </summary>
    public class EfectoNiebla : TGCExampleViewer
    {
        private Effect effect;
        private TgcFog fog;
        private TgcScene scene;
        private TgcSkyBox skyBox;

        public EfectoNiebla(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Pixel Shaders";
            Name = "Efecto Niebla";
            Description = "Muestra como utilizar el efecto niebla y como configurar sus diversos atributos.";
        }

        public override void Init()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new TGCVector3(0, 500, 0);
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");
            skyBox.Init();

            //Cargar escenario de Isla
            var loader = new TgcSceneLoader();
            scene = loader.loadSceneFromFile(MediaDir + "Isla\\Isla-TgcScene.xml");

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "TgcViewer\\TgcFogShader.fx");

            //Camara en 1ra persona
            Camara = new TgcFpsCamera(new TGCVector3(1500, 800, 0), Input);

            //Modifiers para configurar valores de niebla
            Modifiers.addBoolean("FogShader", "FogShader", true);
            Modifiers.addFloat("startDistance", 1, 10000, 2000);
            Modifiers.addFloat("endDistance", 1, 10000, 5000);
            Modifiers.addFloat("density", 0, 1, 0.0025f);
            Modifiers.addColor("color", Color.LightGray);

            fog = new TgcFog();
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            ClearTextures();
            D3DDevice.Instance.Device.BeginScene();
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //Cargar valores de niebla
            var fogShader = (bool)Modifiers["FogShader"];
            fog.Enabled = !(bool)Modifiers["FogShader"];
            fog.StartDistance = (float)Modifiers["startDistance"];
            fog.EndDistance = (float)Modifiers["endDistance"];
            fog.Density = (float)Modifiers["density"];
            fog.Color = (Color)Modifiers["color"];

            if (fog.Enabled)
            {
                fog.updateValues();
            }

            if (fogShader)
            {
                // Cargamos las variables de shader, color del fog.
                effect.SetValue("ColorFog", fog.Color.ToArgb());
                effect.SetValue("CameraPos", TGCVector3.Vector3ToFloat4Array(Camara.Position));
                effect.SetValue("StartFogDistance", fog.StartDistance);
                effect.SetValue("EndFogDistance", fog.EndDistance);
                effect.SetValue("Density", fog.Density);
            }

            //Actualizar valores
            foreach (var mesh in skyBox.Faces)
            {
                if (fogShader)
                {
                    mesh.Effect = effect;
                    mesh.Technique = "RenderScene";
                }
                else
                {
                    mesh.Effect = TgcShaders.Instance.TgcMeshShader;
                    mesh.Technique = "DIFFUSE_MAP";
                }

                mesh.Render();
            }

            //skyBox.render();

            foreach (var mesh in scene.Meshes)
            {
                if (fogShader)
                {
                    mesh.Effect = effect;
                    mesh.Technique = "RenderScene";
                }
                else
                {
                    mesh.Effect = TgcShaders.Instance.TgcMeshShader;
                    mesh.Technique = "DIFFUSE_MAP";
                }
                mesh.UpdateMeshTransform();
                mesh.Render();
            }

            RenderAxis();
            RenderFPS();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        public override void Dispose()
        {
            skyBox.Dispose();
            scene.disposeAll();
        }
    }
}