using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using TGC.Examples.Example;
using Font = System.Drawing.Font;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     Ejemplo default con logo de TGC
    /// </summary>
    public class EjemploDefault : TGCExampleViewer
    {
        private readonly float[] lightPos = { 0, 50, 300 };
        private EjemploDefaultHelpForm helpForm;
        private TgcMesh mesh;
        private TgcText2d textHelp;

        public EjemploDefault(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers)
            : base(mediaDir, shadersDir, userVars, modifiers)
        {
            Category = "Others";
            Name = "Logo de TGC";
            Description = "Logo de TGC";
        }

        public override void Init()
        {
            //Cargar mesh
            var loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(MediaDir + "ModelosTgc\\LogoTGC\\LogoTGC-TgcScene.xml").Meshes[0];

            //Cargar Shader de PhongShading
            mesh.Effect = TgcShaders.Instance.TgcMeshPhongShader;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);

            //Texto help
            textHelp = new TgcText2d(DrawText);
            textHelp.Position = new Point(15, 260);
            textHelp.Size = new Size(500, 100);
            textHelp.changeFont(new Font("TimesNewRoman", 16, FontStyle.Regular));
            textHelp.Color = Color.Yellow;
            textHelp.Align = TgcText2d.TextAlign.LEFT;
            textHelp.Text = "ÅøPor donde empezar? Presionar \"H\"";

            //Help form
            var helpRtf = File.ReadAllText(MediaDir + "ModelosTgc\\LogoTGC\\help.rtf");
            helpForm = new EjemploDefaultHelpForm(helpRtf);

            //Camara
            Camara = new TgcRotationalCamera(new Vector3(), 150f);
        }

        public override void Update()
        {
            PreUpdate();
        }

        public override void Render()
        {
            //BackgroundColor
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            D3DDevice.Instance.Device.BeginScene();
            ClearTextures();

            //Cargar variables shader
            mesh.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.Gray));
            mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.LightBlue));
            mesh.Effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("specularExp", 10f);
            mesh.Effect.SetValue("lightPosition", lightPos);
            mesh.Effect.SetValue("eyePosition",
                TgcParserUtils.vector3ToFloat4Array(Camara.Position));

            mesh.rotateY(-ElapsedTime / 2);
            mesh.render();

            textHelp.render();

            //Help
            if (TgcD3dInput.Instance.keyPressed(Key.H))
            {
                helpForm.ShowDialog();
            }

            PostRender();
        }

        public override void Dispose()
        {
            mesh.dispose();
            textHelp.dispose();
            helpForm.Dispose();
        }
    }
}