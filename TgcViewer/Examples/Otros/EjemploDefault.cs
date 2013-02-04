using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using Examples.Shaders;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils.TgcGeometry;

namespace Examples.Otros
{
    /// <summary>
    /// Ejemplo default con logo de TGC
    /// </summary>
    public class EjemploDefault : TgcExample
    {
        TgcMeshShader mesh;
        float[] lightPos = new float[]{0, 50, 300};
        InterpoladorVaiven interp;

        public override string getCategory()
        {
            return "Otros";
        }

        public override string getName()
        {
            return "Logo de TGC";
        }

        public override string getDescription()
        {
            return "Logo de TGC";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar mesh
            TgcSceneLoader loader = new TgcSceneLoader();
            loader.MeshFactory = new CustomMeshShaderFactory();
            mesh = (TgcMeshShader)loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\LogoTGC\\LogoTGC-TgcScene.xml").Meshes[0];

            //Cargar Shader de PhonhShading
            string compilationErrors;
            mesh.Effect = Effect.FromFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\PhongShading.fx", null, null, ShaderFlags.None, null, out compilationErrors);
            if (mesh.Effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            mesh.Effect.Technique = "NoTextureTechnique";

            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 150;

            GuiController.Instance.BackgroundColor = Color.Black;

            //Interpolador para luz
            interp = new InterpoladorVaiven();
            interp.Min = -300;
            interp.Max = 300;
            interp.Speed = 200;
            interp.reset();
        }


        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar variables de shader
            lightPos[0] = interp.update();
            mesh.Effect.SetValue("fvLightPosition", lightPos);
            mesh.Effect.SetValue("fvEyePosition", TgcParserUtils.vector3ToFloat3Array(GuiController.Instance.RotCamera.getPosition()));
            mesh.Effect.SetValue("fvAmbient", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("fvDiffuse", ColorValue.FromColor(Color.LightBlue));
            mesh.Effect.SetValue("fvSpecular", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("fSpecularPower", 10);

            mesh.rotateY(-elapsedTime/2);
            mesh.render();
        }

        public override void close()
        {
            mesh.Effect.Dispose();
            mesh.dispose();
        }

    }
}
