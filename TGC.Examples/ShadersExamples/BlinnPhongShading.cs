using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Textures;
using TGC.Examples.Camara;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;

namespace TGC.Examples.ShadersExamples
{
    public class BlinnPhongShading : TGCExampleViewer
    {
        private TGCVertex3fModifier lightPositionModifier;
        private TGCBooleanModifier toggleBlinn;
        private TGCColorModifier ambientColorModifier;
        private TGCColorModifier diffuseColorModifier;
        private TGCColorModifier specularColorModifier;
        private TGCFloatModifier ambientModifier;
        private TGCFloatModifier diffuseModifier;
        private TGCFloatModifier specularModifier;
        private TGCFloatModifier specularPowerModifier;

        private TGCBox lightBox;
        private List<TgcMesh> planes;
        private Effect effect;

        private bool previousValue;


        public BlinnPhongShading(string mediaDir, string shadersDir, TgcUserVars userVars, Panel modifiersPanel)
            : base(mediaDir, shadersDir, userVars, modifiersPanel)
        {
            Category = "Pixel Shaders";
            Name = "Blinn Phong";
            Description = "Ejemplo de Blinn Phong contrastado con Phong regular";
        }

        public override void Init()
        {
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "Blinn.fx");

            ConfigureModifiers();
            InitializePlanes();
            InitializeLightBox();


            D3DDevice.Instance.Device.RenderState.CullMode = Cull.CounterClockwise;

            BackgroundColor = Color.DarkGray;

            Camara = new TgcFpsCamera(new TGCVector3(0, 80f, 0), 300f, 0f, Input);
        }

        public override void Update()
        {
            PreUpdate();
            lightBox.Transform = TGCMatrix.Translation(lightPositionModifier.Value);

            effect.SetValue("ambientColor", ColorValue.FromColor(ambientColorModifier.Value));
            effect.SetValue("diffuseColor", ColorValue.FromColor(diffuseColorModifier.Value));
            effect.SetValue("specularColor", ColorValue.FromColor(specularColorModifier.Value));
            effect.SetValue("KAmbient", ambientModifier.Value);
            effect.SetValue("KDiffuse", diffuseModifier.Value);
            effect.SetValue("KSpecular", specularModifier.Value);
            effect.SetValue("shininess", specularPowerModifier.Value);
            effect.SetValue("lightPosition", TGCVector3.TGCVector3ToFloat3Array(lightPositionModifier.Value));
            effect.SetValue("eyePosition", TGCVector3.TGCVector3ToFloat3Array(Camara.Position));

            if(!toggleBlinn.Value.Equals(previousValue))
            {
                if(toggleBlinn.Value)
                    planes[0].Technique = "Blinn";
                else
                    planes[0].Technique = "Phong";
                previousValue = toggleBlinn.Value;
            }

            PostUpdate();
        }


        public override void Render()
        {
            PreRender();
            planes.ForEach(plane => plane.Render());
            lightBox.Render();
            PostRender();
        }

        public override void Dispose()
        {
            lightPositionModifier.Dispose();
            toggleBlinn.Dispose();
            ambientColorModifier.Dispose();
            diffuseColorModifier.Dispose();
            specularColorModifier.Dispose();
            ambientModifier.Dispose();
            diffuseModifier.Dispose();
            specularModifier.Dispose();
            specularPowerModifier.Dispose();

            planes.ForEach(plane => plane.Dispose());
        }

        private void InitializePlanes()
        {
            var baldosas = TgcTexture.createTexture(MediaDir + "Texturas\\baldosasceramica.jpg");
            var lisa = TgcTexture.createTexture(MediaDir + "Texturas\\paredLisa.jpg");

            List<TgcPlane> planeCollection = new List<TgcPlane>
            {
                new TgcPlane(new TGCVector3(-500.0f, 0f, -500.0f), new TGCVector3(1000f, 1f, 1000f), TgcPlane.Orientations.XZplane, baldosas, 10f, 10f),
                new TgcPlane(new TGCVector3(500.0f, 0f, -500.0f), new TGCVector3(0f, 400f, 1000f), TgcPlane.Orientations.YZplane, lisa, 23.2f, 16.8f),
                new TgcPlane(new TGCVector3(500.0f, 0f, -500.0f), new TGCVector3(0f, 400f, 1000f), TgcPlane.Orientations.YZplane, lisa, 23.2f, 16.8f),
                new TgcPlane(new TGCVector3(-500.0f, 0f, -500.0f), new TGCVector3(1000f, 400f, 0f), TgcPlane.Orientations.XYplane, lisa, 23.2f, 16.8f),
                new TgcPlane(new TGCVector3(-500.0f, 0f, -500.0f), new TGCVector3(1000f, 400f, 0f), TgcPlane.Orientations.XYplane, lisa, 23.2f, 16.8f),
            };

            planeCollection.ForEach(plane => plane.updateValues());

            int index = 0;
            planes = planeCollection.ConvertAll(plane => { index++; return plane.toMesh("plane" + index.ToString()); });

            // Orient the planes to the center of the scene
            planes[0].Transform = TGCMatrix.RotationX(FastMath.PI) * planes[0].Transform;
            planes[1].Transform = TGCMatrix.RotationY(FastMath.PI) * planes[1].Transform;
            planes[3].Transform = TGCMatrix.RotationY(FastMath.PI) * planes[3].Transform;

            planes.ForEach(plane => { plane.Effect = effect; plane.Technique = "Wall"; }) ;
            planes[0].Technique = "Blinn";
        }

        private void InitializeLightBox()
        {
            lightBox = new TGCBox();
            lightBox.Color = Color.Yellow;
            lightBox.Size = TGCVector3.One * 10.0f;
            lightBox.updateValues();
        }

        private void ConfigureModifiers()
        {
            lightPositionModifier = AddVertex3f("LightPosition", new TGCVector3(-500f, 0f, -500f), new TGCVector3(500f, 500f, 500f), new TGCVector3(100f, 80f, 0f));
            ambientModifier = AddFloat("Ambient", 0, 1, 0.5f);
            diffuseModifier = AddFloat("Diffuse", 0, 1, 0.75f);
            specularModifier = AddFloat("Specular", 0, 1, 0.5f);
            specularPowerModifier = AddFloat("Shininess", 0f, 50.0f, 15f);
            ambientColorModifier = AddColor("ambient", Color.Gray);
            diffuseColorModifier = AddColor("diffuse", Color.LightGoldenrodYellow);
            specularColorModifier = AddColor("specular", Color.White);
            toggleBlinn = AddBoolean("Blinn Phong", "Usar Blinn Phong", true);
            previousValue = toggleBlinn.Value;
        }

    }
}
