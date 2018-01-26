using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Text;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Examples.Camara;
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
        private TgcText2D textHelp;

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
            mesh.AutoTransform = true;

            //Cargar Shader de PhongShading
            mesh.Effect = TgcShaders.Instance.TgcMeshPhongShader;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);

            //Texto help
            textHelp = new TgcText2D();
            textHelp.Position = new Point(15, 260);
            textHelp.Size = new Size(500, 100);
            textHelp.changeFont(new Font("TimesNewRoman", 16, FontStyle.Regular));
            textHelp.Color = Color.Yellow;
            textHelp.Align = TgcText2D.TextAlign.LEFT;
            textHelp.Text = "ÅøPor donde empezar? Presionar \"H\"";

            //Help form
            var helpRtf = File.ReadAllText(MediaDir + "\\help.rtf");
            helpForm = new EjemploDefaultHelpForm(helpRtf);

            //Camara
            Camara = new TgcRotationalCamera(new TGCVector3(), 150f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            PostUpdate();
        }

        public override void Render()
        {
            //BackgroundColor
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            BeginScene();
            ClearTextures();

            //Cargar variables shader
            mesh.Effect.SetValue("ambientColor", ColorValue.FromColor(Color.Gray));
            mesh.Effect.SetValue("diffuseColor", ColorValue.FromColor(Color.LightBlue));
            mesh.Effect.SetValue("specularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("specularExp", 10f);
            mesh.Effect.SetValue("lightPosition", lightPos);
            mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(Camara.Position));

            mesh.RotateY(-ElapsedTime / 2);
            mesh.Render();

            textHelp.render();

            //Help
            if (Input.keyPressed(Key.H))
            {
                helpForm.ShowDialog();
            }

            PostRender();
        }

        public override void Dispose()
        {
            mesh.Dispose();
            textHelp.Dispose();
            helpForm.Dispose();
        }
    }
}