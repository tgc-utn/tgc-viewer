using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Drawing;
using System.IO;
using TGC.Core;
using TGC.Core._2D;
using TGC.Core.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Utils;
using Font = System.Drawing.Font;

namespace TGC.Examples.Others
{
    /// <summary>
    ///     Ejemplo default con logo de TGC
    /// </summary>
    public class EjemploDefault : TgcExample
    {
        private readonly float[] lightPos = { 0, 50, 300 };
        private EjemploDefaultHelpForm helpForm;
        private TgcMesh mesh;
        private TgcText2d textHelp;

        public EjemploDefault(string mediaDir, string shadersDir, TgcUserVars userVars, TgcModifiers modifiers,
            TgcAxisLines axisLines, TgcCamera camara)
            : base(mediaDir, shadersDir, userVars, modifiers, axisLines, camara)
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
            textHelp = new TgcText2d();
            textHelp.Position = new Point(15, 260);
            textHelp.Size = new Size(500, 100);
            textHelp.changeFont(new Font("TimesNewRoman", 16, FontStyle.Regular));
            textHelp.Color = Color.Yellow;
            textHelp.Align = TgcText2d.TextAlign.LEFT;
            textHelp.Text = "¿Por dónde empezar? Presionar \"H\"";

            //Help form
            var helpRtf = File.ReadAllText(MediaDir + "ModelosTgc\\LogoTGC\\help.rtf");
            helpForm = new EjemploDefaultHelpForm(helpRtf);

            //Camara
            ((TgcRotationalCamera)Camara).CameraCenter = new Vector3(0, 0, 0);
            ((TgcRotationalCamera)Camara).CameraDistance = 150;

            //BackgroundColor
            D3DDevice.Instance.ClearColor = Color.Black;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            IniciarEscena();
            base.Render();

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

            FinalizarEscena();
        }

        public override void Close()
        {
            base.Close();

            mesh.dispose();
            textHelp.dispose();
            helpForm.Dispose();
        }
    }
}