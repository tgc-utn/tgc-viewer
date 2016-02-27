using System.Drawing;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils._2D;
using TGC.Core.Example;
using TGC.Core.Utils;
using Font = System.Drawing.Font;

namespace Examples.Otros
{
    /// <summary>
    ///     Ejemplo default con logo de TGC
    /// </summary>
    public class EjemploDefault : TgcExample
    {
        private EjemploDefaultHelpForm helpForm;
        private readonly float[] lightPos = {0, 50, 300};
        private TgcMesh mesh;
        private TgcText2d textHelp;

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
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar mesh
            var loader = new TgcSceneLoader();
            mesh =
                loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir +
                                         "ModelosTgc\\LogoTGC\\LogoTGC-TgcScene.xml").Meshes[0];

            //Cargar Shader de PhongShading
            mesh.Effect = GuiController.Instance.Shaders.TgcMeshPhongShader;
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);

            //Texto help
            textHelp = new TgcText2d();
            textHelp.Position = new Point(15, 260);
            textHelp.Size = new Size(500, 100);
            textHelp.changeFont(new Font("TimesNewRoman", 16, FontStyle.Regular));
            textHelp.Color = Color.Yellow;
            textHelp.Align = TgcText2d.TextAlign.LEFT;
            textHelp.Text = "¿Por dónde empezar? Presionar \"H\"";

            //Help form
            var helpRtf = File.ReadAllText(GuiController.Instance.ExamplesMediaDir + "ModelosTgc\\LogoTGC\\help.rtf");
            helpForm = new EjemploDefaultHelpForm(helpRtf);

            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(0, 0, 0);
            GuiController.Instance.RotCamera.CameraDistance = 150;

            GuiController.Instance.BackgroundColor = Color.Black;
        }

        public override void render(float elapsedTime)
        {
            var d3dDevice = GuiController.Instance.D3dDevice;

            //Cargar variables shader
            mesh.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.Gray));
            mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.LightBlue));
            mesh.Effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("specularExp", 10f);
            mesh.Effect.SetValue("lightPosition", lightPos);
            mesh.Effect.SetValue("eyePosition",
                TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.RotCamera.getPosition()));

            mesh.rotateY(-elapsedTime/2);
            mesh.render();

            textHelp.render();

            //Help
            if (GuiController.Instance.D3dInput.keyPressed(Key.H))
            {
                helpForm.ShowDialog(GuiController.Instance.MainForm);
            }
        }

        public override void close()
        {
            mesh.dispose();
            textHelp.dispose();
            helpForm.Dispose();
        }
    }
}